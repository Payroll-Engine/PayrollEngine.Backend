-- =============================================================================
-- GetDateAttributeValue
-- Returns DATETIME(6) from a JSON attribute stored as ISO 8601 string.
-- NULL if attribute is not a string or not parseable as datetime.
--
-- T-SQL: RETURN IIF(@type = 1, CAST(@value AS DATETIME2(7)), NULL)
-- MySQL: JSON_TYPE='STRING' + CAST AS DATETIME(6)
-- =============================================================================

USE PayrollEngine;

SET GLOBAL log_bin_trust_function_creators = 1;

DELIMITER $$

DROP FUNCTION IF EXISTS GetDateAttributeValue$$
CREATE FUNCTION GetDateAttributeValue(
    p_attributes LONGTEXT,
    p_name       VARCHAR(255)
)
RETURNS DATETIME(6)
DETERMINISTIC
READS SQL DATA
BEGIN
    DECLARE v_path  VARCHAR(512);
    DECLARE v_type  VARCHAR(20);
    DECLARE v_raw   VARCHAR(50);

    IF p_attributes IS NULL OR p_name IS NULL THEN
        RETURN NULL;
    END IF;

    SET v_path = CONCAT('$.', p_name);
    SET v_type = JSON_TYPE(JSON_EXTRACT(p_attributes, v_path));

    IF v_type = 'STRING' THEN
        SET v_raw = JSON_UNQUOTE(JSON_EXTRACT(p_attributes, v_path));
        RETURN CAST(v_raw AS DATETIME(6));
    END IF;

    RETURN NULL;
END$$

DELIMITER ;
