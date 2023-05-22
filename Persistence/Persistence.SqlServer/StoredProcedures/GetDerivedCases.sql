SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

IF EXISTS (
    SELECT *
    FROM sysobjects
    WHERE id = object_id(N'[dbo].[GetDerivedCases]')
      AND OBJECTPROPERTY(id, N'IsProcedure') = 1
    )
BEGIN
  DROP PROCEDURE dbo.[GetDerivedCases]
END
GO

-- =============================================
-- Get the topmost derived cases (only active).
-- =============================================
CREATE PROCEDURE dbo.[GetDerivedCases]
  -- the tenant
  @tenantId AS INT,
  -- the payroll
  @payrollId AS INT,
  -- the regulation valid from date
  @regulationDate AS DATETIME2(7),
  -- creation date
  @createdBefore AS DATETIME2(7),
  -- the case type
  @caseType AS INT = NULL,
  -- the case names: JSON array of VARCHAR(128)
  @caseNames AS VARCHAR(MAX) = NULL,
  -- the include clusters: JSON array of cluster names VARCHAR(128)
  @includeClusters AS VARCHAR(MAX) = NULL,
  -- the exclude clusters: JSON array of cluster names VARCHAR(128)
  @excludeClusters AS VARCHAR(MAX) = NULL
AS
BEGIN
  -- SET NOCOUNT ON added to prevent extra result sets from
  -- interfering with SELECT statements
  SET NOCOUNT ON;

  -- select all active cases, using the order of case name and payroll level
  SELECT [Regulations].[Id] AS [RegulationId],
    [Regulations].[Level] AS [Level],
    [Regulations].[Priority] AS [Priority],
    -- perfomance hint: don't use [Case].*
    dbo.[Case].[Id],
    dbo.[Case].[Status],
    dbo.[Case].[Created],
    dbo.[Case].[Updated],
    dbo.[Case].[RegulationId],
    dbo.[Case].[CaseType],
    dbo.[Case].[Name],
    dbo.[Case].[NameLocalizations],
    dbo.[Case].[NameSynonyms],
    dbo.[Case].[Description],
    dbo.[Case].[DescriptionLocalizations],
    dbo.[Case].[DefaultReason],
    dbo.[Case].[DefaultReasonLocalizations],
    dbo.[Case].[BaseCase],
    dbo.[Case].[BaseCaseFields],
    dbo.[Case].[OverrideType],
    dbo.[Case].[CancellationType],
    dbo.[Case].[AvailableExpression],
    dbo.[Case].[BuildExpression],
    dbo.[Case].[ValidateExpression],
    dbo.[Case].[Lookups],
    dbo.[Case].[Slots],
 --   dbo.[Case].[Binary],
    dbo.[Case].[ScriptHash],
    dbo.[Case].[Attributes],
    dbo.[Case].[Clusters],
    dbo.[Case].[AvailableActions]
  -- excluded columns
  --dbo.[Case].[BuildActions],
  --dbo.[Case].[ValidateActions],
  --dbo.[Case].[Script],
  --dbo.[Case].[ScriptVersion]
  FROM dbo.[Case]
  INNER JOIN dbo.GetDerivedRegulations(@tenantId, @payrollId, @regulationDate, @createdBefore) AS [Regulations]
    ON dbo.[Case].[RegulationId] = [Regulations].[Id]
  -- active cases only
  WHERE dbo.[Case].[Status] = 0
    AND dbo.[Case].[Created] < @createdBefore
    AND (
      @caseType IS NULL
      OR dbo.[Case].[CaseType] = @caseType
      )
    AND ((@includeClusters IS NULL AND @excludeClusters IS NULL)
        OR dbo.IsMatchingCluster(@includeClusters, @excludeClusters, dbo.[Case].[Clusters]) = 1)
    AND (
      @caseNames IS NULL
      OR LOWER(dbo.[Case].[Name]) IN (
        SELECT LOWER(value)
        FROM OPENJSON(@caseNames)
        )
      )
  -- derived order by sort order
  ORDER BY [Level] DESC,
    [Priority] DESC
END
GO


