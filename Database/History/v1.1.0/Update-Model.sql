USE [PayrollEngine];
GO

SET XACT_ABORT ON
GO

-- =============================================================================
-- VERSION CHECK
-- Guard: abort if the schema is not at version 1.0.0
-- =============================================================================
IF OBJECT_ID('dbo.Version') IS NULL BEGIN
    RAISERROR('Schema not found: dbo.Version does not exist. Run Create-Model.sql first.', 16, 1)
    SET NOEXEC ON
END
GO

DECLARE @MajorVersion int, @MinorVersion int, @SubVersion int
SELECT TOP 1
    @MajorVersion = MajorVersion,
    @MinorVersion = MinorVersion,
    @SubVersion   = SubVersion
FROM dbo.[Version]
ORDER BY MajorVersion DESC, MinorVersion DESC, SubVersion DESC

IF @MajorVersion <> 1 OR @MinorVersion <> 0 OR @SubVersion <> 0 BEGIN
    DECLARE @ActualVersion NVARCHAR(20) =
        CAST(ISNULL(@MajorVersion, -1) AS NVARCHAR) + '.' +
        CAST(ISNULL(@MinorVersion, -1) AS NVARCHAR) + '.' +
        CAST(ISNULL(@SubVersion,   -1) AS NVARCHAR)
    RAISERROR('Version mismatch: expected 1.0.0, found %s', 16, 1, @ActualVersion)
    SET NOEXEC ON
END
GO

BEGIN TRANSACTION
GO

-- =============================================================================
-- TABLE CHANGES
-- =============================================================================

-- (none in this release)

-- =============================================================================
-- INDEX CHANGES
-- =============================================================================

-- (none in this release)

-- =============================================================================
-- FUNCTION CHANGES
-- =============================================================================

-- GetDerivedRegulations: RegulationShare now matched by regulation NAME (via JOIN)
-- instead of version-specific ProviderRegulationId.
--
-- Bug: When multiple versions of a regulation exist (e.g. US.Payroll.Data.Federal.FICA
-- for 2025 and 2026), ExchangeImport creates a RegulationShare for whichever version
-- GetRegulationAsync returns first (often the older one). GetDerivedRegulations then
-- fails to find a share for the other version, silently blocking cross-tenant lookup
-- access (e.g. FicaParameters for 2026 when share points to 2025 Id).
--
-- Fix: The EXISTS subquery joins through Regulation to match by Name rather than Id.
-- A single RegulationShare entry for any version of a regulation now grants access
-- to ALL versions of that regulation family. This aligns with the business intent:
-- sharing a regulation by name, not by a specific version Id.
IF OBJECT_ID('[dbo].[GetDerivedRegulations]') IS NOT NULL
    DROP FUNCTION [dbo].[GetDerivedRegulations];
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Get all active derived regulation ids from the payroll.
-- IsolationLevel < Write (< 3) means Consolidation-only — not a payroll layer.
-- Only shares with IsolationLevel >= Write (3) are eligible as payroll layers.
-- =============================================
CREATE FUNCTION [dbo].[GetDerivedRegulations] (
  @tenantId      AS INT,
  @payrollId     AS INT,
  @regulationDate AS DATETIME2(7),
  @createdBefore  AS DATETIME2(7)
  )
RETURNS TABLE
AS
RETURN (
    WITH GroupRegulation AS (
        SELECT [Regulation].[Id],
          [PayrollLayer].[Level],
          [PayrollLayer].[Priority],
          ROW_NUMBER() OVER (
            PARTITION BY [PayrollLayer].[Id],
            [Regulation].[Name] ORDER BY [Regulation].[ValidFrom] DESC,
              [Regulation].[Created] DESC
            ) AS RowNumber
        FROM [PayrollLayer]
        INNER JOIN [Regulation]
          ON [PayrollLayer].[RegulationName] = [Regulation].[Name]
        WHERE [Regulation].[Status] = 0
          AND (
            [Regulation].[TenantId] = @tenantId
            OR (
              [Regulation].[SharedRegulation] = 1
              AND EXISTS (
                -- Match by regulation NAME so a single RegulationShare entry covers all
                -- ValidFrom versions of the same regulation family (e.g. 2025 and 2026).
                SELECT 1
                FROM [dbo].[RegulationShare] rs
                INNER JOIN [dbo].[Regulation] rp ON rs.[ProviderRegulationId] = rp.[Id]
                WHERE rp.[Name]             = [Regulation].[Name]
                  AND rs.[ConsumerTenantId] = @tenantId
                  AND rs.[IsolationLevel]   >= 3  -- TenantIsolationLevel.Write
              )
            )
          )
          AND [Regulation].[Created] <= @createdBefore
          AND (
            [Regulation].[ValidFrom] IS NULL
            OR [Regulation].[ValidFrom] <= @regulationDate
            )
          AND [PayrollLayer].[Status] = 0
          AND [PayrollLayer].[PayrollId] = @payrollId
        )
    SELECT *
    FROM GroupRegulation
    WHERE RowNumber = 1
    )
GO

-- =============================================================================
-- STORED PROCEDURE CHANGES
-- =============================================================================

-- Result procedures: jobStatus filter uses an exact match instead of a bitwise subset match.
--
-- Bug: PayrunJobStatus is a sequential (non-flags) enum. The filter
-- [JobStatus] & @jobStatus = [JobStatus] matched every status whose value is a bit-subset
-- of the requested status: Complete (3) also matched Draft/Release/Process, Forecast (4)
-- also matched Draft. Open correction jobs could override the last completed result.
--
-- Fix: [JobStatus] = @jobStatus in all wage type, collector and payrun result procedures.

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
            AND pj.[JobStatus] = @jobStatus
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
            AND pj.[JobStatus] = @jobStatus
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
            AND pj.[JobStatus] = @jobStatus
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
            AND pj.[JobStatus] = @jobStatus
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
            AND pj.[JobStatus] = @jobStatus
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
            AND pj.[JobStatus] = @jobStatus
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
            AND pj.[JobStatus] = @jobStatus
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
            AND pj.[JobStatus] = @jobStatus
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
            r.[Id],
            ROW_NUMBER() OVER (
                PARTITION BY r.[WageTypeNumber], r.[Start]
                ORDER BY r.[Created] DESC, r.[Id] DESC
            ) AS RowNumber
        FROM dbo.[WageTypeResult] r
        WHERE r.[TenantId] = @tenantId
          AND r.[EmployeeId] = @employeeId
          -- period filter: single hash → equality seek; multiple → IN list
          AND (
              (@startHashCount = 1 AND r.[StartHash] = @startHash)
              OR (@startHashCount > 1 AND r.[StartHash] IN (
                  SELECT CAST(value AS INT) FROM OPENJSON(@periodStartHashes)))
          )
          AND (@divisionId IS NULL OR r.[DivisionId] = @divisionId)
          AND (@wageTypeNumbers IS NULL OR @wageTypeCount = 0
               OR (@wageTypeCount = 1 AND r.[WageTypeNumber] = @wageTypeNumber)
               OR (@wageTypeCount > 1 AND r.[WageTypeNumber] IN (
                   SELECT CAST(value AS DECIMAL(28, 6)) FROM OPENJSON(@wageTypeNumbers))))
          AND (@evaluationDate IS NULL OR r.[Created] <= @evaluationDate)
          AND (@jobStatus IS NULL
               OR r.[PayrunJobId] IN (
                   SELECT pj.[Id] FROM dbo.[PayrunJob] pj
                   WHERE pj.[JobStatus] = @jobStatus))
          AND (r.[Forecast] IS NULL OR r.[Forecast] = @forecast)
          AND (@noRetro = 0 OR r.[ParentJobId] IS NULL)
          AND (@excludeParentJobId IS NULL OR r.[ParentJobId] IS NULL
               OR r.[ParentJobId] <> @excludeParentJobId)
    )
    -- Phase 2: key lookup only for winning rows
    SELECT r.*
    FROM dbo.[WageTypeResult] r
    INNER JOIN Winners w ON w.[Id] = r.[Id]
    WHERE w.RowNumber = 1
    OPTION (RECOMPILE);
END
GO

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
-- fully denormalized: zero JOINs, all filters on WageTypeCustomResult columns
-- =============================================
CREATE PROCEDURE dbo.[GetConsolidatedWageTypeCustomResults]
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
            r.[Id],
            ROW_NUMBER() OVER (
                PARTITION BY r.[WageTypeNumber], r.[Start]
                ORDER BY r.[Created] DESC, r.[Id] DESC
            ) AS RowNumber
        FROM dbo.[WageTypeCustomResult] r
        WHERE r.[TenantId] = @tenantId
          AND r.[EmployeeId] = @employeeId
          -- period filter: single hash → equality seek; multiple → IN list
          AND (
              (@startHashCount = 1 AND r.[StartHash] = @startHash)
              OR (@startHashCount > 1 AND r.[StartHash] IN (
                  SELECT CAST(value AS INT) FROM OPENJSON(@periodStartHashes)))
          )
          AND (@divisionId IS NULL OR r.[DivisionId] = @divisionId)
          AND (@wageTypeNumbers IS NULL OR @wageTypeCount = 0
               OR (@wageTypeCount = 1 AND r.[WageTypeNumber] = @wageTypeNumber)
               OR (@wageTypeCount > 1 AND r.[WageTypeNumber] IN (
                   SELECT CAST(value AS DECIMAL(28, 6)) FROM OPENJSON(@wageTypeNumbers))))
          AND (@evaluationDate IS NULL OR r.[Created] <= @evaluationDate)
          AND (@jobStatus IS NULL
               OR r.[PayrunJobId] IN (
                   SELECT pj.[Id] FROM dbo.[PayrunJob] pj
                   WHERE pj.[JobStatus] = @jobStatus))
          AND (r.[Forecast] IS NULL OR r.[Forecast] = @forecast)
          AND (@noRetro = 0 OR r.[ParentJobId] IS NULL)
          AND (@excludeParentJobId IS NULL OR r.[ParentJobId] IS NULL
               OR r.[ParentJobId] <> @excludeParentJobId)
    )
    -- Phase 2: key lookup only for winning rows
    SELECT r.*
    FROM dbo.[WageTypeCustomResult] r
    INNER JOIN Winners w ON w.[Id] = r.[Id]
    WHERE w.RowNumber = 1
    OPTION (RECOMPILE);
END
GO

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
-- fully denormalized: zero JOINs, all filters on CollectorResult columns
-- =============================================
CREATE PROCEDURE dbo.[GetConsolidatedCollectorResults]
    @tenantId AS INT,
    @employeeId AS INT,
    @divisionId AS INT = NULL,
    @collectorNameHashes AS VARCHAR(MAX) = NULL,
    @periodStartHashes AS VARCHAR(MAX) = NULL,
    @jobStatus AS INT = NULL,
    @forecast AS VARCHAR(128) = NULL,
    @evaluationDate AS DATETIME2(7) = NULL,
    @noRetro AS BIT = 0,
    @excludeParentJobId AS INT = NULL
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @collectorNameHash INT;
    DECLARE @collectorCount INT;
    SELECT @collectorCount = COUNT(*) FROM OPENJSON(@collectorNameHashes);

    IF (@collectorCount = 1)
    BEGIN
        SELECT @collectorNameHash = CAST(value AS INT)
        FROM OPENJSON(@collectorNameHashes);
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
    -- Index key order: (TenantId, EmployeeId, StartHash, CollectorNameHash)
    -- → seeks directly to the period, constant cost regardless of history
    ;WITH Winners AS (
        SELECT
            r.[Id],
            ROW_NUMBER() OVER (
                PARTITION BY r.[CollectorNameHash], r.[Start]
                ORDER BY r.[Created] DESC, r.[Id] DESC
            ) AS RowNumber
        FROM dbo.[CollectorResult] r
        WHERE r.[TenantId] = @tenantId
          AND r.[EmployeeId] = @employeeId
          -- period filter: single hash → equality seek; multiple → IN list
          AND (
              (@startHashCount = 1 AND r.[StartHash] = @startHash)
              OR (@startHashCount > 1 AND r.[StartHash] IN (
                  SELECT CAST(value AS INT) FROM OPENJSON(@periodStartHashes)))
          )
          AND (@divisionId IS NULL OR r.[DivisionId] = @divisionId)
          AND (@collectorNameHashes IS NULL OR @collectorCount = 0
               OR (@collectorCount = 1 AND r.[CollectorNameHash] = @collectorNameHash)
               OR (@collectorCount > 1 AND r.[CollectorNameHash] IN (
                   SELECT CAST(value AS INT) FROM OPENJSON(@collectorNameHashes))))
          AND (@evaluationDate IS NULL OR r.[Created] <= @evaluationDate)
          AND (@jobStatus IS NULL
               OR r.[PayrunJobId] IN (
                   SELECT pj.[Id] FROM dbo.[PayrunJob] pj
                   WHERE pj.[JobStatus] = @jobStatus))
          AND (r.[Forecast] IS NULL OR r.[Forecast] = @forecast)
          AND (@noRetro = 0 OR r.[ParentJobId] IS NULL)
          AND (@excludeParentJobId IS NULL OR r.[ParentJobId] IS NULL
               OR r.[ParentJobId] <> @excludeParentJobId)
    )
    -- Phase 2: key lookup only for winning rows
    SELECT r.*
    FROM dbo.[CollectorResult] r
    INNER JOIN Winners w ON w.[Id] = r.[Id]
    WHERE w.RowNumber = 1
    OPTION (RECOMPILE);
END
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

IF EXISTS (
    SELECT *
    FROM sysobjects
    WHERE id = object_id(N'[dbo].[GetConsolidatedCollectorCustomResults]')
      AND OBJECTPROPERTY(id, N'IsProcedure') = 1
    )
BEGIN
  DROP PROCEDURE dbo.[GetConsolidatedCollectorCustomResults]
END
GO

-- =============================================
-- Get consolidated collector custom results
-- fully denormalized: zero JOINs, all filters on CollectorCustomResult columns
-- =============================================
CREATE PROCEDURE dbo.[GetConsolidatedCollectorCustomResults]
    @tenantId AS INT,
    @employeeId AS INT,
    @divisionId AS INT = NULL,
    @collectorNameHashes AS VARCHAR(MAX) = NULL,
    @periodStartHashes AS VARCHAR(MAX) = NULL,
    @jobStatus AS INT = NULL,
    @forecast AS VARCHAR(128) = NULL,
    @evaluationDate AS DATETIME2(7) = NULL,
    @noRetro AS BIT = 0,
    @excludeParentJobId AS INT = NULL
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @collectorNameHash INT;
    DECLARE @collectorCount INT;
    SELECT @collectorCount = COUNT(*) FROM OPENJSON(@collectorNameHashes);

    IF (@collectorCount = 1)
    BEGIN
        SELECT @collectorNameHash = CAST(value AS INT)
        FROM OPENJSON(@collectorNameHashes);
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
    -- Index key order: (TenantId, EmployeeId, StartHash, CollectorNameHash)
    -- → seeks directly to the period, constant cost regardless of history
    ;WITH Winners AS (
        SELECT
            r.[Id],
            ROW_NUMBER() OVER (
                PARTITION BY r.[CollectorNameHash], r.[Start]
                ORDER BY r.[Created] DESC, r.[Id] DESC
            ) AS RowNumber
        FROM dbo.[CollectorCustomResult] r
        WHERE r.[TenantId] = @tenantId
          AND r.[EmployeeId] = @employeeId
          -- period filter: single hash → equality seek; multiple → IN list
          AND (
              (@startHashCount = 1 AND r.[StartHash] = @startHash)
              OR (@startHashCount > 1 AND r.[StartHash] IN (
                  SELECT CAST(value AS INT) FROM OPENJSON(@periodStartHashes)))
          )
          AND (@divisionId IS NULL OR r.[DivisionId] = @divisionId)
          AND (@collectorNameHashes IS NULL OR @collectorCount = 0
               OR (@collectorCount = 1 AND r.[CollectorNameHash] = @collectorNameHash)
               OR (@collectorCount > 1 AND r.[CollectorNameHash] IN (
                   SELECT CAST(value AS INT) FROM OPENJSON(@collectorNameHashes))))
          AND (@evaluationDate IS NULL OR r.[Created] <= @evaluationDate)
          AND (@jobStatus IS NULL
               OR r.[PayrunJobId] IN (
                   SELECT pj.[Id] FROM dbo.[PayrunJob] pj
                   WHERE pj.[JobStatus] = @jobStatus))
          AND (r.[Forecast] IS NULL OR r.[Forecast] = @forecast)
          AND (@noRetro = 0 OR r.[ParentJobId] IS NULL)
          AND (@excludeParentJobId IS NULL OR r.[ParentJobId] IS NULL
               OR r.[ParentJobId] <> @excludeParentJobId)
    )
    -- Phase 2: key lookup only for winning rows
    SELECT r.*
    FROM dbo.[CollectorCustomResult] r
    INNER JOIN Winners w ON w.[Id] = r.[Id]
    WHERE w.RowNumber = 1
    OPTION (RECOMPILE);
END
GO

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
                   WHERE pj.[JobStatus] = @jobStatus))
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

-- =============================================================================
-- VERSION SET
-- =============================================================================

DECLARE @errorID int
INSERT INTO dbo.[Version] (
    MajorVersion, MinorVersion, SubVersion, [Owner], [Description])
VALUES (
    1, 1, 0, CURRENT_USER,
    'Payroll Engine: Migration v1.0.0 -> v1.1.0')
SET @errorID = @@ERROR
IF (@errorID <> 0) BEGIN
    PRINT 'Error while updating the Payroll Engine database version.'
END ELSE BEGIN
    PRINT 'Payroll Engine database version successfully updated to release 1.1.0'
END
GO

COMMIT TRANSACTION
GO

SET NOEXEC OFF
GO
