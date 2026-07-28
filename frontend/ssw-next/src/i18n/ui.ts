import { en } from "./en";
import { locales } from "./locales";
import type {
  DomainMessages,
  FrontendMessages,
  LanguageCode,
  UiMessages,
} from "./types";

interface UiLexicon {
  notifications: string;
  components: string;
  currentSelection: string;
  saveDraft: string;
  noUnit: string;
  supply: string;
  extract: string;
  pressure: string;
  layout: string;
  efficiency: string;
  margin: string;
  power: string;
  projectData: string;
  projectName: string;
  customerReference: string;
  freeReference: string;
  documentLanguage: string;
  projectStatus: string;
  technicalReference: string;
  registered: string;
  revision: string;
  current: string;
  lastSaved: string;
  today: string;
  local: string;
  dutyPoint: string;
  supplyAirflow: string;
  extractAirflow: string;
  staticPressure: string;
  results: string;
  compatibleUnits: string;
  orderedByFit: string;
  technicalFit: string;
  calculateAfterSelection: string;
  recommended: string;
  selected: string;
  catalogue: string;
  maximumAirflow: string;
  availablePressure: string;
  nominalEfficiency: string;
  soundPower: string;
  currentUnit: string;
  chooseUnit: string;
  airflowConfiguration: string;
  defaultHint: string;
  orientation: string;
  lowerAccess: string;
  accessPanel: string;
  enableCoil: string;
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
  performance: string;
  enableToCalculate: string;
  noneAssociated: string;
  additionalPressureDrop: string;
  capacity: string;
  airOut: string;
  airPressureDrop: string;
  preheating: string;
  postHeating: string;
  heater: string;
  voltagePhases: string;
  currentValue: string;
  airIn: string;
  maximumAirOut: string;
  relativeHumidityOut: string;
  searchPlaceholder: string;
  allCategories: string;
  selectedCount: string;
  code: string;
  description: string;
  category: string;
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
  readyForReport: string;
  reference: string;
  requestedPressure: string;
  options: string;
  notSelectedFeminine: string;
  notSelectedMasculine: string;
  technicalReport: string;
  reportDescription: string;
  completeSave: string;
  draftSavedAt: string;
  completeReport: string;
  reportReady: string;
}

export const domainMessages: Record<LanguageCode, DomainMessages> = {
  en: { installation: { ceiling: "Ceiling", floor: "Floor", wall: "Wall", internal: "Internal", external: "External" }, airflow: { fresh: "Fresh air", return: "Return air", exhaust: "Exhaust air", supply: "Supply air" }, fluid: { water: "Water", ethyleneGlycol: "Ethylene glycol", propyleneGlycol: "Propylene glycol" } },
  bg: { installation: { ceiling: "Таван", floor: "Под", wall: "Стена", internal: "Вътрешно", external: "Външно" }, airflow: { fresh: "Външен въздух", return: "Връщан въздух", exhaust: "Изхвърлян въздух", supply: "Подаван въздух" }, fluid: { water: "Вода", ethyleneGlycol: "Етиленгликол", propyleneGlycol: "Пропиленгликол" } },
  da: { installation: { ceiling: "Loft", floor: "Gulv", wall: "Væg", internal: "Intern", external: "Ekstern" }, airflow: { fresh: "Udeluft", return: "Returluft", exhaust: "Afkastluft", supply: "Tilluft" }, fluid: { water: "Vand", ethyleneGlycol: "Ethylenglykol", propyleneGlycol: "Propylenglykol" } },
  de: { installation: { ceiling: "Decke", floor: "Boden", wall: "Wand", internal: "Intern", external: "Extern" }, airflow: { fresh: "Außenluft", return: "Abluft", exhaust: "Fortluft", supply: "Zuluft" }, fluid: { water: "Wasser", ethyleneGlycol: "Ethylenglykol", propyleneGlycol: "Propylenglykol" } },
  fr: { installation: { ceiling: "Plafond", floor: "Sol", wall: "Mur", internal: "Interne", external: "Externe" }, airflow: { fresh: "Air neuf", return: "Air repris", exhaust: "Air rejeté", supply: "Air soufflé" }, fluid: { water: "Eau", ethyleneGlycol: "Éthylène glycol", propyleneGlycol: "Propylène glycol" } },
  hu: { installation: { ceiling: "Mennyezet", floor: "Padló", wall: "Fal", internal: "Belső", external: "Külső" }, airflow: { fresh: "Friss levegő", return: "Elszívott levegő", exhaust: "Kidobott levegő", supply: "Befúvó levegő" }, fluid: { water: "Víz", ethyleneGlycol: "Etilénglikol", propyleneGlycol: "Propilénglikol" } },
  is: { installation: { ceiling: "Loft", floor: "Gólf", wall: "Veggur", internal: "Innbyggt", external: "Utanáliggjandi" }, airflow: { fresh: "Útiloft", return: "Frásogsloft", exhaust: "Útblástursloft", supply: "Innblástursloft" }, fluid: { water: "Vatn", ethyleneGlycol: "Etýlen glýkól", propyleneGlycol: "Própýlen glýkól" } },
  it: { installation: { ceiling: "Soffitto", floor: "Pavimento", wall: "Parete", internal: "Interna", external: "Esterna" }, airflow: { fresh: "Aria esterna", return: "Ripresa", exhaust: "Espulsione", supply: "Mandata" }, fluid: { water: "Acqua", ethyleneGlycol: "Glicole etilico", propyleneGlycol: "Glicole propilenico" } },
  nl: { installation: { ceiling: "Plafond", floor: "Vloer", wall: "Wand", internal: "Intern", external: "Extern" }, airflow: { fresh: "Buitenlucht", return: "Retourlucht", exhaust: "Afvoerlucht", supply: "Toevoerlucht" }, fluid: { water: "Water", ethyleneGlycol: "Ethyleenglycol", propyleneGlycol: "Propyleenglycol" } },
  no: { installation: { ceiling: "Tak", floor: "Gulv", wall: "Vegg", internal: "Intern", external: "Ekstern" }, airflow: { fresh: "Uteluft", return: "Avtrekksluft", exhaust: "Avkastluft", supply: "Tilluft" }, fluid: { water: "Vann", ethyleneGlycol: "Etylenglykol", propyleneGlycol: "Propylenglykol" } },
  pl: { installation: { ceiling: "Sufit", floor: "Podłoga", wall: "Ściana", internal: "Wewnętrzna", external: "Zewnętrzna" }, airflow: { fresh: "Powietrze zewnętrzne", return: "Powietrze wywiewane", exhaust: "Powietrze wyrzutowe", supply: "Powietrze nawiewane" }, fluid: { water: "Woda", ethyleneGlycol: "Glikol etylenowy", propyleneGlycol: "Glikol propylenowy" } },
  ro: { installation: { ceiling: "Tavan", floor: "Podea", wall: "Perete", internal: "Internă", external: "Externă" }, airflow: { fresh: "Aer proaspăt", return: "Aer extras", exhaust: "Aer evacuat", supply: "Aer introdus" }, fluid: { water: "Apă", ethyleneGlycol: "Etilenglicol", propyleneGlycol: "Propilenglicol" } },
  sl: { installation: { ceiling: "Strop", floor: "Tla", wall: "Stena", internal: "Notranje", external: "Zunanje" }, airflow: { fresh: "Zunanji zrak", return: "Odvodni zrak", exhaust: "Izpušni zrak", supply: "Dovodni zrak" }, fluid: { water: "Voda", ethyleneGlycol: "Etilen glikol", propyleneGlycol: "Propilen glikol" } },
  sv: { installation: { ceiling: "Tak", floor: "Golv", wall: "Vägg", internal: "Intern", external: "Extern" }, airflow: { fresh: "Uteluft", return: "Frånluft", exhaust: "Avluft", supply: "Tilluft" }, fluid: { water: "Vatten", ethyleneGlycol: "Etylenglykol", propyleneGlycol: "Propylenglykol" } },
};

type SeasonalConditionMessages = Pick<
  UiMessages["preselection"],
  | "regulationPercent"
  | "seasonalConditions"
  | "summerEnabled"
  | "winter"
  | "summer"
  | "outdoorTemperature"
  | "outdoorRelativeHumidity"
  | "returnTemperature"
  | "returnRelativeHumidity"
>;

const seasonalConditionMessages: Record<LanguageCode, SeasonalConditionMessages> = {
  en: { regulationPercent: "Regulation level", seasonalConditions: "Seasonal conditions", summerEnabled: "Enable summer", winter: "Winter", summer: "Summer", outdoorTemperature: "Outdoor temp.", outdoorRelativeHumidity: "Outdoor R.H.", returnTemperature: "Return temp.", returnRelativeHumidity: "Return R.H." },
  bg: { regulationPercent: "Ниво на регулиране", seasonalConditions: "Сезонни условия", summerEnabled: "Активирай лято", winter: "Зима", summer: "Лято", outdoorTemperature: "Външна темп.", outdoorRelativeHumidity: "Външна отн. влажност", returnTemperature: "Темп. връщане", returnRelativeHumidity: "Отн. влажност връщане" },
  da: { regulationPercent: "Reguleringsniveau", seasonalConditions: "Sæsonbetingelser", summerEnabled: "Aktivér sommer", winter: "Vinter", summer: "Sommer", outdoorTemperature: "Udetemperatur", outdoorRelativeHumidity: "Ude-RF", returnTemperature: "Returtemperatur", returnRelativeHumidity: "Retur-RF" },
  de: { regulationPercent: "Regelungsniveau", seasonalConditions: "Saisonbedingungen", summerEnabled: "Sommer aktivieren", winter: "Winter", summer: "Sommer", outdoorTemperature: "Außentemperatur", outdoorRelativeHumidity: "Außen-R.F.", returnTemperature: "Ablufttemperatur", returnRelativeHumidity: "Abluft-R.F." },
  fr: { regulationPercent: "Niveau de régulation", seasonalConditions: "Conditions saisonnières", summerEnabled: "Activer l’été", winter: "Hiver", summer: "Été", outdoorTemperature: "Temp. extérieure", outdoorRelativeHumidity: "H.R. extérieure", returnTemperature: "Temp. air repris", returnRelativeHumidity: "H.R. air repris" },
  hu: { regulationPercent: "Szabályozási szint", seasonalConditions: "Évszakos feltételek", summerEnabled: "Nyár engedélyezése", winter: "Tél", summer: "Nyár", outdoorTemperature: "Külső hőm.", outdoorRelativeHumidity: "Külső relatív pára", returnTemperature: "Elszívott hőm.", returnRelativeHumidity: "Elszívott relatív pára" },
  is: { regulationPercent: "Stýristig", seasonalConditions: "Árstíðarskilyrði", summerEnabled: "Virkja sumar", winter: "Vetur", summer: "Sumar", outdoorTemperature: "Útihiti", outdoorRelativeHumidity: "Rakastig útilofts", returnTemperature: "Hitastig frásogs", returnRelativeHumidity: "Rakastig frásogs" },
  it: { regulationPercent: "Livello di regolazione", seasonalConditions: "Condizioni stagionali", summerEnabled: "Abilita estate", winter: "Inverno", summer: "Estate", outdoorTemperature: "Temp. aria esterna", outdoorRelativeHumidity: "U.R. aria esterna", returnTemperature: "Temp. aria di ripresa", returnRelativeHumidity: "U.R. aria di ripresa" },
  nl: { regulationPercent: "Regelniveau", seasonalConditions: "Seizoenscondities", summerEnabled: "Zomer inschakelen", winter: "Winter", summer: "Zomer", outdoorTemperature: "Buitentemperatuur", outdoorRelativeHumidity: "RV buitenlucht", returnTemperature: "Retourtemperatuur", returnRelativeHumidity: "RV retourlucht" },
  no: { regulationPercent: "Reguleringsnivå", seasonalConditions: "Sesongbetingelser", summerEnabled: "Aktiver sommer", winter: "Vinter", summer: "Sommer", outdoorTemperature: "Utetemperatur", outdoorRelativeHumidity: "RF uteluft", returnTemperature: "Avtrekkstemperatur", returnRelativeHumidity: "RF avtrekksluft" },
  pl: { regulationPercent: "Poziom regulacji", seasonalConditions: "Warunki sezonowe", summerEnabled: "Włącz lato", winter: "Zima", summer: "Lato", outdoorTemperature: "Temp. zewnętrzna", outdoorRelativeHumidity: "Wilg. wzgl. zewn.", returnTemperature: "Temp. wywiewu", returnRelativeHumidity: "Wilg. wzgl. wywiewu" },
  ro: { regulationPercent: "Nivel de reglare", seasonalConditions: "Condiții sezoniere", summerEnabled: "Activează vara", winter: "Iarnă", summer: "Vară", outdoorTemperature: "Temp. exterioară", outdoorRelativeHumidity: "U.R. exterioară", returnTemperature: "Temp. aer extras", returnRelativeHumidity: "U.R. aer extras" },
  sl: { regulationPercent: "Stopnja regulacije", seasonalConditions: "Sezonski pogoji", summerEnabled: "Omogoči poletje", winter: "Zima", summer: "Poletje", outdoorTemperature: "Zunanja temp.", outdoorRelativeHumidity: "Zunanja rel. vlaga", returnTemperature: "Temp. odvodnega zraka", returnRelativeHumidity: "Rel. vlaga odvodnega zraka" },
  sv: { regulationPercent: "Regleringsnivå", seasonalConditions: "Säsongsförhållanden", summerEnabled: "Aktivera sommar", winter: "Vinter", summer: "Sommar", outdoorTemperature: "Utetemperatur", outdoorRelativeHumidity: "RF uteluft", returnTemperature: "Frånluftstemperatur", returnRelativeHumidity: "RF frånluft" },
};

const lexicons: Record<LanguageCode, UiLexicon> = {
  en: {
    notifications: "Notification centre", components: "UI components", currentSelection: "Current selection", saveDraft: "Save draft", noUnit: "No unit", supply: "Supply", extract: "Extract", pressure: "Pressure", layout: "Layout", efficiency: "Efficiency", margin: "Margin", power: "Power",
    projectData: "Project data", projectName: "Project name", customerReference: "Customer reference", freeReference: "Free reference", documentLanguage: "Document language", projectStatus: "Project status", technicalReference: "Technical reference", registered: "Registered", revision: "Revision", current: "Current", lastSaved: "Last saved", today: "Today", local: "Local",
    dutyPoint: "Duty point", supplyAirflow: "Supply airflow", extractAirflow: "Extract airflow", staticPressure: "Static pressure", results: "Search results", compatibleUnits: "compatible units", orderedByFit: "SFP ↑", technicalFit: "Required regulation", calculateAfterSelection: "After selection", recommended: "Recommended", selected: "Selected", catalogue: "Catalogue", maximumAirflow: "Maximum airflow", availablePressure: "Available pressure", nominalEfficiency: "Nominal efficiency", soundPower: "Sound power", currentUnit: "Current unit", chooseUnit: "Choose unit",
    airflowConfiguration: "Airflow configuration", defaultHint: "Recommended default for the selected installation.", orientation: "Airflow orientation", lowerAccess: "Lower access", accessPanel: "Access panel",
    enableCoil: "Enable coil", calculationMode: "Calculation mode", coil: "Coil", fluid: "Fluid", glycol: "Glycol", coolingIn: "Cooling in", coolingOut: "Cooling out", heatingIn: "Heating in", heatingOut: "Heating out", rows: "rows", circuits: "circuits", finSpacing: "fin spacing", performance: "Calculated performance", enableToCalculate: "Enable the coil to calculate its performance.", noneAssociated: "No coil is associated with this unit in the local database.", additionalPressureDrop: "Additional pressure drop applied to the curve", capacity: "Capacity", airOut: "Air out", airPressureDrop: "Air DP",
    preheating: "Electric preheating", postHeating: "Electric post-heating", heater: "Electric heater", voltagePhases: "Voltage / phases", currentValue: "Current", airIn: "Air in", maximumAirOut: "Max. air out temp.", relativeHumidityOut: "R.H. out", searchPlaceholder: "Search code or description", allCategories: "All categories", selectedCount: "selected", code: "Code", description: "Description", category: "Category",
    technicalSheet: "Technical sheet", technicalSheetDescription: "Performance data and configuration", installationManual: "Installation manual", installationManualDescription: "Installation, connections and maintenance", euDeclaration: "EU declaration", euDeclarationDescription: "Compliance and applicable standards", dimensionalDrawing: "Dimensional drawing", dimensionalDrawingDescription: "Overall dimensions and configuration measurements", localPackageNotice: "Availability and generation are managed by the local production workflow according to model, configuration and language.", offlineAvailable: "Available offline", openDocument: "Open document",
    readyForReport: "The selection is ready for technical report generation.", reference: "Reference", requestedPressure: "Required pressure", options: "Options", notSelectedFeminine: "Not selected", notSelectedMasculine: "Not selected", technicalReport: "Technical report", reportDescription: "The document uses the language and reference of the current project.",
    completeSave: "Complete saving in the open production interface.", draftSavedAt: "Draft saved at", completeReport: "Complete report generation in the open production interface.", reportReady: "Report ready",
  },
  it: {
    notifications: "Centro notifiche", components: "Componenti UI", currentSelection: "Selezione corrente", saveDraft: "Salva bozza", noUnit: "Nessuna unità", supply: "Mandata", extract: "Ripresa", pressure: "Pressione", layout: "Layout", efficiency: "Rendimento", margin: "Margine", power: "Potenza",
    projectData: "Dati del progetto", projectName: "Nome progetto", customerReference: "Riferimento cliente", freeReference: "Riferimento libero", documentLanguage: "Lingua documenti", projectStatus: "Stato progetto", technicalReference: "Riferimento tecnico", registered: "Registrato", revision: "Revisione", current: "Corrente", lastSaved: "Ultimo salvataggio", today: "Oggi", local: "Locale",
    dutyPoint: "Punto di lavoro", supplyAirflow: "Portata mandata", extractAirflow: "Portata ripresa", staticPressure: "Pressione statica", results: "Risultato della ricerca", compatibleUnits: "unità compatibili", orderedByFit: "SFP ↑", technicalFit: "Regolazione richiesta", calculateAfterSelection: "Dopo la selezione", recommended: "Consigliata", selected: "Selezionata", catalogue: "Catalogo", maximumAirflow: "Portata massima", availablePressure: "Pressione disponibile", nominalEfficiency: "Rendimento nominale", soundPower: "Potenza sonora", currentUnit: "Unità corrente", chooseUnit: "Scegli unità",
    airflowConfiguration: "Configurazione flussi", defaultHint: "Default consigliato per l'installazione selezionata.", orientation: "Orientamento flussi", lowerAccess: "Accesso inferiore", accessPanel: "Pannello di accesso",
    enableCoil: "Abilita batteria", calculationMode: "Modo di calcolo", coil: "Batteria", fluid: "Fluido", glycol: "Glicole", coolingIn: "Freddo in", coolingOut: "Freddo out", heatingIn: "Caldo in", heatingOut: "Caldo out", rows: "ranghi", circuits: "circuiti", finSpacing: "passo", performance: "Prestazioni calcolate", enableToCalculate: "Abilita la batteria per calcolarne le prestazioni.", noneAssociated: "Nessuna batteria associata a questa unità nel database locale.", additionalPressureDrop: "Perdita aggiuntiva applicata alla curva", capacity: "Potenza", airOut: "Aria out", airPressureDrop: "DP aria",
    preheating: "Preriscaldo elettrico", postHeating: "Post-riscaldo elettrico", heater: "Batteria elettrica", voltagePhases: "Tensione / fasi", currentValue: "Corrente", airIn: "Aria in", maximumAirOut: "Temp. massima aria out", relativeHumidityOut: "U.R. out", searchPlaceholder: "Cerca codice o descrizione", allCategories: "Tutte le categorie", selectedCount: "selezionati", code: "Codice", description: "Descrizione", category: "Categoria",
    technicalSheet: "Scheda tecnica", technicalSheetDescription: "Dati prestazionali e configurazione", installationManual: "Manuale installazione", installationManualDescription: "Montaggio, collegamenti e manutenzione", euDeclaration: "Dichiarazione UE", euDeclarationDescription: "Conformità e norme applicabili", dimensionalDrawing: "Disegno dimensionale", dimensionalDrawingDescription: "Ingombri e quote della configurazione", localPackageNotice: "Disponibilità e generazione sono gestite dal workflow produttivo locale in base a modello, configurazione e lingua.", offlineAvailable: "Disponibile offline", openDocument: "Apri documento",
    readyForReport: "La selezione è pronta per la generazione del report tecnico.", reference: "Riferimento", requestedPressure: "Pressione richiesta", options: "Opzioni", notSelectedFeminine: "Non selezionata", notSelectedMasculine: "Non selezionato", technicalReport: "Report tecnico", reportDescription: "Il documento utilizzerà lingua e riferimento del progetto corrente.",
    completeSave: "Completa il salvataggio nell'interfaccia produttiva aperta.", draftSavedAt: "Bozza salvata alle", completeReport: "Completa la generazione del report nell'interfaccia produttiva aperta.", reportReady: "Report pronto",
  },
  bg: {
    notifications: "Център за известия", components: "UI компоненти", currentSelection: "Текущ подбор", saveDraft: "Запази чернова", noUnit: "Няма агрегат", supply: "Подаване", extract: "Връщане", pressure: "Налягане", layout: "Схема", efficiency: "Ефективност", margin: "Резерв", power: "Мощност",
    projectData: "Данни за проекта", projectName: "Име на проекта", customerReference: "Референция на клиента", freeReference: "Свободна референция", documentLanguage: "Език на документите", projectStatus: "Състояние на проекта", technicalReference: "Техническа референция", registered: "Регистрирано", revision: "Ревизия", current: "Текуща", lastSaved: "Последно записване", today: "Днес", local: "Локално",
    dutyPoint: "Работна точка", supplyAirflow: "Дебит подаване", extractAirflow: "Дебит връщане", staticPressure: "Статично налягане", results: "Резултати от търсенето", compatibleUnits: "съвместими агрегати", orderedByFit: "SFP ↑", technicalFit: "Необходимо регулиране", calculateAfterSelection: "След избора", recommended: "Препоръчано", selected: "Избрано", catalogue: "Каталог", maximumAirflow: "Максимален дебит", availablePressure: "Налично налягане", nominalEfficiency: "Номинална ефективност", soundPower: "Звукова мощност", currentUnit: "Текущ агрегат", chooseUnit: "Избери агрегат",
    airflowConfiguration: "Конфигурация на потоците", defaultHint: "Препоръчана стойност за избрания монтаж.", orientation: "Ориентация на потоците", lowerAccess: "Долен достъп", accessPanel: "Сервизен панел",
    enableCoil: "Активирай батерията", calculationMode: "Режим на изчисление", coil: "Батерия", fluid: "Флуид", glycol: "Гликол", coolingIn: "Охлаждане вход", coolingOut: "Охлаждане изход", heatingIn: "Отопление вход", heatingOut: "Отопление изход", rows: "реда", circuits: "кръга", finSpacing: "стъпка на ламелите", performance: "Изчислени характеристики", enableToCalculate: "Активирайте батерията, за да изчислите характеристиките.", noneAssociated: "В локалната база няма батерия за този агрегат.", additionalPressureDrop: "Допълнителен пад на налягане по кривата", capacity: "Мощност", airOut: "Въздух изход", airPressureDrop: "Пад въздух",
    preheating: "Електрическо предварително нагряване", postHeating: "Електрическо последващо нагряване", heater: "Електрически нагревател", voltagePhases: "Напрежение / фази", currentValue: "Ток", airIn: "Въздух вход", maximumAirOut: "Макс. температура изход", relativeHumidityOut: "Отн. влажност изход", searchPlaceholder: "Търси код или описание", allCategories: "Всички категории", selectedCount: "избрани", code: "Код", description: "Описание", category: "Категория",
    technicalSheet: "Технически лист", technicalSheetDescription: "Характеристики и конфигурация", installationManual: "Ръководство за монтаж", installationManualDescription: "Монтаж, свързване и поддръжка", euDeclaration: "ЕС декларация", euDeclarationDescription: "Съответствие и приложими стандарти", dimensionalDrawing: "Габаритен чертеж", dimensionalDrawingDescription: "Габарити и размери на конфигурацията", localPackageNotice: "Наличността и генерирането се управляват от локалния производствен процес според модела, конфигурацията и езика.", offlineAvailable: "Налично офлайн", openDocument: "Отвори документ",
    readyForReport: "Подборът е готов за технически отчет.", reference: "Референция", requestedPressure: "Необходимо налягане", options: "Опции", notSelectedFeminine: "Не е избрана", notSelectedMasculine: "Не е избран", technicalReport: "Технически отчет", reportDescription: "Документът използва езика и референцията на текущия проект.",
    completeSave: "Завършете записването в отворения производствен интерфейс.", draftSavedAt: "Черновата е записана в", completeReport: "Завършете отчета в отворения производствен интерфейс.", reportReady: "Отчетът е готов",
  },
  da: {
    notifications: "Notifikationscenter", components: "UI-komponenter", currentSelection: "Aktuelt valg", saveDraft: "Gem kladde", noUnit: "Intet aggregat", supply: "Tilluft", extract: "Returluft", pressure: "Tryk", layout: "Layout", efficiency: "Virkningsgrad", margin: "Reserve", power: "Effekt",
    projectData: "Projektdata", projectName: "Projektnavn", customerReference: "Kundereference", freeReference: "Fri reference", documentLanguage: "Dokumentsprog", projectStatus: "Projektstatus", technicalReference: "Teknisk reference", registered: "Registreret", revision: "Revision", current: "Aktuel", lastSaved: "Sidst gemt", today: "I dag", local: "Lokal",
    dutyPoint: "Driftspunkt", supplyAirflow: "Tilluftsmængde", extractAirflow: "Returluftsmængde", staticPressure: "Statisk tryk", results: "Søgeresultater", compatibleUnits: "kompatible aggregater", orderedByFit: "SFP ↑", technicalFit: "Nødvendig regulering", calculateAfterSelection: "Efter valg", recommended: "Anbefalet", selected: "Valgt", catalogue: "Katalog", maximumAirflow: "Maksimal luftmængde", availablePressure: "Tilgængeligt tryk", nominalEfficiency: "Nominel virkningsgrad", soundPower: "Lydeffekt", currentUnit: "Aktuelt aggregat", chooseUnit: "Vælg aggregat",
    airflowConfiguration: "Luftstrømskonfiguration", defaultHint: "Anbefalet standard for den valgte installation.", orientation: "Luftstrømsretning", lowerAccess: "Adgang nedefra", accessPanel: "Servicepanel",
    enableCoil: "Aktivér flade", calculationMode: "Beregningstilstand", coil: "Flade", fluid: "Væske", glycol: "Glykol", coolingIn: "Køling ind", coolingOut: "Køling ud", heatingIn: "Varme ind", heatingOut: "Varme ud", rows: "rækker", circuits: "kredse", finSpacing: "lamelafstand", performance: "Beregnede ydelser", enableToCalculate: "Aktivér fladen for at beregne ydelsen.", noneAssociated: "Ingen flade er knyttet til aggregatet i den lokale database.", additionalPressureDrop: "Ekstra tryktab på kurven", capacity: "Effekt", airOut: "Luft ud", airPressureDrop: "Lufttryktab",
    preheating: "Elektrisk forvarme", postHeating: "Elektrisk eftervarme", heater: "Elvarmeflade", voltagePhases: "Spænding / faser", currentValue: "Strøm", airIn: "Luft ind", maximumAirOut: "Maks. lufttemperatur ud", relativeHumidityOut: "RF ud", searchPlaceholder: "Søg kode eller beskrivelse", allCategories: "Alle kategorier", selectedCount: "valgt", code: "Kode", description: "Beskrivelse", category: "Kategori",
    technicalSheet: "Teknisk datablad", technicalSheetDescription: "Ydelsesdata og konfiguration", installationManual: "Installationsvejledning", installationManualDescription: "Montage, tilslutninger og vedligeholdelse", euDeclaration: "EU-erklæring", euDeclarationDescription: "Overensstemmelse og gældende standarder", dimensionalDrawing: "Måltegning", dimensionalDrawingDescription: "Ydre mål og konfigurationsmål", localPackageNotice: "Tilgængelighed og generering styres af den lokale produktionsproces ud fra model, konfiguration og sprog.", offlineAvailable: "Tilgængelig offline", openDocument: "Åbn dokument",
    readyForReport: "Valget er klar til teknisk rapport.", reference: "Reference", requestedPressure: "Krævet tryk", options: "Valgmuligheder", notSelectedFeminine: "Ikke valgt", notSelectedMasculine: "Ikke valgt", technicalReport: "Teknisk rapport", reportDescription: "Dokumentet bruger det aktuelle projekts sprog og reference.",
    completeSave: "Fuldfør lagringen i den åbne produktionsgrænseflade.", draftSavedAt: "Kladde gemt kl.", completeReport: "Fuldfør rapporten i den åbne produktionsgrænseflade.", reportReady: "Rapport klar",
  },
  de: {
    notifications: "Benachrichtigungszentrum", components: "UI-Komponenten", currentSelection: "Aktuelle Auswahl", saveDraft: "Entwurf speichern", noUnit: "Kein Gerät", supply: "Zuluft", extract: "Abluft", pressure: "Druck", layout: "Layout", efficiency: "Wirkungsgrad", margin: "Reserve", power: "Leistung",
    projectData: "Projektdaten", projectName: "Projektname", customerReference: "Kundenreferenz", freeReference: "Freie Referenz", documentLanguage: "Dokumentsprache", projectStatus: "Projektstatus", technicalReference: "Technische Referenz", registered: "Registriert", revision: "Revision", current: "Aktuell", lastSaved: "Zuletzt gespeichert", today: "Heute", local: "Lokal",
    dutyPoint: "Betriebspunkt", supplyAirflow: "Zuluftvolumenstrom", extractAirflow: "Abluftvolumenstrom", staticPressure: "Statischer Druck", results: "Suchergebnisse", compatibleUnits: "kompatible Geräte", orderedByFit: "SFP ↑", technicalFit: "Erforderliche Regelung", calculateAfterSelection: "Nach der Auswahl", recommended: "Empfohlen", selected: "Ausgewählt", catalogue: "Katalog", maximumAirflow: "Maximaler Volumenstrom", availablePressure: "Verfügbarer Druck", nominalEfficiency: "Nennwirkungsgrad", soundPower: "Schallleistung", currentUnit: "Aktuelles Gerät", chooseUnit: "Gerät wählen",
    airflowConfiguration: "Luftstromkonfiguration", defaultHint: "Empfohlener Standard für die gewählte Installation.", orientation: "Luftstromausrichtung", lowerAccess: "Zugang unten", accessPanel: "Zugangspaneel",
    enableCoil: "Register aktivieren", calculationMode: "Berechnungsart", coil: "Register", fluid: "Medium", glycol: "Glykol", coolingIn: "Kühlung ein", coolingOut: "Kühlung aus", heatingIn: "Heizung ein", heatingOut: "Heizung aus", rows: "Reihen", circuits: "Kreise", finSpacing: "Lamellenabstand", performance: "Berechnete Leistung", enableToCalculate: "Register aktivieren, um die Leistung zu berechnen.", noneAssociated: "In der lokalen Datenbank ist diesem Gerät kein Register zugeordnet.", additionalPressureDrop: "Zusätzlicher Druckverlust auf der Kennlinie", capacity: "Leistung", airOut: "Luft aus", airPressureDrop: "Luft-DP",
    preheating: "Elektrische Vorheizung", postHeating: "Elektrische Nachheizung", heater: "Elektroheizregister", voltagePhases: "Spannung / Phasen", currentValue: "Strom", airIn: "Luft ein", maximumAirOut: "Max. Lufttemperatur aus", relativeHumidityOut: "r.F. aus", searchPlaceholder: "Code oder Beschreibung suchen", allCategories: "Alle Kategorien", selectedCount: "ausgewählt", code: "Code", description: "Beschreibung", category: "Kategorie",
    technicalSheet: "Technisches Datenblatt", technicalSheetDescription: "Leistungsdaten und Konfiguration", installationManual: "Installationsanleitung", installationManualDescription: "Montage, Anschlüsse und Wartung", euDeclaration: "EU-Erklärung", euDeclarationDescription: "Konformität und geltende Normen", dimensionalDrawing: "Maßzeichnung", dimensionalDrawingDescription: "Außenmaße und Konfigurationsabmessungen", localPackageNotice: "Verfügbarkeit und Erstellung werden vom lokalen Produktionsworkflow abhängig von Modell, Konfiguration und Sprache gesteuert.", offlineAvailable: "Offline verfügbar", openDocument: "Dokument öffnen",
    readyForReport: "Die Auswahl ist bereit für den technischen Bericht.", reference: "Referenz", requestedPressure: "Erforderlicher Druck", options: "Optionen", notSelectedFeminine: "Nicht ausgewählt", notSelectedMasculine: "Nicht ausgewählt", technicalReport: "Technischer Bericht", reportDescription: "Das Dokument verwendet Sprache und Referenz des aktuellen Projekts.",
    completeSave: "Speichern in der geöffneten Produktionsoberfläche abschließen.", draftSavedAt: "Entwurf gespeichert um", completeReport: "Berichterstellung in der geöffneten Produktionsoberfläche abschließen.", reportReady: "Bericht bereit",
  },
  fr: {
    notifications: "Centre de notifications", components: "Composants UI", currentSelection: "Sélection actuelle", saveDraft: "Enregistrer le brouillon", noUnit: "Aucune unité", supply: "Soufflage", extract: "Reprise", pressure: "Pression", layout: "Implantation", efficiency: "Rendement", margin: "Marge", power: "Puissance",
    projectData: "Données du projet", projectName: "Nom du projet", customerReference: "Référence client", freeReference: "Référence libre", documentLanguage: "Langue des documents", projectStatus: "État du projet", technicalReference: "Référence technique", registered: "Enregistrée", revision: "Révision", current: "Actuelle", lastSaved: "Dernier enregistrement", today: "Aujourd'hui", local: "Local",
    dutyPoint: "Point de fonctionnement", supplyAirflow: "Débit de soufflage", extractAirflow: "Débit de reprise", staticPressure: "Pression statique", results: "Résultats de recherche", compatibleUnits: "unités compatibles", orderedByFit: "SFP ↑", technicalFit: "Régulation requise", calculateAfterSelection: "Après sélection", recommended: "Recommandée", selected: "Sélectionnée", catalogue: "Catalogue", maximumAirflow: "Débit maximal", availablePressure: "Pression disponible", nominalEfficiency: "Rendement nominal", soundPower: "Puissance acoustique", currentUnit: "Unité actuelle", chooseUnit: "Choisir l'unité",
    airflowConfiguration: "Configuration des flux", defaultHint: "Valeur par défaut recommandée pour l'installation.", orientation: "Orientation des flux", lowerAccess: "Accès inférieur", accessPanel: "Panneau d'accès",
    enableCoil: "Activer la batterie", calculationMode: "Mode de calcul", coil: "Batterie", fluid: "Fluide", glycol: "Glycol", coolingIn: "Froid entrée", coolingOut: "Froid sortie", heatingIn: "Chaud entrée", heatingOut: "Chaud sortie", rows: "rangs", circuits: "circuits", finSpacing: "pas d'ailettes", performance: "Performances calculées", enableToCalculate: "Activez la batterie pour calculer ses performances.", noneAssociated: "Aucune batterie n'est associée à cette unité dans la base locale.", additionalPressureDrop: "Perte de charge supplémentaire appliquée à la courbe", capacity: "Puissance", airOut: "Air sortie", airPressureDrop: "DP air",
    preheating: "Préchauffage électrique", postHeating: "Post-chauffage électrique", heater: "Batterie électrique", voltagePhases: "Tension / phases", currentValue: "Courant", airIn: "Air entrée", maximumAirOut: "Temp. max. air sortie", relativeHumidityOut: "H.R. sortie", searchPlaceholder: "Rechercher code ou description", allCategories: "Toutes les catégories", selectedCount: "sélectionnés", code: "Code", description: "Description", category: "Catégorie",
    technicalSheet: "Fiche technique", technicalSheetDescription: "Performances et configuration", installationManual: "Manuel d'installation", installationManualDescription: "Montage, raccordements et maintenance", euDeclaration: "Déclaration UE", euDeclarationDescription: "Conformité et normes applicables", dimensionalDrawing: "Plan dimensionnel", dimensionalDrawingDescription: "Encombrements et cotes de la configuration", localPackageNotice: "La disponibilité et la génération sont gérées par le workflow de production local selon le modèle, la configuration et la langue.", offlineAvailable: "Disponible hors ligne", openDocument: "Ouvrir le document",
    readyForReport: "La sélection est prête pour le rapport technique.", reference: "Référence", requestedPressure: "Pression requise", options: "Options", notSelectedFeminine: "Non sélectionnée", notSelectedMasculine: "Non sélectionné", technicalReport: "Rapport technique", reportDescription: "Le document utilise la langue et la référence du projet actuel.",
    completeSave: "Terminez l'enregistrement dans l'interface de production ouverte.", draftSavedAt: "Brouillon enregistré à", completeReport: "Terminez le rapport dans l'interface de production ouverte.", reportReady: "Rapport prêt",
  },
  hu: {
    notifications: "Értesítési központ", components: "UI-elemek", currentSelection: "Aktuális kiválasztás", saveDraft: "Piszkozat mentése", noUnit: "Nincs egység", supply: "Befúvás", extract: "Elszívás", pressure: "Nyomás", layout: "Elrendezés", efficiency: "Hatásfok", margin: "Tartalék", power: "Teljesítmény",
    projectData: "Projektadatok", projectName: "Projekt neve", customerReference: "Ügyfélhivatkozás", freeReference: "Szabad hivatkozás", documentLanguage: "Dokumentum nyelve", projectStatus: "Projekt állapota", technicalReference: "Műszaki hivatkozás", registered: "Regisztrálva", revision: "Revízió", current: "Aktuális", lastSaved: "Utolsó mentés", today: "Ma", local: "Helyi",
    dutyPoint: "Munkapont", supplyAirflow: "Befúvó légszállítás", extractAirflow: "Elszívó légszállítás", staticPressure: "Statikus nyomás", results: "Keresési eredmények", compatibleUnits: "kompatibilis egység", orderedByFit: "SFP ↑", technicalFit: "Szükséges szabályozás", calculateAfterSelection: "Kiválasztás után", recommended: "Ajánlott", selected: "Kiválasztva", catalogue: "Katalógus", maximumAirflow: "Maximális légszállítás", availablePressure: "Elérhető nyomás", nominalEfficiency: "Névleges hatásfok", soundPower: "Hangteljesítmény", currentUnit: "Aktuális egység", chooseUnit: "Egység választása",
    airflowConfiguration: "Légáram-konfiguráció", defaultHint: "A kiválasztott telepítés ajánlott alapértéke.", orientation: "Légáram iránya", lowerAccess: "Alsó hozzáférés", accessPanel: "Hozzáférési panel",
    enableCoil: "Hőcserélő engedélyezése", calculationMode: "Számítási mód", coil: "Hőcserélő", fluid: "Közeg", glycol: "Glikol", coolingIn: "Hűtés be", coolingOut: "Hűtés ki", heatingIn: "Fűtés be", heatingOut: "Fűtés ki", rows: "sor", circuits: "kör", finSpacing: "lamellaosztás", performance: "Számított teljesítmény", enableToCalculate: "Engedélyezze a hőcserélőt a számításhoz.", noneAssociated: "A helyi adatbázisban nincs hőcserélő ehhez az egységhez.", additionalPressureDrop: "A görbére alkalmazott további nyomásesés", capacity: "Teljesítmény", airOut: "Kilépő levegő", airPressureDrop: "Levegő DP",
    preheating: "Elektromos előfűtés", postHeating: "Elektromos utófűtés", heater: "Elektromos fűtő", voltagePhases: "Feszültség / fázisok", currentValue: "Áram", airIn: "Belépő levegő", maximumAirOut: "Max. kilépő levegőhőm.", relativeHumidityOut: "Kilépő relatív párat.", searchPlaceholder: "Kód vagy leírás keresése", allCategories: "Minden kategória", selectedCount: "kiválasztva", code: "Kód", description: "Leírás", category: "Kategória",
    technicalSheet: "Műszaki adatlap", technicalSheetDescription: "Teljesítményadatok és konfiguráció", installationManual: "Telepítési útmutató", installationManualDescription: "Szerelés, csatlakozások és karbantartás", euDeclaration: "EU-nyilatkozat", euDeclarationDescription: "Megfelelőség és alkalmazandó szabványok", dimensionalDrawing: "Méretrajz", dimensionalDrawingDescription: "Befoglaló és konfigurációs méretek", localPackageNotice: "Az elérhetőséget és a létrehozást a helyi gyártási munkafolyamat kezeli a modell, konfiguráció és nyelv alapján.", offlineAvailable: "Offline elérhető", openDocument: "Dokumentum megnyitása",
    readyForReport: "A kiválasztás készen áll a műszaki jelentésre.", reference: "Hivatkozás", requestedPressure: "Kért nyomás", options: "Opciók", notSelectedFeminine: "Nincs kiválasztva", notSelectedMasculine: "Nincs kiválasztva", technicalReport: "Műszaki jelentés", reportDescription: "A dokumentum az aktuális projekt nyelvét és hivatkozását használja.",
    completeSave: "Fejezze be a mentést a megnyitott gyártási felületen.", draftSavedAt: "Piszkozat mentve:", completeReport: "Fejezze be a jelentést a megnyitott gyártási felületen.", reportReady: "Jelentés kész",
  },
  is: {
    notifications: "Tilkynningamiðstöð", components: "Viðmótshlutar", currentSelection: "Núverandi val", saveDraft: "Vista drög", noUnit: "Engin eining", supply: "Innblástur", extract: "Frásog", pressure: "Þrýstingur", layout: "Skipulag", efficiency: "Nýtni", margin: "Svigrúm", power: "Afl",
    projectData: "Verkefnisgögn", projectName: "Heiti verkefnis", customerReference: "Tilvísun viðskiptavinar", freeReference: "Frjáls tilvísun", documentLanguage: "Tungumál skjala", projectStatus: "Staða verkefnis", technicalReference: "Tæknileg tilvísun", registered: "Skráð", revision: "Útgáfa", current: "Núverandi", lastSaved: "Síðast vistað", today: "Í dag", local: "Staðbundið",
    dutyPoint: "Vinnupunktur", supplyAirflow: "Innblástursflæði", extractAirflow: "Frásogsflæði", staticPressure: "Stöðuþrýstingur", results: "Leitarniðurstöður", compatibleUnits: "samhæfar einingar", orderedByFit: "SFP ↑", technicalFit: "Nauðsynleg stýring", calculateAfterSelection: "Eftir val", recommended: "Mælt með", selected: "Valin", catalogue: "Vörulisti", maximumAirflow: "Hámarksloftflæði", availablePressure: "Tiltækur þrýstingur", nominalEfficiency: "Nafnnýtni", soundPower: "Hljóðafl", currentUnit: "Núverandi eining", chooseUnit: "Velja einingu",
    airflowConfiguration: "Loftflæðisstilling", defaultHint: "Ráðlagt sjálfgefið gildi fyrir uppsetninguna.", orientation: "Stefna loftflæðis", lowerAccess: "Aðgangur að neðan", accessPanel: "Þjónustulúga",
    enableCoil: "Virkja rafhlöðu", calculationMode: "Reikniaðferð", coil: "Rafhlaða", fluid: "Vökvi", glycol: "Glýkól", coolingIn: "Kæling inn", coolingOut: "Kæling út", heatingIn: "Hitun inn", heatingOut: "Hitun út", rows: "raðir", circuits: "rásir", finSpacing: "lamellubil", performance: "Reiknuð afköst", enableToCalculate: "Virkjaðu rafhlöðuna til að reikna afköst.", noneAssociated: "Engin rafhlaða er tengd þessari einingu í staðbundna gagnagrunninum.", additionalPressureDrop: "Viðbótarþrýstifall á afkastakúrfu", capacity: "Afl", airOut: "Loft út", airPressureDrop: "Loftþrýstifall",
    preheating: "Rafmagnsforhitun", postHeating: "Rafmagnseftirhitun", heater: "Rafmagnshitari", voltagePhases: "Spenna / fasar", currentValue: "Straumur", airIn: "Loft inn", maximumAirOut: "Hámarkshiti lofts út", relativeHumidityOut: "Rakastig út", searchPlaceholder: "Leita að kóða eða lýsingu", allCategories: "Allir flokkar", selectedCount: "valin", code: "Kóði", description: "Lýsing", category: "Flokkur",
    technicalSheet: "Tækniblað", technicalSheetDescription: "Afkastagögn og stilling", installationManual: "Uppsetningarhandbók", installationManualDescription: "Uppsetning, tengingar og viðhald", euDeclaration: "ESB-yfirlýsing", euDeclarationDescription: "Samræmi og viðeigandi staðlar", dimensionalDrawing: "Málteikning", dimensionalDrawingDescription: "Heildarmál og mál stillingar", localPackageNotice: "Framboð og gerð skjala er stjórnað af staðbundnu framleiðsluferli eftir gerð, stillingu og tungumáli.", offlineAvailable: "Tiltækt án nettengingar", openDocument: "Opna skjal",
    readyForReport: "Valið er tilbúið fyrir tækniskýrslu.", reference: "Tilvísun", requestedPressure: "Nauðsynlegur þrýstingur", options: "Valkostir", notSelectedFeminine: "Ekki valin", notSelectedMasculine: "Ekki valinn", technicalReport: "Tækniskýrsla", reportDescription: "Skjalið notar tungumál og tilvísun núverandi verkefnis.",
    completeSave: "Ljúktu vistun í opna framleiðsluviðmótinu.", draftSavedAt: "Drög vistuð kl.", completeReport: "Ljúktu skýrslugerð í opna framleiðsluviðmótinu.", reportReady: "Skýrsla tilbúin",
  },
  nl: {
    notifications: "Meldingencentrum", components: "UI-componenten", currentSelection: "Huidige selectie", saveDraft: "Concept opslaan", noUnit: "Geen unit", supply: "Toevoer", extract: "Retour", pressure: "Druk", layout: "Layout", efficiency: "Rendement", margin: "Marge", power: "Vermogen",
    projectData: "Projectgegevens", projectName: "Projectnaam", customerReference: "Klantreferentie", freeReference: "Vrije referentie", documentLanguage: "Documenttaal", projectStatus: "Projectstatus", technicalReference: "Technische referentie", registered: "Geregistreerd", revision: "Revisie", current: "Actueel", lastSaved: "Laatst opgeslagen", today: "Vandaag", local: "Lokaal",
    dutyPoint: "Werkpunt", supplyAirflow: "Toevoerdebiet", extractAirflow: "Retourdebiet", staticPressure: "Statische druk", results: "Zoekresultaten", compatibleUnits: "compatibele units", orderedByFit: "SFP ↑", technicalFit: "Vereiste regeling", calculateAfterSelection: "Na selectie", recommended: "Aanbevolen", selected: "Geselecteerd", catalogue: "Catalogus", maximumAirflow: "Maximaal debiet", availablePressure: "Beschikbare druk", nominalEfficiency: "Nominaal rendement", soundPower: "Geluidsvermogen", currentUnit: "Huidige unit", chooseUnit: "Kies unit",
    airflowConfiguration: "Luchtstroomconfiguratie", defaultHint: "Aanbevolen standaard voor de gekozen installatie.", orientation: "Luchtstroomrichting", lowerAccess: "Toegang onderzijde", accessPanel: "Toegangspaneel",
    enableCoil: "Batterij inschakelen", calculationMode: "Berekeningsmodus", coil: "Batterij", fluid: "Medium", glycol: "Glycol", coolingIn: "Koeling in", coolingOut: "Koeling uit", heatingIn: "Verwarming in", heatingOut: "Verwarming uit", rows: "rijen", circuits: "circuits", finSpacing: "lamelafstand", performance: "Berekende prestaties", enableToCalculate: "Schakel de batterij in om de prestaties te berekenen.", noneAssociated: "In de lokale database is geen batterij aan deze unit gekoppeld.", additionalPressureDrop: "Extra drukverlies op de curve", capacity: "Vermogen", airOut: "Lucht uit", airPressureDrop: "Lucht-DP",
    preheating: "Elektrische voorverwarming", postHeating: "Elektrische naverwarming", heater: "Elektrische verwarmer", voltagePhases: "Spanning / fasen", currentValue: "Stroom", airIn: "Lucht in", maximumAirOut: "Max. luchttemperatuur uit", relativeHumidityOut: "RV uit", searchPlaceholder: "Zoek code of beschrijving", allCategories: "Alle categorieën", selectedCount: "geselecteerd", code: "Code", description: "Beschrijving", category: "Categorie",
    technicalSheet: "Technisch blad", technicalSheetDescription: "Prestatiegegevens en configuratie", installationManual: "Installatiehandleiding", installationManualDescription: "Montage, aansluitingen en onderhoud", euDeclaration: "EU-verklaring", euDeclarationDescription: "Conformiteit en toepasselijke normen", dimensionalDrawing: "Maattekening", dimensionalDrawingDescription: "Buitenmaten en configuratieafmetingen", localPackageNotice: "Beschikbaarheid en generatie worden door de lokale productieworkflow beheerd op basis van model, configuratie en taal.", offlineAvailable: "Offline beschikbaar", openDocument: "Document openen",
    readyForReport: "De selectie is klaar voor het technische rapport.", reference: "Referentie", requestedPressure: "Vereiste druk", options: "Opties", notSelectedFeminine: "Niet geselecteerd", notSelectedMasculine: "Niet geselecteerd", technicalReport: "Technisch rapport", reportDescription: "Het document gebruikt de taal en referentie van het huidige project.",
    completeSave: "Voltooi het opslaan in de geopende productie-interface.", draftSavedAt: "Concept opgeslagen om", completeReport: "Voltooi het rapport in de geopende productie-interface.", reportReady: "Rapport gereed",
  },
  no: {
    notifications: "Varslingssenter", components: "UI-komponenter", currentSelection: "Gjeldende utvalg", saveDraft: "Lagre utkast", noUnit: "Ingen aggregat", supply: "Tilluft", extract: "Avtrekk", pressure: "Trykk", layout: "Layout", efficiency: "Virkningsgrad", margin: "Margin", power: "Effekt",
    projectData: "Prosjektdata", projectName: "Prosjektnavn", customerReference: "Kundereferanse", freeReference: "Fri referanse", documentLanguage: "Dokumentspråk", projectStatus: "Prosjektstatus", technicalReference: "Teknisk referanse", registered: "Registrert", revision: "Revisjon", current: "Gjeldende", lastSaved: "Sist lagret", today: "I dag", local: "Lokal",
    dutyPoint: "Driftspunkt", supplyAirflow: "Tilluftsmengde", extractAirflow: "Avtrekksmengde", staticPressure: "Statisk trykk", results: "Søkeresultater", compatibleUnits: "kompatible aggregater", orderedByFit: "SFP ↑", technicalFit: "Nødvendig regulering", calculateAfterSelection: "Etter valg", recommended: "Anbefalt", selected: "Valgt", catalogue: "Katalog", maximumAirflow: "Maksimal luftmengde", availablePressure: "Tilgjengelig trykk", nominalEfficiency: "Nominell virkningsgrad", soundPower: "Lydeffekt", currentUnit: "Gjeldende aggregat", chooseUnit: "Velg aggregat",
    airflowConfiguration: "Luftstrømskonfigurasjon", defaultHint: "Anbefalt standard for valgt installasjon.", orientation: "Luftstrømretning", lowerAccess: "Tilgang nedenfra", accessPanel: "Tilgangspanel",
    enableCoil: "Aktiver batteri", calculationMode: "Beregningsmodus", coil: "Batteri", fluid: "Medium", glycol: "Glykol", coolingIn: "Kjøling inn", coolingOut: "Kjøling ut", heatingIn: "Varme inn", heatingOut: "Varme ut", rows: "rader", circuits: "kretser", finSpacing: "lamellavstand", performance: "Beregnede ytelser", enableToCalculate: "Aktiver batteriet for å beregne ytelsen.", noneAssociated: "Ingen batteri er knyttet til dette aggregatet i lokal database.", additionalPressureDrop: "Ekstra trykkfall på kurven", capacity: "Effekt", airOut: "Luft ut", airPressureDrop: "Luft-DP",
    preheating: "Elektrisk forvarme", postHeating: "Elektrisk ettervarme", heater: "Elektrisk varmebatteri", voltagePhases: "Spenning / faser", currentValue: "Strøm", airIn: "Luft inn", maximumAirOut: "Maks. lufttemperatur ut", relativeHumidityOut: "RF ut", searchPlaceholder: "Søk kode eller beskrivelse", allCategories: "Alle kategorier", selectedCount: "valgt", code: "Kode", description: "Beskrivelse", category: "Kategori",
    technicalSheet: "Teknisk datablad", technicalSheetDescription: "Ytelsesdata og konfigurasjon", installationManual: "Installasjonshåndbok", installationManualDescription: "Montering, tilkoblinger og vedlikehold", euDeclaration: "EU-erklæring", euDeclarationDescription: "Samsvar og gjeldende standarder", dimensionalDrawing: "Måltegning", dimensionalDrawingDescription: "Utvendige mål og konfigurasjonsmål", localPackageNotice: "Tilgjengelighet og generering styres av den lokale produksjonsflyten ut fra modell, konfigurasjon og språk.", offlineAvailable: "Tilgjengelig frakoblet", openDocument: "Åpne dokument",
    readyForReport: "Utvalget er klart for teknisk rapport.", reference: "Referanse", requestedPressure: "Krevd trykk", options: "Alternativer", notSelectedFeminine: "Ikke valgt", notSelectedMasculine: "Ikke valgt", technicalReport: "Teknisk rapport", reportDescription: "Dokumentet bruker språket og referansen til gjeldende prosjekt.",
    completeSave: "Fullfør lagringen i det åpne produksjonsgrensesnittet.", draftSavedAt: "Utkast lagret kl.", completeReport: "Fullfør rapporten i det åpne produksjonsgrensesnittet.", reportReady: "Rapport klar",
  },
  pl: {
    notifications: "Centrum powiadomień", components: "Komponenty UI", currentSelection: "Bieżący dobór", saveDraft: "Zapisz wersję roboczą", noUnit: "Brak urządzenia", supply: "Nawiew", extract: "Wywiew", pressure: "Ciśnienie", layout: "Układ", efficiency: "Sprawność", margin: "Zapas", power: "Moc",
    projectData: "Dane projektu", projectName: "Nazwa projektu", customerReference: "Odniesienie klienta", freeReference: "Dowolne odniesienie", documentLanguage: "Język dokumentów", projectStatus: "Stan projektu", technicalReference: "Odniesienie techniczne", registered: "Zarejestrowano", revision: "Rewizja", current: "Bieżąca", lastSaved: "Ostatni zapis", today: "Dzisiaj", local: "Lokalnie",
    dutyPoint: "Punkt pracy", supplyAirflow: "Przepływ nawiewu", extractAirflow: "Przepływ wywiewu", staticPressure: "Ciśnienie statyczne", results: "Wyniki wyszukiwania", compatibleUnits: "kompatybilne urządzenia", orderedByFit: "SFP ↑", technicalFit: "Wymagana regulacja", calculateAfterSelection: "Po wyborze", recommended: "Zalecane", selected: "Wybrano", catalogue: "Katalog", maximumAirflow: "Maksymalny przepływ", availablePressure: "Dostępne ciśnienie", nominalEfficiency: "Sprawność nominalna", soundPower: "Moc akustyczna", currentUnit: "Bieżące urządzenie", chooseUnit: "Wybierz urządzenie",
    airflowConfiguration: "Konfiguracja przepływów", defaultHint: "Zalecane ustawienie dla wybranego montażu.", orientation: "Orientacja przepływów", lowerAccess: "Dostęp od dołu", accessPanel: "Panel dostępu",
    enableCoil: "Włącz baterię", calculationMode: "Tryb obliczeń", coil: "Bateria", fluid: "Czynnik", glycol: "Glikol", coolingIn: "Chłodzenie wejście", coolingOut: "Chłodzenie wyjście", heatingIn: "Grzanie wejście", heatingOut: "Grzanie wyjście", rows: "rzędy", circuits: "obwody", finSpacing: "rozstaw lamel", performance: "Obliczone parametry", enableToCalculate: "Włącz baterię, aby obliczyć jej parametry.", noneAssociated: "Brak baterii przypisanej do urządzenia w lokalnej bazie.", additionalPressureDrop: "Dodatkowy spadek ciśnienia na krzywej", capacity: "Moc", airOut: "Powietrze wyjście", airPressureDrop: "DP powietrza",
    preheating: "Elektryczne ogrzewanie wstępne", postHeating: "Elektryczne ogrzewanie wtórne", heater: "Nagrzewnica elektryczna", voltagePhases: "Napięcie / fazy", currentValue: "Prąd", airIn: "Powietrze wejście", maximumAirOut: "Maks. temp. powietrza wyj.", relativeHumidityOut: "Wilg. wzgl. wyj.", searchPlaceholder: "Szukaj kodu lub opisu", allCategories: "Wszystkie kategorie", selectedCount: "wybrano", code: "Kod", description: "Opis", category: "Kategoria",
    technicalSheet: "Karta techniczna", technicalSheetDescription: "Parametry i konfiguracja", installationManual: "Instrukcja instalacji", installationManualDescription: "Montaż, połączenia i konserwacja", euDeclaration: "Deklaracja UE", euDeclarationDescription: "Zgodność i obowiązujące normy", dimensionalDrawing: "Rysunek wymiarowy", dimensionalDrawingDescription: "Gabaryty i wymiary konfiguracji", localPackageNotice: "Dostępnością i generowaniem zarządza lokalny proces produkcyjny zależnie od modelu, konfiguracji i języka.", offlineAvailable: "Dostępne offline", openDocument: "Otwórz dokument",
    readyForReport: "Dobór jest gotowy do raportu technicznego.", reference: "Odniesienie", requestedPressure: "Wymagane ciśnienie", options: "Opcje", notSelectedFeminine: "Nie wybrano", notSelectedMasculine: "Nie wybrano", technicalReport: "Raport techniczny", reportDescription: "Dokument używa języka i odniesienia bieżącego projektu.",
    completeSave: "Dokończ zapis w otwartym interfejsie produkcyjnym.", draftSavedAt: "Wersja robocza zapisana o", completeReport: "Dokończ raport w otwartym interfejsie produkcyjnym.", reportReady: "Raport gotowy",
  },
  ro: {
    notifications: "Centru de notificări", components: "Componente UI", currentSelection: "Selecția curentă", saveDraft: "Salvează ciorna", noUnit: "Nicio unitate", supply: "Introducere", extract: "Extragere", pressure: "Presiune", layout: "Configurație", efficiency: "Randament", margin: "Rezervă", power: "Putere",
    projectData: "Datele proiectului", projectName: "Numele proiectului", customerReference: "Referință client", freeReference: "Referință liberă", documentLanguage: "Limba documentelor", projectStatus: "Starea proiectului", technicalReference: "Referință tehnică", registered: "Înregistrat", revision: "Revizie", current: "Curentă", lastSaved: "Ultima salvare", today: "Astăzi", local: "Local",
    dutyPoint: "Punct de lucru", supplyAirflow: "Debit introducere", extractAirflow: "Debit extragere", staticPressure: "Presiune statică", results: "Rezultatele căutării", compatibleUnits: "unități compatibile", orderedByFit: "SFP ↑", technicalFit: "Reglaj necesar", calculateAfterSelection: "După selecție", recommended: "Recomandată", selected: "Selectată", catalogue: "Catalog", maximumAirflow: "Debit maxim", availablePressure: "Presiune disponibilă", nominalEfficiency: "Randament nominal", soundPower: "Putere acustică", currentUnit: "Unitatea curentă", chooseUnit: "Alege unitatea",
    airflowConfiguration: "Configurația fluxurilor", defaultHint: "Valoare implicită recomandată pentru instalarea aleasă.", orientation: "Orientarea fluxurilor", lowerAccess: "Acces inferior", accessPanel: "Panou de acces",
    enableCoil: "Activează bateria", calculationMode: "Mod de calcul", coil: "Baterie", fluid: "Fluid", glycol: "Glicol", coolingIn: "Rece intrare", coolingOut: "Rece ieșire", heatingIn: "Cald intrare", heatingOut: "Cald ieșire", rows: "rânduri", circuits: "circuite", finSpacing: "pas lamele", performance: "Performanțe calculate", enableToCalculate: "Activați bateria pentru a-i calcula performanța.", noneAssociated: "Nicio baterie nu este asociată unității în baza locală.", additionalPressureDrop: "Pierdere de presiune suplimentară pe curbă", capacity: "Putere", airOut: "Aer ieșire", airPressureDrop: "DP aer",
    preheating: "Preîncălzire electrică", postHeating: "Postîncălzire electrică", heater: "Baterie electrică", voltagePhases: "Tensiune / faze", currentValue: "Curent", airIn: "Aer intrare", maximumAirOut: "Temp. max. aer ieșire", relativeHumidityOut: "U.R. ieșire", searchPlaceholder: "Caută cod sau descriere", allCategories: "Toate categoriile", selectedCount: "selectate", code: "Cod", description: "Descriere", category: "Categorie",
    technicalSheet: "Fișă tehnică", technicalSheetDescription: "Performanțe și configurație", installationManual: "Manual de instalare", installationManualDescription: "Montaj, conexiuni și întreținere", euDeclaration: "Declarație UE", euDeclarationDescription: "Conformitate și standarde aplicabile", dimensionalDrawing: "Desen dimensional", dimensionalDrawingDescription: "Gabarit și cote de configurație", localPackageNotice: "Disponibilitatea și generarea sunt gestionate de fluxul local de producție în funcție de model, configurație și limbă.", offlineAvailable: "Disponibil offline", openDocument: "Deschide documentul",
    readyForReport: "Selecția este gata pentru raportul tehnic.", reference: "Referință", requestedPressure: "Presiune necesară", options: "Opțiuni", notSelectedFeminine: "Neselectată", notSelectedMasculine: "Neselectat", technicalReport: "Raport tehnic", reportDescription: "Documentul folosește limba și referința proiectului curent.",
    completeSave: "Finalizați salvarea în interfața de producție deschisă.", draftSavedAt: "Ciornă salvată la", completeReport: "Finalizați raportul în interfața de producție deschisă.", reportReady: "Raport gata",
  },
  sl: {
    notifications: "Središče obvestil", components: "Komponente UI", currentSelection: "Trenutni izbor", saveDraft: "Shrani osnutek", noUnit: "Ni enote", supply: "Dovod", extract: "Odvod", pressure: "Tlak", layout: "Postavitev", efficiency: "Izkoristek", margin: "Rezerva", power: "Moč",
    projectData: "Podatki projekta", projectName: "Ime projekta", customerReference: "Sklic naročnika", freeReference: "Prosti sklic", documentLanguage: "Jezik dokumentov", projectStatus: "Stanje projekta", technicalReference: "Tehnični sklic", registered: "Registrirano", revision: "Revizija", current: "Trenutna", lastSaved: "Nazadnje shranjeno", today: "Danes", local: "Lokalno",
    dutyPoint: "Delovna točka", supplyAirflow: "Dovodni pretok", extractAirflow: "Odvodni pretok", staticPressure: "Statični tlak", results: "Rezultati iskanja", compatibleUnits: "združljive enote", orderedByFit: "SFP ↑", technicalFit: "Zahtevana regulacija", calculateAfterSelection: "Po izbiri", recommended: "Priporočeno", selected: "Izbrano", catalogue: "Katalog", maximumAirflow: "Največji pretok", availablePressure: "Razpoložljivi tlak", nominalEfficiency: "Nazivni izkoristek", soundPower: "Zvočna moč", currentUnit: "Trenutna enota", chooseUnit: "Izberi enoto",
    airflowConfiguration: "Konfiguracija pretokov", defaultHint: "Priporočena privzeta vrednost za izbrano namestitev.", orientation: "Usmeritev pretokov", lowerAccess: "Dostop spodaj", accessPanel: "Dostopni panel",
    enableCoil: "Omogoči izmenjevalnik", calculationMode: "Način izračuna", coil: "Izmenjevalnik", fluid: "Medij", glycol: "Glikol", coolingIn: "Hlajenje vstop", coolingOut: "Hlajenje izstop", heatingIn: "Gretje vstop", heatingOut: "Gretje izstop", rows: "vrste", circuits: "krogi", finSpacing: "razmak lamel", performance: "Izračunane zmogljivosti", enableToCalculate: "Omogočite izmenjevalnik za izračun zmogljivosti.", noneAssociated: "V lokalni bazi tej enoti ni dodeljen izmenjevalnik.", additionalPressureDrop: "Dodatni tlačni padec na krivulji", capacity: "Moč", airOut: "Zrak izstop", airPressureDrop: "DP zraka",
    preheating: "Električno predgrevanje", postHeating: "Električno dogrevanje", heater: "Električni grelnik", voltagePhases: "Napetost / faze", currentValue: "Tok", airIn: "Zrak vstop", maximumAirOut: "Najv. temp. zraka izstop", relativeHumidityOut: "Rel. vlaga izstop", searchPlaceholder: "Išči kodo ali opis", allCategories: "Vse kategorije", selectedCount: "izbrano", code: "Koda", description: "Opis", category: "Kategorija",
    technicalSheet: "Tehnični list", technicalSheetDescription: "Zmogljivosti in konfiguracija", installationManual: "Navodila za namestitev", installationManualDescription: "Montaža, priključki in vzdrževanje", euDeclaration: "Izjava EU", euDeclarationDescription: "Skladnost in veljavni standardi", dimensionalDrawing: "Dimenzijska risba", dimensionalDrawingDescription: "Gabariti in mere konfiguracije", localPackageNotice: "Razpoložljivost in izdelavo upravlja lokalni proizvodni potek glede na model, konfiguracijo in jezik.", offlineAvailable: "Na voljo brez povezave", openDocument: "Odpri dokument",
    readyForReport: "Izbor je pripravljen za tehnično poročilo.", reference: "Sklic", requestedPressure: "Zahtevani tlak", options: "Možnosti", notSelectedFeminine: "Ni izbrana", notSelectedMasculine: "Ni izbran", technicalReport: "Tehnično poročilo", reportDescription: "Dokument uporablja jezik in sklic trenutnega projekta.",
    completeSave: "Dokončajte shranjevanje v odprtem proizvodnem vmesniku.", draftSavedAt: "Osnutek shranjen ob", completeReport: "Dokončajte poročilo v odprtem proizvodnem vmesniku.", reportReady: "Poročilo pripravljeno",
  },
  sv: {
    notifications: "Notifieringscenter", components: "UI-komponenter", currentSelection: "Aktuellt val", saveDraft: "Spara utkast", noUnit: "Inget aggregat", supply: "Tilluft", extract: "Frånluft", pressure: "Tryck", layout: "Layout", efficiency: "Verkningsgrad", margin: "Marginal", power: "Effekt",
    projectData: "Projektdata", projectName: "Projektnamn", customerReference: "Kundreferens", freeReference: "Fri referens", documentLanguage: "Dokumentspråk", projectStatus: "Projektstatus", technicalReference: "Teknisk referens", registered: "Registrerad", revision: "Revision", current: "Aktuell", lastSaved: "Senast sparad", today: "I dag", local: "Lokal",
    dutyPoint: "Driftpunkt", supplyAirflow: "Tilluftsflöde", extractAirflow: "Frånluftsflöde", staticPressure: "Statiskt tryck", results: "Sökresultat", compatibleUnits: "kompatibla aggregat", orderedByFit: "SFP ↑", technicalFit: "Nödvändig reglering", calculateAfterSelection: "Efter val", recommended: "Rekommenderad", selected: "Vald", catalogue: "Katalog", maximumAirflow: "Maximalt luftflöde", availablePressure: "Tillgängligt tryck", nominalEfficiency: "Nominell verkningsgrad", soundPower: "Ljudeffekt", currentUnit: "Aktuellt aggregat", chooseUnit: "Välj aggregat",
    airflowConfiguration: "Luftflödeskonfiguration", defaultHint: "Rekommenderat standardvärde för vald installation.", orientation: "Luftflödesriktning", lowerAccess: "Åtkomst underifrån", accessPanel: "Åtkomstpanel",
    enableCoil: "Aktivera batteri", calculationMode: "Beräkningsläge", coil: "Batteri", fluid: "Medium", glycol: "Glykol", coolingIn: "Kyla in", coolingOut: "Kyla ut", heatingIn: "Värme in", heatingOut: "Värme ut", rows: "rader", circuits: "kretsar", finSpacing: "lamellavstånd", performance: "Beräknad prestanda", enableToCalculate: "Aktivera batteriet för att beräkna prestandan.", noneAssociated: "Inget batteri är kopplat till aggregatet i den lokala databasen.", additionalPressureDrop: "Extra tryckfall på kurvan", capacity: "Effekt", airOut: "Luft ut", airPressureDrop: "Luft-DP",
    preheating: "Elektrisk förvärme", postHeating: "Elektrisk eftervärme", heater: "Elvärmebatteri", voltagePhases: "Spänning / faser", currentValue: "Ström", airIn: "Luft in", maximumAirOut: "Max. lufttemperatur ut", relativeHumidityOut: "RF ut", searchPlaceholder: "Sök kod eller beskrivning", allCategories: "Alla kategorier", selectedCount: "valda", code: "Kod", description: "Beskrivning", category: "Kategori",
    technicalSheet: "Tekniskt datablad", technicalSheetDescription: "Prestandadata och konfiguration", installationManual: "Installationshandbok", installationManualDescription: "Montering, anslutningar och underhåll", euDeclaration: "EU-försäkran", euDeclarationDescription: "Överensstämmelse och tillämpliga standarder", dimensionalDrawing: "Måttritning", dimensionalDrawingDescription: "Yttermått och konfigurationsmått", localPackageNotice: "Tillgänglighet och generering hanteras av det lokala produktionsflödet utifrån modell, konfiguration och språk.", offlineAvailable: "Tillgänglig offline", openDocument: "Öppna dokument",
    readyForReport: "Valet är klart för teknisk rapport.", reference: "Referens", requestedPressure: "Begärt tryck", options: "Alternativ", notSelectedFeminine: "Inte vald", notSelectedMasculine: "Inte vald", technicalReport: "Teknisk rapport", reportDescription: "Dokumentet använder språk och referens från aktuellt projekt.",
    completeSave: "Slutför lagringen i det öppna produktionsgränssnittet.", draftSavedAt: "Utkast sparat kl.", completeReport: "Slutför rapporten i det öppna produktionsgränssnittet.", reportReady: "Rapport klar",
  },
};

const baseMessages = (code: LanguageCode): FrontendMessages =>
  code === "en" ? en : locales[code];

const buildUiMessages = (
  code: LanguageCode,
  text: UiLexicon,
): UiMessages => {
  const base = baseMessages(code);
  return {
    aria: {
      notifications: text.notifications,
      selectionSteps: base.common.configuration,
      accessoryCategory: base.steps.accessories.title,
    },
    navigation: {
      componentShowcase: text.components,
      currentSelection: text.currentSelection,
      saveDraft: text.saveDraft,
    },
    context: {
      noUnit: text.noUnit,
      supply: text.supply,
      extract: text.extract,
      pressure: text.pressure,
      layout: text.layout,
      efficiency: text.efficiency,
      margin: text.margin,
      power: text.power,
      configuration: base.common.configuration,
    },
    project: {
      dataTitle: text.projectData,
      dataDescription: base.steps.project.description,
      name: text.projectName,
      defaultName: `${base.steps.project.title} 01`,
      customerReference: text.customerReference,
      referencePlaceholder: text.freeReference,
      documentLanguage: text.documentLanguage,
      statusTitle: text.projectStatus,
      statusDescription: base.tooltips.saveSelection,
      technicalReference: text.technicalReference,
      registered: text.registered,
      revision: text.revision,
      current: text.current,
      lastSaved: text.lastSaved,
      today: text.today,
      local: text.local,
    },
    preselection: {
      dutyPoint: text.dutyPoint,
      dutyPointDescription: base.steps.preselection.description,
      supplyAirflow: text.supplyAirflow,
      extractAirflow: text.extractAirflow,
      staticPressure: text.staticPressure,
      balancedNotice: base.tooltips.preselectionFilters,
      ...seasonalConditionMessages[code],
      results: text.results,
      compatibleUnits: text.compatibleUnits,
      orderedByFit: text.orderedByFit,
      technicalFit: text.technicalFit,
      calculateAfterSelection: text.calculateAfterSelection,
      recommended: text.recommended,
    },
    unit: {
      selected: text.selected,
      catalogue: text.catalogue,
      maximumAirflow: text.maximumAirflow,
      availablePressure: text.availablePressure,
      nominalEfficiency: text.nominalEfficiency,
      soundPower: text.soundPower,
      currentUnit: text.currentUnit,
      chooseUnit: text.chooseUnit,
    },
    installation: {
      typeTitle: base.steps.installation.title,
      typeDescription: base.steps.installation.description,
      ceilingDescription: base.tooltips.installationLayout,
      floorDescription: base.tooltips.installationLayout,
      wallDescription: base.tooltips.installationLayout,
      airflowConfiguration: text.airflowConfiguration,
      defaultHint: text.defaultHint,
      orientationTitle: text.orientation,
      previewDescription: base.tooltips.installationLayout,
      lowerAccess: text.lowerAccess,
      accessPanel: text.accessPanel,
    },
    waterCoil: {
      treatment: base.steps["water-coil"].title,
      enable: text.enableCoil,
      calculationMode: text.calculationMode,
      coil: text.coil,
      fluid: text.fluid,
      glycol: text.glycol,
      coolingIn: text.coolingIn,
      coolingOut: text.coolingOut,
      heatingIn: text.heatingIn,
      heatingOut: text.heatingOut,
      rows: text.rows,
      circuits: text.circuits,
      finSpacing: text.finSpacing,
      calculatedPerformance: text.performance,
      calculatedDescription: base.tooltips.waterCoil,
      enableToCalculate: text.enableToCalculate,
      noneAssociated: text.noneAssociated,
      additionalPressureDrop: text.additionalPressureDrop,
      capacity: text.capacity,
      airOut: text.airOut,
      airPressureDrop: text.airPressureDrop,
    },
    electricHeater: {
      preheating: text.preheating,
      postHeating: text.postHeating,
      enableCalculation: base.actions.calculate,
      unavailable: base.status.unavailable,
      heater: text.heater,
      installation: base.steps.installation.title,
      power: text.power,
      voltagePhases: text.voltagePhases,
      current: text.currentValue,
      airIn: text.airIn,
      maximumAirOut: text.maximumAirOut,
      relativeHumidityOut: text.relativeHumidityOut,
      airPressureDrop: text.airPressureDrop,
      optionalDescription: base.steps["electric-heaters"].description,
    },
    accessories: {
      searchPlaceholder: text.searchPlaceholder,
      allCategories: text.allCategories,
      selectedCount: text.selectedCount,
      code: text.code,
      description: text.description,
      category: text.category,
      installation: base.steps.installation.title,
    },
    documents: {
      technicalSheet: text.technicalSheet,
      technicalSheetDescription: text.technicalSheetDescription,
      installationManual: text.installationManual,
      installationManualDescription: text.installationManualDescription,
      euDeclaration: text.euDeclaration,
      euDeclarationDescription: text.euDeclarationDescription,
      dimensionalDrawing: text.dimensionalDrawing,
      dimensionalDrawingDescription: text.dimensionalDrawingDescription,
      localPackageNotice: text.localPackageNotice,
      offlineAvailable: text.offlineAvailable,
      openDocument: text.openDocument,
    },
    summary: {
      readyForReport: text.readyForReport,
      configuration: base.common.configuration,
      project: base.steps.project.title,
      reference: text.reference,
      unit: base.steps.unit.title,
      installation: base.steps.installation.title,
      dutyPoint: text.dutyPoint,
      supplyAirflow: text.supplyAirflow,
      extractAirflow: text.extractAirflow,
      requestedPressure: text.requestedPressure,
      availableMargin: text.margin,
      options: text.options,
      waterCoil: base.steps["water-coil"].title,
      notSelectedFeminine: text.notSelectedFeminine,
      notSelectedMasculine: text.notSelectedMasculine,
      electricPreheating: text.preheating,
      electricPostHeating: text.postHeating,
      accessories: base.steps.accessories.title,
      technicalReport: text.technicalReport,
      reportDescription: text.reportDescription,
    },
    showcase: {
      localDesignSystem: `${base.common.applicationName} UI`,
      title: text.components,
      description: base.tooltips.help,
      backToSelection: base.actions.back,
      actions: base.common.configuration,
      primary: base.actions.next,
      secondary: base.actions.save,
      destructive: base.actions.cancel,
      fields: base.common.configuration,
      text: text.description,
      exampleValue: base.status.ready,
      airflow: text.supplyAirflow,
      mode: text.calculationMode,
      activeOption: base.status.selected,
      messages: base.common.technicalSelection,
      allChecksPassed: base.status.valid,
      pressureMarginReduced: base.status.warning,
      correctDutyPoint: base.status.invalid,
      metricsAndTokens: base.common.configuration,
    },
    toast: {
      completeSave: text.completeSave,
      draftSavedAt: text.draftSavedAt,
      completeReport: text.completeReport,
      reportReady: text.reportReady,
    },
  };
};

export const uiMessages = Object.fromEntries(
  (Object.keys(lexicons) as LanguageCode[]).map((code) => [
    code,
    buildUiMessages(code, lexicons[code]),
  ]),
) as Record<LanguageCode, UiMessages>;
