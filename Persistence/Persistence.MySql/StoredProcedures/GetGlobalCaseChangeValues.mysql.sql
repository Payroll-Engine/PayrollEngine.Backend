-- =============================================================================
-- GetGlobalCaseChangeValues
-- IIF(@culture IS NULL,...) -> IF(p_culture IS NULL,...)
-- `User` backtick-quoted (reserved word in MySQL)
-- =============================================================================

USE PayrollEngine;

DELIMITER $$

DROP PROCEDURE IF EXISTS GetGlobalCaseChangeValues$$
CREATE PROCEDURE GetGlobalCaseChangeValues(
    IN p_parentId   INT,
    IN p_sql        LONGTEXT,
    IN p_attributes LONGTEXT,
    IN p_culture    VARCHAR(128)
)
BEGIN
    DECLARE v_attrSql       LONGTEXT;
    DECLARE v_pivotSql      LONGTEXT;
    DECLARE v_caseName      LONGTEXT;
    DECLARE v_caseFieldName LONGTEXT;
    DECLARE v_caseSlot      LONGTEXT;

    DECLARE EXIT HANDLER FOR SQLEXCEPTION
    BEGIN
        DROP TEMPORARY TABLE IF EXISTS GlobalCaseChangeValuePivot;
        RESIGNAL;
    END;

    IF p_culture IS NULL THEN
        SET v_caseName      = 'GlobalCaseValue.CaseName';
        SET v_caseFieldName = 'GlobalCaseValue.CaseFieldName';
        SET v_caseSlot      = 'GlobalCaseValue.CaseSlot';
    ELSE
        SET v_caseName      = CONCAT('GetLocalizedValue(GlobalCaseValue.CaseNameLocalizations, ''', p_culture, ''', GlobalCaseValue.CaseName)');
        SET v_caseFieldName = CONCAT('GetLocalizedValue(GlobalCaseValue.CaseFieldNameLocalizations, ''', p_culture, ''', GlobalCaseValue.CaseFieldName)');
        SET v_caseSlot      = CONCAT('GetLocalizedValue(GlobalCaseValue.CaseSlotLocalizations, ''', p_culture, ''', GlobalCaseValue.CaseSlot)');
    END IF;

    SET v_attrSql  = BuildAttributeQuery('GlobalCaseValue.Attributes', p_attributes);
    SET v_pivotSql = CONCAT(
        'CREATE TEMPORARY TABLE GlobalCaseChangeValuePivot AS SELECT',
        ' GlobalCaseChange.TenantId,',
        ' GlobalCaseChange.Id AS CaseChangeId,',
        ' GlobalCaseChange.Created AS CaseChangeCreated,',
        ' GlobalCaseChange.Reason,',
        ' GlobalCaseChange.ValidationCaseName,',
        ' GlobalCaseChange.CancellationType,',
        ' GlobalCaseChange.CancellationId,',
        ' GlobalCaseChange.CancellationDate,',
        ' NULL AS EmployeeId,',
        ' GlobalCaseChange.UserId,',
        ' `User`.Identifier AS UserIdentifier,',
        ' GlobalCaseChange.DivisionId,',
        ' GlobalCaseValue.Id,',
        ' GlobalCaseValue.Created,',
        ' GlobalCaseValue.Updated,',
        ' GlobalCaseValue.Status,',
        ' ', v_caseName,      ' AS CaseName,',
        ' ', v_caseFieldName, ' AS CaseFieldName,',
        ' ', v_caseSlot,      ' AS CaseSlot,',
        ' GlobalCaseValue.CaseRelation,',
        ' GlobalCaseValue.ValueType,',
        ' GlobalCaseValue.Value,',
        ' GlobalCaseValue.NumericValue,',
        ' GlobalCaseValue.Culture,',
        ' GlobalCaseValue.Start,',
        ' GlobalCaseValue.End,',
        ' GlobalCaseValue.Forecast,',
        ' GlobalCaseValue.Tags,',
        ' GlobalCaseValue.Attributes,',
        ' (SELECT COUNT(*) FROM GlobalCaseDocument WHERE CaseValueId = GlobalCaseValue.Id) AS Documents',
        v_attrSql,
        ' FROM GlobalCaseValue',
        ' LEFT JOIN GlobalCaseValueChange ON GlobalCaseValue.Id = GlobalCaseValueChange.CaseValueId',
        ' LEFT JOIN GlobalCaseChange ON GlobalCaseValueChange.CaseChangeId = GlobalCaseChange.Id',
        ' LEFT JOIN `User` ON `User`.Id = GlobalCaseChange.UserId',
        ' WHERE GlobalCaseChange.TenantId = ', CAST(p_parentId AS CHAR));

    DROP TEMPORARY TABLE IF EXISTS GlobalCaseChangeValuePivot;

    SET @_pivot_sql = v_pivotSql;
    PREPARE _pivot_stmt FROM @_pivot_sql;
    EXECUTE _pivot_stmt;
    DEALLOCATE PREPARE _pivot_stmt;

    SET @_query_sql = p_sql;
    PREPARE _query_stmt FROM @_query_sql;
    EXECUTE _query_stmt;
    DEALLOCATE PREPARE _query_stmt;

    DROP TEMPORARY TABLE IF EXISTS GlobalCaseChangeValuePivot;
END$$

DELIMITER ;
