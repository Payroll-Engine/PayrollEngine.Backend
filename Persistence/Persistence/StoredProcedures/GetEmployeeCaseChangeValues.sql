SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

IF EXISTS (
    SELECT *
    FROM sysobjects
    WHERE id = object_id(N'[dbo].[GetEmployeeCaseChangeValues]')
      AND OBJECTPROPERTY(id, N'IsProcedure') = 1
    )
BEGIN
  DROP PROCEDURE dbo.[GetEmployeeCaseChangeValues]
END
GO

-- =============================================
-- Get employee case changes using the attributes pivot
-- do not change the parameter names!
-- =============================================
CREATE PROCEDURE dbo.[GetEmployeeCaseChangeValues]
  -- the employee id
  @parentId AS INT,
  -- the query sql
  @sql AS NVARCHAR(MAX),
  -- the attribute names: JSON array of VARCHAR(128)
  @attributes AS NVARCHAR(MAX) = NULL,
  -- the ISO 639-1 language code
  @language AS NVARCHAR(3) = NULL
AS
BEGIN
  -- SET NOCOUNT ON added to prevent extra result sets from
  -- interfering with SELECT statements
  SET NOCOUNT ON;

    -- pivot select
  DECLARE @pivotSql AS NVARCHAR(MAX);
  DECLARE @attributeSql AS NVARCHAR(MAX);

  -- pivot sql part 1
  SET @pivotSql = 
    N'SELECT *
  INTO ##EmployeeCaseChangeValuePivot
    FROM (
      SELECT
  -- tenant
  [dbo].[Employee].[TenantId],
  -- case change
  [dbo].[EmployeeCaseChange].[Id] AS CaseChangeId,
  [dbo].[EmployeeCaseChange].[Created] AS CaseChangeCreated,
  [dbo].[EmployeeCaseChange].[Reason],
  [dbo].[EmployeeCaseChange].[ValidationCaseName],
  [dbo].[EmployeeCaseChange].[CancellationType],
  [dbo].[EmployeeCaseChange].[CancellationId],
  [dbo].[EmployeeCaseChange].[CancellationDate],
  [dbo].[EmployeeCaseChange].[EmployeeId],
  [dbo].[EmployeeCaseChange].[UserId],
  [dbo].[User].[Identifier] AS UserIdentifier,
  [dbo].[EmployeeCaseChange].[DivisionId],
  -- case value
  [dbo].[EmployeeCaseValue].[Id],
  [dbo].[EmployeeCaseValue].[Created],
  [dbo].[EmployeeCaseValue].[Updated],
  [dbo].[EmployeeCaseValue].[Status],
  -- localized case name
  ' +
  IIF(@language IS NULL, '[dbo].[EmployeeCaseValue].[CaseName]', 'dbo.GetLocalizedValue([dbo].[EmployeeCaseValue].[CaseNameLocalizations], ''' + @language + ''', [dbo].[EmployeeCaseValue].[CaseName])') + ' AS [CaseName],
  -- localized case field name
  ' +
  IIF(@language IS NULL, '[dbo].[EmployeeCaseValue].[CaseFieldName]', 'dbo.GetLocalizedValue([dbo].[EmployeeCaseValue].[CaseFieldNameLocalizations], ''' + @language + ''', [dbo].[EmployeeCaseValue].[CaseFieldName])') + ' AS [CaseFieldName],
  -- localized case slot
  ' +
  IIF(@language IS NULL, '[dbo].[EmployeeCaseValue].[CaseSlot]', 'dbo.GetLocalizedValue([dbo].[EmployeeCaseValue].[CaseSlotLocalizations], ''' + @language + ''', [dbo].[EmployeeCaseValue].[CaseSlot])') + ' AS [CaseSlot],
  [dbo].[EmployeeCaseValue].[CaseRelation],
  [dbo].[EmployeeCaseValue].[ValueType],
  [dbo].[EmployeeCaseValue].[Value],
  [dbo].[EmployeeCaseValue].[NumericValue],
  [dbo].[EmployeeCaseValue].[Start],
  [dbo].[EmployeeCaseValue].[End],
  [dbo].[EmployeeCaseValue].[Forecast],
  [dbo].[EmployeeCaseValue].[Tags],
  [dbo].[EmployeeCaseValue].[Attributes],
  -- documents
  (
      SELECT Count(*)
      FROM [dbo].[EmployeeCaseDocument]
      WHERE [CaseValueId] = [dbo].[EmployeeCaseValue].[Id]
      ) AS Documents'
  -- pivot sql part 2: attribute queries
  SET @attributeSql = dbo.BuildAttributeQuery('[dbo].[EmployeeCaseValue].[Attributes]', @attributes);
  SET @pivotSql = @pivotSql + @attributeSql;
  -- pivot sql part 3
  SET @pivotSql = @pivotSql + N' FROM [dbo].[EmployeeCaseValue]
        LEFT JOIN [dbo].[EmployeeCaseValueChange]
          ON [dbo].[EmployeeCaseValue].[Id] = [dbo].[EmployeeCaseValueChange].[CaseValueId]
        LEFT JOIN [dbo].[EmployeeCaseChange]
          ON [dbo].[EmployeeCaseValueChange].[CaseChangeId] = [dbo].[EmployeeCaseChange].[Id]
        LEFT JOIN [dbo].[User]
          ON [dbo].[User].[Id] = [dbo].[EmployeeCaseChange].[UserId]
        LEFT JOIN [dbo].[Employee]
          ON [dbo].[Employee].[Id] = [dbo].[EmployeeCaseChange].[EmployeeId]
          WHERE ([dbo].[EmployeeCaseChange].[EmployeeId] = ' + CONVERT(VARCHAR(10), @parentId) + ')) AS ECCV';

  -- debug help
  --PRINT CAST(@pivotSql AS NTEXT);

  -- transaction start
  BEGIN TRANSACTION;

  -- start cleanup
  DROP TABLE IF EXISTS ##EmployeeCaseChangeValuePivot;

  -- build pivot table
  EXECUTE dbo.sp_executesql @pivotSql

  -- apply query to pivot table
  EXECUTE dbo.sp_executesql @sql

  -- end cleanup
  DROP TABLE IF EXISTS ##EmployeeCaseChangeValuePivot

  -- transaction end
  COMMIT TRANSACTION;

END
GO


