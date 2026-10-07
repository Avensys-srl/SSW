const titles: Record<string, [string, string, string, string]> = {
  en: ["Exchanger operation", "Exchanger type", "Unit application", "Installation"],
  it: ["Funzionamento scambiatore", "Tipologia scambiatore", "Applicazione unità", "Installazione"],
  fr: ["Fonctionnement échangeur", "Type d’échangeur", "Application unité", "Installation"],
  de: ["Wärmetauscherbetrieb", "Wärmetauschertyp", "Geräteanwendung", "Installation"],
  bg: ["Работа на топлообменника", "Тип топлообменник", "Приложение на уреда", "Монтаж"],
  cs: ["Provoz výměníku", "Typ výměníku", "Použití jednotky", "Instalace"],
  da: ["Varmevekslerfunktion", "Varmevekslertype", "Enhedsanvendelse", "Installation"],
  hu: ["Hőcserélő működése", "Hőcserélő típusa", "Egység alkalmazása", "Telepítés"],
  is: ["Virkni varmaskiptis", "Gerð varmaskiptis", "Notkun einingar", "Uppsetning"],
  nl: ["Werking warmtewisselaar", "Type warmtewisselaar", "Toepassing unit", "Installatie"],
  no: ["Varmevekslerfunksjon", "Varmevekslertype", "Enhetsanvendelse", "Installasjon"],
  pl: ["Działanie wymiennika", "Typ wymiennika", "Zastosowanie jednostki", "Instalacja"],
  ro: ["Funcționare schimbător", "Tip schimbător", "Aplicație unitate", "Instalare"],
  sl: ["Delovanje izmenjevalnika", "Tip izmenjevalnika", "Uporaba enote", "Namestitev"],
  sv: ["Värmeväxlarfunktion", "Värmeväxlartyp", "Enhetstillämpning", "Installation"],
};
const environment: Record<string, [string, string, string]> = {
  en: ["Indoor", "Outdoor", "Both"], it: ["Indoor", "Outdoor", "Entrambe"],
  fr: ["Intérieur", "Extérieur", "Les deux"], de: ["Innen", "Außen", "Beides"],
  bg: ["На закрито", "На открито", "И двете"], cs: ["Vnitřní", "Venkovní", "Obojí"],
  da: ["Indendørs", "Udendørs", "Begge"], hu: ["Beltéri", "Kültéri", "Mindkettő"],
  is: ["Innandyra", "Utandyra", "Bæði"], nl: ["Binnen", "Buiten", "Beide"],
  no: ["Innendørs", "Utendørs", "Begge"], pl: ["Wewnątrz", "Na zewnątrz", "Oba"],
  ro: ["Interior", "Exterior", "Ambele"], sl: ["Notranja", "Zunanja", "Oboje"],
  sv: ["Inomhus", "Utomhus", "Båda"],
};
const reset: Record<string, string> = {
  en: "Clear additional filters", it: "Azzera filtri aggiuntivi",
  fr: "Réinitialiser les filtres supplémentaires", de: "Zusätzliche Filter zurücksetzen",
  bg: "Изчисти допълнителните филтри", cs: "Vymazat další filtry",
  da: "Nulstil ekstra filtre", hu: "További szűrők törlése",
  is: "Hreinsa viðbótarsíur", nl: "Aanvullende filters wissen",
  no: "Nullstill ekstra filtre", pl: "Wyczyść dodatkowe filtry",
  ro: "Resetează filtrele suplimentare", sl: "Ponastavi dodatne filtre",
  sv: "Återställ extra filter",
};
export const classificationLabels = (language: string) => ({
  titles: titles[language] ?? titles.en,
  environment: environment[language] ?? environment.en,
  reset: reset[language] ?? reset.en,
});
