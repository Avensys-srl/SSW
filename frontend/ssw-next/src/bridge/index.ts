import type {
  FollowUpCenterState,
  DimensionalDrawingState,
  MultiProjectState,
  ProductDocumentState,
  ProjectSaveState,
  SelectionBridge,
  SelectionDraft,
} from "./contracts";
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

type NativePreselection = {
  Model: NativeModel;
  RequiredRegulationPercent: number;
  AvailablePressurePa: number;
  AbsorbedPowerW: number;
  CombinedSfp: number;
  SupplySoundPowerDbA?: number;
  SupplySoundPressureDbA?: number;
  BreakoutSoundPowerDbA?: number;
  BreakoutSoundPressureDbA?: number;
};

type NativeAccessory = {
  Code: string;
  Name: string;
  Category: string;
  Installation: string;
  Included: boolean;
  Locked: boolean;
  Enabled: boolean;
  DisabledReason: string;
};

type NativeWaterCoil = {
  Id: number;
  Name: string;
  Mode: "CWD" | "HWD" | "HCD";
  Installation: string;
  InstallationLabel: string;
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
      imbalanceEnabled: false,
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
      waterCoilCustomDisclaimerAccepted: false,
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
      co2: {
        includeInReport: false,
        roomWidthMeters: 7,
        roomLengthMeters: 8,
        roomHeightMeters: 3,
        activityMet: 1.2,
        occupiedPeople: 20,
        occupiedMinutes: 45,
        breakPeople: 0,
        breakMinutes: 15,
        calculationMethod: "maximum-concentration" as const,
        outdoorConcentrationPpm: 380,
        maximumConcentrationPpm: 1000,
        airflowPerAreaLitersPerSecondPerSquareMeter: 0.35,
        airflowPerPersonLitersPerSecond: 10,
      },
      sound: {
        includeInReport: false,
        directivityFactor: 2 as const,
        distance1Meters: 1,
        distance2Meters: 3,
        iso16032Enabled: false,
      },
      preselectionFilters: {
        maximumSfpEnabled: false,
        maximumSfp: 2,
        supplyNoiseEnabled: false,
        supplyNoiseMetric: "LWA" as const,
        maximumSupplyNoiseDbA: 50,
        supplyNoiseDirectivityFactor: 2 as const,
        supplyNoiseDistanceMeters: 1,
        breakoutNoiseEnabled: false,
        breakoutNoiseMetric: "LWA" as const,
        maximumBreakoutNoiseDbA: 50,
        breakoutNoiseDirectivityFactor: 2 as const,
        breakoutNoiseDistanceMeters: 1,
      },
    };
    const compatibleUnits = await this.preselectNative(draft);
    if (compatibleUnits.length === 0) {
      throw new Error("No unit satisfies the initial operating point.");
    }
    draft.selectedUnitId = compatibleUnits[0].Model.Code;
    draft.regulationPercent = numberValue(
      compatibleUnits[0].RequiredRegulationPercent,
      100,
    );
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
      units: compatibleUnits.map((item) =>
        this.mapPreselection(item, native, draft.selectedUnitId),
      ),
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
        enabled: item.Enabled,
        disabledReason: item.DisabledReason,
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

  async preselect(draft: Parameters<SelectionBridge["preselect"]>[0]) {
    return (await this.preselectNative(draft)).map((item) =>
      this.mapPreselection(item),
    );
  }

  async saveDraft(
    draft: Parameters<SelectionBridge["saveDraft"]>[0],
    saveAs = false,
  ) {
    return nativeInvoke<ProjectSaveState>(
      saveAs ? "project.saveAs" : "project.edit",
      this.draftPayload(draft),
    );
  }

  async openDraft(currentDraft: SelectionDraft) {
    const response = await nativeInvoke<any>("project.open");
    if (!response.opened || !response.input) return response;
    return {
      ...response,
      draft: this.mapOpenedDraft(response.input, currentDraft),
    };
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

  async getProductDocuments(draft: SelectionDraft) {
    return nativeInvoke<ProductDocumentState>(
      "documents.list",
      this.draftPayload(draft),
    );
  }

  async openProductDocument(
    documentType: "commercial-sheet" | "installation-manual",
    draft: SelectionDraft,
  ) {
    return nativeInvoke<ProductDocumentState>("documents.open", {
      ...this.draftPayload(draft),
      documentType,
    });
  }

  async getDimensionalDrawing(draft: SelectionDraft) {
    const native = await nativeInvoke<any>("drawing.get", {
      modelCode: draft.selectedUnitId,
      layoutCode: draft.layoutCode,
    });
    return {
      available: native.Available === true,
      code: native.Code,
      revision: native.Revision,
      fileName: native.FileName,
      mimeType: native.MimeType,
      sha256: native.Sha256,
      pageWidthPoints: numberValue(native.PageWidthPoints),
      pageHeightPoints: numberValue(native.PageHeightPoints),
      pageRotation: numberValue(native.PageRotation),
      contentBase64: native.ContentBase64,
      orientation: native.Orientation,
      dimensions: (native.Dimensions ?? []).map((item: any) => ({
        code: item.Code,
        valueMillimeters:
          item.ValueMillimeters == null ? null : numberValue(item.ValueMillimeters),
      })),
      additionalDimensions: (native.AdditionalDimensions ?? []).map((item: any) => ({
        code: item.Code,
        valueMillimeters:
          item.ValueMillimeters == null ? null : numberValue(item.ValueMillimeters),
      })),
      visibleDimensions: (native.VisibleDimensions ?? []).map((item: any) => ({
        code: item.Code,
        valueMillimeters: numberValue(item.ValueMillimeters),
      })),
      unitWeightKilograms: native.UnitWeightKilograms == null ? null : numberValue(native.UnitWeightKilograms),
      packaging: native.Packaging
        ? {
            orientation: native.Packaging.Orientation,
            palletLengthMillimeters: native.Packaging.PalletLengthMillimeters == null ? null : numberValue(native.Packaging.PalletLengthMillimeters),
            palletWidthMillimeters: native.Packaging.PalletWidthMillimeters == null ? null : numberValue(native.Packaging.PalletWidthMillimeters),
            palletHeightMillimeters: native.Packaging.PalletHeightMillimeters == null ? null : numberValue(native.Packaging.PalletHeightMillimeters),
            maxUnits: native.Packaging.MaxUnits == null ? null : numberValue(native.Packaging.MaxUnits),
            palletWeightKilograms: native.Packaging.PalletWeightKilograms == null ? null : numberValue(native.Packaging.PalletWeightKilograms),
            totalWeightKilograms: native.Packaging.TotalWeightKilograms == null ? null : numberValue(native.Packaging.TotalWeightKilograms),
          }
        : undefined,
    } as DimensionalDrawingState;
  }

  async downloadDimensionalDrawing(draft: SelectionDraft, imageBase64: string) {
    return nativeInvoke<{ saved: boolean; cancelled?: boolean; fileName?: string }>(
      "drawing.download",
      {
        modelCode: draft.selectedUnitId,
        layoutCode: draft.layoutCode,
        imageBase64,
      },
    );
  }

  async listNotifications() {
    return nativeInvoke<FollowUpCenterState>("notifications.list");
  }

  async getNotificationSummary() {
    return nativeInvoke<FollowUpCenterState>("notifications.peek");
  }

  async updateNotification(
    id: string,
    action: "reschedule" | "succeeded" | "unsuccessful",
    days?: number,
  ) {
    return nativeInvoke<FollowUpCenterState>("notifications.action", {
      id,
      action,
      days,
    });
  }

  async openNotificationTarget(id: string, currentDraft: SelectionDraft) {
    const response = await nativeInvoke<any>("notifications.openTarget", { id });
    if (response.openedProject) {
      return {
        ...response,
        multiProject: response.project as MultiProjectState,
      };
    }
    if (!response.opened || !response.input) return response;
    return {
      ...response,
      draft: this.mapOpenedDraft(response.input, currentDraft),
    };
  }

  async getMultiProject(reference: string, languageCode: string) {
    return nativeInvoke<MultiProjectState>("project.workspace", {
      reference,
      languageCode,
    });
  }

  async newMultiProject(reference: string, languageCode: string) {
    return nativeInvoke<MultiProjectState>("project.workspaceNew", {
      reference,
      languageCode,
    });
  }

  async openMultiProject() {
    return nativeInvoke<{
      opened: boolean;
      cancelled?: boolean;
      project?: MultiProjectState;
    }>("project.workspaceOpen");
  }

  async saveMultiProject(
    reference: string,
    languageCode: string,
    saveAs = false,
  ) {
    return nativeInvoke<{
      saved: boolean;
      cancelled?: boolean;
      project: MultiProjectState;
    }>(
      saveAs ? "project.workspaceSaveAs" : "project.workspaceSave",
      { reference, languageCode },
    );
  }

  async addCurrentToMultiProject(draft: SelectionDraft, createNew = false) {
    return nativeInvoke<MultiProjectState>(
      "project.workspaceAddCurrent",
      { ...this.draftPayload(draft), createNew },
    );
  }

  async removeMultiProjectItem(itemId: string) {
    return nativeInvoke<MultiProjectState>("project.workspaceRemove", { itemId });
  }

  async openMultiProjectItem(itemId: string, currentDraft: SelectionDraft) {
    const response = await nativeInvoke<any>("project.workspaceOpenItem", { itemId });
    if (!response.opened || !response.input) return response;
    const openedDraft = this.mapOpenedDraft(response.input, currentDraft);
    openedDraft.project.language = currentDraft.project.language;
    return {
      ...response,
      draft: openedDraft,
    };
  }

  async changeMultiProjectLanguage(
    reference: string,
    languageCode: string,
  ) {
    return nativeInvoke<MultiProjectState>("project.workspaceLanguage", {
      reference,
      languageCode,
    });
  }

  async emailMultiProject(
    reference: string,
    languageCode: string,
    schedule: boolean,
    days: number,
  ) {
    return nativeInvoke<{
      prepared: boolean;
      cancelled?: boolean;
      project?: MultiProjectState;
    }>("project.workspaceEmail", {
      reference,
      languageCode,
      schedule,
      days,
    });
  }

  private mapOpenedDraft(native: any, current: SelectionDraft): SelectionDraft {
    const result = structuredClone(current);
    result.project.name = native.ProjectName || result.project.name;
    result.project.customerReference = native.CustomerReference || "";
    result.project.language = native.LanguageCode || result.project.language;
    result.selectedUnitId = native.ModelCode || result.selectedUnitId;
    result.operatingPoint.supplyAirflow = numberValue(native.SupplyAirflowM3h);
    result.operatingPoint.extractAirflow = numberValue(native.ExtractAirflowM3h);
    result.operatingPoint.pressure = numberValue(native.PressurePa);
    result.imbalanceEnabled = Boolean(native.ImbalanceEnabled);
    result.regulationPercent = numberValue(native.RegulationPercent);
    result.summerEnabled = Boolean(native.SummerEnabled);
    result.winterOutdoorTemperature = numberValue(native.WinterOutdoorTemperatureC);
    result.winterOutdoorRh = numberValue(native.WinterOutdoorRhPercent);
    result.winterReturnTemperature = numberValue(native.WinterReturnTemperatureC);
    result.winterReturnRh = numberValue(native.WinterReturnRhPercent);
    result.summerOutdoorTemperature = numberValue(native.SummerOutdoorTemperatureC);
    result.summerOutdoorRh = numberValue(native.SummerOutdoorRhPercent);
    result.summerReturnTemperature = numberValue(native.SummerReturnTemperatureC);
    result.summerReturnRh = numberValue(native.SummerReturnRhPercent);
    result.installationMode = native.InstallationMode || "Ceiling";
    result.layoutCode = native.LayoutCode || result.layoutCode;
    result.waterCoilEnabled = Boolean(native.WaterCoilEnabled);
    result.waterCoilId = numberValue(native.WaterCoilId);
    result.waterCoilMode = native.WaterCoilMode || "HCD";
    result.waterCoilCustomized = Boolean(native.WaterCoilCustomized);
    result.waterCoilCustomDisclaimerAccepted =
      Boolean(native.WaterCoilCustomDisclaimerAccepted);
    result.waterCoilLengthMm = numberValue(native.WaterCoilLengthMm);
    result.waterCoilHeightMm = numberValue(native.WaterCoilHeightMm);
    result.waterCoilRows = numberValue(native.WaterCoilRows);
    result.waterCoilCircuits = numberValue(native.WaterCoilCircuits);
    result.waterCoilFinSpacingMm = numberValue(native.WaterCoilFinSpacingMm);
    result.fluidCode = native.FluidCode || "Water";
    result.glycolPercent = numberValue(native.GlycolPercent);
    result.coolingWaterInletTemperature =
      numberValue(native.CoolingWaterInletTemperatureC);
    result.coolingWaterOutletTemperature =
      numberValue(native.CoolingWaterOutletTemperatureC);
    result.heatingWaterInletTemperature =
      numberValue(native.HeatingWaterInletTemperatureC);
    result.heatingWaterOutletTemperature =
      numberValue(native.HeatingWaterOutletTemperatureC);
    result.electricPreheaterEnabled = Boolean(native.ElectricPreheaterEnabled);
    result.electricPreheaterId = numberValue(native.ElectricPreheaterId);
    result.electricPostheaterEnabled = Boolean(native.ElectricPostheaterEnabled);
    result.electricPostheaterId = numberValue(native.ElectricPostheaterId);
    result.accessoryCodes = Array.isArray(native.AccessoryCodes)
      ? native.AccessoryCodes
      : [];
    const nativeCo2 = native.Co2 ?? {};
    result.co2 = {
      includeInReport: Boolean(nativeCo2.IncludeInReport),
      roomWidthMeters: numberValue(nativeCo2.RoomWidthMeters, 7),
      roomLengthMeters: numberValue(nativeCo2.RoomLengthMeters, 8),
      roomHeightMeters: numberValue(nativeCo2.RoomHeightMeters, 3),
      activityMet: numberValue(nativeCo2.ActivityMet, 1.2),
      occupiedPeople: numberValue(nativeCo2.OccupiedPeople, 20),
      occupiedMinutes: numberValue(nativeCo2.OccupiedMinutes, 45),
      breakPeople: numberValue(nativeCo2.BreakPeople, 0),
      breakMinutes: numberValue(nativeCo2.BreakMinutes, 15),
      calculationMethod:
        nativeCo2.CalculationMethod || "maximum-concentration",
      outdoorConcentrationPpm: numberValue(
        nativeCo2.OutdoorConcentrationPpm,
        380,
      ),
      maximumConcentrationPpm: numberValue(
        nativeCo2.MaximumConcentrationPpm,
        1000,
      ),
      airflowPerAreaLitersPerSecondPerSquareMeter: numberValue(
        nativeCo2.AirflowPerAreaLitersPerSecondPerSquareMeter,
        0.35,
      ),
      airflowPerPersonLitersPerSecond: numberValue(
        nativeCo2.AirflowPerPersonLitersPerSecond,
        10,
      ),
    };
    const nativeSound = native.Sound ?? {};
    const directivity = numberValue(nativeSound.DirectivityFactor, 2);
    result.sound = {
      includeInReport: Boolean(nativeSound.IncludeInReport),
      directivityFactor:
        directivity === 4 || directivity === 8 ? directivity : 2,
      distance1Meters: numberValue(nativeSound.Distance1Meters, 1),
      distance2Meters: numberValue(nativeSound.Distance2Meters, 3),
      iso16032Enabled: Boolean(nativeSound.Iso16032Enabled),
    };
    const nativeFilters = native.PreselectionFilters ?? {};
    const supplyDirectivity = numberValue(nativeFilters.SupplyNoiseDirectivity, 2);
    const breakoutDirectivity = numberValue(nativeFilters.BreakoutNoiseDirectivity, 2);
    result.preselectionFilters = {
      maximumSfpEnabled: Boolean(nativeFilters.MaximumSfpEnabled),
      maximumSfp: numberValue(nativeFilters.MaximumSfp, 2),
      supplyNoiseEnabled: Boolean(nativeFilters.SupplyNoiseEnabled),
      supplyNoiseMetric: nativeFilters.SupplyNoiseMetric === "LPA" ? "LPA" : "LWA",
      maximumSupplyNoiseDbA: numberValue(nativeFilters.MaximumSupplyNoiseDbA, 50),
      supplyNoiseDirectivityFactor:
        supplyDirectivity === 4 || supplyDirectivity === 8 ? supplyDirectivity : 2,
      supplyNoiseDistanceMeters: numberValue(nativeFilters.SupplyNoiseDistanceMeters, 1),
      breakoutNoiseEnabled: Boolean(nativeFilters.BreakoutNoiseEnabled),
      breakoutNoiseMetric: nativeFilters.BreakoutNoiseMetric === "LPA" ? "LPA" : "LWA",
      maximumBreakoutNoiseDbA: numberValue(nativeFilters.MaximumBreakoutNoiseDbA, 50),
      breakoutNoiseDirectivityFactor:
        breakoutDirectivity === 4 || breakoutDirectivity === 8 ? breakoutDirectivity : 2,
      breakoutNoiseDistanceMeters: numberValue(nativeFilters.BreakoutNoiseDistanceMeters, 1),
    };
    return result;
  }

  private calculateNative(
    draft: Parameters<SelectionBridge["calculate"]>[0],
  ): Promise<any> {
    return nativeInvoke("selection.calculate", this.draftPayload(draft));
  }

  private preselectNative(draft: SelectionDraft): Promise<NativePreselection[]> {
    return nativeInvoke("selection.preselect", this.draftPayload(draft));
  }

  private mapPreselection(
    item: NativePreselection,
    calculated?: any,
    calculatedModelCode?: string,
  ) {
    const model = item.Model;
    const isCalculated = model.Code === calculatedModelCode;
    return {
      id: model.Code,
      family: model.SeriesCode,
      model: model.Name || model.Code,
      maxAirflow: numberValue(model.NominalAirflowM3h),
      availablePressure: numberValue(item.AvailablePressurePa),
      efficiency: isCalculated
        ? numberValue(calculated?.Winter?.Curves?.WorkingPointEfficiencyPercent)
        : 0,
      soundPower: 0,
      fitScore: numberValue(item.RequiredRegulationPercent),
      requiredRegulation: numberValue(item.RequiredRegulationPercent),
      absorbedPower: numberValue(item.AbsorbedPowerW),
      sfp: numberValue(item.CombinedSfp),
      supplySoundPowerDbA: item.SupplySoundPowerDbA == null ? undefined : numberValue(item.SupplySoundPowerDbA),
      supplySoundPressureDbA: item.SupplySoundPressureDbA == null ? undefined : numberValue(item.SupplySoundPressureDbA),
      breakoutSoundPowerDbA: item.BreakoutSoundPowerDbA == null ? undefined : numberValue(item.BreakoutSoundPowerDbA),
      breakoutSoundPressureDbA: item.BreakoutSoundPressureDbA == null ? undefined : numberValue(item.BreakoutSoundPressureDbA),
    };
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
      imbalanceEnabled: draft.imbalanceEnabled,
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
      waterCoilCustomDisclaimerAccepted:
        draft.waterCoilCustomDisclaimerAccepted,
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
      installationMode: draft.installationMode,
      layoutCode: draft.layoutCode,
      co2: {
        includeInReport: draft.co2.includeInReport,
        roomWidthMeters: draft.co2.roomWidthMeters,
        roomLengthMeters: draft.co2.roomLengthMeters,
        roomHeightMeters: draft.co2.roomHeightMeters,
        activityMet: draft.co2.activityMet,
        occupiedPeople: draft.co2.occupiedPeople,
        occupiedMinutes: draft.co2.occupiedMinutes,
        breakPeople: draft.co2.breakPeople,
        breakMinutes: draft.co2.breakMinutes,
        calculationMethod: draft.co2.calculationMethod,
        outdoorConcentrationPpm: draft.co2.outdoorConcentrationPpm,
        maximumConcentrationPpm: draft.co2.maximumConcentrationPpm,
        airflowPerAreaLitersPerSecondPerSquareMeter:
          draft.co2.airflowPerAreaLitersPerSecondPerSquareMeter,
        airflowPerPersonLitersPerSecond:
          draft.co2.airflowPerPersonLitersPerSecond,
      },
      sound: {
        includeInReport: draft.sound.includeInReport,
        directivityFactor: draft.sound.directivityFactor,
        distance1Meters: draft.sound.distance1Meters,
        distance2Meters: draft.sound.distance2Meters,
        iso16032Enabled: draft.sound.iso16032Enabled,
      },
      preselectionFilters: { ...draft.preselectionFilters },
    };
  }

  private mapResult(native: any, requiredPressure: number) {
    const winter = native.Winter;
    const summer = native.Summer;
    const pressureMargin =
      numberValue(winter?.Curves?.WorkingPointPressurePa) - requiredPressure;
    const pressure = Math.max(0, pressureMargin);
    const winterThermo = winter?.Result?.Thermodynamics;
    const summerThermo = summer?.Result?.Thermodynamics;
    const validationNotices = (native.Validation?.Issues ?? []).map(
      (issue: { MessageKey?: string; Code?: string; Severity?: number | string }) => {
        const severityValue = String(issue.Severity ?? "").toLowerCase();
        const severity =
          severityValue === "2" || severityValue === "error"
            ? ("danger" as const)
            : severityValue === "0" || severityValue === "information"
              ? ("information" as const)
              : ("warning" as const);
        return {
          message:
            issue.MessageKey ||
            issue.Code ||
            "Configuration requires verification.",
          severity,
        };
      },
    );
    const pressureExceeded =
      Boolean(native.PressureCapacityExceeded) || pressureMargin < -0.5;
    const pressureMessage =
      native.PressureCapacityExceededMessage ||
      "The required airflow and pressure cannot be reached even at 100% regulation.";
    const notices = [
      ...(pressureExceeded
        ? [{ message: pressureMessage, severity: "danger" as const }]
        : []),
      ...validationNotices,
    ];
    const invalid =
      pressureExceeded ||
      notices.some((notice) => notice.severity === "danger");
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
        : notices.length > 0
          ? ("warning" as const)
          : ("valid" as const),
      messages: notices.map((notice) => notice.message),
      notices,
      effectiveRegulationPercent: numberValue(
        native.EffectiveRegulationPercent,
      ),
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
        enabled: item.Enabled,
        disabledReason: item.DisabledReason,
      })),
      aeraulicConnectionCode: native.Layout?.AeraulicConnectionCode ?? "",
      layoutConfigurations: (native.Layout?.Configurations ?? []).map(
        (configuration: {
          Code: string;
          InstallationMode?: string;
          Orientation?: number;
          AccessSide?: "upper" | "lower" | "front";
          ReferenceView?: string;
          IsDefault?: boolean;
        }) => {
          const installationMode = String(configuration.InstallationMode ?? "")
            .trim()
            .toLowerCase();
          return {
            code: configuration.Code,
            orientation: configuration.Orientation === 1 ? "vertical" as const : "horizontal" as const,
            accessSide: configuration.AccessSide,
            referenceView: configuration.ReferenceView,
            installationMode:
              installationMode === "ceiling" ||
              installationMode === "floor" ||
              installationMode === "wall"
                ? installationMode
                : undefined,
            isDefault: Boolean(configuration.IsDefault),
          };
        },
      ),
      horizontalDimensions: (native.Layout?.HorizontalDimensions ?? []).map(
        (item: { Code: string; ValueMillimeters: number | null }) =>
          ({ code: item.Code, valueMillimeters: item.ValueMillimeters })),
      verticalDimensions: (native.Layout?.VerticalDimensions ?? []).map(
        (item: { Code: string; ValueMillimeters: number | null }) =>
          ({ code: item.Code, valueMillimeters: item.ValueMillimeters })),
      layoutCodes: (native.Layout?.Configurations ?? []).map(
        (configuration: { Code: string }) => configuration.Code,
      ),
      flowPorts: (native.Layout?.FlowPorts ?? []).map(
        (port: { FlowCode: "Fresh" | "Return" | "Supply" | "Exhaust"; Position: number }) => ({
          flowCode: port.FlowCode,
          position: numberValue(port.Position),
        }),
      ),
      waterCoils: (native.AvailableWaterCoils as NativeWaterCoil[] ?? []).map(
        (item) => ({
          id: item.Id,
          name: item.Name,
          mode: item.Mode,
          installation: item.Installation,
          installationLabel: item.InstallationLabel,
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
      waterHeatingEnabled: Boolean(native.WaterHeatingEnabled),
      waterHeatingDisabledReason: native.WaterHeatingDisabledReason || "",
      electricPostheaterEnabled: Boolean(native.ElectricPostheaterEnabled),
      electricPostheaterDisabledReason:
        native.ElectricPostheaterDisabledReason || "",
      waterCoilStandardLabel: native.WaterCoilStandardLabel || "Standard",
      waterCoilCustomizedLabel:
        native.WaterCoilCustomizedLabel || "Customized",
      waterCoilCustomDisclaimer: native.WaterCoilCustomDisclaimer || "",
      waterCoilDimensionsNotice: native.WaterCoilDimensionsNotice || "",
      waterCoilQuotationNotice: native.WaterCoilQuotationNotice || "",
      winterCurve: this.mapCurve(winter?.Curves),
      summerCurve: this.mapCurve(summer?.Curves),
      co2Result: native.Co2
        ? {
            roomAreaSquareMeters: numberValue(
              native.Co2.RoomAreaSquareMeters,
            ),
            roomVolumeCubicMeters: numberValue(
              native.Co2.RoomVolumeLiters,
            ) / 1000,
            carbonDioxideGenerationLitersPerSecondPerPerson: numberValue(
              native.Co2.Co2ProductionPerPersonLitersPerHour,
            ) / 3600,
            requiredOutdoorAirflowLitersPerSecond: numberValue(
              native.Co2.RequiredAirflowLitersPerSecond,
            ),
            requiredOutdoorAirflowCubicMetersPerHour: numberValue(
              native.Co2.RequiredAirflowM3h,
            ),
            calculatedMaximumConcentrationPpm: numberValue(
              native.Co2.MaximumCo2Ppm,
            ),
            points: (native.Co2.Points ?? []).map((point: any) => ({
              hours: numberValue(point.Hours),
              ppm: numberValue(point.Ppm),
            })),
          }
        : undefined,
      soundResult: native.Sound
        ? {
            iso16032Available: Boolean(native.Sound.Iso16032Available),
            spectrumRows: (native.Sound.Rows ?? []).map(
              (row: any) => ({
                airPathCode: String(row.Type ?? ""),
                airPathLabel: String(row.Caption ?? row.Type ?? ""),
                octaveBand63HzDb: numberValue(row.Bands?.[0]),
                octaveBand125HzDb: numberValue(row.Bands?.[1]),
                octaveBand250HzDb: numberValue(row.Bands?.[2]),
                octaveBand500HzDb: numberValue(row.Bands?.[3]),
                octaveBand1000HzDb: numberValue(row.Bands?.[4]),
                octaveBand2000HzDb: numberValue(row.Bands?.[5]),
                octaveBand4000HzDb: numberValue(row.Bands?.[6]),
                octaveBand8000HzDb: numberValue(row.Bands?.[7]),
                weightedSoundPowerDbA: numberValue(row.LwA),
                soundPressureAtDistance1DbA:
                  row.Lp1 === null || row.Lp1 === undefined
                    ? null
                    : numberValue(row.Lp1),
                soundPressureAtDistance2DbA:
                  row.Lp2 === null || row.Lp2 === undefined
                    ? null
                    : numberValue(row.Lp2),
              }),
            ),
          }
        : undefined,
    };
  }

  private mapCurve(native: any) {
    if (!native) return undefined;
    const values = (items: unknown): number[] =>
      Array.isArray(items) ? items.map((item) => numberValue(item)) : [];
    return {
      originalAirflows: values(native?.OriginalAirflows),
      originalPressures: values(native?.OriginalPressures),
      originalPowers: values(native?.OriginalPowers),
      regulatedAirflows: values(native?.RegulatedAirflows),
      regulatedPressures: values(native?.RegulatedPressures),
      regulatedPowers: values(native?.RegulatedPowers),
      efficienciesPercent: values(native?.EfficienciesPercent),
      workingPointAirflow: numberValue(native?.WorkingPointAirflow),
      workingPointPressurePa: numberValue(native?.WorkingPointPressurePa),
      workingPointPowerW: numberValue(native?.WorkingPointPowerW),
      workingPointEfficiencyPercent: numberValue(
        native?.WorkingPointEfficiencyPercent,
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
