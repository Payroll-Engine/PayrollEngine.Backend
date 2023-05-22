SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

IF EXISTS (
    SELECT *
    FROM sysobjects
    WHERE id = object_id(N'[dbo].[GetDerivedLookupValues]')
      AND OBJECTPROPERTY(id, N'IsProcedure') = 1
    )
BEGIN
  DROP PROCEDURE dbo.[GetDerivedLookupValues]
END
GO

-- =============================================
-- Get the topmost derived lookup values (only active).
-- =============================================
CREATE PROCEDURE dbo.[GetDerivedLookupValues]
  -- the tenant
  @tenantId AS INT,
  -- the payroll
  @payrollId AS INT,
  -- the regulation valid from date
  @regulationDate AS DATETIME2(7),
  -- creation date
  @createdBefore AS DATETIME2(7),
  -- the lookup names: JSON array of VARCHAR(128)
  @lookupNames AS VARCHAR(MAX) = NULL,
  -- the lookup keys: JSON array of VARCHAR(128)
  @lookupKeys AS VARCHAR(MAX) = NULL
AS
BEGIN
  -- SET NOCOUNT ON added to prevent extra result sets from
  -- interfering with SELECT statements
  SET NOCOUNT ON;

  -- select all active lookup parameters, using the order of lookup name and payroll level
  SELECT [Regulations].[Id] AS [RegulationId],
    [Regulations].[Level] AS [Level],
    [Regulations].[Priority] AS [Priority],
    dbo.[LookupValue].*
  FROM dbo.[LookupValue]
  INNER JOIN dbo.[Lookup]
    ON dbo.[LookupValue].[LookupId] = dbo.[Lookup].[Id]
  INNER JOIN dbo.GetDerivedRegulations(@tenantId, @payrollId, @regulationDate, @createdBefore) AS [Regulations]
    ON dbo.[Lookup].[RegulationId] = [Regulations].[Id]
  -- active lookups only
  WHERE dbo.[LookupValue].[Status] = 0
    AND dbo.[LookupValue].[Created] < @createdBefore
    AND (
      @lookupNames IS NULL
      OR LOWER(dbo.[Lookup].[Name]) IN (
        SELECT LOWER(value)
        FROM OPENJSON(@lookupNames)
        )
      )
    AND (
      -- case sensitive lookup value key
      @lookupKeys IS NULL
      OR dbo.[LookupValue].[Key] IN (
        SELECT value
        FROM OPENJSON(@lookupKeys)
        )
      )
  -- derived order by sort order
  ORDER BY [Level] DESC,
    [Priority] DESC
END
GO


