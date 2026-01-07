using System;
using System.Collections.Generic;

namespace PayrollEngine.Domain.Model;

/// <summary>
/// Extension methods for the <see cref="CaseChangeSetup"/>
/// </summary>
public static class CaseChangeSetupExtensions
{
    /// <summary>
    /// Find case setup
    /// </summary>
    /// <param name="caseChangeSetup">The case change setup</param>
    /// <param name="caseName">The case name</param>
    /// <param name="caseSlot">The case slot</param>
    /// <returns>The case value or null on missing case value</returns>
    public static CaseSetup FindCaseSetup(this CaseChangeSetup caseChangeSetup, string caseName, string caseSlot = null)
    {
        if (string.IsNullOrWhiteSpace(caseName))
        {
            throw new ArgumentException(nameof(caseName));
        }
        return caseChangeSetup == null ? null : FindCaseSetup(caseChangeSetup.Case, caseName, caseSlot);
    }

    private static CaseSetup FindCaseSetup(CaseSetup caseSetup, string caseName, string caseSlot)
    {
        if (caseSetup == null)
        {
            return null;
        }

        // case name and case slot
        if (string.Equals(caseSetup.CaseName, caseName) && string.Equals(caseSetup.CaseSlot, caseSlot))
        {
            return caseSetup;
        }

        // related case values
        if (caseSetup.Values != null)
        {
            foreach (var relatedValue in caseSetup.Values)
            {
                if (relatedValue.CaseRelation != null &&
                    string.Equals(caseName, relatedValue.CaseRelation.TargetCaseName) &&
                    string.Equals(caseSlot, relatedValue.CaseRelation.TargetCaseSlot))
                {
                    // inline case relation
                    return caseSetup;
                }
            }
        }

        // related cases
        if (caseSetup.RelatedCases != null)
        {
            foreach (var relatedCase in caseSetup.RelatedCases)
            {
                var @case = FindCaseSetup(relatedCase, caseName, caseSlot);
                if (@case != null)
                {
                    return @case;
                }
            }
        }

        return null;
    }

    /// <summary>
    /// Find case value
    /// </summary>
    /// <param name="caseChangeSetup">The case change setup</param>
    /// <param name="caseFieldName">The case field name</param>
    /// <param name="caseSlot">The case slot</param>
    /// <returns>The case value or null on missing case value</returns>
    public static CaseValueSetup FindCaseValue(this CaseChangeSetup caseChangeSetup, string caseFieldName, string caseSlot = null)
    {
        if (string.IsNullOrWhiteSpace(caseFieldName))
        {
            throw new ArgumentException(nameof(caseFieldName));
        }
        return caseChangeSetup == null ? null : FindCaseValue(caseChangeSetup.Case, caseFieldName, caseSlot);
    }

    private static CaseValueSetup FindCaseValue(CaseSetup caseSetup, string caseFieldName, string caseSlot)
    {
        if (caseSetup == null)
        {
            return null;
        }

        // case value by slot
        if (string.Equals(caseSetup.CaseSlot, caseSlot))
        {
            if (caseSetup.Values != null)
            {
                foreach (var caseValue in caseSetup.Values)
                {
                    if (string.Equals(caseValue.CaseFieldName, caseFieldName))
                    {
                        return caseValue;
                    }
                }
            }
        }

        // related cases
        if (caseSetup.RelatedCases != null)
        {
            foreach (var relatedCase in caseSetup.RelatedCases)
            {
                var caseValue = FindCaseValue(relatedCase, caseFieldName, caseSlot);
                if (caseValue != null)
                {
                    return caseValue;
                }
            }
        }

        return null;
    }

    /// <param name="caseChangeSetup">The case change setup</param>
    extension(CaseChangeSetup caseChangeSetup)
    {
        /// <summary>
        /// Search for duplicated case value
        /// </summary>
        /// <returns>The duplicated case value, null without duplicates</returns>
        public CaseValue FindDuplicatedCaseValue()
        {
            var caseValueLookup = new Dictionary<Tuple<string, string>, CaseValue>();
            foreach (var caseValue in caseChangeSetup.CollectCaseValues())
            {
                var key = new Tuple<string, string>(caseValue.CaseFieldName, caseValue.CaseSlot);
                if (!caseValueLookup.TryAdd(key, caseValue))
                {
                    // duplicated case value
                    return caseValue;
                }
            }
            return null;
        }

        /// <summary>
        /// Collect all case setups
        /// </summary>
        /// <returns>List if case setups</returns>
        public List<CaseSetup> CollectCaseSetups()
        {
            var caseSetups = new List<CaseSetup>();
            if (caseChangeSetup != null)
            {
                CollectCaseSetups(caseChangeSetup.Case, caseSetups);
            }
            return caseSetups;
        }
    }

    private static void CollectCaseSetups(CaseSetup caseSetup, List<CaseSetup> caseSetups)
    {
        if (caseSetup != null)
        {
            caseSetups.Add(caseSetup);
            if (caseSetup.RelatedCases != null)
            {
                foreach (var relatedCase in caseSetup.RelatedCases)
                {
                    CollectCaseSetups(relatedCase, caseSetups);
                }
            }
        }
    }

    /// <summary>
    /// Collect all case values
    /// </summary>
    /// <param name="caseChangeSetup">The case change setup</param>
    /// <returns>List if case values</returns>
    public static List<CaseValue> CollectCaseValues(this CaseChangeSetup caseChangeSetup)
    {
        var caseValues = new List<CaseValue>();
        if (caseChangeSetup != null)
        {
            CollectCaseValues(caseChangeSetup.Case, caseValues);
        }
        return caseValues;
    }

    private static void CollectCaseValues(CaseSetup caseSetup, List<CaseValue> caseValues, CaseSetup relation = null)
    {
        if (caseSetup != null)
        {
            if (caseSetup.Values != null)
            {
                if (relation != null)
                {
                    foreach (var caseValue in caseSetup.Values)
                    {
                        caseValue.CaseRelation = new()
                        {
                            SourceCaseName = relation.CaseName,
                            SourceCaseSlot = relation.CaseSlot,
                            TargetCaseName = caseSetup.CaseName,
                            TargetCaseSlot = caseSetup.CaseSlot
                        };
                    }
                }
                caseValues.AddRange(caseSetup.Values);
            }

            if (caseSetup.RelatedCases != null)
            {
                foreach (var relatedCase in caseSetup.RelatedCases)
                {
                    CollectCaseValues(relatedCase, caseValues, caseSetup);
                }
            }
        }
    }
}