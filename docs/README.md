# Storico tecnico SSW

Questa cartella e' lo storico condiviso del progetto, versionato nello stesso
repository del codice. Consultare questo indice e i documenti dell'area
interessata all'inizio di ogni nuova sessione, anche usando altre chat o modelli.

## Proposte e decisioni

- [Pubblicazione installer 2.0.0.25, 06/10/2026](RELEASE_2_0_0_25_2026-10-06.md)

- [Archivio offerte e utente licenziato, 06/10/2026](OFFER_ARCHIVE_AND_LICENSE_OWNER_2026-10-06.md)

- [Pubblicazione installer 2.0.0.24, 05/10/2026](RELEASE_2_0_0_24_2026-10-05.md)

- [Test installer 2.x e candidato 2.0.0.24, 05/10/2026](INSTALLER_2X_ACCEPTANCE_2026-10-05.md)
- [Inventario dipendenze legacy, 05/10/2026](LEGACY_DEPENDENCY_INVENTORY_2026-10-05.md)

- [Abbandono graduale della linea SSW 1.x, 05/10/2026](LEGACY_RETIREMENT_ROADMAP.md)

- [Verifica disponibilita documenti online per lingua, 05/10/2026](DOCUMENT_ONLINE_COVERAGE_2026-10-05.md)
- [Verifica ciclo offerte e promemoria, 05/10/2026](OFFER_STATUS_2026-10-05.md)

- [Protezione software, licenze dispositivo e catalogo: piano operativo, 30/09/2026](SOFTWARE_PROTECTION_ROADMAP.md)
- [Layout: simboli aeraulici originali e legenda, 09/09/2026](LAYOUT_AIRFLOW_SYMBOLS_2026-09-09.md)
- [Layout: correzioni flussi e disegno, 09/09/2026](LAYOUT_FLOW_CORRECTIONS_2026-09-09.md)
- [Audit e correzioni della selezione Next, 08/09/2026](AUDIT_CORREZIONI_2026-09-08.md)
- [Drawing dimensionale: proposta e contratto dati](DRAWING_DIMENSIONALE_PROPOSTA.md)
- [Reporting Ecodesign 1253/2014: dati, provenienza e regole di emissione](ECODESIGN_1253_REPORTING_PROPOSAL.md)
- [Installazione e configurazioni](INSTALLATION_LAYOUT_ROADMAP.md)
- [Accessori e funzioni](ACCESSORIES_CONTROL_FUNCTIONS_ROADMAP.md)
- [Separazione UI e backend](UI_BACKEND_SEPARATION_ROADMAP.md)
- [Selezione tecnica](TECHNICAL_SELECTION_ROADMAP.md)
- [Progetti multiselezione](MULTI_SELECTION_PROJECT_ROADMAP.md)
- [Licenze account e dispositivi](DEVICE_LICENSING_ROADMAP.md)

## Contratti e verifiche

- [Ripristino e verifica avvio SSW, 05/10/2026](STARTUP_RECOVERY_2026-10-05.md)
- [Coil regulation catalog audit, 01/10/2026](COIL_CATALOG_AUDIT_2026-10-01.md)
- [Portate sbilanciate](UNBALANCED_AIRFLOW_CONTRACT.md)
- [Versionamento](TECHNICAL_SELECTION_VERSIONING.md)
- [Matrice di test](TECHNICAL_SELECTION_TEST_MATRIX.md)
- [Promemoria commerciali e follow-up](FOLLOW_UP_REMINDERS_ROADMAP.md)
- [Runbook di rilascio](TECHNICAL_SELECTION_RELEASE_RUNBOOK.md)

## Regole di aggiornamento

Per ogni problema significativo o nuova proposta registrare:

1. Data, richiesta originale e perimetro.
2. Stato: proposta, approvata, in corso, implementata, verificata o superata.
3. Problema e prove riproducibili, con riferimenti a file e test del repository.
4. Decisione, alternative scartate e motivazione.
5. Soluzione effettivamente applicata, compatibilita' e migrazioni necessarie.
6. Verifiche eseguite, esito e limiti: build non equivale a test funzionale.
7. Punti aperti e prossimo passo.

Non riscrivere una proposta come se fosse gia' implementata. Aggiungere
checkpoint datati e collegare le decisioni successive che la sostituiscono.
Prima di usare vecchie evidenze, verificarne l'attualita' nel codice e nei dati.
Non inserire password, token, dati clienti o documenti riservati nello storico.

Documenti e riferimenti devono usare percorsi relativi al repository, non
dipendere da cartelle temporanee o dalla memoria di una chat. Ogni nuovo
documento va aggiunto a questo indice e incluso nel commit pertinente.
Sincronizzare tramite il normale remote Git; non copiare manualmente lo
storico tra checkout e non includere modifiche estranee nel medesimo commit.
