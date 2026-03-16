-- =============================================================================
-- PayrollEngine MySQL -- Functions
-- Ports all 8 T-SQL functions to MySQL 8.0+
--
-- Functions ported (GetDerivedRegulations is eliminated -- inlined as CTE):
--   GetTextAttributeValue     -- T-SQL: OPENJSON type=1 -> MySQL: JSON_TYPE='STRING'
--   GetNumericAttributeValue  -- T-SQL: OPENJSON type=2 -> MySQL: JSON_TYPE='INTEGER'|'DOUBLE'
--   GetDateAttributeValue     -- T-SQL: OPENJSON type=1+CAST -> MySQL: JSON_TYPE='STRING'+CAST
--   GetLocalizedValue         -- T-SQL: OPENJSON [key]=@culture -> MySQL: JSON_EXTRACT $."culture"
--   IsMatchingCluster         -- T-SQL: WHILE+OPENJSON -> MySQL: JSON_TABLE+EXISTS
--   BuildAttributeQuery       -- T-SQL: WHILE+OPENJSON+concat -> MySQL: JSON_TABLE+GROUP_CONCAT
--   GetAttributeNames         -- T-SQL: WHILE+OPENJSON+concat -> MySQL: JSON_TABLE+GROUP_CONCAT
--
-- Key findings from PoC (35/35 tests passed):
--   - Culture codes (e.g. "de-CH") require quoted JSON path: $."de-CH"
--     (unquoted $.de-CH fails with ERROR 3143 due to hyphen)
--   - Attribute JSON keys are plain names ("City", not "TA_City")
--     The TA_/NA_/DA_ prefix is the column alias convention, not the JSON key
--   - FOR ORDINALITY in JSON_TABLE preserves array order for GROUP_CONCAT
--
-- All functions validated: see PayrollEngine.Private/database/poc/Results.md
-- =============================================================================

USE PayrollEngine;

SET GLOBAL log_bin_trust_function_creators = 1;

DELIMITER $$

-- =============================================================================
-- GetTextAttributeValue
-- Returns the string value of a JSON attribute key, NULL if type is not string.
--
-- T-SQL original:
--   SELECT @value = value, @type = type FROM OPENJSON(@attributes) WHERE [key] = @name
--   RETURN IIF(@type = 1, @value, NULL)   -- type 1 = string in OPENJSON
--
-- MySQL: JSON_EXTRACT + JSON_TYPE check for 'STRING'
-- =============================================================================
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

-- =============================================================================
-- GetNumericAttributeValue
-- Returns DECIMAL(28,6) value of a JSON attribute, NULL if not numeric.
--
-- T-SQL original:
--   RETURN IIF(@type = 2, CAST(@value AS DECIMAL(28,6)), NULL)  -- type 2 = number
--
-- MySQL: JSON_TYPE checks for 'INTEGER' or 'DOUBLE'
-- =============================================================================
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

-- =============================================================================
-- GetDateAttributeValue
-- Returns DATETIME(6) from a JSON attribute stored as ISO 8601 string.
-- NULL if attribute is not a string or not parseable as datetime.
--
-- T-SQL original:
--   RETURN IIF(@type = 1, CAST(@value AS DATETIME2(7)), NULL)
-- =============================================================================
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

-- =============================================================================
-- GetLocalizedValue
-- Returns the value for the given culture from a JSON localizations object.
-- Falls back to p_fallback if culture key not found.
--
-- T-SQL original:
--   SELECT @value = value FROM OPENJSON(@localizations) WHERE [key] = @culture
--   RETURN IIF(@value IS NULL, @fallback, @value)
--
-- IMPORTANT: Culture codes like "de-CH" contain a hyphen.
-- Unquoted path $.de-CH fails with ERROR 3143 in MySQL.
-- Fix: quoted key syntax $."de-CH" handles any key name including hyphens.
-- This applies to all localization fields: NameLocalizations,
-- DescriptionLocalizations, etc.
-- =============================================================================
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

    -- Quoted key syntax: $."de-CH" handles hyphens and other special chars
    SET v_path  = CONCAT('$."', p_culture, '"');
    SET v_value = JSON_UNQUOTE(JSON_EXTRACT(p_localizations, v_path));

    IF v_value IS NULL THEN
        RETURN p_fallback;
    END IF;

    RETURN v_value;
END$$

-- =============================================================================
-- IsMatchingCluster
-- Tests include/exclude cluster filters against a test cluster array.
-- All arrays are JSON arrays of VARCHAR(128).
--
-- T-SQL original: imperative WHILE loop over OPENJSON
-- MySQL: set-based JSON_TABLE + EXISTS / NOT EXISTS
--
-- Logic:
--   include: every cluster in includeClusters must appear in testClusters
--   exclude: no cluster in excludeClusters may appear in testClusters
--   returns 1 (match) or 0 (no match)
-- =============================================================================
DROP FUNCTION IF EXISTS IsMatchingCluster$$
CREATE FUNCTION IsMatchingCluster(
    p_includeClusters VARCHAR(4000),
    p_excludeClusters VARCHAR(4000),
    p_testClusters    VARCHAR(4000)
)
RETURNS TINYINT(1)
DETERMINISTIC
READS SQL DATA
BEGIN
    IF p_testClusters IS NULL THEN
        SET p_testClusters = '[]';
    END IF;

    -- Include check: any required cluster missing from testClusters -> no match
    IF p_includeClusters IS NOT NULL AND JSON_LENGTH(p_includeClusters) > 0 THEN
        IF EXISTS (
            SELECT 1
            FROM JSON_TABLE(
                p_includeClusters,
                '$[*]' COLUMNS (val VARCHAR(128) PATH '$')
            ) AS inc
            WHERE LENGTH(TRIM(inc.val)) > 0
              AND NOT EXISTS (
                SELECT 1
                FROM JSON_TABLE(
                    p_testClusters,
                    '$[*]' COLUMNS (val VARCHAR(128) PATH '$')
                ) AS tst
                WHERE tst.val = inc.val
              )
        ) THEN
            RETURN 0;
        END IF;
    END IF;

    -- Exclude check: any excluded cluster present in testClusters -> no match
    IF p_excludeClusters IS NOT NULL AND JSON_LENGTH(p_excludeClusters) > 0 THEN
        IF EXISTS (
            SELECT 1
            FROM JSON_TABLE(
                p_excludeClusters,
                '$[*]' COLUMNS (val VARCHAR(128) PATH '$')
            ) AS exc
            WHERE LENGTH(TRIM(exc.val)) > 0
              AND EXISTS (
                SELECT 1
                FROM JSON_TABLE(
                    p_testClusters,
                    '$[*]' COLUMNS (val VARCHAR(128) PATH '$')
                ) AS tst
                WHERE tst.val = exc.val
              )
        ) THEN
            RETURN 0;
        END IF;
    END IF;

    RETURN 1;
END$$

-- =============================================================================
-- BuildAttributeQuery
-- Builds a SQL fragment for dynamic attribute column projection.
-- Used by CaseValue pivot SPs and GetPayrollResultValues.
--
-- T-SQL original: imperative WHILE + OPENJSON + string concatenation
-- MySQL: JSON_TABLE with FOR ORDINALITY + GROUP_CONCAT (order preserved)
--
-- Output format (identical to T-SQL):
--   '' if p_attributes is NULL or empty
--   ',' + fragment + newline  if attributes present
--
-- Attribute prefix convention:
--   TA_ -> GetTextAttributeValue(field, 'stripped_name') AS TA_xxx
--   NA_ -> GetNumericAttributeValue(field, 'stripped_name') AS NA_xxx
--   DA_ -> GetDateAttributeValue(field, 'stripped_name') AS DA_xxx
--   NULL p_attributeField -> NULL AS xxx  (PayrunResult has no attribute field)
--
-- NOTE: Attribute JSON keys are plain names ("City"), not prefixed ("TA_City").
-- The TA_/NA_/DA_ prefix is the output column alias only.
-- =============================================================================
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
                CONCAT(
                    'GetTextAttributeValue(', p_attributeField, ', ''',
                    SUBSTRING(j.val, 4), ''') AS ', j.val)
            WHEN LEFT(j.val, 3) = 'DA_' THEN
                CONCAT(
                    'GetDateAttributeValue(', p_attributeField, ', ''',
                    SUBSTRING(j.val, 4), ''') AS ', j.val)
            WHEN LEFT(j.val, 3) = 'NA_' THEN
                CONCAT(
                    'GetNumericAttributeValue(', p_attributeField, ', ''',
                    SUBSTRING(j.val, 4), ''') AS ', j.val)
            ELSE NULL
        END
        ORDER BY j.idx
        SEPARATOR ', '
    )
    INTO v_parts
    FROM JSON_TABLE(
        p_attributes,
        '$[*]' COLUMNS (
            idx FOR ORDINALITY,
            val VARCHAR(128) PATH '$'
        )
    ) AS j
    WHERE LENGTH(TRIM(j.val)) > 0;

    IF v_parts IS NOT NULL AND LENGTH(v_parts) > 0 THEN
        SET v_sql = CONCAT(',', v_parts, '\n        ');
    END IF;

    RETURN v_sql;
END$$

-- =============================================================================
-- GetAttributeNames
-- Builds a comma-separated list of attribute names from a JSON array.
-- Used by GetPayrollResultValues for the outer SELECT projection.
--
-- T-SQL original: imperative WHILE + OPENJSON + string concatenation
-- MySQL: JSON_TABLE with FOR ORDINALITY + GROUP_CONCAT
--
-- Output format (identical to T-SQL):
--   '' if p_attributes is NULL or empty
--   ',' + names + newline  if non-empty
-- =============================================================================
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
        '$[*]' COLUMNS (
            idx FOR ORDINALITY,
            val VARCHAR(128) PATH '$'
        )
    ) AS j
    WHERE LENGTH(TRIM(j.val)) > 0;

    IF v_parts IS NOT NULL AND LENGTH(v_parts) > 0 THEN
        SET v_sql = CONCAT(',', v_parts, '\n        ');
    END IF;

    RETURN v_sql;
END$$

DELIMITER ;

SELECT 'PayrollEngine MySQL functions created (7/8 -- GetDerivedRegulations eliminated).' AS Result;
