using PayrollEngine.Data;
using PayrollEngine.Domain.Model;
using PayrollEngine.Persistence;
using Dapper;
using Microsoft.AspNetCore.Builder;

namespace PayrollEngine.Api.Core;

public static class ApiDapperExtensions
{
    public static IApplicationBuilder UseDapper(this IApplicationBuilder appBuilder)
    {
        // type handlers

        // calendar configuration
        SqlMapper.AddTypeHandler(new JsonObjectTypeHandler<CalendarConfiguration>());
        // lookup settings
        SqlMapper.AddTypeHandler(new JsonObjectTypeHandler<LookupSettings>());
        // case relation reference
        SqlMapper.AddTypeHandler(new JsonObjectTypeHandler<CaseRelationReference>());
        // case slot list
        SqlMapper.AddTypeHandler(new ListTypeHandler<CaseSlot>());
        // Report data relation
        SqlMapper.AddTypeHandler(new ListTypeHandler<DataRelation>());
        // payroll cluster set list
        SqlMapper.AddTypeHandler(new ListTypeHandler<ClusterSet>());
        // payroll case field reference
        SqlMapper.AddTypeHandler(new ListTypeHandler<CaseFieldReference>());
        // string list
        SqlMapper.AddTypeHandler(new ListTypeHandler<string>());
        // localized strings (e.g. case name)
        SqlMapper.AddTypeHandler(new NamedDictionaryTypeHandler<string>());
        // localized values (e.g. lookup value)
        SqlMapper.AddTypeHandler(new NamedDictionaryTypeHandler<object>());
        return appBuilder;
    }
}