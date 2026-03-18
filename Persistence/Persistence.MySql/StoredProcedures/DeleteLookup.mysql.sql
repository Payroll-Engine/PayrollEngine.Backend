-- =============================================================================
-- DeleteLookup
-- =============================================================================

USE PayrollEngine;

DELIMITER $$

DROP PROCEDURE IF EXISTS DeleteLookup$$
CREATE PROCEDURE DeleteLookup(
    IN p_tenantId INT,
    IN p_lookupId INT
)
BEGIN
    DELETE lva FROM LookupValueAudit lva
    INNER JOIN LookupValue lv ON lva.LookupValueId = lv.Id
    INNER JOIN Lookup lk ON lv.LookupId = lk.Id
    INNER JOIN Regulation r ON lk.RegulationId = r.Id
    WHERE r.TenantId = p_tenantId AND lk.Id = p_lookupId;

    DELETE lv FROM LookupValue lv
    INNER JOIN Lookup lk ON lv.LookupId = lk.Id
    INNER JOIN Regulation r ON lk.RegulationId = r.Id
    WHERE r.TenantId = p_tenantId AND lk.Id = p_lookupId;

    DELETE la FROM LookupAudit la
    INNER JOIN Lookup lk ON la.LookupId = lk.Id
    INNER JOIN Regulation r ON lk.RegulationId = r.Id
    WHERE r.TenantId = p_tenantId AND lk.Id = p_lookupId;

    DELETE lk FROM Lookup lk
    INNER JOIN Regulation r ON lk.RegulationId = r.Id
    WHERE r.TenantId = p_tenantId AND lk.Id = p_lookupId;
END$$

DELIMITER ;
