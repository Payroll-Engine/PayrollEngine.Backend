-- =============================================================================
-- GetEmployeeCaseValues
-- Filter is EmployeeId (not TenantId) -- employee-scoped pivot
-- =============================================================================

USE PayrollEngine;

DELIMITER $$

DROP PROCEDURE IF EXISTS GetEmployeeCaseValues$$
CREATE PROCEDURE GetEmployeeCaseValues(
    IN p_parentId   INT,
    IN p_sql        LONGTEXT,
    IN p_attributes LONGTEXT
)
BEGIN
    DECLARE v_attrSql  LONGTEXT;
    DECLARE v_pivotSql LONGTEXT;

    DECLARE EXIT HANDLER FOR SQLEXCEPTION
    BEGIN
        DROP TEMPORARY TABLE IF EXISTS EmployeeCaseValuePivot;
        RESIGNAL;
    END;

    SET v_attrSql  = BuildAttributeQuery('EmployeeCaseValue.Attributes', p_attributes);
    SET v_pivotSql = CONCAT(
        'CREATE TEMPORARY TABLE EmployeeCaseValuePivot AS SELECT EmployeeCaseValue.*',
        v_attrSql,
        ' FROM EmployeeCaseValue WHERE EmployeeCaseValue.EmployeeId = ',
        CAST(p_parentId AS CHAR));

    DROP TEMPORARY TABLE IF EXISTS EmployeeCaseValuePivot;

    SET @_pivot_sql = v_pivotSql;
    PREPARE _pivot_stmt FROM @_pivot_sql;
    EXECUTE _pivot_stmt;
    DEALLOCATE PREPARE _pivot_stmt;

    SET @_query_sql = p_sql;
    PREPARE _query_stmt FROM @_query_sql;
    EXECUTE _query_stmt;
    DEALLOCATE PREPARE _query_stmt;

    DROP TEMPORARY TABLE IF EXISTS EmployeeCaseValuePivot;
END$$

DELIMITER ;
