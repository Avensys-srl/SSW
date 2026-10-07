const labels: Record<string, readonly [string, string, string]> = {
  it: ["Aggiungi questa unità al progetto", "Aggiorna questa unità nel progetto", "Crea email con i PDF del progetto"],
  en: ["Add this unit to the project", "Update this unit in the project", "Create email with project PDFs"],
  de: ["Dieses Gerät zum Projekt hinzufügen", "Dieses Gerät im Projekt aktualisieren", "E-Mail mit Projekt-PDFs erstellen"],
  fr: ["Ajouter cette unité au projet", "Mettre à jour cette unité dans le projet", "Créer un e-mail avec les PDF du projet"],
  bg: ["Добави този модул към проекта", "Обнови този модул в проекта", "Създай имейл с PDF на проекта"],
  cs: ["Přidat tuto jednotku do projektu", "Aktualizovat tuto jednotku v projektu", "Vytvořit e-mail s PDF projektu"],
  da: ["Tilføj denne enhed til projektet", "Opdater denne enhed i projektet", "Opret e-mail med projektets PDF-filer"],
  hu: ["Egység hozzáadása a projekthez", "Egység frissítése a projektben", "E-mail létrehozása a projekt PDF-jeivel"],
  is: ["Bæta þessari einingu við verkefnið", "Uppfæra þessa einingu í verkefninu", "Búa til tölvupóst með PDF verkefnisins"],
  nl: ["Deze unit aan het project toevoegen", "Deze unit in het project bijwerken", "E-mail met project-PDF's maken"],
  no: ["Legg denne enheten til prosjektet", "Oppdater denne enheten i prosjektet", "Opprett e-post med prosjektets PDF-filer"],
  pl: ["Dodaj tę jednostkę do projektu", "Aktualizuj tę jednostkę w projekcie", "Utwórz e-mail z PDF projektu"],
  ro: ["Adaugă această unitate în proiect", "Actualizează această unitate în proiect", "Creează e-mail cu PDF-urile proiectului"],
  sl: ["Dodaj to enoto v projekt", "Posodobi to enoto v projektu", "Ustvari e-pošto s PDF-ji projekta"],
  sv: ["Lägg till detta aggregat i projektet", "Uppdatera detta aggregat i projektet", "Skapa e-post med projektets PDF-filer"],
};
export const projectActionLabels = (language: string) => labels[language] ?? labels.en;
