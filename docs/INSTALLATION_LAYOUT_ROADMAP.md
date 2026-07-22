# Configurazione installativa e layout dimensionali - Roadmap

Data di approvazione del contratto: 22/07/2026

## Obiettivo

Integrare nella selezione tecnica SSW una scelta guidata della configurazione
installativa dell'unita', separando due informazioni correlate ma distinte:

1. il layout aeraulico, cioe' posizione e significato dei quattro raccordi;
2. il layout dimensionale, cioe' disegno CAD quotato e valori delle quote.

Il cliente non deve conoscere preventivamente sigle come `A3`, `B6` o `T5`.
Deve partire dalla propria necessita' installativa e vedere solamente le
configurazioni compatibili con il modello selezionato.

Questa roadmap e' il contratto di lavoro per gli agenti successivi. Ogni
decisione non esplicitamente modificata in una revisione di questo documento
deve essere considerata approvata.

## Ambito

Sono inclusi:

- schema dati centrale ed editor Explorer;
- associazione tra modelli, layout aeraulici, configurazioni e immagini;
- export offline nel database SDF;
- nuovo tab SSW `Configurazione e dimensioni`;
- scelta guidata, default e fallback;
- persistenza nei file `.sswsel` e negli snapshot tecnici;
- report PDF localizzati;
- salvataggio del dato nel payload online e visualizzazione testuale minima
  nel portale;
- test di compatibilita', regressione, build e rilascio.

Sono esclusi:

- modifica delle quote da parte del cliente;
- editor CAD o generazione automatica dei disegni dimensionali;
- calcolo con portate di mandata e ripresa sbilanciate;
- preselezione generale del modello;
- visualizzazione obbligatoria delle immagini dimensionali nel portale.

## Repository e componenti coinvolti

- SSW desktop, persistenza e RDLC: `D:\mdev\SSW`.
- Explorer centrale: `T:\TECHNO_SOFT\mercurial\CLDCExplorer`.
- libreria di export SDF:
  `T:\TECHNO_SOFT\mercurial\CLDataCentralLib`.
- esportatore principale:
  `CLDataCentralLib\DataCentral\Exporter\CLSSWExporter.cs`.
- editor modello recuperatore esistente:
  `CLDCExplorerLib\DataCentral\Explorer\Product\HeatRecovery\CLHeatRecoveryModelEditForm.cs`.
- eseguibile AV da usare per i test grafici dopo la compilazione:
  `D:\mdev\SSW\SSW\bin\x86\AV`.
- database SDF AV usato dall'applicazione:
  `D:\mdev\SSW\SSW\bin\x86\AV\data\DataCentral.sdf`.
- riferimento funzionale ricevuto:
  `C:\Users\PC\Downloads\configurazione_installazione.pdf`.

La cartella compilata storica
`T:\TECHNO_SOFT\App\CLDCExplorer2.0_ssw` non deve essere modificata.

## Stato dati verificato

La tabella centrale dei modelli contiene gia' dati riutilizzabili:

- `HorVariants` e `VerVariants`;
- `Dimension_A_Hor` ... `Dimension_D_Hor`;
- `Dimension_A_Ver` ... `Dimension_D_Ver`;
- coordinate delle connessioni Fresh, Return, Supply ed `Exaust` sui diversi
  assi. Il refuso storico `Exaust` non va rinominato in modo distruttivo;
- Explorer espone gia' varianti, dimensioni e coordinate nel form del modello.

Audit iniziale sul database SDF AV:

- 111 modelli recuperatore totali;
- 111 modelli con almeno la quota A;
- 52 modelli con almeno un elenco `HorVariants` o `VerVariants` valorizzato;
- i codici configurazione non hanno semantica globale: la stessa sigla puo'
  significare cose diverse in layout aeraulici differenti.

Le immagini esistenti devono essere censite e ricondotte a una sorgente
gestita. Non assumere che file numerati presenti nelle cartelle `bin` siano
la sorgente definitiva o siano tutti ancora validi.

## Contratto funzionale approvato

### Quattro flussi invarianti

Ogni configurazione ha sempre esattamente quattro raccordi e usa una sola
volta ciascuno dei seguenti ruoli:

- `FRESH`: aria esterna di rinnovo;
- `RETURN`: aria di ripresa dagli ambienti;
- `EXHAUST`: aria espulsa all'esterno;
- `SUPPLY`: aria immessa negli ambienti.

Il database e gli editor devono impedire duplicati, posizioni mancanti o un
numero di raccordi diverso da quattro.

### Tipi canonici di installazione

I codici interni stabili sono:

- `CEILING`: unita' appesa al soffitto;
- `FLOOR`: unita' appoggiata a terra;
- `WALL_VERTICAL`: unita' a parete con raccordi disposti alto/basso;
- `WALL_HORIZONTAL`: unita' a parete con raccordi disposti destra/sinistra.

I testi mostrati al cliente sono localizzati. Per continuita' documentale e'
possibile mostrare anche:

- `Parete - collegamenti alto/basso (Nord/Sud)`;
- `Parete - collegamenti destra/sinistra (Est/Ovest)`.

Nord/Sud ed Est/Ovest non sono coordinate assolute. Destra e sinistra devono
essere riferite a una vista dichiarata, normalmente la vista dal lato di
accesso per manutenzione.

### Identita' della configurazione

Il codice commerciale non e' globalmente univoco. L'identita' logica e':

```text
Layout aeraulico + Codice configurazione
```

Esempi come `OSC + A3`, `SSC + A3` e `AltroLayout + A3` rappresentano righe
distinte. SSC e OSC sono solamente esempi: nel database esistono altri layout
aeraulici e il codice non deve contenerne un elenco hardcoded.

Il collegamento deve usare l'identificativo reale del layout aeraulico
esistente nel database centrale. L'Onda A deve identificarne nome fisico,
chiave e relazioni correnti prima di congelare il DDL.

### Lato di accesso e superficie di riferimento

Per rendere comprensibile una macchina simmetrica, ogni configurazione deve
indicare separatamente:

- lato dei pannelli di accesso/manutenzione;
- lato della superficie di riferimento: soffitto, pavimento o parete;
- vista usata dal disegno.

Normalmente superficie di riferimento e accesso sono opposti, ma questa e'
una regola di compilazione e non un calcolo automatico. I due dati devono
restare indipendenti per supportare eccezioni future.

L'immagine deve rendere visibili:

- superficie di riferimento con tratteggio o simbolo riconoscibile;
- pannello di accesso con linea o colore evidente;
- dicitura localizzata `Accesso per manutenzione`;
- dicitura della vista, per esempio `Vista lato accesso`;
- eventuale ingombro minimo di manutenzione, se disponibile.

### Default e fallback

Per i modelli con piu' configurazioni si applica questa priorita':

1. configurazione orizzontale a soffitto `B6`;
2. configurazione orizzontale a soffitto `A4`;
3. altra configurazione orizzontale a soffitto;
4. configurazione esplicitamente predefinita per il modello;
5. prima configurazione disponibile secondo l'ordinamento commerciale.

La priorita' non va implementata come catena di `If` basata sulle sigle. Deve
essere memorizzata nella relazione modello/configurazione tramite
`IsDefault`, `DefaultPriority` e `SortOrder`. I dati iniziali devono produrre
il comportamento B6 -> A4 approvato.

Se esiste una sola configurazione compatibile, SSW la seleziona
automaticamente. Una configurazione precedentemente salvata resta selezionata
se ancora disponibile, anche se non coincide piu' con il default commerciale.

## Esperienza utente SSW

### Nuovo tab

Creare un tab `Configurazione e dimensioni` nell'area delle prestazioni.
Il tab deve essere disponibile quando il modello dispone di almeno una
configurazione valida e di dati dimensionali minimi.

Il layout e' diviso in due colonne responsive e ancorate correttamente.

#### Colonna sinistra - configurazione

- menu del tipo d'installazione;
- elenco o miniature delle sole configurazioni compatibili;
- codice tecnico mostrato come informazione secondaria;
- schema dei quattro raccordi con nomi localizzati;
- lato accesso, superficie di riferimento e vista;
- eventuali accessori richiesti e relative motivazioni.

Il cliente sceglie prima l'esigenza installativa e non la sigla. Quando
cambia installazione, l'elenco delle configurazioni viene rifiltrato senza
perdere altre parti della selezione tecnica.

#### Colonna destra - dimensioni

- disegno CAD quotato in proporzione stabile;
- scelta automatica del disegno orizzontale o verticale coerente con la
  configurazione selezionata;
- tabella read-only lettera/valore/unita';
- eventuali quote o ingombri di manutenzione;
- placeholder localizzato e non bloccante se manca solamente l'immagine;
- errore dati esplicito in Explorer se manca una quota obbligatoria.

Le immagini non devono ridimensionare o deformare il tab. Usare un contenitore
con rapporto d'aspetto stabile e modalita' di visualizzazione uniforme.

### Schema grafico dei flussi

Usare un approccio a template, non un'immagine completa per ogni lingua:

- sfondo neutro con macchina, raccordi, frecce, accesso e superficie;
- quattro anchor normalizzati per i raccordi;
- testi, colori e simboli dei flussi sovrapposti da SSW;
- coordinate normalizzate tra 0 e 1 per essere indipendenti dalla risoluzione;
- rendering ad alta risoluzione per il report e rendering ridimensionato per
  l'interfaccia.

Non limitare lo schema a due immagini o ai soli SSC/OSC. Il catalogo template
deve poter iniziare con pochi disegni condivisi ed essere esteso da Explorer
senza ricompilare SSW.

## Modello dati logico proposto

I nomi fisici devono essere congelati nell'Onda A dopo aver verificato le
entita' esistenti. Il contratto minimo comprende quanto segue.

### Tipi installazione e traduzioni

`CLInstallationTypes`:

- `Id`, `Code`, `Active`, `SortOrder`;
- quattro righe canoniche iniziali.

`CLInstallationTypeTranslations`:

- `IdInstallationType`, `LanguageCode`, `Name`, `Description`;
- vincolo univoco per tipo e lingua;
- fallback: lingua corrente, inglese, codice.

### Immagini

`CLInstallationImages`:

- `Id`, `Code`, `ImageKind` (`FlowTemplate` o `DimensionalDrawing`);
- `MimeType`, `ImageData`, `Width`, `Height`, `Sha256`;
- `Active`, `UpdatedAt`, `UpdatedBy`.

Le immagini devono essere esportate nell'SDF per il funzionamento offline.
Non usare percorsi assoluti del filesystem nella base dati distribuita.

### Template aeraulici

`CLFlowLayoutTemplates`:

- `Id`, `Code`, riferimento immagine;
- riferimento al layout aeraulico esistente;
- orientamento `Horizontal` o `Vertical`;
- vista di riferimento;
- `Active`, `SortOrder`.

`CLFlowLayoutTemplateAnchors`:

- `IdTemplate`, `PositionNumber` da 1 a 4;
- coordinate raccordo e label normalizzate;
- orientamento della freccia e allineamento del testo;
- vincolo univoco template/posizione.

### Configurazioni

`CLFlowConfigurations`:

- `Id`, `Code`;
- riferimento al layout aeraulico esistente;
- `IdInstallationType`, `IdTemplate`;
- orientamento;
- `AccessSide`, `ReferenceSurfaceSide`, `ReferenceView`;
- `Active`, `SortOrder`, note tecniche.

Vincolo univoco: layout aeraulico + codice configurazione.

`CLFlowConfigurationPorts`:

- `IdConfiguration`, `PositionNumber`, `AirRole`;
- quattro righe obbligatorie per configurazione;
- vincoli per una sola occorrenza di ogni posizione e di ogni ruolo.

### Relazioni con modelli

`CLHeatRecoveryModelFlowConfigurations`:

- `IdHeatRecoveryModel`, `IdFlowConfiguration`;
- `IsDefault`, `DefaultPriority`, `SortOrder`, `Active`;
- eventuale riferimento a regole accessori;
- note e audit.

La relazione e' la sorgente della disponibilita' in SSW. Non leggere
direttamente le stringhe legacy dopo la migrazione definitiva.

### Disegni e quote dimensionali

Il censimento deve determinare se il disegno e' condiviso per serie o specifico
del modello. Il modello deve supportare entrambe le precedenze:

1. disegno specifico del modello;
2. disegno della serie;
3. nessuna immagine, con sole quote disponibili.

`CLDimensionalDrawings` deve collegare immagine, orientamento e ambito
serie/modello. Le quote nuove devono essere normalizzabili come righe
lettera/valore/unita'/ordine, mantenendo come fallback le colonne legacy
`Dimension_A_*` ... `Dimension_D_*`.

Non eliminare le colonne legacy nella prima release. L'eventuale rimozione e'
un progetto separato dopo almeno una release di compatibilita'.

## Compatibilita' con dati legacy

- Interpretare `HorVariants` e `VerVariants` solamente nella migrazione e nei
  controlli di confronto.
- Generare relazioni normalizzate senza perdere le stringhe originali.
- Produrre un report di import con modello, layout, codice, orientamento,
  esito e anomalie.
- Non inventare la sequenza dei flussi quando non e' dimostrabile dai dati o
  dalla documentazione: marcare la configurazione come da validare.
- Un modello senza varianti esplicite puo' essere un modello con una sola
  configurazione, non necessariamente un errore.
- Confrontare conteggi e hash prima e dopo ogni migrazione.

## Explorer

Explorer deve offrire editor separati ma collegati:

1. catalogo immagini con anteprima e validazione MIME/dimensioni;
2. catalogo template e anchor dei quattro raccordi;
3. catalogo configurazioni con matrice delle quattro posizioni;
4. matrice modello/configurazione con filtri per serie, modello, layout,
   installazione e orientamento;
5. editor dei disegni dimensionali e delle quote;
6. anteprima finale identica alla composizione usata da SSW.

Validazioni bloccanti:

- quattro posizioni e quattro ruoli univoci;
- immagine valida e hash coerente;
- configurazione associata allo stesso layout aeraulico del template;
- default non duplicato alla stessa priorita' per modello;
- lato accesso, superficie e vista valorizzati;
- disegno dimensionale coerente con l'orientamento;
- nessun salvataggio parziale che faccia perdere le modifiche valide.

Le griglie devono seguire lo stile DevExpress gia' usato da Explorer.

## Export SDF

Estendere `CLSSWExporter` con uno step nominato e diagnosticabile, successivo
all'esportazione dei modelli recuperatore e precedente al manifest finale.

Requisiti:

- creare o aggiornare in modo idempotente le tabelle SQL CE;
- esportare solamente configurazioni e immagini raggiungibili dai modelli AV;
- includere traduzioni, template, anchor, porte, relazioni e quote;
- verificare gli hash delle immagini dopo la copia;
- incrementare `DatabaseSchemaVersion`, `ExporterVersion` e
  `MinimumSSWVersion` quando il contratto viene congelato;
- aggiornare automaticamente la DLL usata da Explorer con il meccanismo di
  sincronizzazione gia' presente nel progetto;
- estendere gli smoke test dell'esportatore;
- non lasciare un SDF parziale in caso di errore.

## Persistenza della selezione

Il file `.sswsel` deve salvare almeno:

- identificativo stabile e codice della configurazione;
- identificativo del layout aeraulico;
- tipo d'installazione e orientamento;
- lato accesso, superficie e vista;
- sequenza dei quattro flussi;
- identificativo/hash del disegno dimensionale;
- snapshot delle quote visualizzate.

Lo snapshot consente di ristampare una selezione storica anche quando il
catalogo viene aggiornato. In modifica si puo' proporre l'aggiornamento ai dati
correnti, ma non sostituire silenziosamente una configurazione non piu'
disponibile.

Il formato corrente e' V2. L'aggiunta richiede una migrazione esplicita
V2 -> V3 con default compatibile per i file precedenti. Un vecchio file senza
layout deve aprirsi e calcolare come oggi, mostrando la configurazione proposta
ma senza alterare il file finche' l'utente non salva.

## Report e portale

### Report

Aggiornare i quattro RDLC esistenti, senza introdurre un quinto report:

- `CLMainReport.rdlc`;
- `CLMainReport_Coil.rdlc`;
- `CLMainReportWithCO2.rdlc`;
- `CLMainReportWithCO2_Coil.rdlc`.

Il blocco deve contenere:

1. configurazione dei flussi con codice, installazione e schema localizzato;
2. lato accesso e superficie di riferimento;
3. disegno CAD quotato;
4. tabella quote a quattro o sei colonne, organizzata come coppie
   lettera/valore.

Il blocco non deve spezzarsi in modo incoerente tra pagine e non deve generare
pagine bianche. Le immagini devono essere renderizzate ad alta risoluzione con
proporzioni stabili, senza testo incorporato dipendente dalla lingua.

### Portale

Il payload online deve conservare tutti i dati della configurazione. Nel primo
rilascio il portale mostra solamente:

- codice configurazione;
- tipo installazione;
- orientamento;
- sequenza dei quattro flussi;
- lato di accesso.

La visualizzazione del disegno dimensionale nel portale e' fuori ambito e puo'
essere aggiunta senza cambiare il contratto del payload.

## Dipendenze con accessori

Una configurazione puo' richiedere accessori. Il riferimento funzionale indica
per esempio che alcune installazioni a pavimento richiedono `SHK`.

Riutilizzare il motore dipendenze del catalogo accessori:

- selezionare automaticamente l'accessorio obbligatorio;
- bloccarne la deselezione mentre la configurazione lo richiede;
- mostrare un tooltip localizzato con la motivazione;
- rimuoverlo automaticamente solo se era stato aggiunto dalla configurazione
  e non e' richiesto da altre scelte;
- riportarlo normalmente nel riepilogo e nel report accessori.

## Ondate operative

### Onda A - Censimento e congelamento contratto

Stima: 20-35 mila token, 0,5-1,5 giorni.

1. Backup verificabile delle tabelle e immagini coinvolte.
2. Individuare l'entita' fisica del layout aeraulico esistente.
3. Esportare inventario di modelli, Hor/VerVariants, quote, layout e immagini.
4. Classificare immagini per modello, serie, orientamento e vista.
5. Produrre elenco anomalie e configurazioni non determinabili.
6. Congelare DDL, enum e regole di precedenza.

Checkpoint A: inventario e mapping approvati; nessuna sequenza di flussi e'
stata inventata.

### Onda B - Database centrale ed Explorer

Stima: 55-90 mila token, 2-4 giorni.

1. Creare migrazione idempotente e rollback.
2. Importare tipi, template, configurazioni, porte e relazioni.
3. Creare editor immagini, configurazioni e matrice modello/configurazione.
4. Implementare anteprima e validazioni.
5. Applicare priorita' B6 -> A4 attraverso i dati.
6. Collegare eventuali requisiti accessori.

Checkpoint B: tutte le configurazioni possono essere gestite senza modificare
codice applicativo e i vincoli impediscono combinazioni invalide.

### Onda C - Export SDF e contratto offline

Stima: 35-60 mila token, 1-2 giorni.

1. Estendere schema SQL CE ed esportatore.
2. Copiare immagini e verificarne gli hash.
3. Aggiornare manifest e versioni minime.
4. Sincronizzare la DLL Explorer.
5. Esportare un SDF AV reale e verificarne dati e dimensioni.

Checkpoint C: Explorer esporta un SDF completo; SSW corrente continua ad
aprire senza regressioni finche' il nuovo tab non viene attivato.

### Onda D - SSW, UI e persistenza

Stima: 55-90 mila token, 2-3 giorni.

1. Creare repository dati read-only per layout e immagini.
2. Implementare il tab a due colonne.
3. Implementare filtri, default, fallback e anteprima.
4. Integrare dipendenze accessori.
5. Estendere modello progetto, snapshot, fingerprint e migrazione V2 -> V3.
6. Verificare apertura di file V1/V2 e round-trip V3.

Checkpoint D: selezione, salvataggio, riapertura e cambio modello conservano
una configurazione valida senza alterare i calcoli termodinamici.

### Onda E - Report, payload e portale

Stima: 45-80 mila token, 2-3 giorni.

1. Estendere dataset e quattro RDLC.
2. Aggiungere traduzioni statiche e testi dati.
3. Renderizzare schema ad alta risoluzione.
4. Estendere snapshot API e dettaglio portale testuale.
5. Collaudare PDF per tutte le combinazioni report esistenti.

Checkpoint E: PDF leggibile e senza pagine bianche; portale e file `.sswsel`
mostrano la stessa configurazione registrata.

### Onda F - Regressione e rilascio

Stima: 25-40 mila token, 1-2 giorni.

1. Eseguire matrice di test per orientamento, installazione e lingua.
2. Verificare default B6, fallback A4 e configurazione unica.
3. Verificare modelli privi di immagini o varianti.
4. Verificare accessori obbligatori e cambio configurazione.
5. Compilare AV/x86, generare installer e manifest.
6. Aggiornare changelog, roadmap, versioni, commit e push di tutti i repository.

Checkpoint F: release installabile e aggiornabile con database coerente.

## Strategia per agenti

Il lavoro puo' essere delegato in background, ma ogni agente deve possedere
un'area esclusiva per evitare modifiche sovrapposte.

Sequenza consigliata:

- Agente 1: Onda A e proposta DDL, senza modificare UI SSW.
- Agente 2, dopo Checkpoint A: Onda B su database ed Explorer.
- Agente 3, dopo il DDL definitivo: Onda C su CLDataCentralLib.
- Agente 4, dopo SDF verificato: Onda D su SSW.
- Agente 5, dopo modello progetto definitivo: Onda E su report e portale.
- Un solo agente integra e chiude Onda F.

Non eseguire in parallelo modifiche al modello dati, all'esportatore e al
consumer SSW prima che il DDL sia congelato. La parallelizzazione utile e'
limitata a censimento immagini, audit dati e preparazione fixture.

Ogni agente deve:

- leggere questa roadmap e aggiornare lo stato della propria onda;
- verificare il worktree prima di modificare file;
- preservare modifiche non proprie;
- usare migrazioni ripetibili e non modifiche manuali non documentate;
- non hardcodare sigle, famiglie, layout o percorsi immagine;
- compilare e testare il repository modificato;
- lasciare evidenze riproducibili e lista dei file toccati;
- non dichiarare completato un checkpoint senza i test previsti.

## Matrice minima di test

- modello con una sola configurazione;
- modello con B6 disponibile;
- modello senza B6 ma con A4;
- modello senza configurazioni a soffitto;
- stesso codice in due layout aeraulici differenti;
- configurazioni verticali e orizzontali;
- tutte e quattro le installazioni canoniche;
- accesso opposto e accesso eccezionalmente non opposto alla superficie;
- configurazione con accessorio obbligatorio;
- immagine modello, immagine serie e immagine mancante;
- file `.sswsel` V1, V2 e V3;
- cambio modello con configurazione non piu' disponibile;
- funzionamento offline completo;
- tutte le 12 lingue, incluse stringhe lunghe;
- quattro varianti RDLC, con e senza coil, CO2, rumore e accessori;
- layout su form ridotto e schermo ad alta DPI.

## Criteri di completamento

La feature e' completa quando:

- il cliente parte dal tipo d'installazione e non dalla sigla;
- compaiono solamente configurazioni valide per il modello;
- B6 e A4 sono priorita' dati e non codice;
- lo schema identifica senza ambiguita' flussi, accesso e superficie;
- quote e disegno sono coerenti con orientamento e configurazione;
- Explorer gestisce nuove configurazioni senza ricompilare SSW;
- SDF permette il funzionamento completamente offline;
- file storici si aprono senza errore o modifica silenziosa;
- report e portale riportano la configurazione effettivamente salvata;
- nessuna regressione interessa calcoli, batterie, accessori o grafici;
- documentazione, versioni, build, installer, manifest e Git sono allineati.

## Budget complessivo

Stima realistica: 190-330 mila token e 7-12 giorni tecnici, esclusi tempi di
attesa per validazione commerciale e disponibilita' delle immagini CAD.

Se le immagini devono essere ridisegnate o ricostruite, aggiungere 30-70 mila
token e 1-3 giorni. Conservare un margine del 20-30% per dati legacy,
impaginazione RDLC e anomalie nella relazione tra sigle e layout aeraulici.

