# Licenze account e dispositivi

Data: 16/09/2026
Stato: implementata localmente, pubblicazione server da eseguire

## Contratto approvato

- Avensys preautorizza nel portale l'email del cliente e genera un PIN numerico
  di sei cifre, monouso e valido sette giorni.
- Una nuova installazione richiede nome, cognome, email e PIN. Il PIN associa
  un solo dispositivo e non viene mai salvato in chiaro dal server.
- Ogni email puo' avere al massimo due dispositivi attivi, denominati
  `Dispositivo 1` e `Dispositivo 2`.
- Le installazioni gia' registrate con il precedente token API non richiedono
  PIN: al primo aggiornamento raccolgono nome, cognome ed email e vengono
  associate conservando l'identita' tecnica esistente.
- Una verifica online rinnova automaticamente una concessione offline di 30
  giorni. Entro tale data SSW continua a funzionare senza rete.
- A concessione scaduta SSW richiede la connessione. Una licenza revocata
  mostra il blocco e invita a contattare Avensys.
- La revoca puo' interessare l'intero utente o il singolo dispositivo. La
  riattivazione conserva l'identita' locale e non richiede un nuovo PIN.
- Il server conta gli avvii distinti e almeno un giorno di utilizzo per giorno
  UTC; il portale mostra contatori e ultimo utilizzo per utente e dispositivo.

## Confini tecnici

- Il token API resta protetto con DPAPI `LocalMachine`; lo stato licenza locale
  e' persistito separatamente e protetto con DPAPI.
- Il PIN non sostituisce il token e non e' una password permanente.
- Le API di selezione esistenti restano compatibili. Le nuove rotte sono
  additive: `license/activate`, `license/claim-legacy`, `license/check`.
- Nessun controllo licenza modifica formule, SDF, file `.sswsel` o report.
- Comandi tecnici e screenshot automatici non devono dipendere da finestre
  interattive; il gate e' applicato al normale avvio desktop.

## Gate di verifica

- [x] Schema server, rotte additive e test servizio API.
- [x] Gestione utenti, PIN, revoca, dispositivi e statistiche nel portale.
- [x] Stato DPAPI e client HTTP nel desktop.
- [x] Finestra di prima attivazione e migrazione installazione esistente.
- [x] Rinnovo silenzioso, uso offline, scadenza e revoca coperti dal contratto
  e dai test locali; collaudo reale post-deploy ancora necessario.
- [x] Test API e portale, frontend e build `AV|x86` completati il 16/09/2026.
- [ ] Avvio grafico del binario esatto contro API migrata.
- [ ] Migrazione applicata e smoke test sull'ambiente pubblicato.

## Checkpoint 16/09/2026

Implementati stato locale cifrato DPAPI, attivazione PIN, associazione legacy
limitata alle installazioni presenti al momento della migrazione, verifica
online, concessione offline, heartbeat ogni sei ore e blocco su revoca. La
sessione resta stabile durante l'esecuzione: i controlli ripetuti aggiornano
l'ultimo utilizzo senza incrementare piu' volte lo stesso avvio.

Verifiche riuscite: test servizio PHP API, 36 test portale, build frontend
TypeScript/Vite, `SelectionIdentitySmoke` con profilo cifrato e chiamate
licenza, build soluzione `AV|x86`. Il test grafico normale e' intenzionalmente
rinviato: prima deve essere applicata la migrazione 007 e pubblicata l'API,
altrimenti una nuova attivazione non puo' concludersi.

## Compatibilita' e rilascio

La migrazione database deve essere pubblicata prima del client. Fino a quel
momento le versioni precedenti continuano a usare le rotte storiche. Il nuovo
client non va distribuito finche' le nuove rotte non rispondono sul server di
produzione. La pubblicazione richiede backup database, prova di attivazione,
prova di secondo dispositivo, rifiuto del terzo, revoca/riattivazione e prova
offline con concessione valida e scaduta.
