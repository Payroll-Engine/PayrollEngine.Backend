using System;
using System.Collections.Generic;
using System.Data;
using System.Runtime.CompilerServices;
using PayrollEngine.Client.Scripting;
using PayrollEngine.Client.Scripting.Function;
using PayrollEngine.Client.Scripting.Runtime;
using PayrollEngine.Domain.Model;
using Task = System.Threading.Tasks.Task;

namespace PayrollEngine.Domain.Scripting.Runtime;

/// <summary>
/// Runtime for the report end function
/// </summary>
public class ReportEndRuntime : ReportRuntime, IReportEndRuntime
{
    /// <inheritdoc />
    public DataSet DataSet { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ReportEndRuntime"/> class.
    /// </summary>
    /// <param name="settings">The runtime settings</param>
    /// <param name="dataSet">The data set</param>
    public ReportEndRuntime(ReportRuntimeSettings settings, DataSet dataSet) :
        base(settings)
    {
        DataSet = dataSet ?? throw new ArgumentNullException(nameof(dataSet));
    }

    /// <summary>The log owner type</summary>
    protected override string LogOwnerType => nameof(ReportEndFunction);

    /// <inheritdoc />
    public DataTable ExecuteQuery(string tableName, string methodName,
        int language, Dictionary<string, string> parameters, bool resultQuery)
    {
        var table = base.ExecuteQuery(tableName, methodName, language, parameters);
        if (resultQuery && table != null)
        {
            // result table
            if (DataSet.Tables.Contains(tableName))
            {
                // remove previous table
                DataSet.Tables.Remove(tableName);
            }
            // new table
            DataSet.Tables.Add(table);
        }
        return table;
    }

    /// <inheritdoc />
    public DataTable ExecuteMergeQuery(string tableName, string methodName, int language,
        string mergeColumn, Dictionary<string, string> parameters, int schemaChange)
    {
        if (string.IsNullOrWhiteSpace(tableName))
        {
            throw new ArgumentException(nameof(tableName));
        }
        if (string.IsNullOrWhiteSpace(methodName))
        {
            throw new ArgumentException(nameof(methodName));
        }
        if (!Enum.IsDefined((Language)language))
        {
            throw new PayrollException($"Invalid language code: {language}");
        }
        if (!Enum.IsDefined((DataMergeSchemaChange)schemaChange))
        {
            throw new PayrollException($"Invalid schema change: {schemaChange}");
        }

        try
        {
            // report query
            var resultTable = QueryService.ExecuteQuery(TenantId, methodName, (Language)language, parameters, ControllerContext);
            if (resultTable == null)
            {
                if (!DataSet.Tables.Contains(tableName))
                {
                    DataSet.Tables.Add(tableName);
                }
                return DataSet.Tables[tableName];
            }
            resultTable.TableName = tableName;
            if (resultTable.Rows.Count > 0 && !string.IsNullOrWhiteSpace(mergeColumn))
            {
                if (!resultTable.Columns.Contains(mergeColumn))
                {
                    throw new ScriptException($"Unknown merge column {mergeColumn}");
                }
                resultTable.PrimaryKey = new[] { resultTable.Columns[mergeColumn] };
            }
            resultTable.AcceptChanges();

            // result table
            var dataTable = DataSet.Tables[tableName];
            if (dataTable == null)
            {
                // new table
                DataSet.Tables.Add(resultTable);
                dataTable = DataSet.Tables[tableName];
            }
            if (dataTable != null && resultTable.Rows.Count > 0)
            {
                // merge with existing table with schema change support
                dataTable.Merge(resultTable, false, (MissingSchemaAction)schemaChange);
                dataTable.AcceptChanges();
            }

            return resultTable;
        }
        catch (Exception exception)
        {
            throw new PayrollException(exception.GetBaseMessage(), exception);
        }
    }

    /// <summary>
    /// Execute the report end script
    /// </summary>
    /// <param name="report">The report</param>
    /// <returns>True if the employee is available</returns>
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal void ExecuteEndScript(ReportSet report)
    {
        try
        {
            var task = Task.Factory.StartNew(() =>
            {
                // create script
                using var script = CreateScript(typeof(ReportEndFunction), report);
                // call the script function
                script.End();
            });
            task.Wait(Timeout);
        }
        catch (ScriptException)
        {
            throw;
        }
        catch (Exception exception)
        {
            throw new ScriptException($"End script error in report {report.Name}: {exception.GetBaseMessage()}", exception);
        }
    }
}