SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

IF EXISTS (
    SELECT *
    FROM sysobjects
    WHERE id = object_id(N'[dbo].[GetWageTypeResults]')
      AND OBJECTPROPERTY(id, N'IsProcedure') = 1
    )
BEGIN
  DROP PROCEDURE dbo.[GetWageTypeResults]
END
GO

-- =============================================
-- Get employee wage type results from a time period
-- fully denormalized: zero JOINs, all filters on WageTypeResult columns
-- =============================================
CREATE PROCEDURE dbo.[GetWageTypeResults]
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
    SELECT TOP (100) PERCENT wtr.*
    FROM dbo.[WageTypeResult] wtr
    WHERE (wtr.[TenantId] = @tenantId)
      AND (wtr.[EmployeeId] = @employeeId)
      AND (
        @divisionId IS NULL
        OR wtr.[DivisionId] = @divisionId
      )
      AND (
        @payrunJobId IS NULL
        OR wtr.[PayrunJobId] = @payrunJobId
      )
      AND (
        @parentPayrunJobId IS NULL
        OR wtr.[ParentJobId] = @parentPayrunJobId
      )
      AND (
        wtr.[WageTypeNumber] = @wageTypeNumber
      )
      AND (
        (@periodStart IS NULL AND @periodEnd IS NULL)
        OR wtr.[Start] BETWEEN @periodStart AND @periodEnd
      )
      AND (
        @jobStatus IS NULL
        OR wtr.[PayrunJobId] IN (
          SELECT pj.[Id] FROM dbo.[PayrunJob] pj
          WHERE pj.[Id] = wtr.[PayrunJobId]
            AND pj.[JobStatus] & @jobStatus = pj.[JobStatus]
        )
      )
      AND (
        wtr.[Forecast] IS NULL
        OR wtr.[Forecast] = @forecast
      )
      AND (
        @evaluationDate IS NULL
        OR wtr.[Created] <= @evaluationDate
      )
    ORDER BY wtr.[Created]
  END
  ELSE
  BEGIN
    -- zero-JOIN query: multiple wage types
    SELECT TOP (100) PERCENT wtr.*
    FROM dbo.[WageTypeResult] wtr
    WHERE (wtr.[TenantId] = @tenantId)
      AND (wtr.[EmployeeId] = @employeeId)
      AND (
        @divisionId IS NULL
        OR wtr.[DivisionId] = @divisionId
      )
      AND (
        @payrunJobId IS NULL
        OR wtr.[PayrunJobId] = @payrunJobId
      )
      AND (
        @parentPayrunJobId IS NULL
        OR wtr.[ParentJobId] = @parentPayrunJobId
      )
      AND (
        @wageTypeNumbers IS NULL
        OR wtr.[WageTypeNumber] IN (
          SELECT CAST(value AS DECIMAL(28, 6))
          FROM OPENJSON(@wageTypeNumbers)
        )
      )
      AND (
        (@periodStart IS NULL AND @periodEnd IS NULL)
        OR wtr.[Start] BETWEEN @periodStart AND @periodEnd
      )
      AND (
        @jobStatus IS NULL
        OR wtr.[PayrunJobId] IN (
          SELECT pj.[Id] FROM dbo.[PayrunJob] pj
          WHERE pj.[Id] = wtr.[PayrunJobId]
            AND pj.[JobStatus] & @jobStatus = pj.[JobStatus]
        )
      )
      AND (
        wtr.[Forecast] IS NULL
        OR wtr.[Forecast] = @forecast
      )
      AND (
        @evaluationDate IS NULL
        OR wtr.[Created] <= @evaluationDate
      )
    ORDER BY wtr.[Created]
  END
END
GO
