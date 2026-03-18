-- =============================================================================
-- GetTextAttributeValue
-- Returns the string value of a JSON attribute key, NULL if type is not string.
--
-- T-SQL: RETURN IIF(@type = 1, @value, NULL)  -- type 1 = string in OPENJSON
-- MySQL: JSON_EXTRACT + JSON_TYPE check for 'STRING'
-- =============================================================================

USE PayrollEngine;

SET GLOBAL log_bin_trust_function_creators = 1;

DELIMITER $$

DROP FUNCTION IF EXISTS GetTextAttributeValue$$
CREATE FUNCTION GetTextAttributeValue(
    p_attributes LONGTEXT,
    p_name       VARCHAR(255)
)
RETURNS LONGTEXT
DETERMINISTIC
READS SQL DATA
BEGIN
    DECLARE v_path  VARCHAR(512);
    DECLARE v_type  VARCHAR(20);

    IF p_attributes IS NULL OR p_name IS NULL THEN
        RETURN NULL;
    END IF;

    SET v_path = CONCAT('$.', p_name);
    SET v_type = JSON_TYPE(JSON_EXTRACT(p_attributes, v_path));

    IF v_type = 'STRING' THEN
        RETURN JSON_UNQUOTE(JSON_EXTRACT(p_attributes, v_path));
    END IF;

    RETURN NULL;
END$$

DELIMITER ;
