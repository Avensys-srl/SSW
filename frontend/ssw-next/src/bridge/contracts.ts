export type StepId =
  | "project"
  | "preselection"
  | "unit"
  | "installation"
  | "water-coil"
  | "electric-heaters"
  | "accessories"
  | "documents"
  | "summary";

export type InstallationMode = "ceiling" | "floor" | "wall";
export type CoilMode = "CWD" | "HWD" | "HCD";

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

export interface UnitOption {
  id: string;
  family: string;
  model: string;
  maxAirflow: number;
  availablePressure: number;
  efficiency: number;
  soundPower: number;
  fitScore: number;
}

export interface AccessoryOption {
  code: string;
  name: string;
  category: string;
  installation: "Internal" | "External";
  included: boolean;
  locked?: boolean;
}

export interface WaterCoilOption {
  id: number;
  name: string;
  mode: CoilMode;
  installation: string;
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

export interface SelectionDraft {
  project: ProjectInfo;
  operatingPoint: OperatingPoint;
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
  accessories?: AccessoryOption[];
  layoutCodes?: string[];
  waterCoils?: WaterCoilOption[];
  electricHeaters?: ElectricHeaterOption[];
  waterCoilResults?: WaterCoilPerformance[];
  electricHeaterResults?: ElectricHeaterPerformance[];
  additionalPressureDropPa?: number;
}

export interface BootstrapData {
  draft: SelectionDraft;
  units: UnitOption[];
  accessories: AccessoryOption[];
  result: SelectionResult;
}

export interface SelectionBridge {
  bootstrap(): Promise<BootstrapData>;
  calculate(draft: SelectionDraft): Promise<SelectionResult>;
  saveDraft(
    draft: SelectionDraft,
  ): Promise<{ savedAt: string; delegated?: boolean }>;
  generateReport(
    draft: SelectionDraft,
  ): Promise<{ fileName: string; delegated?: boolean }>;
}
