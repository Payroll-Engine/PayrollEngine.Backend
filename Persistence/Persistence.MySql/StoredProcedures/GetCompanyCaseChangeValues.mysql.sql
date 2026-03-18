-- =============================================================================
-- GetCompanyCaseChangeValues
-- =============================================================================

USE PayrollEngine;

DELIMITER $$

DROP PROCEDURE IF EXISTS GetCompanyCaseChangeValues$$
CREATE PROCEDURE GetCompanyCaseChangeValues(
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
        DROP TEMPORARY TABLE IF EXISTS CompanyCaseChangeValuePivot;
        RESIGNAL;
    END;

    IF p_culture IS NULL THEN
        SET v_caseName      = 'CompanyCaseValue.CaseName';
        SET v_caseFieldName = 'CompanyCaseValue.CaseFieldName';
        SET v_caseSlot      = 'CompanyCaseValue.CaseSlot';
    ELSE
        SET v_caseName      = CONCAT('GetLocalizedValue(CompanyCaseValue.CaseNameLocalizations, ''', p_culture, ''', CompanyCaseValue.CaseName)');
        SET v_caseFieldName = CONCAT('GetLocalizedValue(CompanyCaseValue.CaseFieldNameLocalizations, ''', p_culture, ''', CompanyCaseValue.CaseFieldName)');
        SET v_caseSlot      = CONCAT('GetLocalizedValue(CompanyCaseValue.CaseSlotLocalizations, ''', p_culture, ''', CompanyCaseValue.CaseSlot)');
    END IF;

    SET v_attrSql  = BuildAttributeQuery('CompanyCaseValue.Attributes', p_attributes);
    SET v_pivotSql = CONCAT(
        'CREATE TEMPORARY TABLE CompanyCaseChangeValuePivot AS SELECT',
        ' CompanyCaseChange.TenantId,',
        ' CompanyCaseChange.Id AS CaseChangeId,',
        ' CompanyCaseChange.Created AS CaseChangeCreated,',
        ' CompanyCaseChange.Reason,',
        ' CompanyCaseChange.ValidationCaseName,',
        ' CompanyCaseChange.CancellationType,',
        ' CompanyCaseChange.CancellationId,',
        ' CompanyCaseChange.CancellationDate,',
        ' NULL AS EmployeeId,',
        ' CompanyCaseChange.UserId,',
        ' `User`.Identifier AS UserIdentifier,',
        ' CompanyCaseChange.DivisionId,',
        ' CompanyCaseValue.Id,',
        ' CompanyCaseValue.Created,',
        ' CompanyCaseValue.Updated,',
        ' CompanyCaseValue.Status,',
        ' ', v_caseName,      ' AS CaseName,',
        ' ', v_caseFieldName, ' AS CaseFieldName,',
        ' ', v_caseSlot,      ' AS CaseSlot,',
        ' CompanyCaseValue.CaseRelation,',
        ' CompanyCaseValue.ValueType,',
        ' CompanyCaseValue.Value,',
        ' CompanyCaseValue.NumericValue,',
        ' CompanyCaseValue.Culture,',
        ' CompanyCaseValue.Start,',
        ' CompanyCaseValue.End,',
        ' CompanyCaseValue.Forecast,',
        ' CompanyCaseValue.Tags,',
        ' CompanyCaseValue.Attributes,',
        ' (SELECT COUNT(*) FROM CompanyCaseDocument WHERE CaseValueId = CompanyCaseValue.Id) AS Documents',
        v_attrSql,
        ' FROM CompanyCaseValue',
        ' LEFT JOIN CompanyCaseValueChange ON CompanyCaseValue.Id = CompanyCaseValueChange.CaseValueId',
        ' LEFT JOIN CompanyCaseChange ON CompanyCaseValueChange.CaseChangeId = CompanyCaseChange.Id',
        ' LEFT JOIN `User` ON `User`.Id = CompanyCaseChange.UserId',
        ' WHERE CompanyCaseChange.TenantId = ', CAST(p_parentId AS CHAR));

    DROP TEMPORARY TABLE IF EXISTS CompanyCaseChangeValuePivot;

    SET @_pivot_sql = v_pivotSql;
    PREPARE _pivot_stmt FROM @_pivot_sql;
    EXECUTE _pivot_stmt;
    DEALLOCATE PREPARE _pivot_stmt;

    SET @_query_sql = p_sql;
    PREPARE _query_stmt FROM @_query_sql;
    EXECUTE _query_stmt;
    DEALLOCATE PREPARE _query_stmt;

    DROP TEMPORARY TABLE IF EXISTS CompanyCaseChangeValuePivot;
END$$

DELIMITER ;
