-- =============================================================================
-- GetGlobalCaseValues
-- Creates TEMPORARY TABLE pivot + executes caller query against it.
-- PREPARE requires user variable (@), not local variable (DECLARE v_...).
-- =============================================================================

USE PayrollEngine;

DELIMITER $$

DROP PROCEDURE IF EXISTS GetGlobalCaseValues$$
CREATE PROCEDURE GetGlobalCaseValues(
    IN p_parentId   INT,
    IN p_sql        LONGTEXT,
    IN p_attributes LONGTEXT
)
BEGIN
    DECLARE v_attrSql  LONGTEXT;
    DECLARE v_pivotSql LONGTEXT;

    DECLARE EXIT HANDLER FOR SQLEXCEPTION
    BEGIN
        DROP TEMPORARY TABLE IF EXISTS GlobalCaseValuePivot;
        RESIGNAL;
    END;

    SET v_attrSql  = BuildAttributeQuery('GlobalCaseValue.Attributes', p_attributes);
    SET v_pivotSql = CONCAT(
        'CREATE TEMPORARY TABLE GlobalCaseValuePivot AS SELECT GlobalCaseValue.*',
        v_attrSql,
        ' FROM GlobalCaseValue WHERE GlobalCaseValue.TenantId = ',
        CAST(p_parentId AS CHAR));

    DROP TEMPORARY TABLE IF EXISTS GlobalCaseValuePivot;

    SET @_pivot_sql = v_pivotSql;
    PREPARE _pivot_stmt FROM @_pivot_sql;
    EXECUTE _pivot_stmt;
    DEALLOCATE PREPARE _pivot_stmt;

    SET @_query_sql = p_sql;
    PREPARE _query_stmt FROM @_query_sql;
    EXECUTE _query_stmt;
    DEALLOCATE PREPARE _query_stmt;

    DROP TEMPORARY TABLE IF EXISTS GlobalCaseValuePivot;
END$$

DELIMITER ;
