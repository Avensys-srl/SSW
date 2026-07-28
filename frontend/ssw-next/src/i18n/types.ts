export const supportedLanguageCodes = [
  "bg",
  "cs",
  "da",
  "de",
  "en",
  "fr",
  "hu",
  "is",
  "it",
  "nl",
  "no",
  "pl",
  "ro",
  "sl",
  "sv",
] as const;

export type LanguageCode = (typeof supportedLanguageCodes)[number];

export const stepIds = [
  "project",
  "preselection",
  "unit",
  "installation",
  "water-coil",
  "electric-heaters",
  "accessories",
  "documents",
  "summary",
] as const;

export type StepId = (typeof stepIds)[number];

export interface StepMessages {
  title: string;
  shortTitle: string;
  description: string;
}

export interface FrontendMessages {
  languageName: string;
  locale: string;
  common: {
    applicationName: string;
    technicalSelection: string;
    activeProject: string;
    configuration: string;
    optional: string;
    required: string;
    loading: string;
    noData: string;
  };
  actions: {
    back: string;
    next: string;
    cancel: string;
    close: string;
    edit: string;
    select: string;
    deselect: string;
    search: string;
    reset: string;
    calculate: string;
    save: string;
    saveAs: string;
    duplicate: string;
    createAlternative: string;
    generateReport: string;
    openHelp: string;
    showTooltips: string;
    hideTooltips: string;
  };
  status: {
    ready: string;
    calculating: string;
    saved: string;
    valid: string;
    warning: string;
    invalid: string;
    selected: string;
    notSelected: string;
    included: string;
    unavailable: string;
    internal: string;
    external: string;
  };
  steps: Record<StepId, StepMessages>;
  tooltips: {
    projectReference: string;
    preselectionFilters: string;
    unitChoice: string;
    installationLayout: string;
    waterCoil: string;
    electricHeaters: string;
    accessories: string;
    documents: string;
    summary: string;
    saveSelection: string;
    generateReport: string;
    help: string;
    notifications: string;
  };
}

export interface DomainMessages {
  installation: {
    ceiling: string;
    floor: string;
    wall: string;
    internal: string;
    external: string;
  };
  airflow: {
    fresh: string;
    return: string;
    exhaust: string;
    supply: string;
  };
  fluid: {
    water: string;
    ethyleneGlycol: string;
    propyleneGlycol: string;
  };
}

export interface UiMessages {
  aria: {
    notifications: string;
    selectionSteps: string;
    accessoryCategory: string;
  };
  navigation: {
    componentShowcase: string;
    currentSelection: string;
    saveDraft: string;
  };
  context: {
    noUnit: string;
    supply: string;
    extract: string;
    pressure: string;
    layout: string;
    efficiency: string;
    margin: string;
    power: string;
    configuration: string;
  };
  project: {
    dataTitle: string;
    dataDescription: string;
    name: string;
    defaultName: string;
    customerReference: string;
    referencePlaceholder: string;
    documentLanguage: string;
    statusTitle: string;
    statusDescription: string;
    technicalReference: string;
    registered: string;
    revision: string;
    current: string;
    lastSaved: string;
    today: string;
    local: string;
  };
  preselection: {
    dutyPoint: string;
    dutyPointDescription: string;
    supplyAirflow: string;
    extractAirflow: string;
    imbalance: string;
    staticPressure: string;
    balancedNotice: string;
    regulationPercent: string;
    seasonalConditions: string;
    summerEnabled: string;
    winter: string;
    summer: string;
    outdoorTemperature: string;
    outdoorRelativeHumidity: string;
    returnTemperature: string;
    returnRelativeHumidity: string;
    results: string;
    compatibleUnits: string;
    orderedByFit: string;
    technicalFit: string;
    calculateAfterSelection: string;
    recommended: string;
  };
  unit: {
    selected: string;
    catalogue: string;
    maximumAirflow: string;
    availablePressure: string;
    nominalEfficiency: string;
    soundPower: string;
    currentUnit: string;
    chooseUnit: string;
  };
  installation: {
    typeTitle: string;
    typeDescription: string;
    ceilingDescription: string;
    floorDescription: string;
    wallDescription: string;
    airflowConfiguration: string;
    defaultHint: string;
    orientationTitle: string;
    previewDescription: string;
    lowerAccess: string;
    accessPanel: string;
  };
  waterCoil: {
    treatment: string;
    enable: string;
    calculationMode: string;
    coil: string;
    fluid: string;
    glycol: string;
    coolingIn: string;
    coolingOut: string;
    heatingIn: string;
    heatingOut: string;
    rows: string;
    circuits: string;
    finSpacing: string;
    calculatedPerformance: string;
    calculatedDescription: string;
    enableToCalculate: string;
    noneAssociated: string;
    additionalPressureDrop: string;
    capacity: string;
    airOut: string;
    airPressureDrop: string;
  };
  electricHeater: {
    preheating: string;
    postHeating: string;
    enableCalculation: string;
    unavailable: string;
    heater: string;
    installation: string;
    power: string;
    voltagePhases: string;
    current: string;
    airIn: string;
    maximumAirOut: string;
    relativeHumidityOut: string;
    airPressureDrop: string;
    optionalDescription: string;
  };
  accessories: {
    searchPlaceholder: string;
    allCategories: string;
    selectedCount: string;
    code: string;
    description: string;
    category: string;
    installation: string;
  };
  documents: {
    technicalSheet: string;
    technicalSheetDescription: string;
    installationManual: string;
    installationManualDescription: string;
    euDeclaration: string;
    euDeclarationDescription: string;
    dimensionalDrawing: string;
    dimensionalDrawingDescription: string;
    localPackageNotice: string;
    offlineAvailable: string;
    openDocument: string;
  };
  summary: {
    readyForReport: string;
    configuration: string;
    project: string;
    reference: string;
    unit: string;
    installation: string;
    dutyPoint: string;
    supplyAirflow: string;
    extractAirflow: string;
    requestedPressure: string;
    availableMargin: string;
    options: string;
    waterCoil: string;
    notSelectedFeminine: string;
    notSelectedMasculine: string;
    electricPreheating: string;
    electricPostHeating: string;
    accessories: string;
    technicalReport: string;
    reportDescription: string;
  };
  showcase: {
    localDesignSystem: string;
    title: string;
    description: string;
    backToSelection: string;
    actions: string;
    primary: string;
    secondary: string;
    destructive: string;
    fields: string;
    text: string;
    exampleValue: string;
    airflow: string;
    mode: string;
    activeOption: string;
    messages: string;
    allChecksPassed: string;
    pressureMarginReduced: string;
    correctDutyPoint: string;
    metricsAndTokens: string;
  };
  toast: {
    completeSave: string;
    draftSavedAt: string;
    completeReport: string;
    reportReady: string;
  };
}

export interface LocalizedFrontendMessages extends FrontendMessages {
  domain: DomainMessages;
  ui: UiMessages;
}
