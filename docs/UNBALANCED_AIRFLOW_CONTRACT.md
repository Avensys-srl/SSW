# Portate sbilanciate - Contratto tecnico

Data prima stesura: 27/07/2026

## Obiettivo

Gestire portate di mandata e ripresa differenti senza ridurre il caso a due
calcoli bilanciati indipendenti. Tutti gli output devono rappresentare i due
rami e i totali coerenti.

Questo documento governa motore, UI, grafici, salvataggio, report, API e
portale.

## Principio fondamentale

Lo scambiatore e' un unico sistema con due flussi accoppiati.

Le due portate producono:

- due punti aeraulici;
- due potenze ventilatori;
- due pressioni disponibili;
- due rendimenti/effectiveness di ramo;
- due condizioni d'uscita;
- un bilancio termico complessivo coerente.

Non e' ammesso calcolare due scenari bilanciati separati e combinarli dopo.

## Input minimi

`AirflowPair`:

- `SupplyAirflow`;
- `ExtractAirflow`.

Condizioni per stagione:

- temperatura e umidita' aria esterna;
- temperatura e umidita' aria ripresa;
- livello di regolazione o comando equivalente per ciascun ramo;
- configurazione ventilatori e curve per ciascun ramo;
- perdite accessorie applicabili a ciascun ramo;
- pressione impianto richiesta per ciascun ramo.

Modalita' bilanciata:

```text
SupplyAirflow = ExtractAirflow
```

I file storici con una sola portata vengono migrati secondo questa regola.

## Output di ramo

Ogni `BranchOperatingPoint` espone almeno:

- branch: `Supply` o `Extract`;
- airflow;
- available static pressure;
- requested/static system pressure;
- fan absorbed power;
- branch SFP;
- regulation level;
- pressure-drop contributions;
- validity/status.

I valori devono rimanere numerici. Unita', arrotondamento e testo localizzato
appartengono alla presentazione.

## Output termodinamici

Per ogni stagione:

- supply outlet temperature and relative humidity;
- exhaust outlet temperature and relative humidity;
- recovered total, sensible and latent heat;
- condensed water;
- supply-side effectiveness;
- extract-side effectiveness;
- eventuali indicatori globali gia' previsti dal prodotto;
- warning di sbilanciamento o dominio non valido.

Le formule definitive di effectiveness devono essere ricavate dalla
documentazione tecnica e dalla libreria autorevole. Un agente non deve
inventare la conversione di una curva monodimensionale in comportamento
sbilanciato.

## Grafici

### Portata/pressione

- curva e punto di lavoro del ramo mandata;
- curva e punto di lavoro del ramo ripresa;
- scala e legenda non ambigue;
- perdite accessorie sottratte solamente al ramo su cui sono installate.

### Potenza assorbita

- punto o curva del ventilatore di mandata;
- punto o curva del ventilatore di ripresa;
- potenza totale come somma esplicita.

```text
FanPowerTotal = FanPowerSupply + FanPowerExtract
```

### Efficienza

Per ogni stagione attiva devono essere disponibili:

- curva o rappresentazione di riferimento;
- working point mandata;
- working point ripresa.

Quindi inverno ed estate possono mostrare due punti ciascuno. I punti sono
risultati dello stesso scenario accoppiato, non di due scenari indipendenti.

### SFP

Secondo la convenzione SSW approvata:

```text
SFP_Total = SFP_Supply + SFP_Extract
```

Devono essere esposti anche i due addendi. Unita' e conversioni SI/IP devono
essere verificate con test dimensionali.

Gli altri indicatori derivati, inclusi SEL e criteri Passivhaus/ERP, devono
avere una regola esplicita prima dell'implementazione. Non estendere
automaticamente la formula bilanciata.

## Batterie e accessori

- PEHD, EHD e water coil attuali operano sul ramo mandata.
- La loro perdita di carico modifica la curva del ramo mandata.
- Accessori futuri possono dichiarare il ramo di appartenenza.
- Un accessorio applicato a entrambi i rami deve produrre due contributi
  distinti.
- Le temperature di ingresso delle batterie dipendono dall'ordine fisico dei
  componenti nel ramo, non da un valore UI scelto liberamente.

## Persistenza

La nuova versione del progetto deve salvare:

- input dei due rami;
- risultati di ramo necessari alla riproducibilita';
- modalita' bilanciata/sbilanciata;
- versione del contratto di calcolo;
- configurazione e perdite applicate a ciascun ramo.

Migrazione da file storico:

1. copiare la portata legacy in entrambi i rami;
2. impostare modalita' bilanciata;
3. non modificare silenziosamente altri dati;
4. ricalcolare con il motore corrente e registrare la versione tecnica.

## Report e portale

Devono mostrare:

- portata, pressione, potenza e SFP di mandata;
- portata, pressione, potenza e SFP di ripresa;
- potenza ventilatori totale;
- SFP totale;
- risultati termodinamici dei due lati;
- grafici con punti di ramo;
- indicazione chiara della modalita' sbilanciata.

Il portale conserva i valori strutturati, non solo una stringa riassuntiva.

## Criteri di accettazione

1. Con portate uguali i risultati coincidono con il caso legacy entro le
   tolleranze approvate.
2. Lo scambio energetico rispetta il bilancio tra i due flussi.
3. Le curve non condividono per errore un unico punto di lavoro.
4. Le perdite di un ramo non spostano la curva dell'altro.
5. La potenza totale coincide con la somma dei ventilatori.
6. SFP totale coincide con la somma dei due SFP di ramo.
7. Ogni stagione mostra entrambi i punti di effectiveness.
8. Salvataggio, riapertura, report e payload ricostruiscono la stessa
   selezione.
9. I file storici continuano ad aprirsi in modalita' bilanciata.
10. Il calcolo funziona senza connessione di rete.

## Decisioni da chiudere prima delle formule

- definizione autorevole delle due effectiveness sotto sbilanciamento;
- sorgente delle curve ventilatore separate per ramo;
- regola SEL sotto sbilanciamento;
- regole Passivhaus, ERP e altri limiti derivati;
- limiti ammessi del rapporto tra le due portate;
- comportamento in caso di punto fuori dominio;
- rappresentazione report quando una stagione non e' attiva.
