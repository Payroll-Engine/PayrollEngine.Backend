-- =============================================================================
-- GetDerivedCaseFieldsOfCase
-- Filtered by case names (not field names).
-- =============================================================================

USE PayrollEngine;

DELIMITER $$

DROP PROCEDURE IF EXISTS GetDerivedCaseFieldsOfCase$$
CREATE PROCEDURE GetDerivedCaseFieldsOfCase(
    IN p_tenantId        INT,
    IN p_payrollId       INT,
    IN p_regulationDate  DATETIME(6),
    IN p_createdBefore   DATETIME(6),
    IN p_caseNames       VARCHAR(4000),
    IN p_includeClusters VARCHAR(4000),
    IN p_excludeClusters VARCHAR(4000)
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
        c.Id AS CaseId, c.CaseType,
        cf.*
    FROM CaseField cf
    INNER JOIN `Case` c ON cf.CaseId = c.Id
    INNER JOIN Regulations reg ON c.RegulationId = reg.Id
    WHERE cf.Status = 0
      AND cf.Created <= p_createdBefore
      AND ((p_includeClusters IS NULL AND p_excludeClusters IS NULL)
           OR IsMatchingCluster(p_includeClusters, p_excludeClusters, cf.Clusters) = 1)
      AND (p_caseNames IS NULL
           OR LOWER(c.Name) IN (
               SELECT LOWER(jt.val)
               FROM JSON_TABLE(p_caseNames, '$[*]' COLUMNS (val VARCHAR(128) PATH '$')) AS jt))
    ORDER BY reg.Level DESC, reg.Priority DESC;
END$$

DELIMITER ;
