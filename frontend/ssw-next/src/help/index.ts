import { normalizeLanguageCode, type LanguageCode } from "../i18n";
import { enHelp } from "./en";
import { localizedHelp } from "./locales";
import type { HelpContent } from "./types";

export type { HelpContent, HelpTopic } from "./types";
export { enHelp } from "./en";

export const getHelpContent = (
  value?: string | null,
): Readonly<HelpContent> => {
  const code: LanguageCode = normalizeLanguageCode(value);
  return code === "en" ? enHelp : localizedHelp[code];
};
