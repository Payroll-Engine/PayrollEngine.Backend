-- =============================================================================
-- GetEmployeeCaseValuesByTenant
-- Direct JOIN query -- no pivot, no temp table needed.
-- OPENJSON(@fieldNames) -> JSON_TABLE(p_fieldNames, '$[*]' COLUMNS ...)
-- =============================================================================

USE PayrollEngine;

DELIMITER $$

DROP PROCEDURE IF EXISTS GetEmployeeCaseValuesByTenant$$
CREATE PROCEDURE GetEmployeeCaseValuesByTenant(
    IN p_tenantId       INT,
    IN p_valueDate      DATETIME(6),
    IN p_evaluationDate DATETIME(6),
    IN p_fieldNames     LONGTEXT,
    IN p_forecast       VARCHAR(128)
)
BEGIN
    SELECT
        ecv.Id, ecv.Status, ecv.Created, ecv.Updated,
        ecv.EmployeeId, ecv.DivisionId,
        ecv.CaseName, ecv.CaseNameLocalizations,
        ecv.CaseFieldName, ecv.CaseFieldNameLocalizations,
        ecv.CaseSlot, ecv.CaseSlotLocalizations,
        ecv.ValueType, ecv.Value, ecv.NumericValue, ecv.Culture,
        ecv.CaseRelation, ecv.CancellationDate, ecv.Start, ecv.End,
        ecv.Forecast, ecv.Tags, ecv.Attributes
    FROM EmployeeCaseValue ecv
    INNER JOIN Employee e ON e.Id = ecv.EmployeeId
    WHERE e.TenantId = p_tenantId
      AND e.Status = 0
      AND ecv.CancellationDate IS NULL
      AND (p_evaluationDate IS NULL OR ecv.Created <= p_evaluationDate)
      AND (p_valueDate IS NULL OR ecv.Start IS NULL OR ecv.Start <= p_valueDate)
      AND (p_valueDate IS NULL OR ecv.End   IS NULL OR ecv.End   >  p_valueDate)
      AND (
          (p_forecast IS NULL     AND ecv.Forecast IS NULL)
          OR (p_forecast IS NOT NULL AND (ecv.Forecast IS NULL OR ecv.Forecast = p_forecast))
      )
      AND (
          p_fieldNames IS NULL
          OR ecv.CaseFieldName IN (
              SELECT jt.val
              FROM JSON_TABLE(p_fieldNames, '$[*]' COLUMNS (val VARCHAR(128) PATH '$')) AS jt)
      )
    ORDER BY ecv.EmployeeId ASC, ecv.CaseFieldName ASC, ecv.Created DESC;
END$$

DELIMITER ;
