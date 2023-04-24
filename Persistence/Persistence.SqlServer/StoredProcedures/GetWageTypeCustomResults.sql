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
  -- better perfomance to indexed column of the wage type number
  if (@wageTypeCount= 1)
  BEGIN
      SELECT @wageTypeNumber = CAST(value AS DECIMAL(28, 6))
          FROM OPENJSON(@wageTypeNumbers);

    SELECT TOP (100) PERCENT dbo.[WageTypeCustomResult].*
    FROM dbo.[PayrollResult]
    INNER JOIN dbo.[PayrunJob]
    ON dbo.[PayrollResult].[PayrunJobId] = dbo.[PayrunJob].[Id]
    INNER JOIN dbo.[WageTypeResult]
    ON dbo.[PayrollResult].[Id] = dbo.[WageTypeResult].[PayrollResultId]
    INNER JOIN dbo.[WageTypeCustomResult]
    ON dbo.[WageTypeResult].[Id] = dbo.[WageTypeCustomResult].[WageTypeResultId]
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
            dbo.[WageTypeResult].[WageTypeNumber] = @wageTypeNumber
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
            OR dbo.[WageTypeResult].[Created] <= @evaluationDate
        )
    ORDER BY dbo.[WageTypeResult].[Created]
END
ELSE
BEGIN
    SELECT TOP (100) PERCENT dbo.[WageTypeCustomResult].*
    FROM dbo.[PayrollResult]
    INNER JOIN dbo.[PayrunJob]
    ON dbo.[PayrollResult].[PayrunJobId] = dbo.[PayrunJob].[Id]
    INNER JOIN dbo.[WageTypeResult]
    ON dbo.[PayrollResult].[Id] = dbo.[WageTypeResult].[PayrollResultId]
    INNER JOIN dbo.[WageTypeCustomResult]
    ON dbo.[WageTypeResult].[Id] = dbo.[WageTypeCustomResult].[WageTypeResultId]
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
            @wageTypeNumbers IS NULL
            OR dbo.[WageTypeResult].[WageTypeNumber] IN (
            SELECT CAST(value AS DECIMAL(28, 6))
            FROM OPENJSON(@wageTypeNumbers)
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
            OR dbo.[WageTypeResult].[Created] <= @evaluationDate
        )
    ORDER BY dbo.[WageTypeResult].[Created]
END
END
GO


