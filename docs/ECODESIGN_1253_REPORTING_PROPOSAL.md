# Reporting Ecodesign 1253/2014

Data: 2026-09-08

Stato: **proposta da implementare**. Questo documento non attesta che SSW sia
gia' in grado di emettere una dichiarazione normativa completa.

## Obiettivo e perimetro

Usare la scheda delle informazioni Ecodesign prevista dal Regolamento (UE)
1253/2014 come primo documento del nuovo sistema di stampa HTML/CSS/SVG. Non
esiste un RDLC da mantenere e lo stesso template potra' servire per anteprima
web e PDF.

Occorre distinguere due documenti:

1. La scheda delle informazioni Ecodesign, secondo l'Allegato IV per le RVU e
   l'Allegato V per le NRVU.
2. La dichiarazione CE di conformita' prevista dalla Direttiva 2009/125/CE,
   che comprende anche fabbricante, norme applicate, altra legislazione CE e
   firma del soggetto autorizzato.

La prima implementazione proposta riguarda la scheda Ecodesign. La
dichiarazione CE firmata resta un documento distinto e successivo.

## Dati disponibili e lacune

SSW dispone gia' di identificazione del modello, serie, layout, portata,
pressione, curve, potenza al punto di lavoro, rendimento calcolato, SFP
complessivo, dimensioni, dati elettrici, dati acustici, installazione e
accessori. Questi dati non sono automaticamente equivalenti ai valori
dichiarabili nelle condizioni normative.

Le principali informazioni ancora da qualificare o introdurre comprendono:

- classificazione RVU/NRVU, UVU/BVU e canalizzata/non canalizzata;
- tipo di azionamento e di sistema di recupero;
- condizioni nominali ufficiali e configurazione commerciale dichiarata;
- SFPint, velocita' frontale e perdite di pressione interne;
- efficienza statica dei ventilatori;
- perdite esterne, interne o carry-over espresse come valori misurati;
- prestazione energetica dei filtri e relativo avviso;
- rumore irradiato dell'involucro qualificato per uso normativo;
- riferimenti a prove, metodi di calcolo, modelli equivalenti e istruzioni di
  disassemblaggio;
- per le RVU, SEC nelle zone climatiche, classe SEC, SPI, tipo/fattore di
  controllo e gli ulteriori dati applicabili alle unita' non canalizzate.

Un riferimento a una norma di misura non sostituisce il relativo valore
misurato. Analogamente, lo SFP complessivo o il rumore ricostruito da SSW non
devono essere pubblicati come SFPint o rumore certificato senza una
validazione tecnica esplicita.

## Provenienza dei valori

Il Regolamento 1253/2014 ammette informazioni ottenute tramite calcolo o
estrapolazione, ma richiede di documentare calcoli, estrapolazioni, verifiche
sperimentali, modelli equivalenti e modelli ai quali e' stato applicato lo
stesso metodo.

`Calculated` non significa quindi stimato o ipotizzato. Le origini ammesse nel
modello dati proposto sono:

| Origine | Significato |
| --- | --- |
| `Measured` | Valore ottenuto da una prova documentata sul modello. |
| `CalculatedFromQualifiedInputs` | Valore ottenuto con formula identificata e riproducibile, usando input misurati, certificati o altrimenti qualificati. |
| `ExtrapolatedValidated` | Valore derivato da un modello equivalente mediante metodo documentato e verificato sperimentalmente. |
| `SupplierDeclared` | Valore dichiarato dal fornitore del componente, quando utilizzabile nel procedimento applicabile. |
| `Unknown` | Informazione assente o non qualificata. |

Origine e stato di approvazione sono dimensioni separate:

```text
Origin: Measured / CalculatedFromQualifiedInputs /
        ExtrapolatedValidated / SupplierDeclared / Unknown
Status: Draft / Verified / Approved / Obsolete
```

Ogni valore deve conservare almeno unita', condizioni di riferimento, origine,
formula e relativa versione quando calcolato, rapporto o documento sorgente,
modello provato o equivalente, data di validazione e approvatore.

Le stime preliminari possono essere usate soltanto per simulazioni interne e
devono restare `Draft`. Non devono essere inserite automaticamente in una
scheda o dichiarazione ufficiale.

## Regola di emissione

Il renderer non inventa valori mancanti e non sostituisce dati normativi con
grandezze SSW solo apparentemente equivalenti. Un PDF ufficiale e' generabile
soltanto quando:

- il prodotto e' classificato nel perimetro normativo corretto;
- tutti i campi obbligatori applicabili sono valorizzati;
- ogni valore obbligatorio e' in stato `Approved`;
- metodo, condizioni, evidenze e versioni sono registrati;
- la configurazione commerciale coincide con quella coperta dai dati;
- algoritmo normativo e template sono versionati e approvati.

Un documento incompleto puo' essere prodotto solo come bozza chiaramente
marcata, senza dichiarazione di conformita'.

## Architettura proposta

Separare il calcolo dalla stampa:

```text
Dati regolamentari approvati + risultati versionati
                         |
                         v
               EcodesignDocument DTO
                         |
                         +-- anteprima web
                         +-- template HTML/CSS/SVG
                         +-- PDF ufficiale
```

Nel database prevedere un'intestazione della dichiarazione e strutture
tipizzate distinte per RVU e NRVU, oltre a norme, legislazione, traduzioni ed
evidenze. Evitare una tabella generica di coppie chiave/valore per i campi
obbligatori: tipi, unita' e vincoli devono essere verificabili.

## Primo passo

Costruire una matrice per una serie rappresentativa:

```text
campo normativo -> sorgente SSW -> misurato/calcolabile -> evidenza -> stato
```

Da questa matrice derivare schema dati, validatore, calcoli ammessi e primo
template. L'estensione a tutte le unita' avviene soltanto dopo aver verificato
completezza e qualifica dei dati, senza riempimenti automatici presunti.

## Riferimenti

- Regolamento (UE) 1253/2014, in particolare articoli 4 e 5 e Allegati IV, V,
  VIII e IX: <https://eur-lex.europa.eu/eli/reg/2014/1253/2020-07-30/eng>
- Direttiva 2009/125/CE, articolo 5 e Allegato VI:
  <https://eur-lex.europa.eu/legal-content/IT/TXT/?uri=CELEX:32009L0125>
- `TECHNICAL_SELECTION_VERSIONING.md`: versionamento indipendente di dati,
  calcoli, template, API e formato selezione.
