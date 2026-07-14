# Formato progetto SSW `.sswsel` V1

Data: 14/07/2026
MIME type proposto: `application/vnd.avensys.ssw-selection+json`

Il file e' JSON UTF-8 senza BOM. Il modello persistente e' indipendente da
WinForms, Entity Framework e RDLC.

## Envelope

Campi obbligatori alla radice:

- `format`: valore fisso `SSWSelection`;
- `selectionFormatVersion`: intero, valore `1`;
- `projectId`: UUID locale stabile del progetto;
- `createdAtUtc` e `modifiedAtUtc`;
- `versions`: base tecnica usata dal progetto;
- `selection`: input tecnici riapribili.

Campi opzionali:

- `features`: codici dei blocchi effettivamente usati;
- `identity`: riferimento bozza, riferimento pubblico e revisione;
- `snapshot`: output calcolati e relativa base di versione.

I token API non vengono salvati in chiaro nel progetto.

## Versioni

`versions` conserva:

- versione software e motore di calcolo;
- schema, revisione e hash logico dell'SDF;
- formato selezione, template report e contratto API.

Lo snapshot ha un proprio blocco `versions`, per distinguere gli input correnti
dall'ultima base di calcolo realmente eseguita.

## Riferimenti ai dati

Un'entita' proveniente dal database usa sempre la struttura:

```json
{
  "id": 123,
  "code": "CLRC 06A OSC",
  "managementCode": "CLRC-06A-OSC",
  "name": "CLRC 06A OSC"
}
```

Il codice stabile e' usato per risolvere l'entita' in un nuovo SDF. L'ID resta
come dato storico e diagnostico. Se il codice non esiste piu', lo snapshot puo'
essere ristampato ma il ricalcolo deve attendere una risoluzione esplicita.

## Input tecnici V1

`selection` comprende:

- cliente e riferimento libero del cliente;
- unita' selezionata;
- scenari inverno ed estate con portate, pressione, regolazione, temperature e
  umidita' in ingresso;
- batteria ad acqua con caso, installazione, modo, fluido, temperature acqua e
  geometria 2510;
- opzioni di report.

La portata di estrazione e' distinta dalla mandata gia' nel V1, anche se la UI
iniziale le mantiene sincronizzate. Questo evita di cambiare formato quando
verra' attivato lo sbilanciamento.

## Snapshot V1

`snapshot` puo' contenere:

- output termodinamici inverno ed estate;
- pressione disponibile, potenza assorbita ed efficienza;
- uno o piu' risultati batteria, identificati da scenario e modo;
- portata, velocita' e perdita di carico lato fluido;
- timestamp e versioni effettive del calcolo.

Lo snapshot non sostituisce gli input e non deve essere usato per un nuovo
calcolo senza prima risolvere i riferimenti contro l'SDF corrente.

## Lettura e scrittura

- Il salvataggio avviene su un file temporaneo nella stessa cartella e viene
  sostituito atomicamente.
- Il loader legge prima l'envelope e rifiuta formati futuri.
- JSON non valido, blocchi obbligatori mancanti o versioni incoerenti generano
  un errore controllato e non modificano il file.
- Il motore di migrazione dei formati precedenti verra' introdotto al punto 06
  della roadmap.

Fixture di riferimento: `docs/examples/selection-v1.sswsel`.
