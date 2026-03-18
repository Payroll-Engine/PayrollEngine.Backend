-- =============================================================================
-- DeleteAllNationalCaseValues
-- =============================================================================

USE PayrollEngine;

DELIMITER $$

DROP PROCEDURE IF EXISTS DeleteAllNationalCaseValues$$
CREATE PROCEDURE DeleteAllNationalCaseValues()
BEGIN
    DELETE FROM NationalCaseValueChange;
    DELETE FROM NationalCaseDocument;
    DELETE FROM NationalCaseValue;
    DELETE FROM NationalCaseChange;
END$$

DELIMITER ;
