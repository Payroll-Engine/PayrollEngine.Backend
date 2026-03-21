-- =============================================================================
-- GetConsolidatedPayrunResults
-- =============================================================================

USE PayrollEngine;

DELIMITER $$

DROP PROCEDURE IF EXISTS GetConsolidatedPayrunResults$$
CREATE PROCEDURE GetConsolidatedPayrunResults(
    IN p_tenantId           INT,
    IN p_employeeId         INT,
    IN p_divisionId         INT,
    IN p_names              VARCHAR(4000),
    IN p_periodStartHashes  VARCHAR(4000),
    IN p_jobStatus          INT,
    IN p_forecast           VARCHAR(128),
    IN p_evaluationDate     DATETIME(6),
    IN p_noRetro            TINYINT(1),
    IN p_excludeParentJobId INT
)
BEGIN
    DECLARE v_name           VARCHAR(128);
    DECLARE v_nameCount      INT;
    DECLARE v_startHash      INT;
    DECLARE v_startHashCount INT;

    SET v_nameCount      = IF(p_names IS NULL,             0, JSON_LENGTH(p_names));
    SET v_startHashCount = IF(p_periodStartHashes IS NULL, 0, JSON_LENGTH(p_periodStartHashes));

    IF v_nameCount = 1 THEN
        SELECT jt.val INTO v_name
        FROM JSON_TABLE(p_names, '$[*]' COLUMNS (val VARCHAR(128) PATH '$')) AS jt LIMIT 1;
    END IF;

    -- single-hash fast path: equality seek on StartHash
    IF v_startHashCount = 1 THEN
        SELECT CAST(jt.val AS SIGNED) INTO v_startHash
        FROM JSON_TABLE(p_periodStartHashes, '$[*]' COLUMNS (val VARCHAR(20) PATH '$')) AS jt LIMIT 1;
    END IF;

    -- Phase 1: select winning IDs via index-only scan
    -- Index key order: (TenantId, EmployeeId, StartHash, Name)
    -- → seeks directly to the period, constant cost regardless of history
    WITH Winners AS (
        SELECT r.Id,
            ROW_NUMBER() OVER (
                PARTITION BY r.Name, r.Start
                ORDER BY r.Created DESC, r.Id DESC
            ) AS RowNumber
        FROM PayrunResult r
        WHERE r.TenantId = p_tenantId
          AND r.EmployeeId = p_employeeId
          -- period filter: single hash → equality seek; multiple → IN list
          AND (v_startHashCount = 0 OR
               (v_startHashCount = 1 AND r.StartHash = v_startHash) OR
               (v_startHashCount > 1 AND r.StartHash IN (
                   SELECT CAST(jt.val AS SIGNED)
                   FROM JSON_TABLE(p_periodStartHashes, '$[*]' COLUMNS (val VARCHAR(20) PATH '$')) AS jt)))
          AND (p_divisionId IS NULL OR r.DivisionId = p_divisionId)
          AND (p_names IS NULL OR v_nameCount = 0
               OR (v_nameCount = 1 AND r.Name = v_name)
               OR (v_nameCount > 1 AND r.Name IN (
                   SELECT jt.val
                   FROM JSON_TABLE(p_names, '$[*]' COLUMNS (val VARCHAR(128) PATH '$')) AS jt)))
          AND (p_evaluationDate IS NULL OR r.Created <= p_evaluationDate)
          AND (p_jobStatus IS NULL OR r.PayrunJobId IN (
                   SELECT pj.Id FROM PayrunJob pj WHERE (pj.JobStatus & p_jobStatus) = pj.JobStatus))
          AND (r.Forecast IS NULL OR r.Forecast = p_forecast)
          AND (p_noRetro = 0 OR r.ParentJobId IS NULL)
          AND (p_excludeParentJobId IS NULL OR r.ParentJobId IS NULL
               OR r.ParentJobId <> p_excludeParentJobId)
    )
    -- Phase 2: key lookup only for winning rows
    SELECT r.*
    FROM PayrunResult r
    INNER JOIN Winners w ON w.Id = r.Id
    WHERE w.RowNumber = 1;
END$$

DELIMITER ;
