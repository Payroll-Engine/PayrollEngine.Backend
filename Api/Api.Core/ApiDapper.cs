using System;
using System.Data;
using Dapper;
using Microsoft.Extensions.DependencyInjection;

namespace PayrollEngine.Api.Core;

public static class ApiDapper
{
    public static void AddDapper(this IServiceCollection services)
    {
        // date time
        SqlMapper.AddTypeMap(typeof(DateTime), DbType.DateTime2);

        // boolean
        // TODO: support boolean type handler
        // https://github.com/StackExchange/Dapper/issues/433
    }
}