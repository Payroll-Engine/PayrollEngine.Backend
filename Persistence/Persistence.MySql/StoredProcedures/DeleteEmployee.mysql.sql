-- =============================================================================
-- DeleteEmployee
-- DELETE t FROM t INNER JOIN -> DELETE t USING t INNER JOIN (MySQL syntax)
-- =============================================================================

USE PayrollEngine;

DELIMITER $$

DROP PROCEDURE IF EXISTS DeleteEmployee$$
CREATE PROCEDURE DeleteEmployee(
    IN p_tenantId   INT,
    IN p_employeeId INT
)
BEGIN
    DELETE pr FROM PayrunResult pr
    INNER JOIN PayrollResult prl ON pr.PayrollResultId = prl.Id
    WHERE prl.TenantId = p_tenantId AND prl.EmployeeId = p_employeeId;

    DELETE wtcr FROM WageTypeCustomResult wtcr
    INNER JOIN WageTypeResult wtr ON wtcr.WageTypeResultId = wtr.Id
    INNER JOIN PayrollResult prl ON wtr.PayrollResultId = prl.Id
    WHERE prl.TenantId = p_tenantId AND prl.EmployeeId = p_employeeId;

    DELETE wtr FROM WageTypeResult wtr
    INNER JOIN PayrollResult prl ON wtr.PayrollResultId = prl.Id
    WHERE prl.TenantId = p_tenantId AND prl.EmployeeId = p_employeeId;

    DELETE ccr FROM CollectorCustomResult ccr
    INNER JOIN CollectorResult cr ON ccr.CollectorResultId = cr.Id
    INNER JOIN PayrollResult prl ON cr.PayrollResultId = prl.Id
    WHERE prl.TenantId = p_tenantId AND prl.EmployeeId = p_employeeId;

    DELETE cr FROM CollectorResult cr
    INNER JOIN PayrollResult prl ON cr.PayrollResultId = prl.Id
    WHERE prl.TenantId = p_tenantId AND prl.EmployeeId = p_employeeId;

    DELETE FROM PayrollResult WHERE TenantId = p_tenantId AND EmployeeId = p_employeeId;

    DELETE pje FROM PayrunJobEmployee pje
    INNER JOIN PayrunJob pj ON pje.PayrunJobId = pj.Id
    WHERE pj.TenantId = p_tenantId AND pje.EmployeeId = p_employeeId;

    DELETE ecvc FROM EmployeeCaseValueChange ecvc
    INNER JOIN EmployeeCaseChange ecc ON ecvc.CaseChangeId = ecc.Id
    INNER JOIN Employee e ON ecc.EmployeeId = e.Id
    WHERE e.TenantId = p_tenantId AND e.Id = p_employeeId;

    DELETE ecc FROM EmployeeCaseChange ecc
    INNER JOIN Employee e ON ecc.EmployeeId = e.Id
    WHERE e.TenantId = p_tenantId AND e.Id = p_employeeId;

    DELETE ecd FROM EmployeeCaseDocument ecd
    INNER JOIN EmployeeCaseValue ecv ON ecd.CaseValueId = ecv.Id
    INNER JOIN Employee e ON ecv.EmployeeId = e.Id
    WHERE e.TenantId = p_tenantId AND e.Id = p_employeeId;

    DELETE ecv FROM EmployeeCaseValue ecv
    INNER JOIN Employee e ON ecv.EmployeeId = e.Id
    WHERE e.TenantId = p_tenantId AND e.Id = p_employeeId;

    DELETE ed FROM EmployeeDivision ed
    INNER JOIN Employee e ON ed.EmployeeId = e.Id
    WHERE e.TenantId = p_tenantId AND e.Id = p_employeeId;

    DELETE FROM Employee WHERE TenantId = p_tenantId AND Id = p_employeeId;
END$$

DELIMITER ;
