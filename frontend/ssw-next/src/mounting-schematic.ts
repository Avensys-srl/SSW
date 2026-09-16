const translations: Record<string, readonly string[]> = {
  en: ["Airflow view from the access-panel side", "Air drawn towards the unit", "Air discharged from the unit", "Panel access / viewing direction", "Optional SHK shelf kit", "Side opposite the view"],
  it: ["Vista dei flussi dal lato del pannello di accesso", "Aria entrante nell'unità", "Aria uscente dall'unità", "Accesso al pannello / direzione di osservazione", "Kit mensola SHK opzionale", "Lato opposto alla vista"],
  de: ["Luftstromansicht von der Zugangsseite", "In das Gerät eintretende Luft", "Aus dem Gerät austretende Luft", "Panelzugang / Blickrichtung", "Optionales SHK-Konsolenkit", "Der Ansicht abgewandte Seite"],
  fr: ["Vue des flux côté panneau d'accès", "Air entrant dans l'unité", "Air sortant de l'unité", "Accès au panneau / direction de vue", "Kit support SHK en option", "Côté opposé à la vue"],
  es: ["Vista de flujos desde el panel de acceso", "Aire entrante", "Aire saliente", "Acceso al panel / dirección de vista", "Kit soporte SHK opcional", "Lado opuesto a la vista"],
  bg: ["Изглед на потоците от страната на панела за достъп", "Входящ въздух", "Изходящ въздух", "Достъп до панела / посока на наблюдение", "Опционален комплект SHK", "Страна, противоположна на изгледа"],
  cs: ["Pohled na proudění ze strany přístupového panelu", "Vzduch vstupující do jednotky", "Vzduch vystupující z jednotky", "Přístup k panelu / směr pohledu", "Volitelná sada konzoly SHK", "Strana odvrácená od pohledu"],
  da: ["Luftstrømsvisning fra adgangspanelets side", "Luft ind i enheden", "Luft ud af enheden", "Paneladgang / synsretning", "Valgfrit SHK-hyldesæt", "Modsat side af visningen"],
  hu: ["Légáramlási nézet a hozzáférési panel felől", "Belépő levegő", "Kilépő levegő", "Panelhozzáférés / nézési irány", "Opcionális SHK konzolkészlet", "A nézettel ellentétes oldal"],
  is: ["Loftflæðissýn frá aðgangshlið", "Loft inn í einingu", "Loft út úr einingu", "Aðgangur / sjónarhorn", "SHK hillusett í boði", "Hlið gegnt sjónarhorninu"],
  nl: ["Luchtstroomaanzicht vanaf het toegangspaneel", "Lucht naar de unit", "Lucht uit de unit", "Paneeltoegang / kijkrichting", "Optionele SHK-steunset", "Zijde tegenover het aanzicht"],
  no: ["Luftstrøm sett fra tilgangspanelet", "Luft inn i enheten", "Luft ut av enheten", "Paneltilgang / synsretning", "Valgfritt SHK-hyllesett", "Motsatt side av visningen"],
  pl: ["Widok przepływów od strony panelu dostępu", "Powietrze wpływające", "Powietrze wypływające", "Dostęp do panelu / kierunek widoku", "Opcjonalny zestaw wspornika SHK", "Strona przeciwna do widoku"],
  ro: ["Vedere fluxuri dinspre panoul de acces", "Aer care intră în unitate", "Aer care iese din unitate", "Acces la panou / direcție de vizualizare", "Kit suport SHK opțional", "Partea opusă vederii"],
  sl: ["Pogled pretokov s strani dostopne plošče", "Zrak v enoto", "Zrak iz enote", "Dostop do plošče / smer pogleda", "Izbirni komplet police SHK", "Stran nasproti pogledu"],
  sv: ["Luftflödesvy från åtkomstpanelens sida", "Luft in i enheten", "Luft ut ur enheten", "Panelåtkomst / visningsriktning", "SHK-hyllsats som tillval", "Sidan mittemot vyn"],
};
export const schematicText = (language: string) => translations[language] ?? translations.en;
export const flowColors: Record<string, string> = { fresh: "#43A047", supply: "#008FD3", return: "#F2B800", exhaust: "#8B5A2B" };
export const flowCircle = (role: string, x: number, y: number, number?: number, rear = false): string => {
  const incoming = role === "fresh" || role === "return";
  const fill = incoming ? "white" : flowColors[role];
  const outline = rear
    ? `<circle cx="${x}" cy="${y}" r="18" fill="${fill}" stroke="white" stroke-width="5"/><circle cx="${x}" cy="${y}" r="18" fill="none" stroke="${flowColors[role]}" stroke-width="3" stroke-dasharray="7 5"/>`
    : `<circle cx="${x}" cy="${y}" r="18" fill="${fill}" stroke="${flowColors[role]}" stroke-width="3"/>`;
  return `${outline}${number === undefined ? "" : `<text x="${x}" y="${y + 5}" text-anchor="middle" font-family="Arial" font-size="14" font-weight="bold" fill="${incoming ? "#18232D" : "white"}">${number}</text>`}`;
};
// Geometry and palette recovered from download.svg; labels are rendered separately.
export const mountingSvg = (mode: string, surface: string, eastWest: boolean, upright = false, accessText = ""): string => {
  if (upright) {
    return `<svg viewBox="0 0 300 215" aria-hidden="true"><rect x="123.3335" y="25" width="53.333" height="170" fill="white" stroke="#91A0AE" stroke-width="2"/><path d="M95 205h110" fill="none" stroke="#D62828" stroke-width="7"/><g fill="none" stroke="#D62828" stroke-width="4" stroke-linecap="round" stroke-linejoin="round"><path d="M25 110h88.3335"/><path d="m101.3335 100 12 10-12 10"/></g><text x="69.16675" y="92" text-anchor="middle" font-family="Arial" font-size="11" fill="#18232D">${accessText}</text></svg>`;
  }
  const wall = mode === "wall";
  const support = wall ? '<path d="M82 35v140"/><path d="M82 70h18m-18 70h18" stroke-width="3"/>' : `<path d="M45 ${mode === "floor" ? 165 : 45}h210"/>${mode === "ceiling" ? '<path d="M85 45v20m130-20v20" stroke-width="3"/>' : ""}`;
  const unit = wall ? '<rect x="100" y="20" width="53.333" height="170"/>' : '<rect x="65" y="65" width="170" height="53.333"/>';
  const arrow = wall ? '<path d="M270 105H163.333"/><path d="m175.333 95-12 10 12 10"/>' : surface === "upper" ? '<path d="M150 15v40"/><path d="m140 43 10 12 10-12"/>' : '<path d="M150 200V128.333"/><path d="m140 140.333 10-12 10 12"/>';
  const rotated = wall && !eastWest;
  return `<svg viewBox="0 0 300 215" aria-hidden="true"><g${rotated ? ' transform="rotate(180 150 105)"' : ""}><g fill="none" stroke="#91A0AE" stroke-width="2">${unit}</g><g fill="none" stroke="#D62828" stroke-width="7">${support}</g><g fill="none" stroke="#D62828" stroke-width="4" stroke-linecap="round" stroke-linejoin="round">${arrow}</g></g></svg>`;
};
