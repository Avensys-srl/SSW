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
  SelectionDraft,
  SelectionResult,
  StepId,
  UnitOption,
} from "./bridge/contracts";

type StepDefinition = {
  id: StepId;
  optional?: boolean;
};

const steps: StepDefinition[] = [
  { id: "project" },
  { id: "preselection" },
  { id: "unit" },
  { id: "installation" },
  { id: "water-coil", optional: true },
  { id: "electric-heaters", optional: true },
  { id: "accessories", optional: true },
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
let helpOpen = false;
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
const helpContent = () => getHelpContent(languageCode());
const helpTitle = (key: keyof LocalizedFrontendMessages["tooltips"]): string =>
  tooltipsEnabled ? messages().tooltips[key] : "";

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
  new Intl.NumberFormat(messages().locale, { maximumFractionDigits }).format(value);

const renderLoading = (): void => {
  const text = messages();
  app.innerHTML = `
    <main class="loading-screen" aria-live="polite">
      <div class="brand-mark" aria-hidden="true">
        <span></span><span></span><span></span>
      </div>
      <div class="loading-copy">
        <strong>SSW Next</strong>
        <span>${escapeHtml(text.common.loading)}</span>
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
  const text = messages();
  const help = helpContent();
  app.innerHTML = `
    <div class="app-shell">
      <header class="topbar">
        <a class="brand" href="#/selection" aria-label="SSW Next">
          <span class="brand-mark small" aria-hidden="true">
            <span></span><span></span><span></span>
          </span>
          <span><strong>SSW</strong><small>${escapeHtml(text.common.technicalSelection)}</small></span>
        </a>
        <div class="topbar-project">
          <span>${escapeHtml(text.common.activeProject)}</span>
          <strong>${escapeHtml(draft.project.name)}</strong>
        </div>
        <div class="topbar-actions">
          <span class="runtime-badge">${icon("hard-drive", 15)} ${escapeHtml(runtimeName())}</span>
          <button class="icon-button" type="button" title="${escapeHtml(helpTitle("notifications"))}" aria-label="${escapeHtml(text.ui.aria.notifications)}">
            ${icon("bell")}
            <span class="notification-dot"></span>
          </button>
          <button class="icon-button" type="button" title="${escapeHtml(helpTitle("help"))}" aria-label="${escapeHtml(text.actions.openHelp)}" data-action="help">
            ${icon("circle-help")}
          </button>
        </div>
      </header>

      <div class="workspace">
        <aside class="step-rail" aria-label="${escapeHtml(text.ui.aria.selectionSteps)}">
          <div class="step-rail-heading">
            <span>${escapeHtml(text.common.configuration)}</span>
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
                      <strong>${escapeHtml(text.steps[step.id].shortTitle)}</strong>
                      ${step.optional ? `<small>${escapeHtml(text.common.optional)}</small>` : ""}
                    </span>
                  </button>`;
              })
              .join("")}
          </nav>
          <a class="showcase-link" href="#/showcase">
            ${icon("panels-top-left")} ${escapeHtml(text.ui.navigation.componentShowcase)}
          </a>
        </aside>

        <main class="main-stage">
          <div class="page-heading">
            <div>
              <span class="eyebrow">${escapeHtml(text.common.technicalSelection)}</span>
              <h1>${escapeHtml(text.steps[currentStep].title)}</h1>
              <p>${escapeHtml(text.steps[currentStep].description)}</p>
            </div>
            <div class="calculation-state ${stateTone()}">
              <span>${icon(result.status === "valid" ? "circle-check" : "triangle-alert")}</span>
              <div>
                <small>${escapeHtml(text.common.technicalSelection)}</small>
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
                : `<button class="button primary" data-action="next" type="button">
                    ${escapeHtml(text.actions.next)} ${icon("arrow-right")}
                  </button>`
            }
          </footer>
        </main>

        <aside class="context-panel">
          <div class="context-heading">
            <span>${escapeHtml(text.ui.navigation.currentSelection)}</span>
            <button class="icon-button subtle" type="button" title="${escapeHtml(text.ui.navigation.saveDraft)}" data-action="save">
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
              <strong>${escapeHtml(unit?.model ?? text.ui.context.noUnit)}</strong>
            </div>
          </div>
          ${renderKeyValues([
            [text.ui.context.supply, `${formatNumber(draft.operatingPoint.supplyAirflow, 0)} m³/h`],
            [text.ui.context.extract, `${formatNumber(draft.operatingPoint.extractAirflow, 0)} m³/h`],
            [text.ui.context.pressure, `${formatNumber(draft.operatingPoint.pressure, 0)} Pa`],
            [text.ui.context.layout, draft.layoutCode],
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
              ${draft.waterCoilEnabled ? `<b>H₂O ${draft.waterCoilMode}</b>` : ""}
              ${draft.electricPreheaterEnabled ? "<b>PEHD</b>" : ""}
              ${draft.electricPostheaterEnabled ? "<b>EHD</b>" : ""}
              ${draft.accessoryCodes.map((code) => `<b>${escapeHtml(code)}</b>`).join("")}
            </div>
          </div>
        </aside>
      </div>
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
              <h3>${escapeHtml(help.topics[currentStep].title)}</h3>
              <p>${escapeHtml(help.topics[currentStep].summary)}</p>
              <div class="inline-notice">${icon("info")}<span>${escapeHtml(help.topics[currentStep].tip)}</span></div>
            </div>
            <p class="help-workflow">${escapeHtml(help.workflowNote)}</p>
            <label class="toggle"><input type="checkbox" data-action="toggle-tooltips" ${tooltipsEnabled ? "checked" : ""}/><span></span><b>${escapeHtml(tooltipsEnabled ? text.actions.hideTooltips : text.actions.showTooltips)}</b></label>
          </section>
        </div>` : ""}
    </div>
  `;
  bindShellEvents();
  renderIcons();
};

const statusLabel = (status: SelectionResult["status"]): string =>
  status === "valid"
    ? messages().status.valid
    : status === "warning"
      ? messages().status.warning
      : messages().status.invalid;

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

const renderProjectStep = (): string => {
  const text = messages();
  return `<div class="content-grid two">
    <section class="panel">
      <div class="panel-heading">
        <span class="panel-icon">${icon("briefcase-business")}</span>
        <div><h2>${escapeHtml(text.ui.project.dataTitle)}</h2><p>${escapeHtml(text.ui.project.dataDescription)}</p></div>
      </div>
      <div class="form-grid">
        ${textField(text.ui.project.name, "project.name", draft!.project.name, text.ui.project.defaultName)}
        ${textField(text.ui.project.customerReference, "project.customerReference", draft!.project.customerReference, text.ui.project.referencePlaceholder)}
        <div class="field">
          <label>${escapeHtml(text.ui.project.documentLanguage)}</label>
          <select data-field="project.language">
            ${languageOptions.map((option) => `<option value="${option.code}" ${option.code === languageCode() ? "selected" : ""}>${escapeHtml(option.name)}</option>`).join("")}
          </select>
        </div>
      </div>
    </section>
    <section class="panel quiet-panel">
      <div class="panel-heading">
        <span class="panel-icon">${icon("folder-check")}</span>
        <div><h2>${escapeHtml(text.ui.project.statusTitle)}</h2><p>${escapeHtml(text.ui.project.statusDescription)}</p></div>
      </div>
      <div class="project-status-list">
        ${statusRow(text.ui.project.technicalReference, "D-B4DB-000154", text.ui.project.registered)}
        ${statusRow(text.ui.project.revision, "R01", text.ui.project.current)}
        ${statusRow(text.ui.project.lastSaved, `${text.ui.project.today}, 10:42`, text.ui.project.local)}
      </div>
    </section>
  </div>`;
};

const renderPreselectionStep = (): string => {
  const text = messages();
  return `<div class="content-grid split-main">
    <section class="panel">
      <div class="panel-heading compact">
        <div><h2>${escapeHtml(text.ui.preselection.dutyPoint)}</h2><p>${escapeHtml(text.ui.preselection.dutyPointDescription)}</p></div>
      </div>
      <div class="operating-point">
        ${numberField(text.ui.preselection.supplyAirflow, "operatingPoint.supplyAirflow", draft!.operatingPoint.supplyAirflow, "m³/h")}
        ${numberField(text.ui.preselection.extractAirflow, "operatingPoint.extractAirflow", draft!.operatingPoint.extractAirflow, "m³/h")}
        ${numberField(text.ui.preselection.staticPressure, "operatingPoint.pressure", draft!.operatingPoint.pressure, "Pa")}
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
        ${data!.units.map((unit, index) => unitCard(unit, index === 0)).join("")}
      </div>
    </section>
  </div>`;
};

const renderUnitStep = (): string => {
  const text = messages();
  return `<div class="unit-comparison">
    ${data!.units
      .map((unit) => {
        const selected = unit.id === draft!.selectedUnitId;
        return `
          <article class="comparison-card ${selected ? "selected" : ""}" data-select-unit="${unit.id}">
            <div class="comparison-top">
              <span>${unit.family}</span>
              ${selected ? `<b>${icon("check")} ${escapeHtml(text.ui.unit.selected)}</b>` : `<b>${unit.fitScore >= 0 ? `${unit.fitScore}% fit` : escapeHtml(text.ui.unit.catalogue)}</b>`}
            </div>
            <div class="product-silhouette">
              <span></span><span></span><span></span>
            </div>
            <h2>${unit.model}</h2>
            ${renderKeyValues([
              [text.ui.unit.maximumAirflow, `${formatNumber(unit.maxAirflow, 0)} m³/h`],
              [text.ui.unit.availablePressure, `${formatNumber(unit.availablePressure, 0)} Pa`],
              [text.ui.unit.nominalEfficiency, unit.efficiency > 0 ? `${formatNumber(unit.efficiency)}%` : text.ui.preselection.calculateAfterSelection],
              [text.ui.unit.soundPower, unit.soundPower > 0 ? `${formatNumber(unit.soundPower, 0)} dB(A)` : text.ui.preselection.calculateAfterSelection],
            ])}
            <button class="button ${selected ? "secondary" : "primary"} full" type="button">
              ${escapeHtml(selected ? text.ui.unit.currentUnit : text.ui.unit.chooseUnit)}
            </button>
          </article>`;
      })
      .join("")}
  </div>`;
};

const renderInstallationStep = (): string => {
  const text = messages();
  return `<div class="content-grid installation-grid">
    <section class="panel">
      <div class="panel-heading compact">
        <div><h2>${escapeHtml(text.ui.installation.typeTitle)}</h2><p>${escapeHtml(text.ui.installation.typeDescription)}</p></div>
      </div>
      <div class="segmented-cards">
        ${choiceCard("ceiling", text.domain.installation.ceiling, text.ui.installation.ceilingDescription, "panel-top")}
        ${choiceCard("floor", text.domain.installation.floor, text.ui.installation.floorDescription, "panel-bottom")}
        ${choiceCard("wall", text.domain.installation.wall, text.ui.installation.wallDescription, "panel-left")}
      </div>
      <div class="field">
        <label for="layoutCode">${escapeHtml(text.ui.installation.airflowConfiguration)}</label>
        <select id="layoutCode" data-field="layoutCode">
          ${(result!.layoutCodes?.length ? result!.layoutCodes : ["B6", "A4", "T5"]).map((value) => `<option ${draft!.layoutCode === value ? "selected" : ""}>${value}</option>`).join("")}
        </select>
        <small>${escapeHtml(text.ui.installation.defaultHint)}</small>
      </div>
    </section>
    <section class="panel layout-preview">
      <div class="panel-heading compact">
        <div><h2>${escapeHtml(text.ui.installation.orientationTitle)}</h2><p>${escapeHtml(text.ui.installation.previewDescription)} ${escapeHtml(draft!.layoutCode)}.</p></div>
        <span class="outline-badge">${escapeHtml(text.ui.installation.lowerAccess)}</span>
      </div>
      <div class="airflow-diagram">
        <div class="flow flow-north fresh">${icon("arrow-down")}<span>${escapeHtml(text.domain.airflow.fresh)}</span></div>
        <div class="flow flow-north return">${icon("arrow-down")}<span>${escapeHtml(text.domain.airflow.return)}</span></div>
        <div class="ahu-plan">
          <span class="core"></span>
          <strong>${selectedUnit()?.model}</strong>
          <small>${escapeHtml(text.ui.installation.accessPanel)}</small>
        </div>
        <div class="flow flow-south exhaust">${icon("arrow-down")}<span>${escapeHtml(text.domain.airflow.exhaust)}</span></div>
        <div class="flow flow-south supply">${icon("arrow-down")}<span>${escapeHtml(text.domain.airflow.supply)}</span></div>
      </div>
    </section>
  </div>`;
};

const renderWaterCoilStep = (): string => {
  const text = messages();
  const coils = result!.waterCoils ?? [];
  const selected = coils.find((item) => item.id === draft!.waterCoilId);
  const available = coils.length > 0;
  return `
    <div class="content-grid two">
      <section class="panel">
        ${toggleHeading(text.ui.waterCoil.treatment, "waterCoilEnabled", draft!.waterCoilEnabled && available, available ? text.ui.waterCoil.enable : text.status.unavailable)}
        <div class="${draft!.waterCoilEnabled && available ? "" : "disabled-section"}">
          <div class="form-grid">
            ${selectField(text.ui.waterCoil.calculationMode, "waterCoilMode", draft!.waterCoilMode, ["CWD", "HWD", "HCD"])}
            ${numberSelectField(text.ui.waterCoil.coil, "waterCoilId", draft!.waterCoilId, coils.map((item) => ({ value: item.id, label: `${item.name} · ${relationInstallationLabel(item.installation)}` })))}
            ${codedSelectField(text.ui.waterCoil.fluid, "fluidCode", draft!.fluidCode, [
              { value: "Water", label: text.domain.fluid.water },
              { value: "Glic_Etil", label: text.domain.fluid.ethyleneGlycol },
              { value: "Glic_Prop", label: text.domain.fluid.propyleneGlycol },
            ])}
            ${numberField(text.ui.waterCoil.glycol, "glycolPercent", draft!.glycolPercent, "%")}
            ${numberField(text.ui.waterCoil.coolingIn, "coolingWaterInletTemperature", draft!.coolingWaterInletTemperature, "°C")}
            ${numberField(text.ui.waterCoil.coolingOut, "coolingWaterOutletTemperature", draft!.coolingWaterOutletTemperature, "°C")}
            ${numberField(text.ui.waterCoil.heatingIn, "heatingWaterInletTemperature", draft!.heatingWaterInletTemperature, "°C")}
            ${numberField(text.ui.waterCoil.heatingOut, "heatingWaterOutletTemperature", draft!.heatingWaterOutletTemperature, "°C")}
          </div>
          ${selected ? `<div class="inline-notice">${icon("ruler")}<span>${selected.lengthMm} × ${selected.heightMm} mm · ${selected.rows} ${escapeHtml(text.ui.waterCoil.rows)} · ${selected.circuits} ${escapeHtml(text.ui.waterCoil.circuits)} · ${escapeHtml(text.ui.waterCoil.finSpacing)} ${formatNumber(selected.finSpacingMm, 1)} mm</span></div>` : ""}
        </div>
      </section>
      <section class="panel">
        <div class="panel-heading compact">
          <div><h2>${escapeHtml(text.ui.waterCoil.calculatedPerformance)}</h2><p>${escapeHtml(text.ui.waterCoil.calculatedDescription)}</p></div>
        </div>
        <div class="result-table">
          ${(result!.waterCoilResults?.length ?? 0) > 0
            ? result!.waterCoilResults!.map((item) =>
                resultRow(
                  item.mode,
                  `${formatNumber(item.capacityW, 0)} W`,
                  `${formatNumber(item.airOutletTemperatureC, 1)} °C · ${formatNumber(item.airOutletRelativeHumidityPercent, 0)}%`,
                  `${formatNumber(item.airPressureDropPa, 0)} Pa`,
                ),
              ).join("")
            : `<div class="empty-state">${escapeHtml(available ? text.ui.waterCoil.enableToCalculate : text.ui.waterCoil.noneAssociated)}</div>`}
        </div>
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
      ${heaterPanel("EHD", text.ui.electricHeater.postHeating, "electricPostheaterEnabled", draft!.electricPostheaterEnabled, "electricPostheaterId", heaters.filter((item) => item.mode === "EHD"))}
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

const renderDocumentsStep = (): string => {
  const text = messages();
  return `<div class="content-grid documents-grid">
    ${documentCard(text.ui.documents.technicalSheet, text.ui.documents.technicalSheetDescription, "PDF", "file-text")}
    ${documentCard(text.ui.documents.installationManual, text.ui.documents.installationManualDescription, "PDF", "book-open")}
    ${documentCard(text.ui.documents.euDeclaration, text.ui.documents.euDeclarationDescription, "PDF", "badge-check")}
    ${documentCard(text.ui.documents.dimensionalDrawing, text.ui.documents.dimensionalDrawingDescription, "PDF", "ruler")}
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
    <div class="summary-layout">
      <section class="summary-main">
        <div class="summary-banner ${stateTone()}">
          <span>${icon(result!.status === "valid" ? "badge-check" : "triangle-alert", 26)}</span>
          <div><strong>${statusLabel(result!.status)}</strong><p>${escapeHtml(result!.messages[0] ?? text.ui.summary.readyForReport)}</p></div>
        </div>
        <div class="summary-section">
          <div class="summary-section-heading"><h2>${escapeHtml(text.ui.summary.configuration)}</h2><button data-step="unit">${escapeHtml(text.actions.edit)}</button></div>
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
          </div>
        </div>
        <div class="summary-section">
          <div class="summary-section-heading"><h2>${escapeHtml(text.ui.summary.options)}</h2><button data-step="accessories">${escapeHtml(text.actions.edit)}</button></div>
          <div class="summary-options">
            ${optionSummary(text.ui.summary.waterCoil, draft!.waterCoilEnabled ? draft!.waterCoilMode : text.ui.summary.notSelectedFeminine, draft!.waterCoilEnabled)}
            ${optionSummary(text.ui.summary.electricPreheating, draft!.electricPreheaterEnabled ? "PEHD" : text.ui.summary.notSelectedMasculine, draft!.electricPreheaterEnabled)}
            ${optionSummary(text.ui.summary.electricPostHeating, draft!.electricPostheaterEnabled ? "EHD" : text.ui.summary.notSelectedMasculine, draft!.electricPostheaterEnabled)}
            ${optionSummary(text.ui.summary.accessories, `${draft!.accessoryCodes.length} ${text.ui.accessories.selectedCount}`, true)}
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
        <h2>${escapeHtml(text.ui.summary.technicalReport)}</h2>
        <p>${escapeHtml(text.ui.summary.reportDescription)}</p>
        <div class="report-file">${icon("file-text")} <span>${escapeHtml(unit?.model?.replaceAll(" ", "_"))}_Report_${languageCode()}.pdf</span></div>
      </aside>
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
  <div class="field compact-field">
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
): string => `
  <div class="field">
    <label>${escapeHtml(label)}</label>
    <select data-field="${field}">
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
  return `
  <article class="ranked-unit ${unit.id === draft!.selectedUnitId ? "selected" : ""}" data-select-unit="${unit.id}">
    <div class="ranked-unit-score"><strong>${unit.fitScore >= 0 ? unit.fitScore : "—"}</strong><span>${unit.fitScore >= 0 ? "%" : ""}</span></div>
    <div><small>${unit.family}</small><strong>${unit.model}</strong></div>
    <div class="ranked-spec"><span>${unit.maxAirflow} m³/h</span><span>${unit.efficiency > 0 ? `${unit.efficiency}%` : escapeHtml(text.ui.preselection.calculateAfterSelection)}</span><span>${unit.soundPower > 0 ? `${unit.soundPower} dB(A)` : escapeHtml(text.ui.preselection.calculateAfterSelection)}</span></div>
    ${recommended ? `<b class="recommended">${icon("sparkles", 14)} ${escapeHtml(text.ui.preselection.recommended)}</b>` : ""}
    ${unit.id === draft!.selectedUnitId ? icon("circle-check", 20) : icon("chevron-right", 20)}
  </article>`;
};

const choiceCard = (value: string, title: string, description: string, iconName: string): string => `
  <button class="choice-card ${draft!.installationMode === value ? "selected" : ""}" data-installation="${value}" type="button">
    <span>${icon(iconName, 22)}</span><strong>${title}</strong><small>${description}</small>
  </button>`;

const toggleHeading = (title: string, field: string, checked: boolean, label: string): string => `
  <div class="panel-heading toggle-heading">
    <div><h2>${escapeHtml(title)}</h2><p>${escapeHtml(messages().ui.electricHeater.optionalDescription)}</p></div>
    <label class="toggle"><input type="checkbox" data-field="${field}" ${checked ? "checked" : ""}/><span></span><b>${label}</b></label>
  </div>`;

const resultRow = (mode: string, power: string, output: string, pressureDrop: string): string => `
  <div><strong>${mode}</strong><span><small>${escapeHtml(messages().ui.waterCoil.capacity)}</small>${power}</span><span><small>${escapeHtml(messages().ui.waterCoil.airOut)}</small>${output}</span><span><small>${escapeHtml(messages().ui.waterCoil.airPressureDrop)}</small>${pressureDrop}</span></div>`;

const heaterPanel = (
  code: string,
  title: string,
  field: string,
  enabled: boolean,
  idField: string,
  heaters: NonNullable<SelectionResult["electricHeaters"]>,
): string => {
  const text = messages();
  const selectedId = code === "PEHD" ? draft!.electricPreheaterId : draft!.electricPostheaterId;
  const selected = heaters.find((item) => item.id === selectedId) ?? heaters[0];
  const performance = result!.electricHeaterResults?.find((item) => item.mode === code);
  return `
    <section class="panel">
      ${toggleHeading(`${code} · ${title}`, field, enabled && heaters.length > 0, heaters.length > 0 ? text.ui.electricHeater.enableCalculation : text.ui.electricHeater.unavailable)}
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
        <label class="data-row ${selected ? "selected" : ""}" role="row">
          <span><input type="checkbox" data-accessory="${escapeHtml(item.code)}" ${selected ? "checked" : ""} ${item.locked ? "disabled" : ""}/></span>
          <strong>${escapeHtml(item.code)}</strong>
          <span>${escapeHtml(item.name)}</span>
          <span>${escapeHtml(item.category)}</span>
          <span><b class="installation-symbol ${item.installation.toLowerCase()}"></b>${escapeHtml(relationInstallationLabel(item.installation))}</span>
        </label>`;
    })
    .join("");

const documentCard = (title: string, description: string, format: string, iconName: string): string => `
  <article class="document-card">
    <span>${icon(iconName, 24)}</span>
    <div><h2>${escapeHtml(title)}</h2><p>${escapeHtml(description)}</p><small>${format} · ${escapeHtml(messages().ui.documents.offlineAvailable)}</small></div>
    <button class="icon-button bordered" title="${escapeHtml(messages().ui.documents.openDocument)}">${icon("external-link")}</button>
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
  document.querySelector<HTMLElement>('[data-action="save"]')?.addEventListener("click", saveDraft);
  document.querySelector<HTMLElement>('[data-action="report"]')?.addEventListener("click", generateReport);
  document.querySelector<HTMLElement>('[data-action="help"]')?.addEventListener("click", () => {
    helpOpen = true;
    renderShell();
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
    "operatingPoint.supplyAirflow": () => { draft!.operatingPoint.supplyAirflow = Number(value); },
    "operatingPoint.extractAirflow": () => { draft!.operatingPoint.extractAirflow = Number(value); },
    "operatingPoint.pressure": () => { draft!.operatingPoint.pressure = Number(value); },
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
    waterCoilEnabled: () => { draft!.waterCoilEnabled = Boolean(value); },
    waterCoilMode: () => { draft!.waterCoilMode = String(value) as SelectionDraft["waterCoilMode"]; },
    waterCoilId: () => { draft!.waterCoilId = Number(value); },
    fluidCode: () => { draft!.fluidCode = String(value) as SelectionDraft["fluidCode"]; },
    glycolPercent: () => { draft!.glycolPercent = Number(value); },
    coolingWaterInletTemperature: () => { draft!.coolingWaterInletTemperature = Number(value); },
    coolingWaterOutletTemperature: () => { draft!.coolingWaterOutletTemperature = Number(value); },
    heatingWaterInletTemperature: () => { draft!.heatingWaterInletTemperature = Number(value); },
    heatingWaterOutletTemperature: () => { draft!.heatingWaterOutletTemperature = Number(value); },
    electricPreheaterEnabled: () => { draft!.electricPreheaterEnabled = Boolean(value); },
    electricPreheaterId: () => { draft!.electricPreheaterId = Number(value); },
    electricPostheaterEnabled: () => { draft!.electricPostheaterEnabled = Boolean(value); },
    electricPostheaterId: () => { draft!.electricPostheaterId = Number(value); },
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
  const waterCoils = result.waterCoils ?? [];
  if (!waterCoils.some((item) => item.id === draft!.waterCoilId)) {
    const first = waterCoils[0];
    draft!.waterCoilEnabled = false;
    draft!.waterCoilId = first?.id ?? 0;
    if (first) {
      draft!.waterCoilMode = first.mode;
      draft!.waterCoilLengthMm = first.lengthMm;
      draft!.waterCoilHeightMm = first.heightMm;
      draft!.waterCoilRows = first.rows;
      draft!.waterCoilCircuits = first.circuits;
      draft!.waterCoilFinSpacingMm = first.finSpacingMm;
    }
  }
  const heaters = result.electricHeaters ?? [];
  const preheaters = heaters.filter((item) => item.mode === "PEHD");
  const postheaters = heaters.filter((item) => item.mode === "EHD");
  if (!preheaters.some((item) => item.id === draft!.electricPreheaterId)) {
    draft!.electricPreheaterEnabled = false;
    draft!.electricPreheaterId =
      preheaters.find((item) => item.isDefault)?.id ?? preheaters[0]?.id ?? 0;
  }
  if (!postheaters.some((item) => item.id === draft!.electricPostheaterId)) {
    draft!.electricPostheaterEnabled = false;
    draft!.electricPostheaterId =
      postheaters.find((item) => item.isDefault)?.id ?? postheaters[0]?.id ?? 0;
  }
  calculating = false;
  renderShell();
};

const saveDraft = async (): Promise<void> => {
  const response = await bridge.saveDraft(structuredClone(draft!));
  if (response.delegated) {
    showToast(messages().ui.toast.completeSave);
    return;
  }
  showToast(`${messages().ui.toast.draftSavedAt} ${new Date(response.savedAt).toLocaleTimeString(messages().locale, { hour: "2-digit", minute: "2-digit" })}`);
};

const generateReport = async (): Promise<void> => {
  const response = await bridge.generateReport(structuredClone(draft!));
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
  data = await bridge.bootstrap();
  draft = structuredClone(data.draft);
  result = structuredClone(data.result);
  renderShell();
};

void bootstrap();
