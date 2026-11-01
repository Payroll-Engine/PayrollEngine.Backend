USE [PayrollEngine];
GO

SET XACT_ABORT ON
GO

-- =============================================================================
-- VERSION CHECK
-- Guard: abort if the schema is not at version 1.1.0
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

IF @MajorVersion <> 1 OR @MinorVersion <> 1 OR @SubVersion <> 0 BEGIN
    DECLARE @ActualVersion NVARCHAR(20) =
        CAST(ISNULL(@MajorVersion, -1) AS NVARCHAR) + '.' +
        CAST(ISNULL(@MinorVersion, -1) AS NVARCHAR) + '.' +
        CAST(ISNULL(@SubVersion,   -1) AS NVARCHAR)
    RAISERROR('Version mismatch: expected 1.1.0, found %s', 16, 1, @ActualVersion)
    SET NOEXEC ON
END
GO

BEGIN TRANSACTION
GO

-- =============================================================================
-- TABLE CHANGES
-- =============================================================================

-- Calendar: anchored week/bi-week periods, pay date offset and cycle assignment
ALTER TABLE [dbo].[Calendar] ADD
    [PeriodAnchorDate] [datetime2](7) NULL,
    [PayDateOffset] [int] NULL,
    [CycleAssignment] [int] NULL;
GO

-- PayrunJob: pay date (existing jobs: period end date)
ALTER TABLE [dbo].[PayrunJob] ADD [PayDate] [datetime2](7) NULL;
GO
UPDATE [dbo].[PayrunJob] SET [PayDate] = CAST(CAST([PeriodEnd] AS date) AS datetime2(7));
GO
ALTER TABLE [dbo].[PayrunJob] ALTER COLUMN [PayDate] [datetime2](7) NOT NULL;
GO

-- Results: denormalized payrun job pay date
ALTER TABLE [dbo].[WageTypeResult] ADD [PayDate] [datetime2](7) NULL;
ALTER TABLE [dbo].[WageTypeCustomResult] ADD [PayDate] [datetime2](7) NULL;
ALTER TABLE [dbo].[CollectorResult] ADD [PayDate] [datetime2](7) NULL;
ALTER TABLE [dbo].[CollectorCustomResult] ADD [PayDate] [datetime2](7) NULL;
ALTER TABLE [dbo].[PayrunResult] ADD [PayDate] [datetime2](7) NULL;
GO

UPDATE r SET r.[PayDate] = j.[PayDate]
FROM [dbo].[WageTypeResult] r INNER JOIN [dbo].[PayrunJob] j ON j.[Id] = r.[PayrunJobId];
UPDATE r SET r.[PayDate] = j.[PayDate]
FROM [dbo].[WageTypeCustomResult] r INNER JOIN [dbo].[PayrunJob] j ON j.[Id] = r.[PayrunJobId];
UPDATE r SET r.[PayDate] = j.[PayDate]
FROM [dbo].[CollectorResult] r INNER JOIN [dbo].[PayrunJob] j ON j.[Id] = r.[PayrunJobId];
UPDATE r SET r.[PayDate] = j.[PayDate]
FROM [dbo].[CollectorCustomResult] r INNER JOIN [dbo].[PayrunJob] j ON j.[Id] = r.[PayrunJobId];
UPDATE r SET r.[PayDate] = j.[PayDate]
FROM [dbo].[PayrunResult] r INNER JOIN [dbo].[PayrunJob] j ON j.[Id] = r.[PayrunJobId];
GO

-- results without job: result end date
UPDATE [dbo].[WageTypeResult] SET [PayDate] = CAST(CAST([End] AS date) AS datetime2(7)) WHERE [PayDate] IS NULL;
UPDATE [dbo].[WageTypeCustomResult] SET [PayDate] = CAST(CAST([End] AS date) AS datetime2(7)) WHERE [PayDate] IS NULL;
UPDATE [dbo].[CollectorResult] SET [PayDate] = CAST(CAST([End] AS date) AS datetime2(7)) WHERE [PayDate] IS NULL;
UPDATE [dbo].[CollectorCustomResult] SET [PayDate] = CAST(CAST([End] AS date) AS datetime2(7)) WHERE [PayDate] IS NULL;
UPDATE [dbo].[PayrunResult] SET [PayDate] = CAST(CAST([End] AS date) AS datetime2(7)) WHERE [PayDate] IS NULL;
GO

ALTER TABLE [dbo].[WageTypeResult] ALTER COLUMN [PayDate] [datetime2](7) NOT NULL;
ALTER TABLE [dbo].[WageTypeCustomResult] ALTER COLUMN [PayDate] [datetime2](7) NOT NULL;
ALTER TABLE [dbo].[CollectorResult] ALTER COLUMN [PayDate] [datetime2](7) NOT NULL;
ALTER TABLE [dbo].[CollectorCustomResult] ALTER COLUMN [PayDate] [datetime2](7) NOT NULL;
ALTER TABLE [dbo].[PayrunResult] ALTER COLUMN [PayDate] [datetime2](7) NOT NULL;
GO

-- Webhook: signing secret (stored encrypted)
ALTER TABLE [dbo].[Webhook] ADD [Secret] [nvarchar](1024) NULL;
GO

-- =============================================================================
-- INDEX CHANGES
-- =============================================================================

-- (none in this release)

-- =============================================================================
-- STORED PROCEDURE CHANGES
-- =============================================================================

-- Result procedures select all result columns (r.*), the new PayDate column
-- is returned without procedure changes.
EXEC sp_refreshsqlmodule N'[dbo].[GetWageTypeResults]';
EXEC sp_refreshsqlmodule N'[dbo].[GetWageTypeCustomResults]';
EXEC sp_refreshsqlmodule N'[dbo].[GetCollectorResults]';
EXEC sp_refreshsqlmodule N'[dbo].[GetCollectorCustomResults]';
EXEC sp_refreshsqlmodule N'[dbo].[GetConsolidatedWageTypeResults]';
EXEC sp_refreshsqlmodule N'[dbo].[GetConsolidatedWageTypeCustomResults]';
EXEC sp_refreshsqlmodule N'[dbo].[GetConsolidatedCollectorResults]';
EXEC sp_refreshsqlmodule N'[dbo].[GetConsolidatedCollectorCustomResults]';
EXEC sp_refreshsqlmodule N'[dbo].[GetConsolidatedPayrunResults]';
GO

-- =============================================================================
-- VERSION SET
-- =============================================================================

DECLARE @errorID int
INSERT INTO dbo.[Version] (
    MajorVersion, MinorVersion, SubVersion, [Owner], [Description])
VALUES (
    1, 2, 0, CURRENT_USER,
    'Payroll Engine: Migration v1.1.0 -> v1.2.0')
SET @errorID = @@ERROR
IF (@errorID <> 0) BEGIN
    PRINT 'Error while updating the Payroll Engine database version.'
END ELSE BEGIN
    PRINT 'Payroll Engine database version successfully updated to release 1.2.0'
END
GO

COMMIT TRANSACTION
GO

SET NOEXEC OFF
GO
