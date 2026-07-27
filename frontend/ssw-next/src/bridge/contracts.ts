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

export interface SelectionDraft {
  project: ProjectInfo;
  operatingPoint: OperatingPoint;
  selectedUnitId: string;
  installationMode: InstallationMode;
  layoutCode: string;
  waterCoilEnabled: boolean;
  waterCoilMode: CoilMode;
  electricPreheaterEnabled: boolean;
  electricPostheaterEnabled: boolean;
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
