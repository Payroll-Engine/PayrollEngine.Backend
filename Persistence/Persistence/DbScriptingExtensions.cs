using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PayrollEngine.Persistence;

public static class DbScriptingExtensions
{
    public static void AppendDbInsert(this StringBuilder stringBuilder, string tableName, ICollection<string> fieldNames)
    {
        if (fieldNames == null)
        {
            throw new ArgumentNullException(nameof(fieldNames));
        }

        var lastValueKey = fieldNames.Last();

        stringBuilder.Append($"INSERT INTO [{tableName}] (");
        foreach (var field in fieldNames)
        {
            stringBuilder.Append($"[{field}]");
            if (field != lastValueKey)
            {
                stringBuilder.Append(",");
            }
        }
        stringBuilder.Append(") VALUES (");
        foreach (var field in fieldNames)
        {
            stringBuilder.Append($"@{field}");
            if (field != lastValueKey)
            {
                stringBuilder.Append(",");
            }
        }
        stringBuilder.Append(");");
    }

    public static void AppendDbUpdate(this StringBuilder stringBuilder, string tableName,
        ICollection<string> fieldNames, int id)
    {
        if (fieldNames == null)
        {
            throw new ArgumentNullException(nameof(fieldNames));
        }

        var lastValueKey = fieldNames.Last();

        stringBuilder.Append($"UPDATE [{tableName}] SET ");
        foreach (var field in fieldNames)
        {
            stringBuilder.Append($"[{field}] = @{field}");
            if (field != lastValueKey)
            {
                stringBuilder.Append(",");
            }
        }
        stringBuilder.Append($" WHERE [Id] = {id};");
    }

    public static void AppendIdentitySelect(this StringBuilder stringBuilder)
    {
        stringBuilder.Append("SELECT CAST(SCOPE_IDENTITY() as int);");
    }
}