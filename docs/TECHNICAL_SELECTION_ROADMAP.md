# Gestione delle selezioni tecniche - Roadmap

Questo documento e' il backlog operativo concordato per introdurre riferimenti
univoci, progetti riapribili, revisioni tecniche, compatibilita' nel tempo e
registrazione centralizzata delle selezioni SSW.

## Stato

- [x] 01. Creare un checkpoint stabile dello stato corrente di SSW.
  Commit: `13a48c1 Improve coil reporting and update workflow`.
- [ ] 02-22. Attivita' residue descritte nelle sezioni seguenti.

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

- Repository: `D:\mdev\SSW` e documentazione condivisa.
- Definire `SoftwareVersion`, `CalculationEngineVersion`,
  `DatabaseSchemaVersion`, `DatabaseDataVersion`, `SelectionFormatVersion`,
  `ReportTemplateVersion` e `ApiContractVersion`.
- Definire quando ogni versione cambia e quando deve nascere una revisione.
- Completato quando esiste una specifica approvata e utilizzabile dagli altri
  repository.
- Stima: 30-50 mila token.

### 03. Metadati e feature nell'esportazione SDF

- Repository: `T:\TECHNO_SOFT\mercurial\CLDataCentralLib`.
- Esportare `CLDatabaseMetadata` e `CLDatabaseFeatures` in ogni SDF.
- Inserire versione schema, revisione dati, exporter, timestamp, cliente,
  versione minima SSW e hash del contenuto.
- Trattare gli SDF senza metadati come `Legacy-0`.
- Stima: 70-120 mila token.

### 04. Controllo compatibilita' SDF in SSW

- Repository: `D:\mdev\SSW`.
- Leggere i metadati prima dell'inizializzazione Entity Framework.
- Disabilitare le feature non presenti senza bloccare le funzioni compatibili.
- Gestire database legacy, troppo vecchi, troppo nuovi o corrotti.
- Mostrare le versioni database nella schermata Info.
- Stima: 50-90 mila token.

### 05. Formato `.sswsel` V1

- Repository: `D:\mdev\SSW`.
- Definire envelope JSON, DTO tecnici, feature, versioni, input, output e
  snapshot calcolato.
- Salvare codici gestionali stabili oltre agli ID numerici.
- Non legare il progetto alla struttura corrente del form o del report.
- Stima: 60-100 mila token.

### 06. Motore di migrazione dei progetti

- Repository: `D:\mdev\SSW`.
- Implementare trasformazioni sequenziali `V1 -> V2 -> ...`.
- Applicare default compatibili per sbilanciamento, resistenze, batteria ed
  estate quando i blocchi non esistono.
- Creare un backup prima del primo salvataggio nel nuovo formato.
- Rifiutare in modo controllato file creati da una versione futura.
- Stima: 70-120 mila token.

### 07. Menu e ciclo di vita del progetto

- Repository: `D:\mdev\SSW`.
- Aggiungere Nuova, Apri, Salva, Salva con nome e Duplica come nuova selezione.
- Gestire modifiche non salvate, titolo del form, file recenti e messaggi.
- Stima: 50-90 mila token.

### 08. Database server delle selezioni

- Ambiente: database web dedicato, separato dai dati prodotto.
- Creare clienti, installazioni tecniche, selezioni padre, revisioni, token e
  audit delle versioni.
- Applicare vincoli univoci, transazioni, indici e conservazione.
- Stima: 60-100 mila token.

### 09. Identita' installazione zero-touch

- Repository: `D:\mdev\SSW` e `A:\webavensys\api`.
- Generare automaticamente InstallationId e token tecnico.
- Conservare il token client in ProgramData protetto da Windows e sul server
  solamente come hash.
- Nessuna schermata di registrazione o approvazione manuale.
- Stima: 50-80 mila token.

### 10. API versionata e idempotente

- Repository: `A:\webavensys\api`.
- Registrare nuove selezioni e revisioni tramite HTTPS.
- Gestire retry senza consumare riferimenti duplicati.
- Aggiungere rate limiting, limiti payload, validazione e logging tecnico.
- Non esporre letture pubbliche tramite il riferimento.
- Stima: 90-150 mila token.

### 11. Generazione del riferimento pubblico

- Ambiente: API e database server.
- Generare 15 cifre casuali crittografiche piu' checksum.
- Applicare vincolo univoco e rigenerazione in caso di collisione.
- Gestire suffisso di revisione `RNN` senza esporre il progressivo interno.
- Stima: 30-50 mila token.

### 12. Bozze offline e progressivo locale

- Repository: `D:\mdev\SSW`.
- Generare InstallationCode e contatore locale atomico in ProgramData.
- Proteggere la numerazione tra piu' istanze con un mutex Windows.
- Collegare il riferimento bozza alla registrazione definitiva.
- Stima: 60-100 mila token.

### 13. Snapshot, hash e revisioni immutabili

- Repository: SSW, API e database server.
- Normalizzare il JSON e calcolare SHA-256 includendo versioni DLL e SDF.
- Distinguere ristampa, modifica tecnica, cambio database e cambio algoritmo.
- Conservare input e output per la riproducibilita' storica.
- Stima: 80-140 mila token.

### 14. Registrazione nel comando Genera report

- Repository: `D:\mdev\SSW`.
- Registrare la selezione prima della costruzione del dataset RDLC.
- Gestire Riprova, Genera bozza e Annulla.
- Mostrare un'attesa non bloccante e impedire richieste duplicate.
- Stima: 90-150 mila token.

### 15. Persistenza automatica accanto al PDF

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

- Punti 02-05: contratto versioni, metadati SDF, controllo compatibilita' e
  formato `.sswsel` V1.
- Il punto 06 viene anticipato solo se la quota residua e i test lo consentono.
- Obiettivo: congelare i contratti prima di modificare API e report.

### Onda B - Persistenza e identita'

- Punti 06-12: migrazioni, menu progetto, database server, installazione
  zero-touch, API, riferimento pubblico e bozze offline.
- Obiettivo: creare e riaprire una selezione senza ancora cambiare gli RDLC.

### Onda C - Registrazione e report

- Punti 13-17: snapshot, revisioni, Genera report, salvataggio automatico,
  intestazione RDLC e geolocalizzazione.
- Obiettivo: completare il flusso cliente end-to-end.

### Onda D - Gestione e rilascio

- Punti 18-22: recupero Avensys, manifest componenti, traduzioni/privacy,
  matrice di test e rilascio pilota.
- Obiettivo: rendere il sistema operabile, aggiornabile e pubblicabile.
