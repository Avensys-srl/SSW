# Matrice di collaudo selezioni tecniche

## Gate automatici

| Area | Caso | Esito richiesto |
| --- | --- | --- |
| Build | `AV|x86` | zero errori |
| Progetto | V1 completo e V1 sparso | apertura, normalizzazione e round-trip |
| Progetto futuro | versione non supportata | rifiuto controllato, nessuna modifica |
| SDF | `Legacy-0`, schema 1 e schema 2 | apertura secondo feature dichiarate |
| SDF futuro/corrotto | schema oltre il massimo o manifest invalido | blocco prima di Entity Framework |
| Identita' | registrazione, cache, rinnovo DPAPI | nessun token in chiaro |
| API | 5.000 riferimenti, retry, concorrenza, R01/R02 | nessuna collisione o duplicato |
| Offline | 12 processi concorrenti | bozze locali univoche e contigue |
| Snapshot | ristampa, input, output, DLL e SDF modificati | classificazione revisione corretta |
| GeoIP | MMDB, fallback SQL e lookup fallito | solo nazione/citta'; report mai bloccato |
| Recupero interno | ricerca e download revisione | autenticazione e audit obbligatori |
| Aggiornamento | manifest, dimensione e SHA-256 | pacchetto alterato rifiutato |
| Localizzazione | 12 RESX e 4 RDLC | chiavi complete, XML valido |
| Report | inverno/estate, coil on/off, CO2/suono | nessuna pagina vuota o sezione errata |

## Collaudo manuale pilota

1. Installazione pulita su macchina Avensys e aggiornamento dalla release
   precedente.
2. Apertura di una fixture `.sswsel`, ricalcolo e salvataggio senza perdita di
   campi sconosciuti.
3. Report online registrato, ristampa invariata e revisione dopo una modifica.
4. Report offline in bozza e successiva registrazione online.
5. Verifica dei quattro layout RDLC in italiano, inglese, tedesco e svedese;
   controllo chiavi automatico per tutte le altre lingue.
6. Ricerca del riferimento nel pannello Avensys e download dello snapshot.
7. Aggiornamento con manifest valido e prova negativa su una copia alterata.
8. Rollback all'installer precedente conservando `.sswsel` e SDF compatibili.

## Evidenze di release

Ogni release conserva: output dei test, manifest JSON firmabile, hash installer,
metadati SDF, versione DLL di calcolo, esito health check API e riferimento
dell'installer precedente. Le prove grafiche usano sempre
`D:\mdev\SSW\SSW\bin\x86\AV\SSW.exe`.
