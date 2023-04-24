SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

IF EXISTS (
    SELECT *
    FROM sysobjects
    WHERE id = object_id(N'[dbo].[GetConsolidatedCollectorResults]')
      AND OBJECTPROPERTY(id, N'IsProcedure') = 1
    )
BEGIN
  DROP PROCEDURE dbo.[GetConsolidatedCollectorResults]
END
GO

-- =============================================
-- Get employee collector results from a time period
-- =============================================
CREATE PROCEDURE dbo.[GetConsolidatedCollectorResults]
  -- the tenant id
  @tenantId AS INT,
  -- the employee id
  @employeeId AS INT,
  -- the division id
  @divisionId AS INT = NULL,
  -- the collector name hashes: JSON array of INT
  @collectorNameHashes AS VARCHAR(MAX) = NULL,
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

  DECLARE @collectorNameHash INT;
  DECLARE @collectorCount INT;
  SELECT @collectorCount = COUNT(*) FROM OPENJSON(@collectorNameHashes);

  -- special query for single collector
  -- better perfomance to indexed column of the collector name
  if (@collectorCount= 1)
  BEGIN
    SELECT @collectorNameHash = CAST(value AS INT)
        FROM OPENJSON(@collectorNameHashes);

    SELECT * FROM (
    SELECT dbo.[PayrollResult].[EmployeeId],
        dbo.[PayrollResult].[DivisionId],
        dbo.[PayrollResult].[PayrunId],
        dbo.[PayrollResult].[PayrunJobId],
        dbo.[PayrunJob].[JobStatus],
        dbo.[PayrunJob].[Forecast],
        dbo.[CollectorResult].*,
        ROW_NUMBER() OVER (
        PARTITION BY
            dbo.[PayrollResult].[PayrunId],
            dbo.[CollectorResult].[CollectorName],
            dbo.[CollectorResult].[Start]
        ORDER BY
            dbo.[CollectorResult].[Created] DESC,
            dbo.[CollectorResult].[Id] DESC
        ) AS RowNumber
    FROM dbo.[CollectorResult]
    INNER JOIN dbo.[PayrollResult]
        ON dbo.[PayrollResult].[Id] = dbo.[CollectorResult].[PayrollResultId]
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
            OR dbo.[CollectorResult].[StartHash] IN (
                SELECT CAST(value AS INT)
                FROM OPENJSON(@periodStartHashes)
                )
        )
        AND (
            @collectorNameHashes IS NULL
            OR dbo.[CollectorResult].[CollectorNameHash] = @collectorNameHash
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
            OR dbo.[CollectorResult].[Created] <= @evaluationDate
        )
    ) AS GroupCollectorResult
    WHERE RowNumber = 1;
END
ELSE
BEGIN
    SELECT * FROM (
    SELECT dbo.[PayrollResult].[EmployeeId],
        dbo.[PayrollResult].[DivisionId],
        dbo.[PayrollResult].[PayrunId],
        dbo.[PayrollResult].[PayrunJobId],
        dbo.[PayrunJob].[JobStatus],
        dbo.[PayrunJob].[Forecast],
        dbo.[CollectorResult].*,
        ROW_NUMBER() OVER (
        PARTITION BY
            dbo.[PayrollResult].[PayrunId],
            dbo.[CollectorResult].[CollectorName],
            dbo.[CollectorResult].[Start]
        ORDER BY
            dbo.[CollectorResult].[Created] DESC,
            dbo.[CollectorResult].[Id] DESC
        ) AS RowNumber
    FROM dbo.[CollectorResult]
    INNER JOIN dbo.[PayrollResult]
        ON dbo.[PayrollResult].[Id] = dbo.[CollectorResult].[PayrollResultId]
    INNER JOIN dbo.[PayrunJob]
        ON dbo.[PayrollResult].[PayrunJobId] = dbo.[PayrunJob].[Id]
    WHERE (dbo.[PayrollResult].[EmployeeId] = @employeeId)
        AND (
            @divisionId IS NULL
            OR dbo.[PayrollResult].[DivisionId] = @divisionId
        )
        AND (
            @periodStartHashes IS NULL
            OR dbo.[CollectorResult].[StartHash] IN (
                SELECT CAST(value AS INT)
                FROM OPENJSON(@periodStartHashes)
                )
        )
        AND (
            @collectorNameHashes IS NULL
            OR dbo.[CollectorResult].[CollectorNameHash] IN (
                SELECT CAST(value AS INT)
                FROM OPENJSON(@collectorNameHashes)
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
            OR dbo.[CollectorResult].[Created] <= @evaluationDate
        )
    ) AS GroupCollectorResult
    WHERE RowNumber = 1;
END
END
GO


