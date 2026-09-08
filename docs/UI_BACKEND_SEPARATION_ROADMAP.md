# Separazione UI e backend e nuova esperienza SSW - Roadmap

Data decisione architetturale: 27/07/2026

Checkpoint 08/09/2026: normalizzazione atomica selezione/risultati, protezioni
contro risposte asincrone obsolete e controlli backend per accessori e
trattamenti incompatibili. Evidenze e limiti in
[AUDIT_CORREZIONI_2026-09-08.md](AUDIT_CORREZIONI_2026-09-08.md).

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
- L'interfaccia WinForms corrente resta riferimento funzionale della linea
  `1.3.0.xx`, ma non e' piu' un fallback runtime di SSW Next.
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

Stato: compatibilita' legacy UI-neutral completata il 27/07/2026; migrazione
normalizzata centrale/SDF e immagini offline ancora aperte.

Eseguire le onde A-C di `INSTALLATION_LAYOUT_ROADMAP.md`:

1. censimento;
2. database centrale ed Explorer;
3. esportazione SDF, immagini, hash e manifest.

Non realizzare il tab WinForms previsto dalla vecchia Onda D.

Checkpoint: layout, immagini e quote sono disponibili offline attraverso
repository UI-neutral.

Implementazione intermedia:

- `CLLegacySdfInstallationLayoutRepository` espone configurazioni orizzontali
  e verticali, default B6/A4, quote A-D e quattro porte senza tipi UI;
- il repository non risolve percorsi storici di rete e segnala esplicitamente
  immagini offline mancanti e dati incompleti;
- il WebView2 preview usa questo repository senza introdurre un nuovo tab
  WinForms;
- il checkpoint completo resta aperto finche' Explorer ed exporter non
  materializzano tabelle normalizzate e immagini binarie autorevoli nell'SDF.

## Onda 4 - Prototipo WebView2

Stato: completata il 27/07/2026 come preview affiancata.

1. [x] Creare il nuovo host desktop affiancato.
2. [x] Configurare frontend locale e hot reload Debug.
3. [x] Creare design system e component demo route.
4. [x] Implementare progetto, preselezione e selezione dettagliata.
5. [x] Collegare un calcolo reale tramite il servizio applicativo.
6. [x] Collegare salvataggio e report al nuovo host.

Checkpoint: una selezione reale puo' essere calcolata, salvata, riaperta e
stampata senza dipendere dalla UI WinForms per l'inserimento dati.

Risultato:

- `SSW.exe --next-ui` avvia WebView2 senza cambiare lo startup produttivo;
- bundle TypeScript/Vite locale costruito e copiato da MSBuild;
- route guidata a otto step e component showcase;
- `NativeSelectionBridge` usa `CLNextUiApplicationService`, non fixture;
- catalogo modelli, calcolo inverno/estate, layout e accessori arrivano
  realmente dall'SDF;
- browser mock limitato allo sviluppo frontend;
- salvataggio e report usano servizi applicativi e dataset RDLC senza aprire
  `CLMainForm`.

Il checkpoint sulla completa indipendenza da `CLMainForm` per
salvataggio/report e' stato chiuso il 29/07/2026. Il viewer RDLC generico resta
WinForms perche' e' un contenitore documentale, non la vecchia UI tecnica.

## Onda 5 - Parita' tecnica

Stato: **completata il 27/07/2026 per il prototipo tecnico bilanciato**.

1. [x] Migrare layout installativo e dimensionale tramite adapter SDF legacy.
2. [x] Migrare water coils.
3. [x] Migrare electric heaters.
4. [x] Migrare accessori.
5. [x] Collegare documenti tecnici, email e follow-up al workflow produttivo.
6. [x] Completare help, tooltip e 14 localizzazioni del prototipo.
7. [x] Migrare calcolo CO2 e rumore, persistenza e dataset RDLC.

Nota UX del 29/07/2026: CO2 e rumore condividono il payload tecnico e il
ricalcolo UI-neutral, ma sono esposti in SSW Next come due passaggi distinti
del workflow (`CO2` e `Calcolo acustico`). In questo modo configurazione,
risultati e inclusione nel report restano indipendenti e immediatamente
leggibili.

Checkpoint: il nuovo percorso copre l'attuale selezione tecnica bilanciata.

Risultato:

- catalogo reale e ricalcolo tecnico bilanciato inverno/estate;
- layout legacy, quote e accessori modello-specifici tramite DTO UI-neutral;
- water coil HEDes con secondo passaggio alla portata realmente disponibile;
- PEHD/EHD con compatibilita', temperature e perdite aerauliche aggiuntive;
- creazione del documento progetto canonico e round-trip del serializer;
- salvataggio, progetti, RDLC, email e follow-up conservati tramite servizi
  applicativi, senza duplicarne le regole nel frontend;
- help contestuale, tooltip disattivabili e shell localizzata nelle 15 lingue;
- calcolo CO2 con i tre metodi legacy, curva a 300 minuti e parametri ambiente;
- calcolo acustico sulle otto bande, LwA, pressioni alle due distanze e
  opzione EN ISO 16032;
- persistenza di entrambe le sezioni nel `.sswsel` e popolamento reale dei
  dataset `SoundPower`, `SoundPowerHeader`, `CO2LevelRoom`, `CO2LevelUse` e
  `CO2LevelParameters`;
- smoke nativo e serializzazione progetto integrati nel gate di release;
- verifica responsive di tutti gli step a 1440x900 e 1024x768.

Il prototipo non deve essere pubblicato come `2.0.0.0` e non sostituisce ancora
la UI corrente. La normalizzazione centrale dei layout e delle immagini CAD
resta un'attivita' dati dell'Onda 3: l'adapter corrente garantisce la parita'
tecnica usando le configurazioni gia' presenti nell'SDF. La dipendenza runtime
da `CLMainForm` e' stata rimossa il 29/07/2026. Restano consentiti dialoghi host
e il viewer RDLC generico; non contengono ne' istanziano la vecchia schermata
di selezione.

Aggiornamenti del prototipo del 28/07/2026:

- la preselezione filtra esclusivamente modelli con un punto di lavoro fisico,
  con pressione, potenza e SFP positivi calcolati dal servizio esistente;
- la scelta del modello porta direttamente alla configurazione installativa;
- le portate di mandata e ripresa restano sincronizzate in entrambe le
  direzioni finche' lo sbilanciamento non viene implementato;
- il controllo `Sbilanciamento` e' visibile ma disabilitato e non selezionato;
- dopo la scelta del modello la nuova UI mantiene in basso i grafici reali di
  pressione, potenza assorbita e rendimento, usando le serie numeriche del
  servizio applicativo e una scala di portata comune.
- la configurazione layout selezionata modifica realmente l'assegnazione dei
  quattro flussi; Fresh e Return sono sempre entranti, Supply ed Exhaust sempre
  uscenti;
- `Genera report` prepara direttamente dataset, grafici e template RDLC tramite
  `CLNextUiReportService`, quindi apre il viewer da SSW Next. Non viene creato
  alcun builder o form legacy invisibile.
- la gestione del progetto e' persistente nella barra laterale destra, insieme
  alla selezione corrente: elenco, apertura, rimozione, salvataggio e invio email
  non richiedono il ritorno alla UI legacy;
- lingua dell'interfaccia e lingua documentale del progetto sono indipendenti:
  un progetto resta mono-lingua, ma puo' essere preparato in una delle 15 lingue
  senza modificare la lingua con cui l'operatore usa SSW;
- il cambio della lingua documentale rigenera in staging tutti i PDF incorporati
  nel progetto e sostituisce il progetto corrente solo se l'intera operazione
  termina con successo. Apertura selezioni ed email mantengono tale lingua.

Checkpoint preselezione del 02/09/2026:

- dopo la verifica del punto di lavoro e il calcolo della regolazione minima,
  la preselezione puo' applicare in modo indipendente un limite SFP, un limite
  acustico sulla mandata e un limite acustico sul rumore irradiato;
- per ciascun limite acustico la grandezza e' mutuamente esclusiva (`LwA` o
  `LpA`); distanza e fattore di direttivita' sono indipendenti tra mandata e
  rumore irradiato e vengono richiesti solo per `LpA`;
- i valori `Supply` e `Breakout` provengono dallo stesso servizio acustico del
  calcolo dettagliato e del report, valutato alla regolazione del candidato;
- i criteri sono tutti opzionali, localizzati nelle 15 lingue e persistiti nel
  `.sswsel`; i file precedenti vengono normalizzati con i filtri disattivati;
- lo smoke nativo verifica esclusione SFP, valori acustici dei candidati e
  soglie `LwA`/`LpA` senza introdurre formule alternative nel frontend.

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

Stato: **in corso**. Separazione runtime completata il 29/07/2026; collaudo e
packaging della release candidate ancora da chiudere.

1. [ ] Eseguire confronto parallelo legacy/nuova UI sull'intera matrice.
2. [ ] Collaudare ridimensionamento, DPI e tutte le lingue.
3. [ ] Collaudare funzionamento offline.
4. [ ] Aggiornare installer e runtime WebView2.
5. [x] Conservare il rollback installando l'ultima release `1.3.0.xx`, senza
   mantenere un fallback interno nel runtime Next.
6. [x] Rimuovere il fallback e l'avvio della UI legacy dal percorso Next.
7. [x] Estrarre risoluzione documenti in `CLProductDocumentService`.
8. [x] Estrarre snapshot calcolato e registrazione tecnica in servizi
   UI-neutral.
9. [x] Estrarre dataset, grafici e scelta template in
   `CLNextUiReportService`.
10. [x] Impostare SSW Next come startup ordinario del branch di
    modernizzazione.

Gate automatici aggiunti:

- nessun riferimento a `CLMainForm`, `OpenLegacy` o `legacy.open` nel programma
  host e nel frontend Next;
- rendering PDF reale del report base, del report con batteria elettrica e del
  report con batteria ad acqua;
- rendering del template CO2 e verifica che dataset CO2 e rumore contengano
  risultati reali;
- rendering localizzato almeno in italiano, bulgaro e norvegese;
- round-trip di `.sswsel` e `.sswproj`;
- screenshot WebView2 nativo dall'eseguibile AV.

Confine conservato intenzionalmente:

- `CLMainForm` resta compilato nella libreria per la manutenzione della linea
  storica e per il comando tecnico di baseline comparativa; non e' referenziato
  o istanziato dal runtime SSW Next;
- rumore e CO2 sono gestiti direttamente da servizi UI-neutral; se non
  selezionati per il report, i flag RDLC li nascondono senza dipendere dalla
  schermata WinForms storica;
- la rimozione fisica dei sorgenti WinForms legacy avverra' solo quando non
  sara' piu' necessario mantenere la linea `1.3.0.xx` dalla stessa soluzione.

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
