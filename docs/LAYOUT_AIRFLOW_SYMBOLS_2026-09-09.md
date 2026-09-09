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
opposti i boccagli inferiori restano interamente dentro la sagoma; nella vista
laterale la loro sezione viene ruotata scambiando larghezza e altezza. La legenda
allinea il bordo sinistro delle case, indipendentemente dalla freccia esterna.

## Verifiche

- `npm run build` nel frontend.
- `node tests/airflow-layout-regression.mjs` per posizioni, risorse e legenda.
- `tests/Invoke-InstallationLayoutReference.ps1` per le sette geometrie report.
- `tests/Invoke-NextUiSmoke.ps1` e controllo visivo delle schermate layout.
- Build AV x86 dell'intera soluzione.
