import type {
  BootstrapData,
  FollowUpCenterState,
  DimensionalDrawingState,
  MultiProjectState,
  ProjectSaveState,
  SelectionBridge,
  SelectionDraft,
  SelectionResult,
} from "./contracts";
import {
  mockBootstrapData,
  mockResult,
  mockUnits,
} from "../mock/selection.fixture";

const wait = (milliseconds: number) =>
  new Promise<void>((resolve) => window.setTimeout(resolve, milliseconds));

export class MockSelectionBridge implements SelectionBridge {
  private multiProject: MultiProjectState = {
    loaded: true,
    dirty: false,
    path: "",
    fileName: "",
    reference: "Project 01",
    languageCode: "en",
    modifiedAt: new Date().toISOString(),
    items: [],
  };
  async bootstrap(): Promise<BootstrapData> {
    await wait(180);
    return structuredClone(mockBootstrapData);
  }

  async preselect(draft: SelectionDraft) {
    await wait(160);
    return structuredClone(
      mockUnits
        .filter(
          (unit) =>
            draft.operatingPoint.supplyAirflow <= unit.maxAirflow &&
            draft.operatingPoint.pressure <= unit.availablePressure &&
            (!draft.preselectionFilters.rotaryOnlyEnabled ||
              unit.family === "6" || unit.family === "9") &&
            (!draft.preselectionFilters.maximumSfpEnabled ||
              unit.sfp <= draft.preselectionFilters.maximumSfp) &&
            (!draft.preselectionFilters.supplyNoiseEnabled ||
              (draft.preselectionFilters.supplyNoiseMetric === "LPA"
                ? (unit.supplySoundPressureDbA ?? Infinity)
                : (unit.supplySoundPowerDbA ?? Infinity)) <=
                draft.preselectionFilters.maximumSupplyNoiseDbA) &&
            (!draft.preselectionFilters.breakoutNoiseEnabled ||
              (draft.preselectionFilters.breakoutNoiseMetric === "LPA"
                ? (unit.breakoutSoundPressureDbA ?? Infinity)
                : (unit.breakoutSoundPowerDbA ?? Infinity)) <=
                draft.preselectionFilters.maximumBreakoutNoiseDbA),
        )
        .sort((left, right) => left.sfp - right.sfp),
    );
  }

  async calculate(draft: SelectionDraft): Promise<SelectionResult> {
    await wait(240);
    const unit = mockUnits.find((candidate) => candidate.id === draft.selectedUnitId);
    const flowRatio = draft.operatingPoint.supplyAirflow / (unit?.maxAirflow ?? 500);
    const accessoryLoss = draft.accessoryCodes.length * 3;
    const coilLoss = draft.waterCoilEnabled ? 12 : 0;
    const heaterLoss =
      (draft.electricPreheaterEnabled ? 10 : 0) +
      (draft.electricPostheaterEnabled ? 10 : 0);
    const availablePressure = Math.max(
      0,
      (unit?.availablePressure ?? 0) -
        draft.operatingPoint.pressure -
        accessoryLoss -
        coilLoss -
        heaterLoss,
    );
    const status =
      availablePressure === 0
        ? "invalid"
        : availablePressure < 35
          ? "warning"
          : "valid";

    return {
      ...structuredClone(mockResult),
      availablePressure,
      winterEfficiency: Math.max(60, (unit?.efficiency ?? 90) - flowRatio * 3),
      summerEfficiency: Math.max(60, (unit?.efficiency ?? 90) - flowRatio * 4),
      absorbedPower: Math.round(130 + flowRatio * 95),
      sfp: Number((0.41 + flowRatio * 0.3).toFixed(2)),
      status,
      messages:
        status === "invalid"
          ? ["La pressione richiesta supera quella disponibile."]
          : status === "warning"
            ? ["Margine di pressione ridotto. Verificare gli accessori selezionati."]
            : [],
      waterCoilResults: draft.waterCoilEnabled
        ? [
            {
              mode: draft.waterCoilMode === "HWD" ? "HWD" : "CWD",
              status: "OK",
              capacityW: draft.waterCoilMode === "HWD" ? 1690 : 1340,
              sensibleCapacityW: draft.waterCoilMode === "HWD" ? 1690 : 362,
              airOutletTemperatureC:
                draft.waterCoilMode === "HWD" ? 68.9 : 17.1,
              airOutletRelativeHumidityPercent:
                draft.waterCoilMode === "HWD" ? 1 : 100,
              condensateLitersPerHour:
                draft.waterCoilMode === "HWD" ? 0 : 1.4,
              airPressureDropPa: 6,
              fluidPressureDropKPa: 32.4,
              fluidFlowLitersPerHour: 84.9,
              fluidVelocityMetersPerSecond: 0.29,
              faceVelocityMetersPerSecond: 0.56,
            },
          ]
        : [],
      electricHeaterResults: [
        ...(draft.electricPreheaterEnabled
          ? [{
              mode: "PEHD" as const,
              heaterCode: "EH-0.9-230",
              powerW: 900,
              currentA: 3.91,
              airInletTemperatureC: -10,
              airOutletTemperatureC: 16.9,
              airOutletRelativeHumidityPercent: 11,
              airPressureDropPa: 10,
            }]
          : []),
        ...(draft.electricPostheaterEnabled
          ? [{
              mode: "EHD" as const,
              heaterCode: "EH-0.75-230",
              powerW: 750,
              currentA: 3.26,
              airInletTemperatureC: 18.6,
              airOutletTemperatureC: 24.5,
              airOutletRelativeHumidityPercent: 7,
              airPressureDropPa: 10,
            }]
          : []),
      ],
      additionalPressureDropPa: coilLoss + heaterLoss,
    };
  }

  async saveDraft(
    _draft: SelectionDraft,
    _saveAs = false,
  ): Promise<ProjectSaveState> {
    await wait(180);
    return {
      saved: true,
      savedAt: new Date().toISOString(),
      path: "C:\\Selections\\SSW-selection.sswsel",
      fileName: "SSW-selection.sswsel",
      localReference: "D-NEXT-000001",
      publicReference: "",
      revision: 0,
    };
  }

  async openDraft(currentDraft: SelectionDraft) {
    return {
      opened: true,
      draft: structuredClone(currentDraft),
      project: await this.saveDraft(currentDraft),
    };
  }

  async generateReport(
    draft: SelectionDraft,
    _documentLanguageCode: string,
  ): Promise<{ fileName: string }> {
    await wait(320);
    const unit = mockUnits.find((candidate) => candidate.id === draft.selectedUnitId);
    const safeModel = (unit?.model ?? "SSW").replaceAll(" ", "_");
    return { fileName: `${safeModel}_Technical_selection.pdf` };
  }

  async listNotifications(): Promise<FollowUpCenterState> {
    return { unreadDueCount: 0, reminders: [] };
  }

  async getNotificationSummary(): Promise<FollowUpCenterState> {
    return { unreadDueCount: 0, reminders: [] };
  }

  async updateNotification(): Promise<FollowUpCenterState> {
    return { unreadDueCount: 0, reminders: [] };
  }

  async openNotificationTarget(
    _id: string,
    currentDraft: SelectionDraft,
  ) {
    return {
      opened: true,
      draft: structuredClone(currentDraft),
      project: await this.saveDraft(currentDraft),
    };
  }

  async getMultiProject(reference: string, languageCode: string) {
    this.multiProject.reference = reference;
    this.multiProject.languageCode = languageCode;
    return structuredClone(this.multiProject);
  }

  async newMultiProject(reference: string, languageCode: string) {
    this.multiProject = {
      loaded: true,
      dirty: true,
      path: "",
      fileName: "",
      reference,
      languageCode,
      modifiedAt: new Date().toISOString(),
      items: [],
    };
    return structuredClone(this.multiProject);
  }

  async openMultiProject() {
    return { opened: true, project: structuredClone(this.multiProject) };
  }

  async saveMultiProject(reference: string, languageCode: string) {
    this.multiProject.reference = reference;
    this.multiProject.languageCode = languageCode;
    this.multiProject.path = "C:\\Selections\\Project_01.sswproj";
    this.multiProject.fileName = "Project_01.sswproj";
    this.multiProject.dirty = false;
    return { saved: true, project: structuredClone(this.multiProject) };
  }

  async addCurrentToMultiProject(draft: SelectionDraft, createNew = false) {
    const unit = mockUnits.find((item) => item.id === draft.selectedUnitId);
    const existing = createNew ? undefined : this.multiProject.items.find((item) => item.current);
    this.multiProject.items = this.multiProject.items.filter((item) => item.itemId !== existing?.itemId).map((item) => ({ ...item, current: false }));
    this.multiProject.items.push({
      itemId: existing?.itemId ?? crypto.randomUUID(),
      selectionProjectId: existing?.selectionProjectId ?? crypto.randomUUID(),
      customerReference: draft.project.customerReference,
      unitName: unit?.model ?? draft.selectedUnitId,
      airflow: draft.operatingPoint.supplyAirflow,
      pressure: draft.operatingPoint.pressure,
      pdfFileName: `${(unit?.model ?? "SSW").replaceAll(" ", "_")}_Report.pdf`,
      languageCode: this.multiProject.languageCode,
      current: true,
      ready: true,
    });
    this.multiProject.dirty = true;
    return structuredClone(this.multiProject);
  }

  async removeMultiProjectItem(itemId: string) {
    this.multiProject.items = this.multiProject.items.filter((item) => item.itemId !== itemId);
    this.multiProject.dirty = true;
    return structuredClone(this.multiProject);
  }

  async openMultiProjectItem(_itemId: string, currentDraft: SelectionDraft) {
    return {
      opened: true,
      draft: structuredClone(currentDraft),
      project: structuredClone(this.multiProject),
    };
  }

  async changeMultiProjectLanguage(
    reference: string,
    languageCode: string,
  ) {
    this.multiProject.reference = reference;
    this.multiProject.languageCode = languageCode;
    this.multiProject.items = this.multiProject.items.map((item) => ({
      ...item,
      languageCode,
      pdfFileName: item.pdfFileName.replace(
        /_Report_[a-z]{2}\.pdf$/i,
        `_Report_${languageCode}.pdf`,
      ),
      ready: true,
    }));
    this.multiProject.dirty = true;
    this.multiProject.modifiedAt = new Date().toISOString();
    return structuredClone(this.multiProject);
  }

  async emailMultiProject() {
    return { prepared: true, project: structuredClone(this.multiProject) };
  }

  async getProductDocuments() {
    return {
      commercialSheetAvailable: true,
      installationManualAvailable: true,
      stepModelAvailable: true,
    };
  }

  async openProductDocument() {
    return {
      commercialSheetAvailable: true,
      installationManualAvailable: true,
      stepModelAvailable: true,
      available: true,
      opened: true,
    };
  }

  async getDimensionalDrawing(): Promise<DimensionalDrawingState> {
    return {
      available: false,
      orientation: "H",
      dimensions: [
        { code: "A", valueMillimeters: 1000 },
        { code: "B", valueMillimeters: 500 },
        { code: "C", valueMillimeters: 300 },
        { code: "D", valueMillimeters: 200 },
      ],
      visibleDimensions: [
        { code: "W", valueMillimeters: 1000 },
        { code: "L", valueMillimeters: 500 },
        { code: "H", valueMillimeters: 300 },
        { code: "D", valueMillimeters: 200 },
      ],
      unitWeightKilograms: 62,
    };
  }

  async downloadDimensionalDrawing() {
    return { saved: true, fileName: "dimensional-drawing.pdf" };
  }
}
