# Layout: correzioni dei flussi e rappresentazione tecnica

Data: 09/09/2026. Release SSW 1.3.0.60; Explorer 2.1.21.0.

## Fonte e criterio

Riferimento: [documentazione originale](references/layout_configuration.pdf),
foglio produttore 01.23. Matrice completa verificabile in
`tests/fixtures/installation-layout-reference.json`: 16 OSC e 6 SSC.
Fresh = rinnovo/aria esterna, Supply = mandata, Return = ripresa,
Exhaust = espulsione. I simboli del documento indicano il rapporto con
l'ambiente interno/esterno; le frecce SSW indicano entrata/uscita dall'unita'.

Le posizioni sono identita' fisiche, non coordinate sullo schermo:
- OSC nord-sud: 1 e 2 sotto; 3 e 4 sopra, da sinistra a destra.
- OSC orizzontale: 1 e 2 frontali; 3 e 4 posteriori, parzialmente nascosti.
- OSC est-ovest: 1 alto sinistra, 2 basso sinistra, 3 alto destra, 4 basso destra.
- SSC: 1, 2, 3, 4 da sinistra a destra sulla stessa faccia.

## Problemi e soluzione

1. Il seed legacy raggruppava erroneamente alcune sequenze OSC: corretti
   A3, A4, B1, B2, B3, B5, C2, D3. SSC era gia' coerente.
2. La UI disegnava 1/2 sopra e 3/4 sotto per tutti gli OSC non est-ovest.
   `airflow-layout.ts` separa ora posizione fisica e slot grafico.
3. Il report est-ovest usava l'ordine sinistra/destra alternato. Ora usa
   sinistra/sinistra/destra/destra, come UI e riferimento.
4. Durante il ricalcolo il draft nuovo poteva essere abbinato ai vecchi flussi.
   Il disegno non viene mostrato finche' il risultato corrente non e' pronto;
   il controllo di versione delle richieste gia' esistente rimane attivo.
5. Rimosso il rombo da UI e report: nessun simbolo di scambiatore inventato.
   Aggiunti contorno pannello, boccagli numerati e bordi nel colore del flusso.
   Le aperture posteriori sono semicircolari nel report come nella UI.
6. Corrette le etichette laterali del report che potevano essere tagliate o
   sovrapposte alle frecce; larghezza misurata e spazi distinti per testo/freccia.
   Il nome modello resta solo nella UI, non dentro il disegno del report.

## Catalogo e compatibilita'

In Explorer: `documents/sql/correct_flow_configuration_ports_20260909.sql`.
Applicata al database centrale: 10 definizioni corrette (8 OSC e 2 A4 EN/LT
gia' importate con geometria OSC). Seconda esecuzione: zero modifiche.
Le righe precedenti sono conservate in `CLFlowPortsBackup20260909`.
La migrazione modifica solo definizioni importate, mai aggiornate manualmente,
con la sequenza precedente esatta. Non modifica relazioni, default o varianti
legacy HorVariants/VerVariants: il csv-exporter continua a usare gli stessi
campi. Il seed per nuove installazioni e il fallback SSW sono allineati.

CLDataCentralLib non richiede modifiche: esporta gia' il catalogo normalizzato.
Eseguita esportazione reale schema 5, exporter 1.4.0: 29 layout, 274 relazioni.
SDF AV distribuito nella cartella di esecuzione SSW dopo backup del precedente.
Il database commerciale non viene aggiunto a Git; migrazione, test e fonte si'.

I progetti salvati conservano il codice configurazione: riaprendoli usano la
corrispondenza corretta del catalogo. I PDF gia' emessi non vengono modificati;
per selezioni/ordini basati sui vecchi disegni e' necessario ricontrollare il layout.

## Verifiche riproducibili

- `node tests/airflow-layout-regression.mjs`: posizioni UI e guardia risultato.
- `node tests/next-selection-regression.mjs`: normalizzazione e risposte obsolete.
- `tests/Invoke-InstallationLayoutReference.ps1`: 22 sequenze legacy, catalogo
  esportato e sette geometrie report; produce PNG temporanei per controllo visivo.
- `tests/Invoke-NextUiSmoke.ps1`: inclusi cambi ripetuti soffitto/parete e di codice.
- Build AV x86 e build Explorer Release x86.

Esiti: test posizioni, test di normalizzazione, catalogo accessori, otto scenari
WebView2 e quattro baseline tecniche passati. Controllate visivamente tutte le
sette viste del report e la UI a larghezza desktop e ridotta. Nei riferimenti
tecnici cambiano solo data/firma del nuovo SDF e la dimensione binaria della
nuova immagine installativa: nessun valore di calcolo modificato.
La build Explorer mantiene gli avvisi preesistenti sui riferimenti opzionali
DevExpress, senza errori di compilazione.

Le nuove configurazioni vanno sempre definite in Explorer: non aggiungere
regole specifiche per codice nella UI. Aggiornare il fixture solo dopo confronto
con documentazione del produttore, non per adattarlo al risultato del codice.
