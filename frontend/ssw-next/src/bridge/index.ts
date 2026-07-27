import type { SelectionBridge } from "./contracts";
import { MockSelectionBridge } from "./mockBridge";

type NativeResponse = {
  requestId: string;
  success: boolean;
  payload?: unknown;
  error?: string;
};

type NativeModel = {
  Id: number;
  Code: string;
  Name: string;
  SeriesCode: string;
  NominalAirflowM3h: number;
  StaticPressurePa: number;
};

type NativeAccessory = {
  Code: string;
  Name: string;
  Category: string;
  Installation: string;
  Included: boolean;
  Locked: boolean;
};

declare global {
  interface Window {
    chrome?: {
      webview?: {
        postMessage(message: unknown): void;
        addEventListener(
          type: "message",
          listener: (event: MessageEvent<NativeResponse>) => void,
        ): void;
        removeEventListener(
          type: "message",
          listener: (event: MessageEvent<NativeResponse>) => void,
        ): void;
      };
    };
  }
}

const nativeInvoke = <T>(
  command: string,
  payload: Record<string, unknown> = {},
): Promise<T> => {
  const webview = window.chrome?.webview;
  if (!webview) {
    return Promise.reject(new Error("WebView2 bridge is unavailable."));
  }

  const requestId = crypto.randomUUID();
  return new Promise<T>((resolve, reject) => {
    const listener = (event: MessageEvent<NativeResponse>) => {
      if (event.data?.requestId !== requestId) return;
      webview.removeEventListener("message", listener);
      if (event.data.success) resolve(event.data.payload as T);
      else reject(new Error(event.data.error || "Desktop bridge request failed."));
    };
    webview.addEventListener("message", listener);
    webview.postMessage({ requestId, command, payload });
  });
};

const numberValue = (value: unknown, fallback = 0): number => {
  const parsed = Number(value);
  return Number.isFinite(parsed) ? parsed : fallback;
};

class NativeSelectionBridge implements SelectionBridge {
  private models: NativeModel[] = [];

  async bootstrap() {
    const initialization = await nativeInvoke<{
      models: NativeModel[];
    }>("app.initialize");
    this.models = initialization.models ?? [];
    if (this.models.length === 0) {
      throw new Error("The local SDF does not contain selectable units.");
    }

    const preferred =
      this.models.find((model) => model.Code === "CLRC 038 OSC") ??
      this.models[0];
    const airflow = Math.max(1, Math.min(100, preferred.NominalAirflowM3h));
    const draft = {
      project: {
        name: "Progetto 01",
        customerReference: "",
        language: "Italiano",
      },
      operatingPoint: {
        supplyAirflow: airflow,
        extractAirflow: airflow,
        pressure: Math.max(0, Math.min(100, preferred.StaticPressurePa)),
      },
      selectedUnitId: preferred.Code,
      installationMode: "ceiling" as const,
      layoutCode: "B6",
      waterCoilEnabled: false,
      waterCoilMode: "HCD" as const,
      electricPreheaterEnabled: false,
      electricPostheaterEnabled: false,
      accessoryCodes: [] as string[],
    };
    const native = await this.calculateNative(draft);
    const defaultLayout = (
      native.Layout?.Configurations as Array<{
        Code: string;
        IsDefault: boolean;
      }> ?? []
    ).find((configuration) => configuration.IsDefault);
    if (defaultLayout?.Code) {
      draft.layoutCode = defaultLayout.Code;
    }
    draft.accessoryCodes = (native.Accessories as NativeAccessory[] ?? [])
      .filter((item) => item.Included)
      .map((item) => item.Code);

    return {
      draft,
      units: this.models.map((model) => ({
        id: model.Code,
        family: model.SeriesCode,
        model: model.Name || model.Code,
        maxAirflow: numberValue(model.NominalAirflowM3h),
        availablePressure: numberValue(model.StaticPressurePa),
        efficiency:
          model.Code === preferred.Code
            ? numberValue(native.Winter?.Curves?.WorkingPointEfficiencyPercent)
            : 0,
        soundPower: 0,
        fitScore: -1,
      })),
      accessories: (native.Accessories as NativeAccessory[] ?? []).map((item) => ({
        code: item.Code,
        name: item.Name,
        category: item.Category,
        installation:
          item.Installation?.toLowerCase() === "internal"
            ? ("Internal" as const)
            : ("External" as const),
        included: item.Included,
        locked: item.Locked,
      })),
      result: this.mapResult(native, draft.operatingPoint.pressure),
    };
  }

  async calculate(draft: Parameters<SelectionBridge["calculate"]>[0]) {
    return this.mapResult(
      await this.calculateNative(draft),
      draft.operatingPoint.pressure,
    );
  }

  async saveDraft() {
    await nativeInvoke("legacy.open");
    return { savedAt: new Date().toISOString(), delegated: true };
  }

  async generateReport(draft: Parameters<SelectionBridge["generateReport"]>[0]) {
    await nativeInvoke("legacy.open");
    const model =
      this.models.find((item) => item.Code === draft.selectedUnitId)?.Name ??
      draft.selectedUnitId;
    return {
      fileName: `${model.replaceAll(" ", "_")}_Report.pdf`,
      delegated: true,
    };
  }

  private calculateNative(
    draft: Parameters<SelectionBridge["calculate"]>[0],
  ): Promise<any> {
    return nativeInvoke("selection.calculate", {
      modelCode: draft.selectedUnitId,
      supplyAirflow: draft.operatingPoint.supplyAirflow,
      extractAirflow: draft.operatingPoint.extractAirflow,
      pressure: draft.operatingPoint.pressure,
      regulation: 100,
    });
  }

  private mapResult(native: any, requiredPressure: number) {
    const winter = native.Winter;
    const summer = native.Summer;
    const pressure = Math.max(
      0,
      numberValue(winter?.Curves?.WorkingPointPressurePa) - requiredPressure,
    );
    const winterThermo = winter?.Result?.Thermodynamics;
    const summerThermo = summer?.Result?.Thermodynamics;
    return {
      supplyTemperature: numberValue(
        winterThermo?.SupplyOutletTemperatureC,
      ),
      summerSupplyTemperature: numberValue(
        summerThermo?.SupplyOutletTemperatureC,
      ),
      availablePressure: pressure,
      winterEfficiency: numberValue(
        winter?.Curves?.WorkingPointEfficiencyPercent,
      ),
      summerEfficiency: numberValue(
        summer?.Curves?.WorkingPointEfficiencyPercent,
      ),
      absorbedPower: numberValue(winter?.Curves?.WorkingPointPowerW),
      sfp: numberValue(
        winter?.Result?.CombinedSpecificFanPowerWPerM3hPerSecond,
      ),
      status: pressure <= 0 ? ("invalid" as const) : ("valid" as const),
      messages:
        pressure <= 0
          ? ["La pressione richiesta supera quella disponibile."]
          : [],
      accessories: (native.Accessories as NativeAccessory[] ?? []).map((item) => ({
        code: item.Code,
        name: item.Name,
        category: item.Category,
        installation:
          item.Installation?.toLowerCase() === "internal"
            ? ("Internal" as const)
            : ("External" as const),
        included: item.Included,
        locked: item.Locked,
      })),
      layoutCodes: (native.Layout?.Configurations ?? []).map(
        (configuration: { Code: string }) => configuration.Code,
      ),
    };
  }
}

export const createBridge = (): SelectionBridge => {
  return window.chrome?.webview
    ? new NativeSelectionBridge()
    : new MockSelectionBridge();
};

export const runtimeName = (): string =>
  window.chrome?.webview ? "WebView2 preview" : "Browser mock";
