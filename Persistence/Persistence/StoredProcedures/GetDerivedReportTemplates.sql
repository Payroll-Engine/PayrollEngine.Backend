SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

IF EXISTS (
    SELECT *
    FROM sysobjects
    WHERE id = object_id(N'[dbo].[GetDerivedReportTemplates]')
      AND OBJECTPROPERTY(id, N'IsProcedure') = 1
    )
BEGIN
  DROP PROCEDURE dbo.[GetDerivedReportTemplates]
END
GO

-- =============================================
-- Get the topmost derived report templates (only active).
-- =============================================
CREATE PROCEDURE dbo.[GetDerivedReportTemplates]
  -- the tenant
  @tenantId AS INT,
  -- the payroll
  @payrollId AS INT,
  -- the regulation valid from date
  @regulationDate AS DATETIME2(7),
  -- creation date
  @createdBefore AS DATETIME2(7),
  -- the report names: JSON array of VARCHAR(128)
  @reportNames AS VARCHAR(MAX) = NULL,
  -- the report language
  @language AS INT = NULL
AS
BEGIN
  -- SET NOCOUNT ON added to prevent extra result sets from
  -- interfering with SELECT statements
  SET NOCOUNT ON;

  -- select all active report templates, using the order of report name and payroll level
  SELECT [Regulations].[Id] AS [RegulationId],
    [Regulations].[Level] AS [Level],
    [Regulations].[Priority] AS [Priority],
    dbo.[ReportTemplate].*
  FROM dbo.[ReportTemplate]
  INNER JOIN dbo.[Report]
    ON dbo.[ReportTemplate].[ReportId] = dbo.[Report].[Id]
  INNER JOIN dbo.GetDerivedRegulations(@tenantId, @payrollId, @regulationDate, @createdBefore) AS [Regulations]
    ON dbo.[Report].[RegulationId] = [Regulations].[Id]
  -- active reports only
  WHERE dbo.[ReportTemplate].[Status] = 0
    AND dbo.[ReportTemplate].[Created] < @createdBefore
    AND (
      @reportNames IS NULL
      OR dbo.[Report].[Name] IN (
        SELECT value
        FROM OPENJSON(@reportNames)
        )
      )
    AND (
      @language IS NULL
      OR dbo.[ReportTemplate].[Language] = @language
      )
  -- derived order by sort order
  ORDER BY [Level] DESC,
    [Priority] DESC
END
GO


