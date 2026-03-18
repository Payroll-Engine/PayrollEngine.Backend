-- =============================================================================
-- GetDerivedReportTemplates
-- `Schema` column is backtick-quoted (reserved word in MySQL)
-- SELECT rt.* expands safely
-- =============================================================================

USE PayrollEngine;

DELIMITER $$

DROP PROCEDURE IF EXISTS GetDerivedReportTemplates$$
CREATE PROCEDURE GetDerivedReportTemplates(
    IN p_tenantId       INT,
    IN p_payrollId      INT,
    IN p_regulationDate DATETIME(6),
    IN p_createdBefore  DATETIME(6),
    IN p_reportNames    VARCHAR(4000),
    IN p_culture        VARCHAR(128)
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
        rt.*
    FROM ReportTemplate rt
    INNER JOIN Report rp ON rt.ReportId = rp.Id
    INNER JOIN Regulations reg ON rp.RegulationId = reg.Id
    WHERE rt.Status = 0
      AND rt.Created <= p_createdBefore
      AND (p_reportNames IS NULL
           OR LOWER(rp.Name) IN (
               SELECT LOWER(jt.val)
               FROM JSON_TABLE(p_reportNames, '$[*]' COLUMNS (val VARCHAR(128) PATH '$')) AS jt))
      AND (p_culture IS NULL OR rt.Culture = p_culture)
    ORDER BY reg.Level DESC, reg.Priority DESC;
END$$

DELIMITER ;
