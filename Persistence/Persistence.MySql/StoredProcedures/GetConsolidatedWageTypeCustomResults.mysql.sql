-- =============================================================================
-- GetConsolidatedWageTypeCustomResults
-- =============================================================================

USE PayrollEngine;

DELIMITER $$

DROP PROCEDURE IF EXISTS GetConsolidatedWageTypeCustomResults$$
CREATE PROCEDURE GetConsolidatedWageTypeCustomResults(
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
        SELECT r.Id,
            ROW_NUMBER() OVER (
                PARTITION BY r.WageTypeNumber, r.Start
                ORDER BY r.Created DESC, r.Id DESC
            ) AS RowNumber
        FROM WageTypeCustomResult r
        WHERE r.TenantId = p_tenantId
          AND r.EmployeeId = p_employeeId
          AND (v_startHashCount = 0 OR
               (v_startHashCount = 1 AND r.StartHash = v_startHash) OR
               (v_startHashCount > 1 AND r.StartHash IN (
                   SELECT CAST(jt.val AS SIGNED)
                   FROM JSON_TABLE(p_periodStartHashes, '$[*]' COLUMNS (val VARCHAR(20) PATH '$')) AS jt)))
          AND (p_divisionId IS NULL OR r.DivisionId = p_divisionId)
          AND (p_wageTypeNumbers IS NULL OR v_wageTypeCount = 0
               OR (v_wageTypeCount = 1 AND r.WageTypeNumber = v_wageTypeNumber)
               OR (v_wageTypeCount > 1 AND r.WageTypeNumber IN (
                   SELECT CAST(jt.val AS DECIMAL(28,6))
                   FROM JSON_TABLE(p_wageTypeNumbers, '$[*]' COLUMNS (val VARCHAR(50) PATH '$')) AS jt)))
          AND (p_evaluationDate IS NULL OR r.Created <= p_evaluationDate)
          AND (p_jobStatus IS NULL OR r.PayrunJobId IN (
                   SELECT pj.Id FROM PayrunJob pj WHERE (pj.JobStatus & p_jobStatus) = pj.JobStatus))
          AND (r.Forecast IS NULL OR r.Forecast = p_forecast)
          AND (p_noRetro = 0 OR r.ParentJobId IS NULL)
          AND (p_excludeParentJobId IS NULL OR r.ParentJobId IS NULL
               OR r.ParentJobId <> p_excludeParentJobId)
    )
    SELECT r.*
    FROM WageTypeCustomResult r
    INNER JOIN Winners w ON w.Id = r.Id
    WHERE w.RowNumber = 1;
END$$

DELIMITER ;
