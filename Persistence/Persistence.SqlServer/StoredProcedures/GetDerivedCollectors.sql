SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

IF EXISTS (
    SELECT *
    FROM sysobjects
    WHERE id = object_id(N'[dbo].[GetDerivedCollectors]')
      AND OBJECTPROPERTY(id, N'IsProcedure') = 1
    )
BEGIN
  DROP PROCEDURE dbo.[GetDerivedCollectors]
END
GO

-- =============================================
-- Get the topmost derived collectors (only active).
-- =============================================
CREATE PROCEDURE dbo.[GetDerivedCollectors]
  -- the tenant
  @tenantId AS INT,
  -- the payroll
  @payrollId AS INT,
  -- the regulation valid from date
  @regulationDate AS DATETIME2(7),
  -- creation date
  @createdBefore AS DATETIME2(7),
  -- the collector names: JSON array of VARCHAR(128)
  @collectorNames AS VARCHAR(MAX) = NULL,
  -- the include clusters: JSON array of cluster names VARCHAR(128)
  @includeClusters AS VARCHAR(MAX) = NULL,
  -- the exclude clusters: JSON array of cluster names VARCHAR(128)
  @excludeClusters AS VARCHAR(MAX) = NULL
AS
BEGIN
  -- SET NOCOUNT ON added to prevent extra result sets from
  -- interfering with SELECT statements
  SET NOCOUNT ON;

  -- select all active collectors, using the order of collector name and payroll level
  SELECT [Regulations].[Id] AS [RegulationId],
    [Regulations].[Level] AS [Level],
    [Regulations].[Priority] AS [Priority],
    -- perfomance hint: don't use [Collector].*
    dbo.[Collector].[Id],
    dbo.[Collector].[Status],
    dbo.[Collector].[Created],
    dbo.[Collector].[Updated],
    dbo.[Collector].[RegulationId],
    dbo.[Collector].[Name],
    dbo.[Collector].[NameLocalizations],
    dbo.[Collector].[CollectMode],
    dbo.[Collector].[Negated],
    dbo.[Collector].[OverrideType],
    dbo.[Collector].[Culture],
    dbo.[Collector].[CollectorGroups],
    dbo.[Collector].[StartExpression],
    dbo.[Collector].[ApplyExpression],
    dbo.[Collector].[EndExpression],
    dbo.[Collector].[StartActions],
    dbo.[Collector].[ApplyActions],
    dbo.[Collector].[EndActions],
    dbo.[Collector].[Threshold],
    dbo.[Collector].[MinResult],
    dbo.[Collector].[MaxResult],
 --   dbo.[Collector].[Binary],
    dbo.[Collector].[ScriptHash],
    dbo.[Collector].[Attributes],
    dbo.[Collector].[Clusters]
  -- excluded columns
  --dbo.[Collector].[Script],
  --dbo.[Collector].[ScriptVersion]
  FROM dbo.[Collector]
  INNER JOIN dbo.GetDerivedRegulations(@tenantId, @payrollId, @regulationDate, @createdBefore) AS [Regulations]
    ON dbo.[Collector].[RegulationId] = [Regulations].[Id]
  -- active collectors only
  WHERE dbo.[Collector].[Status] = 0
    AND dbo.[Collector].[Created] < @createdBefore
    AND ((@includeClusters IS NULL AND @excludeClusters IS NULL)
        OR dbo.IsMatchingCluster(@includeClusters, @excludeClusters, dbo.[Collector].[Clusters]) = 1)
    AND (
      @collectorNames IS NULL
      OR LOWER(dbo.[Collector].[Name]) IN (
        SELECT LOWER(value)
        FROM OPENJSON(@collectorNames)
        )
      )
  -- derived order by collector name
  ORDER BY dbo.[Collector].[Name],
    [Level] DESC,
    [Priority] DESC
END
GO


