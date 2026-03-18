-- =============================================================================
-- GetCollectorCustomResults
-- =============================================================================

USE PayrollEngine;

DELIMITER $$

DROP PROCEDURE IF EXISTS GetCollectorCustomResults$$
CREATE PROCEDURE GetCollectorCustomResults(
    IN p_tenantId            INT,
    IN p_employeeId          INT,
    IN p_divisionId          INT,
    IN p_payrunJobId         INT,
    IN p_parentPayrunJobId   INT,
    IN p_collectorNameHashes VARCHAR(4000),
    IN p_periodStart         DATETIME(6),
    IN p_periodEnd           DATETIME(6),
    IN p_jobStatus           INT,
    IN p_forecast            VARCHAR(128),
    IN p_evaluationDate      DATETIME(6)
)
BEGIN
    DECLARE v_collectorNameHash INT;
    DECLARE v_collectorCount    INT;

    SET v_collectorCount = IF(p_collectorNameHashes IS NULL, 0, JSON_LENGTH(p_collectorNameHashes));

    IF v_collectorCount = 1 THEN
        SELECT CAST(jt.val AS SIGNED) INTO v_collectorNameHash
        FROM JSON_TABLE(p_collectorNameHashes, '$[*]' COLUMNS (val VARCHAR(20) PATH '$')) AS jt
        LIMIT 1;
    END IF;

    SELECT ccr.*
    FROM CollectorCustomResult ccr
    WHERE ccr.TenantId = p_tenantId
      AND ccr.EmployeeId = p_employeeId
      AND (p_divisionId IS NULL        OR ccr.DivisionId = p_divisionId)
      AND (p_payrunJobId IS NULL       OR ccr.PayrunJobId = p_payrunJobId)
      AND (p_parentPayrunJobId IS NULL OR ccr.ParentJobId = p_parentPayrunJobId)
      AND (p_collectorNameHashes IS NULL OR v_collectorCount = 0
           OR (v_collectorCount = 1 AND ccr.CollectorNameHash = v_collectorNameHash)
           OR (v_collectorCount > 1 AND ccr.CollectorNameHash IN (
               SELECT CAST(jt.val AS SIGNED)
               FROM JSON_TABLE(p_collectorNameHashes, '$[*]' COLUMNS (val VARCHAR(20) PATH '$')) AS jt)))
      AND (p_periodStart IS NULL OR ccr.Start BETWEEN p_periodStart AND p_periodEnd)
      AND (p_jobStatus IS NULL OR ccr.PayrunJobId IN (
               SELECT pj.Id FROM PayrunJob pj
               WHERE pj.Id = ccr.PayrunJobId
                 AND (pj.JobStatus & p_jobStatus) = pj.JobStatus))
      AND (ccr.Forecast IS NULL OR ccr.Forecast = p_forecast)
      AND (p_evaluationDate IS NULL OR ccr.Created <= p_evaluationDate)
    ORDER BY ccr.Created;
END$$

DELIMITER ;
