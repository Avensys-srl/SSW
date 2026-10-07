import type { PreselectionFilterSettings } from "./bridge/contracts";

export const resetPreselectionFilters = (filters: PreselectionFilterSettings): PreselectionFilterSettings => ({
  ...filters,
  maximumSfpEnabled: false,
  supplyNoiseEnabled: false,
  breakoutNoiseEnabled: false,
  rotaryOnlyEnabled: false,
  recoveryCategory: "any",
  recoveryOperation: "any",
  exchangerType: "any",
  unitApplication: "any",
  installationEnvironment: "any",
});
