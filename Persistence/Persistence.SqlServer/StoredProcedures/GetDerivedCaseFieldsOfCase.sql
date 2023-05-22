SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

IF EXISTS (
    SELECT *
    FROM sysobjects
    WHERE id = object_id(N'[dbo].[GetDerivedCaseFieldsOfCase]')
      AND OBJECTPROPERTY(id, N'IsProcedure') = 1
    )
BEGIN
  DROP PROCEDURE dbo.[GetDerivedCaseFieldsOfCase]
END
GO

-- =============================================
-- Get the derived case fields of payroll (only active).
-- =============================================
CREATE PROCEDURE dbo.[GetDerivedCaseFieldsOfCase]
  -- the tenant
  @tenantId AS INT,
  -- the payroll
  @payrollId AS INT,
  -- the regulation valid from date
  @regulationDate AS DATETIME2(7),
  -- creation date
  @createdBefore AS DATETIME2(7),
  -- name of the cases: JSON array of VARCHAR(128)
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

  -- select the case fields by name, using the order of the payroll level
  SELECT [Regulations].[Id] AS [RegulationId],
    [Regulations].[Level] AS [Level],
    [Regulations].[Priority] AS [Priority],
    dbo.[Case].[Id] AS [CaseId],
    dbo.[Case].[CaseType] AS [CaseType],
    dbo.[CaseField].*
  FROM dbo.[CaseField]
  INNER JOIN dbo.[Case]
    ON dbo.[CaseField].[CaseId] = dbo.[Case].[Id]
  INNER JOIN dbo.GetDerivedRegulations(@tenantId, @payrollId, @regulationDate, @createdBefore) AS [Regulations]
    ON dbo.[Case].[RegulationId] = [Regulations].[Id]
  -- active case fields only
  WHERE dbo.[CaseField].[Status] = 0
    AND dbo.[CaseField].[Created] < @createdBefore
    AND ((@includeClusters IS NULL AND @excludeClusters IS NULL)
        OR dbo.IsMatchingCluster(@includeClusters, @excludeClusters, dbo.[CaseField].[Clusters]) = 1)
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


