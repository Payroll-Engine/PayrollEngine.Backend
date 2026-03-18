-- =============================================================================
-- GetWageTypeCustomResults
-- =============================================================================

USE PayrollEngine;

DELIMITER $$

DROP PROCEDURE IF EXISTS GetWageTypeCustomResults$$
CREATE PROCEDURE GetWageTypeCustomResults(
    IN p_tenantId          INT,
    IN p_employeeId        INT,
    IN p_divisionId        INT,
    IN p_payrunJobId       INT,
    IN p_parentPayrunJobId INT,
    IN p_wageTypeNumbers   VARCHAR(4000),
    IN p_periodStart       DATETIME(6),
    IN p_periodEnd         DATETIME(6),
    IN p_jobStatus         INT,
    IN p_forecast          VARCHAR(128),
    IN p_evaluationDate    DATETIME(6)
)
BEGIN
    DECLARE v_wageTypeNumber DECIMAL(28,6);
    DECLARE v_wageTypeCount  INT;

    SET v_wageTypeCount = IF(p_wageTypeNumbers IS NULL, 0, JSON_LENGTH(p_wageTypeNumbers));

    IF v_wageTypeCount = 1 THEN
        SELECT CAST(jt.val AS DECIMAL(28,6)) INTO v_wageTypeNumber
        FROM JSON_TABLE(p_wageTypeNumbers, '$[*]' COLUMNS (val VARCHAR(50) PATH '$')) AS jt
        LIMIT 1;
    END IF;

    SELECT wtcr.*
    FROM WageTypeCustomResult wtcr
    WHERE wtcr.TenantId = p_tenantId
      AND wtcr.EmployeeId = p_employeeId
      AND (p_divisionId IS NULL        OR wtcr.DivisionId = p_divisionId)
      AND (p_payrunJobId IS NULL       OR wtcr.PayrunJobId = p_payrunJobId)
      AND (p_parentPayrunJobId IS NULL OR wtcr.ParentJobId = p_parentPayrunJobId)
      AND (p_wageTypeNumbers IS NULL OR v_wageTypeCount = 0
           OR (v_wageTypeCount = 1 AND wtcr.WageTypeNumber = v_wageTypeNumber)
           OR (v_wageTypeCount > 1 AND wtcr.WageTypeNumber IN (
               SELECT CAST(jt.val AS DECIMAL(28,6))
               FROM JSON_TABLE(p_wageTypeNumbers, '$[*]' COLUMNS (val VARCHAR(50) PATH '$')) AS jt)))
      AND (p_periodStart IS NULL OR wtcr.Start BETWEEN p_periodStart AND p_periodEnd)
      AND (p_jobStatus IS NULL OR wtcr.PayrunJobId IN (
               SELECT pj.Id FROM PayrunJob pj
               WHERE pj.Id = wtcr.PayrunJobId
                 AND (pj.JobStatus & p_jobStatus) = pj.JobStatus))
      AND (wtcr.Forecast IS NULL OR wtcr.Forecast = p_forecast)
      AND (p_evaluationDate IS NULL OR wtcr.Created <= p_evaluationDate)
    ORDER BY wtcr.Created;
END$$

DELIMITER ;
