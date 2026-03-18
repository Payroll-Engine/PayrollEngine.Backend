-- =============================================================================
-- GetCollectorResults
-- =============================================================================

USE PayrollEngine;

DELIMITER $$

DROP PROCEDURE IF EXISTS GetCollectorResults$$
CREATE PROCEDURE GetCollectorResults(
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

    SELECT cr.*
    FROM CollectorResult cr
    WHERE cr.TenantId = p_tenantId
      AND cr.EmployeeId = p_employeeId
      AND (p_divisionId IS NULL        OR cr.DivisionId = p_divisionId)
      AND (p_payrunJobId IS NULL       OR cr.PayrunJobId = p_payrunJobId)
      AND (p_parentPayrunJobId IS NULL OR cr.ParentJobId = p_parentPayrunJobId)
      AND (p_collectorNameHashes IS NULL OR v_collectorCount = 0
           OR (v_collectorCount = 1 AND cr.CollectorNameHash = v_collectorNameHash)
           OR (v_collectorCount > 1 AND cr.CollectorNameHash IN (
               SELECT CAST(jt.val AS SIGNED)
               FROM JSON_TABLE(p_collectorNameHashes, '$[*]' COLUMNS (val VARCHAR(20) PATH '$')) AS jt)))
      AND (p_periodStart IS NULL OR cr.Start BETWEEN p_periodStart AND p_periodEnd)
      AND (p_jobStatus IS NULL OR cr.PayrunJobId IN (
               SELECT pj.Id FROM PayrunJob pj
               WHERE pj.Id = cr.PayrunJobId
                 AND (pj.JobStatus & p_jobStatus) = pj.JobStatus))
      AND (cr.Forecast IS NULL OR cr.Forecast = p_forecast)
      AND (p_evaluationDate IS NULL OR cr.Created <= p_evaluationDate)
    ORDER BY cr.Created;
END$$

DELIMITER ;
