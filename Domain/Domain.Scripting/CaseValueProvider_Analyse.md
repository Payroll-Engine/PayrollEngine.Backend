# Code-Analyse: CaseValueProvider.cs

**Datei:** `Domain.Scripting\CaseValueProvider.cs`  
**Datum:** 25.02.2026  
**Fokus:** Fehler, Risiken und Dokumentationslücken in der Zeitwert-Berechnung

---

## 1. Kritische Fehler

### 1.1 Invertierte Slot-Filter-Logik in `GetCaseValuesAsync` (BUG)

**Position:** Innerhalb der `foreach`-Schleife in `GetCaseValuesAsync`

```csharp
// AKTUELL – BUG: filtert Werte MIT passendem Slot heraus statt Werte OHNE
if (!string.IsNullOrWhiteSpace(caseSlot) && string.Equals(caseSlot, caseValue.CaseSlot))
{
    continue;
}
```

Die Bedingung ist invertiert. Wenn ein `caseSlot` angegeben wird, sollen nur Werte dieses Slots berücksichtigt werden. Aktuell werden genau diese Werte übersprungen – das Gegenteil des gewünschten Verhaltens.

**Fix:**
```csharp
if (!string.IsNullOrWhiteSpace(caseSlot) && !string.Equals(caseSlot, caseValue.CaseSlot))
{
    continue;
}
```

**Impact:** Hoch – Alle slot-basierten Abfragen liefern falsche Ergebnisse.

---

### 1.2 `CalculateTimelessCaseValue` kann `null` zurückgeben → NullReferenceException

**Position:** Aufruf in `GetCasePeriodValuesAsync`

```csharp
if (caseField.TimeType == CaseFieldTimeType.Timeless)
{
    var caseValue = CalculateTimelessCaseValue(caseValues);
    values.Add(new()
    {
        Created = caseValue.Created,    // ← NullReferenceException
        Value = caseValue.Value         // ← NullReferenceException
    });
}
```

Wenn alle Case Values ein `Created`-Datum nach dem `EvaluationDate` haben, liefert `MaxBy` null.

**Fix:**
```csharp
var caseValue = CalculateTimelessCaseValue(caseValues);
if (caseValue == null)
{
    return values;
}
```

**Impact:** Hoch – Runtime-Crash bei vordatierten Einträgen.

---

### 1.3 Unerreichbarer Code: `CaseFieldTimeType.Moment` im Switch-Statement

In `GetCasePeriodValuesAsync` wird der `Moment`-Fall zweimal behandelt:

```csharp
// ERSTER Block (mit return → verlässt die Methode)
if (caseField.TimeType == CaseFieldTimeType.Moment)
{
    var value = calculator.CalculatePeriodValue(caseField, caseValues);
    // ...
    return values;
}

// ZWEITER Block (unerreichbar für Moment)
switch (caseField.TimeType)
{
    case CaseFieldTimeType.Moment:   // ← wird nie erreicht
        GetMomentCasePeriodValuesAsync(...);
        break;
}
```

**Impact:** Mittel – Klärung nötig, welcher der beiden Codepfade korrekt ist.

---

## 2. Logik-Risiken

### 2.1 `GetCaseValueRepository` gibt potenziell `null` zurück

Repositories für nicht konfigurierte Scopes sind `null`. Aufrufer wie `GetCaseValueSlotsAsync` prüfen nicht:

```csharp
var caseValueRepository = GetCaseValueRepository(caseType.Value);
return await caseValueRepository.GetCaseValueSlotsAsync(caseFieldName); // ← NRE
```

**Empfehlung:** `InvalidOperationException` in `GetCaseValueRepository` werfen.

---

### 2.2 Grenzwert-Probleme bei Period-Splitting

```csharp
if (periodEnd.IsMidnight()) { periodEnd = periodEnd.PreviousTick(); }
```

Risiken: Tick-Granularität, Off-by-one bei `IsLastMomentOfDay`/`NextTick`-Kombination, keine Validierung `periodStart < periodEnd` nach Korrektur.

---

### 2.3 Race Condition bei `RetroCaseValue`

`RetroCaseValue` wird ohne Synchronisation mutiert. Akzeptabel bei single-threaded Nutzung, muss aber dokumentiert werden.

---

### 2.4 `GetTimeCaseValuesAsync` – Fehlende Null-Prüfung

```csharp
var allCaseFieldNames = caseFieldNames.ToList(); // NRE wenn null
if (!allCaseFieldNames.Any())
{
    throw new ArgumentNullException(nameof(caseFieldNames)); // Falsche Exception
}
```

**Fix:** Expliziter Null-Check vor `ToList()`, `ArgumentException` für leere Liste.

---

### 2.5 `GetCasePeriodValuesAsync` – Falsche Exception-Typen

`ArgumentException` statt `ArgumentNullException` bei `null`-Prüfung.

---

## 3. Code-Qualitäts-Probleme

### 3.1 Irreführendes `Async`-Suffix bei synchronen Methoden

- `GetMomentCasePeriodValuesAsync` → `GetMomentCasePeriodValues`
- `GetAggregationCasePeriodValuesAsync` → `GetAggregationCasePeriodValues`
- `GetCalendarPeriodCasePeriodValuesAsync` → `GetCalendarPeriodCasePeriodValues`

### 3.2 Inkonsistente `ArgumentException`-Nutzung

```csharp
throw new ArgumentException(nameof(caseFieldName));
// Erzeugt: Message = "caseFieldName", ParamName = null
```

### 3.3 Inkonsistentes End-Datum-Handling

`EnsureLastMomentOfDay()` nur im `CalendarPeriod`-Pfad.

### 3.4 Redundante Sortierung

`GetCalendarPeriodCasePeriodValuesAsync` sortiert intern, Aufrufer sortiert nochmals.

---

## 4. Priorisierung

### Priorität 1 – Sofort

| # | Problem | Impact |
|---|---------|--------|
| 1.1 | Invertierte Slot-Filter-Logik | Falsche Daten |
| 1.2 | Null-Referenz bei CalculateTimelessCaseValue | Runtime-Crash |
| 1.3 | Toter Moment-Code | Evtl. fehlende Logik |

### Priorität 2 – Zeitnah

| # | Problem | Impact |
|---|---------|--------|
| 2.1 | Null-Repository nicht abgefangen | Runtime-Crash |
| 2.4 | Fehlende Null-Prüfung / falsche Exception | Irreführende Fehler |
| 2.5 | Falsche Exception-Typen | Irreführende Fehler |

### Priorität 3 – Code-Qualität

| # | Problem | Impact |
|---|---------|--------|
| 2.2 | Tick-Boundary-Risiken | Off-by-one |
| 2.3 | Thread-Safety undokumentiert | Wartbarkeit |
| 3.1 | Async-Suffix synchron | Konvention |
| 3.2 | Inkonsistente Exceptions | Debugging |
| 3.3 | Inkonsistentes End-Datum | Wartbarkeit |
| 3.4 | Doppelte Sortierung | Performance |

---

## 5. Vervollständigte XML-Dokumentation

### Klasse

```csharp
/// <summary>
/// Resolves case field values for scripts during payrun execution.
/// <para>
/// Supports all four case type scopes (Global, National, Company, Employee) through
/// a layered set of cache repositories. The active evaluation period is managed via
/// a push/pop stack to allow temporary period overrides during split-period calculations.
/// </para>
/// <para>
/// <b>Threading:</b> This class is <b>not</b> thread-safe. A single instance must only
/// be used from one logical execution context at a time (typically one payrun job).
/// </para>
/// </summary>
```

### `GetCaseValueSlotsAsync`

```csharp
/// <inheritdoc />
/// <exception cref="ArgumentException">
/// <paramref name="caseFieldName"/> is <c>null</c>, empty or whitespace.
/// </exception>
/// <exception cref="PayrollException">
/// The case field name does not resolve to a known case type.
/// </exception>
```

### `GetTimeCaseValuesAsync(DateTime, CaseType)`

```csharp
/// <inheritdoc />
/// <remarks>
/// Loads all derived case fields for <paramref name="caseType"/>, then delegates to
/// the name-based overload. Returns an empty list when no fields exist.
/// </remarks>
/// <exception cref="ArgumentException">
/// <paramref name="valueDate"/> is not in UTC.
/// </exception>
```

### `GetTimeCaseValuesAsync(DateTime, CaseType, IEnumerable<string>)`

```csharp
/// <inheritdoc />
/// <remarks>
/// For each case field name the method determines the appropriate value based on
/// the field's <see cref="CaseFieldTimeType"/>:
/// <list type="bullet">
///   <item><b>Timeless</b> — most recently created value.</item>
///   <item><b>Moment</b> — latest value whose <c>Start ≤ valueDate</c>.</item>
///   <item><b>Period / CalendarPeriod</b> — most recently created value
///         whose period contains <paramref name="valueDate"/>.</item>
/// </list>
/// Unknown field names or type mismatches are silently skipped.
/// </remarks>
/// <exception cref="ArgumentException">
/// <paramref name="valueDate"/> is not UTC, or <paramref name="caseFieldNames"/> is empty.
/// </exception>
```

### `GetCaseValuesAsync`

```csharp
/// <inheritdoc />
/// <remarks>
/// Returns case values whose <see cref="CaseValue.Created"/> falls within
/// <paramref name="evaluationPeriod"/>. Optional <paramref name="caseSlot"/>
/// restricts to that slot.
/// <para><b>BUG:</b> Slot filter logic is inverted (see analysis 1.1).</para>
/// </remarks>
/// <returns>
/// List of <see cref="CaseFieldValue"/>, or <c>null</c> if field/type unresolvable.
/// </returns>
```

### `GetCaseValueSplitPeriodsAsync`

```csharp
/// <inheritdoc />
/// <remarks>
/// Splits each case value's effective range into sub-periods aligned with the
/// evaluation period via <see cref="CaseValueProviderCalculation.SplitCaseValuePeriods"/>.
/// Returns empty dictionary when no values exist.
/// </remarks>
```

### `GetCasePeriodValuesAsync(DatePeriod, IEnumerable<string>)`

```csharp
/// <inheritdoc />
/// <remarks>
/// Multi-field splitting: (1) load all field values, (2) collect boundary moments,
/// (3) re-evaluate per field × sub-period. Skipped for single-field requests.
/// Tick corrections prevent zero-length or overlapping periods at midnight boundaries.
/// </remarks>
/// <exception cref="InvalidOperationException">
/// A sub-period produced more than one value for a single field.
/// </exception>
```

### `CalculateTimelessCaseValue`

```csharp
/// <summary>
/// Returns the most recently created case value on or before <see cref="EvaluationDate"/>.
/// </summary>
/// <returns>
/// The winning value, or <c>null</c> if none qualifies. <b>Callers must null-check.</b>
/// </returns>
```

### `CalculatePeriodCaseValuesAsync`

```csharp
/// <summary>
/// Loads period-relevant case values and triggers retro detection as side effect.
/// May update <see cref="RetroCaseValue"/>.
/// </summary>
```

### `UpdateRetroCaseValue`

```csharp
/// <remarks>
/// Retro trigger: value created after <see cref="RetroDate"/> with Start before
/// current evaluation period. Only tightens the retro window, never moves it forward.
/// <para><b>Side effect:</b> Mutates <see cref="RetroCaseValue"/>. Not thread-safe.</para>
/// </remarks>
```

### `GetCaseValueRepository`

```csharp
/// <remarks>
/// May return <c>null</c> for scopes not configured during construction.
/// Callers must guard against this.
/// </remarks>
```
