-- =============================================================================
-- GetEmployeeCaseChangeValues
-- Filter is EmployeeId (not TenantId); extra JOIN to Employee for TenantId
-- =============================================================================

USE PayrollEngine;

DELIMITER $$

DROP PROCEDURE IF EXISTS GetEmployeeCaseChangeValues$$
CREATE PROCEDURE GetEmployeeCaseChangeValues(
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
        DROP TEMPORARY TABLE IF EXISTS EmployeeCaseChangeValuePivot;
        RESIGNAL;
    END;

    IF p_culture IS NULL THEN
        SET v_caseName      = 'EmployeeCaseValue.CaseName';
        SET v_caseFieldName = 'EmployeeCaseValue.CaseFieldName';
        SET v_caseSlot      = 'EmployeeCaseValue.CaseSlot';
    ELSE
        SET v_caseName      = CONCAT('GetLocalizedValue(EmployeeCaseValue.CaseNameLocalizations, ''', p_culture, ''', EmployeeCaseValue.CaseName)');
        SET v_caseFieldName = CONCAT('GetLocalizedValue(EmployeeCaseValue.CaseFieldNameLocalizations, ''', p_culture, ''', EmployeeCaseValue.CaseFieldName)');
        SET v_caseSlot      = CONCAT('GetLocalizedValue(EmployeeCaseValue.CaseSlotLocalizations, ''', p_culture, ''', EmployeeCaseValue.CaseSlot)');
    END IF;

    SET v_attrSql  = BuildAttributeQuery('EmployeeCaseValue.Attributes', p_attributes);
    SET v_pivotSql = CONCAT(
        'CREATE TEMPORARY TABLE EmployeeCaseChangeValuePivot AS SELECT',
        ' Employee.TenantId,',
        ' EmployeeCaseChange.Id AS CaseChangeId,',
        ' EmployeeCaseChange.Created AS CaseChangeCreated,',
        ' EmployeeCaseChange.Reason,',
        ' EmployeeCaseChange.ValidationCaseName,',
        ' EmployeeCaseChange.CancellationType,',
        ' EmployeeCaseChange.CancellationId,',
        ' EmployeeCaseChange.CancellationDate,',
        ' EmployeeCaseChange.EmployeeId,',
        ' EmployeeCaseChange.UserId,',
        ' `User`.Identifier AS UserIdentifier,',
        ' EmployeeCaseChange.DivisionId,',
        ' EmployeeCaseValue.Id,',
        ' EmployeeCaseValue.Created,',
        ' EmployeeCaseValue.Updated,',
        ' EmployeeCaseValue.Status,',
        ' ', v_caseName,      ' AS CaseName,',
        ' ', v_caseFieldName, ' AS CaseFieldName,',
        ' ', v_caseSlot,      ' AS CaseSlot,',
        ' EmployeeCaseValue.CaseRelation,',
        ' EmployeeCaseValue.ValueType,',
        ' EmployeeCaseValue.Value,',
        ' EmployeeCaseValue.NumericValue,',
        ' EmployeeCaseValue.Culture,',
        ' EmployeeCaseValue.Start,',
        ' EmployeeCaseValue.End,',
        ' EmployeeCaseValue.Forecast,',
        ' EmployeeCaseValue.Tags,',
        ' EmployeeCaseValue.Attributes,',
        ' (SELECT COUNT(*) FROM EmployeeCaseDocument WHERE CaseValueId = EmployeeCaseValue.Id) AS Documents',
        v_attrSql,
        ' FROM EmployeeCaseValue',
        ' LEFT JOIN EmployeeCaseValueChange ON EmployeeCaseValue.Id = EmployeeCaseValueChange.CaseValueId',
        ' LEFT JOIN EmployeeCaseChange ON EmployeeCaseValueChange.CaseChangeId = EmployeeCaseChange.Id',
        ' LEFT JOIN `User` ON `User`.Id = EmployeeCaseChange.UserId',
        ' LEFT JOIN Employee ON Employee.Id = EmployeeCaseChange.EmployeeId',
        ' WHERE EmployeeCaseChange.EmployeeId = ', CAST(p_parentId AS CHAR));

    DROP TEMPORARY TABLE IF EXISTS EmployeeCaseChangeValuePivot;

    SET @_pivot_sql = v_pivotSql;
    PREPARE _pivot_stmt FROM @_pivot_sql;
    EXECUTE _pivot_stmt;
    DEALLOCATE PREPARE _pivot_stmt;

    SET @_query_sql = p_sql;
    PREPARE _query_stmt FROM @_query_sql;
    EXECUTE _query_stmt;
    DEALLOCATE PREPARE _query_stmt;

    DROP TEMPORARY TABLE IF EXISTS EmployeeCaseChangeValuePivot;
END$$

DELIMITER ;
