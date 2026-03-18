-- =============================================================================
-- GetDerivedLookups
-- =============================================================================

USE PayrollEngine;

DELIMITER $$

DROP PROCEDURE IF EXISTS GetDerivedLookups$$
CREATE PROCEDURE GetDerivedLookups(
    IN p_tenantId       INT,
    IN p_payrollId      INT,
    IN p_regulationDate DATETIME(6),
    IN p_createdBefore  DATETIME(6),
    IN p_lookupNames    VARCHAR(4000)
)
BEGIN
    WITH DerivedRegulations AS (
        SELECT r.Id, pl.Level, pl.Priority,
            ROW_NUMBER() OVER (
                PARTITION BY pl.Id, r.Name
                ORDER BY r.ValidFrom DESC, r.Created DESC
            ) AS RowNumber
        FROM PayrollLayer pl
        INNER JOIN Regulation r ON pl.RegulationName = r.Name
        WHERE r.Status = 0
          AND (r.TenantId = p_tenantId OR r.SharedRegulation = 1)
          AND r.Created <= p_createdBefore
          AND (r.ValidFrom IS NULL OR r.ValidFrom <= p_regulationDate)
          AND pl.Status = 0 AND pl.PayrollId = p_payrollId
    ),
    Regulations AS (SELECT Id, Level, Priority FROM DerivedRegulations WHERE RowNumber = 1)
    SELECT
        reg.Id AS RegulationId, reg.Level, reg.Priority,
        lk.*
    FROM Lookup lk
    INNER JOIN Regulations reg ON lk.RegulationId = reg.Id
    WHERE lk.Status = 0
      AND lk.Created <= p_createdBefore
      AND (p_lookupNames IS NULL
           OR LOWER(lk.Name) IN (
               SELECT LOWER(jt.val)
               FROM JSON_TABLE(p_lookupNames, '$[*]' COLUMNS (val VARCHAR(128) PATH '$')) AS jt))
    ORDER BY reg.Level DESC, reg.Priority DESC;
END$$

DELIMITER ;
