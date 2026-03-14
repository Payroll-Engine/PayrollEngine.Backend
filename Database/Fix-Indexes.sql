-- =============================================================================
-- Fix-Indexes.sql
-- PayrollEngine index corrections — pre v1.0 cleanup
--
-- Fixes:
--   BUG-1  IX_Regulation.UniqueNamePerTenant  — wrong table (was on Payroll)
--   BUG-2  IX_PayrunParameter.UniqueNamePerPayrun — wrong key (Id instead of PayrunId)
--   BUG-3  IX_ReportTemplate.UniqueTemplatePerPeport — wrong key + typo in name
--   PERF-1 IX_PayrunJob.JobStatus — bitwise-only usage, index never seeked
--   PERF-2 IX_Script.FunctionType — bitwise-only usage, index never seeked
--   RED-1  IX_WageType.WageTypeNumber — redundant, all queries are regulation-scoped
--   OPT-1  IX_LookupValue.UniqueValueKeyPerLookup — LookupId as leading key for FK support
--   OPT-2  IX_CaseRelation.* — RegulationId as leading key
--   OPT-3  IX_CaseField.UniqueNamePerCase — CaseId as leading key
--
-- Safe to run multiple times (fully idempotent).
-- Run OUTSIDE of active payrun jobs.
-- Run against: PayrollEngine database
-- =============================================================================

USE [PayrollEngine];
GO

SET NOCOUNT ON;
PRINT '=== PayrollEngine Index Fix — pre v1.0 ===';
PRINT '';

-- =============================================================================
-- PRE-FLIGHT: Duplicate checks for new UNIQUE constraints
-- The script aborts if any duplicates are found.
-- =============================================================================

PRINT '--- Pre-flight duplicate checks ---';

-- BUG-1: Regulation (Name, TenantId)
IF EXISTS (
    SELECT TenantId, [Name]
    FROM dbo.[Regulation]
    GROUP BY TenantId, [Name]
    HAVING COUNT(*) > 1
)
BEGIN
    RAISERROR('PRE-FLIGHT FAILED: Duplicate (TenantId, Name) found in [Regulation]. Fix data before running this script.', 16, 1);
    RETURN;
END
PRINT '[OK] Regulation: no duplicates on (TenantId, Name)';

-- BUG-2: PayrunParameter (PayrunId, Name)
IF EXISTS (
    SELECT [PayrunId], [Name]
    FROM dbo.[PayrunParameter]
    GROUP BY [PayrunId], [Name]
    HAVING COUNT(*) > 1
)
BEGIN
    RAISERROR('PRE-FLIGHT FAILED: Duplicate (PayrunId, Name) found in [PayrunParameter]. Fix data before running this script.', 16, 1);
    RETURN;
END
PRINT '[OK] PayrunParameter: no duplicates on (PayrunId, Name)';

-- BUG-3: ReportTemplate (ReportId, Name)
IF EXISTS (
    SELECT [ReportId], [Name]
    FROM dbo.[ReportTemplate]
    GROUP BY [ReportId], [Name]
    HAVING COUNT(*) > 1
)
BEGIN
    RAISERROR('PRE-FLIGHT FAILED: Duplicate (ReportId, Name) found in [ReportTemplate]. Fix data before running this script.', 16, 1);
    RETURN;
END
PRINT '[OK] ReportTemplate: no duplicates on (ReportId, Name)';

PRINT '';
PRINT '--- Applying fixes ---';
PRINT '';

BEGIN TRANSACTION;
BEGIN TRY

-- =============================================================================
-- BUG-1: IX_Regulation.UniqueNamePerTenant was on [Payroll] instead of [Regulation]
-- =============================================================================

-- Step 1: Remove the wrongly-placed index from [Payroll]
IF EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = 'IX_Regulation.UniqueNamePerTenant'
      AND object_id = OBJECT_ID('dbo.Payroll')
)
BEGIN
    DROP INDEX [IX_Regulation.UniqueNamePerTenant] ON dbo.[Payroll];
    PRINT '[FIX] BUG-1: Dropped IX_Regulation.UniqueNamePerTenant from [Payroll]';
END

-- Step 2: Create the correct index on [Regulation]
IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = 'IX_Regulation.UniqueNamePerTenant'
      AND object_id = OBJECT_ID('dbo.Regulation')
)
BEGIN
    CREATE UNIQUE NONCLUSTERED INDEX [IX_Regulation.UniqueNamePerTenant]
        ON dbo.[Regulation] ([Name] ASC, [TenantId] ASC)
        WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF,
              IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF,
              ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
        ON [PRIMARY];
    PRINT '[FIX] BUG-1: Created IX_Regulation.UniqueNamePerTenant on [Regulation]';
END
ELSE
    PRINT '[SKIP] BUG-1: IX_Regulation.UniqueNamePerTenant on [Regulation] already exists';

-- =============================================================================
-- BUG-2: IX_PayrunParameter.UniqueNamePerPayrun used (Name, Id) — Id is PK,
--        constraint was a tautology. Correct key: (PayrunId, Name)
-- =============================================================================

IF EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = 'IX_PayrunParameter.UniqueNamePerPayrun'
      AND object_id = OBJECT_ID('dbo.PayrunParameter')
)
BEGIN
    DROP INDEX [IX_PayrunParameter.UniqueNamePerPayrun] ON dbo.[PayrunParameter];
    PRINT '[FIX] BUG-2: Dropped IX_PayrunParameter.UniqueNamePerPayrun (was on wrong key)';
END

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = 'IX_PayrunParameter.UniqueNamePerPayrun'
      AND object_id = OBJECT_ID('dbo.PayrunParameter')
)
BEGIN
    CREATE UNIQUE NONCLUSTERED INDEX [IX_PayrunParameter.UniqueNamePerPayrun]
        ON dbo.[PayrunParameter] ([PayrunId] ASC, [Name] ASC)
        WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF,
              IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF,
              ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
        ON [PRIMARY];
    PRINT '[FIX] BUG-2: Created IX_PayrunParameter.UniqueNamePerPayrun on (PayrunId, Name)';
END
ELSE
    PRINT '[SKIP] BUG-2: IX_PayrunParameter.UniqueNamePerPayrun already correct';

-- =============================================================================
-- BUG-3: IX_ReportTemplate.UniqueTemplatePerPeport — typo "Peport" + wrong key (Id)
--        Correct name: UniqueTemplatePerReport, correct key: (ReportId, Name)
-- =============================================================================

-- Remove old index (wrong name + wrong key)
IF EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = 'IX_ReportTemplate.UniqueTemplatePerPeport'
      AND object_id = OBJECT_ID('dbo.ReportTemplate')
)
BEGIN
    DROP INDEX [IX_ReportTemplate.UniqueTemplatePerPeport] ON dbo.[ReportTemplate];
    PRINT '[FIX] BUG-3: Dropped IX_ReportTemplate.UniqueTemplatePerPeport (typo + wrong key)';
END

-- Create correct index with fixed name and key
IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = 'IX_ReportTemplate.UniqueTemplatePerReport'
      AND object_id = OBJECT_ID('dbo.ReportTemplate')
)
BEGIN
    CREATE UNIQUE NONCLUSTERED INDEX [IX_ReportTemplate.UniqueTemplatePerReport]
        ON dbo.[ReportTemplate] ([ReportId] ASC, [Name] ASC)
        WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF,
              IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF,
              ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
        ON [PRIMARY];
    PRINT '[FIX] BUG-3: Created IX_ReportTemplate.UniqueTemplatePerReport on (ReportId, Name)';
END
ELSE
    PRINT '[SKIP] BUG-3: IX_ReportTemplate.UniqueTemplatePerReport already exists';

-- =============================================================================
-- PERF-1: IX_PayrunJob.JobStatus — useless index, bitwise & prevents seeks
-- =============================================================================

IF EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = 'IX_PayrunJob.JobStatus'
      AND object_id = OBJECT_ID('dbo.PayrunJob')
)
BEGIN
    DROP INDEX [IX_PayrunJob.JobStatus] ON dbo.[PayrunJob];
    PRINT '[FIX] PERF-1: Dropped IX_PayrunJob.JobStatus (bitwise filter — index never seeked)';
END
ELSE
    PRINT '[SKIP] PERF-1: IX_PayrunJob.JobStatus already removed';

-- =============================================================================
-- PERF-2: IX_Script.FunctionType — useless index, bitwise & prevents seeks
--         GetFunctionScriptsAsync filters FunctionTypeMask with bitwise AND only
-- =============================================================================

IF EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = 'IX_Script.FunctionType'
      AND object_id = OBJECT_ID('dbo.Script')
)
BEGIN
    DROP INDEX [IX_Script.FunctionType] ON dbo.[Script];
    PRINT '[FIX] PERF-2: Dropped IX_Script.FunctionType (bitwise filter — index never seeked)';
END
ELSE
    PRINT '[SKIP] PERF-2: IX_Script.FunctionType already removed';

-- =============================================================================
-- RED-1: IX_WageType.WageTypeNumber — redundant, all queries are regulation-scoped
--        GetDerivedWageTypes always joins via GetDerivedRegulations (TenantId/PayrollId)
--        ExistsAnyAsync uses composite (RegulationId, WageTypeNumber) index
-- =============================================================================

IF EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = 'IX_WageType.WageTypeNumber'
      AND object_id = OBJECT_ID('dbo.WageType')
)
BEGIN
    DROP INDEX [IX_WageType.WageTypeNumber] ON dbo.[WageType];
    PRINT '[FIX] RED-1: Dropped IX_WageType.WageTypeNumber (redundant — all queries regulation-scoped)';
END
ELSE
    PRINT '[SKIP] RED-1: IX_WageType.WageTypeNumber already removed';

-- ============================================================================
-- OPT-1: IX_LookupValue.UniqueValueKeyPerLookup — LookupId as leading key
--        Enables FK seek on Lookup DELETE; uniqueness unchanged
-- =============================================================================

IF EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = 'IX_LookupValue.UniqueValueKeyPerLookup'
      AND object_id = OBJECT_ID('dbo.LookupValue')
)
BEGIN
    DROP INDEX [IX_LookupValue.UniqueValueKeyPerLookup] ON dbo.[LookupValue];
    PRINT '[OPT] OPT-1: Dropped IX_LookupValue.UniqueValueKeyPerLookup (rebuilding with LookupId leading)';
END

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = 'IX_LookupValue.UniqueValueKeyPerLookup'
      AND object_id = OBJECT_ID('dbo.LookupValue')
)
BEGIN
    CREATE UNIQUE NONCLUSTERED INDEX [IX_LookupValue.UniqueValueKeyPerLookup]
        ON dbo.[LookupValue] ([LookupId] ASC, [LookupHash] ASC)
        WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF,
              IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF,
              ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
        ON [PRIMARY];
    PRINT '[OPT] OPT-1: Created IX_LookupValue.UniqueValueKeyPerLookup on (LookupId, LookupHash)';
END
ELSE
    PRINT '[SKIP] OPT-1: IX_LookupValue.UniqueValueKeyPerLookup already correct';

-- =============================================================================
-- OPT-2a: IX_CaseRelation.SourceCaseName — RegulationId as leading key
-- =============================================================================

IF EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = 'IX_CaseRelation.SourceCaseName'
      AND object_id = OBJECT_ID('dbo.CaseRelation')
)
BEGIN
    DROP INDEX [IX_CaseRelation.SourceCaseName] ON dbo.[CaseRelation];
    PRINT '[OPT] OPT-2a: Dropped IX_CaseRelation.SourceCaseName (rebuilding with RegulationId leading)';
END

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = 'IX_CaseRelation.SourceCaseName'
      AND object_id = OBJECT_ID('dbo.CaseRelation')
)
BEGIN
    CREATE NONCLUSTERED INDEX [IX_CaseRelation.SourceCaseName]
        ON dbo.[CaseRelation] ([RegulationId] ASC, [SourceCaseName] ASC)
        WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF,
              DROP_EXISTING = OFF, ONLINE = OFF,
              ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
        ON [PRIMARY];
    PRINT '[OPT] OPT-2a: Created IX_CaseRelation.SourceCaseName on (RegulationId, SourceCaseName)';
END
ELSE
    PRINT '[SKIP] OPT-2a: IX_CaseRelation.SourceCaseName already correct';

-- =============================================================================
-- OPT-2b: IX_CaseRelation.TargetCaseName — RegulationId as leading key
-- =============================================================================

IF EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = 'IX_CaseRelation.TargetCaseName'
      AND object_id = OBJECT_ID('dbo.CaseRelation')
)
BEGIN
    DROP INDEX [IX_CaseRelation.TargetCaseName] ON dbo.[CaseRelation];
    PRINT '[OPT] OPT-2b: Dropped IX_CaseRelation.TargetCaseName (rebuilding with RegulationId leading)';
END

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = 'IX_CaseRelation.TargetCaseName'
      AND object_id = OBJECT_ID('dbo.CaseRelation')
)
BEGIN
    CREATE NONCLUSTERED INDEX [IX_CaseRelation.TargetCaseName]
        ON dbo.[CaseRelation] ([RegulationId] ASC, [TargetCaseName] ASC)
        WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF,
              DROP_EXISTING = OFF, ONLINE = OFF,
              ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
        ON [PRIMARY];
    PRINT '[OPT] OPT-2b: Created IX_CaseRelation.TargetCaseName on (RegulationId, TargetCaseName)';
END
ELSE
    PRINT '[SKIP] OPT-2b: IX_CaseRelation.TargetCaseName already correct';

-- =============================================================================
-- OPT-2c: IX_CaseRelation.TargetSlot — RegulationId as leading key
-- =============================================================================

IF EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = 'IX_CaseRelation.TargetSlot'
      AND object_id = OBJECT_ID('dbo.CaseRelation')
)
BEGIN
    DROP INDEX [IX_CaseRelation.TargetSlot] ON dbo.[CaseRelation];
    PRINT '[OPT] OPT-2c: Dropped IX_CaseRelation.TargetSlot (rebuilding with RegulationId leading)';
END

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = 'IX_CaseRelation.TargetSlot'
      AND object_id = OBJECT_ID('dbo.CaseRelation')
)
BEGIN
    CREATE NONCLUSTERED INDEX [IX_CaseRelation.TargetSlot]
        ON dbo.[CaseRelation] ([RegulationId] ASC, [TargetCaseSlot] ASC)
        WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF,
              DROP_EXISTING = OFF, ONLINE = OFF,
              ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
        ON [PRIMARY];
    PRINT '[OPT] OPT-2c: Created IX_CaseRelation.TargetSlot on (RegulationId, TargetCaseSlot)';
END
ELSE
    PRINT '[SKIP] OPT-2c: IX_CaseRelation.TargetSlot already correct';

-- =============================================================================
-- OPT-3: IX_CaseField.UniqueNamePerCase — CaseId as leading key
--        Enables FK seek on Case DELETE; uniqueness unchanged
-- =============================================================================

IF EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = 'IX_CaseField.UniqueNamePerCase'
      AND object_id = OBJECT_ID('dbo.CaseField')
)
BEGIN
    DROP INDEX [IX_CaseField.UniqueNamePerCase] ON dbo.[CaseField];
    PRINT '[OPT] OPT-3: Dropped IX_CaseField.UniqueNamePerCase (rebuilding with CaseId leading)';
END

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = 'IX_CaseField.UniqueNamePerCase'
      AND object_id = OBJECT_ID('dbo.CaseField')
)
BEGIN
    CREATE UNIQUE NONCLUSTERED INDEX [IX_CaseField.UniqueNamePerCase]
        ON dbo.[CaseField] ([CaseId] ASC, [Name] ASC)
        WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF,
              IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF,
              ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
        ON [PRIMARY];
    PRINT '[OPT] OPT-3: Created IX_CaseField.UniqueNamePerCase on (CaseId, Name)';
END
ELSE
    PRINT '[SKIP] OPT-3: IX_CaseField.UniqueNamePerCase already correct';

-- =============================================================================
-- All fixes applied successfully
-- =============================================================================

COMMIT TRANSACTION;
PRINT '';
PRINT '=== All index fixes committed successfully ===';

END TRY
BEGIN CATCH
    ROLLBACK TRANSACTION;
    PRINT '';
    PRINT '!!! ERROR — transaction rolled back !!!';
    PRINT 'Error ' + CAST(ERROR_NUMBER() AS NVARCHAR) + ': ' + ERROR_MESSAGE();
    PRINT 'Severity: ' + CAST(ERROR_SEVERITY() AS NVARCHAR)
        + '  State: ' + CAST(ERROR_STATE() AS NVARCHAR)
        + '  Line: ' + CAST(ERROR_LINE() AS NVARCHAR);
    THROW;
END CATCH;
GO
