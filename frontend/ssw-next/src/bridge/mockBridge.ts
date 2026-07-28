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

  async preselect(draft: SelectionDraft) {
    await wait(160);
    return structuredClone(
      mockUnits
        .filter(
          (unit) =>
            draft.operatingPoint.supplyAirflow <= unit.maxAirflow &&
            draft.operatingPoint.pressure <= unit.availablePressure,
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
