SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

IF EXISTS (
    SELECT *
    FROM sysobjects
    WHERE id = object_id(N'[dbo].[GetDerivedWageTypes]')
      AND OBJECTPROPERTY(id, N'IsProcedure') = 1
    )
BEGIN
  DROP PROCEDURE dbo.[GetDerivedWageTypes]
END
GO

-- =============================================
-- Get derived wage types (only active).
-- =============================================
CREATE PROCEDURE dbo.[GetDerivedWageTypes]
  -- the tenant
  @tenantId AS INT,
  -- the payroll,
  @payrollId AS INT,
  -- the regulation valid from date
  @regulationDate AS DATETIME2(7),
  -- creation date
  @createdBefore AS DATETIME2(7),
  -- the wage type numbers: JSON array of DECIMAL(28, 6)
  @wageTypeNumbers AS VARCHAR(MAX) = NULL,
  -- the include clusters: JSON array of cluster names VARCHAR(128)
  @includeClusters AS VARCHAR(MAX) = NULL,
  -- the exclude clusters: JSON array of cluster names VARCHAR(128)
  @excludeClusters AS VARCHAR(MAX) = NULL
AS
BEGIN
  -- SET NOCOUNT ON added to prevent extra result sets from
  -- interfering with SELECT statements
  SET NOCOUNT ON;

  -- select using the wage type number and payroll level
  SELECT [Regulations].[Id] AS [RegulationId],
    [Regulations].[Level] AS [Level],
    [Regulations].[Priority] AS [Priority],
    -- perfomance hint: don't use [WageType].*
    dbo.[WageType].[Id],
    dbo.[WageType].[Status],
    dbo.[WageType].[Created],
    dbo.[WageType].[Updated],
    dbo.[WageType].[RegulationId],
    dbo.[WageType].[Name],
    dbo.[WageType].[NameLocalizations],
    dbo.[WageType].[WageTypeNumber],
    dbo.[WageType].[Description],
    dbo.[WageType].[DescriptionLocalizations],
    dbo.[WageType].[OverrideType],
    dbo.[WageType].[Calendar],
    dbo.[WageType].[Culture],
    dbo.[WageType].[Collectors],
    dbo.[WageType].[CollectorGroups],
    dbo.[WageType].[ValueExpression],
    dbo.[WageType].[ResultExpression],
    dbo.[WageType].[ValueActions],
    dbo.[WageType].[ResultActions],
  --  dbo.[WageType].[Binary],
    dbo.[WageType].[ScriptHash],
    dbo.[WageType].[Attributes],
    dbo.[WageType].[Clusters]
  -- excluded columns
  -- dbo.[WageType].[Script],
  -- dbo.[WageType].[ScriptVersion],
  FROM dbo.[WageType]
  INNER JOIN dbo.GetDerivedRegulations(@tenantId, @payrollId, @regulationDate, @createdBefore) AS [Regulations]
    ON dbo.[WageType].[RegulationId] = [Regulations].[Id]
  -- active wage types only
  WHERE dbo.[WageType].[Status] = 0
    AND dbo.[WageType].[Created] < @createdBefore
    AND ((@includeClusters IS NULL AND @excludeClusters IS NULL)
        OR dbo.IsMatchingCluster(@includeClusters, @excludeClusters, dbo.[WageType].[Clusters]) = 1)
    AND (
      @wageTypeNumbers IS NULL
      OR dbo.[WageType].[WageTypeNumber] IN (
        SELECT CAST(value AS DECIMAL(28, 6))
        FROM OPENJSON(@wageTypeNumbers)
        )
      )
  -- derived order by wage type number
  ORDER BY dbo.[WageType].[WageTypeNumber],
    [Level] DESC,
    [Priority] DESC
END
GO


