-- =============================================================================
-- GetNationalCaseChangeValues
-- =============================================================================

USE PayrollEngine;

DELIMITER $$

DROP PROCEDURE IF EXISTS GetNationalCaseChangeValues$$
CREATE PROCEDURE GetNationalCaseChangeValues(
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
        DROP TEMPORARY TABLE IF EXISTS NationalCaseChangeValuePivot;
        RESIGNAL;
    END;

    IF p_culture IS NULL THEN
        SET v_caseName      = 'NationalCaseValue.CaseName';
        SET v_caseFieldName = 'NationalCaseValue.CaseFieldName';
        SET v_caseSlot      = 'NationalCaseValue.CaseSlot';
    ELSE
        SET v_caseName      = CONCAT('GetLocalizedValue(NationalCaseValue.CaseNameLocalizations, ''', p_culture, ''', NationalCaseValue.CaseName)');
        SET v_caseFieldName = CONCAT('GetLocalizedValue(NationalCaseValue.CaseFieldNameLocalizations, ''', p_culture, ''', NationalCaseValue.CaseFieldName)');
        SET v_caseSlot      = CONCAT('GetLocalizedValue(NationalCaseValue.CaseSlotLocalizations, ''', p_culture, ''', NationalCaseValue.CaseSlot)');
    END IF;

    SET v_attrSql  = BuildAttributeQuery('NationalCaseValue.Attributes', p_attributes);
    SET v_pivotSql = CONCAT(
        'CREATE TEMPORARY TABLE NationalCaseChangeValuePivot AS SELECT',
        ' NationalCaseChange.TenantId,',
        ' NationalCaseChange.Id AS CaseChangeId,',
        ' NationalCaseChange.Created AS CaseChangeCreated,',
        ' NationalCaseChange.Reason,',
        ' NationalCaseChange.ValidationCaseName,',
        ' NationalCaseChange.CancellationType,',
        ' NationalCaseChange.CancellationId,',
        ' NationalCaseChange.CancellationDate,',
        ' NULL AS EmployeeId,',
        ' NationalCaseChange.UserId,',
        ' `User`.Identifier AS UserIdentifier,',
        ' NationalCaseChange.DivisionId,',
        ' NationalCaseValue.Id,',
        ' NationalCaseValue.Created,',
        ' NationalCaseValue.Updated,',
        ' NationalCaseValue.Status,',
        ' ', v_caseName,      ' AS CaseName,',
        ' ', v_caseFieldName, ' AS CaseFieldName,',
        ' ', v_caseSlot,      ' AS CaseSlot,',
        ' NationalCaseValue.CaseRelation,',
        ' NationalCaseValue.ValueType,',
        ' NationalCaseValue.Value,',
        ' NationalCaseValue.NumericValue,',
        ' NationalCaseValue.Culture,',
        ' NationalCaseValue.Start,',
        ' NationalCaseValue.End,',
        ' NationalCaseValue.Forecast,',
        ' NationalCaseValue.Tags,',
        ' NationalCaseValue.Attributes,',
        ' (SELECT COUNT(*) FROM NationalCaseDocument WHERE CaseValueId = NationalCaseValue.Id) AS Documents',
        v_attrSql,
        ' FROM NationalCaseValue',
        ' LEFT JOIN NationalCaseValueChange ON NationalCaseValue.Id = NationalCaseValueChange.CaseValueId',
        ' LEFT JOIN NationalCaseChange ON NationalCaseValueChange.CaseChangeId = NationalCaseChange.Id',
        ' LEFT JOIN `User` ON `User`.Id = NationalCaseChange.UserId',
        ' WHERE NationalCaseChange.TenantId = ', CAST(p_parentId AS CHAR));

    DROP TEMPORARY TABLE IF EXISTS NationalCaseChangeValuePivot;

    SET @_pivot_sql = v_pivotSql;
    PREPARE _pivot_stmt FROM @_pivot_sql;
    EXECUTE _pivot_stmt;
    DEALLOCATE PREPARE _pivot_stmt;

    SET @_query_sql = p_sql;
    PREPARE _query_stmt FROM @_query_sql;
    EXECUTE _query_stmt;
    DEALLOCATE PREPARE _query_stmt;

    DROP TEMPORARY TABLE IF EXISTS NationalCaseChangeValuePivot;
END$$

DELIMITER ;
