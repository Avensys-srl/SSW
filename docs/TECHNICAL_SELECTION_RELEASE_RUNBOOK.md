# Runbook di rilascio progressivo

## Ambienti

1. **Sviluppo**: build `AV|x86`, API test su SQLite e fixture locali.
2. **Pilota Avensys**: database/API reali, una installazione interna, report e
   recupero amministrativo.
3. **Produzione**: pubblicazione dell'installer solo dopo il gate pilota.

Il report definitivo richiede il riferimento server; in assenza di servizio
resta disponibile soltanto la bozza locale esplicitamente scelta dall'utente.

## Gate prima della pubblicazione

1. Eseguire `tests\Invoke-TechnicalSelectionReleaseTests.ps1`.
2. Eseguire il gate Help/UX con un agente dedicato: confrontare le modifiche
   dalla release precedente con guida, tooltip, menu e tutte le localizzazioni;
   validare chiavi XML, valori non vuoti e coerenza tecnica prima della build.
3. Verificare che l'SDF distribuito sia lo schema atteso, contenga le feature
   dichiarate dal manifest e abbia la stessa copertura linguistica di SSW per
   ogni catalogo localizzato. Rigenerarlo dalla sorgente centrale quando i dati
   o le lingue cambiano, senza correggere soltanto la copia nella build.
4. Generare installer e sidecar con `installer\build-installer.ps1`.
5. Eseguire `installer\Test-ReleaseReadiness.ps1` su installer e manifest.
6. Installare sul PC pilota, aprire una fixture storica, produrre R01, ristampa
   R01 e modifica R02.
7. Cercare R01/R02 nel pannello interno e scaricare lo snapshot JSON.
8. Pubblicare insieme `.exe` e `.manifest.json`; mai uno solo dei due.

## Osservabilita'

- `/api/health.php` controlla database, schema minimo corrente e MMDB locale senza
  esporre DSN, credenziali o dati cliente.
- `/api/ssw_check_update.php` dichiara `verified_manifest`, componenti,
  compatibilita' e rollback.
- Errori applicativi e richieste amministrative sono separati; l'audit non
  conserva IP.
- Il log download conserva timestamp/IP per 90 giorni ed e' separato dalle
  selezioni tecniche.

## Rollback

1. Conservare almeno installer e manifest della release precedente.
2. Togliere dalla directory pubblica entrambi i file della release difettosa;
   l'API tornera' automaticamente alla versione precedente.
3. Reinstallare il pacchetto precedente sul pilota.
4. Non retrocedere il database delle selezioni: le migrazioni server sono
   additive. Ripristinare da backup soltanto in caso di corruzione verificata.
5. Un progetto `.sswsel` storico resta apribile; un ricalcolo con componenti
   diversi genera una nuova revisione.

## Arresto del rollout

Interrompere la pubblicazione in caso di health `degraded`, manifest non
verificato, hash diverso, schema SDF futuro, perdita di dati nel round-trip,
duplicazione del riferimento o regressione dei quattro report. La release non
viene resa obbligatoria finche' il pilota non supera tutti i gate.
