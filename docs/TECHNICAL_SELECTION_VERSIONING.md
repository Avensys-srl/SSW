# Contratto di versionamento delle selezioni tecniche

Versione del contratto: 1.0
Data: 14/07/2026

Questo documento definisce le versioni che rendono riproducibili nel tempo i
calcoli SSW. Le versioni sono indipendenti: aggiornare un componente non deve
alterare artificialmente la versione degli altri.

## Identificatori di versione

| Campo | Formato | Responsabile | Incremento |
| --- | --- | --- | --- |
| `SoftwareVersion` | quattro interi `Major.Minor.Patch.Build` | SSW | A ogni release installabile. Il build cresce anche quando cambia soltanto la libreria di calcolo, l'SDF distribuito o il report. |
| `CalculationEngineVersion` | quattro interi `Major.Minor.Patch.Build` | libreria di calcolo | Quando cambia un algoritmo, una curva, una costante, una dipendenza o il contratto input/output che puo' modificare un risultato. |
| `DatabaseSchemaVersion` | intero positivo | CLDataCentralLib | Quando cambia la struttura logica dell'SDF. Gli SDF privi di manifest valgono `0` (`Legacy-0`). |
| `DatabaseDataVersion` | `yyyy.MM.dd.HHmmss-XXXXXXXX` UTC | CLDataCentralLib | A ogni insieme di dati esportato. `XXXXXXXX` e' il prefisso dell'hash logico: la revisione distingue anche esportazioni ravvicinate con contenuti diversi e non rappresenta lo schema. |
| `SelectionFormatVersion` | intero positivo | SSW | Quando cambia l'envelope o la semantica persistente del file `.sswsel`. |
| `ReportTemplateVersion` | intero positivo | SSW/RDLC | Quando cambia struttura, contenuto o logica condizionale del report. Correzioni puramente cosmetiche senza impatto informativo possono mantenere la versione. |
| `ApiContractVersion` | intero positivo nel path API | API | Quando cambia in modo incompatibile il contratto HTTP. Le estensioni opzionali compatibili non richiedono un nuovo major. |

Le versioni correnti introdotte dall'Onda A sono:

```text
DatabaseSchemaVersion = 1
SelectionFormatVersion = 1
ReportTemplateVersion = 1
ApiContractVersion = 1
```

Dal 21/07/2026 il formato progetto corrente e' `SelectionFormatVersion = 2`.
La relativa estensione e la migrazione da V1 sono descritte in
`TECHNICAL_SELECTION_FORMAT_V2.md`.

`SoftwareVersion` e `CalculationEngineVersion` sono lette dalle versioni reali
degli assembly distribuiti e non vengono duplicate come costanti testuali.

## Manifest SDF

Ogni nuovo SDF contiene una riga in `CLDatabaseMetadata` con:

- schema, revisione dati, versione exporter e UTC di esportazione;
- codice cliente e versione minima di SSW;
- `ContentHash`, SHA-256 del contenuto logico del database.

Il contenuto logico comprende nomi di tabelle, colonne, tipi e valori, inclusa
`CLDatabaseFeatures`; esclude soltanto `CLDatabaseMetadata`. Le righe sono
normalizzate e ordinate per hash, per rendere il risultato indipendente
dall'ordine fisico delle pagine SQL Compact.

`CLDatabaseFeatures` dichiara le capacita' dati con codice stabile, versione e
flag di abilitazione. L'assenza delle due tabelle identifica un database
`Legacy-0`; SSW deve inferire soltanto le feature legacy gia' supportate.

L'SDF distribuito e' immutabile e di sola lettura dal punto di vista di SSW.
Non sono ammesse migrazioni in place sul PC del cliente: una modifica allo
schema richiede una nuova esportazione completa.

## Compatibilita' SDF

SSW apre il database soltanto se:

1. il file e' leggibile e il cliente coincide;
2. lo schema e' legacy oppure rientra nell'intervallo supportato;
3. `MinimumSSWVersion` non e' superiore alla release in esecuzione;
4. le feature obbligatorie per una funzione sono presenti e abilitate.

Una feature opzionale assente disabilita soltanto la relativa funzione. Uno
schema futuro o un manifest corrotto interrompe l'apertura con un errore
controllato prima dell'inizializzazione Entity Framework.

## Compatibilita' dei progetti

Un `.sswsel` salva sia i codici gestionali stabili sia gli ID numerici usati al
momento del calcolo. Al caricamento si risolve prima il codice; l'ID e' un dato
storico e un aiuto diagnostico, non la chiave primaria tra database diversi.

Un progetto con `SelectionFormatVersion` futuro viene rifiutato senza essere
modificato. Le migrazioni dei formati supportati sono sequenziali e creano un
backup prima del primo salvataggio nel formato corrente.

Se un componente storico non esiste nel nuovo SDF, lo snapshot resta
ristampabile, ma il ricalcolo e' bloccato finche' l'utente non risolve la
selezione con un componente corrente.

## Regole per le revisioni tecniche

Una ristampa mantiene riferimento e revisione solo quando input tecnici,
output e tutte le versioni della base di calcolo coincidono con l'ultima
revisione registrata.

Nasce sempre una nuova revisione quando cambia almeno uno dei seguenti dati:

- input tecnico o opzione che influenza calcolo o report;
- `CalculationEngineVersion`;
- `DatabaseSchemaVersion`, `DatabaseDataVersion` o `ContentHash`;
- feature dati usata dalla selezione;
- output calcolato normalizzato;
- `ReportTemplateVersion`, se il contenuto tecnico pubblicato cambia.

Un aggiornamento della sola `SoftwareVersion` non crea una revisione se tutte
le versioni tecniche, gli input e gli output restano invariati. La release
installabile viene comunque registrata nell'audit.

## Default per i dati storici

Quando un vecchio progetto non contiene un blocco introdotto in seguito:

- portate sbilanciate: portata di estrazione uguale alla mandata;
- resistenza elettrica: disabilitata;
- batteria ad acqua: disabilitata;
- scenario estivo: comportamento storico a scenario singolo;
- opzioni report nuove: disabilitate, salvo campi obbligatori di legge.

Questi default appartengono al motore di migrazione del progetto e non devono
essere applicati implicitamente a una revisione gia' registrata.

## Regola di rilascio

Una modifica della DLL di calcolo, dei dati SDF o degli RDLC richiede una nuova
`SoftwareVersion`. Nella prima fase il rilascio e' sempre un installer completo
e atomico contenente componenti compatibili tra loro; non viene distribuita una
DLL isolata.
