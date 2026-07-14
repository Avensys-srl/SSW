# Gestione delle selezioni tecniche - Roadmap

Questo documento e' il backlog operativo concordato per introdurre riferimenti
univoci, progetti riapribili, revisioni tecniche, compatibilita' nel tempo e
registrazione centralizzata delle selezioni SSW.

## Stato

- [x] 01. Creare un checkpoint stabile dello stato corrente di SSW.
  Commit: `13a48c1 Improve coil reporting and update workflow`.
- [x] Onda A completata il 14/07/2026: punti 02-05.
- [x] 02. Contratto di versionamento definito in
  `docs/TECHNICAL_SELECTION_VERSIONING.md`.
- [x] 03. Manifest e feature SDF implementati in `CLDataCentralLib`.
- [x] 04. Controllo compatibilita' SDF implementato e verificato in SSW.
- [x] 05. Formato `.sswsel` V1 definito, implementato e verificato.
- [x] Onda B1 completata il 14/07/2026: punti 06, 07 e 12.
- [x] 06. Motore di migrazione e compatibilita' progetti.
- [x] 07. Ciclo di vita locale del progetto e file recenti.
- [x] Onda B2 implementata e collaudata localmente il 14/07/2026: punti 08-11.
- [ ] Deployment operativo B2 su database/API web dedicati.
- [x] Onda C1 completata il 14/07/2026: punti 13 e 15.
- [x] Onda C2 implementata e collaudata localmente il 14/07/2026: punto 14.
- [ ] 16-22. Attivita' residue descritte nelle sezioni seguenti.

## Regole architetturali approvate

- Il riferimento pubblico e' numerico, casuale, non sequenziale e dotato di
  checksum, per esempio `4827-1936-5048-2715-R01`.
- Il progressivo reale resta interno al database e non e' ricavabile dal
  riferimento pubblico.
- Le revisioni sono immutabili. Una ristampa invariata conserva la revisione;
  una modifica tecnica o della base di calcolo ne genera una nuova.
- Il progetto persistente usa l'estensione `.sswsel` e non serializza controlli
  WinForms, entita' EF o strutture RDLC.
- Le installazioni si registrano automaticamente, senza login, autorizzazioni
  manuali o disturbo per cliente e Avensys.
- Offline viene prodotto un report in bozza con riferimento locale progressivo,
  per esempio `D-7K3P-000042`.
- La geolocalizzazione salva solo nazione e citta' approssimativa. L'IP non viene
  conservato nella selezione.
- SSW, motore di calcolo, schema SDF, dati SDF, progetto, report e API hanno
  versioni indipendenti.
- Anche una modifica della sola DLL di calcolo produce una release tecnica e,
  nella prima implementazione, un installer completo.
- Un progetto storico puo' ristampare lo snapshot originale. Un nuovo calcolo
  usa le versioni correnti e genera una nuova revisione.

## Attivita' residue

### 02. Contratto di versionamento

- Stato: **completato** il 14/07/2026.
- Evidenza: `docs/TECHNICAL_SELECTION_VERSIONING.md`.
- Repository: `D:\mdev\SSW` e documentazione condivisa.
- Definire `SoftwareVersion`, `CalculationEngineVersion`,
  `DatabaseSchemaVersion`, `DatabaseDataVersion`, `SelectionFormatVersion`,
  `ReportTemplateVersion` e `ApiContractVersion`.
- Definire quando ogni versione cambia e quando deve nascere una revisione.
- Completato quando esiste una specifica approvata e utilizzabile dagli altri
  repository.
- Stima: 30-50 mila token.

### 03. Metadati e feature nell'esportazione SDF

- Stato: **completato** il 14/07/2026.
- Evidenza: `CLSSWExporter` crea `CLDatabaseMetadata`,
  `CLDatabaseFeatures` e l'hash SHA-256 canonico; build `Release|x86` e smoke
  test del manifest su SDF riusciti, incluso il controllo di stabilita' hash.
- Commit exporter: `d7e1ea0 Add versioned SDF database manifest`.
- Verifica export del 14/07/2026: il primo SDF reale era ancora `Legacy-0`
  perche' CLDCExplorer caricava la precedente DLL dal proprio package locale.
  Il package e la build `Release|x86` di CLDCExplorer sono stati riallineati
  alla DLL `d7e1ea0` con commit Explorer `cf44e20`.
- Collaudo export AVENSYS completato il 14/07/2026: schema 1, revisione
  `2026.07.14.121352-7A280D8C`, exporter 1.0.0, cliente `035889`, tre feature
  attive, 31 batterie e 98 relazioni. Hash memorizzato e hash ricalcolato
  coincidono; il reader SSW accetta database e feature.
- Repository: `T:\TECHNO_SOFT\mercurial\CLDataCentralLib`.
- Esportare `CLDatabaseMetadata` e `CLDatabaseFeatures` in ogni SDF.
- Inserire versione schema, revisione dati, exporter, timestamp, cliente,
  versione minima SSW e hash del contenuto.
- Trattare gli SDF senza metadati come `Legacy-0`.
- Stima: 70-120 mila token.

### 04. Controllo compatibilita' SDF in SSW

- Stato: **completato** il 14/07/2026.
- Evidenza: lettura prima di EF, inferenza `Legacy-0`, gating `WaterCoils`,
  diagnostica Info e test di accettazione schema 1/rifiuto schema futuro.
- Repository: `D:\mdev\SSW`.
- Leggere i metadati prima dell'inizializzazione Entity Framework.
- Disabilitare le feature non presenti senza bloccare le funzioni compatibili.
- Gestire database legacy, troppo vecchi, troppo nuovi o corrotti.
- Mostrare le versioni database nella schermata Info.
- Stima: 50-90 mila token.

### 05. Formato `.sswsel` V1

- Stato: **completato** il 14/07/2026.
- Evidenza: DTO indipendenti dalla UI, serializer atomico, specifica
  `docs/TECHNICAL_SELECTION_FORMAT_V1.md`, fixture V1 e round-trip x86 riuscito;
  il formato futuro viene rifiutato senza modifiche.
- Repository: `D:\mdev\SSW`.
- Definire envelope JSON, DTO tecnici, feature, versioni, input, output e
  snapshot calcolato.
- Salvare codici gestionali stabili oltre agli ID numerici.
- Non legare il progetto alla struttura corrente del form o del report.
- Stima: 60-100 mila token.

### 06. Motore di migrazione dei progetti

- Stato: **completato** il 14/07/2026.
- Evidenza: runner sequenziale predisposto, normalizzazione conservativa dei
  V1 incompleti, blocco futuro `ElectricHeater` disabilitato, backup
  `pre-migration` e rifiuto dei formati futuri. Build AV/x86 e smoke test della
  fixture `docs/examples/selection-v1-sparse.sswsel` riusciti, incluso il
  round-trip con backup e seconda apertura senza nuova migrazione.
- Repository: `D:\mdev\SSW`.
- Implementare trasformazioni sequenziali `V1 -> V2 -> ...`.
- Applicare default compatibili per sbilanciamento, resistenze, batteria ed
  estate quando i blocchi non esistono.
- Creare un backup prima del primo salvataggio nel nuovo formato.
- Rifiutare in modo controllato file creati da una versione futura.
- Stima: 70-120 mila token.

### 07. Menu e ciclo di vita del progetto

- Stato: **completato** il 14/07/2026.
- Evidenza: menu localizzato Nuova/Apri/Salva/Salva con nome/Duplica/Recenti,
  mapping tra form e DTO V1, dirty state, titolo e conferma modifiche non
  salvate. Smoke test AV/x86 riuscito: sei comandi presenti, modifica rilevata,
  round-trip del form da portata 100 a 321 e ritorno a 100, recenti aggiornati
  e stato pulito dopo la riapertura.
- Repository: `D:\mdev\SSW`.
- Aggiungere Nuova, Apri, Salva, Salva con nome e Duplica come nuova selezione.
- Gestire modifiche non salvate, titolo del form, file recenti e messaggi.
- Stima: 50-90 mila token.

### 08. Database server delle selezioni

- Stato: **implementato localmente** il 14/07/2026; migrazione MySQL V1 pronta,
  applicazione sul database web dedicato ancora da eseguire.
- Ambiente: database web dedicato, separato dai dati prodotto.
- Creare clienti, installazioni tecniche, selezioni padre, revisioni, token e
  audit delle versioni.
- Applicare vincoli univoci, transazioni, indici e conservazione.
- Stima: 60-100 mila token.

### 09. Identita' installazione zero-touch

- Stato: **implementato e collaudato localmente** il 14/07/2026.
- Repository: `D:\mdev\SSW` e `A:\webavensys\api`.
- Generare automaticamente InstallationId e token tecnico.
- Conservare il token client in ProgramData protetto da Windows e sul server
  solamente come hash.
- Nessuna schermata di registrazione o approvazione manuale.
- Stima: 50-80 mila token.

### 10. API versionata e idempotente

- Stato: **implementato e collaudato localmente** il 14/07/2026; deployment
  HTTPS e configurazione ambiente ancora da eseguire.
- Repository: `A:\webavensys\api`.
- Registrare nuove selezioni e revisioni tramite HTTPS.
- Gestire retry senza consumare riferimenti duplicati.
- Aggiungere rate limiting, limiti payload, validazione e logging tecnico.
- Non esporre letture pubbliche tramite il riferimento.
- Stima: 90-150 mila token.

### 11. Generazione del riferimento pubblico

- Stato: **implementato e collaudato localmente** il 14/07/2026.
- Ambiente: API e database server.
- Generare 15 cifre casuali crittografiche piu' checksum.
- Applicare vincolo univoco e rigenerazione in caso di collisione.
- Gestire suffisso di revisione `RNN` senza esporre il progressivo interno.
- Stima: 30-50 mila token.

### 12. Bozze offline e progressivo locale

- Stato: **completato** il 14/07/2026.
- Evidenza: stato non segreto in ProgramData, InstallationCode opaco di quattro
  caratteri, progressivo atomico protetto da mutex e riferimenti nel formato
  `D-XXXX-000001`. Smoke test concorrente con 12 processi riuscito: stesso
  InstallationCode, riferimenti univoci e intervallo contiguo 000001-000012.
- Repository: `D:\mdev\SSW`.
- Generare InstallationCode e contatore locale atomico in ProgramData.
- Proteggere la numerazione tra piu' istanze con un mutex Windows.
- Collegare il riferimento bozza alla registrazione definitiva.
- Stima: 60-100 mila token.

### 13. Snapshot, hash e revisioni immutabili

- Stato: **completato** il 14/07/2026.
- Evidenza: JSON canonico con SHA-256 separati per input, output, base di
  calcolo e snapshot; classificazione di ristampa, modifica tecnica, database,
  algoritmo e solo risultato verificata nel client e nell'API. La ristampa
  invariata conserva la revisione e genera un evento audit dedicato.
- Repository: SSW, API e database server.
- Normalizzare il JSON e calcolare SHA-256 includendo versioni DLL e SDF.
- Distinguere ristampa, modifica tecnica, cambio database e cambio algoritmo.
- Conservare input e output per la riproducibilita' storica.
- Stima: 80-140 mila token.

### 14. Registrazione nel comando Genera report

- Stato: **implementato e collaudato localmente** il 14/07/2026; il collaudo
  HTTPS reale richiede il reload Apache e il test end-to-end. Il database
  dedicato e le variabili `SSW_SELECTION_*` sono stati predisposti sul server.
- Evidenza: create/reprint/revision vengono eseguite prima del dataset RDLC con
  chiave idempotente deterministica, blocco dei doppi click, attesa asincrona e
  dialogo localizzato Riprova/Genera bozza/Annulla. Smoke test x86 riuscito per
  R01, ristampa R01, modifica R02 e rendering del dialogo senza sovrapposizioni.
- Repository: `D:\mdev\SSW`.
- Registrare la selezione prima della costruzione del dataset RDLC.
- Gestire Riprova, Genera bozza e Annulla.
- Mostrare un'attesa non bloccante e impedire richieste duplicate.
- Stima: 90-150 mila token.

### 15. Persistenza automatica accanto al PDF

- Stato: **completato** il 14/07/2026.
- Evidenza: export PDF controllato dal viewer, scrittura atomica e successivo
  salvataggio automatico del `.sswsel` omonimo; percorso companion e
  round-trip delle impronte verificati dallo smoke test x86. C2 completa il
  token di ripresa: viene generato dal client, salvato nel progetto e protetto
  sul server solo tramite SHA-256, incluso il recupero dopo una risposta persa
  e il trasferimento tra installazioni dello stesso cliente. Il collaudo
  visuale automatizzato del vecchio viewer WinForms resta manuale per un limite
  del helper desktop (`Interfaccia non supportata`).
- Repository: `D:\mdev\SSW`.
- Salvare il `.sswsel` accanto al PDF dopo la generazione.
- Conservare il token di ripresa della selezione e l'ultima revisione.
- Consentire il trasferimento controllato del progetto su un altro PC.
- Stima: 40-70 mila token.

### 16. Riferimento e stato nei report

- Repository: `D:\mdev\SSW`.
- Aggiornare i quattro RDLC con riferimento, revisione e stato bozza sotto il
  logo, senza alterare il layout approvato delle tabelle.
- Rendere condizionali le future sezioni tecniche.
- Stima: 70-130 mila token.

### 17. Geolocalizzazione approssimativa

- Ambiente: API server.
- Usare un database GeoIP locale aggiornabile.
- Salvare codice nazione, citta', sorgente e livello di accuratezza.
- Non conservare l'IP nella selezione e non inviarlo a servizi esterni.
- Stima: 40-80 mila token.

### 18. Recupero interno Avensys

- Ambiente: applicazione o pannello amministrativo autenticato.
- Cercare per riferimento pubblico, vedere revisioni e scaricare snapshot.
- Impedire qualsiasi accesso incrociato o pubblico ai dati tecnici.
- Stima: 80-140 mila token.

### 19. Manifest e aggiornamenti dei componenti

- Repository: `D:\mdev\SSW` e `A:\webavensys\api`.
- Estendere l'API aggiornamenti con versioni, requisiti e SHA-256 di SSW, DLL,
  SDF e report.
- La prima versione distribuisce sempre un installer completo e atomico.
- Registrare `CalculationEngineVersion` e supportare rollback.
- Stima: 100-180 mila token.

### 20. Traduzioni, informativa e conservazione

- Repository: SSW, report e contenuti web.
- Tradurre tutte le nuove etichette e i messaggi nelle 12 lingue.
- Documentare registrazione tecnica, geolocalizzazione e tempi di conservazione.
- Stima: 40-80 mila token.

### 21. Matrice di test e fixture storiche

- Repository: tutti quelli coinvolti.
- Conservare esempi di ogni SDF e `.sswsel` supportato.
- Testare compatibilita', collisioni, concorrenza, retry, offline, revisioni,
  componenti rimossi e report multilingua.
- Stima: 100-180 mila token.

### 22. Rilascio progressivo

- Ambienti: sviluppo, Avensys pilota e produzione.
- Validare migrazioni, installer, API, database, rollback e osservabilita'.
- Rendere il riferimento obbligatorio nei report definitivi solo dopo il
  collaudo pilota.
- Stima: 50-100 mila token.

## Stima complessiva

- Esecuzione base controllata: 1,17-2,30 milioni di token elaborati.
- Contingenza per debug legacy, RDLC, server e deployment: 25% circa.
- Budget realistico complessivo: 1,5-2,9 milioni di token.
- Uso intenso di subagent, tentativi paralleli o regressioni estese puo' portare
  il totale oltre 3 milioni.

Questa e' una stima di elaborazione, non una previsione diretta della quota
account: cache, modello, modalita' Fast e limiti del workspace possono pesare in
modo differente. Per stimare il residuo finale serve il valore corrente mostrato
da `/usage` nel Codex CLI.

## Strategia di contenimento

- Procedere per checkpoint, con un punto principale per task.
- Evitare subagent salvo analisi realmente parallele.
- Compattare o aprire una nuova task a ogni cambio di repository.
- Leggere solo i file necessari e limitare l'output dei comandi.
- Committare e validare ogni blocco prima di passare al successivo.
- Non iniziare API e report prima di aver congelato contratti e DTO.

## Ondate di esecuzione

### Onda A - Fondazioni locali

- Stato: **completata** il 14/07/2026.
- Checkpoint SSW: `f85451e Establish technical selection foundations`.
- Checkpoint exporter: `d7e1ea0 Add versioned SDF database manifest`.
- Punti 02-05: contratto versioni, metadati SDF, controllo compatibilita' e
  formato `.sswsel` V1.
- Il punto 06 viene anticipato solo se la quota residua e i test lo consentono.
- Obiettivo: congelare i contratti prima di modificare API e report.

### Onda B - Persistenza e identita'

- Stato: **implementazione locale completata** il 14/07/2026; B1 e B2
  collaudate. Il 14/07/2026 e' stato creato su `SERVER01` il database dedicato
  `ssw_selections`, con migrazioni 001-003, nove tabelle e cliente `AV` attivo.
  Utente runtime e configurazione Apache sono predisposti; resta il reload
  Apache e il collaudo HTTPS prima di chiudere il deployment operativo.
- Punti 06-12: migrazioni, menu progetto, database server, installazione
  zero-touch, API, riferimento pubblico e bozze offline.
- Obiettivo: creare e riaprire una selezione senza ancora cambiare gli RDLC.

#### B1 - Progetto locale e funzionamento offline

- Stato: **completato** il 14/07/2026.
- Checkpoint SSW: `89ef893 Complete Wave B1 local selection lifecycle`.
- Evidenza: build AV/x86, fixture V1 minimale con backup, round-trip completo
  del form, file recenti, dirty state e prova concorrente del contatore locale.
1. Punto 06: introdurre il runner di migrazione sequenziale, la validazione
   dell'envelope e le fixture storiche. Non creare artificialmente un formato
   V2 finche' non esiste una modifica reale da migrare.
2. Punto 07: collegare i DTO V1 ai dati del form e aggiungere Nuova, Apri,
   Salva, Salva con nome e Duplica; gestire dirty state, titolo e file recenti.
3. Punto 12: generare InstallationCode e riferimento bozza locale con contatore
   atomico e mutex, senza dipendere dalla rete.
4. Checkpoint B1: una selezione completa deve potersi salvare, chiudere,
   riaprire e ristampare offline conservando scenari e batteria.

#### B2 - Identita' e registrazione centralizzata

- Stato: **implementato e collaudato localmente** il 14/07/2026.
- Checkpoint SSW: `3e02abc Implement Wave B2 central selection identity`.
- Checkpoint SSWweb: `31457dc Add central technical selection API v1`.
- SSW: identita' macchina persistente, token DPAPI in ProgramData, client lazy
  per registrazione e rinnovo, configurazione bootstrap esterna al sorgente.
- API: schema MySQL dedicato, token hash, audit versioni, riferimento Luhn,
  revisioni immutabili, idempotenza, rate limit, limite payload e CLI di revoca.
- Evidenza: test PHP su SQLite con 5.000 riferimenti, retry selezione/revisione,
  rotazione token e conflitti; GET pubblico rifiutato con HTTP 405; build
  completa `AV|x86` e smoke test client x86 register/cache/renew/DPAPI.
- Da attivare: database web, variabili `SSW_SELECTION_*`, bootstrap del cliente
  e backup/purge schedulati. La sorgente e' in `SSWweb/api`; la copia di
  pubblicazione `A:\webavensys\api` e' stata verificata byte per byte.
1. Punto 08: creare lo schema server separato con clienti, installazioni,
   selezioni padre, revisioni, token hash e audit versioni.
2. Punto 11: implementare il riferimento pubblico casuale con checksum,
   vincolo univoco e suffisso revisione, senza esporre il progressivo interno.
3. Punto 10: pubblicare API `/v1` idempotenti per registrazione installazione,
   nuova selezione e nuova revisione, con validazione, limiti e rate limiting.
4. Punto 09: integrare nel client l'identita' zero-touch protetta in
   ProgramData e completare registrazione, rinnovo e revoca tecnica.
5. Checkpoint B2: retry e concorrenza non devono creare riferimenti o revisioni
   duplicate; nessuna lettura pubblica deve essere possibile dal riferimento.

#### Budget e soglie

- Consumo osservato Onda A: circa 9 punti percentuali del limite settimanale,
  inclusi riallineamento Explorer e collaudo dell'SDF reale.
- Quota all'avvio della pianificazione B: 71% residuo; reset 20/07/2026.
- Quota comunicata all'avvio effettivo di B2: 59% residuo; rivalutazione
  richiesta dopo il checkpoint B2 prima di iniziare B3/Onda C.
- Quota comunicata dopo B2: 56% settimanale residuo e 38% di contesto residuo.
  Consumo osservato B2: circa 3 punti percentuali, molto inferiore alla stima
  prudenziale. Capacita' disponibile sopra la riserva minima del 20%: 36 punti.
- Quota comunicata all'avvio di C1: 53% settimanale residuo e 36% di contesto
  residuo, dopo il rinnovo del contesto della sessione.
- Quota comunicata all'avvio di C2: 43% settimanale residuo e 43% di contesto
  residuo. Resta confermata la riserva minima del 20%.
- Suddividere Onda C in checkpoint: C1 punti 13 e 15, C2 punto 14, C3 punto 16,
  C4 punto 17. Rivalutare la quota dopo ogni checkpoint e attivare il database
  web B2 prima del collaudo end-to-end del punto 14.
- Stima B1: 7-11 punti; B2: 11-18 punti; integrazione e regressioni: 4-7 punti.
- Residuo atteso a Onda B conclusa: 35-49%; scenario prudenziale minimo circa
  31% in presenza di problemi di deployment o credenziali server.
- Rivalutare quota e rischi dopo B1 e prima del deployment API. Conservare
  almeno il 20% come riserva e non anticipare modifiche RDLC dell'Onda C.

### Onda C - Registrazione e report

- Stato: **C1 e C2 implementate e collaudate localmente** il 14/07/2026;
  database e configurazione server predisposti. Il deploy ha richiesto la
  compatibilita' con MariaDB 10.1 (`LONGTEXT` per i documenti JSON) e PHP 7.2;
  resta il reload Apache e il collaudo HTTPS end-to-end.
- Checkpoint C1: snapshot canonici e revisioni immutabili lato client/server;
  `.sswsel` generato automaticamente accanto al PDF senza registrazione API.
- Checkpoint SSW: `08055bd Complete Wave C1 immutable selection snapshots`.
- Checkpoint API SSWweb: `4512460 Add immutable selection revision fingerprints`.
- C2: registrazione nel comando report, fallback bozza e token di ripresa
  trasferibile; gli RDLC restano invariati fino a C3.
- Checkpoint SSW C2: `eac6f93 Complete Wave C2 report registration flow`.
- Checkpoint API C2: `8f9b5c6 Protect report registration with resume tokens`;
  sorgenti sincronizzati byte per byte in `A:\webavensys\api`.
- Checkpoint compatibilita' server: `22c3702 Support production PHP and MariaDB
  runtimes`; migrazioni 001-003 applicate su `ssw_selections` il 14/07/2026.
- Punti 13-17: snapshot, revisioni, Genera report, salvataggio automatico,
  intestazione RDLC e geolocalizzazione.
- Obiettivo: completare il flusso cliente end-to-end.

### Onda D - Gestione e rilascio

- Punti 18-22: recupero Avensys, manifest componenti, traduzioni/privacy,
  matrice di test e rilascio pilota.
- Obiettivo: rendere il sistema operabile, aggiornabile e pubblicabile.
