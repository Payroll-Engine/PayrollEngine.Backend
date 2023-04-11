SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

IF EXISTS (
    SELECT *
    FROM sysobjects
    WHERE id = object_id(N'[dbo].[GetConsolidatedWageTypeCustomResults]')
      AND OBJECTPROPERTY(id, N'IsProcedure') = 1
    )
BEGIN
  DROP PROCEDURE dbo.[GetConsolidatedWageTypeCustomResults]
END
GO

-- =============================================
-- Get consolidated custom wage type results
-- =============================================
CREATE PROCEDURE dbo.[GetConsolidatedWageTypeCustomResults]
  -- the tenant id
  @tenantId AS INT,
  -- the employee id
  @employeeId AS INT,
  -- the division id
  @divisionId AS INT = NULL,
  -- the wage type number: JSON array of DECIMAL(28, 6)
  @wageTypeNumbers AS VARCHAR(MAX) = NULL,
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

  DECLARE @wageTypeNumber DECIMAL(28, 6);
  DECLARE @wageTypeCount INT;
  SELECT @wageTypeCount = COUNT(*) FROM OPENJSON(@wageTypeNumbers);

    -- special query for single wage type
  -- better perfomance to indexed column of the wage type number
  if (@wageTypeCount= 1)
  BEGIN
      SELECT @wageTypeNumber = CAST(value AS DECIMAL(28, 6))
          FROM OPENJSON(@wageTypeNumbers);

      SELECT * FROM (
        SELECT dbo.[PayrollResult].[EmployeeId],
          dbo.[PayrollResult].[DivisionId],
          dbo.[PayrollResult].[PayrunId],
          dbo.[PayrollResult].[PayrunJobId],
          dbo.[PayrunJob].[JobStatus],
          dbo.[PayrunJob].[Forecast],
          dbo.[WageTypeCustomResult].*,
          ROW_NUMBER() OVER (
            PARTITION BY
                dbo.[PayrollResult].[PayrunId],
                dbo.[WageTypeResult].[WageTypeNumber],
                dbo.[WageTypeCustomResult].[Start]
            ORDER BY
                dbo.[WageTypeCustomResult].[Created] DESC,
                dbo.[WageTypeCustomResult].[Id] DESC
            ) AS RowNumber
        FROM dbo.[WageTypeResult]
        INNER JOIN dbo.[PayrollResult]
          ON dbo.[PayrollResult].[Id] = dbo.[WageTypeResult].[PayrollResultId]
        INNER JOIN dbo.[WageTypeCustomResult]
          ON dbo.[WageTypeResult].[Id] = dbo.[WageTypeCustomResult].[WageTypeResultId]
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
            OR dbo.[WageTypeCustomResult].[StartHash] IN (
              SELECT CAST(value AS INT)
              FROM OPENJSON(@periodStartHashes)
              )
            )
          AND (
            dbo.[WageTypeCustomResult].[WageTypeNumber] = @wageTypeNumber
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
            OR dbo.[WageTypeCustomResult].[Created] <= @evaluationDate
            )
        ) AS GroupWageTypeResult
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
          dbo.[WageTypeCustomResult].*,
          ROW_NUMBER() OVER (
            PARTITION BY
                dbo.[PayrollResult].[PayrunId],
                dbo.[WageTypeResult].[WageTypeNumber],
                dbo.[WageTypeCustomResult].[Start]
            ORDER BY
                dbo.[WageTypeCustomResult].[Created] DESC,
                dbo.[WageTypeCustomResult].[Id] DESC
            ) AS RowNumber
        FROM dbo.[WageTypeResult]
        INNER JOIN dbo.[PayrollResult]
          ON dbo.[PayrollResult].[Id] = dbo.[WageTypeResult].[PayrollResultId]
        INNER JOIN dbo.[WageTypeCustomResult]
          ON dbo.[WageTypeResult].[Id] = dbo.[WageTypeCustomResult].[WageTypeResultId]
        INNER JOIN dbo.[PayrunJob]
          ON dbo.[PayrollResult].[PayrunJobId] = dbo.[PayrunJob].[Id]
        WHERE (dbo.[PayrollResult].[EmployeeId] = @employeeId)
          AND (
            @divisionId IS NULL
            OR dbo.[PayrollResult].[DivisionId] = @divisionId
            )
          AND (
            @periodStartHashes IS NULL
            OR dbo.[WageTypeCustomResult].[StartHash] IN (
              SELECT CAST(value AS INT)
              FROM OPENJSON(@periodStartHashes)
              )
            )
          AND (
            @wageTypeNumbers IS NULL
            OR dbo.[WageTypeCustomResult].[WageTypeNumber] IN (
              SELECT CAST(value AS DECIMAL(28, 6))
              FROM OPENJSON(@wageTypeNumbers)
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
            OR dbo.[WageTypeCustomResult].[Created] <= @evaluationDate
            )
        ) AS GroupWageTypeResult
       WHERE RowNumber = 1;
 END
END
GO


