import type {
  AccessoryOption,
  BootstrapData,
  SelectionDraft,
  SelectionResult,
  UnitOption,
} from "../bridge/contracts";

export const mockUnits: UnitOption[] = [
  {
    id: "clrc-038-osc",
    family: "ECOP",
    model: "CLRC 038 OSC",
    maxAirflow: 500,
    availablePressure: 426,
    efficiency: 95,
    soundPower: 43,
    fitScore: 98,
  },
  {
    id: "clrc-048-osc",
    family: "ECOP",
    model: "CLRC 048 OSC",
    maxAirflow: 600,
    availablePressure: 438,
    efficiency: 94,
    soundPower: 45,
    fitScore: 91,
  },
  {
    id: "clrc-06a-osc",
    family: "HAKUNA",
    model: "CLRC 06A OSC",
    maxAirflow: 650,
    availablePressure: 473,
    efficiency: 93,
    soundPower: 44,
    fitScore: 87,
  },
];

export const mockAccessories: AccessoryOption[] = [
  {
    code: "KTS EXTRA",
    name: "Touchscreen controller",
    category: "Control interfaces",
    installation: "External",
    included: true,
    locked: true,
  },
  {
    code: "DPC",
    name: "CO2 duct probe",
    category: "Indoor air quality",
    installation: "External",
    included: false,
  },
  {
    code: "IDPH",
    name: "Internal humidity sensor",
    category: "Indoor air quality",
    installation: "Internal",
    included: false,
  },
  {
    code: "CAFS",
    name: "Constant airflow control - supply",
    category: "Airflow control",
    installation: "Internal",
    included: false,
  },
  {
    code: "CAFR",
    name: "Constant airflow control - return",
    category: "Airflow control",
    installation: "Internal",
    included: false,
  },
  {
    code: "MOD TCP/IP",
    name: "Modbus TCP/IP gateway",
    category: "Communication",
    installation: "Internal",
    included: false,
  },
];

export const mockDraft: SelectionDraft = {
  project: {
    name: "Progetto 01",
    customerReference: "Uffici direzionali - piano 2",
    language: "Italiano",
  },
  operatingPoint: {
    supplyAirflow: 300,
    extractAirflow: 300,
    pressure: 220,
  },
  selectedUnitId: "clrc-038-osc",
  installationMode: "ceiling",
  layoutCode: "B6",
  waterCoilEnabled: false,
  waterCoilMode: "HCD",
  electricPreheaterEnabled: false,
  electricPostheaterEnabled: false,
  accessoryCodes: ["KTS EXTRA"],
};

export const mockResult: SelectionResult = {
  supplyTemperature: 18.6,
  summerSupplyTemperature: 27.7,
  availablePressure: 206,
  winterEfficiency: 95,
  summerEfficiency: 94,
  absorbedPower: 176,
  sfp: 0.59,
  status: "valid",
  messages: [],
};

export const mockBootstrapData: BootstrapData = {
  draft: structuredClone(mockDraft),
  units: structuredClone(mockUnits),
  accessories: structuredClone(mockAccessories),
  result: structuredClone(mockResult),
};
