SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

IF EXISTS (
    SELECT *
    FROM sysobjects
    WHERE id = object_id(N'[dbo].[GetDerivedReportParameters]')
      AND OBJECTPROPERTY(id, N'IsProcedure') = 1
    )
BEGIN
  DROP PROCEDURE dbo.[GetDerivedReportParameters]
END
GO

-- =============================================
-- Get the topmost derived report parameters (only active).
-- =============================================
CREATE PROCEDURE dbo.[GetDerivedReportParameters]
  -- the tenant
  @tenantId AS INT,
  -- the payroll
  @payrollId AS INT,
  -- the regulation valid from date
  @regulationDate AS DATETIME2(7),
  -- creation date
  @createdBefore AS DATETIME2(7),
  -- the report names: JSON array of VARCHAR(128)
  @reportNames AS VARCHAR(MAX) = NULL
AS
BEGIN
  -- SET NOCOUNT ON added to prevent extra result sets from
  -- interfering with SELECT statements
  SET NOCOUNT ON;

  -- select all active report parameters, using the order of report name and payroll level
  SELECT [Regulations].[Id] AS [RegulationId],
    [Regulations].[Level] AS [Level],
    [Regulations].[Priority] AS [Priority],
    dbo.[ReportParameter].*
  FROM dbo.[ReportParameter]
  INNER JOIN dbo.[Report]
    ON dbo.[ReportParameter].[ReportId] = dbo.[Report].[Id]
  INNER JOIN dbo.GetDerivedRegulations(@tenantId, @payrollId, @regulationDate, @createdBefore) AS [Regulations]
    ON dbo.[Report].[RegulationId] = [Regulations].[Id]
  -- active reports only
  WHERE dbo.[ReportParameter].[Status] = 0
    AND dbo.[ReportParameter].[Created] < @createdBefore
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


