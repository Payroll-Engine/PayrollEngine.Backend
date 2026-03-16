-- =============================================================================
-- PayrollEngine MySQL -- GetLookupRangeValue Fix
-- Replaces the broken entry in Remaining.mysql.sql
-- Run this AFTER Remaining.mysql.sql if GetLookupRangeValue failed.
-- =============================================================================

USE PayrollEngine;

DELIMITER $$

DROP PROCEDURE IF EXISTS GetLookupRangeValue$$
CREATE PROCEDURE GetLookupRangeValue(
    IN p_lookupId   INT,
    IN p_rangeValue DECIMAL(28,6),
    IN p_keyHash    INT
)
BEGIN
    DECLARE v_rangeSize DECIMAL(28,6) DEFAULT 0.0;
    DECLARE v_minValue  DECIMAL(28,6);
    DECLARE v_maxValue  DECIMAL(28,6);

    SELECT COALESCE(RangeSize, 0.0) INTO v_rangeSize
    FROM Lookup WHERE Id = p_lookupId;

    SELECT MIN(lv.RangeValue), MAX(lv.RangeValue) + v_rangeSize
    INTO v_minValue, v_maxValue
    FROM LookupValue lv
    INNER JOIN Lookup lk ON lv.LookupId = lk.Id
    WHERE lk.Id = p_lookupId;

    IF v_minValue IS NULL
       OR p_rangeValue < v_minValue
       OR p_rangeValue > v_maxValue THEN
        SELECT * FROM LookupValue WHERE 1 = 0;
    ELSE
        SELECT lv.*
        FROM LookupValue lv
        INNER JOIN Lookup lk ON lv.LookupId = lk.Id
        WHERE lk.Id = p_lookupId
          AND lv.RangeValue <= p_rangeValue
          AND (p_keyHash IS NULL OR lv.KeyHash = p_keyHash)
        ORDER BY lv.RangeValue DESC
        LIMIT 1;
    END IF;
END$$

DELIMITER ;

SELECT 'GetLookupRangeValue created.' AS Result;
