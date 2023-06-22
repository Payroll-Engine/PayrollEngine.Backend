SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

IF EXISTS (
    SELECT *
    FROM sysobjects
    WHERE id = object_id(N'[dbo].[GetPayrollResultValues]')
      AND OBJECTPROPERTY(id, N'IsProcedure') = 1
    )
BEGIN
  DROP PROCEDURE dbo.[GetPayrollResultValues]
END
GO

-- =============================================
-- Payroll result values
-- =============================================
CREATE PROCEDURE dbo.[GetPayrollResultValues]
  -- the parent id (unsuses, required from case value query)
  @parentId AS INT,
  -- the query sql
  @sql AS NVARCHAR(MAX),
  -- the employee id
  @employeeId AS INT = NULL,
  -- the attribute names: JSON array of VARCHAR(128)
  @attributes AS NVARCHAR(MAX) = NULL
AS
BEGIN
  -- SET NOCOUNT ON added to prevent extra result sets from
  -- interfering with SELECT statements
  SET NOCOUNT ON;

  -- pivot select
  DECLARE @pivotSql AS NVARCHAR(MAX);
  DECLARE @attributeSql AS NVARCHAR(MAX);

  -- pivot sql begin part
  -- pivot sql part 1
  SET @pivotSql = N'SELECT *
    INTO ##PayrollResultPivot
    FROM (
'
  SET @pivotSql = @pivotSql + 
    N'
  SELECT
  -- tenant
  [PayrollResult].[TenantId],
  -- payroll result
  [PayrollResult].[Id] AS [PayrollResultId],
  [PayrollResult].[Created],
  -- payroll value
  [PayrollValue].[ResultKind],
  [PayrollValue].[ResultId],
  [PayrollValue].[ResultParentId],
  [PayrollValue].[ResultNumber],
  [PayrollValue].[KindName],
  [PayrollValue].[ResultCreated],
  [PayrollValue].[ResultStart],
  [PayrollValue].[ResultEnd],
  [PayrollValue].[ResultType],
  [PayrollValue].[ResultValue],
  [PayrollValue].[ResultNumericValue],
  [PayrollValue].[ResultTags],
  [PayrollValue].[Attributes],
  -- payrun job
  [PayrunJob].[Id] AS [JobId],
  [PayrunJob].[Name] AS [JobName],
  [PayrunJob].[CreatedReason] AS [JobReason],
  [PayrunJob].[Forecast],
  [PayrunJob].[JobStatus],
  [PayrunJob].[CycleName],
  [PayrunJob].[PeriodName],
  [PayrunJob].[PeriodStart],
  [PayrunJob].[PeriodEnd],
  -- payrun
  [Payrun].[Id] AS [PayrunId],
  [Payrun].[Name] AS [PayrunName],
  -- payroll
  [Payroll].[Id] AS [PayrollId],
  [Payroll].[Name] AS [PayrollName],
  -- division
  [Division].[Id] AS [DivisionId],
  [Division].[Name] AS [DivisionName],
  [Division].[Culture],
  -- user
  [User].[Id] AS [UserId],
  [User].[Identifier] AS [UserIdentifier],
  -- employee
  [Employee].[Id] AS [EmployeeId],
  [Employee].[Identifier] AS [EmployeeIdentifier]'
  SET @attributeSql = dbo.GetAttributeNames(@attributes);
  SET @pivotSql = @pivotSql + @attributeSql;
  SET @pivotSql = @pivotSql + '
FROM (
'
  -- pivot sql collector result
  SET @pivotSql = @pivotSql + N'
    -- collector results
  SELECT 10 AS [ResultKind], -- 10: collector
    [PayrollResultId],
    [CollectorResult].[Id] AS [ResultId],
    [PayrollResultId] AS [ResultParentId],
    [CollectorName] AS [KindName],
    0 AS [ResultNumber], -- no custom result
    [CollectorResult].[Created] AS [ResultCreated],
    [CollectorResult].[Start] AS [ResultStart],
    [CollectorResult].[End] AS [ResultEnd],
    [CollectorResult].[Tags] AS [ResultTags],
    [CollectorResult].[Attributes],
    [CollectorResult].[ValueType] AS [ResultType],
    LTRIM([CollectorResult].[Value]) AS [ResultValue],
    [CollectorResult].[Value] AS [ResultNumericValue]';
  SET @attributeSql = dbo.BuildAttributeQuery('[CollectorResult].[Attributes]', @attributes);
  SET @pivotSql = @pivotSql + @attributeSql;
  SET @pivotSql = @pivotSql + N'
    FROM [dbo].[CollectorResult]
  
    UNION ALL

'
  -- pivot sql collector custom result
  SET @pivotSql = @pivotSql + N'
    -- collector custom results
  SELECT 11 AS [ResultKind], -- 11: collector custom
    [PayrollResultId],
    [CollectorCustomResult].[Id] AS [ResultId],
    [CollectorResult].[Id] AS [ResultParentId],
    [CollectorCustomResult].[Source] AS [KindName],
    0 AS [ResultNumber], -- no custom result
    [CollectorCustomResult].[Created] AS [ResultCreated],
    [CollectorCustomResult].[Start] AS [ResultStart],
    [CollectorCustomResult].[End] AS [ResultEnd],
    [CollectorCustomResult].[Tags] AS [ResultTags],
    [CollectorCustomResult].[Attributes],
    [CollectorCustomResult].[ValueType] AS [ResultType],
    LTRIM([CollectorCustomResult].[Value]) AS [ResultValue],
    [CollectorCustomResult].[Value] AS [ResultNumericValue]';
  SET @attributeSql = dbo.BuildAttributeQuery('[CollectorCustomResult].[Attributes]', @attributes);
  SET @pivotSql = @pivotSql + @attributeSql;
  SET @pivotSql = @pivotSql + N'
    FROM [dbo].[CollectorResult]
  INNER JOIN [dbo].[CollectorCustomResult]
    ON [CollectorResult].[Id] = [CollectorCustomResult].[CollectorResultId]
  
  UNION ALL

'
  -- pivot sql wage type result
  SET @pivotSql = @pivotSql + N'
    -- wage type results
  SELECT 20 AS [ResultKind], -- 20: wage type
    [PayrollResultId],
    [WageTypeResult].[Id] AS [ResultId],
    [PayrollResultId] AS [ResultParentId],
    [WageTypeName] AS [KindName],
    [WageTypeNumber] AS [ResultNumber],
    [WageTypeResult].[Created] AS [ResultCreated],
    [WageTypeResult].[Start] AS [ResultStart],
    [WageTypeResult].[End] AS [ResultEnd],
    [WageTypeResult].[Tags] AS [ResultTags],
    [WageTypeResult].[Attributes],
    [WageTypeResult].[ValueType] AS [ResultType],
    LTRIM([WageTypeResult].[Value]) AS [ResultValue],
    [WageTypeResult].[Value] AS [ResultNumericValue]';
  SET @attributeSql = dbo.BuildAttributeQuery('[WageTypeResult].[Attributes]', @attributes);
  SET @pivotSql = @pivotSql + @attributeSql;
  SET @pivotSql = @pivotSql + N'
    FROM [dbo].[WageTypeResult]
  
  UNION ALL

'
  -- pivot sql wage type custom result
  SET @pivotSql = @pivotSql + N'
    -- wage type custom results
  SELECT 21 AS [ResultKind], -- 21: wage type custom
    [PayrollResultId],
    [WageTypeCustomResult].[Id] AS [ResultId],
    [WageTypeResult].[Id] AS [ResultParentId],
    [WageTypeCustomResult].[Source] AS [KindName],
    0 AS [ResultNumber], -- no custom result
    [WageTypeCustomResult].[Created] AS [ResultCreated],
    [WageTypeCustomResult].[Start] AS [ResultStart],
    [WageTypeCustomResult].[End] AS [ResultEnd],
    [WageTypeCustomResult].[Tags] AS [ResultTags],
    [WageTypeCustomResult].[Attributes],
    [WageTypeCustomResult].[ValueType] AS [ResultType],
    LTRIM([WageTypeCustomResult].[Value]) AS [ResultValue],
    [WageTypeCustomResult].[Value] AS [ResultNumericValue]';
  SET @attributeSql = dbo.BuildAttributeQuery('[WageTypeCustomResult].[Attributes]', @attributes);
  SET @pivotSql = @pivotSql + @attributeSql;
  SET @pivotSql = @pivotSql + N'
    FROM [dbo].[WageTypeResult]
  INNER JOIN [dbo].[WageTypeCustomResult]
    ON [WageTypeResult].[Id] = [WageTypeCustomResult].[WageTypeResultId]
  
  UNION ALL

'
  -- pivot sql payrun result
  SET @pivotSql = @pivotSql + N'
    -- payrun results
  SELECT 30 AS [ResultKind], -- 30: payrun
    [PayrollResultId],
    [PayrunResult].[Id] AS [ResultId],
    [PayrollResultId] AS [ResultParentId],
    [PayrunResult].[Name] AS [KindName],
    0 AS [ResultNumber], -- no custom results
    [PayrunResult].[Created] AS [ResultCreated],
    [PayrunResult].[Start] AS [ResultStart],
    [PayrunResult].[End] AS [ResultEnd],
    [PayrunResult].[Tags] AS [ResultTags],
    [PayrunResult].[Attributes],
    [PayrunResult].[ValueType] AS [ResultType],
    LTRIM([PayrunResult].[Value]) AS [ResultValue],
    [PayrunResult].[NumericValue] AS [ResultNumericValue]';
  SET @attributeSql = dbo.BuildAttributeQuery(NULL, @attributes);
  SET @pivotSql = @pivotSql + @attributeSql;
  SET @pivotSql = @pivotSql + N'
    FROM [dbo].[PayrunResult]
'
  -- pivot sql end part
  SET @pivotSql = @pivotSql + N'
  ) PayrollValue
LEFT JOIN
  -- payroll result
  [dbo].[PayrollResult]
  ON [PayrollResult].[Id] = [PayrollValue].[PayrollResultId]
LEFT JOIN
  -- parun job
  [dbo].[PayrunJob]
  ON [PayrollResult].[PayrunJobId] = [PayrunJob].[Id]
LEFT JOIN
  -- payrun
  [dbo].[Payrun]
  ON [PayrunJob].[PayrunId] = [Payrun].[Id]
LEFT JOIN
  -- employee
  [dbo].[Employee]
  ON [PayrollResult].[EmployeeId] = [Employee].[Id]
LEFT JOIN
  -- payroll
  [dbo].[Payroll]
  ON [PayrollResult].[PayrollId] = [Payroll].[Id]
LEFT JOIN
  -- division
  [dbo].[Division]
  ON [Payroll].[DivisionId] = [Division].[Id]
LEFT JOIN
  -- user
  [dbo].[User]
  ON [PayrunJob].[CreatedUserId] = [User].Id ' +
  IIF(@employeeId IS NULL, N'', N'WHERE [dbo].[Employee].[Id] = ' +  cast(@employeeId as varchar(10))) +
  N') AS PCV';

  -- debug help
  --PRINT CAST(@pivotSql AS NTEXT);
  
  -- transaction start
  BEGIN TRANSACTION;

    -- cleanup
  DROP TABLE IF EXISTS ##PayrollResultPivot;

  -- build pivot table
  EXECUTE dbo.sp_executesql @pivotSql;

  -- apply query to pivot table
  EXECUTE dbo.sp_executesql @sql;

  -- cleanup
  DROP TABLE IF EXISTS ##PayrollResultPivot;

  -- transaction end
  COMMIT TRANSACTION;

END
GO


