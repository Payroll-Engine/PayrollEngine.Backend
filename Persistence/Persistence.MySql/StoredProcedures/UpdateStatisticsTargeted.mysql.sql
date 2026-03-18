-- =============================================================================
-- UpdateStatisticsTargeted
-- T-SQL: UPDATE STATISTICS ... WITH FULLSCAN -> MySQL: ANALYZE TABLE
-- =============================================================================

USE PayrollEngine;

DELIMITER $$

DROP PROCEDURE IF EXISTS UpdateStatisticsTargeted$$
CREATE PROCEDURE UpdateStatisticsTargeted()
BEGIN
    ANALYZE TABLE LookupValue;
    ANALYZE TABLE PayrollResult;
    ANALYZE TABLE WageTypeResult;
    ANALYZE TABLE WageTypeCustomResult;
    ANALYZE TABLE CollectorResult;
    ANALYZE TABLE CollectorCustomResult;
    ANALYZE TABLE PayrunResult;
    ANALYZE TABLE GlobalCaseValue;
    ANALYZE TABLE NationalCaseValue;
    ANALYZE TABLE CompanyCaseValue;
    ANALYZE TABLE EmployeeCaseValue;
END$$

DELIMITER ;
