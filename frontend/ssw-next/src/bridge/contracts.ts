export type StepId =
  | "project"
  | "preselection"
  | "installation"
  | "water-coil"
  | "electric-heaters"
  | "accessories"
  | "co2"
  | "sound"
  | "documents"
  | "summary";

export type InstallationMode = "ceiling" | "floor" | "wall";
export type CoilMode = "CWD" | "HWD" | "HCD";

export interface LayoutConfigurationOption {
  code: string;
  installationMode?: InstallationMode;
  orientation?: "horizontal" | "vertical";
  accessSide?: "upper" | "lower" | "front";
  referenceView?: string;
  isDefault: boolean;
}

export interface ProjectInfo {
  name: string;
  customerReference: string;
  language: string;
}

export interface OperatingPoint {
  supplyAirflow: number;
  extractAirflow: number;
  pressure: number;
}

export interface PerformanceCurveData {
  originalAirflows: number[];
  originalPressures: number[];
  originalPowers: number[];
  regulatedAirflows: number[];
  regulatedPressures: number[];
  regulatedPowers: number[];
  efficienciesPercent: number[];
  workingPointAirflow: number;
  workingPointPressurePa: number;
  workingPointPowerW: number;
  workingPointEfficiencyPercent: number;
}

export interface UnitOption {
  id: string;
  family: string;
  model: string;
  maxAirflow: number;
  availablePressure: number;
  efficiency: number;
  soundPower: number;
  fitScore: number;
  requiredRegulation: number;
  absorbedPower: number;
  sfp: number;
  supplySoundPowerDbA?: number;
  supplySoundPressureDbA?: number;
  breakoutSoundPowerDbA?: number;
  breakoutSoundPressureDbA?: number;
}

export type NoiseMetric = "LWA" | "LPA";

export interface PreselectionFilterSettings {
  maximumSfpEnabled: boolean;
  maximumSfp: number;
  supplyNoiseEnabled: boolean;
  supplyNoiseMetric: NoiseMetric;
  maximumSupplyNoiseDbA: number;
  supplyNoiseDirectivityFactor: 2 | 4 | 8;
  supplyNoiseDistanceMeters: number;
  breakoutNoiseEnabled: boolean;
  breakoutNoiseMetric: NoiseMetric;
  maximumBreakoutNoiseDbA: number;
  breakoutNoiseDirectivityFactor: 2 | 4 | 8;
  breakoutNoiseDistanceMeters: number;
}

export interface AccessoryOption {
  code: string;
  name: string;
  category: string;
  installation: "Internal" | "External";
  included: boolean;
  locked?: boolean;
  enabled?: boolean;
  disabledReason?: string;
}

export interface WaterCoilOption {
  id: number;
  name: string;
  mode: CoilMode;
  installation: string;
  installationLabel?: string;
  lengthMm: number;
  heightMm: number;
  rows: number;
  circuits: number;
  finSpacingMm: number;
}

export interface ElectricHeaterOption {
  id: number;
  code: string;
  name: string;
  mode: "PEHD" | "EHD";
  installation: string;
  powerW: number;
  voltageV: number;
  currentA: number;
  phaseCount: number;
  quantity: number;
  isDefault: boolean;
}

export interface WaterCoilPerformance {
  mode: CoilMode;
  status: string;
  capacityW: number;
  sensibleCapacityW: number;
  airOutletTemperatureC: number;
  airOutletRelativeHumidityPercent: number;
  condensateLitersPerHour: number;
  airPressureDropPa: number;
  fluidPressureDropKPa: number;
  fluidFlowLitersPerHour: number;
  fluidVelocityMetersPerSecond: number;
  faceVelocityMetersPerSecond: number;
}

export interface ElectricHeaterPerformance {
  mode: "PEHD" | "EHD";
  heaterCode: string;
  powerW: number;
  currentA: number;
  airInletTemperatureC: number;
  airOutletTemperatureC: number;
  airOutletRelativeHumidityPercent: number;
  airPressureDropPa: number;
}

export type Co2CalculationMethod =
  | "maximum-concentration"
  | "fixed-airflow"
  | "airflow-per-person-and-area";

export interface Co2Settings {
  includeInReport: boolean;
  roomWidthMeters: number;
  roomLengthMeters: number;
  roomHeightMeters: number;
  activityMet: number;
  occupiedPeople: number;
  occupiedMinutes: number;
  breakPeople: number;
  breakMinutes: number;
  calculationMethod: Co2CalculationMethod;
  outdoorConcentrationPpm: number;
  maximumConcentrationPpm: number;
  airflowPerAreaLitersPerSecondPerSquareMeter: number;
  airflowPerPersonLitersPerSecond: number;
}

export interface SoundSettings {
  includeInReport: boolean;
  directivityFactor: 2 | 4 | 8;
  distance1Meters: number;
  distance2Meters: number;
  iso16032Enabled: boolean;
}

export interface Co2CalculationResult {
  roomAreaSquareMeters: number;
  roomVolumeCubicMeters: number;
  carbonDioxideGenerationLitersPerSecondPerPerson: number;
  requiredOutdoorAirflowLitersPerSecond: number;
  requiredOutdoorAirflowCubicMetersPerHour: number;
  calculatedMaximumConcentrationPpm: number;
  points: Array<{
    hours: number;
    ppm: number;
  }>;
}

export interface SoundSpectrumRow {
  airPathCode: string;
  airPathLabel: string;
  octaveBand63HzDb: number;
  octaveBand125HzDb: number;
  octaveBand250HzDb: number;
  octaveBand500HzDb: number;
  octaveBand1000HzDb: number;
  octaveBand2000HzDb: number;
  octaveBand4000HzDb: number;
  octaveBand8000HzDb: number;
  weightedSoundPowerDbA: number;
  soundPressureAtDistance1DbA: number | null;
  soundPressureAtDistance2DbA: number | null;
}

export interface SoundCalculationResult {
  iso16032Available: boolean;
  spectrumRows: SoundSpectrumRow[];
}

export interface SelectionDraft {
  project: ProjectInfo;
  operatingPoint: OperatingPoint;
  imbalanceEnabled: boolean;
  regulationPercent: number;
  summerEnabled: boolean;
  winterOutdoorTemperature: number;
  winterOutdoorRh: number;
  winterReturnTemperature: number;
  winterReturnRh: number;
  summerOutdoorTemperature: number;
  summerOutdoorRh: number;
  summerReturnTemperature: number;
  summerReturnRh: number;
  selectedUnitId: string;
  installationMode: InstallationMode;
  layoutCode: string;
  waterCoilEnabled: boolean;
  waterCoilMode: CoilMode;
  waterCoilId: number;
  waterCoilCustomized: boolean;
  waterCoilCustomDisclaimerAccepted: boolean;
  waterCoilLengthMm: number;
  waterCoilHeightMm: number;
  waterCoilRows: number;
  waterCoilCircuits: number;
  waterCoilFinSpacingMm: number;
  fluidCode: "Water" | "Glic_Etil" | "Glic_Prop";
  glycolPercent: number;
  coolingWaterInletTemperature: number;
  coolingWaterOutletTemperature: number;
  heatingWaterInletTemperature: number;
  heatingWaterOutletTemperature: number;
  electricPreheaterEnabled: boolean;
  electricPreheaterId: number;
  electricPostheaterEnabled: boolean;
  electricPostheaterId: number;
  accessoryCodes: string[];
  co2: Co2Settings;
  sound: SoundSettings;
  preselectionFilters: PreselectionFilterSettings;
}

export interface SelectionResult {
  supplyTemperature: number;
  summerSupplyTemperature: number;
  availablePressure: number;
  winterEfficiency: number;
  summerEfficiency: number;
  absorbedPower: number;
  sfp: number;
  status: "valid" | "warning" | "invalid";
  messages: string[];
  notices?: Array<{
    message: string;
    severity: "information" | "warning" | "danger";
  }>;
  effectiveRegulationPercent?: number;
  accessories?: AccessoryOption[];
  aeraulicConnectionCode?: string;
  layoutConfigurations?: LayoutConfigurationOption[];
  horizontalDimensions?: { code: string; valueMillimeters: number | null }[];
  verticalDimensions?: { code: string; valueMillimeters: number | null }[];
  layoutCodes?: string[];
  flowPorts?: Array<{
    flowCode: "Fresh" | "Return" | "Supply" | "Exhaust";
    position: number;
  }>;
  waterCoils?: WaterCoilOption[];
  electricHeaters?: ElectricHeaterOption[];
  waterCoilResults?: WaterCoilPerformance[];
  electricHeaterResults?: ElectricHeaterPerformance[];
  additionalPressureDropPa?: number;
  waterHeatingEnabled?: boolean;
  waterHeatingDisabledReason?: string;
  electricPostheaterEnabled?: boolean;
  electricPostheaterDisabledReason?: string;
  waterCoilStandardLabel?: string;
  waterCoilCustomizedLabel?: string;
  waterCoilCustomDisclaimer?: string;
  waterCoilDimensionsNotice?: string;
  waterCoilQuotationNotice?: string;
  winterCurve?: PerformanceCurveData;
  summerCurve?: PerformanceCurveData;
  co2Result?: Co2CalculationResult;
  soundResult?: SoundCalculationResult;
}

export interface BootstrapData {
  draft: SelectionDraft;
  units: UnitOption[];
  accessories: AccessoryOption[];
  result: SelectionResult;
}

export interface ProjectSaveState {
  saved: boolean;
  cancelled?: boolean;
  savedAt: string;
  path: string;
  fileName: string;
  localReference: string;
  publicReference: string;
  revision: number;
}

export interface FollowUpReminder {
  id: string;
  targetType: "Selection" | "Project";
  reference: string;
  localPath: string;
  preparedAt: string;
  dueAt: string;
  status: "Pending" | "Succeeded" | "Unsuccessful" | "Cancelled";
  rescheduleCount: number;
  unread: boolean;
  due: boolean;
  fileAvailable: boolean;
}

export interface FollowUpCenterState {
  unreadDueCount: number;
  reminders: FollowUpReminder[];
}

export interface ProductDocumentState {
  commercialSheetAvailable: boolean;
  installationManualAvailable: boolean;
  opened?: boolean;
  available?: boolean;
}

export interface DimensionalDrawingState {
  available: boolean;
  code?: string;
  revision?: string;
  fileName?: string;
  mimeType?: string;
  sha256?: string;
  pageWidthPoints?: number;
  pageHeightPoints?: number;
  pageRotation?: number;
  contentBase64?: string;
  orientation?: "H" | "V";
  dimensions: Array<{
    code: "A" | "B" | "C" | "D";
    valueMillimeters: number | null;
  }>;
  additionalDimensions?: Array<{
    code: string;
    valueMillimeters: number | null;
  }>;
  visibleDimensions?: Array<{
    code: string;
    valueMillimeters: number;
  }>;
  unitWeightKilograms?: number | null;
  packaging?: {
    orientation: "H" | "V";
    palletLengthMillimeters: number | null;
    palletWidthMillimeters: number | null;
    palletHeightMillimeters: number | null;
    maxUnits: number | null;
    palletWeightKilograms: number | null;
    totalWeightKilograms: number | null;
  };
}

export interface MultiProjectItem {
  itemId: string;
  selectionProjectId: string;
  customerReference: string;
  unitName: string;
  airflow?: number;
  pressure?: number;
  pdfFileName: string;
  languageCode: string;
  current: boolean;
  ready: boolean;
}

export interface MultiProjectState {
  loaded: boolean;
  dirty: boolean;
  path: string;
  fileName: string;
  reference: string;
  languageCode: string;
  modifiedAt: string;
  items: MultiProjectItem[];
}

export interface SelectionBridge {
  bootstrap(): Promise<BootstrapData>;
  preselect(draft: SelectionDraft): Promise<UnitOption[]>;
  calculate(draft: SelectionDraft): Promise<SelectionResult>;
  saveDraft(
    draft: SelectionDraft,
    saveAs?: boolean,
  ): Promise<ProjectSaveState>;
  openDraft(
    currentDraft: SelectionDraft,
  ): Promise<{ opened: boolean; cancelled?: boolean; draft?: SelectionDraft; project?: ProjectSaveState }>;
  generateReport(
    draft: SelectionDraft,
  ): Promise<{ fileName: string; delegated?: boolean }>;
  getProductDocuments(draft: SelectionDraft): Promise<ProductDocumentState>;
  openProductDocument(
    documentType: "commercial-sheet" | "installation-manual",
    draft: SelectionDraft,
  ): Promise<ProductDocumentState>;
  getDimensionalDrawing(draft: SelectionDraft): Promise<DimensionalDrawingState>;
  downloadDimensionalDrawing(
    draft: SelectionDraft,
    imageBase64: string,
  ): Promise<{ saved: boolean; cancelled?: boolean; fileName?: string }>;
  listNotifications(): Promise<FollowUpCenterState>;
  getNotificationSummary(): Promise<FollowUpCenterState>;
  updateNotification(
    id: string,
    action: "reschedule" | "succeeded" | "unsuccessful",
    days?: number,
  ): Promise<FollowUpCenterState>;
  openNotificationTarget(
    id: string,
    currentDraft: SelectionDraft,
  ): Promise<{
    opened: boolean;
    draft?: SelectionDraft;
    project?: ProjectSaveState;
    openedProject?: boolean;
    multiProject?: MultiProjectState;
  }>;
  getMultiProject(reference: string, languageCode: string): Promise<MultiProjectState>;
  newMultiProject(reference: string, languageCode: string): Promise<MultiProjectState>;
  openMultiProject(): Promise<{
    opened: boolean;
    cancelled?: boolean;
    project?: MultiProjectState;
  }>;
  saveMultiProject(
    reference: string,
    languageCode: string,
    saveAs?: boolean,
  ): Promise<{ saved: boolean; cancelled?: boolean; project: MultiProjectState }>;
  addCurrentToMultiProject(draft: SelectionDraft, createNew?: boolean): Promise<MultiProjectState>;
  removeMultiProjectItem(itemId: string): Promise<MultiProjectState>;
  openMultiProjectItem(
    itemId: string,
    currentDraft: SelectionDraft,
  ): Promise<{
    opened: boolean;
    draft?: SelectionDraft;
    selection?: ProjectSaveState;
    project?: MultiProjectState;
  }>;
  changeMultiProjectLanguage(
    reference: string,
    languageCode: string,
  ): Promise<MultiProjectState>;
  emailMultiProject(
    reference: string,
    languageCode: string,
    schedule: boolean,
    days: number,
  ): Promise<{ prepared: boolean; cancelled?: boolean; project?: MultiProjectState }>;
}
