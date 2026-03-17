USE [PayrollEngine];
GO

SET XACT_ABORT ON
GO

DECLARE @MajorVersion int, @MinorVersion int, @SubVersion int
SELECT TOP 1
    @MajorVersion = MajorVersion,
    @MinorVersion = MinorVersion,
    @SubVersion   = SubVersion
FROM dbo.[Version]
ORDER BY MajorVersion DESC, MinorVersion DESC, SubVersion DESC

IF @MajorVersion <> 0 OR @MinorVersion <> 9 OR @SubVersion <> 5 BEGIN
    DECLARE @ActualVersion NVARCHAR(20) =
        CAST(ISNULL(@MajorVersion, -1) AS NVARCHAR) + '.' +
        CAST(ISNULL(@MinorVersion, -1) AS NVARCHAR) + '.' +
        CAST(ISNULL(@SubVersion,   -1) AS NVARCHAR)
    RAISERROR('Version mismatch: expected 0.9.5, found %s', 16, 1, @ActualVersion)
    SET NOEXEC ON   -- suppress all subsequent batches incl. BEGIN TRANSACTION
END
GO
BEGIN TRANSACTION
GO

USE [PayrollEngine];
GO

-- ============================================================
-- Delta Update Script
-- Baseline : Baseline.Formatted.sql
-- Current  : Current.Formatted.sql
-- Generated: 2026-03-17 16:16
-- Changes  : +19 added / -13 removed / ~37 modified
-- ============================================================
GO

-- ------------------------------------------------------------
-- Removed objects (13)
-- ------------------------------------------------------------

IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_WageTypeCustomResult.StartHash' AND object_id = OBJECT_ID('[dbo].[WageTypeCustomResult]'))
    DROP INDEX [IX_WageTypeCustomResult.StartHash] ON [dbo].[WageTypeCustomResult];
GO

IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_WageType.WageTypeNumber' AND object_id = OBJECT_ID('[dbo].[WageType]'))
    DROP INDEX [IX_WageType.WageTypeNumber] ON [dbo].[WageType];
GO

IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Script.FunctionType' AND object_id = OBJECT_ID('[dbo].[Script]'))
    DROP INDEX [IX_Script.FunctionType] ON [dbo].[Script];
GO

IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_WageTypeResult.WageTypeNumber' AND object_id = OBJECT_ID('[dbo].[WageTypeResult]'))
    DROP INDEX [IX_WageTypeResult.WageTypeNumber] ON [dbo].[WageTypeResult];
GO

IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_WageTypeResult.StartHash' AND object_id = OBJECT_ID('[dbo].[WageTypeResult]'))
    DROP INDEX [IX_WageTypeResult.StartHash] ON [dbo].[WageTypeResult];
GO

IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_WageTypeCustomResult.WageTypeNumber' AND object_id = OBJECT_ID('[dbo].[WageTypeCustomResult]'))
    DROP INDEX [IX_WageTypeCustomResult.WageTypeNumber] ON [dbo].[WageTypeCustomResult];
GO

IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_PayrunResult.StartHash' AND object_id = OBJECT_ID('[dbo].[PayrunResult]'))
    DROP INDEX [IX_PayrunResult.StartHash] ON [dbo].[PayrunResult];
GO

IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_CollectorResult.CollectorNameHash' AND object_id = OBJECT_ID('[dbo].[CollectorResult]'))
    DROP INDEX [IX_CollectorResult.CollectorNameHash] ON [dbo].[CollectorResult];
GO

IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_CollectorCustomResult.StartHash' AND object_id = OBJECT_ID('[dbo].[CollectorCustomResult]'))
    DROP INDEX [IX_CollectorCustomResult.StartHash] ON [dbo].[CollectorCustomResult];
GO

IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_CollectorCustomResult.CollectorNameHash' AND object_id = OBJECT_ID('[dbo].[CollectorCustomResult]'))
    DROP INDEX [IX_CollectorCustomResult.CollectorNameHash] ON [dbo].[CollectorCustomResult];
GO

IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_PayrunResult.Name' AND object_id = OBJECT_ID('[dbo].[PayrunResult]'))
    DROP INDEX [IX_PayrunResult.Name] ON [dbo].[PayrunResult];
GO

IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_PayrunJob.JobStatus' AND object_id = OBJECT_ID('[dbo].[PayrunJob]'))
    DROP INDEX [IX_PayrunJob.JobStatus] ON [dbo].[PayrunJob];
GO

IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_CollectorResult.StartHash' AND object_id = OBJECT_ID('[dbo].[CollectorResult]'))
    DROP INDEX [IX_CollectorResult.StartHash] ON [dbo].[CollectorResult];
GO

-- ------------------------------------------------------------
-- Modified objects -- drop before re-create (37)
-- ------------------------------------------------------------

ALTER TABLE [dbo].[WageTypeResult] ADD [TenantId] [int] NOT NULL;
GO
ALTER TABLE [dbo].[WageTypeResult] ADD [EmployeeId] [int] NOT NULL;
GO
ALTER TABLE [dbo].[WageTypeResult] ADD [DivisionId] [int] NULL;
GO
ALTER TABLE [dbo].[WageTypeResult] ADD [PayrunJobId] [int] NOT NULL;
GO
ALTER TABLE [dbo].[WageTypeResult] ADD [Forecast] [nvarchar](128) NULL;
GO
ALTER TABLE [dbo].[WageTypeResult] ADD [ParentJobId] [int] NULL;
GO

ALTER TABLE [dbo].[PayrunTrace] ADD [TenantId] [int] NOT NULL;
GO
ALTER TABLE [dbo].[PayrunTrace] ADD [EmployeeId] [int] NOT NULL;
GO
ALTER TABLE [dbo].[PayrunTrace] ADD [DivisionId] [int] NULL;
GO
ALTER TABLE [dbo].[PayrunTrace] ADD [PayrunJobId] [int] NOT NULL;
GO
ALTER TABLE [dbo].[PayrunTrace] ADD [Forecast] [nvarchar](128) NULL;
GO
ALTER TABLE [dbo].[PayrunTrace] ADD [ParentJobId] [int] NULL;
GO

ALTER TABLE [dbo].[PayrunResult] ADD [TenantId] [int] NOT NULL;
GO
ALTER TABLE [dbo].[PayrunResult] ADD [EmployeeId] [int] NOT NULL;
GO
ALTER TABLE [dbo].[PayrunResult] ADD [DivisionId] [int] NULL;
GO
ALTER TABLE [dbo].[PayrunResult] ADD [PayrunJobId] [int] NOT NULL;
GO
ALTER TABLE [dbo].[PayrunResult] ADD [Forecast] [nvarchar](128) NULL;
GO
ALTER TABLE [dbo].[PayrunResult] ADD [ParentJobId] [int] NULL;
GO

ALTER TABLE [dbo].[Report] ADD [ReportIsolation] [int] NOT NULL;
GO

ALTER TABLE [dbo].[CollectorResult] ADD [TenantId] [int] NOT NULL;
GO
ALTER TABLE [dbo].[CollectorResult] ADD [EmployeeId] [int] NOT NULL;
GO
ALTER TABLE [dbo].[CollectorResult] ADD [DivisionId] [int] NULL;
GO
ALTER TABLE [dbo].[CollectorResult] ADD [PayrunJobId] [int] NOT NULL;
GO
ALTER TABLE [dbo].[CollectorResult] ADD [Forecast] [nvarchar](128) NULL;
GO
ALTER TABLE [dbo].[CollectorResult] ADD [ParentJobId] [int] NULL;
GO

ALTER TABLE [dbo].[ReportAudit] ADD [ReportIsolation] [int] NOT NULL;
GO

ALTER TABLE [dbo].[CollectorCustomResult] ADD [TenantId] [int] NOT NULL;
GO
ALTER TABLE [dbo].[CollectorCustomResult] ADD [EmployeeId] [int] NOT NULL;
GO
ALTER TABLE [dbo].[CollectorCustomResult] ADD [DivisionId] [int] NULL;
GO
ALTER TABLE [dbo].[CollectorCustomResult] ADD [PayrunJobId] [int] NOT NULL;
GO
ALTER TABLE [dbo].[CollectorCustomResult] ADD [Forecast] [nvarchar](128) NULL;
GO
ALTER TABLE [dbo].[CollectorCustomResult] ADD [ParentJobId] [int] NULL;
GO

ALTER TABLE [dbo].[WageTypeCustomResult] ADD [TenantId] [int] NOT NULL;
GO
ALTER TABLE [dbo].[WageTypeCustomResult] ADD [EmployeeId] [int] NOT NULL;
GO
ALTER TABLE [dbo].[WageTypeCustomResult] ADD [DivisionId] [int] NULL;
GO
ALTER TABLE [dbo].[WageTypeCustomResult] ADD [PayrunJobId] [int] NOT NULL;
GO
ALTER TABLE [dbo].[WageTypeCustomResult] ADD [Forecast] [nvarchar](128) NULL;
GO
ALTER TABLE [dbo].[WageTypeCustomResult] ADD [ParentJobId] [int] NULL;
GO

IF OBJECT_ID('[dbo].[GetConsolidatedWageTypeResults]') IS NOT NULL
    DROP PROCEDURE [dbo].[GetConsolidatedWageTypeResults];
GO

IF OBJECT_ID('[dbo].[GetWageTypeResults]') IS NOT NULL
    DROP PROCEDURE [dbo].[GetWageTypeResults];
GO

IF OBJECT_ID('[dbo].[GetDerivedRegulations]') IS NOT NULL
    DROP FUNCTION [dbo].[GetDerivedRegulations];
GO

IF OBJECT_ID('[dbo].[GetConsolidatedPayrunResults]') IS NOT NULL
    DROP PROCEDURE [dbo].[GetConsolidatedPayrunResults];
GO

IF OBJECT_ID('[dbo].[GetCollectorResults]') IS NOT NULL
    DROP PROCEDURE [dbo].[GetCollectorResults];
GO

IF OBJECT_ID('[dbo].[GetConsolidatedCollectorResults]') IS NOT NULL
    DROP PROCEDURE [dbo].[GetConsolidatedCollectorResults];
GO

IF OBJECT_ID('[dbo].[GetCollectorCustomResults]') IS NOT NULL
    DROP PROCEDURE [dbo].[GetCollectorCustomResults];
GO

IF OBJECT_ID('[dbo].[GetConsolidatedCollectorCustomResults]') IS NOT NULL
    DROP PROCEDURE [dbo].[GetConsolidatedCollectorCustomResults];
GO

IF OBJECT_ID('[dbo].[DeletePayrunJob]') IS NOT NULL
    DROP PROCEDURE [dbo].[DeletePayrunJob];
GO

IF OBJECT_ID('[dbo].[GetConsolidatedWageTypeCustomResults]') IS NOT NULL
    DROP PROCEDURE [dbo].[GetConsolidatedWageTypeCustomResults];
GO

IF OBJECT_ID('[dbo].[GetWageTypeCustomResults]') IS NOT NULL
    DROP PROCEDURE [dbo].[GetWageTypeCustomResults];
GO

IF OBJECT_ID('[dbo].[DeleteEmployee]') IS NOT NULL
    DROP PROCEDURE [dbo].[DeleteEmployee];
GO

IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_CaseRelation.SourceCaseName' AND object_id = OBJECT_ID('[dbo].[CaseRelation]'))
    DROP INDEX [IX_CaseRelation.SourceCaseName] ON [dbo].[CaseRelation];
GO

IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_CaseRelation.TargetCaseName' AND object_id = OBJECT_ID('[dbo].[CaseRelation]'))
    DROP INDEX [IX_CaseRelation.TargetCaseName] ON [dbo].[CaseRelation];
GO

IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_CaseRelation.TargetSlot' AND object_id = OBJECT_ID('[dbo].[CaseRelation]'))
    DROP INDEX [IX_CaseRelation.TargetSlot] ON [dbo].[CaseRelation];
GO

IF OBJECT_ID('[dbo].[DeleteLookup]') IS NOT NULL
    DROP PROCEDURE [dbo].[DeleteLookup];
GO

IF OBJECT_ID('[dbo].[GetDerivedCaseFields]') IS NOT NULL
    DROP PROCEDURE [dbo].[GetDerivedCaseFields];
GO

IF OBJECT_ID('[dbo].[GetDerivedCaseFieldsOfCase]') IS NOT NULL
    DROP PROCEDURE [dbo].[GetDerivedCaseFieldsOfCase];
GO

IF OBJECT_ID('[dbo].[GetDerivedCaseRelations]') IS NOT NULL
    DROP PROCEDURE [dbo].[GetDerivedCaseRelations];
GO

IF OBJECT_ID('[dbo].[GetDerivedCases]') IS NOT NULL
    DROP PROCEDURE [dbo].[GetDerivedCases];
GO

IF OBJECT_ID('[dbo].[GetDerivedCollectors]') IS NOT NULL
    DROP PROCEDURE [dbo].[GetDerivedCollectors];
GO

IF OBJECT_ID('[dbo].[GetDerivedLookups]') IS NOT NULL
    DROP PROCEDURE [dbo].[GetDerivedLookups];
GO

IF OBJECT_ID('[dbo].[GetDerivedLookupValues]') IS NOT NULL
    DROP PROCEDURE [dbo].[GetDerivedLookupValues];
GO

IF OBJECT_ID('[dbo].[GetDerivedReportParameters]') IS NOT NULL
    DROP PROCEDURE [dbo].[GetDerivedReportParameters];
GO

IF OBJECT_ID('[dbo].[GetDerivedReports]') IS NOT NULL
    DROP PROCEDURE [dbo].[GetDerivedReports];
GO

IF OBJECT_ID('[dbo].[GetDerivedReportTemplates]') IS NOT NULL
    DROP PROCEDURE [dbo].[GetDerivedReportTemplates];
GO

IF OBJECT_ID('[dbo].[GetDerivedScripts]') IS NOT NULL
    DROP PROCEDURE [dbo].[GetDerivedScripts];
GO

IF OBJECT_ID('[dbo].[GetDerivedWageTypes]') IS NOT NULL
    DROP PROCEDURE [dbo].[GetDerivedWageTypes];
GO

IF OBJECT_ID('[dbo].[DeleteTenant]') IS NOT NULL
    DROP PROCEDURE [dbo].[DeleteTenant];
GO

-- ------------------------------------------------------------
-- Added objects (19)
-- ------------------------------------------------------------

/****** Object:  StoredProcedure [dbo].[UpdateStatistics]    Script Date: 01.03.2026 22:35:19 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Update database statistics (tipping point)
-- see https://www.sqlskills.com/blogs/kimberly/the-tipping-point-query-answers/
-- see https://www.brentozar.com/archive/2019/10/how-to-think-like-the-sql-server-engine-whats-the-tipping-point/
-- =============================================
CREATE PROCEDURE [dbo].[UpdateStatistics]
AS
BEGIN
  DECLARE @sql NVARCHAR(MAX) = N'';

  SELECT @sql += N'UPDATE STATISTICS ' + QUOTENAME(s.name) + N'.' + QUOTENAME(t.name) + N' WITH FULLSCAN; '
  FROM sys.tables t
  INNER JOIN sys.schemas s
    ON t.schema_id = s.schema_id;

  EXEC sp_executesql @sql;
END
GO

CREATE PROCEDURE dbo.[UpdateStatisticsTargeted]
AS
BEGIN

UPDATE STATISTICS dbo.[LookupValue]           WITH FULLSCAN;

UPDATE STATISTICS dbo.[PayrollResult]         WITH FULLSCAN;
UPDATE STATISTICS dbo.[WageTypeResult]        WITH FULLSCAN;
UPDATE STATISTICS dbo.[WageTypeCustomResult]  WITH FULLSCAN;
UPDATE STATISTICS dbo.[CollectorResult]       WITH FULLSCAN;
UPDATE STATISTICS dbo.[CollectorCustomResult] WITH FULLSCAN;
UPDATE STATISTICS dbo.[PayrunResult]          WITH FULLSCAN;

UPDATE STATISTICS dbo.[GlobalCaseValue]       WITH FULLSCAN;
UPDATE STATISTICS dbo.[NationalCaseValue]     WITH FULLSCAN;
UPDATE STATISTICS dbo.[CompanyCaseValue]      WITH FULLSCAN;
UPDATE STATISTICS dbo.[EmployeeCaseValue]     WITH FULLSCAN;

END
GO

CREATE PROCEDURE dbo.[GetEmployeeCaseValuesByTenant]
  -- the tenant id
  @tenantId       AS INT,
  -- the value date: only values active at this date are returned (Start <= valueDate < End)
  @valueDate      AS DATETIME2(7) = NULL,
  -- the evaluation date: only values created on or before this date are returned
  @evaluationDate AS DATETIME2(7) = NULL,
  -- the case field names filter: JSON array of NVARCHAR(128), NULL = all fields
  @fieldNames     AS NVARCHAR(MAX) = NULL,
  -- the forecast name: NULL = real values only, name = real + forecast values
  @forecast       AS NVARCHAR(128) = NULL
AS
BEGIN
  SET NOCOUNT ON;

  SELECT
    ecv.[Id],
    ecv.[Status],
    ecv.[Created],
    ecv.[Updated],
    ecv.[EmployeeId],
    ecv.[DivisionId],
    ecv.[CaseName],
    ecv.[CaseNameLocalizations],
    ecv.[CaseFieldName],
    ecv.[CaseFieldNameLocalizations],
    ecv.[CaseSlot],
    ecv.[CaseSlotLocalizations],
    ecv.[ValueType],
    ecv.[Value],
    ecv.[NumericValue],
    ecv.[Culture],
    ecv.[CaseRelation],
    ecv.[CancellationDate],
    ecv.[Start],
    ecv.[End],
    ecv.[Forecast],
    ecv.[Tags],
    ecv.[Attributes]
  FROM dbo.[EmployeeCaseValue] ecv
  INNER JOIN dbo.[Employee] e
    ON e.[Id] = ecv.[EmployeeId]
  WHERE
    e.[TenantId] = @tenantId
    AND e.[Status] = 0
    AND ecv.[CancellationDate] IS NULL
    AND (@evaluationDate IS NULL OR ecv.[Created] <= @evaluationDate)
    AND (@valueDate IS NULL OR ecv.[Start] IS NULL OR ecv.[Start] <= @valueDate)
    AND (@valueDate IS NULL OR ecv.[End]   IS NULL OR ecv.[End]   >  @valueDate)
    AND (
      (@forecast IS NULL     AND ecv.[Forecast] IS NULL)
      OR (@forecast IS NOT NULL AND (ecv.[Forecast] IS NULL OR ecv.[Forecast] = @forecast))
    )
    AND (
      @fieldNames IS NULL
      OR ecv.[CaseFieldName] IN (SELECT [value] FROM OPENJSON(@fieldNames))
    )
  ORDER BY
    ecv.[EmployeeId] ASC,
    ecv.[CaseFieldName] ASC,
    ecv.[Created] DESC
END
GO

/****** Object:  Index [IX_CollectorCustomResult.CollectorResultId]    Script Date: 01.03.2026 22:35:19 ******/
CREATE NONCLUSTERED INDEX [IX_CollectorCustomResult.CollectorResultId] ON [dbo].[CollectorCustomResult] ([CollectorResultId] ASC)
  WITH (
      PAD_INDEX = OFF,
      STATISTICS_NORECOMPUTE = OFF,
      SORT_IN_TEMPDB = OFF,
      DROP_EXISTING = OFF,
      ONLINE = OFF,
      ALLOW_ROW_LOCKS = ON,
      ALLOW_PAGE_LOCKS = ON
      ) ON [PRIMARY]
GO

CREATE NONCLUSTERED INDEX [IX_CollectorCustomResult.Employee_Collector]
ON dbo.[CollectorCustomResult] ([TenantId], [EmployeeId], [StartHash], [CollectorNameHash])
INCLUDE ([Start], [Created], [DivisionId], [Forecast], [ParentJobId], [PayrunJobId]);
GO

CREATE NONCLUSTERED INDEX [IX_CollectorResult.Employee_Collector]
ON dbo.[CollectorResult] ([TenantId], [EmployeeId], [StartHash], [CollectorNameHash])
INCLUDE ([Start], [Created], [DivisionId], [Forecast], [ParentJobId], [PayrunJobId]);
GO

/****** Object:  Index [IX_CollectorResult.PayrollResultId]    Script Date: 01.03.2026 22:35:19 ******/
CREATE NONCLUSTERED INDEX [IX_CollectorResult.PayrollResultId] ON [dbo].[CollectorResult] ([PayrollResultId] ASC)
  WITH (
      PAD_INDEX = OFF,
      STATISTICS_NORECOMPUTE = OFF,
      SORT_IN_TEMPDB = OFF,
      DROP_EXISTING = OFF,
      ONLINE = OFF,
      ALLOW_ROW_LOCKS = ON,
      ALLOW_PAGE_LOCKS = ON
      ) ON [PRIMARY]
GO

/****** Object:  Index [IX_CompanyCaseValue.TenantId_Cover]
-- Covering index for per-tenant company case value queries.
-- Lead key: TenantId (matches the Unique constraint lead key and SP WHERE filter).
-- INCLUDE eliminates Key Lookups on Value/Start/End/CancellationDate/Forecast/Created.
******/
CREATE NONCLUSTERED INDEX [IX_CompanyCaseValue.TenantId_Cover]
  ON [dbo].[CompanyCaseValue] ([TenantId] ASC, [CaseFieldName] ASC)
  INCLUDE ([DivisionId], [Start], [End], [Value], [NumericValue], [CancellationDate], [Forecast], [Created], [Status])
  WITH (
      PAD_INDEX = OFF,
      STATISTICS_NORECOMPUTE = OFF,
      SORT_IN_TEMPDB = OFF,
      DROP_EXISTING = OFF,
      ONLINE = OFF,
      ALLOW_ROW_LOCKS = ON,
      ALLOW_PAGE_LOCKS = ON
      ) ON [PRIMARY]
GO

/****** Object:  Index [IX_Employee.TenantId]
-- Supports tenant-wide employee queries (GetEmployeeCaseValuesByTenant).
-- Lead key: TenantId enables Index Seek when filtering all active employees per tenant.
******/
CREATE NONCLUSTERED INDEX [IX_Employee.TenantId]
  ON [dbo].[Employee] ([TenantId] ASC, [Status] ASC)
  WITH (
      PAD_INDEX = OFF,
      STATISTICS_NORECOMPUTE = OFF,
      SORT_IN_TEMPDB = OFF,
      DROP_EXISTING = OFF,
      ONLINE = OFF,
      ALLOW_ROW_LOCKS = ON,
      ALLOW_PAGE_LOCKS = ON
      ) ON [PRIMARY]
GO

/****** Object:  Index [IX_EmployeeCaseValue.EmployeeId_Cover]
-- Covering index for per-employee and tenant-wide case value queries.
-- Lead key: EmployeeId for the per-employee SP filter (WHERE EmployeeId = @parentId).
-- INCLUDE covers Start/End/Value/CancellationDate/Forecast/Created to avoid Key Lookups
-- on the GetEmployeeCaseValues and GetEmployeeCaseValuesByTenant hot paths.
******/
CREATE NONCLUSTERED INDEX [IX_EmployeeCaseValue.EmployeeId_Cover]
  ON [dbo].[EmployeeCaseValue] ([EmployeeId] ASC, [CaseFieldName] ASC)
  INCLUDE ([DivisionId], [Start], [End], [Value], [NumericValue], [CancellationDate], [Forecast], [Created], [Status])
  WITH (
      PAD_INDEX = OFF,
      STATISTICS_NORECOMPUTE = OFF,
      SORT_IN_TEMPDB = OFF,
      DROP_EXISTING = OFF,
      ONLINE = OFF,
      ALLOW_ROW_LOCKS = ON,
      ALLOW_PAGE_LOCKS = ON
      ) ON [PRIMARY]
GO

/****** Object:  Index [IX_GlobalCaseValue.TenantId_Cover]
-- Covering index for per-tenant global case value queries.
-- Lead key: TenantId (matches the Unique constraint lead key and SP WHERE filter).
-- INCLUDE eliminates Key Lookups on Value/Start/End/CancellationDate/Forecast/Created.
******/
CREATE NONCLUSTERED INDEX [IX_GlobalCaseValue.TenantId_Cover]
  ON [dbo].[GlobalCaseValue] ([TenantId] ASC, [CaseFieldName] ASC)
  INCLUDE ([DivisionId], [Start], [End], [Value], [NumericValue], [CancellationDate], [Forecast], [Created], [Status])
  WITH (
      PAD_INDEX = OFF,
      STATISTICS_NORECOMPUTE = OFF,
      SORT_IN_TEMPDB = OFF,
      DROP_EXISTING = OFF,
      ONLINE = OFF,
      ALLOW_ROW_LOCKS = ON,
      ALLOW_PAGE_LOCKS = ON
      ) ON [PRIMARY]
GO

/****** Object:  Index [IX_NationalCaseValue.TenantId_Cover]
-- Covering index for per-tenant national case value queries.
-- Lead key: TenantId (matches the Unique constraint lead key and SP WHERE filter).
-- INCLUDE eliminates Key Lookups on Value/Start/End/CancellationDate/Forecast/Created.
******/
CREATE NONCLUSTERED INDEX [IX_NationalCaseValue.TenantId_Cover]
  ON [dbo].[NationalCaseValue] ([TenantId] ASC, [CaseFieldName] ASC)
  INCLUDE ([DivisionId], [Start], [End], [Value], [NumericValue], [CancellationDate], [Forecast], [Created], [Status])
  WITH (
      PAD_INDEX = OFF,
      STATISTICS_NORECOMPUTE = OFF,
      SORT_IN_TEMPDB = OFF,
      DROP_EXISTING = OFF,
      ONLINE = OFF,
      ALLOW_ROW_LOCKS = ON,
      ALLOW_PAGE_LOCKS = ON
      ) ON [PRIMARY]
GO

/****** Object:  Index [IX_PayrollResult.EmployeeId_Cover]    Script Date: 01.03.2026 22:35:19 ******/
CREATE NONCLUSTERED INDEX [IX_PayrollResult.EmployeeId_Cover] ON [dbo].[PayrollResult] ([EmployeeId] ASC) INCLUDE (
  [PayrunJobId],
  [DivisionId],
  [PayrunId]
  )
  WITH (
      PAD_INDEX = OFF,
      STATISTICS_NORECOMPUTE = OFF,
      SORT_IN_TEMPDB = OFF,
      DROP_EXISTING = OFF,
      ONLINE = OFF,
      ALLOW_ROW_LOCKS = ON,
      ALLOW_PAGE_LOCKS = ON
      ) ON [PRIMARY]
GO

CREATE NONCLUSTERED INDEX [IX_PayrunResult.Employee_Name]
ON dbo.[PayrunResult] ([TenantId], [EmployeeId], [StartHash], [Name])
INCLUDE ([Start], [Created], [DivisionId], [Forecast], [ParentJobId], [PayrunJobId]);
GO

/****** Object:  Index [IX_PayrunResult.PayrollResultId]    Script Date: 01.03.2026 22:35:19 ******/
CREATE NONCLUSTERED INDEX [IX_PayrunResult.PayrollResultId] ON [dbo].[PayrunResult] ([PayrollResultId] ASC)
  WITH (
      PAD_INDEX = OFF,
      STATISTICS_NORECOMPUTE = OFF,
      SORT_IN_TEMPDB = OFF,
      DROP_EXISTING = OFF,
      ONLINE = OFF,
      ALLOW_ROW_LOCKS = ON,
      ALLOW_PAGE_LOCKS = ON
      ) ON [PRIMARY]
GO

CREATE NONCLUSTERED INDEX [IX_PayrunTrace.Employee]
ON dbo.[PayrunTrace] ([TenantId], [EmployeeId])
INCLUDE ([Created], [DivisionId], [Forecast], [ParentJobId], [PayrunJobId]);

/****** Object:  Index [IX_PayrunTrace.PayrollResultId]    Script Date: 01.03.2026 22:35:19 ******/
CREATE NONCLUSTERED INDEX [IX_PayrunTrace.PayrollResultId] ON [dbo].[PayrunTrace] ([PayrollResultId] ASC)
  WITH (
      PAD_INDEX = OFF,
      STATISTICS_NORECOMPUTE = OFF,
      SORT_IN_TEMPDB = OFF,
      DROP_EXISTING = OFF,
      ONLINE = OFF,
      ALLOW_ROW_LOCKS = ON,
      ALLOW_PAGE_LOCKS = ON
      ) ON [PRIMARY]
GO

CREATE NONCLUSTERED INDEX [IX_WageTypeCustomResult.Employee_WageType]
ON dbo.[WageTypeCustomResult] ([TenantId], [EmployeeId], [StartHash], [WageTypeNumber])
INCLUDE ([Start], [Created], [DivisionId], [Forecast], [ParentJobId], [PayrunJobId]);
GO

CREATE NONCLUSTERED INDEX [IX_WageTypeResult.Employee_WageType]
ON dbo.[WageTypeResult] ([TenantId], [EmployeeId], [StartHash], [WageTypeNumber])
INCLUDE ([Start], [Created], [DivisionId], [Forecast], [ParentJobId], [PayrunJobId]);
GO

/****** Object:  Index [IX_WageTypeResult.PayrollResultId]    Script Date: 01.03.2026 22:35:19 ******/
CREATE NONCLUSTERED INDEX [IX_WageTypeResult.PayrollResultId] ON [dbo].[WageTypeResult] ([PayrollResultId] ASC)
  WITH (
      PAD_INDEX = OFF,
      STATISTICS_NORECOMPUTE = OFF,
      SORT_IN_TEMPDB = OFF,
      DROP_EXISTING = OFF,
      ONLINE = OFF,
      ALLOW_ROW_LOCKS = ON,
      ALLOW_PAGE_LOCKS = ON
      ) ON [PRIMARY]
GO

-- ------------------------------------------------------------
-- Modified objects -- re-create (29)
-- ------------------------------------------------------------

/****** Object:  StoredProcedure [dbo].[GetConsolidatedWageTypeResults]    Script Date: 01.03.2026 22:35:19 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Get employee wage type results from a time period
-- denormalized: TenantId/EmployeeId/DivisionId on WageTypeResult enable direct index seek
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

/****** Object:  StoredProcedure [dbo].[GetWageTypeResults]    Script Date: 01.03.2026 22:35:19 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Get employee wage type results from a time period
-- denormalized: TenantId/EmployeeId/DivisionId on WageTypeResult enable direct index seek
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

/****** Object:  UserDefinedFunction [dbo].[GetDerivedRegulations]    Script Date: 01.03.2026 22:35:18 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Get all active derived regulation ids from the payroll
-- =============================================
CREATE FUNCTION [dbo].[GetDerivedRegulations] (
  -- the tenant
  @tenantId AS INT,
  -- the payroll
  @payrollId AS INT,
  -- the regulation valid from date
  @regulationDate AS DATETIME2(7),
  -- the creation date
  @createdBefore AS DATETIME2(7)
  )
RETURNS TABLE
AS
RETURN (
    WITH GroupRegulation AS (
        SELECT [Regulation].[Id],
          [PayrollLayer].[Level],
          [PayrollLayer].[Priority],
          ROW_NUMBER() OVER (
            -- group by regulation name within a payroll layer
            PARTITION BY [PayrollLayer].[Id],
            [Regulation].[Name] ORDER BY [Regulation].[ValidFrom] DESC,
              -- use latest created in case of same valid from
              [Regulation].[Created] DESC
            ) AS RowNumber
        FROM [PayrollLayer]
        INNER JOIN [Regulation]
          ON [PayrollLayer].[RegulationName] = [Regulation].[Name]
        -- active payroll layers and regulations only
        WHERE [Regulation].[Status] = 0
          -- working tenant or shared regulation
          AND (
            [Regulation].[TenantId] = @tenantId
            OR [Regulation].[SharedRegulation] = 1
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

/****** Object:  StoredProcedure [dbo].[GetConsolidatedPayrunResults]    Script Date: 01.03.2026 22:35:19 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Get consolidated payrun results from a time period
-- denormalized: TenantId/EmployeeId/DivisionId on PayrunResult enable direct index seek
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

    DECLARE @startHash INT;
    DECLARE @startHashCount INT;
    SELECT @startHashCount = COUNT(*) FROM OPENJSON(@periodStartHashes);

    IF (@startHashCount = 1)
    BEGIN
        SELECT @startHash = CAST(value AS INT)
        FROM OPENJSON(@periodStartHashes);
    END;

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
    SELECT r.*
    FROM dbo.[PayrunResult] r
    INNER JOIN Winners w ON w.[Id] = r.[Id]
    WHERE w.RowNumber = 1
    OPTION (RECOMPILE);
END
GO

/****** Object:  StoredProcedure [dbo].[GetCollectorResults]    Script Date: 01.03.2026 22:35:19 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Get employee collector results from a time period
-- denormalized: TenantId/EmployeeId/DivisionId on CollectorResult enable direct index seek
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
            AND pj.[JobStatus] & @jobStatus = pj.[JobStatus]
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
            AND pj.[JobStatus] & @jobStatus = pj.[JobStatus]
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

/****** Object:  StoredProcedure [dbo].[GetConsolidatedCollectorResults]    Script Date: 01.03.2026 22:35:19 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Get employee collector results from a time period
-- denormalized: TenantId/EmployeeId/DivisionId on CollectorResult enable direct index seek
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

    DECLARE @startHash INT;
    DECLARE @startHashCount INT;
    SELECT @startHashCount = COUNT(*) FROM OPENJSON(@periodStartHashes);

    IF (@startHashCount = 1)
    BEGIN
        SELECT @startHash = CAST(value AS INT)
        FROM OPENJSON(@periodStartHashes);
    END;

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
                   WHERE pj.[JobStatus] & @jobStatus = pj.[JobStatus]))
          AND (r.[Forecast] IS NULL OR r.[Forecast] = @forecast)
          AND (@noRetro = 0 OR r.[ParentJobId] IS NULL)
          AND (@excludeParentJobId IS NULL OR r.[ParentJobId] IS NULL
               OR r.[ParentJobId] <> @excludeParentJobId)
    )
    SELECT r.*
    FROM dbo.[CollectorResult] r
    INNER JOIN Winners w ON w.[Id] = r.[Id]
    WHERE w.RowNumber = 1
    OPTION (RECOMPILE);
END
GO

/****** Object:  StoredProcedure [dbo].[GetCollectorCustomResults]    Script Date: 01.03.2026 22:35:19 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Get employee collector custom results from a time period
-- denormalized: TenantId/EmployeeId/DivisionId on CollectorCustomResult enable direct index seek
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
            AND pj.[JobStatus] & @jobStatus = pj.[JobStatus]
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
            AND pj.[JobStatus] & @jobStatus = pj.[JobStatus]
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

/****** Object:  StoredProcedure [dbo].[GetConsolidatedCollectorCustomResults]    Script Date: 01.03.2026 22:35:19 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Get consolidated collector custom results
-- denormalized: TenantId/EmployeeId/DivisionId on CollectorCustomResult enable direct index seek
-- eliminates need to scan through PayrollResult → CollectorResult chain
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

    DECLARE @startHash INT;
    DECLARE @startHashCount INT;
    SELECT @startHashCount = COUNT(*) FROM OPENJSON(@periodStartHashes);

    IF (@startHashCount = 1)
    BEGIN
        SELECT @startHash = CAST(value AS INT)
        FROM OPENJSON(@periodStartHashes);
    END;

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
                   WHERE pj.[JobStatus] & @jobStatus = pj.[JobStatus]))
          AND (r.[Forecast] IS NULL OR r.[Forecast] = @forecast)
          AND (@noRetro = 0 OR r.[ParentJobId] IS NULL)
          AND (@excludeParentJobId IS NULL OR r.[ParentJobId] IS NULL
               OR r.[ParentJobId] <> @excludeParentJobId)
    )
    SELECT r.*
    FROM dbo.[CollectorCustomResult] r
    INNER JOIN Winners w ON w.[Id] = r.[Id]
    WHERE w.RowNumber = 1
    OPTION (RECOMPILE);
END
GO

/****** Object:  StoredProcedure [dbo].[DeletePayrunJob]    Script Date: 01.03.2026 22:35:19 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Detete payrun job including all his related objects
--
CREATE PROCEDURE [dbo].[DeletePayrunJob]
  -- the tenant
  @tenantId AS INT,
  -- the payrun job to delete
  @payrunJobId AS INT
AS
BEGIN
  -- SET NOCOUNT ON added to prevent extra result sets from
  -- interfering with SELECT statements
  SET NOCOUNT ON;

  -- payrun results
  DELETE [dbo].[PayrunResult]
  FROM [dbo].[PayrunResult]
  INNER JOIN [dbo].[PayrollResult]
    ON [dbo].[PayrunResult].[PayrollResultId] = [dbo].[PayrollResult].[Id]
  WHERE [dbo].[PayrollResult].[TenantId] = @tenantId
    AND [dbo].[PayrollResult].[PayrunJobId] = @payrunJobId

  -- wage type custom results
  DELETE [dbo].[WageTypeCustomResult]
  FROM [dbo].[WageTypeCustomResult]
  INNER JOIN [dbo].[WageTypeResult]
    ON [dbo].[WageTypeCustomResult].[WageTypeResultId] = [dbo].[WageTypeResult].[Id]
  INNER JOIN [dbo].[PayrollResult]
    ON [dbo].[WageTypeResult].[PayrollResultId] = [dbo].[PayrollResult].[Id]
  WHERE [dbo].[PayrollResult].[TenantId] = @tenantId
    AND [dbo].[PayrollResult].[PayrunJobId] = @payrunJobId

  -- wage type results
  DELETE [dbo].[WageTypeResult]
  FROM [dbo].[WageTypeResult]
  INNER JOIN [dbo].[PayrollResult]
    ON [dbo].[WageTypeResult].[PayrollResultId] = [dbo].[PayrollResult].[Id]
  WHERE [dbo].[PayrollResult].[TenantId] = @tenantId
    AND [dbo].[PayrollResult].[PayrunJobId] = @payrunJobId

  -- collector custom results
  DELETE [dbo].[CollectorCustomResult]
  FROM [dbo].[CollectorCustomResult]
  INNER JOIN [dbo].[CollectorResult]
    ON [dbo].[CollectorCustomResult].[CollectorResultId] = [dbo].[CollectorResult].[Id]
  INNER JOIN [dbo].[PayrollResult]
    ON [dbo].[CollectorResult].[PayrollResultId] = [dbo].[PayrollResult].[Id]
  WHERE [dbo].[PayrollResult].[TenantId] = @tenantId
    AND [dbo].[PayrollResult].[PayrunJobId] = @payrunJobId

  -- collector results
  DELETE [dbo].[CollectorResult]
  FROM [dbo].[CollectorResult]
  INNER JOIN [dbo].[PayrollResult]
    ON [dbo].[CollectorResult].[PayrollResultId] = [dbo].[PayrollResult].[Id]
  WHERE [dbo].[PayrollResult].[TenantId] = @tenantId
    AND [dbo].[PayrollResult].[PayrunJobId] = @payrunJobId

  -- payroll results
  DELETE
  FROM [dbo].[PayrollResult]
  WHERE [dbo].[PayrollResult].[TenantId] = @tenantId
    AND [dbo].[PayrollResult].[PayrunJobId] = @payrunJobId

  -- payrun job emplyoee
  DELETE [dbo].[PayrunJobEmployee]
  FROM [dbo].[PayrunJobEmployee]
  INNER JOIN [dbo].[PayrunJob]
    ON [dbo].[PayrunJobEmployee].[PayrunJobId] = [dbo].[PayrunJob].[Id]
  WHERE [dbo].[PayrunJob].[TenantId] = @tenantId
    AND [dbo].[PayrunJobEmployee].[PayrunJobId] = @payrunJobId

  -- payrun job
  DELETE [dbo].[PayrunJob]
  FROM [dbo].[PayrunJob]
  WHERE [dbo].[PayrunJob].[TenantId] = @tenantId
    AND [dbo].[PayrunJob].[Id] = @payrunJobId
END
GO

/****** Object:  StoredProcedure [dbo].[GetConsolidatedWageTypeCustomResults]    Script Date: 01.03.2026 22:35:19 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Get consolidated custom wage type results
-- denormalized: TenantId/EmployeeId/DivisionId on WageTypeCustomResult enable direct index seek
-- eliminates need to scan through PayrollResult → WageTypeResult chain
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

    DECLARE @startHash INT;
    DECLARE @startHashCount INT;
    SELECT @startHashCount = COUNT(*) FROM OPENJSON(@periodStartHashes);

    IF (@startHashCount = 1)
    BEGIN
        SELECT @startHash = CAST(value AS INT)
        FROM OPENJSON(@periodStartHashes);
    END;

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
    SELECT r.*
    FROM dbo.[WageTypeCustomResult] r
    INNER JOIN Winners w ON w.[Id] = r.[Id]
    WHERE w.RowNumber = 1
    OPTION (RECOMPILE);
END
GO

/****** Object:  StoredProcedure [dbo].[GetWageTypeCustomResults]    Script Date: 01.03.2026 22:35:19 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Get employee wage type custom results from a time period
-- denormalized: TenantId/EmployeeId/DivisionId on WageTypeCustomResult enable direct index seek
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
            AND pj.[JobStatus] & @jobStatus = pj.[JobStatus]
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
            AND pj.[JobStatus] & @jobStatus = pj.[JobStatus]
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

/****** Object:  StoredProcedure [dbo].[DeleteEmployee]    Script Date: 01.03.2026 22:35:19 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Detete employee including all his related objects
--
CREATE PROCEDURE [dbo].[DeleteEmployee]
  -- the employee tenant
  @tenantId AS INT,
  -- the employee to delete
  @employeeId AS INT
AS
BEGIN
  -- SET NOCOUNT ON added to prevent extra result sets from
  -- interfering with SELECT statements
  SET NOCOUNT ON;

  -- employee payroll results
  DELETE [dbo].[PayrunResult]
  FROM [dbo].[PayrunResult]
  INNER JOIN [dbo].[PayrollResult]
    ON [dbo].[PayrunResult].[PayrollResultId] = [dbo].[PayrollResult].[Id]
  WHERE [dbo].[PayrollResult].[TenantId] = @tenantId
    AND [dbo].[PayrollResult].[EmployeeId] = @employeeId

  DELETE [dbo].[WageTypeCustomResult]
  FROM [dbo].[WageTypeCustomResult]
  INNER JOIN [dbo].[WageTypeResult]
    ON [dbo].[WageTypeCustomResult].[WageTypeResultId] = [dbo].[WageTypeResult].[Id]
  INNER JOIN [dbo].[PayrollResult]
    ON [dbo].[WageTypeResult].[PayrollResultId] = [dbo].[PayrollResult].[Id]
  WHERE [dbo].[PayrollResult].[TenantId] = @tenantId
    AND [dbo].[PayrollResult].[EmployeeId] = @employeeId

  DELETE [dbo].[WageTypeResult]
  FROM [dbo].[WageTypeResult]
  INNER JOIN [dbo].[PayrollResult]
    ON [dbo].[WageTypeResult].[PayrollResultId] = [dbo].[PayrollResult].[Id]
  WHERE [dbo].[PayrollResult].[TenantId] = @tenantId
    AND [dbo].[PayrollResult].[EmployeeId] = @employeeId

  DELETE [dbo].[CollectorCustomResult]
  FROM [dbo].[CollectorCustomResult]
  INNER JOIN [dbo].[CollectorResult]
    ON [dbo].[CollectorCustomResult].[CollectorResultId] = [dbo].[CollectorResult].[Id]
  INNER JOIN [dbo].[PayrollResult]
    ON [dbo].[CollectorResult].[PayrollResultId] = [dbo].[PayrollResult].[Id]
  WHERE [dbo].[PayrollResult].[TenantId] = @tenantId
    AND [dbo].[PayrollResult].[EmployeeId] = @employeeId

  DELETE [dbo].[CollectorResult]
  FROM [dbo].[CollectorResult]
  INNER JOIN [dbo].[PayrollResult]
    ON [dbo].[CollectorResult].[PayrollResultId] = [dbo].[PayrollResult].[Id]
  WHERE [dbo].[PayrollResult].[TenantId] = @tenantId
    AND [dbo].[PayrollResult].[EmployeeId] = @employeeId

  DELETE
  FROM [dbo].[PayrollResult]
  WHERE [TenantId] = @tenantId
    AND [EmployeeId] = @employeeId

  -- employee payrun jobs
  DELETE [dbo].[PayrunJobEmployee]
  FROM [dbo].[PayrunJobEmployee]
  INNER JOIN [dbo].[PayrunJob]
    ON [dbo].[PayrunJobEmployee].[PayrunJobId] = [dbo].[PayrunJob].[Id]
  WHERE [dbo].[PayrunJob].[TenantId] = @tenantId
    AND [dbo].[PayrunJobEmployee].[EmployeeId] = @employeeId

  -- employee
  DELETE [dbo].[EmployeeCaseValueChange]
  FROM [dbo].[EmployeeCaseValueChange]
  INNER JOIN [dbo].[EmployeeCaseChange]
    ON [dbo].[EmployeeCaseValueChange].[CaseChangeId] = [dbo].[EmployeeCaseChange].[Id]
  INNER JOIN [dbo].[Employee]
    ON [dbo].[EmployeeCaseChange].[EmployeeId] = [dbo].[Employee].[Id]
  WHERE [dbo].[Employee].[TenantId] = @tenantId
    AND [dbo].[Employee].[Id] = @employeeId

  DELETE [dbo].[EmployeeCaseChange]
  FROM [dbo].[EmployeeCaseChange]
  INNER JOIN [dbo].[Employee]
    ON [dbo].[EmployeeCaseChange].[EmployeeId] = [dbo].[Employee].[Id]
  WHERE [dbo].[Employee].[TenantId] = @tenantId
    AND [dbo].[Employee].[Id] = @employeeId

  DELETE [dbo].[EmployeeCaseDocument]
  FROM [dbo].[EmployeeCaseDocument]
  INNER JOIN [dbo].[EmployeeCaseValue]
    ON [dbo].[EmployeeCaseDocument].[CaseValueId] = [dbo].[EmployeeCaseValue].[Id]
  INNER JOIN [dbo].[Employee]
    ON [dbo].[EmployeeCaseValue].[EmployeeId] = [dbo].[Employee].[Id]
  WHERE [dbo].[Employee].[TenantId] = @tenantId
    AND [dbo].[Employee].[Id] = @employeeId

  DELETE [dbo].[EmployeeCaseValue]
  FROM [dbo].[EmployeeCaseValue]
  INNER JOIN [dbo].[Employee]
    ON [dbo].[EmployeeCaseValue].[EmployeeId] = [dbo].[Employee].[Id]
  WHERE [dbo].[Employee].[TenantId] = @tenantId
    AND [dbo].[Employee].[Id] = @employeeId

  DELETE [dbo].[EmployeeDivision]
  FROM [dbo].[EmployeeDivision]
  INNER JOIN [dbo].[Employee]
    ON [dbo].[EmployeeDivision].[EmployeeId] = [dbo].[Employee].[Id]
  WHERE [dbo].[Employee].[TenantId] = @tenantId
    AND [dbo].[Employee].[Id] = @employeeId

  DELETE
  FROM [dbo].[Employee]
  WHERE [TenantId] = @tenantId
    AND [Id] = @employeeId
END
GO

/****** Object:  Index [IX_CaseRelation.SourceCaseName]    Script Date: 01.03.2026 22:35:19 ******/
CREATE NONCLUSTERED INDEX [IX_CaseRelation.SourceCaseName] ON [dbo].[CaseRelation] (
  [RegulationId] ASC,
  [SourceCaseName] ASC
  )
  WITH (
      PAD_INDEX = OFF,
      STATISTICS_NORECOMPUTE = OFF,
      SORT_IN_TEMPDB = OFF,
      DROP_EXISTING = OFF,
      ONLINE = OFF,
      ALLOW_ROW_LOCKS = ON,
      ALLOW_PAGE_LOCKS = ON
      ) ON [PRIMARY]
GO

/****** Object:  Index [IX_CaseRelation.TargetCaseName]    Script Date: 01.03.2026 22:35:19 ******/
CREATE NONCLUSTERED INDEX [IX_CaseRelation.TargetCaseName] ON [dbo].[CaseRelation] (
  [RegulationId] ASC,
  [TargetCaseName] ASC
  )
  WITH (
      PAD_INDEX = OFF,
      STATISTICS_NORECOMPUTE = OFF,
      SORT_IN_TEMPDB = OFF,
      DROP_EXISTING = OFF,
      ONLINE = OFF,
      ALLOW_ROW_LOCKS = ON,
      ALLOW_PAGE_LOCKS = ON
      ) ON [PRIMARY]
GO

/****** Object:  Index [IX_CaseRelation.TargetSlot]    Script Date: 01.03.2026 22:35:19 ******/
CREATE NONCLUSTERED INDEX [IX_CaseRelation.TargetSlot] ON [dbo].[CaseRelation] (
  [RegulationId] ASC,
  [TargetCaseSlot] ASC
  )
  WITH (
      PAD_INDEX = OFF,
      STATISTICS_NORECOMPUTE = OFF,
      SORT_IN_TEMPDB = OFF,
      DROP_EXISTING = OFF,
      ONLINE = OFF,
      ALLOW_ROW_LOCKS = ON,
      ALLOW_PAGE_LOCKS = ON
      ) ON [PRIMARY]
GO

/****** Object:  StoredProcedure [dbo].[DeleteLookup]    Script Date: 01.03.2026 22:35:19 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Detete lookup including the lookup audit
--
CREATE PROCEDURE [dbo].[DeleteLookup]
  -- the tenant with the lookup
  @tenantId AS INT,
  -- the p to delete
  @lookupId AS INT
AS
BEGIN
  -- SET NOCOUNT ON added to prevent extra result sets from
  -- interfering with SELECT statements
  SET NOCOUNT ON;

  -- lookup
  DELETE [dbo].[LookupValueAudit]
  FROM [dbo].[LookupValueAudit]
  INNER JOIN [dbo].[LookupValue]
    ON [dbo].[LookupValueAudit].[LookupValueId] = [dbo].[LookupValue].[Id]
  INNER JOIN [dbo].[Lookup]
    ON [dbo].[LookupValue].[LookupId] = [dbo].[Lookup].[Id]
  INNER JOIN [dbo].[Regulation]
    ON [dbo].[Lookup].[RegulationId] = [dbo].[Regulation].[Id]
  WHERE [dbo].[Regulation].[TenantId] = @tenantId
    AND [dbo].[Lookup].Id = @lookupId

  DELETE [dbo].[LookupValue]
  FROM [dbo].[LookupValue]
  INNER JOIN [dbo].[Lookup]
    ON [dbo].[LookupValue].[LookupId] = [dbo].[Lookup].[Id]
  INNER JOIN [dbo].[Regulation]
    ON [dbo].[Lookup].[RegulationId] = [dbo].[Regulation].[Id]
  WHERE [dbo].[Regulation].[TenantId] = @tenantId
    AND [dbo].[Lookup].Id = @lookupId

  DELETE [dbo].[LookupAudit]
  FROM [dbo].[LookupAudit]
  INNER JOIN [dbo].[Lookup]
    ON [dbo].[LookupAudit].[LookupId] = [dbo].[Lookup].[Id]
  INNER JOIN [dbo].[Regulation]
    ON [dbo].[Lookup].[RegulationId] = [dbo].[Regulation].[Id]
  WHERE [dbo].[Regulation].[TenantId] = @tenantId
    AND [dbo].[Lookup].Id = @lookupId

  DELETE [dbo].[Lookup]
  FROM [dbo].[Lookup]
  INNER JOIN [dbo].[Regulation]
    ON [dbo].[Lookup].[RegulationId] = [dbo].[Regulation].[Id]
  WHERE [dbo].[Regulation].[TenantId] = @tenantId
    AND [dbo].[Lookup].Id = @lookupId
END
GO

/****** Object:  StoredProcedure [dbo].[GetDerivedCaseFields]    Script Date: 01.03.2026 22:35:19 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Get the derived case fields of payroll (only active).
-- =============================================
CREATE PROCEDURE [dbo].[GetDerivedCaseFields]
  -- the tenant
  @tenantId AS INT,
  -- the payroll
  @payrollId AS INT,
  -- the regulation valid from date
  @regulationDate AS DATETIME2(7),
  -- creation date
  @createdBefore AS DATETIME2(7),
  -- name of the case fields: JSON array of VARCHAR(128)
  @caseFieldNames AS VARCHAR(MAX) = NULL,
  -- the include clusters: JSON array of cluster names VARCHAR(128)
  @includeClusters AS VARCHAR(MAX) = NULL,
  -- the exclude clusters: JSON array of cluster names VARCHAR(128)
  @excludeClusters AS VARCHAR(MAX) = NULL
AS
BEGIN
  -- SET NOCOUNT ON added to prevent extra result sets from
  -- interfering with SELECT statements
  SET NOCOUNT ON;

  -- select the case fields by name, using the order of the payroll level
  SELECT [Regulations].[Id] AS [RegulationId],
    [Regulations].[Level] AS [Level],
    [Regulations].[Priority] AS [Priority],
    dbo.[Case].[Id] AS [CaseId],
    dbo.[Case].[CaseType] AS [CaseType],
    dbo.[CaseField].*
  FROM dbo.[CaseField]
  INNER JOIN dbo.[Case]
    ON dbo.[CaseField].[CaseId] = dbo.[Case].[Id]
  INNER JOIN dbo.GetDerivedRegulations(@tenantId, @payrollId, @regulationDate, @createdBefore) AS [Regulations]
    ON dbo.[Case].[RegulationId] = [Regulations].[Id]
  -- active case fields only
  WHERE dbo.[CaseField].[Status] = 0
    AND dbo.[CaseField].[Created] <= @createdBefore
    AND (
      (
        @includeClusters IS NULL
        AND @excludeClusters IS NULL
        )
      OR dbo.IsMatchingCluster(@includeClusters, @excludeClusters, dbo.[CaseField].[Clusters]) = 1
      )
    AND (
      @caseFieldNames IS NULL
      OR LOWER(dbo.[CaseField].[Name]) IN (
        SELECT LOWER(value)
        FROM OPENJSON(@caseFieldNames)
        )
      )
  -- sort order
  ORDER BY [Level] DESC,
    [Priority] DESC
END
GO

/****** Object:  StoredProcedure [dbo].[GetDerivedCaseFieldsOfCase]    Script Date: 01.03.2026 22:35:19 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Get the derived case fields of payroll (only active).
-- =============================================
CREATE PROCEDURE [dbo].[GetDerivedCaseFieldsOfCase]
  -- the tenant
  @tenantId AS INT,
  -- the payroll
  @payrollId AS INT,
  -- the regulation valid from date
  @regulationDate AS DATETIME2(7),
  -- creation date
  @createdBefore AS DATETIME2(7),
  -- name of the cases: JSON array of VARCHAR(128)
  @caseNames AS VARCHAR(MAX) = NULL,
  -- the include clusters: JSON array of cluster names VARCHAR(128)
  @includeClusters AS VARCHAR(MAX) = NULL,
  -- the exclude clusters: JSON array of cluster names VARCHAR(128)
  @excludeClusters AS VARCHAR(MAX) = NULL
AS
BEGIN
  -- SET NOCOUNT ON added to prevent extra result sets from
  -- interfering with SELECT statements
  SET NOCOUNT ON;

  -- select the case fields by name, using the order of the payroll level
  SELECT [Regulations].[Id] AS [RegulationId],
    [Regulations].[Level] AS [Level],
    [Regulations].[Priority] AS [Priority],
    dbo.[Case].[Id] AS [CaseId],
    dbo.[Case].[CaseType] AS [CaseType],
    dbo.[CaseField].*
  FROM dbo.[CaseField]
  INNER JOIN dbo.[Case]
    ON dbo.[CaseField].[CaseId] = dbo.[Case].[Id]
  INNER JOIN dbo.GetDerivedRegulations(@tenantId, @payrollId, @regulationDate, @createdBefore) AS [Regulations]
    ON dbo.[Case].[RegulationId] = [Regulations].[Id]
  -- active case fields only
  WHERE dbo.[CaseField].[Status] = 0
    AND dbo.[CaseField].[Created] <= @createdBefore
    AND (
      (
        @includeClusters IS NULL
        AND @excludeClusters IS NULL
        )
      OR dbo.IsMatchingCluster(@includeClusters, @excludeClusters, dbo.[CaseField].[Clusters]) = 1
      )
    AND (
      @caseNames IS NULL
      OR LOWER(dbo.[Case].[Name]) IN (
        SELECT LOWER(value)
        FROM OPENJSON(@caseNames)
        )
      )
  -- derived order by sort order
  ORDER BY [Level] DESC,
    [Priority] DESC
END
GO

/****** Object:  StoredProcedure [dbo].[GetDerivedCaseRelations]    Script Date: 01.03.2026 22:35:19 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Get derived case relations (only active).
-- =============================================
CREATE PROCEDURE [dbo].[GetDerivedCaseRelations]
  -- the tenant
  @tenantId AS INT,
  -- the payroll
  @payrollId AS INT,
  -- the regulation valid from date
  @regulationDate AS DATETIME2(7),
  -- creation date
  @createdBefore AS DATETIME2(7),
  -- the from case name
  @sourceCaseName AS NVARCHAR(128) = NULL,
  -- the to case name
  @targetCaseName AS NVARCHAR(128) = NULL,
  -- the include clusters: JSON array of cluster names VARCHAR(128)
  @includeClusters AS VARCHAR(MAX) = NULL,
  -- the exclude clusters: JSON array of cluster names VARCHAR(128)
  @excludeClusters AS VARCHAR(MAX) = NULL
AS
BEGIN
  -- SET NOCOUNT ON added to prevent extra result sets from
  -- interfering with SELECT statements
  SET NOCOUNT ON;

  -- select all active case relation, using the from/to case name and payroll level
  SELECT [Regulations].[Id] AS [RegulationId],
    [Regulations].[Level] AS [Level],
    [Regulations].[Priority] AS [Priority],
    -- perfomance hint: don't use [CaseRelation].*
    dbo.[CaseRelation].[Id],
    dbo.[CaseRelation].[Status],
    dbo.[CaseRelation].[Created],
    dbo.[CaseRelation].[Updated],
    dbo.[CaseRelation].[RegulationId],
    dbo.[CaseRelation].[SourceCaseName],
    dbo.[CaseRelation].[SourceCaseNameLocalizations],
    dbo.[CaseRelation].[SourceCaseSlot],
    dbo.[CaseRelation].[SourceCaseSlotLocalizations],
    dbo.[CaseRelation].[TargetCaseName],
    dbo.[CaseRelation].[TargetCaseNameLocalizations],
    dbo.[CaseRelation].[TargetCaseSlot],
    dbo.[CaseRelation].[TargetCaseSlotLocalizations],
    dbo.[CaseRelation].[RelationHash],
    dbo.[CaseRelation].[BuildExpression],
    dbo.[CaseRelation].[ValidateExpression],
    dbo.[CaseRelation].[OverrideType],
    dbo.[CaseRelation].[Order],
    --   dbo.[CaseRelation].[Binary],
    dbo.[CaseRelation].[ScriptHash],
    dbo.[CaseRelation].[Attributes],
    dbo.[CaseRelation].[Clusters],
    dbo.[CaseRelation].[BuildActions],
    dbo.[CaseRelation].[ValidateActions]
  -- excluded columns
  --dbo.[CaseRelation].[Script],
  --dbo.[CaseRelation].[ScriptVersion]
  FROM dbo.[CaseRelation]
  INNER JOIN dbo.GetDerivedRegulations(@tenantId, @payrollId, @regulationDate, @createdBefore) AS [Regulations]
    ON dbo.[CaseRelation].[RegulationId] = [Regulations].[Id]
  -- active case relation only
  WHERE dbo.[CaseRelation].[Status] = 0
    AND dbo.[CaseRelation].[Created] <= @createdBefore
    AND (
      @sourceCaseName IS NULL
      OR LOWER(dbo.[CaseRelation].[SourceCaseName]) = LOWER(@sourceCaseName)
      )
    AND (
      @targetCaseName IS NULL
      OR LOWER(dbo.[CaseRelation].[TargetCaseName]) = LOWER(@targetCaseName)
      )
    AND (
      (
        @includeClusters IS NULL
        AND @excludeClusters IS NULL
        )
      OR dbo.IsMatchingCluster(@includeClusters, @excludeClusters, dbo.[CaseRelation].[Clusters]) = 1
      )
  -- derived order by case relation source/target case names
  ORDER BY dbo.[CaseRelation].[SourceCaseName],
    dbo.[CaseRelation].[TargetCaseName],
    [Level] DESC,
    [Priority] DESC
END
GO

/****** Object:  StoredProcedure [dbo].[GetDerivedCases]    Script Date: 01.03.2026 22:35:19 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Get the topmost derived cases (only active).
-- =============================================
CREATE PROCEDURE [dbo].[GetDerivedCases]
  -- the tenant
  @tenantId AS INT,
  -- the payroll
  @payrollId AS INT,
  -- the regulation valid from date
  @regulationDate AS DATETIME2(7),
  -- creation date
  @createdBefore AS DATETIME2(7),
  -- the case type
  @caseType AS INT = NULL,
  -- the case names: JSON array of VARCHAR(128)
  @caseNames AS VARCHAR(MAX) = NULL,
  -- the include clusters: JSON array of cluster names VARCHAR(128)
  @includeClusters AS VARCHAR(MAX) = NULL,
  -- the exclude clusters: JSON array of cluster names VARCHAR(128)
  @excludeClusters AS VARCHAR(MAX) = NULL,
  -- hidden case filter
  @hidden AS BIT = NULL
AS
BEGIN
  -- SET NOCOUNT ON added to prevent extra result sets from
  -- interfering with SELECT statements
  SET NOCOUNT ON;

  -- select all active cases, using the order of case name and payroll level
  SELECT [Regulations].[Id] AS [RegulationId],
    [Regulations].[Level] AS [Level],
    [Regulations].[Priority] AS [Priority],
    -- perfomance hint: don't use [Case].*
    dbo.[Case].[Id],
    dbo.[Case].[Status],
    dbo.[Case].[Created],
    dbo.[Case].[Updated],
    dbo.[Case].[RegulationId],
    dbo.[Case].[CaseType],
    dbo.[Case].[Name],
    dbo.[Case].[NameLocalizations],
    dbo.[Case].[NameSynonyms],
    dbo.[Case].[Description],
    dbo.[Case].[DescriptionLocalizations],
    dbo.[Case].[DefaultReason],
    dbo.[Case].[DefaultReasonLocalizations],
    dbo.[Case].[BaseCase],
    dbo.[Case].[BaseCaseFields],
    dbo.[Case].[OverrideType],
    dbo.[Case].[CancellationType],
    dbo.[Case].[AvailableExpression],
    dbo.[Case].[BuildExpression],
    dbo.[Case].[ValidateExpression],
    dbo.[Case].[Lookups],
    dbo.[Case].[Slots],
    --   dbo.[Case].[Binary],
    dbo.[Case].[ScriptHash],
    dbo.[Case].[Attributes],
    dbo.[Case].[Clusters],
    dbo.[Case].[AvailableActions],
    dbo.[Case].[BuildActions],
    dbo.[Case].[ValidateActions]
  -- excluded columns
  --dbo.[Case].[Script],
  --dbo.[Case].[ScriptVersion]
  FROM dbo.[Case]
  INNER JOIN dbo.GetDerivedRegulations(@tenantId, @payrollId, @regulationDate, @createdBefore) AS [Regulations]
    ON dbo.[Case].[RegulationId] = [Regulations].[Id]
  -- active cases only
  WHERE dbo.[Case].[Status] = 0
    AND dbo.[Case].[Created] <= @createdBefore
    -- hidden filter
    AND (
      @hidden IS NULL
      OR dbo.[Case].[Hidden] = @hidden
      )
    -- case type filter
    AND (
      @caseType IS NULL
      OR dbo.[Case].[CaseType] = @caseType
      )
    -- clusters filter
    AND (
      (
        @includeClusters IS NULL
        AND @excludeClusters IS NULL
        )
      OR dbo.IsMatchingCluster(@includeClusters, @excludeClusters, dbo.[Case].[Clusters]) = 1
      )
    -- case names filter
    AND (
      @caseNames IS NULL
      OR LOWER(dbo.[Case].[Name]) IN (
        SELECT LOWER(value)
        FROM OPENJSON(@caseNames)
        )
      )
  -- derived order by sort order
  ORDER BY [Level] DESC,
    [Priority] DESC
END
GO

/****** Object:  StoredProcedure [dbo].[GetDerivedCollectors]    Script Date: 01.03.2026 22:35:19 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Get the topmost derived collectors (only active).
-- =============================================
CREATE PROCEDURE [dbo].[GetDerivedCollectors]
  -- the tenant
  @tenantId AS INT,
  -- the payroll
  @payrollId AS INT,
  -- the regulation valid from date
  @regulationDate AS DATETIME2(7),
  -- creation date
  @createdBefore AS DATETIME2(7),
  -- the collector names: JSON array of VARCHAR(128)
  @collectorNames AS VARCHAR(MAX) = NULL,
  -- the include clusters: JSON array of cluster names VARCHAR(128)
  @includeClusters AS VARCHAR(MAX) = NULL,
  -- the exclude clusters: JSON array of cluster names VARCHAR(128)
  @excludeClusters AS VARCHAR(MAX) = NULL
AS
BEGIN
  -- SET NOCOUNT ON added to prevent extra result sets from
  -- interfering with SELECT statements
  SET NOCOUNT ON;

  -- select all active collectors, using the order of collector name and payroll level
  SELECT [Regulations].[Id] AS [RegulationId],
    [Regulations].[Level] AS [Level],
    [Regulations].[Priority] AS [Priority],
    -- perfomance hint: don't use [Collector].*
    dbo.[Collector].[Id],
    dbo.[Collector].[Status],
    dbo.[Collector].[Created],
    dbo.[Collector].[Updated],
    dbo.[Collector].[RegulationId],
    dbo.[Collector].[Name],
    dbo.[Collector].[NameLocalizations],
    dbo.[Collector].[CollectMode],
    dbo.[Collector].[Negated],
    dbo.[Collector].[OverrideType],
    dbo.[Collector].[Culture],
    dbo.[Collector].[CollectorGroups],
    dbo.[Collector].[StartExpression],
    dbo.[Collector].[ApplyExpression],
    dbo.[Collector].[EndExpression],
    dbo.[Collector].[StartActions],
    dbo.[Collector].[ApplyActions],
    dbo.[Collector].[EndActions],
    dbo.[Collector].[Threshold],
    dbo.[Collector].[MinResult],
    dbo.[Collector].[MaxResult],
    --   dbo.[Collector].[Binary],
    dbo.[Collector].[ScriptHash],
    dbo.[Collector].[Attributes],
    dbo.[Collector].[Clusters]
  -- excluded columns
  --dbo.[Collector].[Script],
  --dbo.[Collector].[ScriptVersion]
  FROM dbo.[Collector]
  INNER JOIN dbo.GetDerivedRegulations(@tenantId, @payrollId, @regulationDate, @createdBefore) AS [Regulations]
    ON dbo.[Collector].[RegulationId] = [Regulations].[Id]
  -- active collectors only
  WHERE dbo.[Collector].[Status] = 0
    AND dbo.[Collector].[Created] <= @createdBefore
    AND (
      (
        @includeClusters IS NULL
        AND @excludeClusters IS NULL
        )
      OR dbo.IsMatchingCluster(@includeClusters, @excludeClusters, dbo.[Collector].[Clusters]) = 1
      )
    AND (
      @collectorNames IS NULL
      OR LOWER(dbo.[Collector].[Name]) IN (
        SELECT LOWER(value)
        FROM OPENJSON(@collectorNames)
        )
      )
  -- derived order by collector name
  ORDER BY dbo.[Collector].[Name],
    [Level] DESC,
    [Priority] DESC
END
GO

/****** Object:  StoredProcedure [dbo].[GetDerivedLookups]    Script Date: 01.03.2026 22:35:19 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Get the topmost derived lookups (only active).
-- =============================================
CREATE PROCEDURE [dbo].[GetDerivedLookups]
  -- the tenant
  @tenantId AS INT,
  -- the payroll
  @payrollId AS INT,
  -- the regulation valid from date
  @regulationDate AS DATETIME2(7),
  -- creation date
  @createdBefore AS DATETIME2(7),
  -- the lookup names: JSON array of VARCHAR(128)
  @lookupNames AS VARCHAR(MAX) = NULL
AS
BEGIN
  -- SET NOCOUNT ON added to prevent extra result sets from
  -- interfering with SELECT statements
  SET NOCOUNT ON;

  -- select all active lookups, using the order of lookup name and payroll level
  SELECT [Regulations].[Id] AS [RegulationId],
    [Regulations].[Level] AS [Level],
    [Regulations].[Priority] AS [Priority],
    dbo.[Lookup].*
  FROM dbo.[Lookup]
  INNER JOIN dbo.GetDerivedRegulations(@tenantId, @payrollId, @regulationDate, @createdBefore) AS [Regulations]
    ON dbo.[Lookup].[RegulationId] = [Regulations].[Id]
  -- active lookups only
  WHERE dbo.[Lookup].[Status] = 0
    AND dbo.[Lookup].[Created] <= @createdBefore
    AND (
      @lookupNames IS NULL
      OR LOWER(dbo.[Lookup].[Name]) IN (
        SELECT LOWER(value)
        FROM OPENJSON(@lookupNames)
        )
      )
  -- derived order by sort order
  ORDER BY [Level] DESC,
    [Priority] DESC
END
GO

/****** Object:  StoredProcedure [dbo].[GetDerivedLookupValues]    Script Date: 01.03.2026 22:35:19 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Get the topmost derived lookup values (only active).
-- =============================================
CREATE PROCEDURE [dbo].[GetDerivedLookupValues]
  -- the tenant
  @tenantId AS INT,
  -- the payroll
  @payrollId AS INT,
  -- the regulation valid from date
  @regulationDate AS DATETIME2(7),
  -- creation date
  @createdBefore AS DATETIME2(7),
  -- the lookup names: JSON array of VARCHAR(128)
  @lookupNames AS VARCHAR(MAX) = NULL,
  -- the lookup keys: JSON array of VARCHAR(128)
  @lookupKeys AS VARCHAR(MAX) = NULL
AS
BEGIN
  -- SET NOCOUNT ON added to prevent extra result sets from
  -- interfering with SELECT statements
  SET NOCOUNT ON;

  -- select all active lookup parameters, using the order of lookup name and payroll level
  SELECT [Regulations].[Id] AS [RegulationId],
    [Regulations].[Level] AS [Level],
    [Regulations].[Priority] AS [Priority],
    dbo.[LookupValue].*
  FROM dbo.[LookupValue]
  INNER JOIN dbo.[Lookup]
    ON dbo.[LookupValue].[LookupId] = dbo.[Lookup].[Id]
  INNER JOIN dbo.GetDerivedRegulations(@tenantId, @payrollId, @regulationDate, @createdBefore) AS [Regulations]
    ON dbo.[Lookup].[RegulationId] = [Regulations].[Id]
  -- active lookups only
  WHERE dbo.[LookupValue].[Status] = 0
    AND dbo.[LookupValue].[Created] <= @createdBefore
    AND (
      @lookupNames IS NULL
      OR LOWER(dbo.[Lookup].[Name]) IN (
        SELECT LOWER(value)
        FROM OPENJSON(@lookupNames)
        )
      )
    AND (
      -- case sensitive lookup value key
      @lookupKeys IS NULL
      OR dbo.[LookupValue].[Key] IN (
        SELECT value
        FROM OPENJSON(@lookupKeys)
        )
      )
  -- derived order by sort order
  ORDER BY [Level] DESC,
    [Priority] DESC
END
GO

/****** Object:  StoredProcedure [dbo].[GetDerivedReportParameters]    Script Date: 01.03.2026 22:35:19 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Get the topmost derived report parameters (only active).
-- =============================================
CREATE PROCEDURE [dbo].[GetDerivedReportParameters]
  -- the tenant
  @tenantId AS INT,
  -- the payroll
  @payrollId AS INT,
  -- the regulation valid from date
  @regulationDate AS DATETIME2(7),
  -- creation date
  @createdBefore AS DATETIME2(7),
  -- the report names: JSON array of VARCHAR(128)
  @reportNames AS VARCHAR(MAX) = NULL
AS
BEGIN
  -- SET NOCOUNT ON added to prevent extra result sets from
  -- interfering with SELECT statements
  SET NOCOUNT ON;

  -- select all active report parameters, using the order of report name and payroll level
  SELECT [Regulations].[Id] AS [RegulationId],
    [Regulations].[Level] AS [Level],
    [Regulations].[Priority] AS [Priority],
    dbo.[ReportParameter].*
  FROM dbo.[ReportParameter]
  INNER JOIN dbo.[Report]
    ON dbo.[ReportParameter].[ReportId] = dbo.[Report].[Id]
  INNER JOIN dbo.GetDerivedRegulations(@tenantId, @payrollId, @regulationDate, @createdBefore) AS [Regulations]
    ON dbo.[Report].[RegulationId] = [Regulations].[Id]
  -- active reports only
  WHERE dbo.[ReportParameter].[Status] = 0
    AND dbo.[ReportParameter].[Created] <= @createdBefore
    AND (
      @reportNames IS NULL
      OR LOWER(dbo.[Report].[Name]) IN (
        SELECT LOWER(value)
        FROM OPENJSON(@reportNames)
        )
      )
  -- derived order by sort order
  ORDER BY [Level] DESC,
    [Priority] DESC
END
GO

/****** Object:  StoredProcedure [dbo].[GetDerivedReports]    Script Date: 01.03.2026 22:35:19 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Get the topmost derived reports (only active).
-- =============================================
CREATE PROCEDURE [dbo].[GetDerivedReports]
  -- the tenant
  @tenantId AS INT,
  -- the payroll
  @payrollId AS INT,
  -- the regulation valid from date
  @regulationDate AS DATETIME2(7),
  -- creation date
  @createdBefore AS DATETIME2(7),
  -- user type
  @userType AS INT = NULL,
  -- the report names: JSON array of VARCHAR(128)
  @reportNames AS VARCHAR(MAX) = NULL,
  -- the include clusters: JSON array of cluster names VARCHAR(128)
  @includeClusters AS VARCHAR(MAX) = NULL,
  -- the exclude clusters: JSON array of cluster names VARCHAR(128)
  @excludeClusters AS VARCHAR(MAX) = NULL
AS
BEGIN
  -- SET NOCOUNT ON added to prevent extra result sets from
  -- interfering with SELECT statements
  SET NOCOUNT ON;

  -- select all active reports, using the order of report name and payroll level
  SELECT [Regulations].[Id] AS [RegulationId],
    [Regulations].[Level] AS [Level],
    [Regulations].[Priority] AS [Priority],
    -- perfomance hint: don't use [Report].*
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
    --  dbo.[Report].[Binary],
    dbo.[Report].[ScriptHash],
    dbo.[Report].[Attributes],
    dbo.[Report].[Clusters]
  -- excluded columns
  --dbo.[Report].[Script],
  --dbo.[Report].[ScriptVersion]
  FROM dbo.[Report]
  INNER JOIN dbo.GetDerivedRegulations(@tenantId, @payrollId, @regulationDate, @createdBefore) AS [Regulations]
    ON dbo.[Report].[RegulationId] = [Regulations].[Id]
  -- active reports only
  WHERE dbo.[Report].[Status] = 0
    AND dbo.[Report].[Created] <= @createdBefore
    AND (
      @userType IS NULL
      OR dbo.[Report].[UserType] <= @userType
      )
    AND (
      (
        @includeClusters IS NULL
        AND @excludeClusters IS NULL
        )
      OR dbo.IsMatchingCluster(@includeClusters, @excludeClusters, dbo.[Report].[Clusters]) = 1
      )
    AND (
      @reportNames IS NULL
      OR LOWER(dbo.[Report].[Name]) IN (
        SELECT LOWER(value)
        FROM OPENJSON(@reportNames)
        )
      )
  -- derived order by sort order
  ORDER BY [Level] DESC,
    [Priority] DESC
END
GO

/****** Object:  StoredProcedure [dbo].[GetDerivedReportTemplates]    Script Date: 01.03.2026 22:35:19 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Get the topmost derived report templates (only active).
-- =============================================
CREATE PROCEDURE [dbo].[GetDerivedReportTemplates]
  -- the tenant
  @tenantId AS INT,
  -- the payroll
  @payrollId AS INT,
  -- the regulation valid from date
  @regulationDate AS DATETIME2(7),
  -- creation date
  @createdBefore AS DATETIME2(7),
  -- the report names: JSON array of VARCHAR(128)
  @reportNames AS VARCHAR(MAX) = NULL,
  -- the report culture
  @culture AS VARCHAR(128) = NULL
AS
BEGIN
  -- SET NOCOUNT ON added to prevent extra result sets from
  -- interfering with SELECT statements
  SET NOCOUNT ON;

  -- select all active report templates, using the order of report name and payroll level
  SELECT [Regulations].[Id] AS [RegulationId],
    [Regulations].[Level] AS [Level],
    [Regulations].[Priority] AS [Priority],
    dbo.[ReportTemplate].*
  FROM dbo.[ReportTemplate]
  INNER JOIN dbo.[Report]
    ON dbo.[ReportTemplate].[ReportId] = dbo.[Report].[Id]
  INNER JOIN dbo.GetDerivedRegulations(@tenantId, @payrollId, @regulationDate, @createdBefore) AS [Regulations]
    ON dbo.[Report].[RegulationId] = [Regulations].[Id]
  -- active reports only
  WHERE dbo.[ReportTemplate].[Status] = 0
    AND dbo.[ReportTemplate].[Created] <= @createdBefore
    AND (
      @reportNames IS NULL
      OR LOWER(dbo.[Report].[Name]) IN (
        SELECT LOWER(value)
        FROM OPENJSON(@reportNames)
        )
      )
    AND (
      @culture IS NULL
      OR dbo.[ReportTemplate].[Culture] = @culture
      )
  -- derived order by sort order
  ORDER BY [Level] DESC,
    [Priority] DESC
END
GO

/****** Object:  StoredProcedure [dbo].[GetDerivedScripts]    Script Date: 01.03.2026 22:35:19 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Get the topmost derived scripts (only active).
-- =============================================
CREATE PROCEDURE [dbo].[GetDerivedScripts]
  -- the tenant
  @tenantId AS INT,
  -- the payroll
  @payrollId AS INT,
  -- the regulation valid from date
  @regulationDate AS DATETIME2(7),
  -- creation date
  @createdBefore AS DATETIME2(7),
  -- the script names: JSON array of VARCHAR(128)
  @scriptNames AS VARCHAR(MAX) = NULL
AS
BEGIN
  -- SET NOCOUNT ON added to prevent extra result sets from
  -- interfering with SELECT statements
  SET NOCOUNT ON;

  -- select all active scripts, using the order of script name and payroll level
  SELECT [Regulations].[Id] AS [RegulationId],
    [Regulations].[Level] AS [Level],
    [Regulations].[Priority] AS [Priority],
    -- perfomance hint: don't use [Script].*
    dbo.[Script].[Id],
    dbo.[Script].[Status],
    dbo.[Script].[Created],
    dbo.[Script].[Updated],
    dbo.[Script].[RegulationId],
    dbo.[Script].[Name],
    dbo.[Script].[FunctionTypeMask],
    dbo.[Script].[Value]
  FROM dbo.[Script]
  INNER JOIN dbo.GetDerivedRegulations(@tenantId, @payrollId, @regulationDate, @createdBefore) AS [Regulations]
    ON dbo.[Script].[RegulationId] = [Regulations].[Id]
  -- active scripts only
  WHERE dbo.[Script].[Status] = 0
    AND dbo.[Script].[Created] <= @createdBefore
    AND (
      @scriptNames IS NULL
      OR LOWER(dbo.[Script].[Name]) IN (
        SELECT LOWER(value)
        FROM OPENJSON(@scriptNames)
        )
      )
  -- derived order by sort order
  ORDER BY [Level] DESC,
    [Priority] DESC
END
GO

/****** Object:  StoredProcedure [dbo].[GetDerivedWageTypes]    Script Date: 01.03.2026 22:35:19 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Get derived wage types (only active).
-- =============================================
CREATE PROCEDURE [dbo].[GetDerivedWageTypes]
  -- the tenant
  @tenantId AS INT,
  -- the payroll,
  @payrollId AS INT,
  -- the regulation valid from date
  @regulationDate AS DATETIME2(7),
  -- creation date
  @createdBefore AS DATETIME2(7),
  -- the wage type numbers: JSON array of DECIMAL(28, 6)
  @wageTypeNumbers AS VARCHAR(MAX) = NULL,
  -- the include clusters: JSON array of cluster names VARCHAR(128)
  @includeClusters AS VARCHAR(MAX) = NULL,
  -- the exclude clusters: JSON array of cluster names VARCHAR(128)
  @excludeClusters AS VARCHAR(MAX) = NULL
AS
BEGIN
  -- SET NOCOUNT ON added to prevent extra result sets from
  -- interfering with SELECT statements
  SET NOCOUNT ON;

  -- select using the wage type number and payroll level
  SELECT [Regulations].[Id] AS [RegulationId],
    [Regulations].[Level] AS [Level],
    [Regulations].[Priority] AS [Priority],
    -- perfomance hint: don't use [WageType].*
    dbo.[WageType].[Id],
    dbo.[WageType].[Status],
    dbo.[WageType].[Created],
    dbo.[WageType].[Updated],
    dbo.[WageType].[RegulationId],
    dbo.[WageType].[Name],
    dbo.[WageType].[NameLocalizations],
    dbo.[WageType].[WageTypeNumber],
    dbo.[WageType].[Description],
    dbo.[WageType].[DescriptionLocalizations],
    dbo.[WageType].[OverrideType],
    dbo.[WageType].[Calendar],
    dbo.[WageType].[Culture],
    dbo.[WageType].[Collectors],
    dbo.[WageType].[CollectorGroups],
    dbo.[WageType].[ValueExpression],
    dbo.[WageType].[ResultExpression],
    dbo.[WageType].[ValueActions],
    dbo.[WageType].[ResultActions],
    --  dbo.[WageType].[Binary],
    dbo.[WageType].[ScriptHash],
    dbo.[WageType].[Attributes],
    dbo.[WageType].[Clusters]
  -- excluded columns
  -- dbo.[WageType].[Script],
  -- dbo.[WageType].[ScriptVersion],
  FROM dbo.[WageType]
  INNER JOIN dbo.GetDerivedRegulations(@tenantId, @payrollId, @regulationDate, @createdBefore) AS [Regulations]
    ON dbo.[WageType].[RegulationId] = [Regulations].[Id]
  -- active wage types only
  WHERE dbo.[WageType].[Status] = 0
    AND dbo.[WageType].[Created] <= @createdBefore
    AND (
      (
        @includeClusters IS NULL
        AND @excludeClusters IS NULL
        )
      OR dbo.IsMatchingCluster(@includeClusters, @excludeClusters, dbo.[WageType].[Clusters]) = 1
      )
    AND (
      @wageTypeNumbers IS NULL
      OR dbo.[WageType].[WageTypeNumber] IN (
        SELECT CAST(value AS DECIMAL(28, 6))
        FROM OPENJSON(@wageTypeNumbers)
        )
      )
  -- derived order by wage type number
  ORDER BY dbo.[WageType].[WageTypeNumber],
    [Level] DESC,
    [Priority] DESC
END
GO

/****** Object:  StoredProcedure [dbo].[DeleteTenant]    Script Date: 01.03.2026 22:35:19 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Detete tenant including all his related objects
--
CREATE PROCEDURE [dbo].[DeleteTenant]
  -- the tenant to delete
  @tenantId AS INT
AS
BEGIN
  -- SET NOCOUNT ON added to prevent extra result sets from
  -- interfering with SELECT statements
  SET NOCOUNT ON;

  -- payroll results
  DELETE [dbo].[PayrunResult]
  FROM [dbo].[PayrunResult]
  INNER JOIN [dbo].[PayrollResult]
    ON [dbo].[PayrunResult].[PayrollResultId] = [dbo].[PayrollResult].[Id]
  WHERE [dbo].[PayrollResult].[TenantId] = @tenantId

  DELETE [dbo].[WageTypeCustomResult]
  FROM [dbo].[WageTypeCustomResult]
  INNER JOIN [dbo].[WageTypeResult]
    ON [dbo].[WageTypeCustomResult].[WageTypeResultId] = [dbo].[WageTypeResult].[Id]
  INNER JOIN [dbo].[PayrollResult]
    ON [dbo].[WageTypeResult].[PayrollResultId] = [dbo].[PayrollResult].[Id]
  WHERE [dbo].[PayrollResult].[TenantId] = @tenantId

  DELETE [dbo].[WageTypeResult]
  FROM [dbo].[WageTypeResult]
  INNER JOIN [dbo].[PayrollResult]
    ON [dbo].[WageTypeResult].[PayrollResultId] = [dbo].[PayrollResult].[Id]
  WHERE [dbo].[PayrollResult].[TenantId] = @tenantId

  DELETE [dbo].[CollectorCustomResult]
  FROM [dbo].[CollectorCustomResult]
  INNER JOIN [dbo].[CollectorResult]
    ON [dbo].[CollectorCustomResult].[CollectorResultId] = [dbo].[CollectorResult].[Id]
  INNER JOIN [dbo].[PayrollResult]
    ON [dbo].[CollectorResult].[PayrollResultId] = [dbo].[PayrollResult].[Id]
  WHERE [dbo].[PayrollResult].[TenantId] = @tenantId

  DELETE [dbo].[CollectorResult]
  FROM [dbo].[CollectorResult]
  INNER JOIN [dbo].[PayrollResult]
    ON [dbo].[CollectorResult].[PayrollResultId] = [dbo].[PayrollResult].[Id]
  WHERE [dbo].[PayrollResult].[TenantId] = @tenantId

  DELETE
  FROM [dbo].[PayrollResult]
  WHERE [TenantId] = @tenantId

  -- payrun with jobs
  DELETE [dbo].[PayrunJobEmployee]
  FROM [dbo].[PayrunJobEmployee]
  INNER JOIN [dbo].[PayrunJob]
    ON [dbo].[PayrunJobEmployee].[PayrunJobId] = [dbo].[PayrunJob].[Id]
  WHERE [dbo].[PayrunJob].[TenantId] = @tenantId

  DELETE
  FROM [dbo].[PayrunJob]
  WHERE [TenantId] = @tenantId

  DELETE [dbo].[PayrunParameter]
  FROM [dbo].[PayrunParameter]
  INNER JOIN [dbo].[Payrun]
    ON [dbo].[PayrunParameter].[PayrunId] = [dbo].[Payrun].[Id]
  WHERE [dbo].[Payrun].[TenantId] = @tenantId

  DELETE
  FROM [dbo].[Payrun]
  WHERE [TenantId] = @tenantId

  -- payroll with payroll layers
  DELETE [dbo].[PayrollLayer]
  FROM [dbo].[PayrollLayer]
  INNER JOIN [dbo].[Payroll]
    ON [dbo].[PayrollLayer].[PayrollId] = [dbo].[Payroll].[Id]
  WHERE [dbo].[Payroll].[TenantId] = @tenantId

  DELETE
  FROM [dbo].[Payroll]
  WHERE [TenantId] = @tenantId

  -- regulation shares
  DELETE [dbo].[RegulationShare]
  FROM [dbo].[RegulationShare]
  WHERE [dbo].[RegulationShare].[ProviderTenantId] = @tenantId
    OR [dbo].[RegulationShare].[ConsumerTenantId] = @tenantId

  -- regulation
  DELETE [dbo].[ReportTemplateAudit]
  FROM [dbo].[ReportTemplateAudit]
  INNER JOIN [dbo].[ReportTemplate]
    ON [dbo].[ReportTemplateAudit].[ReportTemplateId] = [dbo].[ReportTemplate].[Id]
  INNER JOIN [dbo].[Report]
    ON [dbo].[ReportTemplate].[ReportId] = [dbo].[Report].[Id]
  INNER JOIN [dbo].[Regulation]
    ON [dbo].[Report].[RegulationId] = [dbo].[Regulation].[Id]
  WHERE [dbo].[Regulation].[TenantId] = @tenantId

  DELETE [dbo].[ReportTemplate]
  FROM [dbo].[ReportTemplate]
  INNER JOIN [dbo].[Report]
    ON [dbo].[ReportTemplate].[ReportId] = [dbo].[Report].[Id]
  INNER JOIN [dbo].[Regulation]
    ON [dbo].[Report].[RegulationId] = [dbo].[Regulation].[Id]
  WHERE [dbo].[Regulation].[TenantId] = @tenantId

  DELETE [dbo].[ReportParameterAudit]
  FROM [dbo].[ReportParameterAudit]
  INNER JOIN [dbo].[ReportParameter]
    ON [dbo].[ReportParameterAudit].[ReportParameterId] = [dbo].[ReportParameter].[Id]
  INNER JOIN [dbo].[Report]
    ON [dbo].[ReportParameter].[ReportId] = [dbo].[Report].[Id]
  INNER JOIN [dbo].[Regulation]
    ON [dbo].[Report].[RegulationId] = [dbo].[Regulation].[Id]
  WHERE [dbo].[Regulation].[TenantId] = @tenantId

  DELETE [dbo].[ReportParameter]
  FROM [dbo].[ReportParameter]
  INNER JOIN [dbo].[Report]
    ON [dbo].[ReportParameter].[ReportId] = [dbo].[Report].[Id]
  INNER JOIN [dbo].[Regulation]
    ON [dbo].[Report].[RegulationId] = [dbo].[Regulation].[Id]
  WHERE [dbo].[Regulation].[TenantId] = @tenantId

  DELETE [dbo].[ReportAudit]
  FROM [dbo].[ReportAudit]
  INNER JOIN [dbo].[Report]
    ON [dbo].[ReportAudit].[ReportId] = [dbo].[Report].[Id]
  INNER JOIN [dbo].[Regulation]
    ON [dbo].[Report].[RegulationId] = [dbo].[Regulation].[Id]
  WHERE [dbo].[Regulation].[TenantId] = @tenantId

  DELETE [dbo].[Report]
  FROM [dbo].[Report]
  INNER JOIN [dbo].[Regulation]
    ON [dbo].[Report].[RegulationId] = [dbo].[Regulation].[Id]
  WHERE [dbo].[Regulation].[TenantId] = @tenantId

  DELETE [dbo].[ScriptAudit]
  FROM [dbo].[ScriptAudit]
  INNER JOIN [dbo].[Script]
    ON [dbo].[ScriptAudit].[ScriptId] = [dbo].[Script].[Id]
  INNER JOIN [dbo].[Regulation]
    ON [dbo].[Script].[RegulationId] = [dbo].[Regulation].[Id]
  WHERE [dbo].[Regulation].[TenantId] = @tenantId

  DELETE [dbo].[Script]
  FROM [dbo].[Script]
  INNER JOIN [dbo].[Regulation]
    ON [dbo].[Script].[RegulationId] = [dbo].[Regulation].[Id]
  WHERE [dbo].[Regulation].[TenantId] = @tenantId

  DELETE [dbo].[LookupValueAudit]
  FROM [dbo].[LookupValueAudit]
  INNER JOIN [dbo].[LookupValue]
    ON [dbo].[LookupValueAudit].[LookupValueId] = [dbo].[LookupValue].[Id]
  INNER JOIN [dbo].[Lookup]
    ON [dbo].[LookupValue].[LookupId] = [dbo].[Lookup].[Id]
  INNER JOIN [dbo].[Regulation]
    ON [dbo].[Lookup].[RegulationId] = [dbo].[Regulation].[Id]
  WHERE [dbo].[Regulation].[TenantId] = @tenantId

  DELETE [dbo].[LookupValue]
  FROM [dbo].[LookupValue]
  INNER JOIN [dbo].[Lookup]
    ON [dbo].[LookupValue].[LookupId] = [dbo].[Lookup].[Id]
  INNER JOIN [dbo].[Regulation]
    ON [dbo].[Lookup].[RegulationId] = [dbo].[Regulation].[Id]
  WHERE [dbo].[Regulation].[TenantId] = @tenantId

  DELETE [dbo].[LookupAudit]
  FROM [dbo].[LookupAudit]
  INNER JOIN [dbo].[Lookup]
    ON [dbo].[LookupAudit].[LookupId] = [dbo].[Lookup].[Id]
  INNER JOIN [dbo].[Regulation]
    ON [dbo].[Lookup].[RegulationId] = [dbo].[Regulation].[Id]
  WHERE [dbo].[Regulation].[TenantId] = @tenantId

  DELETE [dbo].[Lookup]
  FROM [dbo].[Lookup]
  INNER JOIN [dbo].[Regulation]
    ON [dbo].[Lookup].[RegulationId] = [dbo].[Regulation].[Id]
  WHERE [dbo].[Regulation].[TenantId] = @tenantId

  DELETE [dbo].[CollectorAudit]
  FROM [dbo].[CollectorAudit]
  INNER JOIN [dbo].[Collector]
    ON [dbo].[CollectorAudit].[CollectorId] = [dbo].[Collector].[Id]
  INNER JOIN [dbo].[Regulation]
    ON [dbo].[Collector].[RegulationId] = [dbo].[Regulation].[Id]
  WHERE [dbo].[Regulation].[TenantId] = @tenantId

  DELETE [dbo].[Collector]
  FROM [dbo].[Collector]
  INNER JOIN [dbo].[Regulation]
    ON [dbo].[Collector].[RegulationId] = [dbo].[Regulation].[Id]
  WHERE [dbo].[Regulation].[TenantId] = @tenantId

  DELETE [dbo].[WageTypeAudit]
  FROM [dbo].[WageTypeAudit]
  INNER JOIN [dbo].[WageType]
    ON [dbo].[WageTypeAudit].[WageTypeId] = [dbo].[WageType].[Id]
  INNER JOIN [dbo].[Regulation]
    ON [dbo].[WageType].[RegulationId] = [dbo].[Regulation].[Id]
  WHERE [dbo].[Regulation].[TenantId] = @tenantId

  DELETE [dbo].[WageType]
  FROM [dbo].[WageType]
  INNER JOIN [dbo].[Regulation]
    ON [dbo].[WageType].[RegulationId] = [dbo].[Regulation].[Id]
  WHERE [dbo].[Regulation].[TenantId] = @tenantId

  DELETE [dbo].[CaseRelationAudit]
  FROM [dbo].[CaseRelationAudit]
  INNER JOIN [dbo].[CaseRelation]
    ON [dbo].[CaseRelationAudit].[CaseRelationId] = [dbo].[CaseRelation].[Id]
  INNER JOIN [dbo].[Regulation]
    ON [dbo].[CaseRelation].[RegulationId] = [dbo].[Regulation].[Id]
  WHERE [dbo].[Regulation].[TenantId] = @tenantId

  DELETE [dbo].[CaseRelation]
  FROM [dbo].[CaseRelation]
  INNER JOIN [dbo].[Regulation]
    ON [dbo].[CaseRelation].[RegulationId] = [dbo].[Regulation].[Id]
  WHERE [dbo].[Regulation].[TenantId] = @tenantId

  DELETE [dbo].[CaseFieldAudit]
  FROM [dbo].[CaseFieldAudit]
  INNER JOIN [dbo].[CaseField]
    ON [dbo].[CaseFieldAudit].[CaseFieldId] = [dbo].[CaseField].[Id]
  INNER JOIN [dbo].[Case]
    ON [dbo].[CaseField].[CaseId] = [dbo].[Case].[Id]
  INNER JOIN [dbo].[Regulation]
    ON [dbo].[Case].[RegulationId] = [dbo].[Regulation].[Id]
  WHERE [dbo].[Regulation].[TenantId] = @tenantId

  DELETE [dbo].[CaseField]
  FROM [dbo].[CaseField]
  INNER JOIN [dbo].[Case]
    ON [dbo].[CaseField].[CaseId] = [dbo].[Case].[Id]
  INNER JOIN [dbo].[Regulation]
    ON [dbo].[Case].[RegulationId] = [dbo].[Regulation].[Id]
  WHERE [dbo].[Regulation].[TenantId] = @tenantId

  DELETE [dbo].[CaseAudit]
  FROM [dbo].[CaseAudit]
  INNER JOIN [dbo].[Case]
    ON [dbo].[CaseAudit].[CaseId] = [dbo].[Case].[Id]
  INNER JOIN [dbo].[Regulation]
    ON [dbo].[Case].[RegulationId] = [dbo].[Regulation].[Id]
  WHERE [dbo].[Regulation].[TenantId] = @tenantId

  DELETE [dbo].[Case]
  FROM [dbo].[Case]
  INNER JOIN [dbo].[Regulation]
    ON [dbo].[Case].[RegulationId] = [dbo].[Regulation].[Id]
  WHERE [dbo].[Regulation].[TenantId] = @tenantId

  DELETE
  FROM [dbo].[Regulation]
  WHERE [TenantId] = @tenantId

  -- employee
  DELETE [dbo].[EmployeeCaseValueChange]
  FROM [dbo].[EmployeeCaseValueChange]
  INNER JOIN [dbo].[EmployeeCaseChange]
    ON [dbo].[EmployeeCaseValueChange].[CaseChangeId] = [dbo].[EmployeeCaseChange].[Id]
  INNER JOIN [dbo].[Employee]
    ON [dbo].[EmployeeCaseChange].[EmployeeId] = [dbo].[Employee].[Id]
  WHERE [dbo].[Employee].[TenantId] = @tenantId

  DELETE [dbo].[EmployeeCaseChange]
  FROM [dbo].[EmployeeCaseChange]
  INNER JOIN [dbo].[Employee]
    ON [dbo].[EmployeeCaseChange].[EmployeeId] = [dbo].[Employee].[Id]
  WHERE [dbo].[Employee].[TenantId] = @tenantId

  DELETE [dbo].[EmployeeCaseDocument]
  FROM [dbo].[EmployeeCaseDocument]
  INNER JOIN [dbo].[EmployeeCaseValue]
    ON [dbo].[EmployeeCaseDocument].[CaseValueId] = [dbo].[EmployeeCaseValue].[Id]
  INNER JOIN [dbo].[Employee]
    ON [dbo].[EmployeeCaseValue].[EmployeeId] = [dbo].[Employee].[Id]
  WHERE [dbo].[Employee].[TenantId] = @tenantId

  DELETE [dbo].[EmployeeCaseValue]
  FROM [dbo].[EmployeeCaseValue]
  INNER JOIN [dbo].[Employee]
    ON [dbo].[EmployeeCaseValue].[EmployeeId] = [dbo].[Employee].[Id]
  WHERE [dbo].[Employee].[TenantId] = @tenantId

  DELETE [dbo].[EmployeeDivision]
  FROM [dbo].[EmployeeDivision]
  INNER JOIN [dbo].[Employee]
    ON [dbo].[EmployeeDivision].[EmployeeId] = [dbo].[Employee].[Id]
  WHERE [dbo].[Employee].[TenantId] = @tenantId

  DELETE
  FROM [dbo].[Employee]
  WHERE [TenantId] = @tenantId

  -- company
  DELETE [dbo].[CompanyCaseValueChange]
  FROM [dbo].[CompanyCaseValueChange]
  INNER JOIN [dbo].[CompanyCaseChange]
    ON [dbo].[CompanyCaseValueChange].[CaseChangeId] = [dbo].[CompanyCaseChange].[Id]
  WHERE [dbo].[CompanyCaseChange].[TenantId] = @tenantId

  DELETE
  FROM [dbo].[CompanyCaseChange]
  WHERE [TenantId] = @tenantId

  DELETE [dbo].[CompanyCaseDocument]
  FROM [dbo].[CompanyCaseDocument]
  INNER JOIN [dbo].[CompanyCaseValue]
    ON [dbo].[CompanyCaseDocument].[CaseValueId] = [dbo].[CompanyCaseValue].[Id]
  WHERE [dbo].[CompanyCaseValue].[TenantId] = @tenantId

  DELETE
  FROM [dbo].[CompanyCaseValue]
  WHERE [TenantId] = @tenantId

  -- national
  DELETE [dbo].[NationalCaseValueChange]
  FROM [dbo].[NationalCaseValueChange]
  INNER JOIN [dbo].[NationalCaseChange]
    ON [dbo].[NationalCaseValueChange].[CaseChangeId] = [dbo].[NationalCaseChange].[Id]
  WHERE [dbo].[NationalCaseChange].[TenantId] = @tenantId

  DELETE
  FROM [dbo].[NationalCaseChange]
  WHERE [TenantId] = @tenantId

  DELETE [dbo].[NationalCaseDocument]
  FROM [dbo].[NationalCaseDocument]
  INNER JOIN [dbo].[NationalCaseValue]
    ON [dbo].[NationalCaseDocument].[CaseValueId] = [dbo].[NationalCaseValue].[Id]
  WHERE [dbo].[NationalCaseValue].[TenantId] = @tenantId

  DELETE
  FROM [dbo].[NationalCaseValue]
  WHERE [TenantId] = @tenantId

  -- Global
  DELETE [dbo].[GlobalCaseValueChange]
  FROM [dbo].[GlobalCaseValueChange]
  INNER JOIN [dbo].[GlobalCaseChange]
    ON [dbo].[GlobalCaseValueChange].[CaseChangeId] = [dbo].[GlobalCaseChange].[Id]
  WHERE [dbo].[GlobalCaseChange].[TenantId] = @tenantId

  DELETE
  FROM [dbo].[GlobalCaseChange]
  WHERE [TenantId] = @tenantId

  DELETE [dbo].[GlobalCaseDocument]
  FROM [dbo].[GlobalCaseDocument]
  INNER JOIN [dbo].[GlobalCaseValue]
    ON [dbo].[GlobalCaseDocument].[CaseValueId] = [dbo].[GlobalCaseValue].[Id]
  WHERE [dbo].[GlobalCaseValue].[TenantId] = @tenantId

  DELETE
  FROM [dbo].[GlobalCaseValue]
  WHERE [TenantId] = @tenantId

  -- webhook
  DELETE [dbo].[WebhookMessage]
  FROM [dbo].[WebhookMessage]
  INNER JOIN [dbo].[Webhook]
    ON [dbo].[WebhookMessage].[WebhookId] = [dbo].[Webhook].[Id]
  WHERE [dbo].[Webhook].[TenantId] = @tenantId

  DELETE
  FROM [dbo].[Webhook]
  WHERE [TenantId] = @tenantId

  -- task
  DELETE
  FROM [dbo].[Task]
  WHERE [TenantId] = @tenantId

  -- log
  DELETE
  FROM [dbo].[Log]
  WHERE [TenantId] = @tenantId

  -- report log
  DELETE [dbo].[ReportLog]
  WHERE [dbo].[ReportLog].[TenantId] = @tenantId

  -- user
  DELETE
  FROM [dbo].[User]
  WHERE [TenantId] = @tenantId

  -- division
  DELETE
  FROM [dbo].[Division]
  WHERE [TenantId] = @tenantId

  -- calendar
  DELETE
  FROM [dbo].[Calendar]
  WHERE [TenantId] = @tenantId

  -- tenant
  DELETE
  FROM [dbo].[Tenant]
  WHERE [Id] = @tenantId
END
GO


DECLARE @errorID int
INSERT INTO dbo.[Version] (
    MajorVersion,
    MinorVersion,
    SubVersion,
    [Owner],
    [Description] )
VALUES (
    0,
    9,
    6,
    CURRENT_USER,
    'Payroll Engine: Migration v0.9.5 -> v0.9.6' )
SET @errorID = @@ERROR
IF ( @errorID <> 0 ) BEGIN
    PRINT 'Error while updating the Payroll Engine database version.'
END
ELSE BEGIN
    PRINT 'Payroll Engine database version successfully updated to release 0.9.6'
END
GO

COMMIT TRANSACTION
GO

SET NOEXEC OFF   -- re-enable execution (in case VERSION_CHECK set it ON)
GO