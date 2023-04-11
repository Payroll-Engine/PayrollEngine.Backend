/* ReportEndFunction */
// ReSharper disable RedundantUsingDirective
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text.Json;

// ReSharper restore RedundantUsingDirective

namespace Ason.Payroll.Client.Scripting.Function;

/// <summary>Report end function</summary>
// ReSharper disable once PartialTypeWithSinglePart
public partial class ReportEndFunction : ReportFunction
{
    /// <summary>Initializes a new instance with the function runtime</summary>
    /// <param name="runtime">The runtime</param>
    public ReportEndFunction(object runtime) :
        base(runtime)
    {
        DataSet = Runtime.DataSet;
    }

    /// <summary>New function instance without runtime (scripting development)</summary>
    /// <param name="sourceFileName">The name of the source file</param>
    public ReportEndFunction(string sourceFileName) :
        base(sourceFileName)
    {
    }

    /// <summary>The report data set</summary>
    public DataSet DataSet { get; }

    #region Tables

    /// <summary>The report data tables</summary>
    public DataTableCollection Tables => DataSet.Tables;

    /// <summary>Determines whether the table contains the specified table</summary>
    /// <param name="tableName">Name of the table</param>
    /// <returns>True for an existing table</returns>
    public bool ContainsTable(string tableName) => Tables.Contains(tableName);

    /// <summary>Add a table to the data set</summary>
    /// <param name="tableName">Name of the table</param>
    public DataTable AddTable(string tableName) => Tables.Add(tableName);

    /// <summary>Add a table to the data set</summary>
    /// <param name="table">The table</param>
    public void AddTable(DataTable table) => Tables.Add(table);

    /// <summary>Remove a table from the data set</summary>
    /// <param name="tableName">Name of the table</param>
    public void RemoveTable(string tableName) => Tables.Remove(tableName);

    /// <summary>Remove multiple tables from the data set</summary>
    /// <param name="tableNames">Name of the tables</param>
    public void RemoveTables(params string[] tableNames)
    {
        foreach (var tableName in tableNames)
        {
            RemoveTable(tableName);
        }
    }

    /// <summary>Remove the table primary key</summary>
    /// <param name="tableName">Name of the table</param>
    public void RemovePrimaryKey(string tableName) =>
        RemovePrimaryKey(Tables[tableName]);

    /// <summary>Remove the table primary key</summary>
    /// <param name="table">The table</param>
    public void RemovePrimaryKey(DataTable table)
    {
        if (table != null)
        {
            table.PrimaryKey = new DataColumn[] { };
        }
    }
    /// <summary>Computes the given expression on the current rows that pass the filter criteria</summary>
    /// <param name="tableName">Name of the table</param>
    /// <param name="expression">The expression to compute</param>
    /// <param name="filter">The filter to limit the rows that evaluate in the expression</param>
    /// <returns>An Object, set to the result of the computation. If the expression evaluates to null, the return value will be Value</returns>
    public object Compute(string tableName, string expression, string filter = null) =>
        Tables[tableName]?.Compute(expression, filter);

    /// <summary>Computes the given expression on the current rows that pass the filter criteria</summary>
    /// <param name="tableName">Name of the table</param>
    /// <param name="expression">The expression to compute</param>
    /// <param name="filter">The filter to limit the rows that evaluate in the expression</param>
    /// <returns>An Object, set to the result of the computation. If the expression evaluates to null, the return value will be Value</returns>
    public T Compute<T>(string tableName, string expression, string filter = null)
    {
        var table = Tables[tableName];
        if (table == null)
        {
            return default;
        }
        return (T)Compute(expression, filter);
    }

    /// <summary>Execute a query on the Api web method and add the table to the set</summary>
    /// <param name="tableName">Target table name</param>
    /// <param name="methodName">The query name</param>
    /// <param name="parameters">The method parameters</param>
    /// <returns>New data table, null on empty collection</returns>
    public DataTable ExecuteResultQuery(string tableName, string methodName, Dictionary<string, string> parameters = null) =>
        ExecuteResultQuery(tableName, methodName, Language, parameters);

    /// <summary>Execute a query on the Api web method and add the table to the set</summary>
    /// <param name="tableName">Target table name</param>
    /// <param name="methodName">The query name</param>
    /// <param name="language">The content language</param>
    /// <param name="parameters">The method parameters</param>
    /// <returns>New data table, null on empty collection</returns>
    public DataTable ExecuteResultQuery(string tableName, string methodName, Language language, Dictionary<string, string> parameters = null) =>
        Runtime.ExecuteResultQuery(tableName, methodName, (int)language, parameters);

    /// <summary>Execute a query on the Api web method and merge the table to the set</summary>
    /// <param name="tableName">Target table name</param>
    /// <param name="methodName">The query name</param>
    /// <param name="mergeColumn">The column used to merge (primary key column)</param>
    /// <param name="parameters">The method parameters</param>
    /// <param name="schemaChange">Action to take when the required data column is missing</param>
    /// <returns>New or expanded data table</returns>
    public DataTable ExecuteResultQuery(string tableName, string methodName, string mergeColumn,
        Dictionary<string, string> parameters = null, DataMergeSchemaChange schemaChange = DataMergeSchemaChange.Add) =>
        ExecuteResultQuery(tableName, methodName, Language, mergeColumn, parameters, schemaChange);

    /// <summary>Execute a query on the Api web method and merge the table to the set</summary>
    /// <param name="tableName">Target table name</param>
    /// <param name="methodName">The query name</param>
    /// <param name="language">The content language</param>
    /// <param name="mergeColumn">The column used to merge (primary key column)</param>
    /// <param name="parameters">The method parameters</param>
    /// <param name="schemaChange">Action to take when the required data column is missing</param>
    /// <returns>New or expanded data table</returns>
    public DataTable ExecuteResultQuery(string tableName, string methodName, Language language, string mergeColumn,
        Dictionary<string, string> parameters = null, DataMergeSchemaChange schemaChange = DataMergeSchemaChange.Add) =>
        Runtime.ExecuteResultQuery(tableName, methodName, (int)language, mergeColumn, parameters, (int)schemaChange);

    /// <summary>Execute a value query on the Api web method and add the table to the set</summary>
    /// <param name="tableName">Target table name</param>
    /// <param name="attributeName">Name of the attribute</param>
    /// <param name="methodName">The query name</param>
    /// <param name="parameters">The method parameters</param>
    /// <returns>Resulting data table, existing will be removed</returns>
    public T ExecuteValueQuery<T>(string tableName, string attributeName, string methodName,
        Dictionary<string, string> parameters)
    {
        if (string.IsNullOrWhiteSpace(attributeName))
        {
            throw new ArgumentException(nameof(attributeName));
        }
        var result = ExecuteResultQuery(tableName, methodName, parameters);
        if (result == null || result.Rows.Count != 1)
        {
            throw new ScriptException($"Single result expected on table {tableName} and attribute {attributeName}");
        }
        return GetValue<T>(result.Rows[0], attributeName);
    }

    /// <summary>Execute a value query on the Api web method and add the table to the set</summary>
    /// <param name="tableName">Target table name</param>
    /// <param name="attributeName">Name of the attribute</param>
    /// <param name="methodName">The query name</param>
    /// <param name="parameters">The method parameters</param>
    /// <param name="defaultValue">The method parameters</param>
    /// <returns>Resulting data table, existing will be removed</returns>
    public T ExecuteValueQuery<T>(string tableName, string attributeName, string methodName,
        Dictionary<string, string> parameters, T defaultValue)
    {
        if (string.IsNullOrWhiteSpace(attributeName))
        {
            throw new ArgumentException(nameof(attributeName));
        }
        var result = ExecuteResultQuery(tableName, methodName, parameters);
        if (result == null || result.Rows.Count != 1)
        {
            return defaultValue;
        }
        return GetValue(result.Rows[0], attributeName, defaultValue);
    }

    #endregion

    #region Columns

    /// <summary>Get table columns</summary>
    /// <param name="tableName">Name of the table</param>
    /// <returns>A column collection</returns>
    public DataColumnCollection Columns(string tableName) => Tables[tableName]?.Columns;

    /// <summary>Add table column</summary>
    /// <param name="tableName">Name of the table</param>
    /// <param name="columnName">Name of the column</param>
    public void AddColumn<T>(string tableName, string columnName) =>
        Tables[tableName]?.Columns.Add(columnName, typeof(T));

    /// <summary>Add table column</summary>
    /// <param name="tableName">Name of the table</param>
    /// <param name="columnName">Name of the column</param>
    /// <param name="expression">The compute expression</param>
    public void AddColumn<T>(string tableName, string columnName, string expression) =>
        Tables[tableName]?.Columns.Add(columnName, typeof(T), expression);

    /// <summary>Rename table column</summary>
    /// <param name="tableName">Name of the table</param>
    /// <param name="oldColumnName">Existing name of the column</param>
    /// <param name="newColumnName">Existing name of the column</param>
    /// <returns>The column name</returns>
    public string RenameColumn(string tableName, string oldColumnName, string newColumnName) =>
        RenameColumn(Tables[tableName], oldColumnName, newColumnName);

    /// <summary>Rename table column</summary>
    /// <param name="table">The table</param>
    /// <param name="oldColumnName">Existing name of the column</param>
    /// <param name="newColumnName">Existing name of the column</param>
    /// <returns>The column name</returns>
    public string RenameColumn(DataTable table, string oldColumnName, string newColumnName)
    {
        if (table == null)
        {
            return null;
        }
        var column = table.Columns[oldColumnName];
        if (column == null)
        {
            return null;
        }
        column.ColumnName = newColumnName;
        return newColumnName;
    }

    /// <summary>Remove table column</summary>
    /// <param name="tableName">Name of the table</param>
    /// <param name="columnName">Name of the column</param>
    public void RemoveColumn(string tableName, string columnName) =>
        Tables[tableName]?.Columns.Remove(columnName);

    /// <summary>Set the table primary key column</summary>
    /// <param name="tableName">Name of the table</param>
    /// <param name="columnName">Name of the column</param>
    public void SetPrimaryKey(string tableName, string columnName) =>
        SetPrimaryKey(Tables[tableName], columnName);

    /// <summary>Set the table primary key column</summary>
    /// <param name="table">The table</param>
    /// <param name="columnName">Name of the column</param>
    public void SetPrimaryKey(DataTable table, string columnName)
    {
        if (table != null)
        {
            var column = table.Columns[columnName];
            table.PrimaryKey = new[] { column };
        }
    }

    #endregion

    #region Rows

    /// <summary>Get table rows</summary>
    /// <param name="tableName">Name of the table</param>
    /// <returns>A row collection</returns>
    public EnumerableRowCollection Rows(string tableName) => Rows(Tables[tableName]);

    /// <summary>Get table rows</summary>
    /// <param name="table">The table</param>
    /// <returns>A row collection</returns>
    public EnumerableRowCollection Rows(DataTable table) => table.AsEnumerable();

    /// <summary>Select table rows by filter</summary>
    /// <param name="tableName">Name of the table</param>
    /// <param name="filterExpression">The filter matching the rows to delete</param>
    public IEnumerable<DataRow> SelectRows(string tableName, string filterExpression) =>
        SelectRows(Tables[tableName], filterExpression);

    /// <summary>Select table rows by filter</summary>
    /// <param name="table">The table</param>
    /// <param name="filterExpression">The filter matching the rows to delete</param>
    public IEnumerable<DataRow> SelectRows(DataTable table, string filterExpression) =>
        table.Select(filterExpression);

    /// <summary>Delete table rows by filter</summary>
    /// <param name="tableName">Name of the table</param>
    /// <param name="filterExpression">The filter matching the rows to delete</param>
    public int DeleteRows(string tableName, string filterExpression) =>
        DeleteRows(Tables[tableName], filterExpression);

    /// <summary>Delete table rows by filter</summary>
    /// <param name="table">The table</param>
    /// <param name="filterExpression">The filter matching the rows to delete</param>
    public int DeleteRows(DataTable table, string filterExpression)
    {
        var deleteCount = 0;
        var deleteRows = SelectRows(table, filterExpression);
        foreach (var deleteRow in deleteRows)
        {
            deleteRow.Delete();
            deleteCount++;
        }
        if (deleteCount > 0)
        {
            table.AcceptChanges();
        }
        return deleteCount;
    }

    #endregion

    #region Relations

    /// <summary>Add relation between two table</summary>
    /// <param name="relationName">The relation name</param>
    /// <param name="parentTableName">The relation parent table name</param>
    /// <param name="childTableName">The relation child table name</param>
    /// <param name="childColumnName">The relation child table column name</param>
    public DataRelation AddRelation(string relationName, string parentTableName, string childTableName,
        string childColumnName) =>
        AddRelation(relationName, parentTableName, "Id", childTableName, childColumnName);

    /// <summary>Add relation between two table</summary>
    /// <param name="relationName">The relation name</param>
    /// <param name="parentTableName">The relation parent table name</param>
    /// <param name="parentColumnName">The relation parent table column name</param>
    /// <param name="childTableName">The relation child table name</param>
    /// <param name="childColumnName">The relation child table column name</param>
    public DataRelation AddRelation(string relationName, string parentTableName, string parentColumnName, string childTableName,
        string childColumnName)
    {
        if (string.IsNullOrWhiteSpace(relationName))
        {
            throw new ArgumentException(nameof(relationName));
        }

        var parentTable = DataSet.Tables[parentTableName];
        if (parentTable == null)
        {
            throw new ScriptException($"Missing relation parent table {parentTableName}");
        }
        var parentColumn = parentTable.Columns[parentColumnName];
        if (parentColumn == null)
        {
            throw new ScriptException($"Missing relation parent column {parentTableName}.{parentColumnName}");
        }
        var childTable = DataSet.Tables[childTableName];
        if (childTable == null)
        {
            throw new ScriptException($"Missing relation child table {childTableName}");
        }
        var childColumn = childTable.Columns[childColumnName];
        if (childColumn == null)
        {
            throw new ScriptException($"Missing relation parent column {childTableName}.{childColumnName}");
        }
        return DataSet.Relations.Add(relationName, parentColumn, childColumn);
    }

    #endregion

    /// <exclude />
    public object End()
    {
                    var employees = Tables["Kumulativjournal"];
            var regulationId = GetParameter("RegulationId");
            var checkStatusUntilDate = GetParameter("CheckStatusUntilDate");
            DateTime statusUntilDate;
            DataTable companyCaseValues;
            var wageTypeDictionary = new Dictionary<string, string> { { string.Empty, string.Empty } };
            var wageTable = AddTable("Wage");
            var collectorTable = AddTable("Collectors");
            var divisionIdentifier = "";
            var languageSelect = GetParameter("LanguageSelect");
            var employeeLanguage = GetParameter<bool>("EmployeeLanguage");

            #region Period
            if (checkStatusUntilDate == "today")
            {
                statusUntilDate = Date.Now;
            }
            else
            {
                statusUntilDate = Convert.ToDateTime(JsonSerializer.Deserialize<string>(GetParameter("CheckStatusUntilDate")));
            }
            var today = Date.Today;
            var kumulativDate = today.Year + "-" + today.Month + "-" + today.Day;
            var kumulativYear = statusUntilDate.Year;
            #endregion
            if (employees == null || employees.Rows.Count == 0)
            {

                #region DivisionId
                var queryParameter = "Id eq '" + GetParameter("PayrollId") + "'";
                var payrolls = ExecuteQuery("Payrolls", "QueryPayrolls", new Dictionary<string, string> { { "TenantId", TenantId.ToString() }, { "Filter", Convert.ToString(queryParameter) } });
                //var PayrollId = GetParameter<int>("Payrolls");

                var queryParameterDivisions = "Name eq '" + payrolls.Rows[0]["Name"] + "'";
                var divisions = ExecuteQuery("Divisions", "QueryDivisions", new Dictionary<string, string> { { "TenantId", TenantId.ToString() }, { "Filter", Convert.ToString(queryParameterDivisions) } });

                divisionIdentifier = Convert.ToString(divisions.Rows[0]["id"]);
                #endregion


                employees = ExecuteResultQuery("Kumulativjournal", "QueryEmployees",
                new Dictionary<string, string>{
                    { "TenantId", TenantId.ToString() },
                    { "DivisionId", divisionIdentifier }
                });
            }
            #region CompanyCaseValues
            //Company case values
            companyCaseValues = ExecuteResultQuery("CompanyCaseValues", "QueryCompanyCaseValues",
                new Dictionary<string, string>{
                    { "TenantId", TenantId.ToString() },
                    { "Filter","start le '"+ statusUntilDate +"'" },
                    { "DivisionId", divisionIdentifier }
                }
            );
            for (var i = companyCaseValues.Rows.Count - 1; i >= 0; i--)
            {
                DataRow companyDataRow = companyCaseValues.Rows[i];
                DataColumnCollection columns = companyCaseValues.Columns;
                var caseFieldName = GetValue<string>(companyDataRow, "CaseFieldName");
                var value = GetValue<string>(companyDataRow, "Value");
                if (!columns.Contains(caseFieldName))
                {
                    companyCaseValues.Columns.Add(caseFieldName, typeof(string));
                    companyCaseValues.Rows[0][caseFieldName] = value;
                }
                if (i > 0)
                {
                    companyDataRow.Delete();
                }
            }
            companyCaseValues.AcceptChanges();

            #endregion

            if (employees == null || employees.Rows.Count == 0)
            {
                // no employees available
                return null;
            }

            // results for each employee
            foreach (DataRow employee in employees.Rows)
            {
                // employee id
                if (employee["Id"] is not int employeeId)
                {
                    throw new ScriptException("Missing employee id");
                }

                // employee case values
                var thisEmployeeCaseValues = ExecuteResultQuery("CaseValues", "QueryEmployeeCaseValues",
                    new Dictionary<string, string>{
                        { "TenantId", TenantId.ToString() },
                        { "EmployeeId", employeeId.ToString() },
                        { "Filter","start le '"+ statusUntilDate +"'" },
                        { "OrderBy", "start desc" }
                    }
                );
                //string thisEmployeeLanguage = null;
                // employee case values
                if (thisEmployeeCaseValues.Rows.Count > 0)
                {
                    string[] crateColumns =
                    {
                        "KumulativYear", "KumulativDate", "CH.Swissdec.CompanyAddressName", "CH.Swissdec.CompanyAddressStreet",
                        "CH.Swissdec.CompanyAddressCity", "CH.Swissdec.CompanyAddressZipCode"
                    };
                    for (var i = thisEmployeeCaseValues.Rows.Count - 1; i >= 0; i--)
                    {
                        // get Row Data from Table
                        DataRow thisEmployeeCaseValuesRow = thisEmployeeCaseValues.Rows[i];
                        DataColumnCollection columns = employees.Columns;
                        var caseFieldName = GetValue<string>(thisEmployeeCaseValuesRow, "CaseFieldName");
                        var value = GetValue<string>(thisEmployeeCaseValuesRow, "Value");
                        var caseStart = Convert.ToDateTime(GetValue<string>(thisEmployeeCaseValuesRow, "start"));

                        if (caseFieldName == "CH.Swissdec.EmployeeLanguage")
                        {
                            //thisEmployeeLanguage = value;
                        }

                        foreach (var newColumn in crateColumns)
                        {
                            if (!columns.Contains(newColumn))
                            {
                                employees.Columns.Add(newColumn, typeof(string));
                            }
                        }
                        employee["KumulativYear"] = kumulativYear;
                        employee["KumulativDate"] = kumulativDate;
                        employee["CH.Swissdec.CompanyAddressName"] = companyCaseValues.Rows[0]["CH.Swissdec.CompanyAddressName"];
                        employee["CH.Swissdec.CompanyAddressStreet"] = companyCaseValues.Rows[0]["CH.Swissdec.CompanyAddressStreet"];
                        employee["CH.Swissdec.CompanyAddressZipCode"] = companyCaseValues.Rows[0]["CH.Swissdec.CompanyAddressZipCode"];
                        employee["CH.Swissdec.CompanyAddressCity"] = companyCaseValues.Rows[0]["CH.Swissdec.CompanyAddressCity"];

                        if (caseStart <= statusUntilDate)
                        {
                            if (!columns.Contains(caseFieldName))
                            {
                                employees.Columns.Add(caseFieldName, typeof(string));
                            }
                        }
                        switch (caseFieldName)
                        {
                            default:
                                employee[caseFieldName] = value;
                                break;
                        }
                    }
                    /*
                    // query wage types
                    var wageType = ExecuteResultQuery("WageType", "QueryWageTypes",
                        new Dictionary<string, string>
                        {
                            { "TenantId", TenantId.ToString() },
                            { "RegulationId", regulationId }
                        }
                    );
                    foreach (DataRow row in wageType.Rows)
                    {
                        #region Language Filter
                        // get 
                        var wageTypeNumber = GetValue<string>(row, "WageTypeNumber");
                        var nameLocalization =
                            JsonSerializer.Deserialize<Dictionary<string, string>>(GetValue<string>(row,
                                "NameLocalizations"));
                        var name = GetValue<string>(row, "Name");

                        //if (nameLocalization == null)
                        //{
                            wageTypeDictionary.Add(wageTypeNumber, name);
                            //continue;
                        //}

                        
                        if (employeeLanguage != true)
                        {
                            
                            if (nameLocalization.ContainsKey(languageSelect))
                            {
                                wageTypeDictionary.Add(wageTypeNumber, nameLocalization[languageSelect]);
                            }
                        }
                        /*
                        else
                        {
                            
                            if (thisEmployeeLanguage != null && nameLocalization.ContainsKey(thisEmployeeLanguage) && nameLocalization != null)
                            {
                                wageTypeDictionary.Add(wNumber, nameLocalization[thisEmployeeLanguage]);
                            }
                            
                            if (thisEmployeeLanguage == null && nameLocalization.ContainsKey(Language.LanguageCode()) && nameLocalization != null)
                            {
                                wageTypeDictionary.Add(wNumber, nameLocalization[Language.LanguageCode()]);
                            }
                            
                        }
                        */
                    //}
                    //RemoveTables("WageType");

                    //#endregion

                }

                // temporary tables for employee
                DataTable employeeWageTypeResultsFinal = AddTable("EmployeeWageTypeResults");
                //DataTable employeeCollectorResults = AddTable("EmployeeCollectorResults");
                // results for each month in year
                for (var month = 1; month <= 12; month++)
                {
                    // set parameter period start
                    var periodStart = Date.MonthStart(statusUntilDate.Year, month);
                    // query payroll results
                    /*
                    var payrollResultId = ExecuteValueQuery<int>("PayrollResult", "id", "QueryPayrollResults",
                        new Dictionary<string, string>
                        {
                            {"TenantId", TenantId.ToString()},
                            {"Filter", "employeeId eq '" + employeeId + "' and periodStart eq '"+periodStart+"'"}
                        }
                    );
                    */
                    // result request parameters (currently the same for wage types and collectors)
                    var queryParameters = new Dictionary<string, string>
                        {
                            {"TenantId", TenantId.ToString()},
                            {"PayrollResultId", (34 + month).ToString()},
                            {"Filter", "start eq '"+periodStart+"'"}
                        };

                    // query wage type results (cleanup previous query data)
                    var employeeWageTypeResults = ExecuteResultQuery("EmployeeWageTypes",
                        "QueryWageTypeResults", queryParameters);
                    foreach (DataRow employeeWageTypeRow in employeeWageTypeResults.Rows)
                    {
                        var wageTypeNumber = employeeWageTypeRow["WageTypeNumber"].ToString();
                        //employeeWageTypeRow["WageTypeName"]          = wageTypeDictionary[wageTypeNumber];

                        // rename value column to month column
                        RenameColumn(employeeWageTypeResults, "Value", $"M{month}");
                        // set merge column
                        SetPrimaryKey(employeeWageTypeResults, "WageTypeNumber");
                        // merge results by wage type number
                        employeeWageTypeResultsFinal.Merge(employeeWageTypeResults, false, MissingSchemaAction.AddWithKey);
                    }
                }
                RemovePrimaryKey(employeeWageTypeResultsFinal);
                wageTable.Merge(employeeWageTypeResultsFinal);

                RemoveTables("EmployeeWageTypeResults", "EmployeeWageTypes");
            }


            // total value column
            //AddColumn<decimal>(wageTable.TableName, "Total", "M1 + M2 + M3 + M4 + M5 + M6 + M7 + M8 + M9 + M10 + M11 + M12");
            // remove empty rows
            //DeleteRows(wageTable, "Total = 0");
            // total value column
            //AddColumn<decimal>(collectorTable.TableName, "Total", "M1 + M2 + M3 + M4 + M5 + M6 + M7 + M8 + M9 + M10 + M11 + M12");
            // remove empty rows
            //DeleteRows(collectorTable, "Total = 0");

            if (wageTable.Rows.Count > 0)
            {
                // report relations: results are related toward employee
                AddColumn<decimal>(wageTable.TableName, "Total", "M1 + M2 + M3 + M4 + M5 + M6 + M7 + M8 + M9 + M10 + M11 + M12");
                DeleteRows(wageTable, "Total = 0");
                AddRelation("Wage", employees.TableName, wageTable.TableName, "EmployeeId");
            }

            /*
            if(collectorTable.Rows.Count > 0)
            {
                // report relations: results are related toward employee
                AddColumn<decimal>(collectorTable.TableName, "Total", "M1 + M2 + M3 + M4 + M5 + M6 + M7 + M8 + M9 + M10 + M11 + M12");
                DeleteRows(collectorTable, "Total = 0");
                AddRelation("Collectors", employees.TableName, collectorTable.TableName, "EmployeeId");
            }
            */
            return null;
;
        // compiler will optimize this out if the code provides a return
#pragma warning disable 162
        return default;
#pragma warning restore 162
    }
}