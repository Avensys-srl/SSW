import type { SelectionDraft, SelectionResult } from "./contracts";

// Normalize before publishing: the returned result must have been calculated
// from exactly this effective configuration, including disabled treatments.
export const normalizeSelection = (input: SelectionDraft, result: SelectionResult): SelectionDraft => {
  const draft = structuredClone(input);
  const layouts = result.layoutConfigurations ?? [];
  const compatible = layouts.filter((item) => item.installationMode === draft.installationMode);
  const layout = compatible.find((item) => item.code === draft.layoutCode)
    ?? compatible.find((item) => item.isDefault) ?? compatible[0]
    ?? layouts.find((item) => item.isDefault) ?? layouts[0];
  if (layout) {
    draft.layoutCode = layout.code;
    if (layout.installationMode) draft.installationMode = layout.installationMode;
  } else if (result.layoutConfigurations) {
    draft.layoutCode = "";
  }
  if (result.soundResult && !result.soundResult.iso16032Available) draft.sound.iso16032Enabled = false;
  draft.regulationPercent = Math.max(draft.regulationPercent, result.effectiveRegulationPercent ?? 0);
  if (result.accessories) draft.accessoryCodes = result.accessories.filter((item) => item.included).map((item) => item.code);
  const coils = result.waterCoils ?? [];
  if (!coils.some((item) => item.id === draft.waterCoilId)) {
    const first = coils[0];
    draft.waterCoilEnabled = false;
    draft.waterCoilCustomized = false;
    draft.waterCoilId = first?.id ?? 0;
    if (first) {
      draft.waterCoilMode = first.mode;
      draft.waterCoilLengthMm = first.lengthMm;
      draft.waterCoilHeightMm = first.heightMm;
      draft.waterCoilRows = first.rows;
      draft.waterCoilCircuits = first.circuits;
      draft.waterCoilFinSpacingMm = first.finSpacingMm;
    }
  }
  const heaters = result.electricHeaters ?? [];
  const preheaters = heaters.filter((item) => item.mode === "PEHD");
  const postheaters = heaters.filter((item) => item.mode === "EHD");
  if (!preheaters.some((item) => item.id === draft.electricPreheaterId)) {
    draft.electricPreheaterEnabled = false;
    draft.electricPreheaterId = preheaters.find((item) => item.isDefault)?.id ?? preheaters[0]?.id ?? 0;
  }
  if (!postheaters.some((item) => item.id === draft.electricPostheaterId)) {
    draft.electricPostheaterEnabled = false;
    draft.electricPostheaterId = postheaters.find((item) => item.isDefault)?.id ?? postheaters[0]?.id ?? 0;
  }
  return draft;
};
