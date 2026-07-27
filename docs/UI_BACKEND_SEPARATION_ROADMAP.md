# Separazione UI e backend e nuova esperienza SSW - Roadmap

Data decisione architetturale: 27/07/2026

## Obiettivo

Separare progressivamente l'orchestrazione tecnica di SSW dalla UI WinForms,
preservando algoritmi, librerie, database SDF, report RDLC, API, file progetto e
funzionamento offline.

La nuova UI deve seguire un percorso guidato e diretto, realizzato come
frontend locale HTML/CSS/TypeScript ospitato da WebView2 nell'applicazione
desktop Windows x86.

## Decisioni approvate

- Si continua nella repository GitHub `Avensys-srl/SSW`.
- Non si crea una copia del prodotto priva della storia Git.
- `master` resta pubblicabile; la modernizzazione procede su un branch
  dedicato.
- L'interfaccia WinForms corrente resta riferimento funzionale e fallback.
- Non vengono riscritti gli algoritmi durante la separazione.
- La nuova UI non introduce inizialmente prezzi, valuta, preventivi, CRM,
  login cliente o gestione commerciale.
- I contratti applicativi supportano subito mandata e ripresa distinte.
- La preselezione, il layout installativo e il controllo delle portate
  sbilanciate vengono realizzati direttamente nella nuova UI, evitando una
  implementazione WinForms destinata a essere rimossa.
- La linea WinForms corrente continua con versioni `1.3.0.xx`.
- La prima pubblicazione ordinaria con la nuova UI come esperienza predefinita
  ha versione `2.0.0.0`.

## Obiettivo release 2.0.0.0

Il passaggio di major identifica una differenza concettuale:

- da form tecnico monolitico a percorso guidato;
- da calcolo orchestrato dai controlli a servizi applicativi UI-neutral;
- da singolo frontend a contratti utilizzabili da desktop e futuro web;
- da portata implicitamente unica a modello predisposto per due rami;
- da navigazione a menu/tab a stato tecnico persistente tra i passi.

La versione non deve essere portata a `2.0.0.0` durante il semplice sviluppo
del contenitore WebView2. Le build preview:

- usano un canale separato;
- non sostituiscono la release pubblica `1.3.0.xx`;
- non vengono pubblicate nel manifest di aggiornamento ordinario;
- riportano una versione informativa preview identificabile.

Il primo rilascio pubblico `2.0.0.0` richiede:

1. nuova UI come entry point predefinito;
2. parita' funzionale dello scope tecnico approvato;
3. risultati bilanciati equivalenti alla baseline;
4. apertura e migrazione dei file storici;
5. salvataggio, report, email, follow-up e offline mode;
6. installer e aggiornamento dalla linea `1.3.0.xx`;
7. rollback verificato verso l'ultima release legacy;
8. help, tooltip e localizzazioni completi.

`SoftwareVersion = 2.0.0.0` non implica automaticamente nuove versioni di
`SelectionFormatVersion`, `DatabaseSchemaVersion`, `CalculationEngineVersion`,
`ReportTemplateVersion` o `ApiContractVersion`. Ciascun contratto viene
incrementato solamente quando cambia il proprio contenuto.

## Flusso tecnico di destinazione

1. Progetto tecnico.
2. Preselezione guidata dei modelli.
3. Selezione dettagliata e prestazioni.
4. Configurazione installativa e dimensionale.
5. Batterie ad acqua.
6. Batterie elettriche.
7. Accessori e funzioni.
8. Documenti tecnici.
9. Riepilogo, email e follow-up.

## Onda 0 - Baseline e protezione regressioni

Stato: **completata il 27/07/2026**.

1. [x] Selezionare fixture `.sswsel` rappresentative.
2. [x] Acquisire risultati numerici, serie dei grafici e dataset report correnti.
3. [x] Coprire inverno, estate, coil, heater, accessori ed entalpici.
4. [x] Definire tolleranze numeriche campo per campo.
5. [x] Automatizzare il confronto dove possibile.

Checkpoint: esiste una baseline ripetibile prima di spostare codice.

Implementazione:

- fixture sintetiche e anonime in
  `tests/fixtures/technical-baselines/inputs`;
- output approvati in `tests/fixtures/technical-baselines/expected`;
- comando diagnostico non interattivo
  `SSW.exe --technical-baseline <fixture> <output>`;
- confronto semantico di snapshot, serie/assi dei grafici e 24 dataset RDLC;
- tolleranze numeriche versionate per grandezza fisica;
- hash logico del manifest SDF e hash della libreria HEDes;
- esecuzione seriale integrata nella matrice
  `tests/Invoke-TechnicalSelectionReleaseTests.ps1`.

Le immagini binarie dei grafici non vengono confrontate pixel per pixel:
dimensioni e presenza restano nei dataset, mentre curve, punti e assi sono
confrontati numericamente per evitare falsi positivi dovuti a GDI+, font o
antialiasing di Windows.

Per approvare deliberatamente una variazione tecnica:

```powershell
tests\Invoke-TechnicalBaselines.ps1 -Update
```

L'aggiornamento delle baseline deve essere revisionato insieme alla modifica
del motore, del database SDF o della libreria HEDes.

## Onda 1 - Contratti applicativi UI-neutral

Stato: **completata il 27/07/2026**.

1. [x] Definire input di selezione e `AirflowPair`.
2. [x] Definire risultati stagionali, aeraulici e di ramo.
3. [x] Definire serie numeriche per i grafici.
4. [x] Separare formattazione/localizzazione dai valori.
5. [x] Definire errori, warning e validazioni strutturate.
6. [x] Mantenere mandata e ripresa uguali in modalita' legacy.

Checkpoint: i contratti non contengono tipi WinForms e possono descrivere
l'attuale selezione senza perdita di informazione.

Implementazione:

- contratti numerici e UI-neutral in `SSWLib/CLSelectionContracts.vb`;
- coppie distinte per portata e valori di ramo, con factory bilanciata;
- input completi per stagioni, water coil, electric heater e accessori;
- risultati distinti supply/extract e campo SFP complessivo;
- serie grafiche numeriche indipendenti da WinForms Chart e GDI+;
- errori e warning strutturati tramite codice, severita', percorso e
  `MessageKey`, senza testo localizzato nel motore;
- mapper in `SSWLib/CLSelectionContractMapper.vb` dal modello `.sswsel` e
  dagli snapshot correnti;
- adapter legacy bilanciato per mantenere extract uguale a supply fino
  all'Onda 6;
- validatore puro in `SSWLib/CLSelectionContractValidator.vb`;
- test contrattuali integrati in `SelectionIdentitySmoke`, inclusa una
  verifica riflessiva contro dipendenze WinForms, Drawing e ReportViewer.

Non sono stati modificati il formato `.sswsel`, gli algoritmi, il motore di
calcolo, i template report o le versioni dei contratti pubblici.

## Onda 2 - Estrazione del motore applicativo

Stato: completata il 27/07/2026 per il percorso termodinamico e aeraulico
bilanciato attuale.

1. [x] Estrarre da `CLMainForm` il coordinamento del calcolo stagionale.
2. [x] Conservare invariati `termo_calc`, HEDes e gli altri algoritmi.
3. [x] Restituire risultati e chart data invece di scrivere nei controlli.
4. [x] Convertire `curva` nell'adapter WinForms che aggiorna i controlli
   correnti.
5. [x] Confrontare adapter e baseline dopo ogni estrazione.

Checkpoint: la UI legacy usa il servizio estratto e non una copia degli
algoritmi.

Implementazione:

- `CLSelectionApplicationService` e DTO UI-neutral per calcolo stagionale,
  punto di lavoro, curve pressione/potenza/rendimento e aree di conformita';
- clonazione preventiva degli array del modello, per evitare mutazioni del
  catalogo durante calcoli ripetuti;
- `curva` conserva esclusivamente il rendering WinForms ed usa il risultato
  numerico del servizio;
- la curva estiva usa direttamente le serie numeriche restituite dal servizio
  e non rilegge piu' i punti dal controllo `Chart`;
- il form costruisce l'input stagionale e formatta i risultati senza
  duplicare le formule;
- matrice `Invoke-TechnicalSelectionReleaseTests.ps1 -Configuration AV`
  completata, incluse le quattro baseline tecniche e la build AV/x86.

Restano volutamente nel form, fino alla migrazione dei rispettivi sottosistemi,
il coordinamento a due passaggi delle batterie, rumore, CO2, report ed email.
Tali flussi consumano gia' il calcolo stagionale estratto e non costituiscono
una seconda implementazione dell'algoritmo aeraulico/termico.

## Onda 3 - Dati layout indipendenti dalla UI

Eseguire le onde A-C di `INSTALLATION_LAYOUT_ROADMAP.md`:

1. censimento;
2. database centrale ed Explorer;
3. esportazione SDF, immagini, hash e manifest.

Non realizzare il tab WinForms previsto dalla vecchia Onda D.

Checkpoint: layout, immagini e quote sono disponibili offline attraverso
repository UI-neutral.

## Onda 4 - Prototipo WebView2

1. Creare il nuovo host desktop affiancato.
2. Configurare frontend locale e hot reload Debug.
3. Creare design system e component demo route.
4. Implementare progetto, preselezione e selezione dettagliata.
5. Collegare un calcolo reale tramite il servizio applicativo.
6. Aprire salvataggio e report legacy dal nuovo host.

Checkpoint: una selezione reale puo' essere calcolata, salvata, riaperta e
stampata senza dipendere dalla UI WinForms per l'inserimento dati.

## Onda 5 - Parita' tecnica

1. Migrare layout installativo e dimensionale.
2. Migrare water coils.
3. Migrare electric heaters.
4. Migrare accessori.
5. Migrare documenti tecnici, email e follow-up.
6. Completare help, tooltip e 14 localizzazioni.

Checkpoint: il nuovo percorso copre l'attuale selezione tecnica bilanciata.

## Onda 6 - Portate sbilanciate

Applicare `UNBALANCED_AIRFLOW_CONTRACT.md`:

1. implementare il calcolo accoppiato;
2. produrre risultati separati di mandata e ripresa;
3. aggiornare grafici e riepiloghi;
4. aggiornare persistenza, fingerprint, API, portale e report;
5. conservare la compatibilita' dei file storici.

Checkpoint: il caso bilanciato resta invariato e quello sbilanciato e'
completo in ogni output.

## Onda 7 - Consolidamento e sostituzione

1. Eseguire confronto parallelo legacy/nuova UI.
2. Collaudare ridimensionamento, DPI e tutte le lingue.
3. Collaudare funzionamento offline.
4. Aggiornare installer e runtime WebView2.
5. Conservare un rollback verso la UI legacy per il periodo concordato.
6. Rimuovere il fallback solo dopo la parita' approvata.

Checkpoint: release candidate `2.0.0.0` installabile su un sistema con
`1.3.0.xx`, con rollback e confronto numerico approvati.

## Lavori che non vanno duplicati

- Nessun nuovo tab WinForms per preselezione o layout.
- Nessuna seconda implementazione dei calcoli.
- Nessun nuovo formato progetto separato.
- Nessuna libreria grafici diversa tra UI nuova e report senza un contratto
  comune delle serie.
- Nessun catalogo dati parallelo al database centrale e all'SDF.

## Ambito commerciale rinviato

Sono esplicitamente successivi:

- listini e prezzi;
- valuta e imposizione;
- documenti di offerta commerciale;
- clienti e responsabili;
- stati CRM e pipeline;
- autenticazione cliente.
