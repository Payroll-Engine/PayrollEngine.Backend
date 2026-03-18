-- =============================================================================
-- IsMatchingCluster
-- Tests include/exclude cluster filters against a test cluster array.
-- All arrays are JSON arrays of VARCHAR(128).
--
-- T-SQL: imperative WHILE loop over OPENJSON
-- MySQL: set-based JSON_TABLE + EXISTS / NOT EXISTS
--
-- Logic:
--   include: every cluster in includeClusters must appear in testClusters
--   exclude: no cluster in excludeClusters may appear in testClusters
--   returns 1 (match) or 0 (no match)
-- =============================================================================

USE PayrollEngine;

SET GLOBAL log_bin_trust_function_creators = 1;

DELIMITER $$

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

    IF p_includeClusters IS NOT NULL AND JSON_LENGTH(p_includeClusters) > 0 THEN
        IF EXISTS (
            SELECT 1
            FROM JSON_TABLE(p_includeClusters, '$[*]' COLUMNS (val VARCHAR(128) PATH '$')) AS inc
            WHERE LENGTH(TRIM(inc.val)) > 0
              AND NOT EXISTS (
                SELECT 1
                FROM JSON_TABLE(p_testClusters, '$[*]' COLUMNS (val VARCHAR(128) PATH '$')) AS tst
                WHERE tst.val = inc.val)
        ) THEN
            RETURN 0;
        END IF;
    END IF;

    IF p_excludeClusters IS NOT NULL AND JSON_LENGTH(p_excludeClusters) > 0 THEN
        IF EXISTS (
            SELECT 1
            FROM JSON_TABLE(p_excludeClusters, '$[*]' COLUMNS (val VARCHAR(128) PATH '$')) AS exc
            WHERE LENGTH(TRIM(exc.val)) > 0
              AND EXISTS (
                SELECT 1
                FROM JSON_TABLE(p_testClusters, '$[*]' COLUMNS (val VARCHAR(128) PATH '$')) AS tst
                WHERE tst.val = exc.val)
        ) THEN
            RETURN 0;
        END IF;
    END IF;

    RETURN 1;
END$$

DELIMITER ;
