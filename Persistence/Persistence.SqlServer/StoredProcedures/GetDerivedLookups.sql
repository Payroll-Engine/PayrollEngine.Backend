SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

IF EXISTS (
    SELECT *
    FROM sysobjects
    WHERE id = object_id(N'[dbo].[GetDerivedLookups]')
      AND OBJECTPROPERTY(id, N'IsProcedure') = 1
    )
BEGIN
  DROP PROCEDURE dbo.[GetDerivedLookups]
END
GO

-- =============================================
-- Get the topmost derived lookups (only active).
-- =============================================
CREATE PROCEDURE dbo.[GetDerivedLookups]
  -- the tenant
  @tenantId AS INT,
  -- the payroll
  @payrollId AS INT,
  -- the regulation valid from date
  @regulationDate AS DATETIME2(7),
  -- creation date
  @createdBefore AS DATETIME2(7),
  -- the lookup names: JSON array of VARCHAR(128)
  @lookupNames AS VARCHAR(MAX) = NULL
AS
BEGIN
  -- SET NOCOUNT ON added to prevent extra result sets from
  -- interfering with SELECT statements
  SET NOCOUNT ON;

  -- select all active lookups, using the order of lookup name and payroll level
  SELECT [Regulations].[Id] AS [RegulationId],
    [Regulations].[Level] AS [Level],
    [Regulations].[Priority] AS [Priority],
    dbo.[Lookup].*
  FROM dbo.[Lookup]
  INNER JOIN dbo.GetDerivedRegulations(@tenantId, @payrollId, @regulationDate, @createdBefore) AS [Regulations]
    ON dbo.[Lookup].[RegulationId] = [Regulations].[Id]
  -- active lookups only
  WHERE dbo.[Lookup].[Status] = 0
    AND dbo.[Lookup].[Created] < @createdBefore
    AND (
      @lookupNames IS NULL
      OR LOWER(dbo.[Lookup].[Name]) IN (
        SELECT LOWER(value)
        FROM OPENJSON(@lookupNames)
        )
      )
  -- derived order by sort order
  ORDER BY [Level] DESC,
    [Priority] DESC
END
GO


