# Accessori e funzioni di controllo - Roadmap

Data di approvazione del contratto: 20/07/2026

## Obiettivo

Completare la selezione tecnica SSW con un catalogo estendibile di accessori
hardware e funzioni di controllo, compatibilita' per serie e modello,
dipendenze, quantita', traduzioni, persistenza e report.

Questa roadmap non comprende i due sviluppi successivi, che restano nel
backlog di progetto:

- calcolo con portate di mandata e ripresa sbilanciate;
- scelta del layout installativo dell'unita'.

## Contratto funzionale approvato

- `Accessory_list.xlsx` e' la sorgente iniziale del catalogo, ma ogni riga e'
  validata prima dell'importazione.
- Hardware e funzioni sono elementi distinti dello stesso catalogo e possono
  essere aggiunti, modificati o disattivati da Explorer.
- Il catalogo generico non duplica CWD, HWD, HCD, EHD, PEHD o altri elementi
  termodinamici gia' gestiti dai tab dedicati.
- `DPS` identifica l'allarme filtro temporizzato; `DPP` identifica l'allarme
  tramite pressostato o sensore di pressione. `DPS-P` non viene importato.
- `IRS` e' il PIR esterno in wallbox; `PIR` e' il PIR interno all'unita'.
- Una famiglia prodotto corrisponde sempre a una riga di `CLSeries`.
- Un nuovo elemento e' disponibile globalmente per impostazione iniziale, ma
  non e' automaticamente selezionato dal cliente.
- La disponibilita' effettiva usa la precedenza modello, serie, globale.
- La quantita' predefinita e' 1 ed e' configurabile per relazione. Le sonde
  CO2 e RH possono avere quantita' massima 2; altri limiti sono data-driven.
- Prezzo e valuta sono previsti ma inizialmente null e non sono mostrati in
  SSW o nel report.
- Un accessorio hardware di serie e' interno, auto-selezionato e non
  deselezionabile. Una funzione di serie e' ugualmente bloccata ma non ha una
  posizione fisica.
- Un elemento opzionale puo' essere interno o esterno e viene selezionato dal
  cliente.
- Un elemento non disponibile e' visibile disabilitato dove utile, ma non
  compare nel report della selezione effettiva.
- Il report contiene soltanto accessori standard e opzioni effettivamente
  selezionate.
- Le funzioni senza accessorio fisico sono raccolte sotto una voce localizzata
  `Controllo unita' / Software`.

## KTS e livelli di controllo

I quattro KTS sono mutuamente esclusivi:

- KTS Basic: livello `Basic`;
- KTS Extra: livello `Extra` e scelta predefinita globale;
- KTS RFM: livello minimo `Extra` con capacita' RFM;
- KTS WiFi: livello minimo `Extra` con capacita' WiFi.

Le relazioni dichiarano un livello minimo, non uno specifico modello KTS. Una
funzione che richiede `Extra o superiore` e' compatibile con Extra, RFM e WiFi.
Basic resta disabilitato quando accessori o funzioni selezionati richiedono il
livello Extra; il tooltip elenca gli elementi che devono essere rimossi per
renderlo selezionabile. Non vengono eseguite sostituzioni silenziose.

`KTS Extra` e' una scelta predefinita sostituibile, non un elemento standard
bloccato, salvo diversa regola esplicita per una serie o un modello.

## Regole relazionali

Ogni relazione modello-elemento espone almeno:

- stato: `Standard`, `Optional`, `Unavailable`;
- installazione: `Internal`, `External`, `NotApplicable`;
- selezione predefinita;
- quantita' predefinita e massima;
- livello minimo KTS;
- ordinamento e stato attivo.

Le dipendenze tra elementi supportano:

- `Enables`: l'elemento dipendente resta disabilitato con tooltip finche'
  manca il prerequisito;
- `Requires`: il requisito viene selezionato e bloccato mentre il dipendente
  e' attivo;
- `Includes`: il componente comprende gia' l'elemento collegato;
- `Conflicts`: i due elementi non possono essere selezionati insieme.

## Tabelle centrali previste

Il nome fisico definitivo viene congelato nell'Onda A, ma il contratto logico
comprende:

- anagrafica elementi e categorie;
- traduzioni per lingua;
- regole globali;
- override per `CLSeries`;
- override per `CLHeatRecoveryModels`;
- dipendenze tra elementi;
- audit minimo di creazione e modifica.

Le tabelle legacy `Accessory`, `AccessoryFamily` e `UnitAccessory` non sono una
sorgente dati. Vengono salvate in un backup verificabile prima della migrazione
e poi escluse dal nuovo flusso.

## Traduzioni

- Le etichette statiche di SSW restano nei RESX.
- Nomi e descrizioni del catalogo sono gestiti in Explorer e nel database.
- L'export SDF contiene le traduzioni delle 12 lingue supportate.
- Il fallback e': lingua selezionata, inglese, acronimo.
- Il report usa gli stessi testi risolti da SSW.
- Le traduzioni complete vengono eseguite solo dopo il congelamento del
  catalogo inglese e delle regole.

## Report

La sezione usa quattro colonne:

| Accessorio | Descrizione | Funzioni associate | Stato |
| --- | --- | --- | --- |
| acronimo | testo localizzato | elenco multilinea | simbolo |

Simbologia prevista:

- quadrato pieno: opzione esterna selezionata;
- rombo pieno: opzione interna selezionata;
- cerchio pieno: montato di serie;
- cerchio vuoto: non disponibile, usato nell'interfaccia ma non nel report
  della selezione effettiva.

## Persistenza

- Il file `.sswsel` salva codici stabili, tipo, quantita', stato e
  installazione degli elementi selezionati.
- Il formato progetto passa da V1 a V2 con migrazione V1 -> V2 che inizializza
  una selezione accessori vuota.
- Le scelte entrano negli hash tecnici e generano una nuova revisione quando
  cambiano.
- Il payload JSON gia' registrato online conserva le scelte senza una nuova
  tabella MySQL obbligatoria.
- SSW Portal mostra accessori e funzioni nel dettaglio della revisione; in
  questa fase non viene implementata la ricerca globale per acronimo.

## Ondate operative

### Onda A - Database pulito e contratto

Stato: **completata il 20/07/2026**.

Stima: 55-85 mila token.

1. Eseguire backup con schema, dati, conteggi e hash delle tre tabelle legacy.
2. Congelare DDL, vincoli, indici, stati e regole di precedenza.
3. Creare migrazione ripetibile e rollback.
4. Preparare test SQL per vincoli, duplicati, dipendenze e quantita'.

Checkpoint A: schema applicato, backup verificato, database vuoto pronto per
il nuovo catalogo e nessuna modifica ancora richiesta a SSW.

Evidenza checkpoint A:

- backup SQL sotto lo schema `legacy`, con conteggi e checksum coincidenti per
  37 accessori, 5 famiglie e 1.513 relazioni della webapp sperimentale;
- archivio portabile XML compresso con SHA-256 verificato;
- nove nuove tabelle, tre trigger di integrita' e vista
  `CLSelectionEffectiveModelItems` applicati su `CLDataCentral2`;
- catalogo iniziale vuoto e tabelle legacy lasciate inalterate;
- migrazione idempotente, rollback su catalogo vuoto, riapplicazione e smoke
  test transazionali completati con successo.

### Onda B - Explorer, catalogo e matrice

Stima: 80-125 mila token.

1. Creare editor anagrafica, categorie e traduzioni.
2. Creare matrice elementi per serie e modelli con celle ereditate, incluse ed
   escluse, filtri e comandi massivi.
3. Creare editor dipendenze e livelli KTS.
4. Aggiungere import/export Excel con validazione e anteprima errori.
5. Importare il catalogo corretto con prezzi null.

Checkpoint B: il catalogo e le relazioni sono completamente gestibili da
Explorer, inclusi nuovi elementi futuri, senza interventi SQL manuali.

Stato al 20/07/2026: completata.

- aggiunto in Explorer il form MDI `Catalogo accessori e funzioni` con editor
  per categorie, elementi, traduzioni, regole serie/modello, collegamenti e
  dipendenze;
- aggiunta la matrice serie/modello con ereditarieta', filtri per tipo e
  categoria e applicazione massiva dello stato alle celle selezionate;
- aggiunti import `.xlsx` con anteprima/validazione e applicazione
  transazionale, piu' export CSV modificabile con Excel;
- importato il catalogo iniziale validato: 62 elementi in 12 categorie, prezzi
  null, quattro varianti KTS, override Quark/Serie 7 SG, sei collegamenti
  accessorio-funzione e quattro dipendenze;
- verificata l'idempotenza del seed e superati gli smoke test SQL; verificata
  anche la lettura delle 79 righe del file Excel mentre il file era aperto;
- Explorer portato alla versione `2.1.19.0` e soluzione compilata con successo
  in `Release|x86`; rimangono solo warning storici per riferimenti DevExpress
  opzionali non installati.

### Onda C - Exporter e SDF schema 3

Stima: 45-75 mila token.

1. Risolvere globale, serie e modello durante l'esportazione.
2. Esportare anagrafiche, traduzioni, dipendenze e relazione effettiva per
   modello.
3. Introdurre feature `AccessoriesAndControlFunctions` e schema SDF 3.
4. Aggiornare manifest, hash, versione minima SSW e copia DLL in Explorer.
5. Verificare export AV reale e fixture SDF storiche.

Checkpoint C: un SDF AV contiene esclusivamente le opzioni effettive dei
modelli esportati ed e' leggibile senza logica di ereditarieta' lato client.

Stato al 20/07/2026: completata.

- `CLDataCentralLib` aggiornata a exporter `1.2.0`, schema SDF `3` e versione
  minima SSW `1.3.0.52`;
- esportate sette tabelle per categorie, elementi, traduzioni, collegamenti,
  dipendenze e relazioni effettive modello-elemento gia' risolte lato server;
- escluse dall'SDF le relazioni con disponibilita' `Unavailable`, mantenute
  soltanto nel database centrale per la gestione amministrativa;
- aggiunta la feature `AccessoriesAndControlFunctions` versione 1 nel manifest
  SDF e pubblicata la DLL con manifest e SHA-256 verificati nell'Explorer;
- export AV reale verificato con 62 elementi e 6.149 relazioni effettive; la
  copia diretta dell'intero catalogo centrale contiene 8.954 relazioni;
- SSW aggiornato per accettare schema 3 e fixture storica immutabile aggiunta;
  build `AV|x86` e matrice completa di compatibilita', persistenza, API e
  release superate.

### Onda D - Selezione SSW e persistenza

Stima: 90-145 mila token.

Stato al 21/07/2026: **D1 completato; D2 e D3 da eseguire**.

- aggiunto il tab dinamico `Accessori e funzioni`, disponibile soltanto quando
  il manifest SDF espone la feature `AccessoriesAndControlFunctions`;
- letti dal nuovo SDF schema 3 catalogo effettivo, categorie, descrizioni,
  stato, installazione, quantita' e funzioni associate, con fallback lingua,
  inglese e acronimo;
- aggiunti ricerca, filtro categoria, raggruppamento visivo, selezione delle
  opzioni, quantita' entro i limiti esportati e riepilogo live degli acronimi;
- gli elementi standard sono preselezionati e bloccati; le selezioni di D1
  restano intenzionalmente in memoria fino al contratto di persistenza D3;
- aggiunto smoke test x86 sul SDF AV reale; verificati 55 elementi e quattro
  accessori con funzioni per il modello campione, oltre alla build `AV|x86`.

Prossimi checkpoint interni:

- D2: motore di dipendenze, conflitti, KTS e tooltip;
- D3: DTO progetto V2, migrazione V1, hash, dirty state e payload online.

1. Aggiungere il tab Accessori e funzioni, raggruppato e filtrabile.
2. Implementare stati standard, opzionale, interno, esterno e non disponibile.
3. Implementare quantita', dipendenze, conflitti e tooltip esplicativi.
4. Implementare il gruppo KTS mutuamente esclusivo e il livello minimo.
5. Mostrare il riepilogo live degli acronimi sotto le condizioni climatiche.
6. Integrare DTO V2, migrazione, snapshot, hash e dirty state.

Checkpoint D: selezione completa salvabile, riapribile e registrabile online;
vecchi `.sswsel` e vecchi SDF restano gestiti in modo controllato.

### Onda E - Report, portale e localizzazione

Stima: 75-120 mila token.

1. Aggiungere dataset e sezione condizionale nei quattro RDLC esistenti.
2. Mostrare solo standard e opzioni selezionate con funzioni multilinea.
3. Mostrare gli stessi dati nel dettaglio di SSW Portal.
4. Completare le traduzioni nelle 12 lingue e i fallback.
5. Verificare layout PDF con selezioni corte, lunghe e multipagina.

Checkpoint E: report e portale ricostruiscono in modo leggibile la selezione
commerciale e funzionale registrata.

### Onda F - Regressione e rilascio

Stima: 45-75 mila token.

1. Testare matrici, dipendenze, quantita', KTS e migrazioni storiche.
2. Testare SDF schema 2/3 e progetto V1/V2.
3. Build AV x86, test grafico dall'eseguibile canonico e report multilingua.
4. Commit e push di ogni repository coinvolto.
5. Generare installer, manifest aggiornamenti e rollback.

Checkpoint F: release installabile pubblicata e repository allineati.

## Budget e regole di arresto

Stima base complessiva: 390-625 mila token.

Contingenza legacy, RDLC e deployment: 20%, per un intervallo prudenziale di
470-750 mila token. La stima misura elaborazione e non corrisponde direttamente
alla percentuale settimanale mostrata dall'interfaccia Codex.

Per evitare lavori incompleti:

- non iniziare un'onda senza budget sufficiente a completarla e collaudarla;
- conservare almeno il 15% della quota settimanale come riserva di debug;
- comunicare lo stato di utilizzo prima di ogni nuova onda;
- produrre un commit autonomo dopo ogni checkpoint;
- non modificare il repository dell'onda successiva prima del checkpoint;
- in caso di quota insufficiente fermarsi su un artefatto stabile, mai tra DDL
  e migrazione o tra formato progetto e relativo reader.

## Uso controllato degli agenti

Gli agenti non sono la modalita' predefinita. Vengono usati soltanto quando il
lavoro e' realmente parallelo e non richiede di rileggere lo stesso contesto.

Impieghi ammessi:

- verifica indipendente delle traduzioni dopo il congelamento dell'inglese;
- audit read-only di uno schema, un RDLC o una matrice di test;
- esecuzione di test separati su repository che non vengono modificati in
  parallelo dal flusso principale;
- classificazione delimitata di righe Excel con regole gia' approvate.

Impieghi esclusi:

- definizione dell'architettura o delle migrazioni centrali;
- modifiche contemporanee agli stessi file o allo stesso database;
- esplorazioni generiche senza un risultato atteso e un limite di output;
- duplicazione di analisi gia' completate dal flusso principale.

Ogni agente deve avere un obiettivo chiuso, file o repository assegnati, un
budget di output ridotto e un criterio di completamento. Di norma viene usato
al massimo un agente ausiliario per volta; due sono ammessi solo su verifiche
completamente indipendenti. Il risultato viene controllato prima di essere
integrato e l'agente viene chiuso appena concluso il compito.

## Ordine dei repository

1. SQL Server `CLDataCentral2` e script versionati in CLDCExplorer.
2. `T:\TECHNO_SOFT\mercurial\CLDCExplorer`.
3. `T:\TECHNO_SOFT\mercurial\CLDataCentralLib`.
4. `D:\mdev\SSW`.
5. `A:\webavensys\ssw2` e pubblicazione SSW Portal.

Ogni passaggio usa il contratto prodotto dal precedente e non introduce un
secondo modello dati parallelo.
