-- =============================================================================
-- GetConsolidatedWageTypeResults
-- ;WITH Winners AS -> WITH Winners AS (MySQL 8.0+ supports CTEs in SPs)
-- OPTION (RECOMPILE) -> removed
-- =============================================================================

USE PayrollEngine;

DELIMITER $$

DROP PROCEDURE IF EXISTS GetConsolidatedWageTypeResults$$
CREATE PROCEDURE GetConsolidatedWageTypeResults(
    IN p_tenantId           INT,
    IN p_employeeId         INT,
    IN p_divisionId         INT,
    IN p_wageTypeNumbers    VARCHAR(4000),
    IN p_periodStartHashes  VARCHAR(4000),
    IN p_jobStatus          INT,
    IN p_forecast           VARCHAR(128),
    IN p_evaluationDate     DATETIME(6),
    IN p_noRetro            TINYINT(1),
    IN p_excludeParentJobId INT
)
BEGIN
    DECLARE v_wageTypeNumber  DECIMAL(28,6);
    DECLARE v_wageTypeCount   INT;
    DECLARE v_startHash       INT;
    DECLARE v_startHashCount  INT;

    SET v_wageTypeCount  = IF(p_wageTypeNumbers IS NULL,   0, JSON_LENGTH(p_wageTypeNumbers));
    SET v_startHashCount = IF(p_periodStartHashes IS NULL, 0, JSON_LENGTH(p_periodStartHashes));

    IF v_wageTypeCount = 1 THEN
        SELECT CAST(jt.val AS DECIMAL(28,6)) INTO v_wageTypeNumber
        FROM JSON_TABLE(p_wageTypeNumbers, '$[*]' COLUMNS (val VARCHAR(50) PATH '$')) AS jt LIMIT 1;
    END IF;

    IF v_startHashCount = 1 THEN
        SELECT CAST(jt.val AS SIGNED) INTO v_startHash
        FROM JSON_TABLE(p_periodStartHashes, '$[*]' COLUMNS (val VARCHAR(20) PATH '$')) AS jt LIMIT 1;
    END IF;

    WITH Winners AS (
        SELECT wtr.Id,
            ROW_NUMBER() OVER (
                PARTITION BY wtr.WageTypeNumber, wtr.Start
                ORDER BY wtr.Created DESC, wtr.Id DESC
            ) AS RowNumber
        FROM WageTypeResult wtr
        WHERE wtr.TenantId = p_tenantId
          AND wtr.EmployeeId = p_employeeId
          AND (v_startHashCount = 0 OR
               (v_startHashCount = 1 AND wtr.StartHash = v_startHash) OR
               (v_startHashCount > 1 AND wtr.StartHash IN (
                   SELECT CAST(jt.val AS SIGNED)
                   FROM JSON_TABLE(p_periodStartHashes, '$[*]' COLUMNS (val VARCHAR(20) PATH '$')) AS jt)))
          AND (p_divisionId IS NULL OR wtr.DivisionId = p_divisionId)
          AND (p_wageTypeNumbers IS NULL OR v_wageTypeCount = 0
               OR (v_wageTypeCount = 1 AND wtr.WageTypeNumber = v_wageTypeNumber)
               OR (v_wageTypeCount > 1 AND wtr.WageTypeNumber IN (
                   SELECT CAST(jt.val AS DECIMAL(28,6))
                   FROM JSON_TABLE(p_wageTypeNumbers, '$[*]' COLUMNS (val VARCHAR(50) PATH '$')) AS jt)))
          AND (p_evaluationDate IS NULL OR wtr.Created <= p_evaluationDate)
          AND (p_jobStatus IS NULL OR wtr.PayrunJobId IN (
                   SELECT pj.Id FROM PayrunJob pj WHERE (pj.JobStatus & p_jobStatus) = pj.JobStatus))
          AND (wtr.Forecast IS NULL OR wtr.Forecast = p_forecast)
          AND (p_noRetro = 0 OR wtr.ParentJobId IS NULL)
          AND (p_excludeParentJobId IS NULL OR wtr.ParentJobId IS NULL
               OR wtr.ParentJobId <> p_excludeParentJobId)
    )
    SELECT wtr.*
    FROM WageTypeResult wtr
    INNER JOIN Winners w ON w.Id = wtr.Id
    WHERE w.RowNumber = 1;
END$$

DELIMITER ;
