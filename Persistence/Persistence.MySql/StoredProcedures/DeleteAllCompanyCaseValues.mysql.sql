-- =============================================================================
-- DeleteAllCompanyCaseValues
-- =============================================================================

USE PayrollEngine;

DELIMITER $$

DROP PROCEDURE IF EXISTS DeleteAllCompanyCaseValues$$
CREATE PROCEDURE DeleteAllCompanyCaseValues()
BEGIN
    DELETE FROM CompanyCaseValueChange;
    DELETE FROM CompanyCaseDocument;
    DELETE FROM CompanyCaseValue;
    DELETE FROM CompanyCaseChange;
END$$

DELIMITER ;
