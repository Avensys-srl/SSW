# Drawing dimensionale

Data: 2026-09-08

Stato: **proposta da implementare**. Questo documento non attesta la presenza
di un visualizzatore dimensionale completo ne' la disponibilita' dei disegni.

## Richiesta e obiettivo

Il drawing non e' un PDF da scaricare: e' una sezione della selezione che mostra
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

## Decisione proposta

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

Preferire un SVG tecnico statico verificato, con tre viste, linee di quota e
lettere A/B/C/D, senza valori numerici incorporati. Questo evita varianti per
lingua o taglia. Un PNG ad alta risoluzione e' un'alternativa per asset legacy.
Non generare un disegno costruttivo attendibile dal solo schema dei flussi.

Explorer dovrebbe permettere importazione, anteprima, revisione e associazione
esplicita del disegno al modello e all'orientamento H/V. Consentire il riuso
dello stesso asset da piu' modelli solo tramite associazione deliberata.
Quando serve, aggiungere una variante per configurazione: non moltiplicare
le immagini quando il disegno e' identico e cambiano solamente le quote.

Metadati minimi proposti: identificativo stabile asset, revisione, MIME type,
dimensioni/viewBox, hash, contenuto, modello, orientamento e configurazione
opzionale. I nomi delle future tabelle sono da definire in Explorer, non sono
uno schema gia' approvato o implementato.

Validare SVG senza script, eventi, riferimenti esterni, font remoti o risorse
di rete; imporre limiti dimensionali. Il disegno deve funzionare offline.
Conservare il rapporto d'aspetto, evitando stiramenti e ritagli delle quote.

## Risoluzione dei dati

1. Validare modello e configurazione tra le relazioni attive del catalogo.
2. Leggere H/V e metadati della configurazione, non dedurli dal suo codice.
3. Risolvere l'asset esplicitamente associato al modello/orientamento e,
   quando presente, alla configurazione specifica.
4. Leggere A/B/C/D dal gruppo Hor o Ver corrispondente.
5. Restituire un DTO con asset/revisione, quote, orientamento, configurazione
   e anomalie. UI, report e salvataggio usano questo contratto comune.

Dato assente o non valido: mostrare quota non disponibile, non `0 mm` e non
un valore dell'altro orientamento. Asset assente: mostrare lo stato di
indisponibilita', senza disegno inventato. Stabilire separatamente se queste
assenze impediscono l'ordine o soltanto la stampa del drawing.

## UI e report

La voce Disegno dimensionale apre una sezione/pannello, non un link PDF fittizio.
Tre viste leggibili, tabella quote accanto su schermi ampi e sotto su quelli
stretti; zoom adatto all'ispezione del disegno. Localizzare titoli, stato,
nomi delle quote e unita', mantenendo A/B/C/D coerenti con l'asset.

Nel report usare il disegno proporzionato e una vera tabella delle quote,
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

## Sequenza e accettazione

1. Censire asset disponibili, corrispondenza A/B/C/D e modelli/orientamenti.
2. Concordare schema e associazioni in Explorer, poi export SDF e migrazione.
3. Implementare resolver backend, DTO e test con dati mancanti/invalidi.
4. Aggiungere visualizzazione UI e tabella report sullo stesso DTO.
5. Verificare SSC verticale e sdraiata, OSC parete nelle due viste, soffitto,
   modelli senza asset, assenza quote, nuova configurazione E2 e file storici.
6. Verificare offline, lingue, rapporto d'aspetto, stampa senza pagine vuote
   e assenza di regressioni nei calcoli e nelle selezioni salvate.

Non iniziare dal solo riquadro grafico: servono prima asset corretti e
associazioni autorevoli. Restano da approvare formato finale, disponibilita'
degli asset e regole di obbligatorieta' commerciale.
