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
-- fully denormalized: zero JOINs, all filters on PayrunResult columns
-- =============================================
CREATE PROCEDURE dbo.[GetConsolidatedPayrunResults]
    @tenantId AS INT,
    @employeeId AS INT,
    @divisionId AS INT = NULL,
    @names AS VARCHAR(MAX) = NULL,
    @periodStartHashes AS VARCHAR(MAX) = NULL,
    @jobStatus AS INT = NULL,
    @forecast AS VARCHAR(128) = NULL,
    @evaluationDate AS DATETIME2(7) = NULL,
    @noRetro AS BIT = 0,
    @excludeParentJobId AS INT = NULL
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @name VARCHAR(128);
    DECLARE @nameCount INT;
    SELECT @nameCount = COUNT(*) FROM OPENJSON(@names);

    IF (@nameCount = 1)
    BEGIN
        SELECT @name = CAST(value AS VARCHAR(128))
        FROM OPENJSON(@names);
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
    -- Index key order: (TenantId, EmployeeId, StartHash, Name)
    -- → seeks directly to the period, constant cost regardless of history
    ;WITH Winners AS (
        SELECT
            r.[Id],
            ROW_NUMBER() OVER (
                PARTITION BY r.[Name], r.[Start]
                ORDER BY r.[Created] DESC, r.[Id] DESC
            ) AS RowNumber
        FROM dbo.[PayrunResult] r
        WHERE r.[TenantId] = @tenantId
          AND r.[EmployeeId] = @employeeId
          -- period filter: single hash → equality seek; multiple → IN list
          AND (
              (@startHashCount = 1 AND r.[StartHash] = @startHash)
              OR (@startHashCount > 1 AND r.[StartHash] IN (
                  SELECT CAST(value AS INT) FROM OPENJSON(@periodStartHashes)))
          )
          AND (@divisionId IS NULL OR r.[DivisionId] = @divisionId)
          AND (@names IS NULL OR @nameCount = 0
               OR (@nameCount = 1 AND r.[Name] = @name)
               OR (@nameCount > 1 AND r.[Name] IN (
                   SELECT CAST(value AS VARCHAR(128)) FROM OPENJSON(@names))))
          AND (@evaluationDate IS NULL OR r.[Created] <= @evaluationDate)
          AND (@jobStatus IS NULL
               OR r.[PayrunJobId] IN (
                   SELECT pj.[Id] FROM dbo.[PayrunJob] pj
                   WHERE pj.[JobStatus] & @jobStatus = pj.[JobStatus]))
          AND (r.[Forecast] IS NULL OR r.[Forecast] = @forecast)
          AND (@noRetro = 0 OR r.[ParentJobId] IS NULL)
          AND (@excludeParentJobId IS NULL OR r.[ParentJobId] IS NULL
               OR r.[ParentJobId] <> @excludeParentJobId)
    )
    -- Phase 2: key lookup only for winning rows
    SELECT r.*
    FROM dbo.[PayrunResult] r
    INNER JOIN Winners w ON w.[Id] = r.[Id]
    WHERE w.RowNumber = 1
    OPTION (RECOMPILE);
END
GO

