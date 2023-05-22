SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

IF EXISTS (
    SELECT *
    FROM sysobjects
    WHERE id = object_id(N'[dbo].[GetDerivedCaseRelations]')
      AND OBJECTPROPERTY(id, N'IsProcedure') = 1
    )
BEGIN
  DROP PROCEDURE dbo.[GetDerivedCaseRelations]
END
GO

-- =============================================
-- Get derived case relations (only active).
-- =============================================
CREATE PROCEDURE dbo.[GetDerivedCaseRelations]
  -- the tenant
  @tenantId AS INT,
  -- the payroll
  @payrollId AS INT,
  -- the regulation valid from date
  @regulationDate AS DATETIME2(7),
  -- creation date
  @createdBefore AS DATETIME2(7),
  -- the from case name
  @sourceCaseName AS NVARCHAR(128) = NULL,
  -- the to case name
  @targetCaseName AS NVARCHAR(128) = NULL,
  -- the include clusters: JSON array of cluster names VARCHAR(128)
  @includeClusters AS VARCHAR(MAX) = NULL,
  -- the exclude clusters: JSON array of cluster names VARCHAR(128)
  @excludeClusters AS VARCHAR(MAX) = NULL
AS
BEGIN
  -- SET NOCOUNT ON added to prevent extra result sets from
  -- interfering with SELECT statements
  SET NOCOUNT ON;

  -- select all active case relation, using the from/to case name and payroll level
  SELECT [Regulations].[Id] AS [RegulationId],
    [Regulations].[Level] AS [Level],
    [Regulations].[Priority] AS [Priority],
    -- perfomance hint: don't use [CaseRelation].*
    dbo.[CaseRelation].[Id],
    dbo.[CaseRelation].[Status],
    dbo.[CaseRelation].[Created],
    dbo.[CaseRelation].[Updated],
    dbo.[CaseRelation].[RegulationId],
    dbo.[CaseRelation].[SourceCaseName],
    dbo.[CaseRelation].[SourceCaseNameLocalizations],
    dbo.[CaseRelation].[SourceCaseSlot],
    dbo.[CaseRelation].[SourceCaseSlotLocalizations],
    dbo.[CaseRelation].[TargetCaseName],
    dbo.[CaseRelation].[TargetCaseNameLocalizations],
    dbo.[CaseRelation].[TargetCaseSlot],
    dbo.[CaseRelation].[TargetCaseSlotLocalizations],
    dbo.[CaseRelation].[RelationHash],
    dbo.[CaseRelation].[BuildExpression],
    dbo.[CaseRelation].[ValidateExpression],
    dbo.[CaseRelation].[OverrideType],
    dbo.[CaseRelation].[Order],
 --   dbo.[CaseRelation].[Binary],
    dbo.[CaseRelation].[ScriptHash],
    dbo.[CaseRelation].[Attributes],
    dbo.[CaseRelation].[Clusters]
  -- excluded columns
  --dbo.[CaseRelation].[Script],
  --dbo.[CaseRelation].[ScriptVersion]
  FROM dbo.[CaseRelation]
  INNER JOIN dbo.GetDerivedRegulations(@tenantId, @payrollId, @regulationDate, @createdBefore) AS [Regulations]
    ON dbo.[CaseRelation].[RegulationId] = [Regulations].[Id]
  -- active case relation only
  WHERE dbo.[CaseRelation].[Status] = 0
    AND dbo.[CaseRelation].[Created] < @createdBefore
    AND (
      @sourceCaseName IS NULL
      OR LOWER(dbo.[CaseRelation].[SourceCaseName]) = LOWER(@sourceCaseName)
      )
    AND (
      @targetCaseName IS NULL
      OR LOWER(dbo.[CaseRelation].[TargetCaseName]) = LOWER(@targetCaseName)
      )
    AND ((@includeClusters IS NULL AND @excludeClusters IS NULL)
        OR dbo.IsMatchingCluster(@includeClusters, @excludeClusters, dbo.[CaseRelation].[Clusters]) = 1)
  -- derived order by case relation source/target case names
  ORDER BY dbo.[CaseRelation].[SourceCaseName],
    dbo.[CaseRelation].[TargetCaseName],
    [Level] DESC,
    [Priority] DESC
END
GO


