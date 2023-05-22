using System;
using System.Data;
using Dapper;
using PayrollEngine.Domain.Model;
using DataRelation = PayrollEngine.Data.DataRelation;

namespace PayrollEngine.Persistence;

public static class DapperTypes
{
    /// <summary>
    /// Setup Dapper type handlers
    /// </summary>
    public static void AddTypeHandlers()
    {
        // type handlers
        // date time
        SqlMapper.AddTypeMap(typeof(DateTime), DbType.DateTime2);
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
    }
}