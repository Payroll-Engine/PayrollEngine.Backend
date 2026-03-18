-- =============================================================================
-- GetCompanyCaseValues
-- =============================================================================

USE PayrollEngine;

DELIMITER $$

DROP PROCEDURE IF EXISTS GetCompanyCaseValues$$
CREATE PROCEDURE GetCompanyCaseValues(
    IN p_parentId   INT,
    IN p_sql        LONGTEXT,
    IN p_attributes LONGTEXT
)
BEGIN
    DECLARE v_attrSql  LONGTEXT;
    DECLARE v_pivotSql LONGTEXT;

    DECLARE EXIT HANDLER FOR SQLEXCEPTION
    BEGIN
        DROP TEMPORARY TABLE IF EXISTS CompanyCaseValuePivot;
        RESIGNAL;
    END;

    SET v_attrSql  = BuildAttributeQuery('CompanyCaseValue.Attributes', p_attributes);
    SET v_pivotSql = CONCAT(
        'CREATE TEMPORARY TABLE CompanyCaseValuePivot AS SELECT CompanyCaseValue.*',
        v_attrSql,
        ' FROM CompanyCaseValue WHERE CompanyCaseValue.TenantId = ',
        CAST(p_parentId AS CHAR));

    DROP TEMPORARY TABLE IF EXISTS CompanyCaseValuePivot;

    SET @_pivot_sql = v_pivotSql;
    PREPARE _pivot_stmt FROM @_pivot_sql;
    EXECUTE _pivot_stmt;
    DEALLOCATE PREPARE _pivot_stmt;

    SET @_query_sql = p_sql;
    PREPARE _query_stmt FROM @_query_sql;
    EXECUTE _query_stmt;
    DEALLOCATE PREPARE _query_stmt;

    DROP TEMPORARY TABLE IF EXISTS CompanyCaseValuePivot;
END$$

DELIMITER ;
