# Protezione SSW: piano operativo e migrazione trasparente

Data: 30/09/2026.

Stato: analisi statica e piano operativo. Le nuove protezioni qui descritte NON
sono implementate o verificate. Questo intervento modifica solo documentazione:
nessun rilascio, revoca, modifica ai dati dei clienti o ai calcoli.

## 1. Obiettivo e confini

Rendere difficile copiare e riutilizzare DLL, catalogo e credenziali SSW fuori
dalle installazioni autorizzate, mantenendo la selezione tecnica offline e
attivando la nuova protezione senza PIN aggiuntivi sui PC gia' autorizzati.

Non promettere un software locale inviolabile. L'obiettivo verificabile e'
bloccare la copia ordinaria, restringere l'accesso ai nuovi dati, rilevare gli
abusi osservabili e aumentare il costo delle manomissioni, senza compromettere
l'affidabilita' dei risultati tecnici o il lavoro dei clienti.

### Decisioni gia' espresse dall'utente

- Autorizzazione della libreria di calcolo tramite licenza firmata dal server,
  associata al dispositivo, non affidata al solo avvio dell'applicazione.
- Massimo due dispositivi per utente, gestiti dal portale esistente.
- Un nuovo PC segue il flusso PIN/licenza; il vecchio dispositivo si revoca
  quando necessario. La migrazione sullo stesso PC non occupa un terzo posto.
- Le installazioni legittime esistenti migrano automaticamente con
  l'aggiornamento, senza una nuova registrazione ordinaria.
- Nessuna modifica delle formule per ragioni di protezione.

### Proposte da validare prima di implementarle

- Mantenere inizialmente la concessione offline di 30 giorni gia' prevista
  dall'architettura attuale; non richiedere una chiamata online per ogni calcolo.
- Usare una chiave dispositivo non esportabile tramite TPM dove disponibile;
  definire esplicitamente il trattamento dei PC senza TPM utilizzabile.
- Proteggere il catalogo a riposo e la distribuzione delle sue chiavi, previa
  verifica della compatibilita' con SQL Server Compact, .NET Framework 4.8 e x86.
- Distribuire prima in osservazione/pilota, poi rendere obbligatoria la nuova
  autorizzazione. La data di chiusura dei canali legacy segue il collaudo.

### Limiti che devono rimanere espliciti

1. Una vecchia DLL con un vecchio SDF gia' copiati non possono essere richiamati
   o resi inutilizzabili retroattivamente da un aggiornamento non installato.
2. Anche sul PC autorizzato qualcuno puo' voler riutilizzare la DLL per
   automatizzare calcoli o offrirli ad altri. Non e' solo un problema di copia
   su un altro computer.
3. Il TPM protegge una chiave; non dimostra che DLL, processo o risultati siano
   originali. I dati decifrati devono comunque essere utilizzabili dal calcolo.
4. Una firma server impedisce di fabbricare una licenza valida, non di tentare
   di modificare il codice locale che la verifica.
5. Un hash dichiarato dal client, anche firmato dalla chiave del PC, non e' una
   prova affidabile dell'integrita' del processo. Non chiamarlo attestazione.
6. Cifrare lo stesso catalogo per ogni PC limita la copia del contenitore; una
   sua estrazione in chiaro puo' comunque esporre i dati comuni ad altri PC.
7. Offline, revoca immediata e controllo affidabile dell'orologio non sono
   garantibili contro chi controlla macchina, snapshot e codice eseguito.
8. Portare dati e calcoli esclusivamente sul server evita di distribuirli al
   client, ma gli utenti autorizzati possono ancora automatizzare le risposte.
   Sarebbe una diversa decisione di prodotto, incompatibile con il pieno offline.

## 2. Evidenze nel sorgente e verifiche mancanti

Ispezione locale del 30/09/2026. Sono osservazioni sul codice, non un penetration
test ne' la dimostrazione che la segnalazione di DLL craccata sia fondata.
Non sono state provate richieste anonime al server di produzione.

| ID | Evidenza | Conseguenza per il piano |
| --- | --- | --- |
| E01 | [CLProgram.cs](../SSW/CLProgram.cs): controllo licenza condizionato alla modalita' di avvio; alcuni runner precedono il controllo, mentre il percorso finale apre l'host. Catalogo e ambiente vengono inizializzati prima del gate. | Verificare e chiudere i percorsi non autorizzati, compresi argomenti non riconosciuti; separare bootstrap e apertura dati. |
| E02 | [CLNextUiApplicationService.vb](../SSWLib/CLNextUiApplicationService.vb) e [CLSelectionApplicationService.vb](../SSWLib/CLSelectionApplicationService.vb): servizi pubblici di selezione/calcolo non subordinati direttamente al gate di startup. | Il controllo solo nell'eseguibile non costituisce il confine della libreria. Mappare anche gli accessi sottostanti. |
| E03 | [CLSelectionCredentialStore.vb](../SSWLib/CLSelectionCredentialStore.vb): identificatore installazione generato come GUID; bearer protetto con DPAPI LocalMachine. [CLDeviceLicense.vb](../SSWLib/CLDeviceLicense.vb): stato locale protetto con DPAPI. | Il GUID non e' identita' hardware. DPAPI e' difesa locale, non una licenza firmata dal server o una prova di integrita'. |
| E04 | API `A:\webavensys\api\v1\lib\TechnicalSelectionService.php`: concessione di 30 giorni, stato utente/installazione, due slot e rinnovo server; risposta attuale non equivalente a una concessione offline firmata autonoma. | Estendere il sistema esistente, senza creare un secondo archivio licenze; conservare le transazioni per i due slot. |
| E05 | [CLEnvironment.vb](../SSWLib/CLEnvironment.vb) e altri consumatori SDF contengono una credenziale catalogo condivisa nel codice. | La password comune non puo' essere trattata come segreto duraturo. Non riportarla in documenti, log o test. |
| E06 | [CLDatabaseCatalogUpdater.cs](../SSW/CLDatabaseCatalogUpdater.cs): manifest/download senza credenziali dispositivo nel client, verifica dimensione/hash, file temporaneo e backup precedente. | Verificare ACL reali del server; progettare autenticazione, firma del manifest, protezione di staging e rollback. Non dedurre dal solo client che ogni URL sia pubblico. |
| E07 | [SSW.iss](../installer/SSW.iss): inclusione ricorsiva dei file con esclusioni; [build-installer.ps1](../installer/build-installer.ps1): percorso di warning quando la firma non e' disponibile. | Creare uno staging di produzione controllato e rendere bloccante l'assenza di firma nel rilascio pubblico. |
| E08 | [UpdateManager.vb](../SSWLib/UpdateManager.vb): controllo dimensione/SHA-256 dell'installer rispetto ai metadati. | Aggiungere autenticita' del manifest e verifica del publisher, senza scambiare un hash per una firma. |
| E09 | Portale `A:\webavensys\ssw-portal\src\LicenseRepository.php`: stati dispositivo, revoca e limite di due dispositivi attivi. | Integrare chiavi, migrazione e recupero in questo flusso; evitare nuove autorizzazioni implicite. |
| E10 | Soluzione legacy .NET Framework 4.8/x86, SQL CE/SDF e componenti nativi; contratti di report e progetti gia' presenti. | La protezione non autorizza una riscrittura del motore, una conversione DB o una rottura dei progetti salvati. |

Prima delle decisioni irreversibili mancano: campione della presunta copia,
inventario dei canali reali di distribuzione, disponibilita' TPM sui PC clienti,
prova del provider SDF con chiavi variabili, costi del catalogo per dispositivo,
verifica dei permessi server e del runtime effettivamente in produzione.

## 3. Architettura proposta e rischio residuo

Flusso previsto:

1. L'installazione possiede una chiave dispositivo e registra la chiave pubblica
   nello stesso record licenza del portale.
2. Il server verifica utente, slot, stato e prova di possesso della chiave;
   rilascia una concessione firmata con diritti e scadenza.
3. Il catalogo protetto e il materiale necessario alla sua apertura sono
   consegnati solo al dispositivo autorizzato, secondo l'esito della prova S06.
4. La libreria verifica concessione e disponibilita' dei dati autorizzati nel
   percorso applicativo condiviso. Il normale calcolo resta locale.
5. Il rinnovo in background aggiorna la concessione; il server puo' rifiutare
   rinnovi e nuovi dati, ma non cancellare copie offline gia' sottratte.

Non usare password statiche condivise fra EXE e DLL, protocolli crittografici
inventati o semplici identificatori di disco/MAC come chiavi segrete.

| Scenario | Protezione da realizzare | Rischio residuo |
| --- | --- | --- |
| Copia di cartella e stato licenza su altro PC | Chiave dispositivo, concessione firmata, catalogo vincolato | Manomissione del codice locale; dati gia' estratti |
| Furto del solo bearer | Prova di possesso sulle operazioni sensibili; rotazione controllata | Fase iniziale di migrazione e compromissione del PC autorizzato |
| Chiamata diretta alla DLL | Gate nei servizi/dati condivisi e riduzione delle API esposte | Uso abusivo sul PC autorizzato; patch del gate |
| Sostituzione DLL/installer | Firma e manifest autentici, verifica publisher, packaging controllato | Il controllo eseguito sul PC compromesso resta aggirabile |
| Recupero catalogo da download o backup | Distribuzione autenticata e protezione a riposo/staging | Esposizione in memoria e vecchi cataloghi distribuiti |
| Revoca di un utente | Stop rinnovi/nuovi dati e applicazione locale dello stato ricevuto | Offline fino alla scadenza; verifier alterato |
| Automazione con account valido | Limiti server, audit e gestione anomalie | Calcoli completamente offline non osservabili dal server |

## 4. Operazioni da eseguire

Tutte le operazioni sotto sono DA FARE. P0 = prerequisito di sicurezza o
decisione bloccante; P1 = necessario prima della distribuzione generale;
P2 = irrobustimento successivo. Il responsabile indicato e' un ruolo da assegnare.
Un gate si chiude solo con prove salvate nel repository, non con il solo build.

### S00 - Perimetro, inventario e baseline

Priorita': P0. Responsabili: prodotto, desktop, backend. Dipendenze: nessuna.

- [ ] Acquisire con autorizzazione il campione della presunta DLL riutilizzata,
  versione, hash e modalita' osservata; distinguere estrazione, copia e patch.
- [ ] Inventariare DLL gestite/native, API pubbliche, SDF, snapshot, esportazioni,
  configurazioni, helper, installer, feed, archivi e copie distribuite.
- [ ] Separare il valore da proteggere: algoritmo, catalogo completo, nuovi dati,
  accesso al servizio, licenze. Documentare quali elementi sono gia' pubblici.
- [ ] Salvare baseline dei risultati numerici e dei flussi UI/report/progetti;
  assegnare i responsabili e fissare ambienti di test separati dalla produzione.

Gate: inventario versionato, minacce concordate, baseline riproducibile e
segnalazione classificata senza assumere un attacco non dimostrato.

### S01 - Politica licenze e comportamento per il cliente

Priorita': P0. Responsabile: prodotto. Dipendenze: S00.

- [ ] Confermare 30 giorni offline iniziali; definire rinnovo anticipato,
  preavvisi, gestione di una prolungata indisponibilita' del servizio e SLA.
- [ ] Distinguere timeout/rete assente da revoca autentica, scadenza e stato
  corrotto: un errore di rete non deve revocare una licenza ancora valida.
- [ ] Definire il comportamento dopo scadenza: niente nuovi calcoli protetti,
  ma accesso e recupero dei progetti/documenti dell'utente quando possibile;
  nessuna cancellazione e nessuna restituzione silenziosa di risultati falsi.
- [ ] Decidere politiche per TPM assente, VM, account Windows diverso,
  reinstallazione del sistema, sostituzione scheda madre e azzeramento TPM.
- [ ] Definire migrazione silenziosa ordinaria e casi eccezionali che richiedono
  assistenza; non promettere assenza di interventi su qualunque PC guasto/clonato.

Gate: tabella approvata stato/evento/azione, messaggi multilingua e responsabilita'
di assistenza. Nessuna dipendenza online per singolo calcolo introdotta di nascosto.

### S02 - Confine di autorizzazione nel client e nella DLL

Priorita': P0. Responsabile: desktop/libreria. Dipendenze: S00-S01.

- [ ] Mappare ogni avvio e accesso al motore: host, CLI, runner, API pubbliche,
  repository dati, componenti legacy/native e metodi richiamabili direttamente.
- [ ] Rifiutare gli argomenti di avvio sconosciuti e richiedere autorizzazione
  su ogni percorso di produzione, non solo sull'avvio senza argomenti.
- [ ] Separare runner e strumenti di sviluppo dall'artefatto pubblico. I test
  devono usare identita'/chiavi di test o artefatti interni distinti; nessun
  flag segreto, variabile d'ambiente o bypass di test nel pacchetto cliente.
- [ ] Introdurre un contesto di autorizzazione condiviso per servizi e dati;
  non fidarsi di un booleano o parametro di licenza fornito dal chiamante.
- [ ] Controllare la concessione anche nelle sessioni lunghe, senza chiamate
  HTTP per formula e senza cambiare arrotondamenti, selezione o prestazioni.

Gate: test di tutti gli ingressi, compresa chiamata diretta senza UI, e stesso
risultato numerico a parita' di input/dati. Questo ostacola l'uso ordinario non
autorizzato, ma non e' una prova di resistenza assoluta alle patch.

### S03 - Chiavi server, formato delle concessioni e fiducia

Priorita': P0. Responsabili: backend, sicurezza, operations. Dipendenze: S01.

- [ ] Separare chiavi di firma licenze, certificato di firma software e chiavi
  catalogo; separare inoltre ambienti di test e produzione.
- [ ] Definire un formato standard firmato con algoritmi ammessi esplicitamente:
  emittente, destinatario/prodotto, ID utente/installazione, impronta chiave
  dispositivo, diritti, emissione/scadenza, versione protocollo e identificativo
  chiave di firma. Il payload non deve contenere segreti da rendere pubblici.
- [ ] Scegliere librerie mantenute e compatibili con .NET 4.8 e backend reale;
  gestire serializzazione, errori e rotazione senza crittografia artigianale.
- [ ] Custodire le chiavi private server fuori da repository, web root, pacchetti
  e log, con accessi minimi, backup protetto e audit dell'uso.
- [ ] Progettare rotazione, sovrapposizione temporanea delle chiavi pubbliche,
  revoca per compromissione e recupero dei client offline di lungo periodo.

Gate: concessioni valide accettate; firma alterata, prodotto/dispositivo errato,
chiave sconosciuta e scadenza rifiutati; prova documentata di rotazione e ripristino.

### S04 - Identita' dispositivo e prova di possesso

Priorita': P0. Responsabili: desktop, backend. Dipendenze: S01, S03.

- [ ] Censire il parco e provare una chiave non esportabile tramite provider TPM;
  definire autorizzazioni locali e ciclo di vita senza basarsi su seriali fragili.
- [ ] Se il server deve distinguere una chiave realmente TPM da una software,
  implementare/verificare attestazione adeguata. Non fidarsi del campo "TPM=true".
- [ ] Associare la chiave pubblica al record installazione esistente. Conservare
  limite di due slot, vincoli DB, transazioni e gestione richieste simultanee.
- [ ] Richiedere prova di possesso legata a richiesta/operazione e a challenge
  monouso o equivalente anti-replay su rinnovo, consegna chiavi e accessi sensibili;
  non lasciare una rotta alternativa che accetti il solo vecchio bearer.
- [ ] Consentire rotazione con la chiave precedente o procedura controllata del
  portale; la conoscenza del GUID non deve autorizzare una nuova chiave.
- [ ] Documentare l'eventuale fallback software come protezione piu' debole;
  verificare anche l'uso della chiave da processi locali non autorizzati.

Gate: copia semplice su altro PC non funzionante; replay rifiutato; terzo slot
rifiutato anche in concorrenza; recupero autorizzato riproducibile. Il possesso
della chiave non viene presentato come attestazione dell'intero software.

### S05 - Migrazione silenziosa e bootstrap

Priorita': P0. Responsabili: desktop, backend, portale. Dipendenze: S02-S04, prova S06.

- [ ] Pubblicare prima estensioni additive server/DB, mantenendo il servizio
  operativo durante il pilota. Preparare backup e ripristino delle associazioni.
- [ ] Usare l'identita' e l'autorizzazione attuali per migrare lo stesso record:
  nessun nuovo utente, PIN ordinario o consumo di un altro posto dispositivo.
- [ ] Rendere il primo abbinamento idempotente e atomico, con stato server
  persistente; dopo l'abbinamento non accettare un secondo abbinamento diverso
  giustificato soltanto dalle vecchie credenziali.
- [ ] Gestire il rischio iniziale: un bearer legacy gia' rubato puo' competere
  per il primo abbinamento. Stabilire controlli/rischio accettato e revisione dei
  conflitti; il vecchio GUID non permette di dimostrare con certezza il PC fisico.
- [ ] Separare bootstrap da catalogo: configurazione minima, localizzazione e
  rinnovo licenza devono funzionare prima di aprire l'SDF protetto. Evitare la
  dipendenza circolare "serve il DB per autorizzare l'apertura del DB".
- [ ] Preparare nuovo stato/chiave/catalogo e verificarli prima del passaggio;
  registrare il completamento solo dopo un calcolo di controllo riuscito.
- [ ] Provare arresto, rete interrotta e riavvio in ogni fase. Un eventuale periodo
  transitorio e' limitato dal server, non prorogabile all'infinito reinstallando.
- [ ] Dopo la migrazione completa, impedire per quel record rinnovi/consegna di
  nuovi dati tramite il protocollo legacy; un fallimento non deve creare un bypass.

Gate: migrazione dei due PC gia' attivi senza prompt PIN e senza nuovi slot;
interruzioni recuperabili; conflitti visibili al gestore, non autorizzati ciecamente.

### S06 - Catalogo SDF e protezione dei dati a riposo

Priorita': P0 per fattibilita', P1 per integrazione. Responsabili: desktop/dati/backend.
Dipendenze: S00-S01; distribuzione definitiva dopo S03-S05.

- [ ] Prototipare su copie di test l'apertura del catalogo con segreto variabile,
  enumerando tutti i consumatori SQL CE, Entity Framework, calcolatori e componenti
  nativi. Verificare quali primitive e protezioni offre realmente il provider.
- [ ] Non approvare un contenitore cifrato che al primo avvio esporta un SDF
  interamente in chiaro su disco. Se il provider lo impone, riportare il limite
  e decidere un'alternativa prima di promettere la protezione.
- [ ] Confrontare chiavi catalogo per dispositivo e chiave contenuto comune
  incapsulata per dispositivo: quest'ultima semplifica la distribuzione ma
  l'estrazione di una sola chiave puo' esporre tutte le copie di quel contenuto.
- [ ] Definire formato autenticato/firma dei dati e gestione delle chiavi con
  primitive standard; non confondere la sola password SQL CE con integrita'
  crittografica o con la protezione hardware dell'intero database.
- [ ] Misurare generazione/consegna per dispositivo, caching server, dimensioni,
  RAM x86, tempi di avvio e tempi di selezione prima della scelta finale.
- [ ] Eliminare la credenziale universale dai nuovi percorsi protetti e gestire
  in modo coerente temporanei, backup `.previous`, rollback e cache di aggiornamento.
- [ ] Ridurre esposizione di chiavi/dati in log e dump prodotti dall'applicazione;
  non promettere di impedire tutti i dump creati dal sistema o da un amministratore.
- [ ] Distinguere versione/hash logico del catalogo e hash del contenitore cifrato,
  per mantenere tracciabilita' e riproducibilita' delle selezioni.
- [ ] Conservare formule e schema ove possibile. Un cambio DB/runtime necessita
  di decisione separata, migrazione e nuova matrice di regressione.

Gate: prova end-to-end di autorizzazione, apertura e aggiornamento senza segreti
comuni nel nuovo pacchetto; risultati tecnici identici; nessun SDF in chiaro creato
dal nuovo flusso. Limiti delle vecchie copie e del dato in memoria documentati.

### S07 - Distribuzione catalogo e protezione dei servizi

Priorita': P0 per inventario accessi, P1 per chiusura controllata.
Responsabili: backend/operations. Dipendenze: S03-S06 per il nuovo protocollo.

- [ ] Verificare accesso reale a manifest, SDF, export, mirror, backup, cache e
  percorsi di test; catalogare anche link storici e copie non gestite dal client.
- [ ] Tenere catalogo master e chiavi fuori dalla web root. Distribuire soltanto
  artefatti previsti a utenti/dispositivi con diritto valido.
- [ ] Proteggere consegna chiavi e download con autorizzazione server e prova di
  possesso. Se usati, URL temporanei consegnano solo contenuto cifrato, con durata
  limitata; non sostituiscono l'autorizzazione della chiave o della licenza.
- [ ] Separare autorizzazioni per account/dispositivo/prodotto e verificare
  accessi incrociati; limitare tentativi PIN, attivazione, rinnovi e download.
- [ ] Validare TLS e certificati senza fallback permissivi; distinguere risposte
  server autentiche dagli errori di trasporto. Validare schema, dimensioni e
  campi consentiti delle richieste prima di modificare licenze o consegnare dati.
- [ ] Rivedere accessi amministrativi, MFA, privilegi, sessioni, CSRF e audit del
  portale; verificare runtime e dipendenze server effettivi e relativo supporto.
- [ ] Definire rate limit basati sui flussi legittimi, metriche, allarmi e
  revisione umana, evitando revoche automatiche per semplici picchi o IP condivisi.
- [ ] Chiudere distribuzione dei NUOVI dati in chiaro e rotte legacy dopo il
  pilota/migrazione; non interrompere senza preavviso i clienti ancora legittimi.

Gate: test su ambiente isolato di accesso anonimo, cross-account, revoca, replay
e limiti; piano esplicito per gli URL legacy. Le copie gia' diffuse restano un limite.

### S08 - Offline, scadenza e revoca

Priorita': P1. Responsabili: desktop/backend. Dipendenze: S01, S03-S05.

- [ ] Verificare firma, diritti, prodotto, dispositivo e validita' anche offline;
  usare cache protetta e rinnovo in background con retry/backoff controllati.
- [ ] Non estendere localmente una scadenza perche' il server non risponde;
  eventuali concessioni di emergenza devono essere firmate e tracciate.
- [ ] Usare riferimenti temporali server autenticati e rilevare arretramenti
  semplici dell'orologio; dichiarare i limiti contro rollback completi/snapshot.
- [ ] Applicare le scadenze nelle sessioni gia' aperte e gestire revoca ricevuta,
  riavvii, fusi orari e processi concorrenti in modo consistente.
- [ ] Conservare lavori/progetti, fornire messaggi specifici e una procedura di
  assistenza senza esporre segreti o trasformare l'errore in risultati numerici.

Gate: offline valido funzionante; offline scaduto non rinnova autonomamente;
revoca impedisce nuovi rinnovi anche in concorrenza. La latenza offline della
revoca e il residuo rischio di patch/rollback sono esplicitati, non nascosti.

### S09 - Firma, pacchetti e aggiornamento

Priorita': P1. Responsabili: desktop/release/operations. Dipendenze: S03-S06.

- [ ] Definire manifest firmato di release/catalogo con componenti, hash, versioni
  compatibili e politica di rollback; verificare firma e publisher dell'installer.
- [ ] Mantenere Authenticode, timestamp e rotazione certificati con regole
  controllate. Le strong name .NET non sostituiscono l'autenticita' di sicurezza.
- [ ] Rendere fallita una build pubblicabile priva di firma valida; verificare
  l'artefatto finito e lo staging ammesso, non solo l'esito del comando di firma.
- [ ] Escludere runner, PDB destinati solo al debug, test fixture, segreti e
  strumenti interni; mantenere i simboli in archivio interno protetto.
- [ ] Aggiornare in modo atomico componenti compatibili e conservare un rollback
  autorizzato che non riapra silenziosamente la distribuzione dei dati in chiaro.
- [ ] Verificare collegamenti/taskbar e installazioni multiple: aggiornare un
  percorso non dimostra che il cliente stia avviando quel percorso.
- [ ] Applicare requisiti minimi del protocollo server dopo la transizione;
  non fidarsi del solo numero versione auto-dichiarato per dimostrare integrita'.
- [ ] Distinguere migrazione automatica dalla possibilita' di installare senza
  privilegi/restart: non aggirare UAC, policy aziendali o permessi del cliente.

Gate: pacchetto incompleto/non firmato rifiutato; aggiornamento e rollback provati;
eseguibile realmente avviato verificato; nessun artefatto di test nel prodotto.

### S10 - Portale, supporto e osservabilita'

Priorita': P1. Responsabili: portale/backend/prodotto. Dipendenze: S04-S08.

- [ ] Mostrare per dispositivo stato migrazione, tipo di chiave verificato o
  non verificato, protocollo, ultimo rinnovo e scadenza, senza pubblicare segreti.
- [ ] Separare "versione comunicata dal client" da verifica pacchetto/firma;
  non mostrare "DLL integra" sulla base di un hash riferito dal client.
- [ ] Aggiungere recupero/rotazione chiave, sostituzione PC e revoca con
  autorizzazioni, conferma e audit; preservare sempre il massimo di due slot.
- [ ] Definire eventi di sicurezza essenziali, correlazione per dispositivo e
  tempi di conservazione; niente PIN, bearer, chiavi o cataloghi nei log.
- [ ] Preparare istruzioni per supporto, messaggi multilingua e gestione dei
  falsi positivi. Il percorso normale di migrazione non aggiunge popup o email.

Gate: un operatore distingue rete assente, scadenza, revoca e conflitto di chiave;
puo' recuperare il cliente senza concessioni infinite o un terzo dispositivo.

### S11 - Irrobustimento aggiuntivo, senza false garanzie

Priorita': P2. Responsabile: desktop/sicurezza. Dipendenze: S02-S10 stabilizzati.

- [ ] Valutare offuscamento e anti-tamper compatibili con reflection,
  serializzazione, RDLC e chiamate native; misurare regressioni e manutenibilita'.
- [ ] Ridurre API esportate e dati consegnati a UI/progetti/report al necessario;
  verificare che snapshot e diagnostica non esportino involontariamente il master.
- [ ] Valutare identificatori di provenienza nei metadati, mai alterazioni di
  curve, rendimenti o valori tecnici per marcare le copie.
- [ ] Valutare un servizio online per funzioni realmente separabili solo con
  decisione di prodotto esplicita; quote e controlli sugli abusi restano necessari.
- [ ] Escludere driver invasivi, blocchi indiscriminati di VM/debugger e rilevamenti
  fragili che rischiano di fermare clienti senza dimostrare un abuso.

Gate: beneficio misurato da revisione indipendente e regressioni superate;
documentare difficolta' aggiunta, non "impossibilita' di cracking".

### S12 - Collaudo, pilota e distribuzione generale

Priorita': P0 come autorizzazione al rilascio. Responsabili: QA/release/prodotto.
Dipendenze: S00-S10; S11 non deve ritardare le protezioni fondamentali.

- [ ] Implementare e conservare la matrice sotto in ambienti isolati, con
  account/dati di test e nessuna chiave di produzione nei runner.
- [ ] Richiedere una revisione indipendente del protocollo, della migrazione e
  dei confini client/server; includere test negativi, non solo attivazione riuscita.
- [ ] Eseguire baseline tecniche, UI, report PDF e riapertura progetti salvati;
  archiviare esiti e scostamenti ammessi senza aggiornare baseline per nascondere errori.
- [ ] Preparare runbook di rollout, rollback ed emergenza, supporto e allarmi;
  definire stop del pilota per perdita accesso, errori numerici o slot duplicati.
- [ ] Avviare pilota rappresentativo, poi rollout graduale; chiudere il legacy
  solo dopo conferma server del buon esito sui dispositivi previsti.
- [ ] Aggiornare roadmap, inventario, release note e checklist di pubblicazione.
  Il rilascio richiede autorizzazione esplicita; questo piano non la costituisce.

Gate: criteri di accettazione approvati con evidenze, pilota senza regressioni
bloccanti e recupero provato. Un build riuscito non basta.

## 5. Ordine di esecuzione e stati di migrazione

Ordine raccomandato:

1. S00-S01: confermare minacce, vincoli e politica cliente.
2. S02 e prova S06: chiudere i percorsi applicativi non autorizzati e dimostrare
   la fattibilita' della protezione SDF prima di progettare tutta la distribuzione.
3. S03-S04: infrastruttura delle chiavi, concessioni e identita' dispositivo.
4. S05-S08 con portale S10: migrazione, dati protetti, server e gestione offline.
5. S09 e S12: pacchetto firmato, revisione, pilota e distribuzione controllata.
6. S11: ulteriori difese, in base ai risultati e al rischio residuo.

Stati da rappresentare separatamente da ACTIVE/REVOKED dell'utente:

`legacy autorizzato -> migrazione in corso -> chiave registrata -> catalogo
verificato -> protezione attiva`.

Stati di attenzione: attesa rete, recupero richiesto, concessione scaduta,
revocato. Le transizioni sono idempotenti e registrate; un riavvio non crea una
nuova identita'. Un errore nel download non deve eliminare il progetto del cliente.

Distribuzione: backend compatibile -> installazioni interne -> osservazione
limitata nel tempo -> pilota -> aggiornamento generale -> chiusura dei canali
legacy per i nuovi dati. Separare il rollback dell'applicazione dal rollback
delle policy: non riabilitare credenziali revocate come misura di emergenza.

## 6. Matrice minima di accettazione

Tutti i casi sono da eseguire; la colonna esito indica il risultato richiesto,
non una verifica gia' svolta. Le prove di abuso sono autorizzate solo sul test.

| ID | Prova | Esito richiesto |
| --- | --- | --- |
| T01 | Utente con uno o due PC attivi migra | Stessi record/slot, nessun PIN ordinario, progetti conservati |
| T02 | Retry, crash o rete interrotta durante migrazione | Ripresa coerente, nessuna identita'/concessione duplicata |
| T03 | Credenziali legacy presentate per chiavi in conflitto | Nessun rebind automatico dopo enrollment; rischio del primo bind gestito e documentato |
| T04 | Copia normale cartella/stato su terzo PC | Nessun calcolo protetto o nuovo catalogo autorizzato dalla sola copia |
| T05 | Ingressi DLL senza concessione, modalita' di avvio non ammesse | Rifiuto coerente; nessun runner/bypass di produzione |
| T06 | Firma, destinatario, prodotto, diritti o dispositivo errati | Concessione rifiutata; errori tracciabili senza segreti |
| T07 | Richiesta sensibile ripetuta o alterata | Anti-replay e associazione richiesta/operazione verificati |
| T08 | Terza attivazione e richieste concorrenti | Mai piu' di due dispositivi attivi |
| T09 | Offline con concessione valida e server non raggiungibile | Funzioni autorizzate disponibili, nessuna revoca per timeout |
| T10 | Scadenza offline e sessione lasciata aperta | Nessuna proroga locale; stop dei nuovi calcoli e recupero lavori |
| T11 | Revoca seguita da rinnovo concorrente | Nessun rinnovo consentito dopo revoca confermata; latenza offline dichiarata |
| T12 | Orologio arretrato, riavvio, snapshot VM | Casi rilevabili gestiti; limiti residui riportati, nessuna garanzia assoluta |
| T13 | TPM azzerato, PC sostituito, nuovo account Windows | Recupero secondo policy, audit e nessun terzo slot |
| T14 | PC senza TPM/VM supportata | Policy esplicita e livello di protezione correttamente dichiarato |
| T15 | Aggiornamento catalogo, temporanei, backup e rollback | Dati protetti nel nuovo flusso; nessun master/credenziale nei log |
| T16 | Download anonimo o cross-account, URL scaduto | ACL verificate; nessuna chiave/concessione o nuovo catalogo in chiaro esposto |
| T17 | Hash/versione dichiarati falsamente dal client | Non considerati prova di integrita'; decisioni server fondate su diritti e protocollo |
| T18 | Manifest alterato, installer non firmato/publisher diverso | Installazione pubblica rifiutata, con rotazione certificati lecita funzionante |
| T19 | Collegamento taskbar vecchio e installazioni multiple | Diagnosi del percorso reale; versione pubblicata non confusa con quella in uso |
| T20 | Baseline calcoli, ricerca, curve, rendimenti, layout | Nessuna variazione tecnica introdotta dalle protezioni |
| T21 | Progetti storici, report e PDF multilingua | Compatibilita', impaginazione e recupero preservati |
| T22 | Picchi legittimi, IP condivisi, retry di rete | Limiti non bloccano il lavoro normale; anomalie revisionabili |
| T23 | Rotazione/compromissione chiavi e ripristino server | Procedura provata, senza fallback non firmato o credenziali infinite |
| T24 | Pacchetto finale e diagnostica | Nessun runner, segreto o dato sorgente non previsto distribuito |
| T25 | DLL vecchia e nuovo catalogo, componenti aggiornati solo in parte | Incompatibilita' rilevata e recuperabile; nessun fallback in chiaro o risultato tecnico non valido |

Per regressioni e rilascio usare come base la
[matrice tecnica](TECHNICAL_SELECTION_TEST_MATRIX.md), il
[runbook](TECHNICAL_SELECTION_RELEASE_RUNBOOK.md), le
[baseline](../tests/Invoke-TechnicalBaselines.ps1) e i
[test selezione Next](../tests/next-selection-regression.mjs).
Aggiungere le prove di sicurezza, non sostituire quelle funzionali.

## 7. Decisioni aperte e condizioni di stop

| Decisione | Orientamento proposto | Prima di procedere |
| --- | --- | --- |
| Durata offline | Conservare 30 giorni iniziali | Conferma prodotto e comportamento dopo scadenza |
| TPM/fallback | TPM preferito, alternativa esplicitamente piu' debole | Censimento PC, test provider/attestazione e supporto |
| Protezione SDF | Chiavi non universali e protezione a riposo | Prova S06 riuscita senza plaintext staging e senza regressioni |
| Migrazione da vecchio bearer | Automatica per casi coerenti; gestione conflitti | Accettazione del rischio iniziale o ulteriore verifica dei casi dubbi |
| Chiusura legacy | Solo dopo pilota, per distribuzione futura | Inventario canali, rollback e copertura migrazione |
| Quota/telemetria | Minimo necessario e limiti basati su dati reali | Misura flussi legittimi, retention e responsabilita' operative |
| Calcoli/dati solo online | Fuori dal perimetro offline approvato | Decisione commerciale/tecnica separata |

Non distribuire se: si perdono progetti, cambia un risultato tecnico senza
motivo funzionale, vengono duplicati slot, una rotta legacy rinnova senza le
nuove prove dopo enrollment, il catalogo viene esportato in chiaro dal nuovo
flusso, oppure mancano recupero e rotazione chiavi verificati.

## 8. Stato e checkpoint

- [x] 30/09/2026: ricostruzione delle decisioni e ispezione statica dei punti
  principali desktop/libreria, aggiornamenti, API e portale.
- [x] 30/09/2026: piano operativo, priorita', dipendenze, rischi residui e criteri
  di accettazione documentati.
- [ ] Assegnazione responsabili e conferma decisioni aperte.
- [ ] Implementazione e collaudo S00-S12.
- [ ] Pilota e autorizzazione alla distribuzione generale.

La roadmap [licenze account e dispositivi](DEVICE_LICENSING_ROADMAP.md) resta
il riferimento del flusso esistente; questo documento ne descrive l'evoluzione
di protezione, senza dichiararla gia' rilasciata.

## 9. Riferimenti tecnici

Fonti primarie consultate per le distinzioni progettuali; non costituiscono
certificazione di sicurezza del prodotto.

- [Microsoft: uso del TPM in Windows](https://learn.microsoft.com/en-us/windows/security/hardware-security/tpm/how-windows-uses-the-tpm): protezione delle chiavi e confine hardware.
- [Microsoft: TPM key attestation](https://learn.microsoft.com/en-us/windows-server/identity/ad-ds/manage/component-updates/tpm-key-attestation): distinguere chiave dichiarata e origine hardware attestata; non e' attestazione della DLL SSW.
- [Microsoft: CryptProtectData](https://learn.microsoft.com/en-us/windows/win32/api/dpapi/nf-dpapi-cryptprotectdata): ambito utente/macchina e limiti della protezione DPAPI.
- [Microsoft: strong-named assemblies](https://learn.microsoft.com/en-us/dotnet/standard/assembly/strong-named): strong name non equivalente a confine di sicurezza.
- [OWASP MASVS: resilience](https://mas.owasp.org/MASVS/11-MASVS-RESILIENCE/): principi di difesa contro manomissione; fonte mobile usata come riferimento, non come certificazione desktop.
- [OWASP API6: sensitive business flows](https://owasp.org/API-Security/editions/2023/en/0xa6-unrestricted-access-to-sensitive-business-flows/): abuso automatizzato anche con accesso legittimo.
