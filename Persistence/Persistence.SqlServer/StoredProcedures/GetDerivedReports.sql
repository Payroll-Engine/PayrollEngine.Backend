SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

IF EXISTS (
    SELECT *
    FROM sysobjects
    WHERE id = object_id(N'[dbo].[GetDerivedReports]')
      AND OBJECTPROPERTY(id, N'IsProcedure') = 1
    )
BEGIN
  DROP PROCEDURE dbo.[GetDerivedReports]
END
GO

-- =============================================
-- Get the topmost derived reports (only active).
-- =============================================
CREATE PROCEDURE dbo.[GetDerivedReports]
  -- the tenant
  @tenantId AS INT,
  -- the payroll
  @payrollId AS INT,
  -- the regulation valid from date
  @regulationDate AS DATETIME2(7),
  -- creation date
  @createdBefore AS DATETIME2(7),
  -- user type
  @userType AS INT = NULL,
-- the report names: JSON array of VARCHAR(128)
  @reportNames AS VARCHAR(MAX) = NULL,
  -- the include clusters: JSON array of cluster names VARCHAR(128)
  @includeClusters AS VARCHAR(MAX) = NULL,
  -- the exclude clusters: JSON array of cluster names VARCHAR(128)
  @excludeClusters AS VARCHAR(MAX) = NULL
AS
BEGIN
  -- SET NOCOUNT ON added to prevent extra result sets from
  -- interfering with SELECT statements
  SET NOCOUNT ON;

  -- select all active reports, using the order of report name and payroll level
  SELECT [Regulations].[Id] AS [RegulationId],
    [Regulations].[Level] AS [Level],
    [Regulations].[Priority] AS [Priority],
    -- perfomance hint: don't use [Report].*
    dbo.[Report].[Id],
    dbo.[Report].[Status],
    dbo.[Report].[Created],
    dbo.[Report].[Updated],
    dbo.[Report].[RegulationId],
    dbo.[Report].[Name],
    dbo.[Report].[NameLocalizations],
    dbo.[Report].[Description],
    dbo.[Report].[DescriptionLocalizations],
    dbo.[Report].[Category],
    dbo.[Report].[Queries],
    dbo.[Report].[Relations],
    dbo.[Report].[AttributeMode],
    dbo.[Report].[UserType],
    dbo.[Report].[BuildExpression],
    dbo.[Report].[StartExpression],
    dbo.[Report].[EndExpression],
  --  dbo.[Report].[Binary],
    dbo.[Report].[ScriptHash],
    dbo.[Report].[Attributes],
    dbo.[Report].[Clusters]
  -- excluded columns
  --dbo.[Report].[Script],
  --dbo.[Report].[ScriptVersion]
  FROM dbo.[Report]
  INNER JOIN dbo.GetDerivedRegulations(@tenantId, @payrollId, @regulationDate, @createdBefore) AS [Regulations]
    ON dbo.[Report].[RegulationId] = [Regulations].[Id]
  -- active reports only
  WHERE dbo.[Report].[Status] = 0
    AND dbo.[Report].[Created] < @createdBefore
    AND (@userType IS NULL OR dbo.[Report].[UserType] <= @userType)
    AND ((@includeClusters IS NULL AND @excludeClusters IS NULL)
        OR dbo.IsMatchingCluster(@includeClusters, @excludeClusters, dbo.[Report].[Clusters]) = 1)
    AND (
      @reportNames IS NULL
      OR LOWER(dbo.[Report].[Name]) IN (
        SELECT LOWER(value)
        FROM OPENJSON(@reportNames)
        )
      )
  -- derived order by sort order
  ORDER BY [Level] DESC,
    [Priority] DESC
END
GO


