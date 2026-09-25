import { airflowSlot, airflowSide } from "./airflow-layout";
import { schematicText, flowCircle, mountingSvg } from "./mounting-schematic";
import { projectActionLabels } from "./project-action-labels";
import { releaseInfo } from "./release-info.generated";
import {
  Activity,
  ArrowDown,
  ArrowLeft,
  ArrowRight,
  BadgeCheck,
  Bell,
  BookOpen,
  BriefcaseBusiness,
  Check,
  ChevronRight,
  CircleCheck,
  CircleHelp,
  CircleX,
  createIcons,
  ExternalLink,
  FileDown,
  FileText,
  FolderCheck,
  Gauge,
  HardDrive,
  HardDriveDownload,
  Info,
  LoaderCircle,
  Mail,
  Minus,
  PanelsTopLeft,
  PanelBottom,
  PanelLeft,
  PanelTop,
  Ruler,
  Save,
  Search,
  Settings,
  Sparkles,
  Trash2,
  TrendingUp,
  TriangleAlert,
  Wind,
  Zap,
} from "lucide";
import pdfWorkerUrl from "pdfjs-dist/build/pdf.worker.min.mjs?url";
import "./styles.css";
import { createBridge, logClientError } from "./bridge";
import { normalizeSelection } from "./bridge/normalizeSelection";
import { getHelpContent } from "./help";
import {
  getMessages,
  languageOptions,
  normalizeLanguageCode,
  type LocalizedFrontendMessages,
} from "./i18n";
import type {
  AccessoryOption,
  BootstrapData,
  DimensionalDrawingState,
  FollowUpCenterState,
  FollowUpReminder,
  InstallationMode,
  LayoutConfigurationOption,
  MultiProjectState,
  PerformanceCurveData,
  ProductDocumentState,
  ProjectSaveState,
  SelectionDraft,
  SelectionResult,
  StepId,
  UnitOption,
  WaterCoilPerformance,
} from "./bridge/contracts";

type StepDefinition = {
  id: StepId;
  optional?: boolean;
};

const steps: StepDefinition[] = [
  { id: "project" },
  { id: "preselection" },
  { id: "installation" },
  { id: "water-coil", optional: true },
  { id: "electric-heaters", optional: true },
  { id: "accessories", optional: true },
  { id: "co2", optional: true },
  { id: "sound", optional: true },
  { id: "documents", optional: true },
  { id: "summary" },
];

const bridge = createBridge();
const app = document.querySelector<HTMLDivElement>("#app");
const iconSet = {
  Activity,
  ArrowDown,
  ArrowLeft,
  ArrowRight,
  BadgeCheck,
  Bell,
  BookOpen,
  BriefcaseBusiness,
  Check,
  ChevronRight,
  CircleCheck,
  CircleHelp,
  CircleX,
  ExternalLink,
  FileDown,
  FileText,
  FolderCheck,
  Gauge,
  HardDrive,
  HardDriveDownload,
  Info,
  LoaderCircle,
  Mail,
  Minus,
  PanelsTopLeft,
  PanelBottom,
  PanelLeft,
  PanelTop,
  Ruler,
  Save,
  Search,
  Settings,
  Sparkles,
  Trash2,
  TrendingUp,
  TriangleAlert,
  Wind,
  Zap,
};

if (!app) {
  throw new Error("Application root not found.");
}

let data: BootstrapData | null = null;
let draft: SelectionDraft | null = null;
let result: SelectionResult | null = null;
let currentStep: StepId = "project";
let maximumReachableStepIndex = 1;
let confirmedUnitId: string | null = null;
let installationReviewRequired = false;
const visitedSteps = new Set<StepId>(["project"]);
const skippedOptionalSteps = new Set<StepId>();
let calculating = false;
let busyMessage: string | null = null;
let pendingProjectLanguage: string | null = null;
let activeProjectDocumentLanguage: string | null = null;
let saveLanguagePromptOpen = false;
let saveLanguagePromptSaveAs = false;
let saveLanguageChoice = "it";
let saveLanguageRemember = false;
let toastMessage = "";
let helpOpen = false;
let releaseInfoOpen = false;
let notificationCenterOpen = false;
let notificationState: FollowUpCenterState = {
  unreadDueCount: 0,
  reminders: [],
};
let projectState: ProjectSaveState | null = null;
let projectDirty = false;
let multiProjectState: MultiProjectState | null = null;
let projectEmailOpen = false;
let projectEmailSchedule = true;
let projectEmailDays = 7;
let preselectionRequestVersion = 0;
let additionalCriteriaOpen = false;
let calculationRequestVersion = 0;
let calculationFailed = false;
let productDocuments: ProductDocumentState | null = null;
let productDocumentsLoading = false;
let dimensionalDrawing: DimensionalDrawingState | null = null;
let dimensionalDrawingOpen = false;
let dimensionalDrawingLoading = false;
let dimensionalDrawingKey = "";
let dimensionalPreviewImage = "";
let dimensionalLargeImage = "";
let dimensionalRenderVersion = 0;
let tooltipsEnabled =
  window.localStorage.getItem("ssw-next.help-tooltips") !== "false";

const languageCode = () => {
  const raw = draft?.project.language ?? "it";
  const option = languageOptions.find(
    (item) => item.code === raw || item.name.toLowerCase() === raw.toLowerCase(),
  );
  return option?.code ?? normalizeLanguageCode(raw);
};

const messages = (): Readonly<LocalizedFrontendMessages> =>
  getMessages(languageCode());

const interfaceLanguageLabels: Record<string, string> = {
  en: "Interface language", it: "Lingua interfaccia", bg: "Език на интерфейса",
  cs: "Jazyk rozhraní", da: "Grænsefladesprog", de: "Sprache der Benutzeroberfläche",
  fr: "Langue de l’interface", hu: "Felület nyelve", is: "Tungumál viðmóts",
  nl: "Interfacetaal", no: "Grensesnittspråk", pl: "Język interfejsu",
  ro: "Limba interfeței", sl: "Jezik vmesnika", sv: "Gränssnittsspråk",
};

const interfaceLanguageLabel = (): string =>
  interfaceLanguageLabels[languageCode()] ?? interfaceLanguageLabels.en;

const accessLabels: Record<string, string> = {
  en: "Access", it: "Accesso", bg: "Достъп", cs: "Přístup", da: "Adgang",
  de: "Zugang", fr: "Accès", hu: "Hozzáférés", is: "Aðgangur",
  nl: "Toegang", no: "Tilgang", pl: "Dostęp", ro: "Acces", sl: "Dostop",
  sv: "Åtkomst",
};
const accessLabel = (): string => accessLabels[languageCode()] ?? accessLabels.en;

const observerAccessLabels: Record<string, string> = {
  en: "Observer-side access", it: "Accesso lato osservatore", bg: "Достъп от страната на наблюдателя",
  cs: "Přístup ze strany pozorovatele", da: "Adgang fra observatørsiden",
  de: "Zugang auf Betrachterseite", fr: "Accès côté observateur",
  hu: "Hozzáférés a megfigyelő oldaláról", is: "Aðgangur frá áhorfendahlið",
  nl: "Toegang aan waarnemerszijde", no: "Tilgang fra observatørsiden",
  pl: "Dostęp od strony obserwatora", ro: "Acces dinspre observator",
  sl: "Dostop s strani opazovalca", sv: "Åtkomst från betraktarsidan",
};
const observerAccessLabel = (): string => observerAccessLabels[languageCode()] ?? observerAccessLabels.en;

const wallViewLabels: Record<string, readonly [string, string]> = {
  en: ["East-West wall", "North-South wall"], it: ["Murale Est-Ovest", "Murale Nord-Sud"],
  bg: ["Стена изток-запад", "Стена север-юг"], cs: ["Stěna východ-západ", "Stěna sever-jih"],
  da: ["Væg øst-vest", "Væg nord-syd"], de: ["Wand Ost-West", "Wand Nord-Süd"],
  fr: ["Murale Est-Ouest", "Murale Nord-Sud"], hu: ["Fali kelet-nyugat", "Fali észak-dél"],
  is: ["Veggur austur-vestur", "Veggur norður-suður"], nl: ["Wand oost-west", "Wand noord-zuid"],
  no: ["Vegg øst-vest", "Vegg nord-sør"], pl: ["Ściana wschód-zachód", "Ściana północ-południe"],
  ro: ["Perete est-vest", "Perete nord-sud"], sl: ["Stena vzhod-zahod", "Stena sever-jug"],
  sv: ["Vägg öst-väst", "Vägg nord-syd"],
};

const installationViewLabel = (mode: InstallationMode, eastWestWall: boolean): string => {
  const installation = messages().domain.installation;
  if (mode === "ceiling") return installation.ceiling;
  if (mode === "floor") return installation.floor;
  const wallLabels = wallViewLabels[languageCode()] ?? wallViewLabels.en;
  return wallLabels[eastWestWall ? 0 : 1];
};

const localizedSoundPath = (rawCode: string, rawLabel: string): string => {
  const code = rawCode.trim().replace(/^_+/, "").toLowerCase();
  const text = messages();
  const labels: Record<string, string> = {
    fresh: text.domain.airflow.fresh,
    supply: text.domain.airflow.supply,
    exhaust: text.domain.airflow.exhaust,
    return: text.domain.airflow.return,
    breakout: text.ui.co2Sound.breakoutNoise,
  };
  return labels[code] ?? (rawLabel || rawCode);
};
const helpContent = () => getHelpContent(languageCode());
const releaseLabels = (): { title: string; version: string; builtAt: string; close: string } => ({
  en: { title: "Release information", version: "Release", builtAt: "Build date and time", close: "Close" },
  bg: { title: "Информация за версията", version: "Версия", builtAt: "Дата и час на компилация", close: "Затвори" },
  cs: { title: "Informace o verzi", version: "Verze", builtAt: "Datum a čas sestavení", close: "Zavřít" },
  da: { title: "Versionsoplysninger", version: "Version", builtAt: "Builddato og -tid", close: "Luk" },
  de: { title: "Versionsinformationen", version: "Version", builtAt: "Build-Datum und -Uhrzeit", close: "Schließen" },
  fr: { title: "Informations sur la version", version: "Version", builtAt: "Date et heure de compilation", close: "Fermer" },
  hu: { title: "Verzióinformáció", version: "Verzió", builtAt: "Build dátuma és időpontja", close: "Bezárás" },
  is: { title: "Útgáfuupplýsingar", version: "Útgáfa", builtAt: "Dagsetning og tími smíði", close: "Loka" },
  it: { title: "Informazioni sulla release", version: "Release", builtAt: "Data e ora della build", close: "Chiudi" },
  nl: { title: "Versie-informatie", version: "Versie", builtAt: "Builddatum en -tijd", close: "Sluiten" },
  no: { title: "Versjonsinformasjon", version: "Versjon", builtAt: "Byggedato og -tid", close: "Lukk" },
  pl: { title: "Informacje o wersji", version: "Wersja", builtAt: "Data i godzina kompilacji", close: "Zamknij" },
  ro: { title: "Informații despre versiune", version: "Versiune", builtAt: "Data și ora compilării", close: "Închide" },
  sl: { title: "Informacije o različici", version: "Različica", builtAt: "Datum in čas gradnje", close: "Zapri" },
  sv: { title: "Versionsinformation", version: "Version", builtAt: "Byggdatum och tid", close: "Stäng" },
}[languageCode()] ?? { title: "Release information", version: "Release", builtAt: "Build date and time", close: "Close" });
const saveLanguagePromptText = (): { title: string; message: string; useLanguage: string; remember: string; save: string; cancel: string } => ({
  it: { title: "Lingua dei documenti", message: "In quale lingua deve essere preparata l'offerta finale?", useLanguage: "Lingua dei documenti", remember: "Non chiedermelo più", save: "Salva", cancel: "Annulla" },
  en: { title: "Document language", message: "Which language should be used for the final offer?", useLanguage: "Document language", remember: "Do not ask again", save: "Save", cancel: "Cancel" },
  fr: { title: "Langue des documents", message: "Dans quelle langue l'offre finale doit-elle être préparée ?", useLanguage: "Langue des documents", remember: "Ne plus demander", save: "Enregistrer", cancel: "Annuler" },
  de: { title: "Dokumentsprache", message: "In welcher Sprache soll das endgültige Angebot erstellt werden?", useLanguage: "Dokumentsprache", remember: "Nicht mehr fragen", save: "Speichern", cancel: "Abbrechen" },
} as Record<string, { title: string; message: string; useLanguage: string; remember: string; save: string; cancel: string }>)[languageCode()] ?? { title: "Document language", message: "Which language should be used for the final offer?", useLanguage: "Document language", remember: "Do not ask again", save: "Save", cancel: "Cancel" };
const helpTitle = (key: keyof LocalizedFrontendMessages["tooltips"]): string =>
  tooltipsEnabled ? messages().tooltips[key] : "";

type WorkflowText = {
  openSelection: string;
  saveAs: string;
  notSaved: string;
  savedFile: string;
  selectionSummary: string;
  notifications: string;
  noNotifications: string;
  due: string;
  upcoming: string;
  closed: string;
  reschedule: string;
  successful: string;
  unsuccessful: string;
  days: string;
  fileMissing: string;
  modified: string;
};

const workflowTexts: Record<string, WorkflowText> = {
  it: { openSelection: "Apri selezione", saveAs: "Salva con nome", notSaved: "Non ancora salvata", savedFile: "File salvato", selectionSummary: "Riepilogo selezione", notifications: "Promemoria", noNotifications: "Nessun promemoria presente.", due: "Scaduto", upcoming: "In programma", closed: "Chiuso", reschedule: "Riprogramma", successful: "Chiudi con successo", unsuccessful: "Chiudi senza successo", days: "giorni", fileMissing: "File locale non disponibile", modified: "Modificata" },
  en: { openSelection: "Open selection", saveAs: "Save as", notSaved: "Not saved yet", savedFile: "Saved file", selectionSummary: "Selection summary", notifications: "Reminders", noNotifications: "No reminders.", due: "Due", upcoming: "Upcoming", closed: "Closed", reschedule: "Reschedule", successful: "Close successful", unsuccessful: "Close unsuccessful", days: "days", fileMissing: "Local file unavailable", modified: "Modified" },
  cs: { openSelection: "Otevřít výběr", saveAs: "Uložit jako", notSaved: "Dosud neuloženo", savedFile: "Uložený soubor", selectionSummary: "Souhrn výběru", notifications: "Připomínky", noNotifications: "Žádné připomínky.", due: "Po termínu", upcoming: "Naplánováno", closed: "Uzavřeno", reschedule: "Přeplánovat", successful: "Uzavřít úspěšně", unsuccessful: "Uzavřít neúspěšně", days: "dnů", fileMissing: "Místní soubor není dostupný", modified: "Změněno" },
  de: { openSelection: "Auswahl öffnen", saveAs: "Speichern unter", notSaved: "Noch nicht gespeichert", savedFile: "Gespeicherte Datei", selectionSummary: "Auswahlübersicht", notifications: "Erinnerungen", noNotifications: "Keine Erinnerungen.", due: "Fällig", upcoming: "Geplant", closed: "Geschlossen", reschedule: "Neu planen", successful: "Erfolgreich schließen", unsuccessful: "Erfolglos schließen", days: "Tage", fileMissing: "Lokale Datei nicht verfügbar", modified: "Geändert" },
  fr: { openSelection: "Ouvrir la sélection", saveAs: "Enregistrer sous", notSaved: "Pas encore enregistrée", savedFile: "Fichier enregistré", selectionSummary: "Résumé de la sélection", notifications: "Rappels", noNotifications: "Aucun rappel.", due: "Échu", upcoming: "Planifié", closed: "Fermé", reschedule: "Replanifier", successful: "Clôturer avec succès", unsuccessful: "Clôturer sans succès", days: "jours", fileMissing: "Fichier local indisponible", modified: "Modifiée" },
};

const workflowText = (): WorkflowText =>
  workflowTexts[languageCode()] ?? workflowTexts.en;

type DocumentBusyText = {
  checking: string;
  opening: string;
};

const documentBusyTexts: Record<string, DocumentBusyText> = {
  bg: { checking: "Проверка на наличните документи", opening: "Подготовка и изтегляне на документа" },
  cs: { checking: "Kontrola dostupných dokumentů", opening: "Příprava a stahování dokumentu" },
  da: { checking: "Kontrollerer tilgængelige dokumenter", opening: "Forbereder og downloader dokumentet" },
  de: { checking: "Verfügbare Dokumente werden geprüft", opening: "Dokument wird vorbereitet und heruntergeladen" },
  en: { checking: "Checking available documents", opening: "Preparing and downloading the document" },
  fr: { checking: "Vérification des documents disponibles", opening: "Préparation et téléchargement du document" },
  hu: { checking: "Elérhető dokumentumok ellenőrzése", opening: "A dokumentum előkészítése és letöltése" },
  is: { checking: "Athuga tiltæk skjöl", opening: "Undirbý og sæki skjalið" },
  it: { checking: "Verifica dei documenti disponibili", opening: "Preparazione e download del documento" },
  nl: { checking: "Beschikbare documenten controleren", opening: "Document voorbereiden en downloaden" },
  no: { checking: "Kontrollerer tilgjengelige dokumenter", opening: "Forbereder og laster ned dokumentet" },
  pl: { checking: "Sprawdzanie dostępnych dokumentów", opening: "Przygotowywanie i pobieranie dokumentu" },
  ro: { checking: "Verificarea documentelor disponibile", opening: "Pregătirea și descărcarea documentului" },
  sl: { checking: "Preverjanje razpoložljivih dokumentov", opening: "Priprava in prenos dokumenta" },
  sv: { checking: "Kontrollerar tillgängliga dokument", opening: "Förbereder och hämtar dokumentet" },
};

const documentBusyText = (): DocumentBusyText =>
  documentBusyTexts[languageCode()] ?? documentBusyTexts.en;

type DrawingUiText = {
  confirmLayout: string;
  downloadPdf: string;
};

const drawingUiTexts: Record<string, DrawingUiText> = {
  bg: { confirmLayout: "Моля, потвърдете конфигурацията.", downloadPdf: "Изтегляне на PDF" },
  cs: { confirmLayout: "Potvrďte prosím uspořádání.", downloadPdf: "Stáhnout PDF" },
  da: { confirmLayout: "Bekræft venligst layoutet.", downloadPdf: "Download PDF" },
  de: { confirmLayout: "Bitte bestätigen Sie das Layout.", downloadPdf: "PDF herunterladen" },
  en: { confirmLayout: "Please confirm the layout.", downloadPdf: "Download PDF" },
  fr: { confirmLayout: "Veuillez confirmer la configuration.", downloadPdf: "Télécharger le PDF" },
  hu: { confirmLayout: "Kérjük, erősítse meg az elrendezést.", downloadPdf: "PDF letöltése" },
  is: { confirmLayout: "Vinsamlegast staðfestið uppsetninguna.", downloadPdf: "Sækja PDF" },
  it: { confirmLayout: "Si prega di confermare il layout.", downloadPdf: "Scarica PDF" },
  nl: { confirmLayout: "Bevestig de lay-out.", downloadPdf: "PDF downloaden" },
  no: { confirmLayout: "Bekreft oppsettet.", downloadPdf: "Last ned PDF" },
  pl: { confirmLayout: "Proszę potwierdzić układ.", downloadPdf: "Pobierz PDF" },
  ro: { confirmLayout: "Vă rugăm să confirmați configurația.", downloadPdf: "Descărcați PDF-ul" },
  sl: { confirmLayout: "Potrdite postavitev.", downloadPdf: "Prenesi PDF" },
  sv: { confirmLayout: "Bekräfta layouten.", downloadPdf: "Hämta PDF" },
};

const drawingUiText = (): DrawingUiText =>
  drawingUiTexts[languageCode()] ?? drawingUiTexts.en;

const renderDimensionalValues = (drawing: DimensionalDrawingState): string => {
  const dimensionRows = (drawing.visibleDimensions ?? [])
    .map((item) => `<tr><th>${escapeHtml(item.code)}</th><td>${formatNumber(item.valueMillimeters, 0)} mm</td></tr>`)
    .join("");
  const weightRow = drawing.unitWeightKilograms != null && drawing.unitWeightKilograms !== 0
    ? `<tr><th>Peso</th><td>${formatNumber(drawing.unitWeightKilograms, 0)} kg</td></tr>`
    : "";
  return `<table>${dimensionRows ? `<tbody>${dimensionRows}</tbody>` : ""}${weightRow ? `<tbody class="additional-dimensions">${weightRow}</tbody>` : ""}</table>`;
};

const renderDimensionalSurface = (large: boolean): string => {
  const image = large ? dimensionalLargeImage : dimensionalPreviewImage;
  if (image) {
    return `<img src="${image}" alt="${escapeHtml(messages().ui.documents.dimensionalDrawing)}" />`;
  }
  return `<canvas data-dimensional-canvas="${large ? "large" : "preview"}" aria-label="${escapeHtml(messages().ui.documents.dimensionalDrawing)}"></canvas>`;
};

const renderDimensionalDrawing = (): string => {
  if (!dimensionalDrawingOpen || !dimensionalDrawing) return "";
  const text = messages();
  const drawing = dimensionalDrawing;
  return `<div class="modal-backdrop" data-action="close-dimensional-drawing">
    <section class="dimensional-drawing-dialog" role="dialog" aria-modal="true" aria-labelledby="dimensional-drawing-title">
      <header class="panel-heading compact">
        <div>
          <h2 id="dimensional-drawing-title">${escapeHtml(text.ui.documents.dimensionalDrawing)}</h2>
          <p>${escapeHtml(text.ui.documents.dimensionalDrawingDescription)}</p>
        </div>
        <div class="dimensional-dialog-actions">
          <button class="button secondary compact-button" data-action="download-dimensional-drawing" type="button">${icon("file-down", 15)} ${escapeHtml(drawingUiText().downloadPdf)}</button>
          <button class="icon-button bordered" data-action="close-dimensional-drawing" aria-label="${escapeHtml(text.actions.close)}">${icon("circle-x")}</button>
        </div>
      </header>
      <div class="dimensional-drawing-content">
        <div class="dimensional-image-frame large">
          ${drawing.available && drawing.contentBase64
            ? `${renderDimensionalSurface(true)}
              ${drawing.brandingLogoBase64
                ? `<img class="dimensional-preview-logo" src="data:image/png;base64,${drawing.brandingLogoBase64}" alt="Avensys" />`
                : ""}`
            : `<div class="empty-state">${escapeHtml(text.status.unavailable)}</div>`}
        </div>
        <aside class="dimensional-values">
          ${renderDimensionalValues(drawing)}
        </aside>
      </div>
    </section>
  </div>`;
};

const renderInlineDimensionalDrawing = (): string => {
  const text = messages();
  const drawing = dimensionalDrawing;
  return `<section class="panel dimensional-inline-panel">
    <div class="panel-heading compact">
      <div><h2>${escapeHtml(text.ui.documents.dimensionalDrawing)}</h2><p>${escapeHtml(text.ui.documents.dimensionalDrawingDescription)}</p></div>
    </div>
    ${dimensionalDrawingLoading || !drawing
      ? `<div class="dimensional-inline-loading">${icon("loader-circle")} ${escapeHtml(text.status.calculating)}</div>`
      : `<div class="dimensional-inline-content">
          <button class="dimensional-inline-preview" type="button" data-action="open-dimensional-drawing" ${drawing.available ? "" : "disabled"}>
            ${drawing.available && drawing.contentBase64
              ? renderDimensionalSurface(false)
              : `<span class="empty-state">${escapeHtml(text.status.unavailable)}</span>`}
          </button>
          <div class="dimensional-values compact">${renderDimensionalValues(drawing)}</div>
        </div>`}
  </section>`;
};

type MultiProjectText = {
  workspace: string;
  description: string;
  newProject: string;
  openProject: string;
  saveProject: string;
  saveProjectAs: string;
  addCurrent: string;
  emailProject: string;
  empty: string;
  reference: string;
  unit: string;
  airflow: string;
  pressure: string;
  pdf: string;
  status: string;
  ready: string;
  stale: string;
  open: string;
  remove: string;
  unsaved: string;
  emailTitle: string;
  emailDescription: string;
  schedule: string;
  followUpDays: string;
  prepareEmail: string;
  cancel: string;
  removeConfirm: string;
};

const multiProjectTexts: Record<string, MultiProjectText> = {
  en: { workspace: "Project units", description: "Collect, update and send all technical selections in one project.", newProject: "New", openProject: "Open", saveProject: "Save project", saveProjectAs: "Save as", addCurrent: "Add or update current selection", emailProject: "Prepare project email", empty: "No selections have been added to this project.", reference: "Reference", unit: "Selected unit", airflow: "Airflow", pressure: "Pressure", pdf: "PDF", status: "Status", ready: "Ready", stale: "Update required", open: "Open", remove: "Remove", unsaved: "Unsaved changes", emailTitle: "Project email", emailDescription: "All project PDFs will be attached to the localized message.", schedule: "Schedule customer follow-up", followUpDays: "Follow-up after", prepareEmail: "Open email", cancel: "Cancel", removeConfirm: "Remove this selection from the project?" },
  it: { workspace: "Unità del progetto", description: "Raccogli, aggiorna e invia tutte le selezioni tecniche in un unico progetto.", newProject: "Nuovo", openProject: "Apri", saveProject: "Salva progetto", saveProjectAs: "Salva con nome", addCurrent: "Aggiungi o aggiorna la selezione corrente", emailProject: "Prepara email progetto", empty: "Nessuna selezione è stata aggiunta al progetto.", reference: "Riferimento", unit: "Unità selezionata", airflow: "Portata", pressure: "Pressione", pdf: "PDF", status: "Stato", ready: "Pronto", stale: "Da aggiornare", open: "Apri", remove: "Rimuovi", unsaved: "Modifiche non salvate", emailTitle: "Email progetto", emailDescription: "Tutti i PDF del progetto saranno allegati al testo localizzato.", schedule: "Programma il sollecito cliente", followUpDays: "Sollecito dopo", prepareEmail: "Apri email", cancel: "Annulla", removeConfirm: "Rimuovere questa selezione dal progetto?" },
  cs: { workspace: "Jednotky projektu", description: "Shromažďujte, aktualizujte a odesílejte technické výběry v jednom projektu.", newProject: "Nový", openProject: "Otevřít", saveProject: "Uložit projekt", saveProjectAs: "Uložit jako", addCurrent: "Přidat nebo aktualizovat aktuální výběr", emailProject: "Připravit e-mail projektu", empty: "Do projektu nebyl přidán žádný výběr.", reference: "Reference", unit: "Vybraná jednotka", airflow: "Průtok vzduchu", pressure: "Tlak", pdf: "PDF", status: "Stav", ready: "Připraveno", stale: "Vyžaduje aktualizaci", open: "Otevřít", remove: "Odebrat", unsaved: "Neuložené změny", emailTitle: "E-mail projektu", emailDescription: "Všechny PDF projektu budou přiloženy k lokalizované zprávě.", schedule: "Naplánovat připomenutí zákazníka", followUpDays: "Připomenout za", prepareEmail: "Otevřít e-mail", cancel: "Zrušit", removeConfirm: "Odebrat tento výběr z projektu?" },
  de: { workspace: "Geräte im Projekt", description: "Technische Auswahlen in einem Projekt sammeln, aktualisieren und senden.", newProject: "Neu", openProject: "Öffnen", saveProject: "Projekt speichern", saveProjectAs: "Speichern unter", addCurrent: "Aktuelle Auswahl hinzufügen oder aktualisieren", emailProject: "Projekt-E-Mail vorbereiten", empty: "Diesem Projekt wurden keine Auswahlen hinzugefügt.", reference: "Referenz", unit: "Ausgewähltes Gerät", airflow: "Luftmenge", pressure: "Druck", pdf: "PDF", status: "Status", ready: "Bereit", stale: "Aktualisierung erforderlich", open: "Öffnen", remove: "Entfernen", unsaved: "Nicht gespeicherte Änderungen", emailTitle: "Projekt-E-Mail", emailDescription: "Alle Projekt-PDFs werden an die lokalisierte Nachricht angehängt.", schedule: "Kundennachverfolgung planen", followUpDays: "Nachverfolgung nach", prepareEmail: "E-Mail öffnen", cancel: "Abbrechen", removeConfirm: "Diese Auswahl aus dem Projekt entfernen?" },
  fr: { workspace: "Unités du projet", description: "Regroupez, mettez à jour et envoyez les sélections techniques dans un seul projet.", newProject: "Nouveau", openProject: "Ouvrir", saveProject: "Enregistrer le projet", saveProjectAs: "Enregistrer sous", addCurrent: "Ajouter ou mettre à jour la sélection courante", emailProject: "Préparer l’e-mail du projet", empty: "Aucune sélection n’a été ajoutée à ce projet.", reference: "Référence", unit: "Unité sélectionnée", airflow: "Débit", pressure: "Pression", pdf: "PDF", status: "État", ready: "Prêt", stale: "Mise à jour requise", open: "Ouvrir", remove: "Supprimer", unsaved: "Modifications non enregistrées", emailTitle: "E-mail du projet", emailDescription: "Tous les PDF du projet seront joints au message localisé.", schedule: "Planifier le suivi client", followUpDays: "Suivi après", prepareEmail: "Ouvrir l’e-mail", cancel: "Annuler", removeConfirm: "Supprimer cette sélection du projet ?" },
  bg: { workspace: "Модули в проекта", description: "Събирайте, актуализирайте и изпращайте техническите избори в един проект.", newProject: "Нов", openProject: "Отвори", saveProject: "Запази проекта", saveProjectAs: "Запази като", addCurrent: "Добави или актуализирай текущия избор", emailProject: "Подготви имейл за проекта", empty: "Към проекта няма добавени избори.", reference: "Референция", unit: "Избран модул", airflow: "Въздушен дебит", pressure: "Налягане", pdf: "PDF", status: "Състояние", ready: "Готово", stale: "Нужно е обновяване", open: "Отвори", remove: "Премахни", unsaved: "Незапазени промени", emailTitle: "Имейл за проекта", emailDescription: "Всички PDF файлове ще бъдат приложени към локализираното съобщение.", schedule: "Планирай проследяване с клиента", followUpDays: "Проследяване след", prepareEmail: "Отвори имейла", cancel: "Отказ", removeConfirm: "Да се премахне ли този избор от проекта?" },
  da: { workspace: "Projektets aggregater", description: "Saml, opdater og send tekniske valg i ét projekt.", newProject: "Nyt", openProject: "Åbn", saveProject: "Gem projekt", saveProjectAs: "Gem som", addCurrent: "Tilføj eller opdater aktuelt valg", emailProject: "Forbered projektmail", empty: "Der er ikke tilføjet valg til projektet.", reference: "Reference", unit: "Valgt aggregat", airflow: "Luftmængde", pressure: "Tryk", pdf: "PDF", status: "Status", ready: "Klar", stale: "Opdatering kræves", open: "Åbn", remove: "Fjern", unsaved: "Ikke-gemte ændringer", emailTitle: "Projektmail", emailDescription: "Alle projektets PDF-filer vedhæftes den lokaliserede besked.", schedule: "Planlæg kundeopfølgning", followUpDays: "Opfølgning efter", prepareEmail: "Åbn mail", cancel: "Annuller", removeConfirm: "Fjern dette valg fra projektet?" },
  hu: { workspace: "A projekt egységei", description: "A műszaki kiválasztások gyűjtése, frissítése és küldése egy projektben.", newProject: "Új", openProject: "Megnyitás", saveProject: "Projekt mentése", saveProjectAs: "Mentés másként", addCurrent: "Aktuális kiválasztás hozzáadása vagy frissítése", emailProject: "Projekt e-mail előkészítése", empty: "A projekthez még nincs kiválasztás hozzáadva.", reference: "Hivatkozás", unit: "Kiválasztott egység", airflow: "Légszállítás", pressure: "Nyomás", pdf: "PDF", status: "Állapot", ready: "Kész", stale: "Frissítés szükséges", open: "Megnyitás", remove: "Eltávolítás", unsaved: "Nem mentett módosítások", emailTitle: "Projekt e-mail", emailDescription: "A projekt összes PDF-fájlja csatolva lesz a lokalizált üzenethez.", schedule: "Ügyfélkövetés ütemezése", followUpDays: "Követés ennyi nap múlva", prepareEmail: "E-mail megnyitása", cancel: "Mégse", removeConfirm: "Eltávolítja ezt a kiválasztást a projektből?" },
  is: { workspace: "Einingar verkefnisins", description: "Safnaðu, uppfærðu og sendu tæknilegt val í einu verkefni.", newProject: "Nýtt", openProject: "Opna", saveProject: "Vista verkefni", saveProjectAs: "Vista sem", addCurrent: "Bæta við eða uppfæra núverandi val", emailProject: "Undirbúa verkefnispóst", empty: "Engu vali hefur verið bætt við verkefnið.", reference: "Tilvísun", unit: "Valin eining", airflow: "Loftflæði", pressure: "Þrýstingur", pdf: "PDF", status: "Staða", ready: "Tilbúið", stale: "Uppfærsla nauðsynleg", open: "Opna", remove: "Fjarlægja", unsaved: "Óvistaðar breytingar", emailTitle: "Verkefnispóstur", emailDescription: "Öll PDF-skjöl verkefnisins verða hengd við staðfærða skilaboðið.", schedule: "Skipuleggja eftirfylgni við viðskiptavin", followUpDays: "Eftirfylgni eftir", prepareEmail: "Opna tölvupóst", cancel: "Hætta við", removeConfirm: "Fjarlægja þetta val úr verkefninu?" },
  nl: { workspace: "Projectunits", description: "Verzamel, actualiseer en verzend technische selecties in één project.", newProject: "Nieuw", openProject: "Openen", saveProject: "Project opslaan", saveProjectAs: "Opslaan als", addCurrent: "Huidige selectie toevoegen of bijwerken", emailProject: "Projectmail voorbereiden", empty: "Er zijn geen selecties aan dit project toegevoegd.", reference: "Referentie", unit: "Geselecteerde unit", airflow: "Luchtdebiet", pressure: "Druk", pdf: "PDF", status: "Status", ready: "Gereed", stale: "Bijwerken vereist", open: "Openen", remove: "Verwijderen", unsaved: "Niet-opgeslagen wijzigingen", emailTitle: "Projectmail", emailDescription: "Alle PDF-bestanden van het project worden aan het gelokaliseerde bericht toegevoegd.", schedule: "Klantopvolging plannen", followUpDays: "Opvolging na", prepareEmail: "E-mail openen", cancel: "Annuleren", removeConfirm: "Deze selectie uit het project verwijderen?" },
  no: { workspace: "Prosjektaggregater", description: "Samle, oppdater og send tekniske valg i ett prosjekt.", newProject: "Nytt", openProject: "Åpne", saveProject: "Lagre prosjekt", saveProjectAs: "Lagre som", addCurrent: "Legg til eller oppdater gjeldende valg", emailProject: "Forbered prosjekt-e-post", empty: "Ingen valg er lagt til i prosjektet.", reference: "Referanse", unit: "Valgt aggregat", airflow: "Luftmengde", pressure: "Trykk", pdf: "PDF", status: "Status", ready: "Klar", stale: "Oppdatering kreves", open: "Åpne", remove: "Fjern", unsaved: "Ulagrede endringer", emailTitle: "Prosjekt-e-post", emailDescription: "Alle PDF-filer i prosjektet legges ved den lokaliserte meldingen.", schedule: "Planlegg kundeoppfølging", followUpDays: "Oppfølging etter", prepareEmail: "Åpne e-post", cancel: "Avbryt", removeConfirm: "Fjerne dette valget fra prosjektet?" },
  pl: { workspace: "Jednostki projektu", description: "Zbieraj, aktualizuj i wysyłaj dobory techniczne w jednym projekcie.", newProject: "Nowy", openProject: "Otwórz", saveProject: "Zapisz projekt", saveProjectAs: "Zapisz jako", addCurrent: "Dodaj lub zaktualizuj bieżący dobór", emailProject: "Przygotuj e-mail projektu", empty: "Do projektu nie dodano żadnego doboru.", reference: "Referencja", unit: "Wybrana jednostka", airflow: "Przepływ powietrza", pressure: "Ciśnienie", pdf: "PDF", status: "Stan", ready: "Gotowe", stale: "Wymaga aktualizacji", open: "Otwórz", remove: "Usuń", unsaved: "Niezapisane zmiany", emailTitle: "E-mail projektu", emailDescription: "Wszystkie pliki PDF projektu zostaną dołączone do zlokalizowanej wiadomości.", schedule: "Zaplanuj kontakt z klientem", followUpDays: "Kontakt po", prepareEmail: "Otwórz e-mail", cancel: "Anuluj", removeConfirm: "Usunąć ten dobór z projektu?" },
  ro: { workspace: "Unitățile proiectului", description: "Colectați, actualizați și trimiteți selecțiile tehnice într-un singur proiect.", newProject: "Nou", openProject: "Deschide", saveProject: "Salvează proiectul", saveProjectAs: "Salvează ca", addCurrent: "Adaugă sau actualizează selecția curentă", emailProject: "Pregătește e-mailul proiectului", empty: "Nu a fost adăugată nicio selecție în proiect.", reference: "Referință", unit: "Unitate selectată", airflow: "Debit de aer", pressure: "Presiune", pdf: "PDF", status: "Stare", ready: "Pregătit", stale: "Necesită actualizare", open: "Deschide", remove: "Elimină", unsaved: "Modificări nesalvate", emailTitle: "E-mail proiect", emailDescription: "Toate PDF-urile proiectului vor fi atașate mesajului localizat.", schedule: "Programează revenirea la client", followUpDays: "Revenire după", prepareEmail: "Deschide e-mailul", cancel: "Anulează", removeConfirm: "Eliminați această selecție din proiect?" },
  sl: { workspace: "Enote projekta", description: "Zbirajte, posodabljajte in pošiljajte tehnične izbire v enem projektu.", newProject: "Novo", openProject: "Odpri", saveProject: "Shrani projekt", saveProjectAs: "Shrani kot", addCurrent: "Dodaj ali posodobi trenutno izbiro", emailProject: "Pripravi e-pošto projekta", empty: "V projekt ni bila dodana nobena izbira.", reference: "Referenca", unit: "Izbrana enota", airflow: "Pretok zraka", pressure: "Tlak", pdf: "PDF", status: "Stanje", ready: "Pripravljeno", stale: "Potrebna posodobitev", open: "Odpri", remove: "Odstrani", unsaved: "Neshranjene spremembe", emailTitle: "E-pošta projekta", emailDescription: "Vse datoteke PDF projekta bodo priložene lokaliziranemu sporočilu.", schedule: "Načrtuj stik s stranko", followUpDays: "Stik po", prepareEmail: "Odpri e-pošto", cancel: "Prekliči", removeConfirm: "Odstranim to izbiro iz projekta?" },
  sv: { workspace: "Projektaggregat", description: "Samla, uppdatera och skicka tekniska val i ett projekt.", newProject: "Nytt", openProject: "Öppna", saveProject: "Spara projekt", saveProjectAs: "Spara som", addCurrent: "Lägg till eller uppdatera aktuellt val", emailProject: "Förbered projektmeddelande", empty: "Inga val har lagts till i projektet.", reference: "Referens", unit: "Valt aggregat", airflow: "Luftflöde", pressure: "Tryck", pdf: "PDF", status: "Status", ready: "Klar", stale: "Uppdatering krävs", open: "Öppna", remove: "Ta bort", unsaved: "Osparade ändringar", emailTitle: "Projektmeddelande", emailDescription: "Alla projektets PDF-filer bifogas det lokaliserade meddelandet.", schedule: "Planera kunduppföljning", followUpDays: "Uppföljning efter", prepareEmail: "Öppna e-post", cancel: "Avbryt", removeConfirm: "Ta bort detta val från projektet?" },
};

const multiProjectText = (): MultiProjectText =>
  multiProjectTexts[languageCode()] ?? multiProjectTexts.en;

const escapeHtml = (value: unknown): string =>
  String(value ?? "")
    .replaceAll("&", "&amp;")
    .replaceAll("<", "&lt;")
    .replaceAll(">", "&gt;")
    .replaceAll('"', "&quot;")
    .replaceAll("'", "&#039;");

const icon = (name: string, size = 17): string =>
  `<i data-lucide="${name}" width="${size}" height="${size}" aria-hidden="true"></i>`;

const renderIcons = (): void => {
  createIcons({ icons: iconSet });
};

const renderNotificationCenter = (): string => {
  if (!notificationCenterOpen) return "";
  const copy = workflowText();
  const pending = notificationState.reminders
    .filter((item) => item.status === "Pending")
    .sort((left, right) => left.dueAt.localeCompare(right.dueAt));
  const closed = notificationState.reminders
    .filter((item) => item.status !== "Pending")
    .sort((left, right) => right.dueAt.localeCompare(left.dueAt));
  const rows = [...pending, ...closed]
    .map((item) => renderNotificationRow(item, copy))
    .join("");
  return `<div class="modal-backdrop" data-action="close-notifications">
    <section class="notification-dialog" role="dialog" aria-modal="true">
      <header>
        <div><h2>${escapeHtml(copy.notifications)}</h2><p>${escapeHtml(messages().tooltips.notifications)}</p></div>
        <button class="icon-button bordered" data-action="close-notifications" aria-label="${escapeHtml(messages().actions.close)}">${icon("circle-x")}</button>
      </header>
      <div class="notification-list">
        ${rows || `<div class="empty-state">${icon("bell")}<p>${escapeHtml(copy.noNotifications)}</p></div>`}
      </div>
    </section>
  </div>`;
};

const renderNotificationRow = (
  item: FollowUpReminder,
  copy: WorkflowText,
): string => {
  const closed = item.status !== "Pending";
  const state = closed ? copy.closed : item.due ? copy.due : copy.upcoming;
  const due = new Date(item.dueAt).toLocaleDateString(messages().locale);
  return `<article class="notification-row ${item.due && !closed ? "due" : ""}" data-reminder-open="${item.id}" title="${escapeHtml(item.fileAvailable ? copy.openSelection : copy.fileMissing)}">
    <div class="notification-status">${icon(closed ? "circle-check" : item.due ? "triangle-alert" : "bell")}<span>${escapeHtml(state)}</span></div>
    <div class="notification-copy">
      <strong>${escapeHtml(item.reference || item.targetType)}</strong>
      <small>${escapeHtml(due)}${item.fileAvailable ? "" : ` · ${escapeHtml(copy.fileMissing)}`}</small>
    </div>
    ${closed ? "" : `<div class="notification-actions">
      <label><input type="number" min="1" max="90" value="7" data-reminder-days="${item.id}" aria-label="${escapeHtml(copy.days)}"><span>${escapeHtml(copy.days)}</span></label>
      <button class="button secondary compact-button" data-reminder-action="reschedule" data-reminder-id="${item.id}">${escapeHtml(copy.reschedule)}</button>
      <button class="button secondary compact-button" data-reminder-action="succeeded" data-reminder-id="${item.id}">${escapeHtml(copy.successful)}</button>
      <button class="button secondary compact-button" data-reminder-action="unsuccessful" data-reminder-id="${item.id}">${escapeHtml(copy.unsuccessful)}</button>
    </div>`}
  </article>`;
};

const renderProjectEmailDialog = (): string => {
  if (!projectEmailOpen) return "";
  const copy = multiProjectText();
  return `<div class="modal-backdrop" data-action="close-project-email">
    <section class="project-email-dialog" role="dialog" aria-modal="true">
      <header>
        <div><h2>${escapeHtml(copy.emailTitle)}</h2><p>${escapeHtml(copy.emailDescription)}</p></div>
        <button class="icon-button bordered" data-action="close-project-email" aria-label="${escapeHtml(copy.cancel)}">${icon("circle-x")}</button>
      </header>
      <div class="project-email-options">
        <label class="toggle">
          <input type="checkbox" data-project-email-schedule ${projectEmailSchedule ? "checked" : ""}>
          <span></span><b>${escapeHtml(copy.schedule)}</b>
        </label>
        <label class="field ${projectEmailSchedule ? "" : "is-disabled"}">
          <span>${escapeHtml(copy.followUpDays)}</span>
          <span class="input-with-unit">
            <input type="number" min="1" max="90" value="${projectEmailDays}" data-project-email-days ${projectEmailSchedule ? "" : "disabled"}>
            <span>${escapeHtml(workflowText().days)}</span>
          </span>
        </label>
      </div>
      <footer>
        <button class="button secondary" data-action="close-project-email">${escapeHtml(copy.cancel)}</button>
        <button class="button primary" data-action="send-project-email">${icon("mail")} ${escapeHtml(copy.prepareEmail)}</button>
      </footer>
    </section>
  </div>`;
};

const selectedUnit = (): UnitOption | undefined =>
  data?.units.find((unit) => unit.id === draft?.selectedUnitId);

const stepIndex = (step: StepId): number =>
  steps.findIndex((candidate) => candidate.id === step);

const canNavigateToStep = (step: StepId): boolean =>
  stepIndex(step) <= maximumReachableStepIndex;

const unlockConfiguredWorkflow = (): void => {
  maximumReachableStepIndex = steps.length - 1;
};

const lockWorkflowAtPreselection = (): void => {
  maximumReachableStepIndex = 1;
};

const confirmInstallationReview = (): void => {
  installationReviewRequired = false;
};

const resetOptionalStepVisits = (): void => {
  steps.filter((step) => step.optional).forEach((step) => visitedSteps.delete(step.id));
  skippedOptionalSteps.clear();
};

const markSkippedOptionalSteps = (from: StepId, to: StepId): void => {
  const fromIndex = stepIndex(from);
  const toIndex = stepIndex(to);
  if (toIndex <= fromIndex) return;
  steps.slice(fromIndex + 1, toIndex).forEach((step) => {
    if (step.optional && !visitedSteps.has(step.id)) skippedOptionalSteps.add(step.id);
  });
};

const restoreConfiguredWorkflow = (): void => {
  confirmedUnitId = draft?.selectedUnitId || null;
  confirmInstallationReview();
  visitedSteps.clear();
  visitedSteps.add("project");
  unlockConfiguredWorkflow();
};

const stateTone = (): string => result?.status ?? "valid";

const formatNumber = (value: number, maximumFractionDigits = 1): string =>
  new Intl.NumberFormat(messages().locale, { maximumFractionDigits }).format(value);

type ChartSeries = {
  x: number[];
  y: number[];
  className: string;
  label: string;
};

const finiteMaximum = (values: number[], fallback: number): number => {
  const finite = values.filter((value) => Number.isFinite(value) && value >= 0);
  return finite.length > 0 ? Math.max(...finite) : fallback;
};

const roundedAxisMaximum = (value: number): number => {
  const positive = Math.max(1, value);
  const magnitude = 10 ** Math.floor(Math.log10(positive));
  return Math.ceil(positive / magnitude) * magnitude;
};

const chartPath = (
  series: ChartSeries,
  xMaximum: number,
  yMinimum: number,
  yMaximum: number,
): string => {
  const points = series.x
    .map((x, index) => ({ x, y: series.y[index] }))
    .filter(
      (point) =>
        Number.isFinite(point.x) &&
        Number.isFinite(point.y) &&
        point.x >= 0,
    );
  if (points.length === 0) return "";
  const left = 42;
  const top = 10;
  const width = 544;
  const height = 145;
  const scaleX = (value: number) => left + (Math.max(0, value) / xMaximum) * width;
  const scaleY = (value: number) =>
    top +
    (1 - (Math.max(yMinimum, Math.min(yMaximum, value)) - yMinimum) /
      Math.max(1, yMaximum - yMinimum)) *
      height;
  return points
    .map(
      (point, index) =>
        `${index === 0 ? "M" : "L"} ${scaleX(point.x).toFixed(2)} ${scaleY(point.y).toFixed(2)}`,
    )
    .join(" ");
};

const renderPerformanceChart = (
  title: string,
  series: ChartSeries[],
  workingPoints: Array<{ x: number; y: number; className: string }>,
  xMaximum: number,
  yMinimum: number,
  yMaximum: number,
): string => {
  const left = 42;
  const top = 10;
  const width = 544;
  const height = 145;
  const scaleX = (value: number) => left + (Math.max(0, value) / xMaximum) * width;
  const scaleY = (value: number) =>
    top +
    (1 - (Math.max(yMinimum, Math.min(yMaximum, value)) - yMinimum) /
      Math.max(1, yMaximum - yMinimum)) *
      height;
  const ticks = [0, 0.25, 0.5, 0.75, 1];
  return `
    <figure class="performance-chart">
      <figcaption>
        <strong>${escapeHtml(title)}</strong>
        <span>${series.map((item) => `<i class="${item.className}"></i>${escapeHtml(item.label)}`).join("")}</span>
      </figcaption>
      <svg viewBox="0 0 600 190" role="img" aria-label="${escapeHtml(title)}">
        ${ticks
          .map((tick) => {
            const x = left + width * tick;
            const y = top + height * (1 - tick);
            return `
              <line class="performance-grid" x1="${x}" y1="${top}" x2="${x}" y2="${top + height}"></line>
              <line class="performance-grid" x1="${left}" y1="${y}" x2="${left + width}" y2="${y}"></line>
              <text class="performance-tick" x="${x}" y="174" text-anchor="middle">${escapeHtml(formatNumber(xMaximum * tick, 0))}</text>
              <text class="performance-tick" x="35" y="${y + 3}" text-anchor="end">${escapeHtml(formatNumber(yMinimum + (yMaximum - yMinimum) * tick, 0))}</text>`;
          })
          .join("")}
        <line class="performance-axis" x1="${left}" y1="${top}" x2="${left}" y2="${top + height}"></line>
        <line class="performance-axis" x1="${left}" y1="${top + height}" x2="${left + width}" y2="${top + height}"></line>
        ${series
          .map((item) => {
            const path = chartPath(item, xMaximum, yMinimum, yMaximum);
            return path
              ? `<path class="performance-line ${item.className}" d="${path}"></path>`
              : "";
          })
          .join("")}
        ${workingPoints
          .filter(
            (point) =>
              Number.isFinite(point.x) &&
              Number.isFinite(point.y) &&
              point.x >= 0,
          )
          .map(
            (point) =>
              `<circle class="performance-point ${point.className}" cx="${scaleX(point.x)}" cy="${scaleY(point.y)}" r="4.5"></circle>`,
          )
          .join("")}
        <text class="performance-axis-title" x="${left + width / 2}" y="188" text-anchor="middle">${escapeHtml(messages().ui.preselection.supplyAirflow)} [m³/h]</text>
      </svg>
    </figure>`;
};

const renderPerformanceStrip = (): string => {
  if (!draft || !result || !selectedUnit()) return "";
  const winter: PerformanceCurveData | undefined = result.winterCurve;
  if (!winter || winter.regulatedAirflows.length === 0) return "";
  const summer = result.summerCurve;
  const airflowValues = [
    ...winter.originalAirflows,
    ...winter.regulatedAirflows,
    ...(summer?.regulatedAirflows ?? []),
    winter.workingPointAirflow,
    summer?.workingPointAirflow ?? 0,
  ];
  const xMaximum = roundedAxisMaximum(finiteMaximum(airflowValues, 100));
  const pressureMaximum = roundedAxisMaximum(
    finiteMaximum(
      [...winter.originalPressures, ...winter.regulatedPressures],
      winter.workingPointPressurePa,
    ),
  );
  const powerMaximum = roundedAxisMaximum(
    finiteMaximum(
      [...winter.originalPowers, ...winter.regulatedPowers],
      winter.workingPointPowerW,
    ),
  );
  const regulationLabel = `${formatNumber(draft.regulationPercent, 0)}%`;
  const text = messages();
  return `<section class="performance-strip">
    ${renderPerformanceChart(
      `${text.ui.context.pressure} [Pa]`,
      [
        { x: winter.originalAirflows, y: winter.originalPressures, className: "original", label: "100%" },
        { x: winter.regulatedAirflows, y: winter.regulatedPressures, className: "regulated", label: regulationLabel },
      ],
      [{ x: winter.workingPointAirflow, y: winter.workingPointPressurePa, className: "winter" }],
      xMaximum,
      0,
      pressureMaximum,
    )}
    ${renderPerformanceChart(
      `${text.ui.context.power} [W]`,
      [
        { x: winter.regulatedAirflows, y: winter.regulatedPowers, className: "regulated", label: regulationLabel },
      ],
      [{ x: winter.workingPointAirflow, y: winter.workingPointPowerW, className: "winter" }],
      xMaximum,
      0,
      powerMaximum,
    )}
    ${renderPerformanceChart(
      `${text.ui.context.efficiency} - ${text.ui.preselection.winter} [%]`,
      [
        { x: winter.originalAirflows, y: winter.efficienciesPercent, className: "winter", label: text.ui.preselection.winter },
      ],
      [
        { x: winter.workingPointAirflow, y: winter.workingPointEfficiencyPercent, className: "winter" },
      ],
      xMaximum,
      60,
      100,
    )}
    ${draft.summerEnabled && summer
      ? renderPerformanceChart(
          `${text.ui.context.efficiency} - ${text.ui.preselection.summer} [%]`,
          [
            { x: summer.originalAirflows, y: summer.efficienciesPercent, className: "summer", label: text.ui.preselection.summer },
          ],
          [
            { x: summer.workingPointAirflow, y: summer.workingPointEfficiencyPercent, className: "summer" },
          ],
          xMaximum,
          60,
          100,
        )
      : ""}
  </section>`;
};

const renderCo2Chart = (
  points: Array<{ hours: number; ppm: number }>,
): string => {
  const finitePoints = points.filter(
    (point) =>
      Number.isFinite(point.hours) &&
      Number.isFinite(point.ppm) &&
      point.hours >= 0 &&
      point.hours <= 5 &&
      point.ppm >= 0,
  );
  if (finitePoints.length === 0) return "";

  const left = 62;
  const top = 18;
  const width = 510;
  const height = 190;
  const xMaximum = 5;
  const pointMaximum = finiteMaximum(
    finitePoints.map((point) => point.ppm),
    2000,
  );
  const yMaximum = Math.max(2000, Math.ceil(pointMaximum / 100) * 100);
  const scaleX = (value: number) => left + (value / xMaximum) * width;
  const scaleY = (value: number) =>
    top + (1 - Math.min(yMaximum, value) / yMaximum) * height;
  const path = finitePoints
    .map(
      (point, index) =>
        `${index === 0 ? "M" : "L"} ${scaleX(point.hours).toFixed(2)} ${scaleY(point.ppm).toFixed(2)}`,
    )
    .join(" ");
  const xTicks = [0, 1, 2, 3, 4, 5];
  const yTicks = [0, 0.25, 0.5, 0.75, 1];

  return `<figure class="co2-chart">
    <figcaption><strong>CO₂ [ppm]</strong><span>t [h]</span></figcaption>
    <svg viewBox="0 0 600 250" role="img" aria-label="CO₂ [ppm]">
      ${xTicks
        .map((tick) => {
          const x = scaleX(tick);
          return `<line class="co2-chart-grid" x1="${x}" y1="${top}" x2="${x}" y2="${top + height}"></line>
            <text class="co2-chart-tick" x="${x}" y="226" text-anchor="middle">${tick}</text>`;
        })
        .join("")}
      ${yTicks
        .map((tick) => {
          const y = top + height * (1 - tick);
          return `<line class="co2-chart-grid" x1="${left}" y1="${y}" x2="${left + width}" y2="${y}"></line>
            <text class="co2-chart-tick" x="54" y="${y + 4}" text-anchor="end">${escapeHtml(formatNumber(yMaximum * tick, 0))}</text>`;
        })
        .join("")}
      <line class="co2-chart-axis" x1="${left}" y1="${top}" x2="${left}" y2="${top + height}"></line>
      <line class="co2-chart-axis" x1="${left}" y1="${top + height}" x2="${left + width}" y2="${top + height}"></line>
      <path class="co2-chart-line" d="${path}"></path>
      <text class="co2-chart-axis-title" x="${left + width / 2}" y="246" text-anchor="middle">t [h]</text>
      <text class="co2-chart-axis-title" transform="translate(15 ${top + height / 2}) rotate(-90)" text-anchor="middle">CO₂ [ppm]</text>
    </svg>
  </figure>`;
};

const renderLoading = (): void => {
  app.innerHTML = `
    <main class="loading-screen" aria-live="polite">
      <div class="brand-mark" aria-hidden="true">
        <span></span><span></span><span></span>
      </div>
      <div class="loading-copy">
        <strong>SSW Next</strong>
        <div class="loading-languages">
          <span lang="en">Please wait a moment...</span>
          <span lang="fr">Veuillez patienter un instant...</span>
          <span lang="de">Bitte warten Sie einen Moment...</span>
          <span lang="it">Pazientare un attimo...</span>
        </div>
      </div>
      <div class="loading-line"><span></span></div>
    </main>
  `;
};

const renderBusyOverlay = (message: string): string => `
  <div class="busy-overlay" role="status" aria-live="assertive" aria-busy="true">
    <div class="busy-dialog">
      <span class="busy-icon">${icon("loader-circle", 34)}</span>
      <strong>${escapeHtml(message)}</strong>
      <span>${escapeHtml(messages().status.calculating)}</span>
      <div class="loading-line"><span></span></div>
    </div>
  </div>`;

const renderStartupError = (error: unknown): void => {
  const detail = error instanceof Error ? error.message : String(error);
  app.innerHTML = `
    <main class="loading-screen startup-error" role="alert">
      <div class="brand-mark" aria-hidden="true">
        <span></span><span></span><span></span>
      </div>
      <div class="loading-copy">
        <strong>SSW Next</strong>
        <span>Impossibile completare l'inizializzazione dell'interfaccia.</span>
      </div>
      <p class="startup-error-detail">${escapeHtml(detail)}</p>
      <div class="startup-error-actions">
        <button class="button button-primary" id="startup-retry" type="button">Riprova</button>
      </div>
    </main>
  `;
  document.querySelector<HTMLButtonElement>("#startup-retry")?.addEventListener(
    "click",
    () => void bootstrap(),
  );
};

const renderShell = (): void => {
  if (!data || !draft || !result) {
    return;
  }
  visitedSteps.add(currentStep);
  skippedOptionalSteps.delete(currentStep);

  const route = window.location.hash || "#/selection";
  if (route.startsWith("#/showcase")) {
    renderShowcase();
    return;
  }

  const unit = selectedUnit();
  const text = messages();
  const workflow = workflowText();
  const help = helpContent();
  const activeStepMessage = stepMessage(currentStep);
  const activeHelpTopic = currentHelpTopic();
  const currentStepIndex = stepIndex(currentStep);
  const followingStep = steps[Math.min(steps.length - 1, currentStepIndex + 1)];
  const canGoForward = !calculating && !calculationFailed && canNavigateToStep(followingStep.id);
  const winterCoil = result.waterCoilResults?.find((item) => item.mode === "HWD");
  const summerCoil = result.waterCoilResults?.find((item) => item.mode === "CWD");
  const winterPostheater = result.electricHeaterResults?.find((item) => item.mode === "EHD");
  const projectReference = projectState?.publicReference || projectState?.localReference || workflow.notSaved;
  const projectRevision = projectState?.revision ? `R${String(projectState.revision).padStart(2, "0")}` : "-";
  const projectSavedAt = projectDirty
    ? workflow.modified
    : projectState?.savedAt
      ? new Date(projectState.savedAt).toLocaleString(messages().locale)
      : workflow.notSaved;
  app.innerHTML = `
    <div class="app-shell">
      ${busyMessage ? renderBusyOverlay(busyMessage) : ""}
      <header class="topbar">
        <a class="brand" href="#/selection" aria-label="SSW Next">
          <span class="brand-mark small" aria-hidden="true">
            <span></span><span></span><span></span>
          </span>
          <span><strong>SSW</strong><small>${escapeHtml(text.common.technicalSelection)}</small></span>
        </a>
        <div class="topbar-project">
          <span>${escapeHtml(text.common.activeProject)}</span>
          <strong>${escapeHtml(draft.project.customerReference.trim() || draft.project.name)}</strong>
        </div>
        <div class="topbar-actions">
          <button class="icon-button" type="button" data-action="notifications" title="${escapeHtml(helpTitle("notifications"))}" aria-label="${escapeHtml(text.ui.aria.notifications)}">
            ${icon("bell")}
            ${notificationState.unreadDueCount > 0 ? `<span class="notification-dot"></span>` : ""}
          </button>
          <button class="release-button" type="button" data-action="release-info" title="${escapeHtml(releaseLabels().title)}" aria-label="${escapeHtml(releaseLabels().title)}">
            ${icon("info", 15)}<span>${escapeHtml(releaseInfo.version)}</span>
          </button>
          <button class="icon-button" type="button" title="${escapeHtml(helpTitle("help"))}" aria-label="${escapeHtml(text.actions.openHelp)}" data-action="help">
            ${icon("circle-help")}
          </button>
        </div>
      </header>

      <div class="workspace${currentStep === "project" ? " workspace-project" : ""}">
        <aside class="step-rail" aria-label="${escapeHtml(text.ui.aria.selectionSteps)}">
          <div class="step-rail-heading">
            <span>${escapeHtml(text.common.configuration)}</span>
            <strong>${steps.findIndex((step) => step.id === currentStep) + 1} / ${steps.length}</strong>
          </div>
          <nav>
            ${steps
              .map((step, index) => {
                const currentIndex = stepIndex(currentStep);
                const enabled = canNavigateToStep(step.id);
                const status =
                  step.id === currentStep
                    ? "active"
                    : index < currentIndex
                      ? "complete"
                      : "";
                const attention =
                  step.id === "installation" && installationReviewRequired
                    ? "attention"
                    : "";
                const skipped =
                  step.optional === true &&
                  skippedOptionalSteps.has(step.id)
                    ? "skipped"
                    : "";
                return `
                  <button class="step-button ${status} ${attention} ${skipped}" data-step="${step.id}" type="button" ${enabled ? "" : "disabled aria-disabled=\"true\""}>
                    <span class="step-index">${
                      status === "complete" && !skipped ? icon("check", 14) : index + 1
                    }</span>
                    <span>
                      <strong>${escapeHtml(stepMessage(step.id).shortTitle)}</strong>
                      ${step.optional ? `<small>${escapeHtml(text.common.optional)}</small>` : ""}
                    </span>
                  </button>`;
              })
              .join("")}
          </nav>
        </aside>

        <main class="main-stage">
          <div class="page-heading">
            <div>
              <span class="eyebrow">${escapeHtml(text.common.technicalSelection)}</span>
              <h1>${escapeHtml(activeStepMessage.title)}</h1>
              <p>${escapeHtml(activeStepMessage.description)}</p>
            </div>
            ${steps.findIndex((step) => step.id === currentStep) >= 1 && steps.findIndex((step) => step.id === currentStep) <= 8 && selectedUnit()?.model ? `<strong class="page-selected-unit">${escapeHtml(selectedUnit()!.model.toUpperCase())}</strong>` : ""}
            <div class="calculation-state ${stateTone()}">
              <span>${icon(result.status === "valid" ? "circle-check" : "triangle-alert")}</span>
              <div>
                <small>${escapeHtml(text.common.technicalSelection)}</small>
                <strong>${calculating ? escapeHtml(text.status.calculating) : statusLabel(calculationFailed ? "invalid" : result.status)}</strong>
              </div>
            </div>
          </div>

          <section class="step-content" aria-live="polite">
            ${renderStep(currentStep)}
          </section>

          ${
            currentStep !== "project" && currentStep !== "preselection"
              ? renderPerformanceStrip()
              : ""
          }

          <footer class="step-footer">
            <button class="button secondary" data-action="previous" type="button" ${
              currentStep === "project" ? "disabled" : ""
            }>
              ${icon("arrow-left")} ${escapeHtml(text.actions.back)}
            </button>
            <div class="footer-meta">
              <span>${calculating ? `${icon("loader-circle")} ${escapeHtml(text.status.calculating)}` : escapeHtml(text.status.ready)}</span>
            </div>
            ${
              currentStep === "summary"
                ? `<button class="button primary" data-action="report" type="button">
                    ${icon("file-down")} ${escapeHtml(text.actions.generateReport)}
                  </button>`
                : `<button class="button primary" data-action="next" type="button" ${canGoForward ? "" : "disabled"}>
                    ${escapeHtml(text.actions.next)} ${icon("arrow-right")}
                  </button>`
            }
          </footer>
        </main>

        ${currentStep !== "project" ? `<aside class="context-panel">
          ${currentStep === "preselection" ? `
            <section class="context-project-status">
              <div class="context-heading">
                <span>${escapeHtml(text.ui.project.statusTitle)}</span>
                <span class="context-status-badge">${escapeHtml(projectState?.publicReference ? text.ui.project.registered : text.ui.project.local)}</span>
              </div>
              <div class="project-status-list">
                ${statusRow(text.ui.project.technicalReference, projectReference, projectState?.publicReference ? text.ui.project.registered : text.ui.project.local)}
                ${statusRow(text.ui.project.revision, projectRevision, text.ui.project.current)}
                ${statusRow(text.ui.project.lastSaved, projectSavedAt, projectState?.fileName || text.ui.project.local)}
              </div>
            </section>` : ""}
          <div class="context-heading">
            <span>${escapeHtml(text.ui.navigation.currentSelection)}</span>
            <button class="icon-button subtle" type="button" title="${escapeHtml(text.ui.navigation.saveDraft)}" data-action="save">
              ${icon("save")}
            </button>
          </div>
          <div class="unit-lockup">
            <div>
              <strong>${escapeHtml(unit?.model ?? text.ui.context.noUnit)}</strong>
            </div>
          </div>
          ${renderKeyValues([
            [text.ui.context.supply, `${formatNumber(draft.operatingPoint.supplyAirflow, 0)} m³/h`],
            [text.ui.context.extract, `${formatNumber(draft.operatingPoint.extractAirflow, 0)} m³/h`],
            [text.ui.context.pressure, `${formatNumber(draft.operatingPoint.pressure, 0)} Pa`],
            [text.ui.context.layout, draft.layoutCode],
            [`${text.ui.preselection.winter} · ${text.ui.preselection.supplyAirTemperature} (HX)`, `${formatNumber(result.supplyTemperature, 1)} °C`],
            ...(draft.waterCoilEnabled && winterCoil
              ? [[`${text.ui.preselection.winter} · ${text.ui.preselection.supplyAirTemperature} (${draft.waterCoilMode})`, `${formatNumber(winterCoil.airOutletTemperatureC, 1)} °C`] as [string, string]]
              : []),
            ...(draft.electricPostheaterEnabled && winterPostheater
              ? [[`${text.ui.preselection.winter} · ${text.ui.preselection.supplyAirTemperature} (EHD)`, `${formatNumber(winterPostheater.airOutletTemperatureC, 1)} °C`] as [string, string]]
              : []),
            ...(draft.summerEnabled
              ? [[`${text.ui.preselection.summer} · ${text.ui.preselection.supplyAirTemperature} (HX)`, `${formatNumber(result.summerSupplyTemperature, 1)} °C`] as [string, string]]
              : []),
            ...(draft.summerEnabled && draft.waterCoilEnabled && summerCoil
              ? [[`${text.ui.preselection.summer} · ${text.ui.preselection.supplyAirTemperature} (${draft.waterCoilMode})`, `${formatNumber(summerCoil.airOutletTemperatureC, 1)} °C`] as [string, string]]
              : []),
          ])}
          <div class="context-divider"></div>
          <div class="metric-grid">
            ${metric(text.ui.context.efficiency, `${formatNumber(result.winterEfficiency)}%`, "trending-up")}
            ${metric(text.ui.context.margin, `${formatNumber(result.availablePressure, 0)} Pa`, "gauge")}
            ${metric(text.ui.context.power, `${formatNumber(result.absorbedPower, 0)} W`, "zap")}
            ${metric("SFP", `${formatNumber(result.sfp, 2)}`, "activity")}
          </div>
          <div class="selection-tags">
            <span>${escapeHtml(text.ui.context.configuration)}</span>
            <div>
              ${draft.waterCoilEnabled ? `<b>${draft.waterCoilMode}</b>` : ""}
              ${draft.electricPreheaterEnabled ? "<b>PEHD</b>" : ""}
              ${draft.electricPostheaterEnabled ? "<b>EHD</b>" : ""}
              ${draft.accessoryCodes.map((code) => `<b>${escapeHtml(code)}</b>`).join("")}
            </div>
          </div>
          ${renderMultiProjectSidebar()}
        </aside>` : ""}
      </div>
      ${saveLanguagePromptOpen ? (() => {
        const prompt = saveLanguagePromptText();
        return `<div class="modal-backdrop" data-action="close-save-language">
          <section class="help-dialog save-language-dialog" role="dialog" aria-modal="true" aria-labelledby="save-language-title">
            <div class="panel-heading">
              <span class="panel-icon">${icon("languages")}</span>
              <div><h2 id="save-language-title">${escapeHtml(prompt.title)}</h2><p>${escapeHtml(prompt.message)}</p></div>
              <button class="icon-button bordered" type="button" data-action="close-save-language" aria-label="${escapeHtml(prompt.cancel)}">${icon("circle-x")}</button>
            </div>
            <label class="field"><span>${escapeHtml(prompt.useLanguage)}</span>
              <select data-save-language>
                ${languageOptions.map((option) => `<option value="${option.code}" ${option.code === saveLanguageChoice ? "selected" : ""}>${escapeHtml(option.name)}</option>`).join("")}
              </select>
            </label>
            <label class="toggle"><input type="checkbox" data-save-language-remember ${saveLanguageRemember ? "checked" : ""}/><span></span><b>${escapeHtml(prompt.remember)}</b></label>
            <footer class="dialog-actions"><button class="button secondary" type="button" data-action="close-save-language">${escapeHtml(prompt.cancel)}</button><button class="button primary" type="button" data-action="confirm-save-language">${icon("save")} ${escapeHtml(prompt.save)}</button></footer>
          </section>
        </div>`;
      })() : ""}
      ${toastMessage ? `<div class="toast">${icon("circle-check")} ${escapeHtml(toastMessage)}</div>` : ""}
      ${helpOpen ? `
        <div class="modal-backdrop" data-action="close-help">
          <section class="help-dialog" role="dialog" aria-modal="true" aria-labelledby="help-title">
            <div class="panel-heading">
              <span class="panel-icon">${icon("circle-help")}</span>
              <div><h2 id="help-title">${escapeHtml(help.title)}</h2><p>${escapeHtml(help.introduction)}</p></div>
              <button class="icon-button bordered" data-action="close-help" aria-label="${escapeHtml(text.actions.close)}">${icon("circle-x")}</button>
            </div>
            <div class="help-topic">
              <h3>${escapeHtml(activeHelpTopic.title)}</h3>
              <p>${escapeHtml(activeHelpTopic.summary)}</p>
              <div class="inline-notice">${icon("info")}<span>${escapeHtml(activeHelpTopic.tip)}</span></div>
            </div>
            <p class="help-workflow">${escapeHtml(help.workflowNote)}</p>
            <label class="toggle"><input type="checkbox" data-action="toggle-tooltips" ${tooltipsEnabled ? "checked" : ""}/><span></span><b>${escapeHtml(tooltipsEnabled ? text.actions.hideTooltips : text.actions.showTooltips)}</b></label>
          </section>
        </div>` : ""}
      ${releaseInfoOpen ? `
        <div class="modal-backdrop" data-action="close-release-info">
          <section class="release-dialog" role="dialog" aria-modal="true" aria-labelledby="release-info-title">
            <div class="panel-heading">
              <span class="panel-icon">${icon("info")}</span>
              <div><h2 id="release-info-title">${escapeHtml(releaseLabels().title)}</h2></div>
              <button class="icon-button bordered" data-action="close-release-info" aria-label="${escapeHtml(releaseLabels().close)}">${icon("circle-x")}</button>
            </div>
            <dl class="release-details">
              <div><dt>${escapeHtml(releaseLabels().version)}</dt><dd>${escapeHtml(releaseInfo.version)}</dd></div>
              <div><dt>${escapeHtml(releaseLabels().builtAt)}</dt><dd>${escapeHtml(new Date(releaseInfo.builtAt).toLocaleString(languageCode()))}</dd></div>
            </dl>
          </section>
        </div>` : ""}
      ${renderNotificationCenter()}
      ${renderProjectEmailDialog()}
      ${renderDimensionalDrawing()}
    </div>
  `;
  bindShellEvents();
  renderIcons();
  if (currentStep === "installation") void ensureDimensionalDrawing();
  void renderDimensionalCanvases();
};

const statusLabel = (status: SelectionResult["status"]): string =>
  status === "valid"
    ? messages().status.valid
    : status === "warning"
      ? messages().status.warning
      : messages().status.invalid;

const stepMessage = (step: StepId): {
  title: string;
  shortTitle: string;
  description: string;
} => {
  const text = messages();
  if (step === "co2") {
    return {
      title: text.ui.co2Sound.co2Title,
      shortTitle: "CO₂",
      description: text.ui.co2Sound.co2Description,
    };
  }
  if (step === "sound") {
    return {
      title: text.ui.co2Sound.soundTitle,
      shortTitle: text.ui.co2Sound.soundTitle,
      description: text.ui.co2Sound.soundDescription,
    };
  }
  return text.steps[step as keyof typeof text.steps];
};

const currentHelpTopic = (): {
  title: string;
  summary: string;
  tip: string;
} => {
  if (currentStep === "co2") {
    const copy = messages().ui.co2Sound;
    return {
      title: copy.co2Title,
      summary: copy.co2Description,
      tip: copy.awaitingBackendResults,
    };
  }
  if (currentStep === "sound") {
    const copy = messages().ui.co2Sound;
    return {
      title: copy.soundTitle,
      summary: copy.soundDescription,
      tip: copy.awaitingBackendResults,
    };
  }
  return helpContent().topics[
    currentStep as keyof ReturnType<typeof helpContent>["topics"]
  ];
};

const renderStep = (step: StepId): string => {
  switch (step) {
    case "project":
      return renderProjectStep();
    case "preselection":
      return renderPreselectionStep();
    case "installation":
      return renderInstallationStep();
    case "water-coil":
      return renderWaterCoilStep();
    case "electric-heaters":
      return renderElectricStep();
    case "accessories":
      return renderAccessoriesStep();
    case "co2":
      return renderCo2Step();
    case "sound":
      return renderSoundStep();
    case "documents":
      return renderDocumentsStep();
    case "summary":
      return renderSummaryStep();
  }
};

const renderProjectStep = (): string => {
  const text = messages();
  const copy = workflowText();
  const documentLanguage = pendingProjectLanguage || projectDocumentLanguage();
  return `<div class="content-grid">
    <section class="panel">
      <div class="panel-heading">
        <span class="panel-icon">${icon("briefcase-business")}</span>
        <div><h2>${escapeHtml(text.ui.project.dataTitle)}</h2><p>${escapeHtml(text.ui.project.dataDescription)}</p></div>
      </div>
      <div class="form-grid">
        ${textField(text.ui.project.name, "project.name", draft!.project.name, text.ui.project.defaultName)}
        ${textField(text.ui.project.customerReference, "project.customerReference", draft!.project.customerReference, text.ui.project.referencePlaceholder)}
        <div class="field project-language-field">
          <label>${escapeHtml(interfaceLanguageLabel())}</label>
          <select data-field="project.language">
            ${languageOptions.map((option) => `<option value="${option.code}" ${option.code === languageCode() ? "selected" : ""}>${escapeHtml(option.name)}</option>`).join("")}
          </select>
        </div>
        <div class="field project-language-field">
          <label>${escapeHtml(text.ui.project.documentLanguage)}</label>
          <select data-project-language>
            ${languageOptions.map((option) => `<option value="${option.code}" ${option.code === documentLanguage ? "selected" : ""}>${escapeHtml(option.name)}</option>`).join("")}
          </select>
        </div>
      </div>
      <div class="project-command-row">
        <button class="button secondary" type="button" data-action="open-selection">${icon("folder-check")} ${escapeHtml(copy.openSelection)}</button>
        <button class="button secondary" type="button" data-action="save-as">${icon("save")} ${escapeHtml(copy.saveAs)}</button>
      </div>
    </section>
  </div>`;
};

const renderMultiProjectSidebar = (): string => {
  const copy = multiProjectText();
  const state = multiProjectState;
  const currentItem = state?.items.find((item) => item.current);
  const currentMatchesDraft = Boolean(
    currentItem && selectedUnit()?.model.trim().toLocaleUpperCase() === currentItem.unitName.trim().toLocaleUpperCase(),
  );
  const items = state?.items.map((item) => `
    <article class="context-project-item ${item.current ? "current" : ""}" data-project-item="${escapeHtml(item.itemId)}">
      <button class="context-project-open" type="button" data-project-open="${escapeHtml(item.itemId)}" title="${escapeHtml(copy.open)}">
        <span>
          <strong>${escapeHtml(item.customerReference || item.unitName)}</strong>
          <small>${escapeHtml(item.unitName)}</small>
        </span>
        <span class="context-project-duty">
          <small>${item.airflow == null ? "-" : `${formatNumber(item.airflow, 0)} m³/h`}</small>
          <small>${item.pressure == null ? "-" : `${formatNumber(item.pressure, 0)} Pa`}</small>
        </span>
      </button>
      <button class="icon-button" type="button" data-project-remove="${escapeHtml(item.itemId)}" title="${escapeHtml(copy.remove)}">${icon("trash-2", 14)}</button>
    </article>`).join("") ?? "";
  const modified = state?.modifiedAt
    ? new Date(state.modifiedAt).toLocaleString(messages().locale)
    : "";
  return `<section class="context-project">
    <div class="context-project-heading">
      <div>
        <span>${escapeHtml(copy.workspace)}</span>
        <strong>${state?.items.length ?? 0}</strong>
      </div>
      <small title="${escapeHtml(state?.fileName || copy.unsaved)}">${escapeHtml(state?.fileName || copy.unsaved)}</small>
    </div>
    <div class="context-project-actions">
      <button class="icon-button bordered" type="button" data-action="project-new" title="${escapeHtml(copy.newProject)}">${icon("file-text", 15)}</button>
      <button class="icon-button bordered" type="button" data-action="project-open" title="${escapeHtml(copy.openProject)}">${icon("folder-check", 15)}</button>
      <button class="icon-button bordered" type="button" data-action="project-save" title="${escapeHtml(copy.saveProject)}">${icon("save", 15)}</button>
      <button class="icon-button bordered" type="button" data-action="project-save-as" title="${escapeHtml(copy.saveProjectAs)}">${icon("hard-drive-download", 15)}</button>
    </div>
    <div class="context-project-list">
      ${items || `<p class="context-project-empty">${escapeHtml(copy.empty)}</p>`}
    </div>
    <small class="context-project-state">${state?.dirty ? escapeHtml(copy.unsaved) : escapeHtml(modified)}</small>
    <button class="button primary context-project-command" type="button" data-action="project-add-current">${icon("file-down")} ${escapeHtml(projectActionLabels(languageCode())[currentMatchesDraft ? 1 : 0])}</button>
    ${currentMatchesDraft ? `<button class="button secondary context-project-command" type="button" data-action="project-add-new">${icon("plus")} ${escapeHtml(projectActionLabels(languageCode())[0])}</button>` : ""}
    <button class="button secondary context-project-command" type="button" data-action="project-email" ${state?.items.length ? "" : "disabled"}>${icon("mail")} ${escapeHtml(projectActionLabels(languageCode())[2])}</button>
  </section>`;
};

const renderPreselectionStep = (): string => {
  const text = messages();
  const filters = draft!.preselectionFilters;
  const activeCriteria = [
    filters.maximumSfpEnabled ? text.ui.preselection.maximumSfp : "",
    filters.supplyNoiseEnabled ? text.ui.preselection.supplyNoise : "",
    filters.breakoutNoiseEnabled ? text.ui.preselection.breakoutNoise : "",
    filters.rotaryOnlyEnabled ? text.ui.preselection.rotaryOnly : "",
  ].filter(Boolean);
  const noiseCriterion = (
    title: string,
    enabledField: string,
    enabled: boolean,
    metricField: string,
    metric: "LWA" | "LPA",
    maximumField: string,
    maximum: number,
    distanceField: string,
    distance: number,
    directivityField: string,
    directivity: 2 | 4 | 8,
  ): string => `<fieldset class="selection-criterion ${enabled ? "" : "criterion-disabled"}">
    <legend>${escapeHtml(title)}</legend>
    <label class="toggle criterion-toggle">
      <input type="checkbox" data-field="${enabledField}" ${enabled ? "checked" : ""}/>
      <span></span><b>${escapeHtml(text.ui.preselection.enableCriterion)}</b>
    </label>
    <div class="criterion-controls">
      <div class="field compact-field">
        <label>${escapeHtml(text.ui.preselection.soundQuantity)}</label>
        <select data-field="${metricField}" ${enabled ? "" : "disabled"}>
          <option value="LWA" ${metric === "LWA" ? "selected" : ""}>${escapeHtml(text.ui.preselection.soundPowerLevel)}</option>
          <option value="LPA" ${metric === "LPA" ? "selected" : ""}>${escapeHtml(text.ui.preselection.soundPressureLevel)}</option>
        </select>
      </div>
      ${technicalNumberField(text.ui.preselection.maximumLevel, maximumField, maximum, "dB(A)", !enabled, 0)}
      ${metric === "LPA"
        ? `${technicalNumberField(text.ui.preselection.distance, distanceField, distance, "m", !enabled, 0.1)}
          <div class="field compact-field">
            <label>${escapeHtml(text.ui.preselection.directivityFactor)}</label>
            <select data-field="${directivityField}" ${enabled ? "" : "disabled"}>
              ${[2, 4, 8].map((value) => `<option value="${value}" ${value === directivity ? "selected" : ""}>Q = ${value}</option>`).join("")}
            </select>
          </div>`
        : ""}
    </div>
  </fieldset>`;
  return `<div class="content-grid split-main">
    <section class="panel">
      <div class="panel-heading compact">
        <div><h2>${escapeHtml(text.ui.preselection.dutyPoint)}</h2><p>${escapeHtml(text.ui.preselection.dutyPointDescription)}</p></div>
      </div>
      <div class="operating-point">
        ${numberField(text.ui.preselection.supplyAirflow, "operatingPoint.supplyAirflow", draft!.operatingPoint.supplyAirflow, "m³/h")}
        ${numberField(text.ui.preselection.extractAirflow, "operatingPoint.extractAirflow", draft!.operatingPoint.extractAirflow, "m³/h")}
        ${numberField(text.ui.preselection.staticPressure, "operatingPoint.pressure", draft!.operatingPoint.pressure, "Pa")}
        <label class="toggle imbalance-toggle" title="${escapeHtml(text.ui.preselection.balancedNotice)}">
          <input type="checkbox" data-field="imbalanceEnabled" ${draft!.imbalanceEnabled ? "checked" : ""} disabled />
          <span></span><b>${escapeHtml(text.ui.preselection.imbalance)}</b>
        </label>
      </div>
      <div class="preselection-technical">
        <div class="technical-controls">
          ${technicalNumberField(text.ui.preselection.regulationPercent, "regulationPercent", draft!.regulationPercent, "%", false, 0, 100)}
          <label class="toggle seasonal-toggle">
            <input type="checkbox" data-field="summerEnabled" ${draft!.summerEnabled ? "checked" : ""}/>
            <span></span><b>${escapeHtml(text.ui.preselection.summerEnabled)}</b>
          </label>
        </div>
        <h3>${escapeHtml(text.ui.preselection.seasonalConditions)}</h3>
        <div class="season-grid">
          <fieldset class="season-card">
            <legend>${escapeHtml(text.ui.preselection.winter)}</legend>
            <div class="season-fields">
              ${technicalNumberField(text.ui.preselection.outdoorTemperature, "winterOutdoorTemperature", draft!.winterOutdoorTemperature, "°C")}
              ${technicalNumberField(text.ui.preselection.outdoorRelativeHumidity, "winterOutdoorRh", draft!.winterOutdoorRh, "%", false, 0, 100)}
              ${technicalNumberField(text.ui.preselection.returnTemperature, "winterReturnTemperature", draft!.winterReturnTemperature, "°C")}
              ${technicalNumberField(text.ui.preselection.returnRelativeHumidity, "winterReturnRh", draft!.winterReturnRh, "%", false, 0, 100)}
            </div>
          </fieldset>
          <fieldset class="season-card ${draft!.summerEnabled ? "" : "disabled-section"}">
            <legend>${escapeHtml(text.ui.preselection.summer)}</legend>
            <div class="season-fields">
              ${technicalNumberField(text.ui.preselection.outdoorTemperature, "summerOutdoorTemperature", draft!.summerOutdoorTemperature, "°C", !draft!.summerEnabled)}
              ${technicalNumberField(text.ui.preselection.outdoorRelativeHumidity, "summerOutdoorRh", draft!.summerOutdoorRh, "%", !draft!.summerEnabled, 0, 100)}
              ${technicalNumberField(text.ui.preselection.returnTemperature, "summerReturnTemperature", draft!.summerReturnTemperature, "°C", !draft!.summerEnabled)}
              ${technicalNumberField(text.ui.preselection.returnRelativeHumidity, "summerReturnRh", draft!.summerReturnRh, "%", !draft!.summerEnabled, 0, 100)}
            </div>
          </fieldset>
        </div>
      </div>
      <details class="additional-selection ${activeCriteria.length ? "has-active-criteria" : ""}" ${additionalCriteriaOpen ? "open" : ""}>
        <summary>
          <div><h3>${escapeHtml(text.ui.preselection.additionalCriteria)}</h3><p>${escapeHtml(text.ui.preselection.additionalCriteriaDescription)}</p></div>
          <span class="criteria-state"><b>${activeCriteria.length}/4</b>${activeCriteria.length ? escapeHtml(activeCriteria.join(" · ")) : escapeHtml(text.ui.summary.notSelectedMasculine)}</span>
        </summary>
        <div class="additional-selection-fields">
        <fieldset class="selection-criterion sfp-criterion ${filters.maximumSfpEnabled ? "" : "criterion-disabled"}">
          <legend>${escapeHtml(text.ui.preselection.maximumSfp)}</legend>
          <label class="toggle criterion-toggle">
            <input type="checkbox" data-field="preselectionFilters.maximumSfpEnabled" ${filters.maximumSfpEnabled ? "checked" : ""}/>
            <span></span><b>${escapeHtml(text.ui.preselection.enableCriterion)}</b>
          </label>
          ${technicalNumberField(text.ui.preselection.maximumSfp, "preselectionFilters.maximumSfp", filters.maximumSfp, "kW/(m³/s)", !filters.maximumSfpEnabled, 0)}
        </fieldset>
        ${noiseCriterion(text.ui.preselection.supplyNoise, "preselectionFilters.supplyNoiseEnabled", filters.supplyNoiseEnabled, "preselectionFilters.supplyNoiseMetric", filters.supplyNoiseMetric, "preselectionFilters.maximumSupplyNoiseDbA", filters.maximumSupplyNoiseDbA, "preselectionFilters.supplyNoiseDistanceMeters", filters.supplyNoiseDistanceMeters, "preselectionFilters.supplyNoiseDirectivityFactor", filters.supplyNoiseDirectivityFactor)}
        ${noiseCriterion(text.ui.preselection.breakoutNoise, "preselectionFilters.breakoutNoiseEnabled", filters.breakoutNoiseEnabled, "preselectionFilters.breakoutNoiseMetric", filters.breakoutNoiseMetric, "preselectionFilters.maximumBreakoutNoiseDbA", filters.maximumBreakoutNoiseDbA, "preselectionFilters.breakoutNoiseDistanceMeters", filters.breakoutNoiseDistanceMeters, "preselectionFilters.breakoutNoiseDirectivityFactor", filters.breakoutNoiseDirectivityFactor)}
        <fieldset class="selection-criterion rotary-criterion ${filters.rotaryOnlyEnabled ? "" : "criterion-disabled"}">
          <legend>${escapeHtml(text.ui.preselection.rotaryOnly)}</legend>
          <label class="toggle criterion-toggle">
            <input type="checkbox" data-field="preselectionFilters.rotaryOnlyEnabled" ${filters.rotaryOnlyEnabled ? "checked" : ""}/>
            <span></span><b>${escapeHtml(text.ui.preselection.enableCriterion)}</b>
          </label>
          <p class="criterion-description">${escapeHtml(text.ui.preselection.rotaryOnlyDescription)}</p>
        </fieldset>
        </div>
      </details>
      <div class="inline-notice">
        ${icon("info")}
        <span>${escapeHtml(text.ui.preselection.balancedNotice)}</span>
      </div>
    </section>
    <section class="panel">
      <div class="panel-heading compact">
        <div><h2>${escapeHtml(text.ui.preselection.results)}</h2><p>${data!.units.length} ${escapeHtml(text.ui.preselection.compatibleUnits)}, ${escapeHtml(text.ui.preselection.orderedByFit)}.</p></div>
        <span class="score-badge">${escapeHtml(text.ui.preselection.technicalFit)}</span>
      </div>
      <div class="ranked-list">
        ${data!.units.length > 0
          ? data!.units.map((unit, index) => unitCard(unit, index === 0)).join("")
          : `<div class="empty-state">${escapeHtml(text.status.unavailable)}</div>`}
      </div>
    </section>
  </div>`;
};

const renderFlowPorts = (): string => {
  const fallback = [
    { flowCode: "Fresh" as const, position: 1 },
    { flowCode: "Return" as const, position: 2 },
    { flowCode: "Exhaust" as const, position: 3 },
    { flowCode: "Supply" as const, position: 4 },
  ];
  const candidatePorts = result!.flowPorts ?? [];
  const portsAreComplete =
    candidatePorts.length === 4 &&
    new Set(candidatePorts.map((port) => port.position)).size === 4 &&
    new Set(candidatePorts.map((port) => port.flowCode)).size === 4 &&
    candidatePorts.every(
      (port) =>
        port.position >= 1 &&
        port.position <= 4 &&
        fallback.some((fallbackPort) => fallbackPort.flowCode === port.flowCode),
    );
  if (!portsAreComplete) return "";
  const ports = [...candidatePorts];
  const sameSide = isSameSideConnection();
  const sameSideFloorFacing = isSameSideFlatFloor();
  const splitPortFour = isFsVsHciModel() && !sameSide;
  const stConnection = isStConnection();
  const oppositeSideEastWest = !sameSide &&
    !(draft?.installationMode === "wall" &&
      selectedLayoutConfiguration()?.referenceView === "OSC_NORTH_SOUTH");

  return ports
    .sort((left, right) => left.position - right.position)
    .map((port) => {
      const role = port.flowCode.toLowerCase();
      const incoming = port.flowCode === "Fresh" || port.flowCode === "Return";
      if (stConnection) {
        const placement = stFlowPlacement(draft?.layoutCode ?? "", port.position);
        return `<g data-port="${port.position}" data-flow="${role}" data-rear="${placement.rear}" class="${incoming ? "incoming" : "outgoing"}">${flowCircle(role, placement.x, placement.y, port.position, placement.rear)}</g>`;
      }
      const side = airflowSide(port.position, sameSide, sameSideFloorFacing, oppositeSideEastWest);
      const slot = airflowSlot(port.position, sameSide, oppositeSideEastWest);
      let x = sameSide ? 55 + (slot - 1) * 63 : oppositeSideEastWest ? (side === "west" ? 30 : 270) : (slot === 1 || slot === 3 ? 115 : 185);
      let y = sameSide ? 30 : oppositeSideEastWest ? (slot === 1 || slot === 3 ? 65 : 135) : (side === "north" ? 30 : 270);
      if (splitPortFour && port.position === 3) {
        x = 270;
        y = oppositeSideEastWest ? 100 : 150;
      }
      if (splitPortFour && port.position === 4) {
        const top = 30;
        const bottom = oppositeSideEastWest ? 170 : 270;
        const splitX = oppositeSideEastWest ? 235 : 185;
        return `<g data-port="${port.position}" data-flow="${role}" data-duplicate="true" class="${incoming ? "incoming" : "outgoing"}">${flowCircle(role, splitX, top, port.position)}${flowCircle(role, splitX, bottom, port.position)}</g>`;
      }
      return `<g data-port="${port.position}" data-flow="${role}" class="${incoming ? "incoming" : "outgoing"}">${flowCircle(role, x, y, port.position)}</g>`;
    })
    .join("");
};

const renderFlowLegend = (surface: AccessSurface): string => {
  const labels = messages().domain.airflow;
  const legend = schematicText(languageCode());
  return `<aside class="schematic-legend" aria-label="${escapeHtml(messages().ui.installation.orientationTitle)}">
    ${([
      ["fresh", labels.fresh],
      ["supply", labels.supply],
      ["return", labels.return],
      ["exhaust", labels.exhaust],
    ] as const).map(([role, label]) => `<div class="airflow-legend-item ${role}">
      <svg viewBox="0 0 48 48" aria-hidden="true">${flowCircle(role, 24, 24)}</svg><span>${escapeHtml(label)}</span>
    </div>`).join("")}
    <div class="schematic-legend-key"><i class="circle-key hollow"></i><span>${escapeHtml(legend[1])}</span></div>
    <div class="schematic-legend-key"><i class="circle-key solid"></i><span>${escapeHtml(legend[2])}</span></div>
    <div class="schematic-legend-key"><svg viewBox="0 0 48 48" aria-hidden="true"><path d="M24 5v35m-8-10 8 10 8-10" fill="none" stroke="#D62828" stroke-width="3"/></svg><span>${escapeHtml(legend[3])}</span></div>
    <span class="visually-hidden">${escapeHtml(surface === "front" ? observerAccessLabel() : accessLabel())}</span>
  </aside>`;
};

const layoutConfigurations = (): LayoutConfigurationOption[] => {
  if (result?.layoutConfigurations) return result.layoutConfigurations;
  return (result?.layoutCodes ?? [])
    .map((code) => ({ code, isDefault: false }));
};

const layoutsForInstallation = (mode: InstallationMode): LayoutConfigurationOption[] =>
  layoutConfigurations().filter(
    (configuration) =>
      !configuration.installationMode || configuration.installationMode === mode,
  );

const installationAvailable = (mode: InstallationMode): boolean =>
  layoutsForInstallation(mode).length > 0;

const preferredLayout = (mode: InstallationMode): LayoutConfigurationOption | undefined => {
  const compatible = layoutsForInstallation(mode);
  return (
    compatible.find((configuration) => configuration.code === draft?.layoutCode) ??
    compatible.find((configuration) => configuration.isDefault) ??
    compatible[0]
  );
};

const isSameSideConnection = (): boolean => {
  const view = selectedLayoutConfiguration()?.referenceView;
  if (view) return view.startsWith("SSC_");
  const connection = `${result?.aeraulicConnectionCode ?? ""} ${selectedUnit()?.model ?? ""}`
    .toUpperCase();
  return connection.includes("SSC") || connection.includes("SAME SIDE");
};

const isFsVsHciModel = (): boolean =>
  /(^|\s)(FS|VS|HCI)(\s|$)/i.test(selectedUnit()?.model ?? "");

const isStConnection = (): boolean =>
  (result?.aeraulicConnectionCode ?? "").toUpperCase() === "ST" ||
  /(^|\s)ST(\s|$)/i.test(selectedUnit()?.model ?? "");

const stHasRearPort = (configurationCode: string): boolean =>
  ["UH", "LU", "HU", "HH", "LH"].includes(configurationCode.toUpperCase());

type StFlowPlacement = { x: number; y: number; rear: boolean };

const stFlowPlacement = (configurationCode: string, position: number): StFlowPlacement => {
  const code = configurationCode.toUpperCase();
  if (position === 3) return { x: 150, y: 100, rear: false };
  if (position === 4) return { x: 150, y: 220, rear: false };
  if (position === 1) {
    const rear = ["HU", "HH", "LH"].includes(code);
    return { x: rear ? 110 : 115, y: rear ? 60 : 30, rear };
  }
  const lowerRear = ["LU", "LH"].includes(code);
  const upperRear = code === "UH";
  if (lowerRear) return { x: 194, y: 240, rear: true };
  if (upperRear) return { x: 185, y: 60, rear: true };
  return { x: 185, y: code === "HH" ? 60 : 30, rear: code === "HH" };
};

type AccessSurface = "upper" | "lower" | "front";

const selectedLayoutConfiguration = (): LayoutConfigurationOption | undefined =>
  layoutConfigurations().find((item) => item.code === draft?.layoutCode);

const isSameSideUprightFloor = (): boolean =>
  isSameSideConnection() &&
  draft?.installationMode === "floor" &&
  selectedLayoutConfiguration()?.referenceView === "SSC_UPRIGHT";

const isSameSideFlatFloor = (): boolean =>
  isSameSideConnection() &&
  draft?.installationMode === "floor" &&
  selectedLayoutConfiguration()?.referenceView === "SSC_FLAT";

const isFrontAccessHorizontalEastWestFloor = (): boolean => {
  const configuration = selectedLayoutConfiguration();
  return draft?.installationMode === "floor" &&
    configuration?.orientation === "horizontal" &&
    configuration.referenceView === "OSC_EAST_WEST" &&
    configuration.accessSide === "front";
};

const isOppositeSideEastWestWall = (): boolean =>
  !isSameSideConnection() &&
  draft?.installationMode === "wall" &&
  selectedLayoutConfiguration()?.referenceView === "OSC_EAST_WEST";

const accessSurface = (): AccessSurface => {
  const configured = selectedLayoutConfiguration()?.accessSide;
  if (configured) return configured;
  if (isSameSideUprightFloor() || draft?.installationMode === "wall") return "front";
  if (draft?.installationMode === "floor") return "upper";
  return "lower";
};

const accessSurfaceLabel = (surface: AccessSurface): string => {
  const installation = messages().ui.installation;
  if (surface === "upper") return installation.upperAccess;
  if (surface === "front") return installation.frontAccess;
  return installation.lowerAccess;
};

const renderInstallationStep = (): string => {
  const text = messages();
  const compatibleLayouts = layoutsForInstallation(draft!.installationMode);
  const surface = accessSurface();
  const uprightSscFloor = isSameSideUprightFloor();
  const uprightStFloor = isStConnection() && draft!.installationMode === "floor";
  const uprightFloor = uprightSscFloor || uprightStFloor || isFrontAccessHorizontalEastWestFloor();
  const stFlowLayout = isStConnection();
  const showStRearSideKey = stFlowLayout && stHasRearPort(draft!.layoutCode);
  const northSouthWall = !isSameSideConnection() && draft!.installationMode === "wall" && selectedLayoutConfiguration()?.referenceView === "OSC_NORTH_SOUTH";
  const oppositeSideWallClass = draft!.installationMode === "wall"
    ? isOppositeSideEastWestWall() ? "wall-east-west" : "wall-north-south"
    : "";
  const connectionClass = isSameSideConnection()
    ? `connection-ssc installation-${draft!.installationMode} ${isSameSideFlatFloor() ? "ssc-flat-floor" : "ssc-same-side"} ${isSameSideUprightFloor() ? "ssc-upright-floor" : ""}`
    : `connection-osc installation-${draft!.installationMode} ${oppositeSideWallClass}`;
  return `<div class="content-grid installation-grid">
    <section class="panel">
      <div class="panel-heading compact">
        <div><h2>${escapeHtml(text.ui.installation.typeTitle)}</h2><p>${escapeHtml(text.ui.installation.typeDescription)}</p></div>
      </div>
      <div class="segmented-cards">
        ${choiceCard("ceiling", text.domain.installation.ceiling, text.ui.installation.ceilingDescription, "panel-top", !installationAvailable("ceiling"))}
        ${choiceCard("floor", text.domain.installation.floor, text.ui.installation.floorDescription, "panel-bottom", !installationAvailable("floor"))}
        ${choiceCard("wall", text.domain.installation.wall, text.ui.installation.wallDescription, "panel-left", !installationAvailable("wall"))}
      </div>
      <div class="field">
        <label for="layoutCode">${escapeHtml(text.ui.installation.airflowConfiguration)}</label>
        <select id="layoutCode" data-field="layoutCode">
          ${compatibleLayouts.map((configuration) => `<option ${draft!.layoutCode === configuration.code ? "selected" : ""}>${configuration.code}</option>`).join("")}
        </select>
        <small>${escapeHtml(text.ui.installation.defaultHint)}</small>
        ${installationReviewRequired ? `<div class="layout-confirmation-warning" role="alert">${icon("triangle-alert", 15)} ${escapeHtml(drawingUiText().confirmLayout)}</div>` : ""}
      </div>
    </section>
    <section class="panel layout-preview">
      <div class="panel-heading compact">
        <div><h2>${escapeHtml(text.ui.installation.orientationTitle)}</h2><p>${escapeHtml(text.ui.installation.previewDescription)} ${escapeHtml(draft!.layoutCode)}.</p></div>
      </div>
      ${compatibleLayouts.length > 0 && !calculating && !calculationFailed ? `<div class="airflow-layout-body">
        <div class="installation-schematics ${connectionClass}" data-layout="${escapeHtml(draft!.layoutCode)}">
          <div class="schematic-mounting"><strong>${escapeHtml(installationViewLabel(draft!.installationMode, isOppositeSideEastWestWall()))}</strong>
            ${mountingSvg(draft!.installationMode, surface, isOppositeSideEastWestWall(), uprightFloor, escapeHtml(accessSurfaceLabel(surface)))}
            ${uprightFloor ? "" : `<span>${escapeHtml(accessSurfaceLabel(surface))}</span>`}
            ${draft!.installationMode === "floor" && !stFlowLayout ? `<p class="shk-note">${escapeHtml(schematicText(languageCode())[4])}</p>` : ""}
          </div>
          <div class="schematic-airflow"><strong>${escapeHtml(selectedUnit()?.model ?? "")} · ${escapeHtml(draft!.layoutCode)}</strong>
            <svg viewBox="0 0 300 ${northSouthWall || stFlowLayout ? 300 : 200}" role="img" aria-label="${escapeHtml(schematicText(languageCode())[0])}"><rect x="${northSouthWall || stFlowLayout ? 80 : 30}" y="30" width="${northSouthWall || stFlowLayout ? 140 : 240}" height="${northSouthWall || stFlowLayout ? 240 : 140}" fill="white" stroke="#91A0AE" stroke-width="2"/><foreignObject x="${northSouthWall || stFlowLayout ? 105 : 60}" y="${stFlowLayout ? 125 : 65}" width="${northSouthWall || stFlowLayout ? 90 : 180}" height="${stFlowLayout ? 70 : northSouthWall ? 170 : 70}"><div xmlns="http://www.w3.org/1999/xhtml" class="airflow-inner-caption">${escapeHtml(schematicText(languageCode())[0])}</div></foreignObject>${renderFlowPorts()}</svg>
            ${showStRearSideKey ? `<div class="st-rear-side-key"><span aria-hidden="true"></span><b>${escapeHtml(schematicText(languageCode())[5])}</b></div>` : ""}
          </div>
        </div>
        ${renderFlowLegend(surface)}
      </div>` : `<div class="empty-state airflow-pending" aria-busy="${calculating}">${escapeHtml(calculating ? text.ui.installation.orientationTitle : text.status.unavailable)}</div>`}
    </section>
    ${renderInlineDimensionalDrawing()}
  </div>`;
};

const renderWaterCoilStep = (): string => {
  const text = messages();
  const coils = result!.waterCoils ?? [];
  const selected = coils.find((item) => item.id === draft!.waterCoilId);
  const available = coils.length > 0;
  const customized = draft!.waterCoilCustomized;
  const externalGeometry = selected?.installation?.toLowerCase() !== "internal";
  const coolingEnabled =
    draft!.waterCoilMode === "CWD" || draft!.waterCoilMode === "HCD";
  const heatingEnabled =
    (draft!.waterCoilMode === "HWD" || draft!.waterCoilMode === "HCD") &&
    result!.waterHeatingEnabled !== false;
  const heatingReason = result!.waterHeatingDisabledReason ?? "";
  return `
    <div class="content-grid two">
      <section class="panel">
        ${toggleHeading(text.ui.waterCoil.treatment, "waterCoilEnabled", draft!.waterCoilEnabled && available, available ? text.ui.waterCoil.enable : text.status.unavailable)}
        <div class="${draft!.waterCoilEnabled && available ? "" : "disabled-section"}">
          <div class="form-grid">
            <div class="field">
              <label>${escapeHtml(text.ui.waterCoil.calculationMode)}</label>
              <select data-field="waterCoilMode" ${heatingReason ? `title="${escapeHtml(heatingReason)}"` : ""}>
                ${["CWD", "HWD", "HCD"].map((mode) =>
                  `<option value="${mode}" ${draft!.waterCoilMode === mode ? "selected" : ""} ${mode !== "CWD" && result!.waterHeatingEnabled === false ? "disabled" : ""}>${mode}</option>`,
                ).join("")}
              </select>
            </div>
            ${numberSelectField(text.ui.waterCoil.coil, "waterCoilId", draft!.waterCoilId, coils.map((item) => ({ value: item.id, label: `${item.name} · ${item.installationLabel || relationInstallationLabel(item.installation)}` })))}
            <div class="field">
              <label>${escapeHtml(text.ui.waterCoil.calculationMode)}</label>
              <select data-field="waterCoilCustomized">
                <option value="false" ${customized ? "" : "selected"}>${escapeHtml(result!.waterCoilStandardLabel ?? "Standard")}</option>
                <option value="true" ${customized ? "selected" : ""}>${escapeHtml(result!.waterCoilCustomizedLabel ?? "Customized")}</option>
              </select>
            </div>
            ${codedSelectField(text.ui.waterCoil.fluid, "fluidCode", draft!.fluidCode, [
              { value: "Water", label: text.domain.fluid.water },
              { value: "Glic_Etil", label: text.domain.fluid.ethyleneGlycol },
              { value: "Glic_Prop", label: text.domain.fluid.propyleneGlycol },
            ])}
            ${technicalNumberField(text.ui.waterCoil.glycol, "glycolPercent", draft!.glycolPercent, "%", draft!.fluidCode === "Water", 0, 60)}
            ${technicalNumberField(text.ui.waterCoil.coolingIn, "coolingWaterInletTemperature", draft!.coolingWaterInletTemperature, "°C", !coolingEnabled)}
            ${technicalNumberField(text.ui.waterCoil.coolingOut, "coolingWaterOutletTemperature", draft!.coolingWaterOutletTemperature, "°C", !coolingEnabled, draft!.coolingWaterInletTemperature + 1)}
            ${technicalNumberField(text.ui.waterCoil.heatingIn, "heatingWaterInletTemperature", draft!.heatingWaterInletTemperature, "°C", !heatingEnabled)}
            ${technicalNumberField(text.ui.waterCoil.heatingOut, "heatingWaterOutletTemperature", draft!.heatingWaterOutletTemperature, "°C", !heatingEnabled, undefined, draft!.heatingWaterInletTemperature - 1)}
          </div>
          ${selected ? `<div class="coil-geometry-grid">
            ${technicalNumberField("L [mm]", "waterCoilLengthMm", draft!.waterCoilLengthMm || selected.lengthMm, "mm", !customized || !externalGeometry, 1)}
            ${technicalNumberField("H [mm]", "waterCoilHeightMm", draft!.waterCoilHeightMm || selected.heightMm, "mm", !customized || !externalGeometry, 1)}
            ${technicalNumberField(text.ui.waterCoil.rows, "waterCoilRows", draft!.waterCoilRows || selected.rows, "", !customized, 1)}
            ${technicalNumberField(text.ui.waterCoil.circuits, "waterCoilCircuits", draft!.waterCoilCircuits || selected.circuits, "", !customized, 1)}
            ${codedSelectField(text.ui.waterCoil.finSpacing, "waterCoilFinSpacingMm", String(draft!.waterCoilFinSpacingMm || selected.finSpacingMm), [1.6, 1.8, 2, 2.1, 2.3, 2.5, 2.8, 3].map((value) => ({ value: String(value), label: `${formatNumber(value, 1)} mm` })), !customized)}
          </div>` : ""}
          ${customized ? `<div class="inline-notice warning">${icon("triangle-alert")}<span>${escapeHtml(result!.waterCoilDimensionsNotice ?? "")}<br/><b>${escapeHtml(result!.waterCoilQuotationNotice ?? "")}</b></span></div>` : ""}
          ${heatingReason ? `<div class="inline-notice warning">${icon("triangle-alert")}<span>${escapeHtml(heatingReason)}</span></div>` : ""}
        </div>
      </section>
      <section class="panel">
        <div class="panel-heading compact">
          <div><h2>${escapeHtml(text.ui.waterCoil.calculatedPerformance)}</h2><p>${escapeHtml(text.ui.waterCoil.calculatedDescription)}</p></div>
        </div>
        <div class="result-table">
          ${(result!.waterCoilResults?.length ?? 0) > 0
            ? result!.waterCoilResults!.map((item) => resultRow(item)).join("")
            : `<div class="empty-state">${escapeHtml(available ? text.ui.waterCoil.enableToCalculate : text.ui.waterCoil.noneAssociated)}</div>`}
        </div>
        ${(result!.notices?.length ?? 0) > 0
          ? `<div class="coil-messages">${result!.notices!.map((notice) =>
              `<div class="inline-notice ${notice.severity}">${icon(notice.severity === "danger" ? "circle-alert" : "triangle-alert")}<span>${escapeHtml(notice.message)}</span></div>`,
            ).join("")}</div>`
          : ""}
        ${result!.additionalPressureDropPa ? `<div class="inline-notice">${icon("gauge")}<span>${escapeHtml(text.ui.waterCoil.additionalPressureDrop)}: <b>${formatNumber(result!.additionalPressureDropPa, 0)} Pa</b></span></div>` : ""}
      </section>
    </div>`;
};

const renderElectricStep = (): string => {
  const text = messages();
  const heaters = result!.electricHeaters ?? [];
  return `
    <div class="content-grid two">
      ${heaterPanel("PEHD", text.ui.electricHeater.preheating, "electricPreheaterEnabled", draft!.electricPreheaterEnabled, "electricPreheaterId", heaters.filter((item) => item.mode === "PEHD"))}
      ${heaterPanel(
        "EHD",
        text.ui.electricHeater.postHeating,
        "electricPostheaterEnabled",
        draft!.electricPostheaterEnabled,
        "electricPostheaterId",
        heaters.filter((item) => item.mode === "EHD"),
        result!.electricPostheaterEnabled !== false,
        result!.electricPostheaterDisabledReason ?? "",
      )}
    </div>`;
};

const renderAccessoriesStep = (): string => {
  const text = messages();
  return `<section class="panel table-panel">
    <div class="table-toolbar">
      <div class="search-box">${icon("search")}<input type="search" placeholder="${escapeHtml(text.ui.accessories.searchPlaceholder)}" data-accessory-search /></div>
      <select aria-label="${escapeHtml(text.ui.aria.accessoryCategory)}" data-accessory-category>
        <option>${escapeHtml(text.ui.accessories.allCategories)}</option>
        ${[...new Set(data!.accessories.map((item) => item.category))].map((value) => `<option>${value}</option>`).join("")}
      </select>
      <span>${draft!.accessoryCodes.length} ${escapeHtml(text.ui.accessories.selectedCount)}</span>
    </div>
    <div class="data-table" role="table">
      <div class="data-row header" role="row">
        <span></span><span>${escapeHtml(text.ui.accessories.code)}</span><span>${escapeHtml(text.ui.accessories.description)}</span><span>${escapeHtml(text.ui.accessories.category)}</span><span>${escapeHtml(text.ui.accessories.installation)}</span>
      </div>
      <div data-accessory-rows>
        ${renderAccessoryRows(data!.accessories)}
      </div>
    </div>
  </section>`;
};

const renderCo2Step = (): string => {
  const copy = messages().ui.co2Sound;
  const co2 = draft!.co2;
  const co2Result = result!.co2Result;
  const disabled = calculating;
  const disabledAttribute = disabled ? "disabled" : "";
  const methodOptions = [
    {
      value: "maximum-concentration",
      label: copy.maximumConcentrationMethod,
    },
    { value: "fixed-airflow", label: copy.fixedAirflowMethod },
    {
      value: "airflow-per-person-and-area",
      label: copy.personAndAreaMethod,
    },
  ];

  return `
    <div class="co2-step ${disabled ? "calculation-pending" : ""}" aria-busy="${disabled}">
      <div class="co2-layout">
        <section class="panel">
          <div class="panel-heading toggle-heading">
            <div>
              <h2>${escapeHtml(copy.co2Title)}</h2>
              <p>${escapeHtml(copy.co2Description)}</p>
            </div>
            <label class="toggle">
              <input type="checkbox" data-field="co2.includeInReport" ${co2.includeInReport ? "checked" : ""} ${disabledAttribute}/>
              <span></span><b>${escapeHtml(copy.includeCo2InReport)}</b>
            </label>
          </div>
          <fieldset class="settings-fieldset" ${disabledAttribute}>
            <legend>${escapeHtml(copy.roomDimensions)}</legend>
            <div class="co2-dimensions-grid">
              ${technicalNumberField(copy.width, "co2.roomWidthMeters", co2.roomWidthMeters, "m", disabled, 0.1)}
              ${technicalNumberField(copy.length, "co2.roomLengthMeters", co2.roomLengthMeters, "m", disabled, 0.1)}
              ${technicalNumberField(copy.height, "co2.roomHeightMeters", co2.roomHeightMeters, "m", disabled, 0.1)}
            </div>
          </fieldset>
          <fieldset class="settings-fieldset" ${disabledAttribute}>
            <legend>${escapeHtml(copy.useProfile)}</legend>
            <div class="co2-profile-grid">
              ${technicalNumberField(copy.activity, "co2.activityMet", co2.activityMet, "met", disabled, 0.1)}
              ${technicalNumberField(copy.occupiedPeople, "co2.occupiedPeople", co2.occupiedPeople, "", disabled, 0)}
              ${technicalNumberField(copy.occupiedMinutes, "co2.occupiedMinutes", co2.occupiedMinutes, "min", disabled, 0)}
              ${technicalNumberField(copy.breakPeople, "co2.breakPeople", co2.breakPeople, "", disabled, 0)}
              ${technicalNumberField(copy.breakMinutes, "co2.breakMinutes", co2.breakMinutes, "min", disabled, 0)}
            </div>
          </fieldset>
          <div class="co2-method-grid">
            ${codedSelectField(copy.calculationMethod, "co2.calculationMethod", co2.calculationMethod, methodOptions, disabled)}
            ${technicalNumberField(copy.outdoorConcentration, "co2.outdoorConcentrationPpm", co2.outdoorConcentrationPpm, "ppm", disabled, 0)}
            ${technicalNumberField(copy.maximumConcentration, "co2.maximumConcentrationPpm", co2.maximumConcentrationPpm, "ppm", disabled, 0)}
            ${technicalNumberField(copy.airflowPerArea, "co2.airflowPerAreaLitersPerSecondPerSquareMeter", co2.airflowPerAreaLitersPerSecondPerSquareMeter, "l/s·m²", disabled, 0)}
            ${technicalNumberField(copy.airflowPerPerson, "co2.airflowPerPersonLitersPerSecond", co2.airflowPerPersonLitersPerSecond, "l/s", disabled, 0)}
          </div>
        </section>

        <section class="panel co2-results-panel">
          <div class="panel-heading">
            <span class="panel-icon">${icon("activity")}</span>
            <div><h2>${escapeHtml(copy.co2Results)}</h2><p>${escapeHtml(copy.co2Description)}</p></div>
          </div>
          ${
            co2Result
              ? `<div class="co2-result-grid">
                  ${metric(copy.roomArea, `${formatNumber(co2Result.roomAreaSquareMeters, 1)} m²`, "ruler")}
                  ${metric(copy.roomVolume, `${formatNumber(co2Result.roomVolumeCubicMeters, 1)} m³`, "panels-top-left")}
                  ${metric(copy.co2Generation, `${formatNumber(co2Result.carbonDioxideGenerationLitersPerSecondPerPerson, 4)} l/s`, "activity")}
                  ${metric(copy.requiredOutdoorAirflow, `${formatNumber(co2Result.requiredOutdoorAirflowLitersPerSecond, 1)} l/s`, "wind")}
                  ${metric(copy.requiredOutdoorAirflow, `${formatNumber(co2Result.requiredOutdoorAirflowCubicMetersPerHour, 0)} m³/h`, "wind")}
                  ${metric(copy.calculatedMaximumConcentration, `${formatNumber(co2Result.calculatedMaximumConcentrationPpm, 0)} ppm`, "gauge")}
                </div>
                ${renderCo2Chart(co2Result.points)}`
              : `<div class="empty-calculation">${icon("loader-circle")}<span>${escapeHtml(copy.awaitingBackendResults)}</span></div>`
          }
        </section>
      </div>
    </div>`;
};

const renderSoundStep = (): string => {
  const copy = messages().ui.co2Sound;
  const sound = draft!.sound;
  const soundResult = result!.soundResult;
  const soundRows = soundResult?.spectrumRows ?? [];
  const iso16032Available = soundResult?.iso16032Available ?? false;
  const disabled = calculating;
  const disabledAttribute = disabled ? "disabled" : "";
  const distance1Heading = copy.soundPressureAtDistance.replace(
    "{distance}",
    formatNumber(sound.distance1Meters),
  );
  const distance2Heading = copy.soundPressureAtDistance.replace(
    "{distance}",
    formatNumber(sound.distance2Meters),
  );

  return `
    <div class="sound-step ${disabled ? "calculation-pending" : ""}" aria-busy="${disabled}">
      <section class="panel sound-panel">
        <div class="panel-heading toggle-heading">
          <div>
            <h2>${escapeHtml(copy.soundTitle)}</h2>
            <p>${escapeHtml(copy.soundDescription)}</p>
          </div>
          <label class="toggle">
            <input type="checkbox" data-field="sound.includeInReport" ${sound.includeInReport ? "checked" : ""} ${disabledAttribute}/>
            <span></span><b>${escapeHtml(copy.includeSoundInReport)}</b>
          </label>
        </div>
        <div class="sound-settings-grid">
          ${codedSelectField(
            copy.directivityFactor,
            "sound.directivityFactor",
            String(sound.directivityFactor),
            [2, 4, 8].map((value) => ({ value: String(value), label: `Q = ${value}` })),
            disabled,
          )}
          ${technicalNumberField(copy.firstDistance, "sound.distance1Meters", sound.distance1Meters, "m", disabled, 0.1)}
          ${technicalNumberField(copy.secondDistance, "sound.distance2Meters", sound.distance2Meters, "m", disabled, 0.1)}
          ${
            iso16032Available
              ? `<label class="toggle iso-toggle ${disabled ? "disabled" : ""}">
                  <input type="checkbox" data-field="sound.iso16032Enabled" ${sound.iso16032Enabled ? "checked" : ""} ${disabledAttribute}/>
                  <span></span><b>${escapeHtml(copy.iso16032)}</b>
                </label>`
              : ""
          }
        </div>
        <div class="spectrum-heading">
          <h3>${escapeHtml(copy.spectralResults)}</h3>
        </div>
        ${
          soundRows.length > 0
            ? `<div class="sound-spectrum-wrap">
                <table class="sound-spectrum-table">
                  <thead>
                    <tr>
                      <th>${escapeHtml(copy.airPath)}</th>
                      <th>63 Hz</th><th>125 Hz</th><th>250 Hz</th><th>500 Hz</th>
                      <th>1 kHz</th><th>2 kHz</th><th>4 kHz</th><th>8 kHz</th>
                      <th>${escapeHtml(copy.weightedSoundPower)}</th>
                      <th>${escapeHtml(distance1Heading)}</th>
                      <th>${escapeHtml(distance2Heading)}</th>
                    </tr>
                  </thead>
                  <tbody>
                    ${soundRows
                      .map(
                        (row) => `<tr>
                          <th scope="row"><strong>${escapeHtml(localizedSoundPath(row.airPathCode, row.airPathLabel))}</strong></th>
                          <td>${formatNumber(row.octaveBand63HzDb)}</td>
                          <td>${formatNumber(row.octaveBand125HzDb)}</td>
                          <td>${formatNumber(row.octaveBand250HzDb)}</td>
                          <td>${formatNumber(row.octaveBand500HzDb)}</td>
                          <td>${formatNumber(row.octaveBand1000HzDb)}</td>
                          <td>${formatNumber(row.octaveBand2000HzDb)}</td>
                          <td>${formatNumber(row.octaveBand4000HzDb)}</td>
                          <td>${formatNumber(row.octaveBand8000HzDb)}</td>
                          <td><strong>${formatNumber(row.weightedSoundPowerDbA)}</strong></td>
                          <td>${row.soundPressureAtDistance1DbA === null ? "-" : formatNumber(row.soundPressureAtDistance1DbA)}</td>
                          <td>${row.soundPressureAtDistance2DbA === null ? "-" : formatNumber(row.soundPressureAtDistance2DbA)}</td>
                        </tr>`,
                      )
                      .join("")}
                  </tbody>
                </table>
              </div>`
            : `<div class="empty-calculation">${icon("loader-circle")}<span>${escapeHtml(copy.awaitingBackendResults)}</span></div>`
        }
      </section>
    </div>`;
};

const renderDocumentsStep = (): string => {
  const text = messages();
  const applicationDocuments: [string, string, string, string, string, string] = ({
    bg: ["Сертификати", "Текст на тръжната процедура", "3D модел", "Документ за въвеждане в експлоатация", "Инструкции за смяна на филтъра", "Технически лист на материала в експлоатация"],
    cs: ["Certifikace", "Text zadávací dokumentace", "3D model", "Dokument uvedení do provozu", "Pokyny k výměně filtru", "Technický list materiálu v provozu"],
    da: ["Certificeringer", "Tekst til udbudsmateriale", "3D-model", "Ibrugtagningdokument", "Instruktioner til udskiftning af filter", "Teknisk datablad for materiale i drift"],
    de: ["Zertifizierungen", "Ausschreibungstext", "3D-Modell", "Inbetriebnahmedokument", "Anleitung zum Filterwechsel", "Technisches Datenblatt des eingesetzten Materials"],
    en: ["Certifications", "Tender specification text", "3D model", "Commissioning document", "Filter replacement instructions", "Technical data sheet of material in service"],
    fr: ["Certifications", "Texte du dossier d'appel d'offres", "Modèle 3D", "Document de mise en service", "Instructions de remplacement du filtre", "Fiche technique du matériau en service"],
    hu: ["Tanúsítványok", "Közbeszerzési műszaki leírás", "3D modell", "Üzembe helyezési dokumentum", "Szűrőcsere utasításai", "Üzemben lévő anyag műszaki adatlapja"],
    is: ["Vottanir", "Text útboðsgagna", "3D-líkan", "Gangsetningarskjal", "Leiðbeiningar um síuskipti", "Tæknilegt gagnablað efnis í notkun"],
    it: ["Certificazioni", "Testo bando di gara", "Modello 3D", "Documento di messa in servizio", "Istruzioni per la sostituzione del filtro", "Scheda tecnica del materiale in servizio"],
    nl: ["Certificeringen", "Tekst van de aanbestedingsspecificatie", "3D-model", "Inbedrijfstellingsdocument", "Instructies voor filtervervanging", "Technisch gegevensblad van materiaal in gebruik"],
    no: ["Sertifiseringer", "Tekst til konkurransegrunnlag", "3D-modell", "Igangkjøringsdokument", "Instruksjoner for filterbytte", "Teknisk datablad for materiale i drift"],
    pl: ["Certyfikaty", "Tekst specyfikacji przetargowej", "Model 3D", "Dokument uruchomienia", "Instrukcja wymiany filtra", "Karta techniczna materiału w eksploatacji"],
    ro: ["Certificări", "Textul caietului de sarcini", "Model 3D", "Document de punere în funcțiune", "Instrucțiuni de înlocuire a filtrului", "Fișa tehnică a materialului în exploatare"],
    sl: ["Certifikati", "Besedilo razpisne dokumentacije", "3D-model", "Dokument za zagon", "Navodila za zamenjavo filtra", "Tehnični list materiala v uporabi"],
    sv: ["Certifieringar", "Text för upphandlingsunderlag", "3D-modell", "Driftsättningsdokument", "Instruktioner för filterbyte", "Tekniskt datablad för material i drift"],
  } as Record<string, [string, string, string, string, string, string]>)[languageCode()] ?? ["Certifications", "Tender specification text", "3D model", "Commissioning document", "Filter replacement instructions", "Technical data sheet of material in service"];
  const applicationTitle: string = {
    bg: "Документи за приложения (референтни случаи)", cs: "Dokumenty k aplikacím (případové studie)", da: "Applikationsdokumenter (cases)", de: "Anwendungsdokumente (Referenzfälle)", en: "Application documents (Study cases)", fr: "Documents d'application (études de cas)", hu: "Alkalmazási dokumentumok (esettanulmányok)", is: "Notkunarskjöl (dæmisögur)", it: "Documenti applicativi (casi studio)", nl: "Toepassingsdocumenten (praktijkcases)", no: "Applikasjonsdokumenter (referanser)", pl: "Dokumenty aplikacyjne (studia przypadków)", ro: "Documente de aplicație (studii de caz)", sl: "Dokumenti aplikacij (študije primerov)", sv: "Applikationsdokument (referensfall)",
  }[languageCode()] ?? "Application documents (Study cases)";
  const applicationDescription: string = {
    bg: "Референтни приложения и казуси", cs: "Referenční aplikace a případové studie", da: "Referenceapplikationer og cases", de: "Referenzanwendungen und Fallstudien", en: "Reference applications and study cases", fr: "Applications de référence et études de cas", hu: "Referenciaalkalmazások és esettanulmányok", is: "Viðmiðunarnotkun og dæmisögur", it: "Applicazioni di riferimento e casi studio", nl: "Referentietoepassingen en praktijkcases", no: "Referanseapplikasjoner og caser", pl: "Aplikacje referencyjne i studia przypadków", ro: "Aplicații de referință și studii de caz", sl: "Referenčne aplikacije in študije primerov", sv: "Referensapplikationer och fallstudier",
  }[languageCode()] ?? "Reference applications and study cases";
  return `<div class="content-grid documents-grid">
    ${productDocumentsLoading ? `
      <div class="documents-loading" role="status" aria-live="polite">
        <span class="busy-icon">${icon("loader-circle", 28)}</span>
        <div>
          <strong>${escapeHtml(text.common.loading)}</strong>
          <span>${escapeHtml(text.ui.documents.localPackageNotice)}</span>
          <div class="loading-line"><span></span></div>
        </div>
      </div>` : ""}
    ${documentCard(text.ui.documents.technicalSheet, text.ui.documents.technicalSheetDescription, "PDF", "file-text", "commercial-sheet", productDocuments?.commercialSheetAvailable === true)}
    ${documentCard(text.ui.documents.dimensionalDrawing, text.ui.documents.dimensionalDrawingDescription, "PDF", "ruler", "dimensional-drawing", dimensionalDrawing?.available === true)}
    ${documentCard(text.ui.documents.installationManual, text.ui.documents.installationManualDescription, "PDF", "book-open", "installation-manual", productDocuments?.installationManualAvailable === true)}
    ${documentCard(applicationTitle, applicationDescription, "PDF", "files", null, false)}
    ${documentCard(applicationDocuments[0], "", "PDF", "badge-check", null, false)}
    ${documentCard(applicationDocuments[1], "", "PDF", "file-text", null, false)}
    ${documentCard(applicationDocuments[2], "", "STEP", "box", "step-model", productDocuments?.stepModelAvailable === true)}
    ${documentCard(applicationDocuments[3], "", "PDF", "clipboard-check", null, false)}
    ${documentCard(applicationDocuments[4], "", "PDF", "refresh-cw", null, false)}
    ${documentCard(applicationDocuments[5], "", "PDF", "file-cog", null, false)}
  </div>
  <div class="inline-notice success">
    ${icon("hard-drive-download")}
    <span>${escapeHtml(text.ui.documents.localPackageNotice)}</span>
  </div>`;
};

const renderSummaryStep = (): string => {
  const text = messages();
  const unit = selectedUnit();
  return `
    <div class="summary-layout summary-layout-single">
      <section class="summary-main">
        <div class="summary-banner ${stateTone()}">
          <span>${icon(result!.status === "valid" ? "badge-check" : "triangle-alert", 26)}</span>
          <div><strong>${statusLabel(result!.status)}</strong><p>${escapeHtml(result!.messages[0] ?? text.ui.summary.readyForReport)}</p></div>
        </div>
        <div class="summary-section">
          <div class="summary-section-heading"><h2>${escapeHtml(text.ui.summary.configuration)}</h2><button data-step="preselection">${escapeHtml(text.actions.edit)}</button></div>
          ${renderKeyValues([
            [text.ui.summary.project, draft!.project.name],
            [text.ui.summary.reference, draft!.project.customerReference || "-"],
            [text.ui.summary.unit, `${unit?.family} · ${unit?.model}`],
            [text.ui.summary.installation, `${installationLabel(draft!.installationMode)} · ${draft!.layoutCode}`],
          ], "wide")}
        </div>
        <div class="summary-section">
          <div class="summary-section-heading"><h2>${escapeHtml(text.ui.summary.dutyPoint)}</h2><button data-step="preselection">${escapeHtml(text.actions.edit)}</button></div>
          <div class="summary-metrics">
            ${summaryMetric(text.ui.summary.supplyAirflow, `${formatNumber(draft!.operatingPoint.supplyAirflow, 0)} m³/h`)}
            ${summaryMetric(text.ui.summary.extractAirflow, `${formatNumber(draft!.operatingPoint.extractAirflow, 0)} m³/h`)}
            ${summaryMetric(text.ui.summary.requestedPressure, `${formatNumber(draft!.operatingPoint.pressure, 0)} Pa`)}
            ${summaryMetric(text.ui.summary.availableMargin, `${formatNumber(result!.availablePressure, 0)} Pa`)}
            ${summaryMetric(`${text.ui.preselection.winter} · ${text.ui.preselection.supplyAirTemperature} (HX)`, `${formatNumber(result!.supplyTemperature, 1)} °C`)}
            ${draft!.waterCoilEnabled && result!.waterCoilResults?.find((item) => item.mode === "HWD")
              ? summaryMetric(`${text.ui.preselection.winter} · ${text.ui.preselection.supplyAirTemperature} (${draft!.waterCoilMode})`, `${formatNumber(result!.waterCoilResults!.find((item) => item.mode === "HWD")!.airOutletTemperatureC, 1)} °C`)
              : ""}
            ${draft!.electricPostheaterEnabled && result!.electricHeaterResults?.find((item) => item.mode === "EHD")
              ? summaryMetric(`${text.ui.preselection.winter} · ${text.ui.preselection.supplyAirTemperature} (EHD)`, `${formatNumber(result!.electricHeaterResults!.find((item) => item.mode === "EHD")!.airOutletTemperatureC, 1)} °C`)
              : ""}
            ${draft!.summerEnabled ? summaryMetric(`${text.ui.preselection.summer} · ${text.ui.preselection.supplyAirTemperature} (HX)`, `${formatNumber(result!.summerSupplyTemperature, 1)} °C`) : ""}
            ${draft!.summerEnabled && draft!.waterCoilEnabled && result!.waterCoilResults?.find((item) => item.mode === "CWD")
              ? summaryMetric(`${text.ui.preselection.summer} · ${text.ui.preselection.supplyAirTemperature} (${draft!.waterCoilMode})`, `${formatNumber(result!.waterCoilResults!.find((item) => item.mode === "CWD")!.airOutletTemperatureC, 1)} °C`)
              : ""}
          </div>
        </div>
        <div class="summary-section">
          <div class="summary-section-heading"><h2>${escapeHtml(text.ui.summary.options)}</h2><button data-step="accessories">${escapeHtml(text.actions.edit)}</button></div>
          <div class="summary-options">
            ${optionSummary(text.ui.summary.waterCoil, draft!.waterCoilEnabled ? draft!.waterCoilMode : text.ui.summary.notSelectedFeminine, draft!.waterCoilEnabled)}
            ${optionSummary(text.ui.summary.electricPreheating, draft!.electricPreheaterEnabled ? "PEHD" : text.ui.summary.notSelectedMasculine, draft!.electricPreheaterEnabled)}
            ${optionSummary(text.ui.summary.electricPostHeating, draft!.electricPostheaterEnabled ? "EHD" : text.ui.summary.notSelectedMasculine, draft!.electricPostheaterEnabled)}
            ${optionSummary(text.ui.summary.accessories,
              draft!.accessoryCodes.length ? `<ul class="summary-accessory-list">${draft!.accessoryCodes.map((code) => {
                const accessory = data!.accessories.find((item) => item.code === code);
                return `<li>${escapeHtml(code)}${accessory ? ` · ${escapeHtml(accessory.name)}` : ""}</li>`;
              }).join("")}</ul>` : escapeHtml(text.ui.summary.notSelectedMasculine),
              draft!.accessoryCodes.length > 0)}
          </div>
        </div>
      </section>
    </div>`;
};

const renderShowcase = (): void => {
  const text = messages();
  app.innerHTML = `
    <main class="showcase">
      <header class="showcase-header">
        <div>
          <span class="eyebrow">${escapeHtml(text.ui.showcase.localDesignSystem)}</span>
          <h1>${escapeHtml(text.ui.showcase.title)}</h1>
          <p>${escapeHtml(text.ui.showcase.description)}</p>
        </div>
        <a class="button secondary" href="#/selection">${icon("arrow-left")} ${escapeHtml(text.ui.showcase.backToSelection)}</a>
      </header>
      <section class="showcase-section">
        <h2>${escapeHtml(text.ui.showcase.actions)}</h2>
        <div class="showcase-row">
          <button class="button primary">${icon("check")} ${escapeHtml(text.ui.showcase.primary)}</button>
          <button class="button secondary">${icon("save")} ${escapeHtml(text.ui.showcase.secondary)}</button>
          <button class="button danger">${icon("trash-2")} ${escapeHtml(text.ui.showcase.destructive)}</button>
          <button class="icon-button bordered">${icon("settings")}</button>
        </div>
      </section>
      <section class="showcase-section">
        <h2>${escapeHtml(text.ui.showcase.fields)}</h2>
        <div class="showcase-fields">
          ${textField(text.ui.showcase.text, "demo", text.ui.showcase.exampleValue, "")}
          ${numberField(text.ui.showcase.airflow, "flow", 300, "m³/h")}
          ${selectField(text.ui.showcase.mode, "mode", "HCD", ["CWD", "HWD", "HCD"])}
          <label class="toggle"><input type="checkbox" checked /><span></span><b>${escapeHtml(text.ui.showcase.activeOption)}</b></label>
        </div>
      </section>
      <section class="showcase-section">
        <h2>${escapeHtml(text.ui.showcase.messages)}</h2>
        <div class="showcase-messages">
          <div class="summary-banner valid">${icon("circle-check")}<div><strong>${escapeHtml(text.status.valid)}</strong><p>${escapeHtml(text.ui.showcase.allChecksPassed)}</p></div></div>
          <div class="summary-banner warning">${icon("triangle-alert")}<div><strong>${escapeHtml(text.status.warning)}</strong><p>${escapeHtml(text.ui.showcase.pressureMarginReduced)}</p></div></div>
          <div class="summary-banner invalid">${icon("circle-x")}<div><strong>${escapeHtml(text.status.invalid)}</strong><p>${escapeHtml(text.ui.showcase.correctDutyPoint)}</p></div></div>
        </div>
      </section>
      <section class="showcase-section">
        <h2>${escapeHtml(text.ui.showcase.metricsAndTokens)}</h2>
        <div class="metric-grid showcase-metrics">
          ${metric(text.ui.context.efficiency, "95%", "trending-up")}
          ${metric(text.ui.context.pressure, "206 Pa", "gauge")}
          ${metric(text.ui.context.power, "176 W", "zap")}
          ${metric("SFP", "0,59", "activity")}
        </div>
        <div class="color-tokens">
          <span style="--swatch:#12233f">Ink</span>
          <span style="--swatch:#28549c">Action</span>
          <span style="--swatch:#d62c3b">Brand</span>
          <span style="--swatch:#23835c">Success</span>
          <span style="--swatch:#d98b16">Warning</span>
        </div>
      </section>
    </main>
  `;
  renderIcons();
};

const textField = (label: string, field: string, value: string, placeholder: string): string => `
  <div class="field">
    <label>${label}</label>
    <input type="text" data-field="${field}" value="${escapeHtml(value)}" placeholder="${escapeHtml(placeholder)}" />
  </div>`;

const numberField = (label: string, field: string, value: number, unit: string): string => `
  <div class="field">
    <label>${label}</label>
    <div class="input-with-unit"><input type="number" min="0" data-field="${field}" value="${value}" /><span>${unit}</span></div>
  </div>`;

const technicalNumberField = (
  label: string,
  field: string,
  value: number,
  unit: string,
  disabled = false,
  min?: number,
  max?: number,
): string => `
  <div class="field compact-field ${disabled ? "is-disabled" : ""}">
    <label>${escapeHtml(label)}</label>
    <div class="input-with-unit">
      <input type="number" data-field="${field}" value="${value}" ${disabled ? "disabled" : ""} ${min === undefined ? "" : `min="${min}"`} ${max === undefined ? "" : `max="${max}"`}/>
      <span>${escapeHtml(unit)}</span>
    </div>
  </div>`;

const selectField = (label: string, field: string, value: string, options: string[]): string => `
  <div class="field">
    <label>${label}</label>
    <select data-field="${field}">
      ${options.map((option) => `<option ${option === value ? "selected" : ""}>${escapeHtml(option)}</option>`).join("")}
    </select>
  </div>`;

const codedSelectField = (
  label: string,
  field: string,
  value: string,
  options: Array<{ value: string; label: string }>,
  disabled = false,
): string => `
  <div class="field">
    <label>${escapeHtml(label)}</label>
    <select data-field="${field}" ${disabled ? "disabled" : ""}>
      ${options.map((option) => `<option value="${escapeHtml(option.value)}" ${option.value === value ? "selected" : ""}>${escapeHtml(option.label)}</option>`).join("")}
    </select>
  </div>`;

const numberSelectField = (
  label: string,
  field: string,
  value: number,
  options: Array<{ value: number; label: string }>,
): string => `
  <div class="field">
    <label>${label}</label>
    <select data-field="${field}" ${options.length === 0 ? "disabled" : ""}>
      ${options.map((option) => `<option value="${option.value}" ${option.value === value ? "selected" : ""}>${escapeHtml(option.label)}</option>`).join("")}
    </select>
  </div>`;

const renderKeyValues = (pairs: [string, string][], className = ""): string => `
  <dl class="key-values ${className}">
    ${pairs.map(([label, value]) => `<div><dt>${escapeHtml(label)}</dt><dd>${escapeHtml(value)}</dd></div>`).join("")}
  </dl>`;

const metric = (label: string, value: string, iconName: string): string => `
  <div class="metric"><span>${icon(iconName)}</span><div><small>${label}</small><strong>${value}</strong></div></div>`;

const statusRow = (label: string, value: string, status: string): string => `
  <div><span>${label}</span><strong>${value}</strong><b>${status}</b></div>`;

const unitCard = (unit: UnitOption, recommended: boolean): string => {
  const text = messages();
  const filters = draft!.preselectionFilters;
  const acousticValues = [
    filters.supplyNoiseEnabled
      ? `${text.ui.preselection.supplyNoise}: ${formatNumber(filters.supplyNoiseMetric === "LPA" ? unit.supplySoundPressureDbA ?? 0 : unit.supplySoundPowerDbA ?? 0, 1)} dB(A) ${filters.supplyNoiseMetric}`
      : "",
    filters.breakoutNoiseEnabled
      ? `${text.ui.preselection.breakoutNoise}: ${formatNumber(filters.breakoutNoiseMetric === "LPA" ? unit.breakoutSoundPressureDbA ?? 0 : unit.breakoutSoundPowerDbA ?? 0, 1)} dB(A) ${filters.breakoutNoiseMetric}`
      : "",
  ].filter(Boolean);
  return `
  <article class="ranked-unit ${unit.id === draft!.selectedUnitId ? "selected" : ""}" data-select-unit="${unit.id}">
    <div class="ranked-unit-score"><strong>${formatNumber(unit.requiredRegulation, 0)}</strong><span>%</span></div>
    <div><strong>${unit.model}</strong></div>
    <div class="ranked-spec"><span>${formatNumber(unit.availablePressure, 0)} Pa</span><span>${formatNumber(unit.absorbedPower, 0)} W</span><span>SFP ${formatNumber(unit.sfp, 2)}</span>${acousticValues.map((value) => `<span>${escapeHtml(value)}</span>`).join("")}</div>
    ${recommended ? `<b class="recommended">${icon("sparkles", 14)} ${escapeHtml(text.ui.preselection.recommended)}</b>` : ""}
    ${unit.id === draft!.selectedUnitId ? icon("circle-check", 20) : icon("chevron-right", 20)}
  </article>`;
};

const choiceCard = (value: string, title: string, description: string, iconName: string, disabled = false): string => `
  <button class="choice-card ${draft!.installationMode === value ? "selected" : ""}" data-installation="${value}" type="button" ${disabled ? "disabled aria-disabled=\"true\"" : ""}>
    <span>${icon(iconName, 22)}</span><strong>${title}</strong><small>${description}</small>
  </button>`;

const toggleHeading = (title: string, field: string, checked: boolean, label: string): string => `
  <div class="panel-heading toggle-heading">
    <div><h2>${escapeHtml(title)}</h2><p>${escapeHtml(messages().ui.electricHeater.optionalDescription)}</p></div>
    <label class="toggle"><input type="checkbox" data-field="${field}" ${checked ? "checked" : ""}/><span></span><b>${label}</b></label>
  </div>`;

const constrainedToggleHeading = (
  title: string,
  field: string,
  checked: boolean,
  label: string,
  disabled: boolean,
  disabledReason: string,
): string => `
  <div class="panel-heading toggle-heading">
    <div><h2>${escapeHtml(title)}</h2><p>${escapeHtml(messages().ui.electricHeater.optionalDescription)}</p></div>
    <label class="toggle ${disabled ? "disabled" : ""}" title="${escapeHtml(disabledReason)}">
      <input type="checkbox" data-field="${field}" ${checked ? "checked" : ""} ${disabled ? "disabled" : ""}/>
      <span></span><b>${escapeHtml(disabled ? disabledReason : label)}</b>
    </label>
  </div>`;

const resultRow = (item: WaterCoilPerformance): string => `
  <div class="coil-result">
    <strong>${escapeHtml(item.mode)}<small>${escapeHtml(item.status)}</small></strong>
    <span><small>${escapeHtml(messages().ui.waterCoil.capacity)}</small>${formatNumber(item.capacityW, 0)} W</span>
    <span><small>Sensible</small>${formatNumber(item.sensibleCapacityW, 0)} W</span>
    <span><small>${escapeHtml(messages().ui.waterCoil.airOut)}</small>${formatNumber(item.airOutletTemperatureC, 1)} °C · ${formatNumber(item.airOutletRelativeHumidityPercent, 0)}%</span>
    <span><small>Cond.</small>${formatNumber(item.condensateLitersPerHour, 2)} l/h</span>
    <span><small>${escapeHtml(messages().ui.waterCoil.airPressureDrop)}</small>${formatNumber(item.airPressureDropPa, 0)} Pa</span>
    <span><small>DP fluid</small>${formatNumber(item.fluidPressureDropKPa, 1)} kPa</span>
    <span><small>Flow fluid</small>${formatNumber(item.fluidFlowLitersPerHour, 0)} l/h</span>
    <span><small>Speed fluid</small>${formatNumber(item.fluidVelocityMetersPerSecond, 2)} m/s</span>
    <span><small>Face speed</small>${formatNumber(item.faceVelocityMetersPerSecond, 2)} m/s</span>
  </div>`;

const heaterPanel = (
  code: string,
  title: string,
  field: string,
  enabled: boolean,
  idField: string,
  heaters: NonNullable<SelectionResult["electricHeaters"]>,
  selectionEnabled = true,
  disabledReason = "",
): string => {
  const text = messages();
  const selectedId = code === "PEHD" ? draft!.electricPreheaterId : draft!.electricPostheaterId;
  const selected = heaters.find((item) => item.id === selectedId) ?? heaters[0];
  const performance = result!.electricHeaterResults?.find((item) => item.mode === code);
  return `
    <section class="panel">
      ${constrainedToggleHeading(
        `${code} · ${title}`,
        field,
        enabled && heaters.length > 0,
        heaters.length > 0 ? text.ui.electricHeater.enableCalculation : text.ui.electricHeater.unavailable,
        heaters.length === 0 || !selectionEnabled,
        heaters.length === 0 ? text.ui.electricHeater.unavailable : disabledReason,
      )}
      <div class="${enabled && heaters.length > 0 ? "" : "disabled-section"}">
        ${numberSelectField(text.ui.electricHeater.heater, idField, selectedId, heaters.map((item) => ({ value: item.id, label: item.name || item.code })))}
        ${renderKeyValues([
          [text.ui.electricHeater.installation, selected ? relationInstallationLabel(selected.installation) : "-"],
          [text.ui.electricHeater.power, selected ? `${formatNumber(selected.powerW, 0)} W` : "-"],
          [text.ui.electricHeater.voltagePhases, selected ? `${formatNumber(selected.voltageV, 0)} V / ${selected.phaseCount}` : "-"],
          [text.ui.electricHeater.current, selected ? `${formatNumber(selected.currentA, 2)} A` : "-"],
          [text.ui.electricHeater.airIn, performance ? `${formatNumber(performance.airInletTemperatureC, 1)} °C` : "-"],
          [text.ui.electricHeater.maximumAirOut, performance ? `${formatNumber(performance.airOutletTemperatureC, 1)} °C` : "-"],
          [text.ui.electricHeater.relativeHumidityOut, performance ? `${formatNumber(performance.airOutletRelativeHumidityPercent, 0)}%` : "-"],
          [text.ui.electricHeater.airPressureDrop, performance ? `${formatNumber(performance.airPressureDropPa, 1)} Pa` : "-"],
        ], "wide")}
      </div>
    </section>`;
};

const renderAccessoryRows = (accessories: AccessoryOption[]): string =>
  accessories
    .map((item) => {
      const selected = draft!.accessoryCodes.includes(item.code);
      return `
        <label class="data-row ${selected ? "selected" : ""} ${item.enabled === false ? "disabled" : ""}" role="row" title="${escapeHtml(item.disabledReason ?? "")}">
          <span><input type="checkbox" data-accessory="${escapeHtml(item.code)}" ${selected ? "checked" : ""} ${item.locked || item.enabled === false ? "disabled" : ""}/></span>
          <strong>${escapeHtml(item.code)}</strong>
          <span>${escapeHtml(item.name)}</span>
          <span>${escapeHtml(item.category)}</span>
          <span><b class="installation-symbol ${item.installation.toLowerCase()}"></b>${escapeHtml(relationInstallationLabel(item.installation))}</span>
        </label>`;
    })
    .join("");

const documentCard = (
  title: string,
  description: string,
  format: string,
  iconName: string,
  documentType: "commercial-sheet" | "dimensional-drawing" | "installation-manual" | "step-model" | null,
  available: boolean,
): string => `
  <article class="document-card ${available ? "" : "disabled"}">
    <span>${icon(iconName, 24)}</span>
    <div><h2>${escapeHtml(title)}</h2><p>${escapeHtml(description)}</p><small>${format}${available ? ` · ${escapeHtml(messages().ui.documents.offlineAvailable)}` : ""}</small></div>
    <button class="icon-button bordered" ${documentType ? `data-document="${documentType}"` : ""} title="${escapeHtml(messages().ui.documents.openDocument)}" ${available ? "" : "disabled"}>${icon("external-link")}</button>
  </article>`;

const summaryMetric = (label: string, value: string): string => `<div><span>${label}</span><strong>${value}</strong></div>`;

const optionSummary = (label: string, value: string, enabled: boolean): string => `
  <div class="${enabled ? "enabled" : ""}"><span>${enabled ? icon("check", 14) : icon("minus", 14)}</span><div><small>${label}</small><strong>${value}</strong></div></div>`;

const installationLabel = (mode: SelectionDraft["installationMode"]): string =>
  messages().domain.installation[mode];

const relationInstallationLabel = (installation: string): string =>
  installation.toLowerCase() === "internal"
    ? messages().domain.installation.internal
    : messages().domain.installation.external;

const currentDimensionalDrawingKey = (): string =>
  `${draft?.selectedUnitId ?? ""}|${draft?.layoutCode ?? ""}`;

const ensureDimensionalDrawing = async (): Promise<void> => {
  if (!draft || (currentStep !== "installation" && currentStep !== "documents")) return;
  const key = currentDimensionalDrawingKey();
  if (!key || dimensionalDrawingKey === key) return;

  dimensionalDrawingKey = key;
  dimensionalDrawing = null;
  dimensionalDrawingLoading = true;
  dimensionalPreviewImage = "";
  dimensionalLargeImage = "";
  dimensionalRenderVersion++;
  renderShell();
  try {
    const resolved = await bridge.getDimensionalDrawing(structuredClone(draft));
    if (key !== currentDimensionalDrawingKey()) return;
    dimensionalDrawing = resolved;
  } catch (error) {
    if (key === currentDimensionalDrawingKey()) {
      logClientError(error);
      toastMessage = error instanceof Error ? error.message : String(error);
    }
  } finally {
    if (key === currentDimensionalDrawingKey()) {
      dimensionalDrawingLoading = false;
      renderShell();
    }
  }
};

const renderDimensionalCanvases = async (): Promise<void> => {
  const drawing = dimensionalDrawing;
  if (!drawing?.available || !drawing.contentBase64) return;
  const canvases = Array.from(
    document.querySelectorAll<HTMLCanvasElement>("[data-dimensional-canvas]"),
  );
  if (canvases.length === 0) return;

  const version = ++dimensionalRenderVersion;
  try {
    const pdfjs = await import("pdfjs-dist");
    pdfjs.GlobalWorkerOptions.workerSrc = pdfWorkerUrl;
    const binary = window.atob(drawing.contentBase64);
    const bytes = Uint8Array.from(binary, (character) => character.charCodeAt(0));
    const pdf = await pdfjs.getDocument({ data: bytes }).promise;
    const page = await pdf.getPage(1);
    const baseViewport = page.getViewport({ scale: 1 });
    for (const canvas of canvases) {
      if (!canvas.isConnected || version !== dimensionalRenderVersion) return;
      const large = canvas.dataset.dimensionalCanvas === "large";
      const maximumWidth = Math.max(
        320,
        Math.min(large ? 1320 : 760, canvas.parentElement?.clientWidth ?? 760),
      );
      const maximumHeight = large ? 760 : 210;
      const cssScale = Math.min(
        maximumWidth / baseViewport.width,
        maximumHeight / baseViewport.height,
      );
      const pixelRatio = Math.min(2, window.devicePixelRatio || 1);
      const viewport = page.getViewport({
        scale: cssScale * pixelRatio,
      });
      canvas.width = Math.ceil(viewport.width);
      canvas.height = Math.ceil(viewport.height);
      canvas.style.width = `${Math.round(viewport.width / pixelRatio)}px`;
      canvas.style.height = `${Math.round(viewport.height / pixelRatio)}px`;
      const context = canvas.getContext("2d", { alpha: false });
      if (!context) continue;
      await page.render({ canvas, canvasContext: context, viewport }).promise;
      const rendered = canvas.toDataURL("image/png");
      if (large) dimensionalLargeImage = rendered;
      else dimensionalPreviewImage = rendered;
    }
    await pdf.destroy();
  } catch (error) {
    logClientError(error);
  }
};

const bindShellEvents = (): void => {
  document
    .querySelector<HTMLDetailsElement>(".additional-selection")
    ?.addEventListener("toggle", (event) => {
      additionalCriteriaOpen = (event.currentTarget as HTMLDetailsElement).open;
    });

  document.querySelectorAll<HTMLElement>("[data-step]").forEach((element) => {
    element.addEventListener("click", async () => {
      const step = element.dataset.step as StepId;
      if (calculating || (calculationFailed && stepIndex(step) > 1) || !canNavigateToStep(step)) return;
      if (
        currentStep === "installation" &&
        stepIndex(step) > stepIndex(currentStep)
      ) {
        confirmInstallationReview();
      }
      markSkippedOptionalSteps(currentStep, step);
      currentStep = step;
      renderShell();
      if (step === "documents") await refreshProductDocuments();
    });
  });

  document.querySelectorAll<HTMLElement>("[data-select-unit]").forEach((element) => {
    element.addEventListener("click", async () => {
      const selectedUnitId = element.dataset.selectUnit ?? draft!.selectedUnitId;
      const modelChanged =
        confirmedUnitId !== null && confirmedUnitId !== selectedUnitId;
      draft!.selectedUnitId = selectedUnitId;
      const unit = selectedUnit();
      if (unit) draft!.regulationPercent = unit.requiredRegulation;
      if (!await recalculate() || draft!.selectedUnitId !== selectedUnitId) return;
      confirmedUnitId = selectedUnitId;
      if (modelChanged) {
        installationReviewRequired = true;
        resetOptionalStepVisits();
      }
      unlockConfiguredWorkflow();
      currentStep = "installation";
      renderShell();
    });
  });

  document.querySelectorAll<HTMLElement>("[data-installation]").forEach((element) => {
    element.addEventListener("click", async () => {
      if (element.hasAttribute("disabled")) return;
      const mode = element.dataset.installation as InstallationMode;
      const configuration = preferredLayout(mode);
      if (!configuration) return;
      draft!.installationMode = mode;
      draft!.layoutCode = configuration.code;
      confirmInstallationReview();
      await recalculate();
    });
  });

  document.querySelectorAll<HTMLInputElement | HTMLSelectElement>("[data-field]").forEach((element) => {
    element.addEventListener("input", () => {
      const field = element.dataset.field ?? "";
      if (
        field !== "operatingPoint.supplyAirflow" &&
        field !== "operatingPoint.extractAirflow"
      ) {
        return;
      }
      applyFieldValue(field, element);
      const pairedField =
        field === "operatingPoint.supplyAirflow"
          ? "operatingPoint.extractAirflow"
          : "operatingPoint.supplyAirflow";
      const pairedInput = document.querySelector<HTMLInputElement>(
        `[data-field="${pairedField}"]`,
      );
      if (pairedInput) {
        pairedInput.value = String(
          field === "operatingPoint.supplyAirflow"
            ? draft!.operatingPoint.extractAirflow
            : draft!.operatingPoint.supplyAirflow,
        );
      }
    });
    element.addEventListener("change", async () => {
      const field = element.dataset.field ?? "";
      if (
        field === "waterCoilCustomized" &&
        element.value === "true" &&
        !draft!.waterCoilCustomDisclaimerAccepted
      ) {
        if (!window.confirm(result!.waterCoilCustomDisclaimer ?? "")) {
          element.value = "false";
          return;
        }
        draft!.waterCoilCustomDisclaimerAccepted = true;
      }
      applyFieldValue(field, element);
      if (field === "layoutCode") confirmInstallationReview();
      if (currentStep === "preselection") {
        await refreshPreselection();
      } else {
        await recalculate();
      }
    });
  });

  const bindAccessoryRows = (): void => {
    document.querySelectorAll<HTMLInputElement>("[data-accessory]").forEach((element) => {
      element.addEventListener("change", async () => {
        const code = element.dataset.accessory ?? "";
        draft!.accessoryCodes = element.checked
          ? [...new Set([...draft!.accessoryCodes, code])]
          : draft!.accessoryCodes.filter((value) => value !== code);
        await recalculate();
      });
    });
  };
  bindAccessoryRows();

  const search = document.querySelector<HTMLInputElement>("[data-accessory-search]");
  const category = document.querySelector<HTMLSelectElement>("[data-accessory-category]");
  const filterAccessories = (): void => {
    const query = search?.value.trim().toLowerCase() ?? "";
    const allCategories = messages().ui.accessories.allCategories;
    const selectedCategory = category?.value ?? allCategories;
    const filtered = data!.accessories.filter(
      (item) =>
        (selectedCategory === allCategories || item.category === selectedCategory) &&
        (!query || `${item.code} ${item.name}`.toLowerCase().includes(query)),
    );
    const rows = document.querySelector<HTMLElement>("[data-accessory-rows]");
    if (rows) {
      rows.innerHTML = renderAccessoryRows(filtered);
      bindAccessoryRows();
      renderIcons();
    }
  };
  search?.addEventListener("input", filterAccessories);
  category?.addEventListener("change", filterAccessories);

  document.querySelector<HTMLElement>('[data-action="previous"]')?.addEventListener("click", () => navigate(-1));
  document.querySelector<HTMLElement>('[data-action="next"]')?.addEventListener("click", () => navigate(1));
  document.querySelector<HTMLElement>('[data-action="save"]')?.addEventListener("click", () => requestSaveDraft(false));
  document.querySelector<HTMLElement>('[data-action="save-as"]')?.addEventListener("click", () => requestSaveDraft(true));
  document.querySelector<HTMLSelectElement>("[data-save-language]")?.addEventListener("change", (event) => {
    saveLanguageChoice = (event.currentTarget as HTMLSelectElement).value;
  });
  document.querySelector<HTMLInputElement>("[data-save-language-remember]")?.addEventListener("change", (event) => {
    saveLanguageRemember = (event.currentTarget as HTMLInputElement).checked;
  });
  document.querySelectorAll<HTMLElement>('[data-action="close-save-language"]').forEach((element) => {
    element.addEventListener("click", (event) => {
      if (element.classList.contains("modal-backdrop") && event.target !== element) return;
      saveLanguagePromptOpen = false;
      renderShell();
    });
  });
  document.querySelector<HTMLElement>('[data-action="confirm-save-language"]')?.addEventListener("click", async () => {
    draft!.project.language = saveLanguageChoice;
    if (saveLanguageRemember) window.localStorage.setItem("ssw-next.save-language-prompt", "false");
    saveLanguagePromptOpen = false;
    renderShell();
    await saveDraft(saveLanguagePromptSaveAs);
  });
  document.querySelector<HTMLElement>('[data-action="open-selection"]')?.addEventListener("click", openDraft);
  document.querySelector<HTMLElement>('[data-action="report"]')?.addEventListener("click", generateReport);
  document.querySelector<HTMLElement>('[data-action="open-dimensional-drawing"]')?.addEventListener("click", async () => {
    await ensureDimensionalDrawing();
    if (!dimensionalDrawing?.available) return;
    dimensionalDrawingOpen = true;
    renderShell();
  });
  document.querySelectorAll<HTMLElement>('[data-action="close-dimensional-drawing"]').forEach((element) => {
    element.addEventListener("click", (event) => {
      if (element.classList.contains("modal-backdrop") && event.target !== element) return;
      dimensionalDrawingOpen = false;
      renderShell();
    });
  });
  document.querySelector<HTMLElement>('[data-action="download-dimensional-drawing"]')?.addEventListener("click", async () => {
    if (!dimensionalDrawing?.available) return;
    await renderDimensionalCanvases();
    const canvas = document.querySelector<HTMLCanvasElement>('[data-dimensional-canvas="large"]');
    const imageDataUrl = dimensionalLargeImage || canvas?.toDataURL("image/png") || dimensionalPreviewImage;
    if (!imageDataUrl) return;
    const response = await bridge.downloadDimensionalDrawing(
      structuredClone(draft!),
      imageDataUrl.replace(/^data:image\/png;base64,/, ""),
    );
    if (response.saved && response.fileName) showToast(response.fileName);
  });
  document.querySelectorAll<HTMLButtonElement>("[data-document]").forEach((button) => {
    button.addEventListener("click", async () => {
      const documentType = button.dataset.document as
        | "commercial-sheet"
        | "dimensional-drawing"
        | "installation-manual"
        | "step-model";
      if (documentType === "dimensional-drawing") {
        await ensureDimensionalDrawing();
        if (!dimensionalDrawing?.available) return;
        dimensionalDrawingOpen = true;
        renderShell();
        return;
      }
      busyMessage = documentBusyText().opening;
      renderShell();
      try {
        productDocuments = await bridge.openProductDocument(
          documentType,
          structuredClone(draft!),
        );
      } catch (error) {
        logClientError(error);
      } finally {
        busyMessage = null;
        renderShell();
      }
    });
  });
  document.querySelector<HTMLElement>('[data-action="project-new"]')?.addEventListener("click", newMultiProject);
  document.querySelector<HTMLElement>('[data-action="project-open"]')?.addEventListener("click", openMultiProject);
  document.querySelector<HTMLElement>('[data-action="project-save"]')?.addEventListener("click", () => saveMultiProject(false));
  document.querySelector<HTMLElement>('[data-action="project-save-as"]')?.addEventListener("click", () => saveMultiProject(true));
  document.querySelector<HTMLElement>('[data-action="project-add-current"]')?.addEventListener("click", () => addCurrentToMultiProject());
  document.querySelector<HTMLElement>('[data-action="project-add-new"]')?.addEventListener("click", () => addCurrentToMultiProject(true));
  document.querySelector<HTMLSelectElement>("[data-project-language]")?.addEventListener("change", async (event) => {
    const targetLanguage = (event.currentTarget as HTMLSelectElement).value;
    const targetLanguageName =
      languageOptions.find((option) => option.code === targetLanguage)?.name ??
      targetLanguage;
    pendingProjectLanguage = targetLanguage;
    busyMessage =
      `${messages().ui.project.documentLanguage}: ${targetLanguageName}`;
    calculating = true;
    renderShell();
    try {
      const updatedProject = await bridge.changeMultiProjectLanguage(
        draft!.project.name,
        targetLanguage,
      );
      if (
        normalizeLanguageCode(updatedProject.languageCode) !==
        normalizeLanguageCode(targetLanguage)
      ) {
        throw new Error(
          `Project document language was not updated to ${targetLanguage}.`,
        );
      }
      activeProjectDocumentLanguage = normalizeLanguageCode(targetLanguage);
      multiProjectState = updatedProject;
      syncDraftFromMultiProject();
      showToast(
        `${messages().ui.project.documentLanguage}: ${targetLanguageName}`,
      );
    } finally {
      pendingProjectLanguage = null;
      busyMessage = null;
      calculating = false;
      renderShell();
    }
  });
  document.querySelector<HTMLElement>('[data-action="project-email"]')?.addEventListener("click", () => {
    projectEmailOpen = true;
    renderShell();
  });
  document.querySelectorAll<HTMLElement>('[data-action="close-project-email"]').forEach((element) => {
    element.addEventListener("click", (event) => {
      if (element.classList.contains("modal-backdrop") && event.target !== element) return;
      projectEmailOpen = false;
      renderShell();
    });
  });
  document.querySelector<HTMLInputElement>("[data-project-email-schedule]")?.addEventListener("change", (event) => {
    projectEmailSchedule = (event.currentTarget as HTMLInputElement).checked;
    renderShell();
  });
  document.querySelector<HTMLInputElement>("[data-project-email-days]")?.addEventListener("change", (event) => {
    projectEmailDays = Math.max(1, Math.min(90, Number((event.currentTarget as HTMLInputElement).value || 7)));
  });
  document.querySelector<HTMLElement>('[data-action="send-project-email"]')?.addEventListener("click", sendMultiProjectEmail);
  document.querySelectorAll<HTMLElement>("[data-project-open]").forEach((button) => {
    button.addEventListener("click", () => openMultiProjectItem(button.dataset.projectOpen ?? ""));
  });
  document.querySelectorAll<HTMLElement>("[data-project-remove]").forEach((button) => {
    button.addEventListener("click", () => removeMultiProjectItem(button.dataset.projectRemove ?? ""));
  });
  document.querySelectorAll<HTMLElement>("[data-project-item]").forEach((row) => {
    row.addEventListener("dblclick", (event) => {
      if ((event.target as HTMLElement).closest("button")) return;
      void openMultiProjectItem(row.dataset.projectItem ?? "");
    });
  });
  document.querySelector<HTMLElement>('[data-action="notifications"]')?.addEventListener("click", async () => {
    notificationState = await bridge.listNotifications();
    notificationCenterOpen = true;
    renderShell();
  });
  document.querySelectorAll<HTMLElement>('[data-action="close-notifications"]').forEach((element) => {
    element.addEventListener("click", (event) => {
      if (element.classList.contains("modal-backdrop") && event.target !== element) return;
      notificationCenterOpen = false;
      renderShell();
    });
  });
  document.querySelectorAll<HTMLButtonElement>("[data-reminder-action]").forEach((button) => {
    button.addEventListener("click", async () => {
      const id = button.dataset.reminderId ?? "";
      const action = button.dataset.reminderAction as
        | "reschedule"
        | "succeeded"
        | "unsuccessful";
      const daysInput = document.querySelector<HTMLInputElement>(
        `[data-reminder-days="${id}"]`,
      );
      const days = Math.max(1, Math.min(90, Number(daysInput?.value ?? 7)));
      notificationState = await bridge.updateNotification(id, action, days);
      renderShell();
    });
  });
  document.querySelectorAll<HTMLElement>("[data-reminder-open]").forEach((row) => {
    row.addEventListener("dblclick", async (event) => {
      if ((event.target as HTMLElement).closest(".notification-actions")) return;
      const response = await bridge.openNotificationTarget(
        row.dataset.reminderOpen ?? "",
        structuredClone(draft!),
      );
      if (response.openedProject && response.multiProject) {
        multiProjectState = response.multiProject;
        activeProjectDocumentLanguage = normalizeLanguageCode(
          response.multiProject.languageCode,
        );
        draft!.project.name = response.multiProject.reference;
        notificationCenterOpen = false;
        currentStep = "project";
        renderShell();
        return;
      }
      if (!response.opened || !response.draft) return;
      draft = response.draft;
      projectState = response.project ?? null;
      projectDirty = false;
      notificationCenterOpen = false;
      if (!await recalculate()) return;
      restoreConfiguredWorkflow();
      currentStep = "project";
      renderShell();
    });
  });
  document.querySelector<HTMLElement>('[data-action="help"]')?.addEventListener("click", () => {
    helpOpen = true;
    renderShell();
  });
  document.querySelector<HTMLElement>('[data-action="release-info"]')?.addEventListener("click", () => {
    releaseInfoOpen = true;
    renderShell();
  });
  document.querySelectorAll<HTMLElement>('[data-action="close-release-info"]').forEach((element) => {
    element.addEventListener("click", (event) => {
      if (element.classList.contains("modal-backdrop") && event.target !== element) return;
      releaseInfoOpen = false;
      renderShell();
    });
  });
  document.querySelectorAll<HTMLElement>('[data-action="close-help"]').forEach((element) => {
    element.addEventListener("click", (event) => {
      if (
        element.classList.contains("modal-backdrop") &&
        event.target !== element
      ) return;
      helpOpen = false;
      renderShell();
    });
  });
  document.querySelector<HTMLInputElement>('[data-action="toggle-tooltips"]')?.addEventListener("change", (event) => {
    tooltipsEnabled = (event.currentTarget as HTMLInputElement).checked;
    window.localStorage.setItem(
      "ssw-next.help-tooltips",
      String(tooltipsEnabled),
    );
    renderShell();
  });
};

const applyFieldValue = (
  field: string,
  element: HTMLInputElement | HTMLSelectElement,
): void => {
  const value = element instanceof HTMLInputElement && element.type === "checkbox"
    ? element.checked
    : element instanceof HTMLInputElement && element.type === "number"
      ? Number(element.value)
      : element.value;

  const setters: Record<string, () => void> = {
    "project.name": () => { draft!.project.name = String(value); },
    "project.customerReference": () => { draft!.project.customerReference = String(value); },
    "project.language": () => { draft!.project.language = String(value); },
    "operatingPoint.supplyAirflow": () => {
      draft!.operatingPoint.supplyAirflow = Number(value);
      if (!draft!.imbalanceEnabled) {
        draft!.operatingPoint.extractAirflow = Number(value);
      }
    },
    "operatingPoint.extractAirflow": () => {
      draft!.operatingPoint.extractAirflow = Number(value);
      if (!draft!.imbalanceEnabled) {
        draft!.operatingPoint.supplyAirflow = Number(value);
      }
    },
    "operatingPoint.pressure": () => { draft!.operatingPoint.pressure = Number(value); },
    imbalanceEnabled: () => { draft!.imbalanceEnabled = Boolean(value); },
    regulationPercent: () => { draft!.regulationPercent = Number(value); },
    summerEnabled: () => { draft!.summerEnabled = Boolean(value); },
    winterOutdoorTemperature: () => { draft!.winterOutdoorTemperature = Number(value); },
    winterOutdoorRh: () => { draft!.winterOutdoorRh = Number(value); },
    winterReturnTemperature: () => { draft!.winterReturnTemperature = Number(value); },
    winterReturnRh: () => { draft!.winterReturnRh = Number(value); },
    summerOutdoorTemperature: () => { draft!.summerOutdoorTemperature = Number(value); },
    summerOutdoorRh: () => { draft!.summerOutdoorRh = Number(value); },
    summerReturnTemperature: () => { draft!.summerReturnTemperature = Number(value); },
    summerReturnRh: () => { draft!.summerReturnRh = Number(value); },
    layoutCode: () => { draft!.layoutCode = String(value); },
    waterCoilEnabled: () => {
      draft!.waterCoilEnabled = Boolean(value);
      if (draft!.waterCoilEnabled && draft!.electricPostheaterEnabled) {
        draft!.waterCoilMode = "CWD";
      }
    },
    waterCoilMode: () => { draft!.waterCoilMode = String(value) as SelectionDraft["waterCoilMode"]; },
    waterCoilId: () => {
      draft!.waterCoilId = Number(value);
      const coil = result?.waterCoils?.find((item) => item.id === draft!.waterCoilId);
      if (coil) {
        draft!.waterCoilLengthMm = coil.lengthMm;
        draft!.waterCoilHeightMm = coil.heightMm;
        draft!.waterCoilRows = coil.rows;
        draft!.waterCoilCircuits = coil.circuits;
        draft!.waterCoilFinSpacingMm = coil.finSpacingMm;
      }
    },
    waterCoilCustomized: () => {
      draft!.waterCoilCustomized = String(value) === "true";
      if (!draft!.waterCoilCustomized) {
        draft!.waterCoilCustomDisclaimerAccepted = false;
      }
    },
    waterCoilLengthMm: () => { draft!.waterCoilLengthMm = Math.max(1, Number(value)); },
    waterCoilHeightMm: () => { draft!.waterCoilHeightMm = Math.max(1, Number(value)); },
    waterCoilRows: () => { draft!.waterCoilRows = Math.max(1, Math.round(Number(value))); },
    waterCoilCircuits: () => { draft!.waterCoilCircuits = Math.max(1, Math.round(Number(value))); },
    waterCoilFinSpacingMm: () => { draft!.waterCoilFinSpacingMm = Number(value); },
    fluidCode: () => { draft!.fluidCode = String(value) as SelectionDraft["fluidCode"]; },
    glycolPercent: () => { draft!.glycolPercent = Number(value); },
    coolingWaterInletTemperature: () => { draft!.coolingWaterInletTemperature = Number(value); },
    coolingWaterOutletTemperature: () => {
      draft!.coolingWaterOutletTemperature = Math.max(
        draft!.coolingWaterInletTemperature + 1,
        Number(value),
      );
    },
    heatingWaterInletTemperature: () => { draft!.heatingWaterInletTemperature = Number(value); },
    heatingWaterOutletTemperature: () => {
      draft!.heatingWaterOutletTemperature = Math.min(
        draft!.heatingWaterInletTemperature - 1,
        Number(value),
      );
    },
    electricPreheaterEnabled: () => { draft!.electricPreheaterEnabled = Boolean(value); },
    electricPreheaterId: () => { draft!.electricPreheaterId = Number(value); },
    electricPostheaterEnabled: () => { draft!.electricPostheaterEnabled = Boolean(value); },
    electricPostheaterId: () => { draft!.electricPostheaterId = Number(value); },
    "co2.includeInReport": () => { draft!.co2.includeInReport = Boolean(value); },
    "co2.roomWidthMeters": () => { draft!.co2.roomWidthMeters = Math.max(0.1, Number(value)); },
    "co2.roomLengthMeters": () => { draft!.co2.roomLengthMeters = Math.max(0.1, Number(value)); },
    "co2.roomHeightMeters": () => { draft!.co2.roomHeightMeters = Math.max(0.1, Number(value)); },
    "co2.activityMet": () => { draft!.co2.activityMet = Math.max(0.1, Number(value)); },
    "co2.occupiedPeople": () => { draft!.co2.occupiedPeople = Math.max(0, Math.round(Number(value))); },
    "co2.occupiedMinutes": () => { draft!.co2.occupiedMinutes = Math.max(0, Number(value)); },
    "co2.breakPeople": () => { draft!.co2.breakPeople = Math.max(0, Math.round(Number(value))); },
    "co2.breakMinutes": () => { draft!.co2.breakMinutes = Math.max(0, Number(value)); },
    "co2.calculationMethod": () => {
      draft!.co2.calculationMethod = String(value) as SelectionDraft["co2"]["calculationMethod"];
    },
    "co2.outdoorConcentrationPpm": () => {
      draft!.co2.outdoorConcentrationPpm = Math.max(0, Number(value));
    },
    "co2.maximumConcentrationPpm": () => {
      draft!.co2.maximumConcentrationPpm = Math.max(0, Number(value));
    },
    "co2.airflowPerAreaLitersPerSecondPerSquareMeter": () => {
      draft!.co2.airflowPerAreaLitersPerSecondPerSquareMeter = Math.max(0, Number(value));
    },
    "co2.airflowPerPersonLitersPerSecond": () => {
      draft!.co2.airflowPerPersonLitersPerSecond = Math.max(0, Number(value));
    },
    "sound.includeInReport": () => { draft!.sound.includeInReport = Boolean(value); },
    "preselectionFilters.rotaryOnlyEnabled": () => { draft!.preselectionFilters.rotaryOnlyEnabled = Boolean(value); },
    "preselectionFilters.maximumSfpEnabled": () => { draft!.preselectionFilters.maximumSfpEnabled = Boolean(value); },
    "preselectionFilters.maximumSfp": () => { draft!.preselectionFilters.maximumSfp = Math.max(0, Number(value)); },
    "preselectionFilters.supplyNoiseEnabled": () => { draft!.preselectionFilters.supplyNoiseEnabled = Boolean(value); },
    "preselectionFilters.supplyNoiseMetric": () => { draft!.preselectionFilters.supplyNoiseMetric = String(value) === "LPA" ? "LPA" : "LWA"; },
    "preselectionFilters.maximumSupplyNoiseDbA": () => { draft!.preselectionFilters.maximumSupplyNoiseDbA = Math.max(0, Number(value)); },
    "preselectionFilters.supplyNoiseDistanceMeters": () => { draft!.preselectionFilters.supplyNoiseDistanceMeters = Math.max(0.1, Number(value)); },
    "preselectionFilters.supplyNoiseDirectivityFactor": () => { draft!.preselectionFilters.supplyNoiseDirectivityFactor = Number(value) as 2 | 4 | 8; },
    "preselectionFilters.breakoutNoiseEnabled": () => { draft!.preselectionFilters.breakoutNoiseEnabled = Boolean(value); },
    "preselectionFilters.breakoutNoiseMetric": () => { draft!.preselectionFilters.breakoutNoiseMetric = String(value) === "LPA" ? "LPA" : "LWA"; },
    "preselectionFilters.maximumBreakoutNoiseDbA": () => { draft!.preselectionFilters.maximumBreakoutNoiseDbA = Math.max(0, Number(value)); },
    "preselectionFilters.breakoutNoiseDistanceMeters": () => { draft!.preselectionFilters.breakoutNoiseDistanceMeters = Math.max(0.1, Number(value)); },
    "preselectionFilters.breakoutNoiseDirectivityFactor": () => { draft!.preselectionFilters.breakoutNoiseDirectivityFactor = Number(value) as 2 | 4 | 8; },
    "sound.directivityFactor": () => {
      const factor = Number(value);
      draft!.sound.directivityFactor = factor === 4 || factor === 8 ? factor : 2;
    },
    "sound.distance1Meters": () => {
      draft!.sound.distance1Meters = Math.max(0.1, Number(value));
    },
    "sound.distance2Meters": () => {
      draft!.sound.distance2Meters = Math.max(0.1, Number(value));
    },
    "sound.iso16032Enabled": () => { draft!.sound.iso16032Enabled = Boolean(value); },
  };
  if (setters[field]) {
    setters[field]();
    projectDirty = true;
  }
};

const navigate = async (offset: number): Promise<void> => {
  const currentIndex = stepIndex(currentStep);
  const next = steps[Math.max(0, Math.min(steps.length - 1, currentIndex + offset))];
  if (!canNavigateToStep(next.id)) return;
  if (offset > 0 && currentStep === "installation") {
    confirmInstallationReview();
  }
  markSkippedOptionalSteps(currentStep, next.id);
  currentStep = next.id;
  renderShell();
  if (next.id === "documents") await refreshProductDocuments();
};

const refreshProductDocuments = async (): Promise<void> => {
  productDocuments = null;
  productDocumentsLoading = true;
  busyMessage = documentBusyText().checking;
  renderShell();
  try {
    productDocuments = await bridge.getProductDocuments(
      structuredClone(draft!),
    );
  } catch (error) {
    logClientError(error);
    productDocuments = {
      commercialSheetAvailable: false,
      installationManualAvailable: false,
      stepModelAvailable: false,
    };
  } finally {
    productDocumentsLoading = false;
    busyMessage = null;
  }
  await ensureDimensionalDrawing();
  renderShell();
};

const refreshPreselection = async (): Promise<void> => {
  const requestVersion = ++preselectionRequestVersion;
  const calculationVersion = ++calculationRequestVersion;
  const original = JSON.stringify(draft);
  const previousConfirmedUnitId = confirmedUnitId;
  const isCurrent = () => requestVersion === preselectionRequestVersion && JSON.stringify(draft) === original;
  calculating = true;
  calculationFailed = false;
  renderShell();
  try {
    const refreshedUnits = await bridge.preselect(structuredClone(draft!));
    if (!isCurrent()) return;
    data!.units = refreshedUnits;
    const current = refreshedUnits.find((unit) => unit.id === draft!.selectedUnitId) ?? refreshedUnits[0];
    if (!current) {
      draft!.selectedUnitId = "";
      confirmedUnitId = null;
      confirmInstallationReview();
      lockWorkflowAtPreselection();
      return;
    }
    draft!.selectedUnitId = current.id;
    if (previousConfirmedUnitId !== null && previousConfirmedUnitId !== current.id) {
      installationReviewRequired = true;
      resetOptionalStepVisits();
      confirmedUnitId = current.id;
    }
    draft!.regulationPercent = current.requiredRegulation;
    await recalculate();
  } catch (error) {
    if (isCurrent()) {
      calculationFailed = true;
      logClientError(error);
      toastMessage = String(error instanceof Error ? error.message : error);
    }
  } finally {
    if (requestVersion === preselectionRequestVersion && calculationVersion === calculationRequestVersion) {
      calculating = false;
      renderShell();
    }
  }
};

const recalculate = async (): Promise<boolean> => {
  const version = ++calculationRequestVersion;
  const original = JSON.stringify(draft);
  let effective = structuredClone(draft!);
  const isCurrent = () => version === calculationRequestVersion && JSON.stringify(draft) === original;
  calculating = true;
  calculationFailed = false;
  renderShell();
  try {
    for (let attempt = 0; attempt < 6; attempt++) {
      const calculated = await bridge.calculate(structuredClone(effective));
      if (!isCurrent()) return false;
      const normalized = normalizeSelection(effective, calculated);
      if (JSON.stringify(normalized) !== JSON.stringify(effective)) {
        effective = normalized;
        continue;
      }
      draft = effective;
      result = calculated;
      if (result.accessories && data) data.accessories = result.accessories;
      return true;
    }
    throw new Error("Selection normalization did not converge.");
  } catch (error) {
    if (isCurrent()) {
      calculationFailed = true;
      logClientError(error);
      toastMessage = String(error instanceof Error ? error.message : error);
    }
    return false;
  } finally {
    if (version === calculationRequestVersion) {
      calculating = false;
      renderShell();
    }
  }
};

const requestSaveDraft = (saveAs = false): void => {
  if (calculating || calculationFailed) return;
  if (window.localStorage.getItem("ssw-next.save-language-prompt") === "false") {
    void saveDraft(saveAs);
    return;
  }
  saveLanguagePromptSaveAs = saveAs;
  saveLanguageChoice = languageCode();
  saveLanguageRemember = false;
  saveLanguagePromptOpen = true;
  renderShell();
};

const saveDraft = async (saveAs = false): Promise<void> => {
  if (calculating || calculationFailed) return;
  const response = await bridge.saveDraft(structuredClone(draft!), saveAs);
  if (response.cancelled || !response.saved) return;
  projectState = response;
  projectDirty = false;
  showToast(`${messages().ui.toast.draftSavedAt} ${new Date(response.savedAt).toLocaleTimeString(messages().locale, { hour: "2-digit", minute: "2-digit" })}`);
};

const syncDraftFromMultiProject = (): void => {
  if (!multiProjectState?.loaded) return;
  draft!.project.name = multiProjectState.reference || draft!.project.name;
};

const projectDocumentLanguage = (): string =>
  activeProjectDocumentLanguage ||
  multiProjectState?.languageCode ||
  languageCode();

const newMultiProject = async (): Promise<void> => {
  multiProjectState = await bridge.newMultiProject(
    draft!.project.name,
    languageCode(),
  );
  activeProjectDocumentLanguage = normalizeLanguageCode(
    multiProjectState.languageCode || languageCode(),
  );
  syncDraftFromMultiProject();
  renderShell();
};

const openMultiProject = async (): Promise<void> => {
  const response = await bridge.openMultiProject();
  if (!response.opened || !response.project) return;
  multiProjectState = response.project;
  activeProjectDocumentLanguage = normalizeLanguageCode(
    response.project.languageCode,
  );
  syncDraftFromMultiProject();
  renderShell();
};

const saveMultiProject = async (saveAs = false): Promise<void> => {
  const response = await bridge.saveMultiProject(
    draft!.project.name,
    projectDocumentLanguage(),
    saveAs,
  );
  if (response.cancelled || !response.saved) return;
  multiProjectState = response.project;
  showToast(`${multiProjectText().saveProject}: ${response.project.fileName}`);
};

const addCurrentToMultiProject = async (requestedCreateNew?: boolean): Promise<void> => {
  if (calculating || calculationFailed) return;
  const previous = multiProjectState?.items.find((item) => item.current);
  const currentModel = selectedUnit()?.model.trim().toLocaleUpperCase() ?? "";
  const previousModel = previous?.unitName.trim().toLocaleUpperCase() ?? "";
  const createNew = requestedCreateNew ?? Boolean(previous && currentModel !== previousModel);
  const updating = !createNew && !!previous;
  const reference = window.prompt(multiProjectText().reference, draft!.project.customerReference);
  if (reference === null || !reference.trim()) return;
  const selection = structuredClone(draft!);
  selection.project.customerReference = reference.trim();
  if (updating && !window.confirm(`${projectActionLabels(languageCode())[1]}?\n${previous.unitName} · ${previous.customerReference}\n→ ${selectedUnit()?.model ?? ""} · ${reference.trim()}`)) return;
  calculating = true;
  renderShell();
  try {
    multiProjectState = await bridge.addCurrentToMultiProject(
      selection,
      createNew,
    );
    syncDraftFromMultiProject();
    const current = multiProjectState.items.find((item) => item.current);
    window.alert(`${multiProjectText().ready}: ${projectActionLabels(languageCode())[updating ? 1 : 0]}\n${current?.unitName ?? ""} · ${current?.customerReference ?? reference}\n${multiProjectText().workspace}: ${multiProjectState.items.length}`);
  } finally {
    calculating = false;
    renderShell();
  }
};

const openMultiProjectItem = async (itemId: string): Promise<void> => {
  if (!itemId) return;
  const response = await bridge.openMultiProjectItem(
    itemId,
    structuredClone(draft!),
  );
  if (!response.opened || !response.draft) return;
  draft = response.draft;
  projectState = response.selection ?? null;
  if (response.project) multiProjectState = response.project;
  projectDirty = false;
  if (!await recalculate()) return;
  restoreConfiguredWorkflow();
  currentStep = "project";
  renderShell();
};

const removeMultiProjectItem = async (itemId: string): Promise<void> => {
  if (!itemId || !window.confirm(multiProjectText().removeConfirm)) return;
  multiProjectState = await bridge.removeMultiProjectItem(itemId);
  renderShell();
};

const sendMultiProjectEmail = async (): Promise<void> => {
  const response = await bridge.emailMultiProject(
    draft!.project.name,
    projectDocumentLanguage(),
    projectEmailSchedule,
    projectEmailDays,
  );
  if (!response.prepared) return;
  if (response.project) {
    const requestedLanguage = projectDocumentLanguage();
    if (
      normalizeLanguageCode(response.project.languageCode) !==
      normalizeLanguageCode(requestedLanguage)
    ) {
      throw new Error(
        `Project email language was not prepared in ${requestedLanguage}.`,
      );
    }
    multiProjectState = response.project;
    activeProjectDocumentLanguage =
      normalizeLanguageCode(requestedLanguage);
  }
  projectEmailOpen = false;
  notificationState = await bridge.getNotificationSummary();
  showToast(multiProjectText().prepareEmail);
};

const openDraft = async (): Promise<void> => {
  const response = await bridge.openDraft(structuredClone(draft!));
  if (!response.opened || !response.draft) return;
  draft = response.draft;
  projectState = response.project ?? null;
  projectDirty = false;
  if (!await recalculate()) return;
  restoreConfiguredWorkflow();
  currentStep = "project";
  renderShell();
};

const generateReport = async (): Promise<void> => {
  if (calculating || calculationFailed) return;
  const response = await bridge.generateReport(
    structuredClone(draft!),
    projectDocumentLanguage(),
  );
  if (response.delegated) {
    showToast(messages().ui.toast.completeReport);
    return;
  }
  showToast(`${messages().ui.toast.reportReady}: ${response.fileName}`);
};

const showToast = (message: string): void => {
  toastMessage = message;
  renderShell();
  window.setTimeout(() => {
    toastMessage = "";
    renderShell();
  }, 2800);
};

window.addEventListener("hashchange", renderShell);

const bootstrap = async (): Promise<void> => {
  renderLoading();
  try {
    data = await bridge.bootstrap();
    draft = structuredClone(data.draft);
    result = structuredClone(data.result);
    confirmedUnitId = null;
    confirmInstallationReview();
    visitedSteps.clear();
    visitedSteps.add("project");
    skippedOptionalSteps.clear();
    lockWorkflowAtPreselection();
    multiProjectState = await bridge.getMultiProject(
      draft.project.name,
      languageCode(),
    );
    activeProjectDocumentLanguage = normalizeLanguageCode(
      multiProjectState.languageCode || languageCode(),
    );
    notificationState = await bridge.getNotificationSummary();
    renderShell();
  } catch (error) {
    logClientError(error);
    renderStartupError(error);
  }
};

void bootstrap();
