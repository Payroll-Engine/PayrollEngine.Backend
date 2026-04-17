USE [PayrollEngine];
GO

SET XACT_ABORT ON
GO

-- =============================================================================
-- VERSION CHECK
-- Guard: abort if the schema is not at version 0.9.7
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

IF @MajorVersion <> 0 OR @MinorVersion <> 9 OR @SubVersion <> 7 BEGIN
    DECLARE @ActualVersion NVARCHAR(20) =
        CAST(ISNULL(@MajorVersion, -1) AS NVARCHAR) + '.' +
        CAST(ISNULL(@MinorVersion, -1) AS NVARCHAR) + '.' +
        CAST(ISNULL(@SubVersion,   -1) AS NVARCHAR)
    RAISERROR('Version mismatch: expected 0.9.7, found %s', 16, 1, @ActualVersion)
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
        [ClusterSetWageTypePeriod]  AS ClusterSetWageTypePeriod,
        [ClusterSetWageTypeLookup]  AS ClusterSetWageTypeLookup
    FOR JSON PATH, WITHOUT_ARRAY_WRAPPER
)
WHERE [ClusterSetCase]           IS NOT NULL
   OR [ClusterSetCaseField]      IS NOT NULL
   OR [ClusterSetCollector]      IS NOT NULL
   OR [ClusterSetCollectorRetro] IS NOT NULL
   OR [ClusterSetWageType]       IS NOT NULL
   OR [ClusterSetWageTypeRetro]  IS NOT NULL
   OR [ClusterSetCaseValue]      IS NOT NULL
   OR [ClusterSetWageTypePeriod] IS NOT NULL
   OR [ClusterSetWageTypeLookup] IS NOT NULL;
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
ALTER TABLE [dbo].[Payroll] DROP COLUMN [ClusterSetWageTypeLookup];
GO

-- =============================================================================
-- VERSION SET
-- =============================================================================

DECLARE @errorID int
INSERT INTO dbo.[Version] (
    MajorVersion, MinorVersion, SubVersion, [Owner], [Description])
VALUES (
    1, 0, 0, CURRENT_USER,
    'Payroll Engine: Migration v0.9.7 -> v1.0.0')
SET @errorID = @@ERROR
IF (@errorID <> 0) BEGIN
    PRINT 'Error while updating the Payroll Engine database version.'
END ELSE BEGIN
    PRINT 'Payroll Engine database version successfully updated to release 1.0.0'
END
GO

COMMIT TRANSACTION
GO

SET NOEXEC OFF
GO
