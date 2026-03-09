SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

IF EXISTS (
    SELECT *
    FROM sysobjects
    WHERE id = object_id(N'[dbo].[GetConsolidatedWageTypeResults]')
      AND OBJECTPROPERTY(id, N'IsProcedure') = 1
    )
BEGIN
  DROP PROCEDURE dbo.[GetConsolidatedWageTypeResults]
END
GO

-- =============================================
-- Get employee wage type results from a time period
-- fully denormalized: zero JOINs, all filters on WageTypeResult columns
-- =============================================
CREATE PROCEDURE dbo.[GetConsolidatedWageTypeResults]
    @tenantId AS INT,
    @employeeId AS INT,
    @divisionId AS INT = NULL,
    @wageTypeNumbers AS VARCHAR(MAX) = NULL,
    @periodStartHashes AS VARCHAR(MAX) = NULL,
    @jobStatus AS INT = NULL,
    @forecast AS VARCHAR(128) = NULL,
    @evaluationDate AS DATETIME2(7) = NULL,
    @noRetro AS BIT = 0,
    @excludeParentJobId AS INT = NULL
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @wageTypeNumber DECIMAL(28, 6);
    DECLARE @wageTypeCount INT;
    SELECT @wageTypeCount = COUNT(*) FROM OPENJSON(@wageTypeNumbers);

    IF (@wageTypeCount = 1)
    BEGIN
        SELECT @wageTypeNumber = CAST(value AS DECIMAL(28, 6))
        FROM OPENJSON(@wageTypeNumbers);
    END;

    -- single-hash fast path: equality seek on StartHash
    DECLARE @startHash INT;
    DECLARE @startHashCount INT;
    SELECT @startHashCount = COUNT(*) FROM OPENJSON(@periodStartHashes);

    IF (@startHashCount = 1)
    BEGIN
        SELECT @startHash = CAST(value AS INT)
        FROM OPENJSON(@periodStartHashes);
    END;

    -- Phase 1: select winning IDs via index-only scan
    -- Index key order: (TenantId, EmployeeId, StartHash, WageTypeNumber)
    -- → seeks directly to the period, constant cost regardless of history
    ;WITH Winners AS (
        SELECT
            wtr.[Id],
            ROW_NUMBER() OVER (
                PARTITION BY wtr.[WageTypeNumber], wtr.[Start]
                ORDER BY wtr.[Created] DESC, wtr.[Id] DESC
            ) AS RowNumber
        FROM dbo.[WageTypeResult] wtr
        WHERE wtr.[TenantId] = @tenantId
          AND wtr.[EmployeeId] = @employeeId
          -- period filter: single hash → equality seek; multiple → IN list
          AND (
              (@startHashCount = 1 AND wtr.[StartHash] = @startHash)
              OR (@startHashCount > 1 AND wtr.[StartHash] IN (
                  SELECT CAST(value AS INT) FROM OPENJSON(@periodStartHashes)))
          )
          AND (@divisionId IS NULL OR wtr.[DivisionId] = @divisionId)
          AND (@wageTypeNumbers IS NULL OR @wageTypeCount = 0
               OR (@wageTypeCount = 1 AND wtr.[WageTypeNumber] = @wageTypeNumber)
               OR (@wageTypeCount > 1 AND wtr.[WageTypeNumber] IN (
                   SELECT CAST(value AS DECIMAL(28, 6)) FROM OPENJSON(@wageTypeNumbers))))
          AND (@evaluationDate IS NULL OR wtr.[Created] <= @evaluationDate)
          AND (@jobStatus IS NULL
               OR wtr.[PayrunJobId] IN (
                   SELECT pj.[Id] FROM dbo.[PayrunJob] pj
                   WHERE pj.[JobStatus] & @jobStatus = pj.[JobStatus]))
          AND (wtr.[Forecast] IS NULL OR wtr.[Forecast] = @forecast)
          AND (@noRetro = 0 OR wtr.[ParentJobId] IS NULL)
          AND (@excludeParentJobId IS NULL OR wtr.[ParentJobId] IS NULL
               OR wtr.[ParentJobId] <> @excludeParentJobId)
    )
    -- Phase 2: key lookup only for winning rows
    SELECT wtr.*
    FROM dbo.[WageTypeResult] wtr
    INNER JOIN Winners w ON w.[Id] = wtr.[Id]
    WHERE w.RowNumber = 1
    OPTION (RECOMPILE);
END
GO