# Audit e correzioni della selezione Next

Data: 2026-09-08

Stato: correzioni applicate nel worktree di sviluppo; verifiche indicate sotto.
La sincronizzazione dello storico non costituisce pubblicazione della release.
Il repository conteneva gia' numerose modifiche precedenti: il commit della
documentazione non include automaticamente tale lavoro o i sorgenti in corso.

## Richiesta

Correggere le criticita' rilevate nell'audit, preservare le formule e la resa
della UI, documentare la proposta drawing e conservare uno storico in Git.

## Problemi e soluzioni

| Problema verificato | Correzione applicata |
| --- | --- |
| Dopo cambio modello la UI disattivava una batteria ma conservava il risultato calcolato con essa | Normalizzazione su copia, ricalcolo fino a stato stabile e pubblicazione congiunta di input effettivo e risultato; gli ID non validi non scelgono piu' una batteria diversa nel backend |
| Accessori non disponibili o incompatibili potevano restare inclusi e finire nel file | Normalizzazione backend di disponibilita', gruppi esclusivi, standard, livello KTS e dipendenze; selezioni incoerenti rifiutate al salvataggio; controllo dei cicli |
| Catalogo normalizzato vuoto o danneggiato ripescava configurazioni legacy | Fallback solo per schema realmente precedente; tabelle parziali/mancanti nello schema 4, metadati invalidi, duplicati e ruoli aria incoerenti producono errore; un modello senza relazioni rimane senza configurazioni |
| Eccezioni lasciavano l'attesa attiva; risposte vecchie potevano sostituire quelle nuove | Versionamento richieste, confronto con input corrente, gestione errori e finally per ricerca/calcolo; navigazione e operazioni di salvataggio/report protette durante attesa o errore |
| Il programma forzava BASIC ignorando il catalogo | Default BASIC spostato nei dati centrali, rimosso override runtime; mantenute possibili eccezioni di serie/modello |
| UI e report deducevano geometria e accesso da liste di codici duplicate | Propagati Orientation, ReferenceView e AccessSide; UI e report usano il catalogo; le sole regole storiche restano nell'adattatore legacy; esposte anche quote Hor/Ver nel bridge |

I risultati del calcolo non devono essere considerati aggiornati finche' la
normalizzazione non converge. Il limite di iterazioni produce un errore
esplicito, non un risultato parzialmente aggiornato.

Le configurazioni sconosciute non vengono trasformate in B6/A4/T5. I vecchi
SDF continuano ad avere l'adattatore storico; nei database nuovi prevalgono
esclusivamente le relazioni attive esportate da Explorer.

## Dati e migrazione KTS

Nel catalogo centrale, prima della correzione: KTS EXTRA default, BASIC non
default; nessuna relazione KTS di override per serie/modello presente.

Applicato lo script transazionale e ripetibile nel repository CLDCExplorer:
`documents/sql/selection_catalog/008_default_kts_basic.sql`.
Modifica solo i due default globali BASIC/EXTRA, non le eccezioni.

Export AV verificato tramite il tool del repository CLDataCentralLib:
schema 4, exporter 1.3.0, 62 articoli, 6149 relazioni accessori,
29 configurazioni, 274 relazioni modello/configurazione.
Aggiornato il SDF della build operativa dopo backup della copia precedente.
Nessuna credenziale e' inclusa in questo documento o negli script di migrazione.

## Dato ancora da completare in Explorer

`CLRC 038 OSC` (Id 97 nell'SDF verificato) non ha relazioni normalizzate di
installazione. Il vecchio fallback lo nascondeva. Non sono state inventate o
riattivate associazioni: vanno assegnate tramite Explorer secondo il catalogo
tecnico approvato e poi riesportate.

Le nuove selezioni senza configurazioni non possono essere salvate come valide.
I report storici restano leggibili: mostrano indisponibilita' della configurazione
senza generare un disegno fittizio. Questa distinzione e' emersa durante il test
di regressione dei file legacy ed evita di bloccare i risultati tecnici storici.

## File principali

- `frontend/ssw-next/src/bridge/normalizeSelection.ts`: normalizzazione pura.
- `frontend/ssw-next/src/main.ts`: orchestrazione asincrona e geometria UI.
- `frontend/ssw-next/src/bridge/contracts.ts` e `bridge/index.ts`: metadati e quote.
- `SSWLib/CLNextUiApplicationService.vb`: selezioni accessori/trattamenti e guardie.
- `SSWLib/CLSelectionCatalog.vb`: default autorevole dal database.
- `SSWLib/CLInstallationLayoutRepository.vb`: catalogo e compatibilita' legacy.
- `SSWLib/CLInstallationLayoutReportRenderer.vb`: geometria dai metadati.

## Verifiche riproducibili

Eseguire dalla radice del repository, con dipendenze locali installate:

```powershell
node tests/next-selection-regression.mjs
& .\tests\Invoke-AccessoryCatalogSmoke.ps1
& .\tests\Invoke-NextUiSmoke.ps1
& .\tests\Invoke-TechnicalBaselines.ps1
& .\tests\Invoke-TechnicalBaselines.ps1 -NumericOnly
```

Build: `SSW.sln`, configurazione `AV`, piattaforma `x86`.
Eseguibile da verificare: `SSW/bin/x86/AV/SSW.exe`, non una copia installata.
Il test catalogo altera soltanto una copia temporanea dell'SDF.

Esiti gia' verificati:

- build AV/x86 e TypeScript/Vite superate;
- normalizzazione stabile, ricalcolo dopo disattivazione, errori e risposte
  fuori ordine superati nel test frontend;
- default BASIC, override modello EXTRA, standard esclusivo, accessorio non
  disponibile, dipendenza mancante, livello controller e ciclo contraddittorio
  verificati nel test catalogo;
- quattro ruoli univoci, schema 4 parziale/mancante e fallback schema 3 verificati;
- smoke applicativo Next con salvataggio, riapertura e report superato;
- screenshot automatici layout, CO2 e rumore superati; layout anche ispezionato
  visivamente, con quattro flussi, proporzioni integre e KTS BASIC;
- esito della matrice numerica registrato nel checkpoint finale seguente.

Il confronto completo con le vecchie fixture segnala differenze nelle caption
del rumore ora localizzate, nel dataset di configurazione esteso e nei metadati
del database passato da schema 3 a 4. Non sono stati sovrascritti gli expected
per nascondere queste differenze. `-NumericOnly` confronta tutti i valori
numerici di riferimento, comprese serie e risultati dei dataset, con le stesse
tolleranze; esclude testi/metadati non numerici e nuovi campi aggiuntivi.
Non equivale alla convalida completa di tutti gli output del report.

Corretto anche il lettore JSON del test per PowerShell 7.5: le date ISO devono
restare stringhe, altrimenti il confronto ricorsivo di `DateTime.Date` non
termina. Il test resta compatibile con Windows PowerShell.

## Limiti e seguito

Checkpoint finale 08/09/2026: tutte e quattro le baseline numeriche superate
(`electric-heaters`, `enthalpic-enrfc27`, `standard-accessories`,
`water-coil-hcd`), senza aggiornare i riferimenti attesi. Restano le differenze
non numeriche descritte sopra e l'assegnazione dei modelli privi di relazioni.

Il drawing dimensionale resta una proposta distinta, descritta in
[DRAWING_DIMENSIONALE_PROPOSTA.md](DRAWING_DIMENSIONALE_PROPOSTA.md).
Il trasporto delle quote nel bridge non significa che gli asset CAD siano
disponibili, che lo schema di gestione immagini sia implementato o che la
sezione drawing sia stata completata.

Non sono state cambiate formule, versione di rilascio, installer o canale
aggiornamenti. Le richieste native restano sull'architettura desktop esistente;
non e' stata introdotta una parallelizzazione dei motori nativi non verificata.
