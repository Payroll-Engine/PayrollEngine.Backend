-- =============================================================================
-- DeleteAllGlobalCaseValues
-- =============================================================================

USE PayrollEngine;

DELIMITER $$

DROP PROCEDURE IF EXISTS DeleteAllGlobalCaseValues$$
CREATE PROCEDURE DeleteAllGlobalCaseValues()
BEGIN
    DELETE FROM GlobalCaseValueChange;
    DELETE FROM GlobalCaseDocument;
    DELETE FROM GlobalCaseValue;
    DELETE FROM GlobalCaseChange;
END$$

DELIMITER ;
