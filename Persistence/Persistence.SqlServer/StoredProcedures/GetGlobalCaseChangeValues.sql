SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

IF EXISTS (
    SELECT *
    FROM sysobjects
    WHERE id = object_id(N'[dbo].[GetGlobalCaseChangeValues]')
      AND OBJECTPROPERTY(id, N'IsProcedure') = 1
    )
BEGIN
  DROP PROCEDURE dbo.[GetGlobalCaseChangeValues]
END
GO

-- =============================================
-- Get global case changes using the attributes pivot
-- do not change the parameter names!
-- =============================================
CREATE PROCEDURE dbo.[GetGlobalCaseChangeValues]
  -- the global id
  @parentId AS INT,
  -- the query sql
  @sql AS NVARCHAR(MAX),
  -- the attribute names: JSON array of VARCHAR(128)
  @attributes AS NVARCHAR(MAX) = NULL,
  -- the cultue
  @culture AS NVARCHAR(128) = NULL
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
  INTO ##GlobalCaseChangeValuePivot
    FROM (
      SELECT
  -- tenant
  [dbo].[GlobalCaseChange].[TenantId],
  -- case change
  [dbo].[GlobalCaseChange].[Id] AS CaseChangeId,
  [dbo].[GlobalCaseChange].[Created] AS CaseChangeCreated,
  [dbo].[GlobalCaseChange].[Reason],
  [dbo].[GlobalCaseChange].[ValidationCaseName],
  [dbo].[GlobalCaseChange].[CancellationType],
  [dbo].[GlobalCaseChange].[CancellationId],
  [dbo].[GlobalCaseChange].[CancellationDate],
  NULL AS [EmployeeId],
  [dbo].[GlobalCaseChange].[UserId],
  [dbo].[User].[Identifier] AS UserIdentifier,
  [dbo].[GlobalCaseChange].[DivisionId],
  -- case value
  [dbo].[GlobalCaseValue].[Id],
  [dbo].[GlobalCaseValue].[Created],
  [dbo].[GlobalCaseValue].[Updated],
  [dbo].[GlobalCaseValue].[Status],
  -- localized case name
  ' +
  IIF(@culture IS NULL, '[dbo].[GlobalCaseValue].[CaseName]', 'dbo.GetLocalizedValue([dbo].[GlobalCaseValue].[CaseNameLocalizations], ''' + @culture + ''', [dbo].[GlobalCaseValue].[CaseName])') + ' AS [CaseName],
  -- localized case field name
  ' +
  IIF(@culture IS NULL, '[dbo].[GlobalCaseValue].[CaseFieldName]', 'dbo.GetLocalizedValue([dbo].[GlobalCaseValue].[CaseFieldNameLocalizations], ''' + @culture + ''', [dbo].[GlobalCaseValue].[CaseFieldName])') + ' AS [CaseFieldName],
  -- localized case slot
  ' +
  IIF(@culture IS NULL, '[dbo].[GlobalCaseValue].[CaseSlot]', 'dbo.GetLocalizedValue([dbo].[GlobalCaseValue].[CaseSlotLocalizations], ''' + @culture + ''', [dbo].[GlobalCaseValue].[CaseSlot])') + ' AS [CaseSlot],
  [dbo].[GlobalCaseValue].[CaseRelation],
  [dbo].[GlobalCaseValue].[ValueType],
  [dbo].[GlobalCaseValue].[Value],
  [dbo].[GlobalCaseValue].[NumericValue],
  [dbo].[GlobalCaseValue].[Start],
  [dbo].[GlobalCaseValue].[End],
  [dbo].[GlobalCaseValue].[Forecast],
  [dbo].[GlobalCaseValue].[Tags],
  [dbo].[GlobalCaseValue].[Attributes],
  -- documents
  (
      SELECT Count(*)
      FROM [dbo].[GlobalCaseDocument]
      WHERE [CaseValueId] = [dbo].[GlobalCaseValue].[Id]
      ) AS Documents'
    -- pivot sql part 2: attribute queries
    SET @attributeSql = dbo.BuildAttributeQuery('[dbo].[GlobalCaseValue].[Attributes]', @attributes);
    SET @pivotSql = @pivotSql + @attributeSql;
    -- pivot sql part 3
    SET @pivotSql = @pivotSql + N'  FROM [dbo].[GlobalCaseValue]
        LEFT JOIN [dbo].[GlobalCaseValueChange]
          ON [dbo].[GlobalCaseValue].[Id] = [dbo].[GlobalCaseValueChange].[CaseValueId]
        LEFT JOIN [dbo].[GlobalCaseChange]
          ON [dbo].[GlobalCaseValueChange].[CaseChangeId] = [dbo].[GlobalCaseChange].[Id]
        LEFT JOIN [dbo].[User]
          ON [dbo].[User].[Id] = [dbo].[GlobalCaseChange].[UserId]
          WHERE ([dbo].[GlobalCaseChange].[TenantId] = ' + CONVERT(VARCHAR(10), @parentId) + ')) AS GCCV';

  -- debug help
  --PRINT CAST(@pivotSql AS NTEXT);

  -- transaction start
  BEGIN TRANSACTION;

  -- start cleanup
  DROP TABLE IF EXISTS ##GlobalCaseChangeValuePivot;

  -- build pivot table
  EXECUTE dbo.sp_executesql @pivotSql

  -- apply query to pivot table
  EXECUTE dbo.sp_executesql @sql

  -- cleanup
  DROP TABLE IF EXISTS ##GlobalCaseChangeValuePivot;

  -- transaction end
  COMMIT TRANSACTION;

END
GO


