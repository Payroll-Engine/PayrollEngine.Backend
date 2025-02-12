SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

IF EXISTS (
    SELECT *
    FROM sysobjects
    WHERE id = object_id(N'[dbo].[GetCompanyCaseChangeValues]')
      AND OBJECTPROPERTY(id, N'IsProcedure') = 1
    )
BEGIN
  DROP PROCEDURE dbo.[GetCompanyCaseChangeValues]
END
GO

-- =============================================
-- Get company case changes using the attributes pivot
-- do not change the parameter names!
-- =============================================
CREATE PROCEDURE dbo.[GetCompanyCaseChangeValues]
  -- the company id
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
  INTO ##CompanyCaseChangeValuePivot
    FROM (
      SELECT
  -- tenant
  [dbo].[CompanyCaseChange].[TenantId],
  -- case change
  [dbo].[CompanyCaseChange].[Id] AS CaseChangeId,
  [dbo].[CompanyCaseChange].[Created] AS CaseChangeCreated,
  [dbo].[CompanyCaseChange].[Reason],
  [dbo].[CompanyCaseChange].[ValidationCaseName],
  [dbo].[CompanyCaseChange].[CancellationType],
  [dbo].[CompanyCaseChange].[CancellationId],
  [dbo].[CompanyCaseChange].[CancellationDate],
  NULL AS [EmployeeId],
  [dbo].[CompanyCaseChange].[UserId],
  [dbo].[User].[Identifier] AS UserIdentifier,
  [dbo].[CompanyCaseChange].[DivisionId],
  -- case value
  [dbo].[CompanyCaseValue].[Id],
  [dbo].[CompanyCaseValue].[Created],
  [dbo].[CompanyCaseValue].[Updated],
  [dbo].[CompanyCaseValue].[Status],
  -- localized case name
  ' +
  IIF(@culture IS NULL, '[dbo].[CompanyCaseValue].[CaseName]', 'dbo.GetLocalizedValue([dbo].[CompanyCaseValue].[CaseNameLocalizations], ''' + @culture + ''', [dbo].[CompanyCaseValue].[CaseName])') + ' AS [CaseName],
  -- localized case field name
  ' +
  IIF(@culture IS NULL, '[dbo].[CompanyCaseValue].[CaseFieldName]', 'dbo.GetLocalizedValue([dbo].[CompanyCaseValue].[CaseFieldNameLocalizations], ''' + @culture + ''', [dbo].[CompanyCaseValue].[CaseFieldName])') + ' AS [CaseFieldName],
  -- localized case slot
  ' +
  IIF(@culture IS NULL, '[dbo].[CompanyCaseValue].[CaseSlot]', 'dbo.GetLocalizedValue([dbo].[CompanyCaseValue].[CaseSlotLocalizations], ''' + @culture + ''', [dbo].[CompanyCaseValue].[CaseSlot])') + ' AS [CaseSlot],
  [dbo].[CompanyCaseValue].[CaseRelation],
  [dbo].[CompanyCaseValue].[ValueType],
  [dbo].[CompanyCaseValue].[Value],
  [dbo].[CompanyCaseValue].[NumericValue],
  [dbo].[CompanyCaseValue].[Start],
  [dbo].[CompanyCaseValue].[End],
  [dbo].[CompanyCaseValue].[Forecast],
  [dbo].[CompanyCaseValue].[Tags],
  [dbo].[CompanyCaseValue].[Attributes],
  -- documents
  (
      SELECT Count(*)
      FROM [dbo].[CompanyCaseDocument]
      WHERE [CaseValueId] = [dbo].[CompanyCaseValue].[Id]
      ) AS Documents'
  -- pivot sql part 2: attribute queries
  SET @attributeSql = dbo.BuildAttributeQuery('[dbo].[CompanyCaseValue].[Attributes]', @attributes);
  SET @pivotSql = @pivotSql + @attributeSql;
  -- pivot sql part 3
  SET @pivotSql = @pivotSql + N'  FROM [dbo].[CompanyCaseValue]
        LEFT JOIN [dbo].[CompanyCaseValueChange]
          ON [dbo].[CompanyCaseValue].[Id] = [dbo].[CompanyCaseValueChange].[CaseValueId]
        LEFT JOIN [dbo].[CompanyCaseChange]
          ON [dbo].[CompanyCaseValueChange].[CaseChangeId] = [dbo].[CompanyCaseChange].[Id]
        LEFT JOIN [dbo].[User]
          ON [dbo].[User].[Id] = [dbo].[CompanyCaseChange].[UserId]
        WHERE ([dbo].[CompanyCaseChange].[TenantId] = ' + CONVERT(VARCHAR(10), @parentId) + ')) AS CCCV';

  -- debug help
  --PRINT CAST(@pivotSql AS NTEXT);

  -- transacton isolation level
  SET TRANSACTION ISOLATION LEVEL READ COMMITTED

  BEGIN TRY

    -- transaction start
    BEGIN TRANSACTION;

    -- start cleanup
    DROP TABLE IF EXISTS ##CompanyCaseChangeValuePivot;

    -- build pivot table
    EXECUTE dbo.sp_executesql @pivotSql

    -- apply query to pivot table
    EXECUTE dbo.sp_executesql @sql

    -- cleanup
    DROP TABLE IF EXISTS ##CompanyCaseChangeValuePivot;

    -- transaction end
    COMMIT TRANSACTION;
  END TRY
  BEGIN CATCH
    IF @@TRANCOUNT > 0
      ROLLBACK TRANSACTION;
  END CATCH;

END
GO


