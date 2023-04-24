SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

IF EXISTS (
    SELECT *
    FROM sysobjects
    WHERE id = object_id(N'[dbo].[GetCollectorResults]')
      AND OBJECTPROPERTY(id, N'IsProcedure') = 1
    )
BEGIN
  DROP PROCEDURE dbo.[GetCollectorResults]
END
GO

-- =============================================
-- Get employee collector results from a time period
-- =============================================
CREATE PROCEDURE dbo.[GetCollectorResults]
  -- the tenant id
  @tenantId AS INT,
  -- the employee id
  @employeeId AS INT,
  -- the division id
  @divisionId AS INT = NULL,
  -- the payrun job id
  @payrunJobId AS INT = NULL,
  -- the parent payrun job id
  @parentPayrunJobId AS INT = NULL,
  -- the collector name hashes: JSON array of INT
  @collectorNameHashes AS VARCHAR(MAX) = NULL,
  -- period start
  @periodStart AS DATETIME2(7) = NULL,
  -- period end
  @periodEnd AS DATETIME2(7) = NULL,
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

    SELECT TOP (100) PERCENT dbo.[CollectorResult].*
    FROM dbo.[PayrollResult]
    INNER JOIN dbo.[PayrunJob]
    ON dbo.[PayrollResult].[PayrunJobId] = dbo.[PayrunJob].[Id]
    INNER JOIN dbo.[CollectorResult]
    ON dbo.[PayrollResult].[Id] = dbo.[CollectorResult].[PayrollResultId]
    WHERE (dbo.[PayrollResult].[TenantId] = @tenantId)
        AND (dbo.[PayrollResult].[EmployeeId] = @employeeId)
        AND (
            @divisionId IS NULL
            OR dbo.[PayrollResult].[DivisionId] = @divisionId
        )
        AND (
            @payrunJobId IS NULL
            OR dbo.[PayrunJob].[Id] = @payrunJobId
        )
        AND (
            @parentPayrunJobId IS NULL
            OR dbo.[PayrunJob].[ParentJobId] = @parentPayrunJobId
        )
        AND (
            @collectorNameHashes IS NULL
            OR dbo.[CollectorResult].[CollectorNameHash] = @collectorNameHash
        )
        AND (
            (@periodStart IS NULL AND @periodEnd IS NULL)
            OR
            dbo.[PayrunJob].[PeriodStart] BETWEEN @periodStart AND @periodEnd
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
    ORDER BY dbo.[CollectorResult].[Created]
END
ELSE
BEGIN
    SELECT TOP (100) PERCENT dbo.[CollectorResult].*
    FROM dbo.[PayrollResult]
    INNER JOIN dbo.[PayrunJob]
    ON dbo.[PayrollResult].[PayrunJobId] = dbo.[PayrunJob].[Id]
    INNER JOIN dbo.[CollectorResult]
    ON dbo.[PayrollResult].[Id] = dbo.[CollectorResult].[PayrollResultId]
    WHERE (dbo.[PayrollResult].[TenantId] = @tenantId)
        AND (dbo.[PayrollResult].[EmployeeId] = @employeeId)
        AND (
            @divisionId IS NULL
            OR dbo.[PayrollResult].[DivisionId] = @divisionId
            )
        AND (
            @payrunJobId IS NULL
            OR dbo.[PayrunJob].[Id] = @payrunJobId
            )
        AND (
            @parentPayrunJobId IS NULL
            OR dbo.[PayrunJob].[ParentJobId] = @parentPayrunJobId
            )
        AND (
            @collectorNameHashes IS NULL
            OR dbo.[CollectorResult].[CollectorNameHash] IN (
                SELECT value
                FROM OPENJSON(@collectorNameHashes)
            )
        )
        AND (
            (@periodStart IS NULL AND @periodEnd IS NULL)
            OR
            dbo.[PayrunJob].[PeriodStart] BETWEEN @periodStart AND @periodEnd
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
    ORDER BY dbo.[CollectorResult].[Created]
END
END
GO


