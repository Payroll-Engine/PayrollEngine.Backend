using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

namespace PayrollEngine.Persistence;

public static class DbScriptingExtensions
{
    extension(StringBuilder stringBuilder)
    {
        public void AppendDbInsert(string tableName, ICollection<string> fieldNames)
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

        public void AppendDbUpdate(string tableName,
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

        public void AppendIdentitySelect()
        {
            stringBuilder.Append("SELECT CAST(SCOPE_IDENTITY() as int);");
        }
    }
}