# Layout: simboli aeraulici originali e legenda

Data: 09/09/2026. Release SSW 1.3.0.61.

## Richiesta

Sostituire frecce e nomi collocati attorno all'unita con i quattro simboli
aeraulici originali del produttore. Mantenere i nomi localizzati in una legenda
fissa sul lato destro, ridurre lo spazio del disegno e rendere meno allungata la
sagoma dell'unita senza sovrapporre simboli, boccagli o pannello di accesso.

## Decisione

I quattro PNG originali sono conservati senza reinterpretazione come risorse
versionate. Il frontend li distribuisce da `public/airflow`; il renderer del
report usa copie incorporate in `SSWLib.dll`, così il report non dipende da file
esterni presenti nella cartella di esecuzione.

Sul disegno ogni connessione mostra soltanto il simbolo e il numero fisico del
boccaglio. La legenda mostra sempre, nello stesso ordine, aria esterna, mandata,
ripresa ed espulsione usando le traduzioni già disponibili per tutte le lingue.
Le identità dei flussi e le posizioni continuano a provenire dal catalogo
normalizzato: questa modifica non introduce regole specifiche per configurazione.

La zona grafica e la legenda hanno colonne separate. Le sagome molto larghe sono
state rese più compatte sia nella UI sia nel report, mantenendo gli spazi minimi
necessari per quattro connessioni SSC e per le viste OSC nord-sud ed est-ovest.
Le icone e i rispettivi boccagli condividono lo stesso centro fisico. Le quattro
case hanno la stessa dimensione visiva; nei simboli fresh ed exhaust la freccia
esterna può estendersi oltre tale ingombro. Lo smoke test WebView2 verifica
l'allineamento dopo ogni cambio di configurazione supportato.

I simboli sotto la macchina condividono la stessa linea di base. Nei layout OSC
con connessioni circolari i boccagli inferiori sono centrati sull'asse orizzontale
della sagoma. Le connessioni rettangolari contrapposte restano invece esterne su
entrambi i lati. Nella vista laterale la sezione `46 x 28` viene ruotata in
`28 x 46` e resta adiacente all'unita. La legenda allinea il bordo sinistro delle
case, indipendentemente dalla freccia esterna.

La fila inferiore dei layout con boccagli circolari interni e delle configurazioni
SSC inferiori e' leggermente rialzata. Le configurazioni con boccagli rettangolari
esterni conservano invece il maggiore spazio necessario a evitare interferenze.

Le etichette dei pannelli di accesso superiore e inferiore aderiscono al bordo ma
rimangono interamente dentro la sagoma. Nei layout SSC piani i quattro boccagli
circolari condividono la mezzeria orizzontale della macchina.

Nei soli layout SSC piani il nome dell'unita viene allontanato dall'etichetta del
pannello: verso l'alto per l'accesso inferiore e verso il basso per l'accesso
superiore. Entrambe le scritte restano interne senza sovrapporsi.

La codifica cromatica comune a UI e report segue la convenzione richiesta:
SUP/supply blu (`#168bd2`), ODA/fresh verde (`#279b55`), ETA/return giallo
(`#e2c600`) ed EHA/exhaust rosso-bruno (`#a6533c`). La legenda mostra un tratto
colorato sotto la dicitura localizzata e i boccagli usano lo stesso colore.
Il contorno colorato dei boccagli usa un tratto rinforzato da 3 px nella UI e
da 5 px nel renderer ad alta risoluzione del report.
I numeri dei boccagli sono centrati in Arial grassetto, 13 px nella UI e 11 pt
nel report, per restare leggibili anche in stampa.
La sagoma dell'unita usa un riempimento bianco pieno e un contorno principale
nero sia nella UI sia nel report; la linea interna resta secondaria e piu chiara.
Tutti i testi interni sono neri e in Arial. L'etichetta del pannello ha fondo
bianco e bordo nero; la sagoma non usa ombre inferiori.
Nei report `OSC_EAST_WEST` l'unita mantiene la proporzione compatta della vista
laterale UI; i boccagli restano aderenti ai bordi e le casette all'esterno.
Nella vista parete `OSC_EAST_WEST` ogni casetta e allineata alla riga del proprio
boccaglio; l'interasse verticale delle due righe e circa due larghezze icona.

## Verifiche

- `npm run build` nel frontend.
- `node tests/airflow-layout-regression.mjs` per posizioni, risorse e legenda.
- `tests/Invoke-InstallationLayoutReference.ps1` per le sette geometrie report.
- `tests/Invoke-NextUiSmoke.ps1` e controllo visivo delle schermate layout.
- Build AV x86 dell'intera soluzione.
