-- =============================================================================
-- DeleteAllCaseValues
-- Delegates to the four scope-specific procedures.
-- =============================================================================

USE PayrollEngine;

DELIMITER $$

DROP PROCEDURE IF EXISTS DeleteAllCaseValues$$
CREATE PROCEDURE DeleteAllCaseValues()
BEGIN
    CALL DeleteAllGlobalCaseValues();
    CALL DeleteAllNationalCaseValues();
    CALL DeleteAllCompanyCaseValues();
    CALL DeleteAllEmployeeCaseValues();
END$$

DELIMITER ;
