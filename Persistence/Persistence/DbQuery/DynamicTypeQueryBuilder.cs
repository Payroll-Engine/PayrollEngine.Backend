﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace PayrollEngine.Persistence.DbQuery;

/// <summary>
/// Build type query with dynamic columns
/// </summary>
internal sealed class DynamicTypeQueryBuilder<T> : TypeQueryBuilder<T>
{
    /// <summary>
    /// The container name
    /// </summary>
    private List<string> ContainerNames { get; }

    /// <summary>
    /// Dynamic query columns
    /// </summary>
    internal List<string> DynamicColumns { get; } = [];

    internal DynamicTypeQueryBuilder(IEnumerable<string> containerNames = null)
    {
        ContainerNames = containerNames?.Distinct().ToList();
    }

    /// <inheritdoc />
    protected override IQueryContext QueryContext => this;

    /// <inheritdoc />
    protected override Type GetColumnType(string name)
    {
        // type column
        var type = base.GetColumnType(name);
        if (type != null)
        {
            return type;
        }

        // dynamic column
        EnsureDynamicColumn(name);
        return null;
    }

    /// <inheritdoc />
    /// <remarks>Do not call the base class method</remarks>
    protected override string ValidateColumn(string name)
    {
        // type column
        var validName = FindTypeColumn(name);
        if (validName != null)
        {
            return validName;
        }

        // dynamic column
        if (ContainerNames.Any())
        {
            var valid = false;
            foreach (var containerName in ContainerNames)
            {
                // container column
                if (string.Equals(name, containerName, StringComparison.InvariantCultureIgnoreCase))
                {
                    throw new QueryException($"Invalid query field name {name}, conflicts with {containerName}");
                }
                if (name.StartsWith(containerName))
                {
                    valid = true;
                    break;
                }
            }
            if (!valid)
            {
                throw new QueryException($"Invalid query field {name}, should start with {string.Join(", ", ContainerNames)}");
            }
        }
        EnsureDynamicColumn(name);
        return name;
    }

    private void EnsureDynamicColumn(string name)
    {
        if (!DynamicColumns.Contains(name))
        {
            DynamicColumns.Add(name);
        }
    }
}