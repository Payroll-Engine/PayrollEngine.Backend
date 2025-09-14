SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

IF EXISTS (
    SELECT *
    FROM sysobjects
    WHERE id = object_id(N'[dbo].[GetNationalCaseChangeValues]')
      AND OBJECTPROPERTY(id, N'IsProcedure') = 1
    )
BEGIN
  DROP PROCEDURE dbo.[GetNationalCaseChangeValues]
END
GO

-- =============================================
-- Get national case changes using the attributes pivot
-- do not change the parameter names!
-- =============================================
CREATE PROCEDURE dbo.[GetNationalCaseChangeValues]
  -- the national id
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
  INTO ##NationalCaseChangeValuePivot
    FROM (
      SELECT
  -- tenant
  [dbo].[NationalCaseChange].[TenantId],
  -- case change
  [dbo].[NationalCaseChange].[Id] AS CaseChangeId,
  [dbo].[NationalCaseChange].[Created] AS CaseChangeCreated,
  [dbo].[NationalCaseChange].[Reason],
  [dbo].[NationalCaseChange].[ValidationCaseName],
  [dbo].[NationalCaseChange].[CancellationType],
  [dbo].[NationalCaseChange].[CancellationId],
  [dbo].[NationalCaseChange].[CancellationDate],
  NULL AS [EmployeeId],
  [dbo].[NationalCaseChange].[UserId],
  [dbo].[User].[Identifier] AS UserIdentifier,
  [dbo].[NationalCaseChange].[DivisionId],
  -- case value
  [dbo].[NationalCaseValue].[Id],
  [dbo].[NationalCaseValue].[Created],
  [dbo].[NationalCaseValue].[Updated],
  [dbo].[NationalCaseValue].[Status],
    -- localized case name
  ' +
  IIF(@culture IS NULL, '[dbo].[NationalCaseValue].[CaseName]', 'dbo.GetLocalizedValue([dbo].[NationalCaseValue].[CaseNameLocalizations], ''' + @culture + ''', [dbo].[NationalCaseValue].[CaseName])') + ' AS [CaseName],
  -- localized case field name
  ' +
  IIF(@culture IS NULL, '[dbo].[NationalCaseValue].[CaseFieldName]', 'dbo.GetLocalizedValue([dbo].[NationalCaseValue].[CaseFieldNameLocalizations], ''' + @culture + ''', [dbo].[NationalCaseValue].[CaseFieldName])') + ' AS [CaseFieldName],
  -- localized case slot
  ' +
  IIF(@culture IS NULL, '[dbo].[NationalCaseValue].[CaseSlot]', 'dbo.GetLocalizedValue([dbo].[NationalCaseValue].[CaseSlotLocalizations], ''' + @culture + ''', [dbo].[NationalCaseValue].[CaseSlot])') + ' AS [CaseSlot],
  [dbo].[NationalCaseValue].[CaseRelation],
  [dbo].[NationalCaseValue].[ValueType],
  [dbo].[NationalCaseValue].[Value],
  [dbo].[NationalCaseValue].[NumericValue],
  [dbo].[NationalCaseValue].[Culture],
  [dbo].[NationalCaseValue].[Start],
  [dbo].[NationalCaseValue].[End],
  [dbo].[NationalCaseValue].[Forecast],
  [dbo].[NationalCaseValue].[Tags],
  [dbo].[NationalCaseValue].[Attributes],
  -- documents
  (
      SELECT Count(*)
      FROM [dbo].[NationalCaseDocument]
      WHERE [CaseValueId] = [dbo].[NationalCaseValue].[Id]
      ) AS Documents'
    -- pivot sql part 2: attribute queries
    SET @attributeSql = dbo.BuildAttributeQuery('[dbo].[NationalCaseValue].[Attributes]', @attributes);
    SET @pivotSql = @pivotSql + @attributeSql;
    -- pivot sql part 3
    SET @pivotSql = @pivotSql + N'  FROM [dbo].[NationalCaseValue]
        LEFT JOIN [dbo].[NationalCaseValueChange]
          ON [dbo].[NationalCaseValue].[Id] = [dbo].[NationalCaseValueChange].[CaseValueId]
        LEFT JOIN [dbo].[NationalCaseChange]
          ON [dbo].[NationalCaseValueChange].[CaseChangeId] = [dbo].[NationalCaseChange].[Id]
        LEFT JOIN [dbo].[User]
          ON [dbo].[User].[Id] = [dbo].[NationalCaseChange].[UserId]
          WHERE ([dbo].[NationalCaseChange].[TenantId] = ' + CONVERT(VARCHAR(10), @parentId) + ')) AS NCCV';

  -- debug help
  --PRINT CAST(@pivotSql AS NTEXT);

  BEGIN TRY

    -- transaction start
    BEGIN TRANSACTION;

    -- start cleanup
    DROP TABLE IF EXISTS ##NationalCaseChangeValuePivot;

    -- build pivot table
    EXECUTE dbo.sp_executesql @pivotSql

    -- apply query to pivot table
    EXECUTE dbo.sp_executesql @sql

    -- cleanup
    DROP TABLE IF EXISTS ##NationalCaseChangeValuePivot;

    -- transaction end
    COMMIT TRANSACTION;
  END TRY
  BEGIN CATCH
    IF @@TRANCOUNT > 0
      ROLLBACK TRANSACTION;
  END CATCH;

END
GO


