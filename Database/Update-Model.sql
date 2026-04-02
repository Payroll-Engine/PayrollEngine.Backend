USE [PayrollEngine];
GO

SET XACT_ABORT ON
GO

-- =============================================================================
-- VERSION CHECK
-- Guard: abort if the schema is not at version 0.9.6
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

IF @MajorVersion <> 0 OR @MinorVersion <> 9 OR @SubVersion <> 6 BEGIN
    DECLARE @ActualVersion NVARCHAR(20) =
        CAST(ISNULL(@MajorVersion, -1) AS NVARCHAR) + '.' +
        CAST(ISNULL(@MinorVersion, -1) AS NVARCHAR) + '.' +
        CAST(ISNULL(@SubVersion,   -1) AS NVARCHAR)
    RAISERROR('Version mismatch: expected 0.9.6, found %s', 16, 1, @ActualVersion)
    SET NOEXEC ON
END
GO

BEGIN TRANSACTION
GO

-- =============================================================================
-- TABLE CHANGES
-- =============================================================================

-- Payroll: consolidate individual ClusterSetXxx columns into single ClusterSet JSON column
ALTER TABLE [dbo].[Payroll] ADD [ClusterSet] [nvarchar](max) NULL;
GO

-- migrate existing data into the new JSON column
UPDATE [dbo].[Payroll]
SET [ClusterSet] = (
    SELECT
        [ClusterSetCase]            AS ClusterSetCase,
        [ClusterSetCaseField]       AS ClusterSetCaseField,
        [ClusterSetCollector]       AS ClusterSetCollector,
        [ClusterSetCollectorRetro]  AS ClusterSetCollectorRetro,
        [ClusterSetWageType]        AS ClusterSetWageType,
        [ClusterSetWageTypeRetro]   AS ClusterSetWageTypeRetro,
        [ClusterSetCaseValue]       AS ClusterSetCaseValue,
        [ClusterSetWageTypePeriod]  AS ClusterSetWageTypePeriod
    FOR JSON PATH, WITHOUT_ARRAY_WRAPPER
)
WHERE [ClusterSetCase]           IS NOT NULL
   OR [ClusterSetCaseField]      IS NOT NULL
   OR [ClusterSetCollector]      IS NOT NULL
   OR [ClusterSetCollectorRetro] IS NOT NULL
   OR [ClusterSetWageType]       IS NOT NULL
   OR [ClusterSetWageTypeRetro]  IS NOT NULL
   OR [ClusterSetCaseValue]      IS NOT NULL
   OR [ClusterSetWageTypePeriod] IS NOT NULL;
GO

-- drop individual columns (now superseded by ClusterSet JSON)
ALTER TABLE [dbo].[Payroll] DROP COLUMN [ClusterSetCase];
ALTER TABLE [dbo].[Payroll] DROP COLUMN [ClusterSetCaseField];
ALTER TABLE [dbo].[Payroll] DROP COLUMN [ClusterSetCollector];
ALTER TABLE [dbo].[Payroll] DROP COLUMN [ClusterSetCollectorRetro];
ALTER TABLE [dbo].[Payroll] DROP COLUMN [ClusterSetWageType];
ALTER TABLE [dbo].[Payroll] DROP COLUMN [ClusterSetWageTypeRetro];
ALTER TABLE [dbo].[Payroll] DROP COLUMN [ClusterSetCaseValue];
ALTER TABLE [dbo].[Payroll] DROP COLUMN [ClusterSetWageTypePeriod];
GO

-- Payrun: RetroTimeType (enum) → RetroBackCycles (int)
-- -1 = unlimited (was: Anytime = 0), 0 = current cycle (was: Cycle = 1)
EXEC sp_rename 'dbo.Payrun.RetroTimeType', 'RetroBackCycles', 'COLUMN';
GO
UPDATE [dbo].[Payrun] SET [RetroBackCycles] = -1 WHERE [RetroBackCycles] = 0; -- Anytime → unlimited
GO
UPDATE [dbo].[Payrun] SET [RetroBackCycles] = 0  WHERE [RetroBackCycles] = 1; -- Cycle → current cycle
GO

-- PayrollResult: new denormalized name fields
ALTER TABLE [dbo].[PayrollResult] ADD [PayrollName]        [nvarchar](128) NULL;
GO
ALTER TABLE [dbo].[PayrollResult] ADD [PayrunName]         [nvarchar](128) NULL;
GO
ALTER TABLE [dbo].[PayrollResult] ADD [PayrunJobName]      [nvarchar](128) NULL;
GO
ALTER TABLE [dbo].[PayrollResult] ADD [EmployeeIdentifier] [nvarchar](128) NULL;
GO
ALTER TABLE [dbo].[PayrollResult] ADD [DivisionName]       [nvarchar](128) NULL;
GO

-- RegulationShare: new IsolationLevel column (DEFAULT 3 = TenantIsolationLevel.Write)
ALTER TABLE [dbo].[RegulationShare] ADD [IsolationLevel] [int] NOT NULL
    CONSTRAINT [DF_RegulationShare_IsolationLevel] DEFAULT 3;
GO

-- =============================================================================
-- INDEX CHANGES
-- =============================================================================

-- New index on RegulationShare for IsolationLevel-aware regulation lookups
CREATE NONCLUSTERED INDEX [IX_RegulationShare.Consumer_Provider_Level]
ON [dbo].[RegulationShare] (
    [ConsumerTenantId]     ASC,
    [ProviderRegulationId] ASC,
    [IsolationLevel]       ASC
);
GO

-- =============================================================================
-- FUNCTION CHANGES
-- =============================================================================

-- GetDerivedRegulations: added IsolationLevel >= 3 guard for shared regulations
IF OBJECT_ID('[dbo].[GetDerivedRegulations]') IS NOT NULL
    DROP FUNCTION [dbo].[GetDerivedRegulations];
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
                SELECT 1 FROM [dbo].[RegulationShare] rs
                WHERE rs.[ProviderRegulationId] = [Regulation].[Id]
                  AND rs.[ConsumerTenantId]     = @tenantId
                  AND rs.[IsolationLevel]       >= 3  -- TenantIsolationLevel.Write
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

-- GetConsolidatedWageTypeResults: rewritten with StartHash-based index seek
IF OBJECT_ID('[dbo].[GetConsolidatedWageTypeResults]') IS NOT NULL
    DROP PROCEDURE [dbo].[GetConsolidatedWageTypeResults];
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
                   WHERE pj.[JobStatus] & @jobStatus = pj.[JobStatus]))
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

-- GetDerivedReports: added ReportIsolation column to SELECT
IF OBJECT_ID('[dbo].[GetDerivedReports]') IS NOT NULL
    DROP PROCEDURE [dbo].[GetDerivedReports];
GO

-- =============================================
-- Get the topmost derived reports (only active).
-- =============================================
CREATE PROCEDURE dbo.[GetDerivedReports]
  @tenantId       AS INT,
  @payrollId      AS INT,
  @regulationDate AS DATETIME2(7),
  @createdBefore  AS DATETIME2(7),
  @userType       AS INT = NULL,
  @reportNames    AS VARCHAR(MAX) = NULL,
  @includeClusters AS VARCHAR(MAX) = NULL,
  @excludeClusters AS VARCHAR(MAX) = NULL
AS
BEGIN
  SET NOCOUNT ON;

  SELECT [Regulations].[Id] AS [RegulationId],
    [Regulations].[Level] AS [Level],
    [Regulations].[Priority] AS [Priority],
    dbo.[Report].[Id],
    dbo.[Report].[Status],
    dbo.[Report].[Created],
    dbo.[Report].[Updated],
    dbo.[Report].[RegulationId],
    dbo.[Report].[Name],
    dbo.[Report].[NameLocalizations],
    dbo.[Report].[Description],
    dbo.[Report].[DescriptionLocalizations],
    dbo.[Report].[Category],
    dbo.[Report].[Queries],
    dbo.[Report].[Relations],
    dbo.[Report].[AttributeMode],
    dbo.[Report].[UserType],
    dbo.[Report].[ReportIsolation],
    dbo.[Report].[BuildExpression],
    dbo.[Report].[StartExpression],
    dbo.[Report].[EndExpression],
    dbo.[Report].[ScriptHash],
    dbo.[Report].[Attributes],
    dbo.[Report].[Clusters]
  FROM dbo.[Report]
  INNER JOIN dbo.GetDerivedRegulations(@tenantId, @payrollId, @regulationDate, @createdBefore) AS [Regulations]
    ON dbo.[Report].[RegulationId] = [Regulations].[Id]
  WHERE dbo.[Report].[Status] = 0
    AND dbo.[Report].[Created] <= @createdBefore
    AND (@userType IS NULL OR dbo.[Report].[UserType] <= @userType)
    AND ((@includeClusters IS NULL AND @excludeClusters IS NULL)
        OR dbo.IsMatchingCluster(@includeClusters, @excludeClusters, dbo.[Report].[Clusters]) = 1)
    AND (
      @reportNames IS NULL
      OR LOWER(dbo.[Report].[Name]) IN (
        SELECT LOWER(value) FROM OPENJSON(@reportNames)
        )
      )
  ORDER BY [Level] DESC, [Priority] DESC
END
GO

-- =============================================================================
-- VERSION SET
-- =============================================================================

DECLARE @errorID int
INSERT INTO dbo.[Version] (
    MajorVersion, MinorVersion, SubVersion, [Owner], [Description])
VALUES (
    0, 9, 7, CURRENT_USER,
    'Payroll Engine: Migration v0.9.6 -> v0.9.7')
SET @errorID = @@ERROR
IF (@errorID <> 0) BEGIN
    PRINT 'Error while updating the Payroll Engine database version.'
END ELSE BEGIN
    PRINT 'Payroll Engine database version successfully updated to release 0.9.7'
END
GO

COMMIT TRANSACTION
GO

SET NOEXEC OFF
GO
