-- =============================================
-- Targeted statistics update for high-volume tables
-- see https://www.sqlskills.com/blogs/kimberly/the-tipping-point-query-answers/
-- see https://www.brentozar.com/archive/2019/10/how-to-think-like-the-sql-server-engine-whats-the-tipping-point/

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

IF EXISTS (
    SELECT *
    FROM sysobjects
    WHERE id = object_id(N'[dbo].[UpdateStatisticsTargeted]')
      AND OBJECTPROPERTY(id, N'IsProcedure') = 1
    )
BEGIN
  DROP PROCEDURE dbo.[UpdateStatisticsTargeted]
END
GO

-- =============================================
-- Update statistics for high-volume tables only (tipping point prevention)
-- Covers tables that receive large bulk imports or accumulate many rows:
--   - LookupValue         (bulk-imported during regulation setup)
--   - Payrun results      (WageTypeResult, WageTypeCustomResult, CollectorResult,
--                          CollectorCustomResult, PayrunResult, PayrollResult)
--   - Case values         (GlobalCaseValue, NationalCaseValue, CompanyCaseValue,
--                          EmployeeCaseValue)
-- see https://www.sqlskills.com/blogs/kimberly/the-tipping-point-query-answers/
-- see https://www.brentozar.com/archive/2019/10/how-to-think-like-the-sql-server-engine-whats-the-tipping-point/
-- =============================================
CREATE PROCEDURE dbo.[UpdateStatisticsTargeted]
AS
BEGIN

UPDATE STATISTICS dbo.[LookupValue]          WITH FULLSCAN;

UPDATE STATISTICS dbo.[PayrollResult]        WITH FULLSCAN;
UPDATE STATISTICS dbo.[WageTypeResult]       WITH FULLSCAN;
UPDATE STATISTICS dbo.[WageTypeCustomResult] WITH FULLSCAN;
UPDATE STATISTICS dbo.[CollectorResult]      WITH FULLSCAN;
UPDATE STATISTICS dbo.[CollectorCustomResult] WITH FULLSCAN;
UPDATE STATISTICS dbo.[PayrunResult]         WITH FULLSCAN;

UPDATE STATISTICS dbo.[GlobalCaseValue]      WITH FULLSCAN;
UPDATE STATISTICS dbo.[NationalCaseValue]    WITH FULLSCAN;
UPDATE STATISTICS dbo.[CompanyCaseValue]     WITH FULLSCAN;
UPDATE STATISTICS dbo.[EmployeeCaseValue]    WITH FULLSCAN;

END
GO
