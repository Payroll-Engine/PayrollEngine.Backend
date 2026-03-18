-- =============================================================================
-- BuildAttributeQuery
-- Builds a SQL fragment for dynamic attribute column projection.
-- Used by CaseValue pivot SPs and GetPayrollResultValues.
--
-- T-SQL: imperative WHILE + OPENJSON + string concatenation
-- MySQL: JSON_TABLE with FOR ORDINALITY + GROUP_CONCAT (order preserved)
--
-- Output: '' if empty, ',' + fragment + newline if attributes present
--
-- Attribute prefix convention:
--   TA_ -> GetTextAttributeValue(field, 'name') AS TA_xxx
--   NA_ -> GetNumericAttributeValue(field, 'name') AS NA_xxx
--   DA_ -> GetDateAttributeValue(field, 'name') AS DA_xxx
--   NULL field -> NULL AS xxx  (PayrunResult has no attribute field)
--
-- NOTE: Attribute JSON keys are plain names ("City"), not prefixed ("TA_City").
-- The TA_/NA_/DA_ prefix is the output column alias only.
-- =============================================================================

USE PayrollEngine;

SET GLOBAL log_bin_trust_function_creators = 1;

DELIMITER $$

DROP FUNCTION IF EXISTS BuildAttributeQuery$$
CREATE FUNCTION BuildAttributeQuery(
    p_attributeField LONGTEXT,
    p_attributes     LONGTEXT
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

    SELECT GROUP_CONCAT(
        CASE
            WHEN p_attributeField IS NULL THEN
                CONCAT('NULL AS ', j.val)
            WHEN LEFT(j.val, 3) = 'TA_' THEN
                CONCAT('GetTextAttributeValue(', p_attributeField, ', ''', SUBSTRING(j.val, 4), ''') AS ', j.val)
            WHEN LEFT(j.val, 3) = 'DA_' THEN
                CONCAT('GetDateAttributeValue(', p_attributeField, ', ''', SUBSTRING(j.val, 4), ''') AS ', j.val)
            WHEN LEFT(j.val, 3) = 'NA_' THEN
                CONCAT('GetNumericAttributeValue(', p_attributeField, ', ''', SUBSTRING(j.val, 4), ''') AS ', j.val)
            ELSE NULL
        END
        ORDER BY j.idx
        SEPARATOR ', '
    )
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
