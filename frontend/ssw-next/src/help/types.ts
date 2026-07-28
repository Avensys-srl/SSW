import type { LanguageCode, StepId } from "../i18n";

type HelpStepId = Exclude<StepId, "unit">;

export interface HelpTopic {
  title: string;
  summary: string;
  tip: string;
}
export interface HelpContent {
  language: LanguageCode;
  title: string;
  introduction: string;
  workflowNote: string;
  topics: Record<HelpStepId, HelpTopic>;
}
