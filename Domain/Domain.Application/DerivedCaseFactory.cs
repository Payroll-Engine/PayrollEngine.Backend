using System.Collections.Generic;
using System.Linq;
using PayrollEngine.Domain.Model;

namespace PayrollEngine.Domain.Application;

public static class DerivedCaseFactory
{
    public static CaseSet BuildCase(IList<Case> cases, string caseSlot, Language language)
    {
        // validation
        cases.ValidateDerivedTypes();

        // copy the most derived case
        var derivedCase = new CaseSet(cases.First())
        {
            // case slot
            CaseSlot = caseSlot,
            // collect derived attributes
            Attributes = cases.CollectDerivedAttributes(@case => @case.Attributes)
        };

        var languageCode = language.LanguageCode();

        // display name
        derivedCase.DisplayName = derivedCase.Name;
        foreach (var @case in cases)
        {
            if (@case.NameLocalizations != null && @case.NameLocalizations.TryGetValue(languageCode, out var localization))
            {
                derivedCase.DisplayName = localization;
                break;
            }
        }

        // description
        foreach (var @case in cases)
        {
            if (@case.DescriptionLocalizations != null && @case.DescriptionLocalizations.TryGetValue(languageCode, out var localization))
            {
                derivedCase.Description = localization;
                break;
            }
            if (!string.IsNullOrWhiteSpace(@case.Description))
            {
                derivedCase.Description = @case.Description;
                break;
            }
        }

        // default reason
        foreach (var @case in cases)
        {
            if (@case.DefaultReasonLocalizations != null && @case.DefaultReasonLocalizations.TryGetValue(languageCode, out var localization))
            {
                derivedCase.DefaultReason = localization;
                break;
            }
            if (!string.IsNullOrWhiteSpace(@case.DefaultReason))
            {
                derivedCase.DefaultReason = @case.DefaultReason;
                break;
            }
        }

        return derivedCase;
    }

    public static CaseFieldSet BuildCaseField(IGrouping<string, CaseField> caseFields, Language language)
    {
        var caseFieldList = caseFields.ToList();

        // validation
        caseFieldList.ValidateDerivedTypes();

        // copy the most derived case field
        var derivedCaseField = new CaseFieldSet(caseFields.First())
        {
            // collect derived attributes
            Attributes = caseFieldList.CollectDerivedAttributes(caseField => caseField.Attributes),
            // collect derived value attributes
            ValueAttributes = caseFieldList.CollectDerivedAttributes(caseField => caseField.ValueAttributes)
        };

        var languageCode = language.LanguageCode();

        // name: most derived localized
        derivedCaseField.DisplayName = derivedCaseField.Name;
        derivedCaseField.CaseSlot = derivedCaseField.CaseSlot;
        derivedCaseField.CaseSlotLocalizations = derivedCaseField.CaseSlotLocalizations;
        foreach (var caseField in caseFieldList)
        {
            if (caseField.NameLocalizations != null && caseField.NameLocalizations.TryGetValue(languageCode, out var localization))
            {
                derivedCaseField.DisplayName = localization;
                break;
            }
        }

        // description: most derived localized or most derived
        foreach (var caseField in caseFieldList)
        {
            if (caseField.DescriptionLocalizations != null && caseField.DescriptionLocalizations.TryGetValue(languageCode, out var localization))
            {
                derivedCaseField.Description = localization;
                break;
            }
            if (!string.IsNullOrWhiteSpace(caseField.Description))
            {
                derivedCaseField.Description = caseField.Description;
                break;
            }
        }

        // default start: most derived
        foreach (var caseField in caseFieldList)
        {
            if (!string.IsNullOrWhiteSpace(caseField.DefaultStart))
            {
                derivedCaseField.DefaultStart = caseField.DefaultStart;
                break;
            }
        }

        // default end: most derived
        foreach (var caseField in caseFieldList)
        {
            if (!string.IsNullOrWhiteSpace(caseField.DefaultEnd))
            {
                derivedCaseField.DefaultEnd = caseField.DefaultEnd;
                break;
            }
        }

        // default value: most derived
        foreach (var caseField in caseFieldList)
        {
            if (!string.IsNullOrWhiteSpace(caseField.DefaultValue))
            {
                derivedCaseField.DefaultValue = caseField.DefaultValue;
                break;
            }
        }

        // lookup settings: most derived
        foreach (var caseField in caseFieldList)
        {
            if (caseField.LookupSettings != null)
            {
                derivedCaseField.LookupSettings = caseField.LookupSettings;
                break;
            }
        }

        return derivedCaseField;
    }
}