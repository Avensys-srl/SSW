# Selezioni tecniche: informativa operativa e conservazione

## Finalita' e dati trattati

Quando viene generato un report definitivo, SSW registra sul servizio Avensys
il progetto tecnico necessario a identificare, riaprire e revisionare la
selezione. Sono inclusi gli input, gli output, le versioni dei componenti, il
riferimento pubblico opaco e gli identificativi tecnici casuali
dell'installazione. Non vengono richiesti nome o credenziali all'utilizzatore.

Il server ricava localmente dal solo indirizzo sorgente il codice nazione e una
citta' approssimativa. Salva esclusivamente questi due risultati, la sorgente
del database GeoIP e l'accuratezza dichiarata. L'indirizzo IP non viene scritto
nella selezione, negli snapshot o nell'audit applicativo e non viene inviato a
servizi esterni. Il database locale e' DB-IP City Lite e viene aggiornato con
procedura atomica.

I log HTTP dell'infrastruttura e il log separato dei download possono contenere
indirizzi IP per sicurezza e diagnosi. Non sono collegati al contenuto delle
selezioni tecniche.

## Tempi operativi

| Dato | Durata | Cancellazione |
| --- | --- | --- |
| Idempotenza e rate limit API | scadenza tecnica incorporata | giornaliera |
| Token scaduti o revocati | 30 giorni | giornaliera |
| Audit applicativo e amministrativo | 730 giorni | giornaliera, configurabile |
| Log download | 90 giorni | giornaliera |
| Selezioni, revisioni e geolocalizzazione associata | proposta: 10 anni dall'ultima revisione | non automatizzata finche' la policy non e' approvata |
| Installazione tecnica casuale | finche' referenziata da selezioni conservate | revoca immediata disponibile |

Il termine decennale e' una scelta organizzativa proposta per tracciabilita'
tecnica e commerciale, non una conclusione legale incorporata nel software.
Prima di attivare la cancellazione automatica Avensys deve validare finalita',
base giuridica e termini con il proprio referente privacy. Il criterio segue i
principi di minimizzazione e limitazione della conservazione dell'articolo 5
GDPR e rende esplicito il periodo o il relativo criterio richiesto
dall'articolo 13.

## Controlli

- Il pannello interno e' disabilitato finche' non viene configurato un hash di
  password esterno alla web root.
- Consultazioni ed esportazioni dal pannello sono registrate nell'audit.
- Gli snapshot sono immutabili; una modifica crea una nuova revisione.
- Il riferimento pubblico non espone il progressivo interno e non consente
  alcuna lettura pubblica.
- `purge_technical_selection_operational_data.php` elimina dati operativi e
  audit scaduti, mai selezioni o revisioni.
- `purge_download_log.php` applica la conservazione del log download.

## Testo breve localizzato

Le etichette applicative e di report restano nelle dodici lingue SSW. Il testo
breve da usare in eventuali schermate informative e' semanticamente:

> La selezione tecnica e le sue revisioni vengono registrate da Avensys. La
> localita' e' approssimativa e l'indirizzo IP non viene conservato nella
> selezione.

Le versioni approvate per `bg`, `da`, `de`, `en`, `fr`, `hu`, `it`, `nl`,
`pl`, `ro`, `sl` e `sv` sono mantenute in
`api/privacy/technical-selection-notice.json`.
