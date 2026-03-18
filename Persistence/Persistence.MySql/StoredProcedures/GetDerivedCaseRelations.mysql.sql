-- =============================================================================
-- GetDerivedCaseRelations
-- cr.`Order` backtick-quoted (reserved keyword in MySQL)
-- Excludes Binary, Script, ScriptVersion (performance hint)
-- =============================================================================

USE PayrollEngine;

DELIMITER $$

DROP PROCEDURE IF EXISTS GetDerivedCaseRelations$$
CREATE PROCEDURE GetDerivedCaseRelations(
    IN p_tenantId        INT,
    IN p_payrollId       INT,
    IN p_regulationDate  DATETIME(6),
    IN p_createdBefore   DATETIME(6),
    IN p_sourceCaseName  VARCHAR(128),
    IN p_targetCaseName  VARCHAR(128),
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
        cr.Id, cr.Status, cr.Created, cr.Updated, cr.RegulationId,
        cr.SourceCaseName, cr.SourceCaseNameLocalizations,
        cr.SourceCaseSlot, cr.SourceCaseSlotLocalizations,
        cr.TargetCaseName, cr.TargetCaseNameLocalizations,
        cr.TargetCaseSlot, cr.TargetCaseSlotLocalizations,
        cr.RelationHash, cr.BuildExpression, cr.ValidateExpression,
        cr.OverrideType, cr.`Order`,
        cr.ScriptHash, cr.Attributes, cr.Clusters,
        cr.BuildActions, cr.ValidateActions
    FROM CaseRelation cr
    INNER JOIN Regulations reg ON cr.RegulationId = reg.Id
    WHERE cr.Status = 0
      AND cr.Created <= p_createdBefore
      AND (p_sourceCaseName IS NULL
           OR LOWER(cr.SourceCaseName) = LOWER(p_sourceCaseName))
      AND (p_targetCaseName IS NULL
           OR LOWER(cr.TargetCaseName) = LOWER(p_targetCaseName))
      AND ((p_includeClusters IS NULL AND p_excludeClusters IS NULL)
           OR IsMatchingCluster(p_includeClusters, p_excludeClusters, cr.Clusters) = 1)
    ORDER BY cr.SourceCaseName, cr.TargetCaseName, reg.Level DESC, reg.Priority DESC;
END$$

DELIMITER ;
