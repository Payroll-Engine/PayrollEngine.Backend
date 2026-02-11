using System;
using System.Linq;
using System.Text.Json;
using System.Collections.Generic;

namespace PayrollEngine.Domain.Model;

/// <summary>Extension methods for lookup sets</summary>
/// <remarks>Code duplicated in PayrollEngine.Client.Model</remarks>
public static class LookupSetExtensions
{
    /// <summary>Apply range value</summary>
    /// <param name="lookup">The lookup</param>
    /// <param name="rangeValue">The range value</param>
    /// <param name="valueFieldName">Value field name</param>
    /// <returns>Lookup range value</returns>
    public static decimal ApplyRangeValue(this LookupSet lookup, decimal rangeValue, string valueFieldName = null)
    {
        if (rangeValue == 0)
        {
            return 0;
        }
        switch (lookup.RangeMode)
        {
            case LookupRangeMode.Threshold:
                return lookup.ApplyThresholdRangeValue(rangeValue, valueFieldName);
            case LookupRangeMode.Progressive:
                return lookup.ApplyProgressiveRangeValue(rangeValue, valueFieldName);
        }
        return 0;
    }

    #region Threshold

    /// <summary>Apply progressive factor value</summary>
    /// <param name="lookup">The lookup</param>
    /// <param name="rangeValue">The range value</param>
    /// <param name="valueFieldName">Value field name</param>
    /// <remarks>The first lookup range value must be zero.</remarks>
    /// <returns>Summary of all lookup ranges</returns>
    private static decimal ApplyThresholdRangeValue(this LookupSet lookup, decimal rangeValue, string valueFieldName = null)
    {
        // ranges
        var ranges = GetLookupRanges(lookup, valueFieldName);

        // select threshold range
        var result = ranges?.FirstOrDefault(x => x.IsThreshold(rangeValue));
        var factor = result?.Factor ?? 0;
        return rangeValue * factor;
    }

    #endregion

    #region Progressive

    /// <summary>Apply progressive factor value</summary>
    /// <param name="lookup">The lookup</param>
    /// <param name="rangeValue">The range value</param>
    /// <param name="valueFieldName">Value field name</param>
    /// <remarks>The first lookup range value must be zero.</remarks>
    /// <returns>Summary of all lookup ranges</returns>
    private static decimal ApplyProgressiveRangeValue(this LookupSet lookup, decimal rangeValue, string valueFieldName = null)
    {
        // ranges
        var ranges = GetLookupRanges(lookup, valueFieldName);

        // calculate factor for each lookup range
        var result = ranges.Select(x => x.GetRangeFactorValue(rangeValue)).Sum();
        return result;
    }

    #endregion

    #region Range Brackets

    /// <summary>Build range brackets with computed upper bounds</summary>
    /// <param name="lookup">The lookup set</param>
    /// <param name="rangeValue">Optional value to find matching bracket(s)</param>
    /// <returns>Lookup range result with all brackets and optional match</returns>
    public static LookupRangeResult BuildRangeBrackets(this LookupSet lookup, decimal? rangeValue = null)
    {
        if (lookup == null)
        {
            throw new ArgumentNullException(nameof(lookup));
        }
        if (lookup.Values == null || !lookup.Values.Any())
        {
            throw new ArgumentException(nameof(lookup));
        }

        // range value guard
        if (rangeValue.HasValue && lookup.Values.Any(x => x.RangeValue == null))
        {
            throw new PayrollException($"Lookup range value request on lookup {lookup.Name} with missing range value.");
        }

        // build brackets
        var brackets = new List<LookupRangeBracket>();
        var precision = (decimal)Math.Pow(10, -SystemSpecification.DecimalScale);
        if (lookup.Values != null)
        {
            foreach (var lookupValue in lookup.Values)
            {
                // ignore lookup values without range and lookup value
                if (lookupValue.RangeValue == null || string.IsNullOrWhiteSpace(lookupValue.Value))
                {
                    continue;
                }

                // update previous upper bound
                if (brackets.Count > 0)
                {
                    brackets.Last().RangeEnd = lookupValue.RangeValue.Value - precision;
                }

                // add new bracket
                brackets.Add(new LookupRangeBracket
                {
                    Key = lookupValue.Key,
                    Value = lookupValue.Value,
                    RangeStart = lookupValue.RangeValue.Value
                });
            }

            // last bracket
            var last = brackets.LastOrDefault();
            if (last != null)
            {
                if (lookup.RangeSize.HasValue)
                {
                    // limited by the lookup range size
                    last.RangeEnd = last.RangeStart + lookup.RangeSize.Value - precision;
                }
                else
                {
                    last.SetUnlimited();
                }
            }
        }

        // range result
        var result = new LookupRangeResult
        {
            RangeMode = lookup.RangeMode,
            RangeSize = lookup.RangeSize,
            Brackets = brackets
        };

        // bracket range values
        if (rangeValue.HasValue)
        {
            foreach (var bracket in brackets)
            {
                switch (lookup.RangeMode)
                {
                    case LookupRangeMode.Threshold:
                        if (rangeValue.Value >= bracket.RangeStart &&
                            (rangeValue.Value <= bracket.RangeEnd || bracket.IsUnlimited))
                        {
                            bracket.RangeValue = rangeValue.Value - bracket.RangeStart;
                        }
                        break;
                    case LookupRangeMode.Progressive:
                        if (rangeValue.Value >= bracket.RangeEnd)
                        {
                            // full range value
                            bracket.RangeValue = bracket.RangeEnd - bracket.RangeStart + precision;
                        }
                        else if (rangeValue.Value >= bracket.RangeStart)
                        {
                            // partial range value
                            bracket.RangeValue = rangeValue.Value - bracket.RangeStart;
                        }
                        break;
                }
            }
        }

        return result;
    }

    #endregion

    #region Lookup Range

    private sealed class LookupRange
    {
        internal decimal Factor { get; }
        private decimal Start { get; }
        private decimal? End { get; set; }

        internal LookupRange(decimal factor, decimal start)
        {
            Factor = factor;
            Start = start;
        }

        internal void SetEnd(decimal end) => End = end;
        internal void SetEndByOffset(decimal offset) =>
            End = Start + offset;

        internal bool IsThreshold(decimal rangeValue) =>
            rangeValue >= Start &&
            (!End.HasValue || rangeValue < End.Value);

        internal decimal GetRangeFactorValue(decimal rangeValue)
        {
            // undefined
            if ((End.HasValue && End <= Start) || Factor == 0 || rangeValue == 0)
            {
                return 0;
            }
            // outside (ignore start is equals rangeValue)
            if (Start >= rangeValue || (End.HasValue && End < 0))
            {
                return 0;
            }

            // lookup range intersection with the range value (0...range value)
            var rangeStart = Math.Max(Start, 0);
            var rangeEnd = End.HasValue ? Math.Min(End.Value, rangeValue) : rangeValue;
            var rangeSize = rangeEnd - rangeStart;
            return rangeSize > 0 ? rangeSize * Factor : 0;
        }

        public override string ToString() =>
            $"{Start} - {End} ({Factor})";
    }

    private static List<LookupRange> GetLookupRanges(LookupSet lookup, string valueFieldName = null)
    {
        if (!lookup.Values.Any())
        {
            return [];
        }

        // ranges
        var lookupValues = lookup.Values.OrderBy(x => x.RangeValue).ToList();
        var ranges = new List<LookupRange>();
        for (var i = 0; i < lookupValues.Count; i++)
        {
            var lookupValue = lookupValues[i];

            // ignore lookup values without range and lookup value
            if (lookupValue.RangeValue == null || string.IsNullOrWhiteSpace(lookupValue.Value))
            {
                continue;
            }

            // first value need to be zero
            if (i == 0 && lookupValue.RangeValue.Value != 0)
            {
                throw new PayrollException(
                    $"Get range factor requires a start range value of zero ({lookupValue.RangeValue.Value}).");
            }

            // factor
            decimal? factor = null;
            if (string.IsNullOrWhiteSpace(valueFieldName))
            {
                // decimal factor from lookup value
                factor = JsonSerializer.Deserialize<decimal?>(lookupValue.Value);
            }
            else
            {
                // decimal factor from JSON object filed
                var values = JsonSerializer.Deserialize<Dictionary<string, object>>(lookupValue.Value);
                if (values != null && values[valueFieldName] is JsonElement jsonElement)
                {
                    factor = jsonElement.GetDecimal();
                }
            }

            if (!factor.HasValue)
            {
                continue;
            }

            // update previous end
            if (i > 0)
            {
                ranges[i - 1].SetEnd(lookupValue.RangeValue.Value);
            }

            // add new range
            ranges.Add(new LookupRange(
                factor: factor.Value,
                start: lookupValue.RangeValue.Value));
        }

        // last range
        if (lookup.RangeSize.HasValue)
        {
            var last = ranges.Last();
            last.SetEndByOffset(lookup.RangeSize.Value);
        }

        return ranges;
    }

    #endregion

}