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
-- fully denormalized: zero JOINs, all filters on CollectorResult columns
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
  -- better performance to indexed column of the collector name
  IF (@collectorCount = 1)
  BEGIN
    SELECT @collectorNameHash = CAST(value AS INT)
      FROM OPENJSON(@collectorNameHashes);

    -- zero-JOIN query: single collector optimization
    SELECT TOP (100) PERCENT cr.*
    FROM dbo.[CollectorResult] cr
    WHERE (cr.[TenantId] = @tenantId)
      AND (cr.[EmployeeId] = @employeeId)
      AND (
        @divisionId IS NULL
        OR cr.[DivisionId] = @divisionId
      )
      AND (
        @payrunJobId IS NULL
        OR cr.[PayrunJobId] = @payrunJobId
      )
      AND (
        @parentPayrunJobId IS NULL
        OR cr.[ParentJobId] = @parentPayrunJobId
      )
      AND (
        @collectorNameHashes IS NULL
        OR cr.[CollectorNameHash] = @collectorNameHash
      )
      AND (
        (@periodStart IS NULL AND @periodEnd IS NULL)
        OR cr.[Start] BETWEEN @periodStart AND @periodEnd
      )
      AND (
        @jobStatus IS NULL
        OR cr.[PayrunJobId] IN (
          SELECT pj.[Id] FROM dbo.[PayrunJob] pj
          WHERE pj.[Id] = cr.[PayrunJobId]
            AND pj.[JobStatus] & @jobStatus = pj.[JobStatus]
        )
      )
      AND (
        cr.[Forecast] IS NULL
        OR cr.[Forecast] = @forecast
      )
      AND (
        @evaluationDate IS NULL
        OR cr.[Created] <= @evaluationDate
      )
    ORDER BY cr.[Created]
  END
  ELSE
  BEGIN
    -- zero-JOIN query: multiple collectors
    SELECT TOP (100) PERCENT cr.*
    FROM dbo.[CollectorResult] cr
    WHERE (cr.[TenantId] = @tenantId)
      AND (cr.[EmployeeId] = @employeeId)
      AND (
        @divisionId IS NULL
        OR cr.[DivisionId] = @divisionId
      )
      AND (
        @payrunJobId IS NULL
        OR cr.[PayrunJobId] = @payrunJobId
      )
      AND (
        @parentPayrunJobId IS NULL
        OR cr.[ParentJobId] = @parentPayrunJobId
      )
      AND (
        @collectorNameHashes IS NULL
        OR cr.[CollectorNameHash] IN (
          SELECT value
          FROM OPENJSON(@collectorNameHashes)
        )
      )
      AND (
        (@periodStart IS NULL AND @periodEnd IS NULL)
        OR cr.[Start] BETWEEN @periodStart AND @periodEnd
      )
      AND (
        @jobStatus IS NULL
        OR cr.[PayrunJobId] IN (
          SELECT pj.[Id] FROM dbo.[PayrunJob] pj
          WHERE pj.[Id] = cr.[PayrunJobId]
            AND pj.[JobStatus] & @jobStatus = pj.[JobStatus]
        )
      )
      AND (
        cr.[Forecast] IS NULL
        OR cr.[Forecast] = @forecast
      )
      AND (
        @evaluationDate IS NULL
        OR cr.[Created] <= @evaluationDate
      )
    ORDER BY cr.[Created]
  END
END
GO
