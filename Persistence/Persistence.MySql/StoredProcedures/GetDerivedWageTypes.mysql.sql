-- =============================================================================
-- GetDerivedWageTypes
-- Excludes Binary, Script, ScriptVersion (performance hint identical to T-SQL)
-- =============================================================================

USE PayrollEngine;

DELIMITER $$

DROP PROCEDURE IF EXISTS GetDerivedWageTypes$$
CREATE PROCEDURE GetDerivedWageTypes(
    IN p_tenantId        INT,
    IN p_payrollId       INT,
    IN p_regulationDate  DATETIME(6),
    IN p_createdBefore   DATETIME(6),
    IN p_wageTypeNumbers VARCHAR(4000),
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
        wt.Id, wt.Status, wt.Created, wt.Updated, wt.RegulationId,
        wt.Name, wt.NameLocalizations, wt.WageTypeNumber,
        wt.Description, wt.DescriptionLocalizations,
        wt.OverrideType, wt.ValueType, wt.Calendar, wt.Culture,
        wt.Collectors, wt.CollectorGroups,
        wt.ValueExpression, wt.ResultExpression,
        wt.ValueActions, wt.ResultActions,
        wt.ScriptHash, wt.Attributes, wt.Clusters
    FROM WageType wt
    INNER JOIN Regulations reg ON wt.RegulationId = reg.Id
    WHERE wt.Status = 0
      AND wt.Created <= p_createdBefore
      AND ((p_includeClusters IS NULL AND p_excludeClusters IS NULL)
           OR IsMatchingCluster(p_includeClusters, p_excludeClusters, wt.Clusters) = 1)
      AND (p_wageTypeNumbers IS NULL
           OR wt.WageTypeNumber IN (
               SELECT CAST(jt.val AS DECIMAL(28,6))
               FROM JSON_TABLE(p_wageTypeNumbers, '$[*]' COLUMNS (val VARCHAR(50) PATH '$')) AS jt))
    ORDER BY wt.WageTypeNumber, reg.Level DESC, reg.Priority DESC;
END$$

DELIMITER ;
