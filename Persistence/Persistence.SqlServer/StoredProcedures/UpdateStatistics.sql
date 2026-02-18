-- =============================================
-- Cleanup database tipping point
-- see https://www.sqlskills.com/blogs/kimberly/the-tipping-point-query-answers/
-- see https://www.brentozar.com/archive/2019/10/how-to-think-like-the-sql-server-engine-whats-the-tipping-point/
-- see 

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

IF EXISTS (
    SELECT *
    FROM sysobjects
    WHERE id = object_id(N'[dbo].[UpdateStatistics]')
      AND OBJECTPROPERTY(id, N'IsProcedure') = 1
    )
BEGIN
  DROP PROCEDURE dbo.[UpdateStatistics]
END
GO

-- =============================================
-- Update database statistics (tipping point)
-- see https://www.sqlskills.com/blogs/kimberly/the-tipping-point-query-answers/
-- see https://www.brentozar.com/archive/2019/10/how-to-think-like-the-sql-server-engine-whats-the-tipping-point/
-- =============================================
CREATE PROCEDURE dbo.[UpdateStatistics]
AS
BEGIN

DECLARE @sql nvarchar(MAX) = N'';
SELECT @sql += N'UPDATE STATISTICS ' + QUOTENAME(s.name) + N'.' + QUOTENAME(t.name) + N' WITH FULLSCAN; '
FROM sys.tables t
	JOIN sys.schemas s ON t.schema_id = s.schema_id;

EXEC sp_executesql @sql;

END
GO
