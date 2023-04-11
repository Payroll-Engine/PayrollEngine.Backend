﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace PayrollEngine.Domain.Model;

/// <summary>
/// Extension methods for the <see cref="CaseSet"/>
/// </summary>
public static class CaseSetExtensions
{

    /// <summary>
    /// Find case by name, considering related cases
    /// </summary>
    /// <param name="caseSet">The case to search</param>
    /// <param name="caseName">The name of the case</param>
    /// <returns>The value case field matching the name, null on missing case field</returns>
    public static CaseSet FindCase(this CaseSet caseSet, string caseName)
    {
        if (caseSet == null)
        {
            return null;
        }
        if (string.IsNullOrWhiteSpace(caseName))
        {
            throw new ArgumentException(nameof(caseName));
        }

        // local case
        if (string.Equals(caseSet.Name, caseName))
        {
            return caseSet;
        }

        //  search case recursively in related cases
        if (caseSet.RelatedCases != null)
        {
            foreach (var relatedCase in caseSet.RelatedCases)
            {
                var @case = FindCase(relatedCase, caseName);
                if (@case != null)
                {
                    return @case;
                }
            }
        }
        return null;
    }

    /// <summary>
    /// Find case field by name, considering related cases
    /// </summary>
    /// <param name="caseSet">The case to search</param>
    /// <param name="caseFieldName">The name of the case field</param>
    /// <returns>The value case field matching the name, null on missing case field</returns>
    public static CaseFieldSet FindCaseField(this CaseSet caseSet, string caseFieldName)
    {
        if (caseSet == null)
        {
            return null;
        }
        if (string.IsNullOrWhiteSpace(caseFieldName))
        {
            throw new ArgumentException(nameof(caseFieldName));
        }

        // case value reference
        var caseValueReference = new CaseValueReference(caseFieldName);

        // find local value case field
        CaseFieldSet caseFieldSet;
        if (caseValueReference.HasCaseSlot)
        {
            // case field name and case slot
            caseFieldSet = caseSet.Fields?.FirstOrDefault(x => string.Equals(x.Name, caseValueReference.CaseFieldName) &&
                                                               string.Equals(x.CaseSlot, caseValueReference.CaseSlot));
        }
        else
        {
            // case field name only
            caseFieldSet = caseSet.Fields?.FirstOrDefault(x => string.Equals(x.Name, caseValueReference.CaseFieldName));
        }

        //  search field recursively in related cases
        if (caseFieldSet == null && caseSet.RelatedCases != null)
        {
            foreach (var relatedCase in caseSet.RelatedCases)
            {
                caseFieldSet = FindCaseField(relatedCase, caseFieldName);
                if (caseFieldSet != null)
                {
                    return caseFieldSet;
                }
            }
        }
        return caseFieldSet;
    }

    /// <summary>
    /// Collect case fields
    /// </summary>
    /// <param name="caseSet">The case to collect</param>
    /// <returns>List of case fields, including fields of related cases</returns>
    public static IList<CaseFieldSet> CollectFields(this CaseSet caseSet)
    {
        var values = new List<CaseFieldSet>();
        if (caseSet != null)
        {
            CollectFields(caseSet, values);
        }
        return values;
    }

    private static void CollectFields(CaseSet caseSet, List<CaseFieldSet> values)
    {
        if (caseSet == null)
        {
            return;
        }

        // case fields
        if (caseSet.Fields != null)
        {
            values.AddRange(caseSet.Fields);
        }

        // search case fields recursively in related cases
        if (caseSet.RelatedCases != null)
        {
            foreach (var relatedCase in caseSet.RelatedCases)
            {
                CollectFields(relatedCase, values);
            }
        }
    }

    /// <summary>
    /// Set the case cancellation date
    /// </summary>
    /// <param name="caseSet">The case to setup</param>
    /// <param name="cancellationDate">The case cancellation date</param>
    public static void SetCancellationDate(this CaseSet caseSet, DateTime? cancellationDate)
    {
        if (caseSet == null)
        {
            return;
        }

        // local case
        caseSet.CancellationDate = cancellationDate;

        //  search case recursively in related cases
        if (caseSet.RelatedCases != null)
        {
            foreach (var relatedCase in caseSet.RelatedCases)
            {
                SetCancellationDate(relatedCase, cancellationDate);
            }
        }
    }
}