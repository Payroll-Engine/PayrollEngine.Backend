-- =============================================================================
-- GetNumericAttributeValue
-- Returns DECIMAL(28,6) value of a JSON attribute, NULL if not numeric.
--
-- T-SQL: RETURN IIF(@type = 2, CAST(@value AS DECIMAL(28,6)), NULL)
-- MySQL: JSON_TYPE checks for 'INTEGER' or 'DOUBLE'
-- =============================================================================

USE PayrollEngine;

SET GLOBAL log_bin_trust_function_creators = 1;

DELIMITER $$

DROP FUNCTION IF EXISTS GetNumericAttributeValue$$
CREATE FUNCTION GetNumericAttributeValue(
    p_attributes LONGTEXT,
    p_name       VARCHAR(255)
)
RETURNS DECIMAL(28,6)
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

    IF v_type IN ('INTEGER', 'DOUBLE') THEN
        RETURN CAST(JSON_EXTRACT(p_attributes, v_path) AS DECIMAL(28,6));
    END IF;

    RETURN NULL;
END$$

DELIMITER ;
