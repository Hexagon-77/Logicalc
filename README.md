# Documentație Logicalc - Conversie de baze numerice

**Autor:** Pîrvulescu Șerban

---

## Enunțul problemei
Se cere realizarea unei aplicații care să efectueze conversia unui număr dintr-o bază numerică în alta. Utilizatorul introduce numărul, baza inițială și baza finală, iar programul returnează rezultatul conversiei folosind una dintre următoarele metode:
- Conversie prin împărțiri succesive
- Conversie prin bază intermediară (baza 10)
- Conversie rapidă pentru baze puteri ale lui 2

De asemenea, utilizatorul poate opta pentru afișarea detaliată a pașilor de conversie prin intermediul pictogramei cu ochi.

---

## Diagrama de apel a subalgoritmilor

```cs
Calc_Click()
|
|-> SuccessiveDivision()     // daca metoda aleasa este "împărțiri"
|-> IntermediaryConversion() // daca metoda aleasa este "intermediar"
|     |
|     |-> ConvertToDecimal()
|     |-> SuccessiveDivision()
|
|-> RapidConversion()        // daca metoda aleasa este "rapid"
```

---

## Specificarea tipurilor de date folosite

- `string` - pentru stocarea numerelor convertite
- `int` - pentru valorile numerice utilizate în calcule
- `bool` - pentru verificarea vizibilității soluției
- `enum` - `BaseConversionType` pentru alegerea metodei de conversie
- `List<(string, int)>` - pentru stocarea soluțiilor anterioare

---

## Subalgoritmii principali

#### a) Date, rezultate, precondiții, postcondiții (1p)

**SuccessiveDivision(value, newBase, out steps)**
- *Date de intrare:* `value` - număr întreg pozitiv, `newBase` - baza țintă
- *Rezultate:* `string` - numărul convertit în noua bază
- *Condiții:* `value > 0`, `newBase > 1`

**IntermediaryConversion(value, oldBase, newBase, out steps)**
- *Date de intrare:* `value` - număr în baza `oldBase`, `newBase` - baza țintă
- *Rezultate:* `string` - numărul convertit în `newBase`
- *Condiții:* `value` trebuie să fie un număr valid în `oldBase`

**RapidConversion(value, oldBase, newBase, out steps)**
- *Date de intrare:* `value` - număr în `oldBase`, `oldBase` și `newBase` puteri ale lui 2
- *Rezultate:* `string` - conversia în `newBase`
- *Condiții:* `oldBase` și `newBase` sunt puteri ale lui 2, `oldBase < newBase`

---

## Pseudocod

**SuccessiveDivision(value, newBase, out steps)**
```
steps <- ""
result <- ""
Cât timp value > 0:
    remainder = value % newBase
    Adaugă remainder la începutul lui result
    Salvează pașii calculului în steps
    value <- value / newBase
Returnează -> result și steps
```

**IntermediaryConversion(value, oldBase, newBase, out steps)**
```
Convertește value în baza 10 folosind ConvertToDecimal
Salvează pașii în steps
Apelează SuccessiveDivision pentru conversia din baza 10 în newBase
Returnează -> rezultatul și pașii
```

**RapidConversion(val, oldBase, newBase, out steps)**
```
Calculează stepValue <- conversia lui value din oldBase în baza 10
Determină groupSize <- log2(newBase) - log2(oldBase) + 1
Cât timp stepValue > 0:
    Împarte în grupuri conform groupSize
    Stochează rezultatul în steps
Returnează -> numărul convertit
```

---

## Date de test


#### Conversie prin împărțiri succesive
| Input       | Rezultat       |
|------------|-------------|
| 25 (baza 10) → baza 2 | 11001 |
| 100 (baza 10) → baza 8 | 144 |

#### Conversie prin bază intermediară
| Input       | Rezultat       |
|------------|-------------|
| 1010 (baza 2) → baza 16 | A |
| 77 (baza 8) → baza 2 | 111111 |

#### Conversie rapidă
| Input       | Rezultat       |
|------------|-------------|
| 1101 (baza 2) → baza 8 | 15 |
| F (baza 16) → baza 2 | 1111 |


