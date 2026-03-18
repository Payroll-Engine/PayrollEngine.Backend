-- =============================================================================
-- DeletePayrunJob
-- =============================================================================

USE PayrollEngine;

DELIMITER $$

DROP PROCEDURE IF EXISTS DeletePayrunJob$$
CREATE PROCEDURE DeletePayrunJob(
    IN p_tenantId    INT,
    IN p_payrunJobId INT
)
BEGIN
    DELETE pr FROM PayrunResult pr
    INNER JOIN PayrollResult prl ON pr.PayrollResultId = prl.Id
    WHERE prl.TenantId = p_tenantId AND prl.PayrunJobId = p_payrunJobId;

    DELETE wtcr FROM WageTypeCustomResult wtcr
    INNER JOIN WageTypeResult wtr ON wtcr.WageTypeResultId = wtr.Id
    INNER JOIN PayrollResult prl ON wtr.PayrollResultId = prl.Id
    WHERE prl.TenantId = p_tenantId AND prl.PayrunJobId = p_payrunJobId;

    DELETE wtr FROM WageTypeResult wtr
    INNER JOIN PayrollResult prl ON wtr.PayrollResultId = prl.Id
    WHERE prl.TenantId = p_tenantId AND prl.PayrunJobId = p_payrunJobId;

    DELETE ccr FROM CollectorCustomResult ccr
    INNER JOIN CollectorResult cr ON ccr.CollectorResultId = cr.Id
    INNER JOIN PayrollResult prl ON cr.PayrollResultId = prl.Id
    WHERE prl.TenantId = p_tenantId AND prl.PayrunJobId = p_payrunJobId;

    DELETE cr FROM CollectorResult cr
    INNER JOIN PayrollResult prl ON cr.PayrollResultId = prl.Id
    WHERE prl.TenantId = p_tenantId AND prl.PayrunJobId = p_payrunJobId;

    DELETE FROM PayrollResult WHERE TenantId = p_tenantId AND PayrunJobId = p_payrunJobId;

    DELETE pje FROM PayrunJobEmployee pje
    INNER JOIN PayrunJob pj ON pje.PayrunJobId = pj.Id
    WHERE pj.TenantId = p_tenantId AND pje.PayrunJobId = p_payrunJobId;

    DELETE FROM PayrunJob WHERE TenantId = p_tenantId AND Id = p_payrunJobId;
END$$

DELIMITER ;
