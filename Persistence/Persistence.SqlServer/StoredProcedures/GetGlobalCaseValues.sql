SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

IF EXISTS (
    SELECT *
    FROM sysobjects
    WHERE id = object_id(N'[dbo].[GetGlobalCaseValues]')
      AND OBJECTPROPERTY(id, N'IsProcedure') = 1
    )
BEGIN
  DROP PROCEDURE dbo.[GetGlobalCaseValues]
END
GO

-- =============================================
-- Get global case values using the attributes pivot
-- do not change the parameter names!
-- =============================================
CREATE PROCEDURE dbo.[GetGlobalCaseValues]
  -- the global id
  @parentId AS INT,
  -- the query sql
  @sql AS NVARCHAR(MAX),
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

  -- pivot sql part 1
  SET @pivotSql = N'SELECT *
    INTO ##GlobalCaseValuePivot
    FROM (
      SELECT [dbo].[GlobalCaseValue].*'

    -- pivot sql part 2: attribute queries
    SET @attributeSql = dbo.BuildAttributeQuery('[dbo].[GlobalCaseValue].[Attributes]', @attributes);
    SET @pivotSql = @pivotSql + @attributeSql;
    -- pivot sql part 3
    SET @pivotSql = @pivotSql + N'  FROM dbo.GlobalCaseValue
      WHERE GlobalCaseValue.TenantId = ' + CAST(@parentId AS VARCHAR(10)) + ') AS GCVA';

  -- debug help
  --PRINT CAST(@pivotSql AS NTEXT);

  -- transaction start
  BEGIN TRANSACTION;

  -- cleanup
  DROP TABLE IF EXISTS ##GlobalCaseValuePivot;

  -- build pivot table
  EXECUTE dbo.sp_executesql @pivotSql

  -- apply query to pivot table
  EXECUTE dbo.sp_executesql @sql

  -- cleanup
  DROP TABLE IF EXISTS ##GlobalCaseValuePivot;
  
  -- transaction end
  COMMIT TRANSACTION;

END
GO


