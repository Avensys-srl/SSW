import { en } from "./en";
import { locales } from "./locales";
import {
  supportedLanguageCodes,
  type FrontendMessages,
  type LanguageCode,
  type LocalizedFrontendMessages,
} from "./types";
import { domainMessages, uiMessages } from "./ui";

export { en } from "./en";
export type {
  FrontendMessages,
  DomainMessages,
  LanguageCode,
  LocalizedFrontendMessages,
  StepId,
  StepMessages,
  UiMessages,
} from "./types";
export { stepIds, supportedLanguageCodes } from "./types";

const aliases: Readonly<Record<string, LanguageCode>> = {
  nb: "no",
  nn: "no",
};

export const normalizeLanguageCode = (
  value: string | null | undefined,
): LanguageCode => {
  const candidate = (value ?? "en").trim().toLowerCase().split(/[-_]/u)[0] ?? "en";
  const aliased = aliases[candidate] ?? candidate;
  return supportedLanguageCodes.includes(aliased as LanguageCode)
    ? (aliased as LanguageCode)
    : "en";
};

export const getMessages = (
  value?: string | null,
): Readonly<LocalizedFrontendMessages> => {
  const code = normalizeLanguageCode(value);
  const base: FrontendMessages = code === "en" ? en : locales[code];
  return {
    ...base,
    domain: domainMessages[code],
    ui: uiMessages[code],
  };
};

export const languageOptions = supportedLanguageCodes.map((code) => {
  const messages = getMessages(code);
  return {
    code,
    name: messages.languageName,
    locale: messages.locale,
  };
});

export const resolveBrowserLanguage = (
  languages: readonly string[] = globalThis.navigator?.languages ?? [],
): LanguageCode => normalizeLanguageCode(languages[0]);
