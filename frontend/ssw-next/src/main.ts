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
import "./styles.css";
import { createBridge, runtimeName } from "./bridge";
import type {
  AccessoryOption,
  BootstrapData,
  SelectionDraft,
  SelectionResult,
  StepId,
  UnitOption,
} from "./bridge/contracts";

type StepDefinition = {
  id: StepId;
  label: string;
  shortLabel: string;
  optional?: boolean;
};

const steps: StepDefinition[] = [
  { id: "project", label: "Progetto", shortLabel: "Progetto" },
  { id: "preselection", label: "Preselezione", shortLabel: "Ricerca" },
  { id: "unit", label: "Unità", shortLabel: "Unità" },
  { id: "installation", label: "Installazione", shortLabel: "Layout" },
  { id: "water-coil", label: "Batterie acqua", shortLabel: "Acqua", optional: true },
  {
    id: "electric-heaters",
    label: "Batterie elettriche",
    shortLabel: "Elettriche",
    optional: true,
  },
  { id: "accessories", label: "Accessori", shortLabel: "Accessori", optional: true },
  { id: "documents", label: "Documenti", shortLabel: "Documenti", optional: true },
  { id: "summary", label: "Riepilogo", shortLabel: "Riepilogo" },
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
let calculating = false;
let toastMessage = "";

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

const selectedUnit = (): UnitOption | undefined =>
  data?.units.find((unit) => unit.id === draft?.selectedUnitId);

const stateTone = (): string => result?.status ?? "valid";

const formatNumber = (value: number, maximumFractionDigits = 1): string =>
  new Intl.NumberFormat("it-IT", { maximumFractionDigits }).format(value);

const renderLoading = (): void => {
  app.innerHTML = `
    <main class="loading-screen" aria-live="polite">
      <div class="brand-mark" aria-hidden="true">
        <span></span><span></span><span></span>
      </div>
      <div class="loading-copy">
        <strong>SSW Next</strong>
        <span>Preparazione dell'ambiente tecnico locale...</span>
      </div>
      <div class="loading-line"><span></span></div>
    </main>
  `;
};

const renderShell = (): void => {
  if (!data || !draft || !result) {
    return;
  }

  const route = window.location.hash || "#/selection";
  if (route.startsWith("#/showcase")) {
    renderShowcase();
    return;
  }

  const unit = selectedUnit();
  app.innerHTML = `
    <div class="app-shell">
      <header class="topbar">
        <a class="brand" href="#/selection" aria-label="SSW Next">
          <span class="brand-mark small" aria-hidden="true">
            <span></span><span></span><span></span>
          </span>
          <span><strong>SSW</strong><small>Technical selection</small></span>
        </a>
        <div class="topbar-project">
          <span>Progetto attivo</span>
          <strong>${escapeHtml(draft.project.name)}</strong>
        </div>
        <div class="topbar-actions">
          <span class="runtime-badge">${icon("hard-drive", 15)} ${escapeHtml(runtimeName())}</span>
          <button class="icon-button" type="button" title="Centro notifiche" aria-label="Centro notifiche">
            ${icon("bell")}
            <span class="notification-dot"></span>
          </button>
          <button class="icon-button" type="button" title="Aiuto" aria-label="Aiuto">
            ${icon("circle-help")}
          </button>
        </div>
      </header>

      <div class="workspace">
        <aside class="step-rail" aria-label="Fasi della selezione">
          <div class="step-rail-heading">
            <span>Configurazione</span>
            <strong>${steps.findIndex((step) => step.id === currentStep) + 1} / ${steps.length}</strong>
          </div>
          <nav>
            ${steps
              .map((step, index) => {
                const currentIndex = steps.findIndex(
                  (candidate) => candidate.id === currentStep,
                );
                const status =
                  step.id === currentStep
                    ? "active"
                    : index < currentIndex
                      ? "complete"
                      : "";
                return `
                  <button class="step-button ${status}" data-step="${step.id}" type="button">
                    <span class="step-index">${
                      status === "complete" ? icon("check", 14) : index + 1
                    }</span>
                    <span>
                      <strong>${step.label}</strong>
                      ${step.optional ? "<small>Opzionale</small>" : ""}
                    </span>
                  </button>`;
              })
              .join("")}
          </nav>
          <a class="showcase-link" href="#/showcase">
            ${icon("panels-top-left")} Componenti UI
          </a>
        </aside>

        <main class="main-stage">
          <div class="page-heading">
            <div>
              <span class="eyebrow">Selezione tecnica guidata</span>
              <h1>${steps.find((step) => step.id === currentStep)?.label}</h1>
              <p>${stepDescription(currentStep)}</p>
            </div>
            <div class="calculation-state ${stateTone()}">
              <span>${icon(result.status === "valid" ? "circle-check" : "triangle-alert")}</span>
              <div>
                <small>Verifica tecnica</small>
                <strong>${statusLabel(result.status)}</strong>
              </div>
            </div>
          </div>

          <section class="step-content" aria-live="polite">
            ${renderStep(currentStep)}
          </section>

          <footer class="step-footer">
            <button class="button secondary" data-action="previous" type="button" ${
              currentStep === "project" ? "disabled" : ""
            }>
              ${icon("arrow-left")} Indietro
            </button>
            <div class="footer-meta">
              <span>${calculating ? `${icon("loader-circle")} Calcolo in corso` : "Dati salvati localmente"}</span>
            </div>
            ${
              currentStep === "summary"
                ? `<button class="button primary" data-action="report" type="button">
                    ${icon("file-down")} Genera report
                  </button>`
                : `<button class="button primary" data-action="next" type="button">
                    Continua ${icon("arrow-right")}
                  </button>`
            }
          </footer>
        </main>

        <aside class="context-panel">
          <div class="context-heading">
            <span>Selezione corrente</span>
            <button class="icon-button subtle" type="button" title="Salva bozza" data-action="save">
              ${icon("save")}
            </button>
          </div>
          <div class="unit-lockup">
            <div class="unit-visual">
              <span class="duct supply"></span>
              <span class="duct exhaust"></span>
              <div class="unit-body">${icon("wind", 25)}</div>
            </div>
            <div>
              <small>${escapeHtml(unit?.family ?? "-")}</small>
              <strong>${escapeHtml(unit?.model ?? "Nessuna unità")}</strong>
            </div>
          </div>
          ${renderKeyValues([
            ["Mandata", `${formatNumber(draft.operatingPoint.supplyAirflow, 0)} m³/h`],
            ["Ripresa", `${formatNumber(draft.operatingPoint.extractAirflow, 0)} m³/h`],
            ["Pressione", `${formatNumber(draft.operatingPoint.pressure, 0)} Pa`],
            ["Layout", draft.layoutCode],
          ])}
          <div class="context-divider"></div>
          <div class="metric-grid">
            ${metric("Rendimento", `${formatNumber(result.winterEfficiency)}%`, "trending-up")}
            ${metric("Margine", `${formatNumber(result.availablePressure, 0)} Pa`, "gauge")}
            ${metric("Potenza", `${formatNumber(result.absorbedPower, 0)} W`, "zap")}
            ${metric("SFP", `${formatNumber(result.sfp, 2)}`, "activity")}
          </div>
          <div class="selection-tags">
            <span>Configurazione</span>
            <div>
              ${draft.waterCoilEnabled ? `<b>H₂O ${draft.waterCoilMode}</b>` : ""}
              ${draft.electricPreheaterEnabled ? "<b>PEHD</b>" : ""}
              ${draft.electricPostheaterEnabled ? "<b>EHD</b>" : ""}
              ${draft.accessoryCodes.map((code) => `<b>${escapeHtml(code)}</b>`).join("")}
            </div>
          </div>
        </aside>
      </div>
      ${toastMessage ? `<div class="toast">${icon("circle-check")} ${escapeHtml(toastMessage)}</div>` : ""}
    </div>
  `;
  bindShellEvents();
  renderIcons();
};

const stepDescription = (step: StepId): string => {
  const descriptions: Record<StepId, string> = {
    project: "Identifica il lavoro e imposta lingua e riferimento del cliente.",
    preselection: "Definisci il punto di lavoro per restringere le unità compatibili.",
    unit: "Confronta le alternative tecniche e conferma il modello.",
    installation: "Scegli posizione e configurazione dei quattro flussi.",
    "water-coil": "Configura il trattamento ad acqua disponibile per l'unità.",
    "electric-heaters": "Aggiungi preriscaldo o post-riscaldo elettrico.",
    accessories: "Completa la selezione con controllo, sensori e comunicazione.",
    documents: "Verifica la documentazione tecnica disponibile offline.",
    summary: "Controlla la configurazione prima di generare il report.",
  };
  return descriptions[step];
};

const statusLabel = (status: SelectionResult["status"]): string =>
  status === "valid" ? "Configurazione valida" : status === "warning" ? "Da verificare" : "Non valida";

const renderStep = (step: StepId): string => {
  switch (step) {
    case "project":
      return renderProjectStep();
    case "preselection":
      return renderPreselectionStep();
    case "unit":
      return renderUnitStep();
    case "installation":
      return renderInstallationStep();
    case "water-coil":
      return renderWaterCoilStep();
    case "electric-heaters":
      return renderElectricStep();
    case "accessories":
      return renderAccessoriesStep();
    case "documents":
      return renderDocumentsStep();
    case "summary":
      return renderSummaryStep();
  }
};

const renderProjectStep = (): string => `
  <div class="content-grid two">
    <section class="panel">
      <div class="panel-heading">
        <span class="panel-icon">${icon("briefcase-business")}</span>
        <div><h2>Dati del progetto</h2><p>Usati nel file di selezione e nel report.</p></div>
      </div>
      <div class="form-grid">
        ${textField("Nome progetto", "project.name", draft!.project.name, "Progetto 01")}
        ${textField("Riferimento cliente", "project.customerReference", draft!.project.customerReference, "Riferimento libero")}
        ${selectField("Lingua documenti", "project.language", draft!.project.language, [
          "Italiano", "English", "Français", "Deutsch", "Svenska", "Norsk", "Íslenska",
        ])}
      </div>
    </section>
    <section class="panel quiet-panel">
      <div class="panel-heading">
        <span class="panel-icon">${icon("folder-check")}</span>
        <div><h2>Stato progetto</h2><p>La bozza resta disponibile anche offline.</p></div>
      </div>
      <div class="project-status-list">
        ${statusRow("Riferimento tecnico", "D-B4DB-000154", "Registrato")}
        ${statusRow("Revisione", "R01", "Corrente")}
        ${statusRow("Ultimo salvataggio", "Oggi, 10:42", "Locale")}
      </div>
    </section>
  </div>`;

const renderPreselectionStep = (): string => `
  <div class="content-grid split-main">
    <section class="panel">
      <div class="panel-heading compact">
        <div><h2>Punto di lavoro</h2><p>Inserisci i valori nominali dell'impianto.</p></div>
      </div>
      <div class="operating-point">
        ${numberField("Portata mandata", "operatingPoint.supplyAirflow", draft!.operatingPoint.supplyAirflow, "m³/h")}
        ${numberField("Portata ripresa", "operatingPoint.extractAirflow", draft!.operatingPoint.extractAirflow, "m³/h")}
        ${numberField("Pressione statica", "operatingPoint.pressure", draft!.operatingPoint.pressure, "Pa")}
      </div>
      <div class="inline-notice">
        ${icon("info")}
        <span>Le portate sono predisposte come valori indipendenti. In questa fase il motore tecnico esegue ancora il calcolo bilanciato.</span>
      </div>
    </section>
    <section class="panel">
      <div class="panel-heading compact">
        <div><h2>Risultato della ricerca</h2><p>${data!.units.length} unità compatibili, ordinate per aderenza.</p></div>
        <span class="score-badge">Fit tecnico</span>
      </div>
      <div class="ranked-list">
        ${data!.units.map((unit, index) => unitCard(unit, index === 0)).join("")}
      </div>
    </section>
  </div>`;

const renderUnitStep = (): string => `
  <div class="unit-comparison">
    ${data!.units
      .map((unit) => {
        const selected = unit.id === draft!.selectedUnitId;
        return `
          <article class="comparison-card ${selected ? "selected" : ""}" data-select-unit="${unit.id}">
            <div class="comparison-top">
              <span>${unit.family}</span>
              ${selected ? `<b>${icon("check")} Selezionata</b>` : `<b>${unit.fitScore >= 0 ? `${unit.fitScore}% fit` : "Catalogo"}</b>`}
            </div>
            <div class="product-silhouette">
              <span></span><span></span><span></span>
            </div>
            <h2>${unit.model}</h2>
            ${renderKeyValues([
              ["Portata massima", `${formatNumber(unit.maxAirflow, 0)} m³/h`],
              ["Pressione disponibile", `${formatNumber(unit.availablePressure, 0)} Pa`],
              ["Rendimento nominale", unit.efficiency > 0 ? `${formatNumber(unit.efficiency)}%` : "Dopo la selezione"],
              ["Potenza sonora", unit.soundPower > 0 ? `${formatNumber(unit.soundPower, 0)} dB(A)` : "Dopo la selezione"],
            ])}
            <button class="button ${selected ? "secondary" : "primary"} full" type="button">
              ${selected ? "Unità corrente" : "Scegli unità"}
            </button>
          </article>`;
      })
      .join("")}
  </div>`;

const renderInstallationStep = (): string => `
  <div class="content-grid installation-grid">
    <section class="panel">
      <div class="panel-heading compact">
        <div><h2>Tipo di installazione</h2><p>La posizione filtra i layout disponibili.</p></div>
      </div>
      <div class="segmented-cards">
        ${choiceCard("ceiling", "Soffitto", "Unità sospesa, accesso dal basso", "panel-top")}
        ${choiceCard("floor", "Pavimento", "Unità appoggiata, accesso frontale", "panel-bottom")}
        ${choiceCard("wall", "Parete", "Unità verticale, accesso laterale", "panel-left")}
      </div>
      <div class="field">
        <label for="layoutCode">Configurazione flussi</label>
        <select id="layoutCode" data-field="layoutCode">
          ${(result!.layoutCodes?.length ? result!.layoutCodes : ["B6", "A4", "T5"]).map((value) => `<option ${draft!.layoutCode === value ? "selected" : ""}>${value}</option>`).join("")}
        </select>
        <small>Default consigliato per l'installazione selezionata.</small>
      </div>
    </section>
    <section class="panel layout-preview">
      <div class="panel-heading compact">
        <div><h2>Orientamento flussi</h2><p>Anteprima informativa della configurazione ${draft!.layoutCode}.</p></div>
        <span class="outline-badge">Accesso inferiore</span>
      </div>
      <div class="airflow-diagram">
        <div class="flow flow-north fresh">${icon("arrow-down")}<span>Aria esterna</span></div>
        <div class="flow flow-north return">${icon("arrow-down")}<span>Ripresa</span></div>
        <div class="ahu-plan">
          <span class="core"></span>
          <strong>${selectedUnit()?.model}</strong>
          <small>Pannello di accesso</small>
        </div>
        <div class="flow flow-south exhaust">${icon("arrow-down")}<span>Espulsione</span></div>
        <div class="flow flow-south supply">${icon("arrow-down")}<span>Mandata</span></div>
      </div>
    </section>
  </div>`;

const renderWaterCoilStep = (): string => `
  <div class="content-grid two">
    <section class="panel">
      ${toggleHeading("Trattamento ad acqua", "waterCoilEnabled", draft!.waterCoilEnabled, "Abilita batteria")}
      <div class="${draft!.waterCoilEnabled ? "" : "disabled-section"}">
        <div class="form-grid">
          ${selectField("Modo di calcolo", "waterCoilMode", draft!.waterCoilMode, ["CWD", "HWD", "HCD"])}
          ${selectField("Installazione", "waterInstallation", "Interna", ["Interna", "Esterna"])}
          ${selectField("Batteria", "waterModel", "iHCD Serie A 06A", ["iHCD Serie A 06A", "CWD 033 - 043 - 053"])}
          ${selectField("Fluido", "fluid", "Acqua", ["Acqua", "Glicole etilico", "Glicole propilenico"])}
        </div>
      </div>
    </section>
    <section class="panel">
      <div class="panel-heading compact">
        <div><h2>Prestazioni previste</h2><p>Il calcolo della batteria resta disponibile nell'interfaccia produttiva.</p></div>
      </div>
      <div class="result-table">
        ${resultRow("Riscaldamento", "1.690 W", "68,9 °C", "5 Pa")}
        ${resultRow("Raffreddamento", "1.340 W", "17,1 °C", "6 Pa")}
      </div>
      <div class="mini-chart" aria-label="Curva perdita di carico dimostrativa">
        <svg viewBox="0 0 520 170" role="img">
          <path class="grid-line" d="M42 20V140M42 140H500M42 100H500M42 60H500"/>
          <path class="chart-line orange" d="M42 135 C155 126 250 105 330 76 C408 48 456 32 500 23"/>
          <circle class="chart-dot" cx="330" cy="76" r="5"/>
        </svg>
      </div>
    </section>
  </div>`;

const renderElectricStep = (): string => `
  <div class="content-grid two">
    ${heaterPanel("PEHD", "Preriscaldo elettrico", "electricPreheaterEnabled", draft!.electricPreheaterEnabled, "EH-0.9-230", "900 W", "16,9 °C")}
    ${heaterPanel("EHD", "Post-riscaldo elettrico", "electricPostheaterEnabled", draft!.electricPostheaterEnabled, "EH-0.75-230", "750 W", "24,5 °C")}
  </div>`;

const renderAccessoriesStep = (): string => `
  <section class="panel table-panel">
    <div class="table-toolbar">
      <div class="search-box">${icon("search")}<input type="search" placeholder="Cerca codice o descrizione" data-accessory-search /></div>
      <select aria-label="Categoria accessori" data-accessory-category>
        <option>Tutte le categorie</option>
        ${[...new Set(data!.accessories.map((item) => item.category))].map((value) => `<option>${value}</option>`).join("")}
      </select>
      <span>${draft!.accessoryCodes.length} selezionati</span>
    </div>
    <div class="data-table" role="table">
      <div class="data-row header" role="row">
        <span></span><span>Codice</span><span>Descrizione</span><span>Categoria</span><span>Installazione</span>
      </div>
      <div data-accessory-rows>
        ${renderAccessoryRows(data!.accessories)}
      </div>
    </div>
  </section>`;

const renderDocumentsStep = (): string => `
  <div class="content-grid documents-grid">
    ${documentCard("Scheda tecnica", "Dati prestazionali e configurazione", "PDF", "file-text")}
    ${documentCard("Manuale installazione", "Montaggio, collegamenti e manutenzione", "PDF", "book-open")}
    ${documentCard("Dichiarazione UE", "Conformità e norme applicabili", "PDF", "badge-check")}
    ${documentCard("Disegno dimensionale", "Ingombri e quote della configurazione", "PDF", "ruler")}
  </div>
  <div class="inline-notice success">
    ${icon("hard-drive-download")}
    <span>Tutti i documenti mostrati sono disponibili nel pacchetto locale dimostrativo.</span>
  </div>`;

const renderSummaryStep = (): string => {
  const unit = selectedUnit();
  return `
    <div class="summary-layout">
      <section class="summary-main">
        <div class="summary-banner ${stateTone()}">
          <span>${icon(result!.status === "valid" ? "badge-check" : "triangle-alert", 26)}</span>
          <div><strong>${statusLabel(result!.status)}</strong><p>${result!.messages[0] ?? "La selezione è pronta per la generazione del report tecnico."}</p></div>
        </div>
        <div class="summary-section">
          <div class="summary-section-heading"><h2>Configurazione</h2><button data-step="unit">Modifica</button></div>
          ${renderKeyValues([
            ["Progetto", draft!.project.name],
            ["Riferimento", draft!.project.customerReference || "-"],
            ["Unità", `${unit?.family} · ${unit?.model}`],
            ["Installazione", `${installationLabel(draft!.installationMode)} · ${draft!.layoutCode}`],
          ], "wide")}
        </div>
        <div class="summary-section">
          <div class="summary-section-heading"><h2>Punto di lavoro</h2><button data-step="preselection">Modifica</button></div>
          <div class="summary-metrics">
            ${summaryMetric("Portata mandata", `${formatNumber(draft!.operatingPoint.supplyAirflow, 0)} m³/h`)}
            ${summaryMetric("Portata ripresa", `${formatNumber(draft!.operatingPoint.extractAirflow, 0)} m³/h`)}
            ${summaryMetric("Pressione richiesta", `${formatNumber(draft!.operatingPoint.pressure, 0)} Pa`)}
            ${summaryMetric("Margine disponibile", `${formatNumber(result!.availablePressure, 0)} Pa`)}
          </div>
        </div>
        <div class="summary-section">
          <div class="summary-section-heading"><h2>Opzioni</h2><button data-step="accessories">Modifica</button></div>
          <div class="summary-options">
            ${optionSummary("Batteria acqua", draft!.waterCoilEnabled ? draft!.waterCoilMode : "Non selezionata", draft!.waterCoilEnabled)}
            ${optionSummary("Preriscaldo elettrico", draft!.electricPreheaterEnabled ? "PEHD" : "Non selezionato", draft!.electricPreheaterEnabled)}
            ${optionSummary("Post-riscaldo elettrico", draft!.electricPostheaterEnabled ? "EHD" : "Non selezionato", draft!.electricPostheaterEnabled)}
            ${optionSummary("Accessori", `${draft!.accessoryCodes.length} selezionati`, true)}
          </div>
        </div>
      </section>
      <aside class="summary-report">
        <div class="report-sheet">
          <div class="report-brand"><span class="brand-mark tiny"><span></span><span></span><span></span></span><strong>Avensys</strong></div>
          <div class="report-lines"><span></span><span></span><span></span></div>
          <div class="report-title"></div>
          <div class="report-table"><span></span><span></span><span></span><span></span></div>
          <div class="report-charts"><span></span><span></span></div>
        </div>
        <h2>Report tecnico</h2>
        <p>Il documento utilizzerà lingua e riferimento del progetto corrente.</p>
        <div class="report-file">${icon("file-text")} <span>${escapeHtml(unit?.model?.replaceAll(" ", "_"))}_Report_it.pdf</span></div>
      </aside>
    </div>`;
};

const renderShowcase = (): void => {
  app.innerHTML = `
    <main class="showcase">
      <header class="showcase-header">
        <div>
          <span class="eyebrow">Design system locale</span>
          <h1>Component showcase</h1>
          <p>Controlli e stati riutilizzabili per SSW Next.</p>
        </div>
        <a class="button secondary" href="#/selection">${icon("arrow-left")} Torna alla selezione</a>
      </header>
      <section class="showcase-section">
        <h2>Azioni</h2>
        <div class="showcase-row">
          <button class="button primary">${icon("check")} Primaria</button>
          <button class="button secondary">${icon("save")} Secondaria</button>
          <button class="button danger">${icon("trash-2")} Distruttiva</button>
          <button class="icon-button bordered">${icon("settings")}</button>
        </div>
      </section>
      <section class="showcase-section">
        <h2>Campi</h2>
        <div class="showcase-fields">
          ${textField("Testo", "demo", "Valore di esempio", "")}
          ${numberField("Portata", "flow", 300, "m³/h")}
          ${selectField("Modalità", "mode", "HCD", ["CWD", "HWD", "HCD"])}
          <label class="toggle"><input type="checkbox" checked /><span></span><b>Opzione attiva</b></label>
        </div>
      </section>
      <section class="showcase-section">
        <h2>Messaggi</h2>
        <div class="showcase-messages">
          <div class="summary-banner valid">${icon("circle-check")}<div><strong>Configurazione valida</strong><p>Tutti i controlli sono stati superati.</p></div></div>
          <div class="summary-banner warning">${icon("triangle-alert")}<div><strong>Da verificare</strong><p>Il margine di pressione è ridotto.</p></div></div>
          <div class="summary-banner invalid">${icon("circle-x")}<div><strong>Non valida</strong><p>Correggere il punto di lavoro.</p></div></div>
        </div>
      </section>
      <section class="showcase-section">
        <h2>Metriche e token</h2>
        <div class="metric-grid showcase-metrics">
          ${metric("Rendimento", "95%", "trending-up")}
          ${metric("Pressione", "206 Pa", "gauge")}
          ${metric("Potenza", "176 W", "zap")}
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

const selectField = (label: string, field: string, value: string, options: string[]): string => `
  <div class="field">
    <label>${label}</label>
    <select data-field="${field}">
      ${options.map((option) => `<option ${option === value ? "selected" : ""}>${escapeHtml(option)}</option>`).join("")}
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

const unitCard = (unit: UnitOption, recommended: boolean): string => `
  <article class="ranked-unit ${unit.id === draft!.selectedUnitId ? "selected" : ""}" data-select-unit="${unit.id}">
    <div class="ranked-unit-score"><strong>${unit.fitScore >= 0 ? unit.fitScore : "—"}</strong><span>${unit.fitScore >= 0 ? "%" : ""}</span></div>
    <div><small>${unit.family}</small><strong>${unit.model}</strong></div>
    <div class="ranked-spec"><span>${unit.maxAirflow} m³/h</span><span>${unit.efficiency > 0 ? `${unit.efficiency}%` : "Da calcolare"}</span><span>${unit.soundPower > 0 ? `${unit.soundPower} dB(A)` : "Da calcolare"}</span></div>
    ${recommended ? `<b class="recommended">${icon("sparkles", 14)} Consigliata</b>` : ""}
    ${unit.id === draft!.selectedUnitId ? icon("circle-check", 20) : icon("chevron-right", 20)}
  </article>`;

const choiceCard = (value: string, title: string, description: string, iconName: string): string => `
  <button class="choice-card ${draft!.installationMode === value ? "selected" : ""}" data-installation="${value}" type="button">
    <span>${icon(iconName, 22)}</span><strong>${title}</strong><small>${description}</small>
  </button>`;

const toggleHeading = (title: string, field: string, checked: boolean, label: string): string => `
  <div class="panel-heading toggle-heading">
    <div><h2>${title}</h2><p>Configurazione opzionale associata al modello.</p></div>
    <label class="toggle"><input type="checkbox" data-field="${field}" ${checked ? "checked" : ""}/><span></span><b>${label}</b></label>
  </div>`;

const resultRow = (mode: string, power: string, output: string, pressureDrop: string): string => `
  <div><strong>${mode}</strong><span><small>Potenza</small>${power}</span><span><small>Aria out</small>${output}</span><span><small>DP aria</small>${pressureDrop}</span></div>`;

const heaterPanel = (
  code: string,
  title: string,
  field: string,
  enabled: boolean,
  model: string,
  power: string,
  output: string,
): string => `
  <section class="panel">
    ${toggleHeading(`${code} · ${title}`, field, enabled, "Abilita calcolo")}
    <div class="${enabled ? "" : "disabled-section"}">
      ${renderKeyValues([
        ["Batteria elettrica", model],
        ["Installazione", "Interna"],
        ["Potenza", power],
        ["Tensione / fasi", "230 V / 1"],
        ["Temp. massima aria out", output],
        ["DP aria", "10 Pa"],
      ], "wide")}
    </div>
  </section>`;

const renderAccessoryRows = (accessories: AccessoryOption[]): string =>
  accessories
    .map((item) => {
      const selected = draft!.accessoryCodes.includes(item.code);
      return `
        <label class="data-row ${selected ? "selected" : ""}" role="row">
          <span><input type="checkbox" data-accessory="${escapeHtml(item.code)}" ${selected ? "checked" : ""} ${item.locked ? "disabled" : ""}/></span>
          <strong>${escapeHtml(item.code)}</strong>
          <span>${escapeHtml(item.name)}</span>
          <span>${escapeHtml(item.category)}</span>
          <span><b class="installation-symbol ${item.installation.toLowerCase()}"></b>${item.installation === "Internal" ? "Interna" : "Esterna"}</span>
        </label>`;
    })
    .join("");

const documentCard = (title: string, description: string, format: string, iconName: string): string => `
  <article class="document-card">
    <span>${icon(iconName, 24)}</span>
    <div><h2>${title}</h2><p>${description}</p><small>${format} · Disponibile offline</small></div>
    <button class="icon-button bordered" title="Apri documento">${icon("external-link")}</button>
  </article>`;

const summaryMetric = (label: string, value: string): string => `<div><span>${label}</span><strong>${value}</strong></div>`;

const optionSummary = (label: string, value: string, enabled: boolean): string => `
  <div class="${enabled ? "enabled" : ""}"><span>${enabled ? icon("check", 14) : icon("minus", 14)}</span><div><small>${label}</small><strong>${value}</strong></div></div>`;

const installationLabel = (mode: SelectionDraft["installationMode"]): string =>
  ({ ceiling: "Soffitto", floor: "Pavimento", wall: "Parete" })[mode];

const bindShellEvents = (): void => {
  document.querySelectorAll<HTMLElement>("[data-step]").forEach((element) => {
    element.addEventListener("click", () => {
      const step = element.dataset.step as StepId;
      currentStep = step;
      renderShell();
    });
  });

  document.querySelectorAll<HTMLElement>("[data-select-unit]").forEach((element) => {
    element.addEventListener("click", async () => {
      draft!.selectedUnitId = element.dataset.selectUnit ?? draft!.selectedUnitId;
      await recalculate();
    });
  });

  document.querySelectorAll<HTMLElement>("[data-installation]").forEach((element) => {
    element.addEventListener("click", () => {
      draft!.installationMode = element.dataset.installation as SelectionDraft["installationMode"];
      renderShell();
    });
  });

  document.querySelectorAll<HTMLInputElement | HTMLSelectElement>("[data-field]").forEach((element) => {
    element.addEventListener("change", async () => {
      applyFieldValue(element.dataset.field ?? "", element);
      await recalculate();
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
    const selectedCategory = category?.value ?? "Tutte le categorie";
    const filtered = data!.accessories.filter(
      (item) =>
        (selectedCategory === "Tutte le categorie" || item.category === selectedCategory) &&
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
  document.querySelector<HTMLElement>('[data-action="save"]')?.addEventListener("click", saveDraft);
  document.querySelector<HTMLElement>('[data-action="report"]')?.addEventListener("click", generateReport);
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
    "operatingPoint.supplyAirflow": () => { draft!.operatingPoint.supplyAirflow = Number(value); },
    "operatingPoint.extractAirflow": () => { draft!.operatingPoint.extractAirflow = Number(value); },
    "operatingPoint.pressure": () => { draft!.operatingPoint.pressure = Number(value); },
    layoutCode: () => { draft!.layoutCode = String(value); },
    waterCoilEnabled: () => { draft!.waterCoilEnabled = Boolean(value); },
    waterCoilMode: () => { draft!.waterCoilMode = String(value) as SelectionDraft["waterCoilMode"]; },
    electricPreheaterEnabled: () => { draft!.electricPreheaterEnabled = Boolean(value); },
    electricPostheaterEnabled: () => { draft!.electricPostheaterEnabled = Boolean(value); },
  };
  setters[field]?.();
};

const navigate = (offset: number): void => {
  const currentIndex = steps.findIndex((step) => step.id === currentStep);
  const next = steps[Math.max(0, Math.min(steps.length - 1, currentIndex + offset))];
  currentStep = next.id;
  renderShell();
};

const recalculate = async (): Promise<void> => {
  calculating = true;
  renderShell();
  result = await bridge.calculate(structuredClone(draft!));
  if (result.accessories && data) {
    data.accessories = result.accessories;
    const availableCodes = new Set(result.accessories.map((item) => item.code));
    draft!.accessoryCodes = draft!.accessoryCodes.filter((code) => availableCodes.has(code));
    for (const item of result.accessories) {
      if (item.included && !draft!.accessoryCodes.includes(item.code)) {
        draft!.accessoryCodes.push(item.code);
      }
    }
  }
  if (result.layoutCodes?.length && !result.layoutCodes.includes(draft!.layoutCode)) {
    draft!.layoutCode = result.layoutCodes[0];
  }
  calculating = false;
  renderShell();
};

const saveDraft = async (): Promise<void> => {
  const response = await bridge.saveDraft(structuredClone(draft!));
  if (response.delegated) {
    showToast("Completa il salvataggio nell'interfaccia produttiva aperta.");
    return;
  }
  showToast(`Bozza salvata alle ${new Date(response.savedAt).toLocaleTimeString("it-IT", { hour: "2-digit", minute: "2-digit" })}`);
};

const generateReport = async (): Promise<void> => {
  const response = await bridge.generateReport(structuredClone(draft!));
  if (response.delegated) {
    showToast("Completa la generazione del report nell'interfaccia produttiva aperta.");
    return;
  }
  showToast(`Report pronto: ${response.fileName}`);
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
  data = await bridge.bootstrap();
  draft = structuredClone(data.draft);
  result = structuredClone(data.result);
  renderShell();
};

void bootstrap();
