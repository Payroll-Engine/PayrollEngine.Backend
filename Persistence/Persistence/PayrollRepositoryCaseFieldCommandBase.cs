using System;
using System.Collections.Generic;
using System.Linq;
using PayrollEngine.Domain.Model;

namespace PayrollEngine.Persistence;

internal abstract class PayrollRepositoryCaseFieldCommandBase : PayrollRepositoryCommandBase
{

    protected PayrollRepositoryCaseFieldCommandBase(IDbContext dbContext) :
        base(dbContext)
    {
    }

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
                derivedCaseField.NameLocalizations = CollectDerivedValue(derivedCaseFields, x => x.NameLocalizations);
                derivedCaseField.Description = CollectDerivedValue(derivedCaseFields, x => x.Description);
                derivedCaseField.DescriptionLocalizations = CollectDerivedValue(derivedCaseFields, x => x.DescriptionLocalizations);
                derivedCaseField.ValueType = CollectDerivedValue(derivedCaseFields, x => x.ValueType);
                derivedCaseField.ValueScope = CollectDerivedValue(derivedCaseFields, x => x.ValueScope);
                derivedCaseField.TimeType = CollectDerivedValue(derivedCaseFields, x => x.TimeType);
                derivedCaseField.TimeUnit = CollectDerivedValue(derivedCaseFields, x => x.TimeUnit);
                derivedCaseField.CancellationMode = CollectDerivedValue(derivedCaseFields, x => x.CancellationMode);
                derivedCaseField.ValueCreationMode = CollectDerivedValue(derivedCaseFields, x => x.ValueCreationMode);
                derivedCaseField.StartDateType = CollectDerivedValue(derivedCaseFields, x => x.StartDateType);
                derivedCaseField.EndDateType = CollectDerivedValue(derivedCaseFields, x => x.EndDateType);
                derivedCaseField.DefaultStart = CollectDerivedValue(derivedCaseFields, x => x.DefaultStart);
                derivedCaseField.DefaultEnd = CollectDerivedValue(derivedCaseFields, x => x.DefaultEnd);
                derivedCaseField.DefaultValue = CollectDerivedValue(derivedCaseFields, x => x.DefaultValue);
                derivedCaseField.Tags = CollectDerivedList(derivedCaseFields, x => x.Tags);
                derivedCaseField.LookupSettings = CollectDerivedValue(derivedCaseFields, x => x.LookupSettings);
                derivedCaseField.Clusters = CollectDerivedList(derivedCaseFields, x => x.Clusters);
                derivedCaseField.BuildActions = CollectDerivedList(derivedCaseFields, x => x.BuildActions);
                derivedCaseField.ValidateActions = CollectDerivedList(derivedCaseFields, x => x.ValidateActions);
                derivedCaseField.Attributes = CollectDerivedAttributes(derivedCaseFields);
                derivedCaseField.ValueAttributes = CollectDerivedAttributes(derivedCaseFields);
                // remove the current level for the next iteration
                derivedCaseFields.Remove(derivedCaseField);
            }
        }
    }
}