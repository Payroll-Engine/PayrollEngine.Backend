-- =============================================================================
-- GetLocalizedValue
-- Returns the value for the given culture from a JSON localizations object.
-- Falls back to p_fallback if culture key not found.
--
-- T-SQL: SELECT @value = value FROM OPENJSON(@localizations) WHERE [key] = @culture
-- MySQL: JSON_EXTRACT with quoted key syntax $."de-CH" (handles hyphens)
--
-- IMPORTANT: Culture codes like "de-CH" contain a hyphen.
-- Unquoted path $.de-CH fails with ERROR 3143. Use $."de-CH" instead.
-- =============================================================================

USE PayrollEngine;

SET GLOBAL log_bin_trust_function_creators = 1;

DELIMITER $$

DROP FUNCTION IF EXISTS GetLocalizedValue$$
CREATE FUNCTION GetLocalizedValue(
    p_localizations LONGTEXT,
    p_culture       VARCHAR(128),
    p_fallback      LONGTEXT
)
RETURNS LONGTEXT
DETERMINISTIC
READS SQL DATA
BEGIN
    DECLARE v_path  VARCHAR(512);
    DECLARE v_value LONGTEXT;

    IF p_localizations IS NULL OR p_culture IS NULL THEN
        RETURN p_fallback;
    END IF;

    SET v_path  = CONCAT('$."', p_culture, '"');
    SET v_value = JSON_UNQUOTE(JSON_EXTRACT(p_localizations, v_path));

    IF v_value IS NULL THEN
        RETURN p_fallback;
    END IF;

    RETURN v_value;
END$$

DELIMITER ;
