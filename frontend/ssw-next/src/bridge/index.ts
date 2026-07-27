import type { SelectionBridge, SelectionDraft } from "./contracts";
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

type NativeWaterCoil = {
  Id: number;
  Name: string;
  Mode: "CWD" | "HWD" | "HCD";
  Installation: string;
  LengthMm: number;
  HeightMm: number;
  Rows: number;
  Circuits: number;
  FinSpacingMm: number;
};

type NativeElectricHeater = {
  Id: number;
  Code: string;
  Name: string;
  Mode: "PEHD" | "EHD";
  Installation: string;
  PowerW: number;
  VoltageV: number;
  CurrentA: number;
  PhaseCount: number;
  Quantity: number;
  IsDefault: boolean;
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
    const timeout = window.setTimeout(() => {
      webview.removeEventListener("message", listener);
      reject(new Error(`Desktop bridge timeout while executing ${command}.`));
    }, 30000);
    const listener = (event: MessageEvent<NativeResponse>) => {
      if (event.data?.requestId !== requestId) return;
      window.clearTimeout(timeout);
      webview.removeEventListener("message", listener);
      if (event.data.success) resolve(event.data.payload as T);
      else reject(new Error(event.data.error || "Desktop bridge request failed."));
    };
    webview.addEventListener("message", listener);
    webview.postMessage({ requestId, command, payload });
  });
};

export const logClientError = (error: unknown): void => {
  const webview = window.chrome?.webview;
  if (!webview) return;
  const message = error instanceof Error ? error.stack || error.message : String(error);
  webview.postMessage({
    requestId: crypto.randomUUID(),
    command: "app.clientError",
    payload: { message },
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
    const draft: SelectionDraft = {
      project: {
        name: "Progetto 01",
        customerReference: "",
        language: "it",
      },
      operatingPoint: {
        supplyAirflow: airflow,
        extractAirflow: airflow,
        pressure: Math.max(0, Math.min(100, preferred.StaticPressurePa)),
      },
      regulationPercent: 100,
      summerEnabled: true,
      winterOutdoorTemperature: -10,
      winterOutdoorRh: 80,
      winterReturnTemperature: 20,
      winterReturnRh: 60,
      summerOutdoorTemperature: 32,
      summerOutdoorRh: 80,
      summerReturnTemperature: 26,
      summerReturnRh: 50,
      selectedUnitId: preferred.Code,
      installationMode: "ceiling" as const,
      layoutCode: "B6",
      waterCoilEnabled: false,
      waterCoilMode: "HCD" as const,
      waterCoilId: 0,
      waterCoilCustomized: false,
      waterCoilLengthMm: 0,
      waterCoilHeightMm: 0,
      waterCoilRows: 0,
      waterCoilCircuits: 0,
      waterCoilFinSpacingMm: 0,
      fluidCode: "Water" as const,
      glycolPercent: 10,
      coolingWaterInletTemperature: 7,
      coolingWaterOutletTemperature: 12,
      heatingWaterInletTemperature: 80,
      heatingWaterOutletTemperature: 70,
      electricPreheaterEnabled: false,
      electricPreheaterId: 0,
      electricPostheaterEnabled: false,
      electricPostheaterId: 0,
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
    const defaultCoil = (native.AvailableWaterCoils as NativeWaterCoil[] ?? [])[0];
    if (defaultCoil) {
      draft.waterCoilId = defaultCoil.Id;
      draft.waterCoilMode = defaultCoil.Mode;
      draft.waterCoilLengthMm = defaultCoil.LengthMm;
      draft.waterCoilHeightMm = defaultCoil.HeightMm;
      draft.waterCoilRows = defaultCoil.Rows;
      draft.waterCoilCircuits = defaultCoil.Circuits;
      draft.waterCoilFinSpacingMm = defaultCoil.FinSpacingMm;
    }
    const heaters = native.AvailableElectricHeaters as NativeElectricHeater[] ?? [];
    draft.electricPreheaterId =
      heaters.find((item) => item.Mode === "PEHD" && item.IsDefault)?.Id ??
      heaters.find((item) => item.Mode === "PEHD")?.Id ??
      0;
    draft.electricPostheaterId =
      heaters.find((item) => item.Mode === "EHD" && item.IsDefault)?.Id ??
      heaters.find((item) => item.Mode === "EHD")?.Id ??
      0;

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

  async saveDraft(draft: Parameters<SelectionBridge["saveDraft"]>[0]) {
    await nativeInvoke("project.edit", this.draftPayload(draft));
    return { savedAt: new Date().toISOString(), delegated: true };
  }

  async generateReport(draft: Parameters<SelectionBridge["generateReport"]>[0]) {
    await nativeInvoke("report.generate", this.draftPayload(draft));
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
    return nativeInvoke("selection.calculate", this.draftPayload(draft));
  }

  private draftPayload(
    draft: Parameters<SelectionBridge["calculate"]>[0],
  ): Record<string, unknown> {
    return {
      projectName: draft.project.name,
      customerReference: draft.project.customerReference,
      languageCode: draft.project.language,
      modelCode: draft.selectedUnitId,
      supplyAirflow: draft.operatingPoint.supplyAirflow,
      extractAirflow: draft.operatingPoint.extractAirflow,
      pressure: draft.operatingPoint.pressure,
      regulation: draft.regulationPercent,
      summerEnabled: draft.summerEnabled,
      winterOutdoorTemperature: draft.winterOutdoorTemperature,
      winterOutdoorRh: draft.winterOutdoorRh,
      winterReturnTemperature: draft.winterReturnTemperature,
      winterReturnRh: draft.winterReturnRh,
      summerOutdoorTemperature: draft.summerOutdoorTemperature,
      summerOutdoorRh: draft.summerOutdoorRh,
      summerReturnTemperature: draft.summerReturnTemperature,
      summerReturnRh: draft.summerReturnRh,
      waterCoilEnabled: draft.waterCoilEnabled,
      waterCoilId: draft.waterCoilId,
      waterCoilMode: draft.waterCoilMode,
      waterCoilCustomized: draft.waterCoilCustomized,
      waterCoilLengthMm: draft.waterCoilLengthMm,
      waterCoilHeightMm: draft.waterCoilHeightMm,
      waterCoilRows: draft.waterCoilRows,
      waterCoilCircuits: draft.waterCoilCircuits,
      waterCoilFinSpacingMm: draft.waterCoilFinSpacingMm,
      fluidCode: draft.fluidCode,
      glycolPercent: draft.glycolPercent,
      coolingWaterInletTemperature: draft.coolingWaterInletTemperature,
      coolingWaterOutletTemperature: draft.coolingWaterOutletTemperature,
      heatingWaterInletTemperature: draft.heatingWaterInletTemperature,
      heatingWaterOutletTemperature: draft.heatingWaterOutletTemperature,
      electricPreheaterEnabled: draft.electricPreheaterEnabled,
      electricPreheaterId: draft.electricPreheaterId,
      electricPostheaterEnabled: draft.electricPostheaterEnabled,
      electricPostheaterId: draft.electricPostheaterId,
      accessoryCodes: draft.accessoryCodes,
    };
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
    const validationMessages = (native.Validation?.Issues ?? []).map(
      (issue: { MessageKey?: string; Code?: string }) =>
        issue.MessageKey || issue.Code || "Configurazione da verificare.",
    );
    const invalid = pressure <= 0;
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
      status: invalid
        ? ("invalid" as const)
        : validationMessages.length > 0
          ? ("warning" as const)
          : ("valid" as const),
      messages: [
        ...(invalid
          ? ["La pressione richiesta supera quella disponibile."]
          : []),
        ...validationMessages,
      ],
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
      waterCoils: (native.AvailableWaterCoils as NativeWaterCoil[] ?? []).map(
        (item) => ({
          id: item.Id,
          name: item.Name,
          mode: item.Mode,
          installation: item.Installation,
          lengthMm: numberValue(item.LengthMm),
          heightMm: numberValue(item.HeightMm),
          rows: numberValue(item.Rows),
          circuits: numberValue(item.Circuits),
          finSpacingMm: numberValue(item.FinSpacingMm),
        }),
      ),
      electricHeaters: (
        native.AvailableElectricHeaters as NativeElectricHeater[] ?? []
      ).map((item) => ({
        id: item.Id,
        code: item.Code,
        name: item.Name,
        mode: item.Mode,
        installation: item.Installation,
        powerW: numberValue(item.PowerW),
        voltageV: numberValue(item.VoltageV),
        currentA: numberValue(item.CurrentA),
        phaseCount: numberValue(item.PhaseCount),
        quantity: numberValue(item.Quantity),
        isDefault: Boolean(item.IsDefault),
      })),
      waterCoilResults: (native.WaterCoilResults ?? []).map((item: any) => ({
        mode: item.Mode,
        status: item.StatusCode,
        capacityW: numberValue(item.CapacityW),
        sensibleCapacityW: numberValue(item.SensibleCapacityW),
        airOutletTemperatureC: numberValue(item.AirOutletTemperatureC),
        airOutletRelativeHumidityPercent: numberValue(
          item.AirOutletRelativeHumidityPercent,
        ),
        condensateLitersPerHour: numberValue(item.CondensateLitersPerHour),
        airPressureDropPa: numberValue(item.AirPressureDropPa),
        fluidPressureDropKPa: numberValue(item.FluidPressureDropKPa),
        fluidFlowLitersPerHour: numberValue(item.FluidFlowLitersPerHour),
        fluidVelocityMetersPerSecond: numberValue(
          item.FluidVelocityMetersPerSecond,
        ),
        faceVelocityMetersPerSecond: numberValue(
          item.FaceVelocityMetersPerSecond,
        ),
      })),
      electricHeaterResults: (native.ElectricHeaterResults ?? []).map(
        (item: any) => ({
          mode: item.Mode,
          heaterCode: item.HeaterCode,
          powerW: numberValue(item.PowerW),
          currentA: numberValue(item.CurrentA),
          airInletTemperatureC: numberValue(item.AirInletTemperatureC),
          airOutletTemperatureC: numberValue(item.AirOutletTemperatureC),
          airOutletRelativeHumidityPercent: numberValue(
            item.AirOutletRelativeHumidityPercent,
          ),
          airPressureDropPa: numberValue(item.AirPressureDropPa),
        }),
      ),
      additionalPressureDropPa: numberValue(native.AdditionalPressureDropPa),
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
