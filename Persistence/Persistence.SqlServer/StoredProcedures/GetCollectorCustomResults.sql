SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

IF EXISTS (
    SELECT *
    FROM sysobjects
    WHERE id = object_id(N'[dbo].[GetCollectorCustomResults]')
      AND OBJECTPROPERTY(id, N'IsProcedure') = 1
    )
BEGIN
  DROP PROCEDURE dbo.[GetCollectorCustomResults]
END
GO

-- =============================================
-- Get employee collector custom results from a time period
-- fully denormalized: zero JOINs, all filters on CollectorCustomResult columns
-- =============================================
CREATE PROCEDURE dbo.[GetCollectorCustomResults]
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
  -- better performance to indexed column of the collector name
  IF (@collectorCount = 1)
  BEGIN
    SELECT @collectorNameHash = CAST(value AS INT)
      FROM OPENJSON(@collectorNameHashes);

    -- zero-JOIN query: single collector optimization
    SELECT TOP (100) PERCENT ccr.*
    FROM dbo.[CollectorCustomResult] ccr
    WHERE (ccr.[TenantId] = @tenantId)
      AND (ccr.[EmployeeId] = @employeeId)
      AND (
        @divisionId IS NULL
        OR ccr.[DivisionId] = @divisionId
      )
      AND (
        @payrunJobId IS NULL
        OR ccr.[PayrunJobId] = @payrunJobId
      )
      AND (
        @parentPayrunJobId IS NULL
        OR ccr.[ParentJobId] = @parentPayrunJobId
      )
      AND (
        @collectorNameHashes IS NULL
        OR ccr.[CollectorNameHash] = @collectorNameHash
      )
      AND (
        (@periodStart IS NULL AND @periodEnd IS NULL)
        OR ccr.[Start] BETWEEN @periodStart AND @periodEnd
      )
      AND (
        @jobStatus IS NULL
        OR ccr.[PayrunJobId] IN (
          SELECT pj.[Id] FROM dbo.[PayrunJob] pj
          WHERE pj.[Id] = ccr.[PayrunJobId]
            AND pj.[JobStatus] & @jobStatus = pj.[JobStatus]
        )
      )
      AND (
        ccr.[Forecast] IS NULL
        OR ccr.[Forecast] = @forecast
      )
      AND (
        @evaluationDate IS NULL
        OR ccr.[Created] <= @evaluationDate
      )
    ORDER BY ccr.[Created]
  END
  ELSE
  BEGIN
    -- zero-JOIN query: multiple collectors
    SELECT TOP (100) PERCENT ccr.*
    FROM dbo.[CollectorCustomResult] ccr
    WHERE (ccr.[TenantId] = @tenantId)
      AND (ccr.[EmployeeId] = @employeeId)
      AND (
        @divisionId IS NULL
        OR ccr.[DivisionId] = @divisionId
      )
      AND (
        @payrunJobId IS NULL
        OR ccr.[PayrunJobId] = @payrunJobId
      )
      AND (
        @parentPayrunJobId IS NULL
        OR ccr.[ParentJobId] = @parentPayrunJobId
      )
      AND (
        @collectorNameHashes IS NULL
        OR ccr.[CollectorNameHash] IN (
          SELECT value
          FROM OPENJSON(@collectorNameHashes)
        )
      )
      AND (
        (@periodStart IS NULL AND @periodEnd IS NULL)
        OR ccr.[Start] BETWEEN @periodStart AND @periodEnd
      )
      AND (
        @jobStatus IS NULL
        OR ccr.[PayrunJobId] IN (
          SELECT pj.[Id] FROM dbo.[PayrunJob] pj
          WHERE pj.[Id] = ccr.[PayrunJobId]
            AND pj.[JobStatus] & @jobStatus = pj.[JobStatus]
        )
      )
      AND (
        ccr.[Forecast] IS NULL
        OR ccr.[Forecast] = @forecast
      )
      AND (
        @evaluationDate IS NULL
        OR ccr.[Created] <= @evaluationDate
      )
    ORDER BY ccr.[Created]
  END
END
GO
