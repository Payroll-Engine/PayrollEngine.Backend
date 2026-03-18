-- =============================================================================
-- DeleteAllEmployeeCaseValues
-- =============================================================================

USE PayrollEngine;

DELIMITER $$

DROP PROCEDURE IF EXISTS DeleteAllEmployeeCaseValues$$
CREATE PROCEDURE DeleteAllEmployeeCaseValues()
BEGIN
    DELETE FROM EmployeeCaseValueChange;
    DELETE FROM EmployeeCaseDocument;
    DELETE FROM EmployeeCaseValue;
    DELETE FROM EmployeeCaseChange;
END$$

DELIMITER ;
