-- =============================================================================
-- GetAttributeNames
-- Builds a comma-separated list of attribute names from a JSON array.
-- Used by GetPayrollResultValues for the outer SELECT projection.
--
-- T-SQL: imperative WHILE + OPENJSON + string concatenation
-- MySQL: JSON_TABLE with FOR ORDINALITY + GROUP_CONCAT
--
-- Output: '' if empty, ',' + names + newline if non-empty
-- =============================================================================

USE PayrollEngine;

SET GLOBAL log_bin_trust_function_creators = 1;

DELIMITER $$

DROP FUNCTION IF EXISTS GetAttributeNames$$
CREATE FUNCTION GetAttributeNames(
    p_attributes LONGTEXT
)
RETURNS LONGTEXT
DETERMINISTIC
READS SQL DATA
BEGIN
    DECLARE v_parts LONGTEXT;
    DECLARE v_sql   LONGTEXT DEFAULT '';

    IF p_attributes IS NULL THEN
        RETURN v_sql;
    END IF;

    IF JSON_LENGTH(p_attributes) = 0 THEN
        RETURN v_sql;
    END IF;

    SELECT GROUP_CONCAT(j.val ORDER BY j.idx SEPARATOR ', ')
    INTO v_parts
    FROM JSON_TABLE(
        p_attributes,
        '$[*]' COLUMNS (idx FOR ORDINALITY, val VARCHAR(128) PATH '$')
    ) AS j
    WHERE LENGTH(TRIM(j.val)) > 0;

    IF v_parts IS NOT NULL AND LENGTH(v_parts) > 0 THEN
        SET v_sql = CONCAT(',', v_parts, '\n        ');
    END IF;

    RETURN v_sql;
END$$

DELIMITER ;
