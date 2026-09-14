# Drawing dimensionale

Data: 2026-09-08

Stato: **prima integrazione implementata e verificata**. Catalogo, associazioni,
export SDF e visualizzazione SSW sono disponibili; la stampa nel report resta
un'attivita' separata e non e' inclusa in questo checkpoint.

## Richiesta e obiettivo

Il drawing e' una sezione della selezione che mostra
il disegno tecnico specifico del modello, con tre viste e lettere sulle quote.
Accanto al disegno si mostrano i valori effettivi della configurazione scelta:

| Lettera nel disegno | Quota mostrata | Unita' |
| --- | --- | --- |
| A | L, lunghezza | mm |
| B | W, larghezza | mm |
| C | H, altezza | mm |
| D | Diametro attacchi | mm |

Il database contiene i valori distinti `Dimension_A_Hor` ... `Dimension_D_Hor`
e `Dimension_A_Ver` ... `Dimension_D_Ver`. Non ricavare A/B/C ruotando o
scambiando arbitrariamente i valori. Anche D va letto dal gruppo appropriato.

## Decisione adottata

Separare tre responsabilita': asset del disegno, geometria della configurazione
e quote numeriche del modello. La UI e il report devono consumare lo stesso
risultato risolto dal backend, senza introdurre altre liste di codici A1/B6/E2.

L'orientamento dimensionale H/V proviene dalla configurazione installativa
selezionata. Non coincide automaticamente con soffitto/pavimento/parete:
per esempio SSC a pavimento puo' essere sdraiata o svilupparsi in verticale.
`InstallationMode`, `Orientation`, `ReferenceView` e `AccessSide` hanno quindi
significati distinti e vanno conservati.

Il modello porta il layout aeraulico SSC/OSC e le proprie quote. Tale layout
non basta a identificare il disegno costruttivo: due modelli OSC possono avere
carter, attacchi e ingombri diversi. Nessun fallback a un disegno OSC generico
presentato come se fosse quello reale del prodotto.

## Asset e gestione in Explorer

Il formato autorevole adottato e' il PDF vettoriale esportato dal CAD: una
pagina A4 con vista landscape ottenuta tramite rotazione di 90 gradi, tre viste,
linee di quota e lettere A/B/C/D. Il PDF non contiene nome o taglia del modello,
perche' lo stesso asset puo' essere associato deliberatamente a piu' modelli.
Non generare un disegno costruttivo dal solo schema dei flussi.

Explorer dovrebbe permettere importazione, anteprima, revisione e associazione
esplicita del disegno al modello e all'orientamento H/V. Consentire il riuso
dello stesso asset da piu' modelli solo tramite associazione deliberata.
Quando serve, aggiungere una variante per configurazione: non moltiplicare
le immagini quando il disegno e' identico e cambiano solamente le quote.

I metadati sono persistiti in `CLDimensionalDrawings`; le relazioni riusabili
modello/disegno sono in `CLHeatRecoveryModelDimensionalDrawings`. Lo scope `B`
vale per entrambi gli orientamenti, mentre `H` e `V` permettono override mirati.
Il catalogo memorizza revisione, MIME type, dimensioni pagina, rotazione, hash
SHA-256, contenuto PDF e stato attivo.

Explorer valida intestazione PDF, pagina A4 landscape e limite di 20 MB. Il
disegno e' incluso nell'SDF e funziona offline. Il visualizzatore conserva il
rapporto d'aspetto, evitando stiramenti e ritagli delle quote.

## Risoluzione dei dati

1. Validare modello e configurazione tra le relazioni attive del catalogo.
2. Leggere H/V e metadati della configurazione, non dedurli dal suo codice.
3. Risolvere l'asset esplicitamente associato al modello/orientamento e,
   quando presente, alla configurazione specifica.
4. Leggere A/B/C/D dal gruppo Hor o Ver corrispondente.
5. Restituire un DTO con asset/revisione, quote, orientamento, configurazione
   e anomalie. UI, report e salvataggio usano questo contratto comune.

Dato assente, non valido o uguale a zero: non mostrare la relativa riga nella
schermata dimensionale e non usare un valore dell'altro orientamento. I dati
di pallet e imballaggio restano nel database e nell'SDF, ma non sono mostrati
in questa schermata. Asset assente: mostrare lo stato di
indisponibilita', senza disegno inventato. Stabilire separatamente se queste
assenze impediscono l'ordine o soltanto la stampa del drawing.

## UI e report

La voce Disegno dimensionale apre una sezione/pannello, non un link PDF fittizio.
Tre viste leggibili, tabella quote accanto su schermi ampi e sotto su quelli
stretti; zoom adatto all'ispezione del disegno. La UI espone soltanto le sigle
commerciali L/W/H/D, mantenendo internamente la corrispondenza A/B/C/D del
database e dell'asset.

Dal riquadro ingrandito si puo' esportare un PDF A4 landscape autonomo: il
disegno renderizzato conserva le proporzioni e una legenda L/W/H/D viene
posizionata in basso a destra. L'esportazione non mostra codice, revisione o
nome del PDF sorgente, poiche' uno stesso asset puo' servire piu' modelli.

Quando verra' introdotto nel report, usare il disegno proporzionato e una vera tabella delle quote,
non una cattura della UI con numeri e testi incollati. Riutilizzare il medesimo
DTO per prevenire le divergenze gia' osservate nello schema di installazione.

Per riproducibilita' delle selezioni registrate, conservare riferimento e
revisione/hash del disegno, orientamento e quote utilizzate nello snapshot;
definire la migrazione dei file storici senza alterarli silenziosamente.

## Punti di integrazione verificabili

- `SSWLib/CLInstallationLayoutRepository.vb`: snapshot di installazione,
  `HorizontalDimensions`, `VerticalDimensions`, `DimensionalImage` e anomalie.
- `frontend/ssw-next/src/bridge/contracts.ts`: contratti tipizzati della UI.
- `frontend/ssw-next/src/bridge/index.ts`: mapping backend/frontend.
- `frontend/ssw-next/src/main.ts`: sezione documenti e disegno dei flussi.
- `SSWLib/CLInstallationLayoutReportRenderer.vb`: schema installativo;
  non e' il disegno dimensionale costruttivo e non lo sostituisce.
- Explorer ed esportatore SDF: gestione e pubblicazione offline degli asset;
  lavorare sui relativi repository sorgente, non sui programmi compilati.

## Checkpoint 08/09/2026

- Explorer 2.1.20.0 gestisce catalogo PDF, anteprima, matrice modello/disegno,
  associazioni H/V e applicazione in blocco per serie.
- `PRIME-3V-V7.2-BD` rev. `V7.2-BD` e' associato con scope `B` a tutti i
  modelli della serie PRIME presenti nel database centrale.
- CLDataCentralLib exporter 1.4.0 pubblica schema SDF 5 e feature
  `DimensionalDrawings`, includendo solo asset attivi raggiungibili.
- SSW 1.3.0.59 risolve l'override H/V prima del fallback B, renderizza la prima
  pagina PDF come immagine integrata nella schermata Installazione e mostra
  L/W/H/D senza inventare valori mancanti. Il clic sull'anteprima apre la
  stessa immagine ingrandita, senza toolbar o nome file, e consente di salvare
  il PDF autonomo con legenda in basso a destra.
- Il formato selezione 3 conserva nello snapshot orientamento, riferimento
  tecnico dell'asset, hash e quote A/B/C/D. Non incorpora il PDF nel progetto.
- Il PDF campione e' versionato in
  `documents/assets/dimensional/CLRC_Prime_30_V7.2_BD.pdf` nel repository
  Explorer; nell'SDF viene memorizzato una sola volta e riusato per relazione.

## Quote aggiuntive e schema 6

Le quote storicamente conservate nella tabella MySQL `docdata.model` sono ora
centralizzate in SQL Server. `CLHeatRecoveryModelDimensions` contiene ingombri
base e override H/V; `CLHeatRecoveryModelPackaging` contiene dimensioni e pesi
di imballaggio. Zero non rappresenta una quota: durante la migrazione viene
convertito in `NULL`.

L'exporter 1.5.0 pubblica le due tabelle nel database SQL Server Compact con
schema 6 e feature `AdditionalModelDimensions`. SSW mantiene A/B/C/D come quote
del drawing costruttivo e presenta le quote aggiuntive separatamente, scegliendo
gli override coerenti con l'orientamento installativo. Il peso totale e' sempre
derivato da numero unita', peso unitario centrale e peso pallet.

## Sequenza residua e accettazione

1. Censire asset disponibili, corrispondenza A/B/C/D e modelli/orientamenti.
2. Estendere il catalogo quando arrivano altri PDF e associare gli scope H/V.
3. Ampliare i test con dati mancanti, hash non valido e relazioni conflittuali.
4. Aggiungere la tabella al report usando lo stesso DTO, quando richiesta.
5. Verificare SSC verticale e sdraiata, OSC parete nelle due viste, soffitto,
   modelli senza asset, assenza quote, nuova configurazione E2 e file storici.
6. Verificare offline, lingue, rapporto d'aspetto, stampa senza pagine vuote
   e assenza di regressioni nei calcoli e nelle selezioni salvate.

Il formato e il percorso dati sono ora definiti. Restano da decidere le regole
di obbligatorieta' commerciale e il momento in cui includere il drawing nei
report tecnici.
