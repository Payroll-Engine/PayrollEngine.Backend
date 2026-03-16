using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PayrollEngine.Domain.Model;

namespace PayrollEngine.Persistence;

public static class DbScriptingExtensions
{
    extension(StringBuilder stringBuilder)
    {
        public void AppendDbInsert(string tableName, ICollection<string> fieldNames, IDbContext context)
        {
            if (fieldNames == null)
            {
                throw new ArgumentNullException(nameof(fieldNames));
            }

            var lastValueKey = fieldNames.Last();

            stringBuilder.Append($"INSERT INTO {context.QuoteIdentifier(tableName)} (");
            foreach (var field in fieldNames)
            {
                stringBuilder.Append(context.QuoteIdentifier(field));
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
            ICollection<string> fieldNames, int id, IDbContext context)
        {
            if (fieldNames == null)
            {
                throw new ArgumentNullException(nameof(fieldNames));
            }

            var lastValueKey = fieldNames.Last();

            stringBuilder.Append($"UPDATE {context.QuoteIdentifier(tableName)} SET ");
            foreach (var field in fieldNames)
            {
                stringBuilder.Append($"{context.QuoteIdentifier(field)} = @{field}");
                if (field != lastValueKey)
                {
                    stringBuilder.Append(",");
                }
            }
            stringBuilder.Append($" WHERE {context.QuoteIdentifier("Id")} = {id};");
        }

        public void AppendIdentitySelect(IDbContext context)
        {
            stringBuilder.Append(context.LastInsertIdSql);
        }
    }
}
