import type {
  BootstrapData,
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
  async bootstrap(): Promise<BootstrapData> {
    await wait(180);
    return structuredClone(mockBootstrapData);
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
    };
  }

  async saveDraft(_draft: SelectionDraft): Promise<{ savedAt: string }> {
    await wait(180);
    return { savedAt: new Date().toISOString() };
  }

  async generateReport(
    draft: SelectionDraft,
  ): Promise<{ fileName: string }> {
    await wait(320);
    const unit = mockUnits.find((candidate) => candidate.id === draft.selectedUnitId);
    const safeModel = (unit?.model ?? "SSW").replaceAll(" ", "_");
    return { fileName: `${safeModel}_Technical_selection.pdf` };
  }
}
