SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

IF EXISTS (
    SELECT *
    FROM sysobjects
    WHERE id = object_id(N'[dbo].[GetConsolidatedPayrunResults]')
      AND OBJECTPROPERTY(id, N'IsProcedure') = 1
    )
BEGIN
  DROP PROCEDURE dbo.[GetConsolidatedPayrunResults]
END
GO

-- =============================================
-- Get consolidated payrun results from a time period
-- =============================================
CREATE PROCEDURE dbo.[GetConsolidatedPayrunResults]
  -- the tenant id
  @tenantId AS INT,
  -- the employee id
  @employeeId AS INT,
  -- the division id
  @divisionId AS INT = NULL,
  -- the result names: JSON array of VARCHAR(128)
  @names AS VARCHAR(MAX) = NULL,
  -- the period start hashes: JSON array of INT
  @periodStartHashes AS VARCHAR(MAX) = NULL,
  -- payrun job status (bit mask)
  @jobStatus AS INT = NULL,
  -- the forecast name
  @forecast AS VARCHAR(128) = NULL,
  -- evaluation date
  @evaluationDate AS DATETIME2(7) = NULL
AS
BEGIN
  -- SET NOCOUNT ON added to prevent extra result sets from
  -- interfering with SELECT statements
  SET NOCOUNT ON;

  SELECT * FROM (
    SELECT dbo.[PayrollResult].[EmployeeId],
      dbo.[PayrollResult].[DivisionId],
      dbo.[PayrollResult].[PayrunId],
      dbo.[PayrollResult].[PayrunJobId],
      dbo.[PayrunJob].[JobStatus],
      dbo.[PayrunJob].[Forecast],
      dbo.[PayrunResult].*,
      ROW_NUMBER() OVER (
        PARTITION BY 
            dbo.[PayrollResult].[PayrunId],
            dbo.[PayrunResult].[Name],
            dbo.[PayrunResult].[Start]
        ORDER BY
            dbo.[PayrunResult].[Created] DESC,
            dbo.[PayrunResult].[Id] DESC
        ) AS RowNumber
    FROM dbo.[PayrunResult]
    INNER JOIN dbo.[PayrollResult]
      ON dbo.[PayrollResult].[Id] = dbo.[PayrunResult].[PayrollResultId]
    INNER JOIN dbo.[PayrunJob]
      ON dbo.[PayrollResult].[PayrunJobId] = dbo.[PayrunJob].[Id]
    WHERE (dbo.[PayrunJob].[TenantId] = @tenantId)
      AND (dbo.[PayrollResult].[EmployeeId] = @employeeId)
      AND (
        @divisionId IS NULL
        OR dbo.[PayrollResult].[DivisionId] = @divisionId
        )
      AND (
        @periodStartHashes IS NULL
        OR dbo.[PayrunResult].[StartHash] IN (
          SELECT CAST(value AS INT)
          FROM OPENJSON(@periodStartHashes)
          )
        )
      AND (
        @names IS NULL
        OR LOWER(dbo.[PayrunResult].[Name]) IN (
          SELECT LOWER(value)
          FROM OPENJSON(@names)
          )
        )
      AND (
        @jobStatus IS NULL
        OR dbo.[PayrunJob].[JobStatus] & @jobStatus = dbo.[PayrunJob].[JobStatus]
        )
      AND (
        [PayrunJob].[Forecast] IS NULL
        OR [PayrunJob].[Forecast] = @forecast
        )
      AND (
        @evaluationDate IS NULL
        OR dbo.[PayrunResult].[Created] <= @evaluationDate
        )
    ) AS GroupPayrunResult
  WHERE RowNumber = 1;
END
GO


