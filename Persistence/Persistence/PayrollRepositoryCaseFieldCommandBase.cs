using System;
using System.Collections.Generic;
using System.Linq;
using PayrollEngine.Domain.Model;

namespace PayrollEngine.Persistence;

internal abstract class PayrollRepositoryCaseFieldCommandBase(IDbContext dbContext) : PayrollRepositoryCommandBase(dbContext)
{
    protected static void BuildDerivedCaseFields(List<DerivedCaseField> caseFields, OverrideType? overrideType = null)
    {
        // argument check
        if (caseFields == null)
        {
            throw new ArgumentNullException(nameof(caseFields));
        }
        if (!caseFields.Any())
        {
            return;
        }

        // resulting case fields
        var caseFieldsByKey = caseFields.GroupBy(x => x.Name).ToList();

        // override filter
        if (overrideType.HasValue)
        {
            ApplyOverrideFilter(caseFieldsByKey, caseFields, overrideType.Value);
            // update case fields
            caseFieldsByKey = caseFields.GroupBy(x => x.Name).ToList();
        }

        // collect derived values
        foreach (var caseField in caseFieldsByKey)
        {
            // derived order
            var derivedCaseFields = caseField.OrderByDescending(x => x.Level).ThenByDescending(x => x.Priority).ToList();

            // derived case fields
            while (derivedCaseFields.Count > 1)
            {
                // collect values to the topmost case field
                var derivedCaseField = derivedCaseFields.First();
                // non-derived fields: name, all non-nullable and expressions
                derivedCaseField.NameLocalizations = CollectDerivedValue(derivedCaseFields, x => x.NameLocalizations);
                derivedCaseField.Description = CollectDerivedValue(derivedCaseFields, x => x.Description);
                derivedCaseField.DescriptionLocalizations = CollectDerivedValue(derivedCaseFields, x => x.DescriptionLocalizations);
                derivedCaseField.Culture = CollectDerivedValue(derivedCaseFields, x => x.Culture);
                derivedCaseField.DefaultStart = CollectDerivedValue(derivedCaseFields, x => x.DefaultStart);
                derivedCaseField.DefaultEnd = CollectDerivedValue(derivedCaseFields, x => x.DefaultEnd);
                derivedCaseField.DefaultValue = CollectDerivedValue(derivedCaseFields, x => x.DefaultValue);
                derivedCaseField.Tags = CollectDerivedList(derivedCaseFields, x => x.Tags);
                derivedCaseField.LookupSettings = CollectDerivedValue(derivedCaseFields, x => x.LookupSettings);
                derivedCaseField.Clusters = CollectDerivedList(derivedCaseFields, x => x.Clusters);
                derivedCaseField.Attributes = CollectDerivedAttributes(derivedCaseFields);
                derivedCaseField.ValueAttributes = CollectDerivedAttributes(derivedCaseFields);
                // remove the current level for the next iteration
                derivedCaseFields.Remove(derivedCaseField);
            }
        }
    }
}