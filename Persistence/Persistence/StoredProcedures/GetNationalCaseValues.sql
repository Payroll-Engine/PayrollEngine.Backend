SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

IF EXISTS (
    SELECT *
    FROM sysobjects
    WHERE id = object_id(N'[dbo].[GetNationalCaseValues]')
      AND OBJECTPROPERTY(id, N'IsProcedure') = 1
    )
BEGIN
  DROP PROCEDURE dbo.[GetNationalCaseValues]
END
GO

-- =============================================
-- Get national case values using the attributes pivot
-- do not change the parameter names!
-- =============================================
CREATE PROCEDURE dbo.[GetNationalCaseValues]
  -- the national id
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
    INTO ##NationalCaseValuePivot
    FROM (
      SELECT [dbo].[NationalCaseValue].*'

    -- pivot sql part 2: attribute queries
    SET @attributeSql = dbo.BuildAttributeQuery('[dbo].[NationalCaseValue].[Attributes]', @attributes);
    SET @pivotSql = @pivotSql + @attributeSql;
    -- pivot sql part 3
    SET @pivotSql = @pivotSql + N'  FROM dbo.NationalCaseValue
      WHERE NationalCaseValue.TenantId = ' + CAST(@parentId AS VARCHAR(10)) + ') AS NCVA';

  -- debug help
  --PRINT CAST(@pivotSql AS NTEXT);

  -- transaction start
  BEGIN TRANSACTION;

  -- cleanup
  DROP TABLE IF EXISTS ##NationalCaseValuePivot;

  -- build pivot table
  EXECUTE dbo.sp_executesql @pivotSql

  -- apply query to pivot table
  EXECUTE dbo.sp_executesql @sql

  -- cleanup
  DROP TABLE IF EXISTS ##NationalCaseValuePivot;

  -- transaction end
  COMMIT TRANSACTION;

END
GO


