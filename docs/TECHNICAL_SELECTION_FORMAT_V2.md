# Formato progetto SSW `.sswsel` V2

Data: 21/07/2026

Il formato V2 estende il contratto V1 con la selezione persistente di
accessori hardware e funzioni di controllo. Envelope, identita', scenari,
snapshot e regole di sicurezza restano quelli definiti dal formato V1.

## Blocco accessori

`selection.accessories` e' un array obbligatorio, eventualmente vuoto. Ogni
elemento selezionato contiene:

| Campo | Significato |
| --- | --- |
| `code` | Acronimo stabile usato per risolvere l'elemento nel catalogo SDF. |
| `itemType` | Tipo logico, per esempio `Accessory` o `ControlFunction`. |
| `quantity` | Quantita' selezionata, intero maggiore o uguale a 1. |
| `availability` | Stato storico: `Standard`, `Optional` o `Unavailable`. |
| `installationType` | Installazione storica: `Internal`, `External` o `NotApplicable`. |

Gli ID numerici del database non vengono persistiti. Il codice e' la chiave
stabile tra esportazioni SDF diverse.

Esempio:

```json
"accessories": [
  {
    "code": "KTS EXTRA",
    "itemType": "Accessory",
    "quantity": 1,
    "availability": "Optional",
    "installationType": "External"
  }
]
```

## Caricamento e compatibilita'

- Le scelte risolte nel catalogo corrente vengono ripristinate rispettando
  quantita', dipendenze, conflitti, elementi standard e livello KTS.
- Un codice non piu' presente nell'SDF viene conservato nel progetto durante
  il successivo salvataggio, evitando perdita silenziosa di informazioni.
- Gli elementi standard correnti restano sempre selezionati anche quando non
  erano presenti nel file storico.
- Un progetto V1 viene migrato in memoria a V2 con `accessories: []`.
- Al primo salvataggio sullo stesso percorso viene creata una copia
  `*.pre-migration-v1.bak` prima della sostituzione atomica.
- Un formato futuro continua a essere rifiutato senza modificare il file.

## Revisioni e API

Gli accessori fanno parte dell'input tecnico canonico. Cambiare codice o
quantita' modifica il fingerprint tecnico e genera una nuova revisione; una
ristampa invariata mantiene la revisione corrente.

Il payload di registrazione online include direttamente
`selection.Accessories`. Non e' richiesta una seconda struttura parallela nel
client o un nuovo endpoint API.

Per compatibilita' con i fingerprint V1, un array accessori vuoto viene omesso
soltanto durante la canonicalizzazione dell'hash. Un array non vuoto viene
sempre incluso.
