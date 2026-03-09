SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

IF EXISTS (
    SELECT *
    FROM sysobjects
    WHERE id = object_id(N'[dbo].[GetWageTypeCustomResults]')
      AND OBJECTPROPERTY(id, N'IsProcedure') = 1
    )
BEGIN
  DROP PROCEDURE dbo.[GetWageTypeCustomResults]
END
GO

-- =============================================
-- Get employee wage type custom results from a time period
-- fully denormalized: zero JOINs, all filters on WageTypeCustomResult columns
-- =============================================
CREATE PROCEDURE dbo.[GetWageTypeCustomResults]
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
  -- the wage type numbers: JSON array of DECIMAL(28, 6)
  @wageTypeNumbers AS VARCHAR(MAX) = NULL,
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

  DECLARE @wageTypeNumber DECIMAL(28, 6);
  DECLARE @wageTypeCount INT;
  SELECT @wageTypeCount = COUNT(*) FROM OPENJSON(@wageTypeNumbers);

  -- special query for single wage type
  -- better performance to indexed column of the wage type number
  IF (@wageTypeCount = 1)
  BEGIN
    SELECT @wageTypeNumber = CAST(value AS DECIMAL(28, 6))
      FROM OPENJSON(@wageTypeNumbers);

    -- zero-JOIN query: single wage type optimization
    SELECT TOP (100) PERCENT wtcr.*
    FROM dbo.[WageTypeCustomResult] wtcr
    WHERE (wtcr.[TenantId] = @tenantId)
      AND (wtcr.[EmployeeId] = @employeeId)
      AND (
        @divisionId IS NULL
        OR wtcr.[DivisionId] = @divisionId
      )
      AND (
        @payrunJobId IS NULL
        OR wtcr.[PayrunJobId] = @payrunJobId
      )
      AND (
        @parentPayrunJobId IS NULL
        OR wtcr.[ParentJobId] = @parentPayrunJobId
      )
      AND (
        wtcr.[WageTypeNumber] = @wageTypeNumber
      )
      AND (
        (@periodStart IS NULL AND @periodEnd IS NULL)
        OR wtcr.[Start] BETWEEN @periodStart AND @periodEnd
      )
      AND (
        @jobStatus IS NULL
        OR wtcr.[PayrunJobId] IN (
          SELECT pj.[Id] FROM dbo.[PayrunJob] pj
          WHERE pj.[Id] = wtcr.[PayrunJobId]
            AND pj.[JobStatus] & @jobStatus = pj.[JobStatus]
        )
      )
      AND (
        wtcr.[Forecast] IS NULL
        OR wtcr.[Forecast] = @forecast
      )
      AND (
        @evaluationDate IS NULL
        OR wtcr.[Created] <= @evaluationDate
      )
    ORDER BY wtcr.[Created]
  END
  ELSE
  BEGIN
    -- zero-JOIN query: multiple wage types
    SELECT TOP (100) PERCENT wtcr.*
    FROM dbo.[WageTypeCustomResult] wtcr
    WHERE (wtcr.[TenantId] = @tenantId)
      AND (wtcr.[EmployeeId] = @employeeId)
      AND (
        @divisionId IS NULL
        OR wtcr.[DivisionId] = @divisionId
      )
      AND (
        @payrunJobId IS NULL
        OR wtcr.[PayrunJobId] = @payrunJobId
      )
      AND (
        @parentPayrunJobId IS NULL
        OR wtcr.[ParentJobId] = @parentPayrunJobId
      )
      AND (
        @wageTypeNumbers IS NULL
        OR wtcr.[WageTypeNumber] IN (
          SELECT CAST(value AS DECIMAL(28, 6))
          FROM OPENJSON(@wageTypeNumbers)
        )
      )
      AND (
        (@periodStart IS NULL AND @periodEnd IS NULL)
        OR wtcr.[Start] BETWEEN @periodStart AND @periodEnd
      )
      AND (
        @jobStatus IS NULL
        OR wtcr.[PayrunJobId] IN (
          SELECT pj.[Id] FROM dbo.[PayrunJob] pj
          WHERE pj.[Id] = wtcr.[PayrunJobId]
            AND pj.[JobStatus] & @jobStatus = pj.[JobStatus]
        )
      )
      AND (
        wtcr.[Forecast] IS NULL
        OR wtcr.[Forecast] = @forecast
      )
      AND (
        @evaluationDate IS NULL
        OR wtcr.[Created] <= @evaluationDate
      )
    ORDER BY wtcr.[Created]
  END
END
GO
