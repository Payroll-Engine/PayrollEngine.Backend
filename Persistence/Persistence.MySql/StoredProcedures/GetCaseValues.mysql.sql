-- =============================================================================
-- PayrollEngine MySQL -- CaseValue Pivot Stored Procedures (Phase 4)
-- Ports all 8 T-SQL CaseValue pivot SPs to MySQL 8.0+
--
-- Key translations (Blocker 3 + Blocker 4 patterns, validated in PoC):
--
--   ##GlobalTempTable        -> CREATE TEMPORARY TABLE (session-scoped)
--   sp_executesql @pivotSql  -> PREPARE s FROM @pivotSql; EXECUTE s; DEALLOCATE PREPARE s;
--   BEGIN TRY / END CATCH    -> DECLARE EXIT HANDLER FOR SQLEXCEPTION
--   IIF(@culture IS NULL,..) -> IF(p_culture IS NULL, ...)
--   [dbo].[User].[Identifier]-> `User`.Identifier  (backtick for reserved word)
--   CONVERT(VARCHAR(10), @id)-> CAST(p_parentId AS CHAR)
--   SELECT * INTO ##Pivot ... -> CREATE TEMPORARY TABLE PivotTable AS SELECT ...
--
-- IMPORTANT: MySQL PREPARE statement source (@var) requires a user variable (@var),
-- not a local variable (DECLARE v_var). The pivot SQL is built in a local variable
-- and then assigned to a user variable before PREPARE.
--
-- SPs in this file (8):
--   GetGlobalCaseValues
--   GetNationalCaseValues
--   GetCompanyCaseValues
--   GetEmployeeCaseValues
--   GetGlobalCaseChangeValues
--   GetNationalCaseChangeValues
--   GetCompanyCaseChangeValues
--   GetEmployeeCaseChangeValues
-- =============================================================================

USE PayrollEngine;

DELIMITER $$

-- =============================================================================
-- GetGlobalCaseValues
-- T-SQL: SELECT * INTO ##GlobalCaseValuePivot FROM (SELECT GlobalCaseValue.* ...) AS GCVA
-- MySQL: CREATE TEMPORARY TABLE + PREPARE/EXECUTE
-- =============================================================================
DROP PROCEDURE IF EXISTS GetGlobalCaseValues$$
CREATE PROCEDURE GetGlobalCaseValues(
    IN p_parentId   INT,
    IN p_sql        LONGTEXT,
    IN p_attributes LONGTEXT
)
BEGIN
    DECLARE v_attrSql   LONGTEXT;
    DECLARE v_pivotSql  LONGTEXT;

    DECLARE EXIT HANDLER FOR SQLEXCEPTION
    BEGIN
        DROP TEMPORARY TABLE IF EXISTS GlobalCaseValuePivot;
        RESIGNAL;
    END;

    -- Build attribute fragment (may be empty string)
    SET v_attrSql = BuildAttributeQuery('GlobalCaseValue.Attributes', p_attributes);

    -- Build pivot SQL
    SET v_pivotSql = CONCAT(
        'CREATE TEMPORARY TABLE GlobalCaseValuePivot AS SELECT GlobalCaseValue.*',
        v_attrSql,
        ' FROM GlobalCaseValue WHERE GlobalCaseValue.TenantId = ',
        CAST(p_parentId AS CHAR));

    -- Cleanup before
    DROP TEMPORARY TABLE IF EXISTS GlobalCaseValuePivot;

    -- Build pivot table via PREPARE/EXECUTE
    -- PREPARE requires a user variable (@), not a local variable
    SET @_pivot_sql = v_pivotSql;
    PREPARE _pivot_stmt FROM @_pivot_sql;
    EXECUTE _pivot_stmt;
    DEALLOCATE PREPARE _pivot_stmt;

    -- Execute caller query against pivot table
    SET @_query_sql = p_sql;
    PREPARE _query_stmt FROM @_query_sql;
    EXECUTE _query_stmt;
    DEALLOCATE PREPARE _query_stmt;

    -- Cleanup after
    DROP TEMPORARY TABLE IF EXISTS GlobalCaseValuePivot;
END$$

-- =============================================================================
-- GetNationalCaseValues
-- =============================================================================
DROP PROCEDURE IF EXISTS GetNationalCaseValues$$
CREATE PROCEDURE GetNationalCaseValues(
    IN p_parentId   INT,
    IN p_sql        LONGTEXT,
    IN p_attributes LONGTEXT
)
BEGIN
    DECLARE v_attrSql   LONGTEXT;
    DECLARE v_pivotSql  LONGTEXT;

    DECLARE EXIT HANDLER FOR SQLEXCEPTION
    BEGIN
        DROP TEMPORARY TABLE IF EXISTS NationalCaseValuePivot;
        RESIGNAL;
    END;

    SET v_attrSql = BuildAttributeQuery('NationalCaseValue.Attributes', p_attributes);

    SET v_pivotSql = CONCAT(
        'CREATE TEMPORARY TABLE NationalCaseValuePivot AS SELECT NationalCaseValue.*',
        v_attrSql,
        ' FROM NationalCaseValue WHERE NationalCaseValue.TenantId = ',
        CAST(p_parentId AS CHAR));

    DROP TEMPORARY TABLE IF EXISTS NationalCaseValuePivot;

    SET @_pivot_sql = v_pivotSql;
    PREPARE _pivot_stmt FROM @_pivot_sql;
    EXECUTE _pivot_stmt;
    DEALLOCATE PREPARE _pivot_stmt;

    SET @_query_sql = p_sql;
    PREPARE _query_stmt FROM @_query_sql;
    EXECUTE _query_stmt;
    DEALLOCATE PREPARE _query_stmt;

    DROP TEMPORARY TABLE IF EXISTS NationalCaseValuePivot;
END$$

-- =============================================================================
-- GetCompanyCaseValues
-- =============================================================================
DROP PROCEDURE IF EXISTS GetCompanyCaseValues$$
CREATE PROCEDURE GetCompanyCaseValues(
    IN p_parentId   INT,
    IN p_sql        LONGTEXT,
    IN p_attributes LONGTEXT
)
BEGIN
    DECLARE v_attrSql   LONGTEXT;
    DECLARE v_pivotSql  LONGTEXT;

    DECLARE EXIT HANDLER FOR SQLEXCEPTION
    BEGIN
        DROP TEMPORARY TABLE IF EXISTS CompanyCaseValuePivot;
        RESIGNAL;
    END;

    SET v_attrSql = BuildAttributeQuery('CompanyCaseValue.Attributes', p_attributes);

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

-- =============================================================================
-- GetEmployeeCaseValues
-- T-SQL: WHERE EmployeeCaseValue.EmployeeId = @parentId  (not TenantId)
-- =============================================================================
DROP PROCEDURE IF EXISTS GetEmployeeCaseValues$$
CREATE PROCEDURE GetEmployeeCaseValues(
    IN p_parentId   INT,
    IN p_sql        LONGTEXT,
    IN p_attributes LONGTEXT
)
BEGIN
    DECLARE v_attrSql   LONGTEXT;
    DECLARE v_pivotSql  LONGTEXT;

    DECLARE EXIT HANDLER FOR SQLEXCEPTION
    BEGIN
        DROP TEMPORARY TABLE IF EXISTS EmployeeCaseValuePivot;
        RESIGNAL;
    END;

    SET v_attrSql = BuildAttributeQuery('EmployeeCaseValue.Attributes', p_attributes);

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

-- =============================================================================
-- GetGlobalCaseChangeValues
-- T-SQL: IIF(@culture IS NULL, col, dbo.GetLocalizedValue(...))
-- MySQL: IF(p_culture IS NULL, col, GetLocalizedValue(...))
-- Note: `User` backtick-quoted (reserved word)
-- =============================================================================
DROP PROCEDURE IF EXISTS GetGlobalCaseChangeValues$$
CREATE PROCEDURE GetGlobalCaseChangeValues(
    IN p_parentId   INT,
    IN p_sql        LONGTEXT,
    IN p_attributes LONGTEXT,
    IN p_culture    VARCHAR(128)
)
BEGIN
    DECLARE v_attrSql   LONGTEXT;
    DECLARE v_pivotSql  LONGTEXT;
    DECLARE v_caseName      LONGTEXT;
    DECLARE v_caseFieldName LONGTEXT;
    DECLARE v_caseSlot      LONGTEXT;

    DECLARE EXIT HANDLER FOR SQLEXCEPTION
    BEGIN
        DROP TEMPORARY TABLE IF EXISTS GlobalCaseChangeValuePivot;
        RESIGNAL;
    END;

    -- Localization expressions: plain column or GetLocalizedValue call
    IF p_culture IS NULL THEN
        SET v_caseName      = 'GlobalCaseValue.CaseName';
        SET v_caseFieldName = 'GlobalCaseValue.CaseFieldName';
        SET v_caseSlot      = 'GlobalCaseValue.CaseSlot';
    ELSE
        SET v_caseName      = CONCAT('GetLocalizedValue(GlobalCaseValue.CaseNameLocalizations, ''', p_culture, ''', GlobalCaseValue.CaseName)');
        SET v_caseFieldName = CONCAT('GetLocalizedValue(GlobalCaseValue.CaseFieldNameLocalizations, ''', p_culture, ''', GlobalCaseValue.CaseFieldName)');
        SET v_caseSlot      = CONCAT('GetLocalizedValue(GlobalCaseValue.CaseSlotLocalizations, ''', p_culture, ''', GlobalCaseValue.CaseSlot)');
    END IF;

    SET v_attrSql = BuildAttributeQuery('GlobalCaseValue.Attributes', p_attributes);

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

-- =============================================================================
-- GetNationalCaseChangeValues
-- =============================================================================
DROP PROCEDURE IF EXISTS GetNationalCaseChangeValues$$
CREATE PROCEDURE GetNationalCaseChangeValues(
    IN p_parentId   INT,
    IN p_sql        LONGTEXT,
    IN p_attributes LONGTEXT,
    IN p_culture    VARCHAR(128)
)
BEGIN
    DECLARE v_attrSql   LONGTEXT;
    DECLARE v_pivotSql  LONGTEXT;
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

    SET v_attrSql = BuildAttributeQuery('NationalCaseValue.Attributes', p_attributes);

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

-- =============================================================================
-- GetCompanyCaseChangeValues
-- =============================================================================
DROP PROCEDURE IF EXISTS GetCompanyCaseChangeValues$$
CREATE PROCEDURE GetCompanyCaseChangeValues(
    IN p_parentId   INT,
    IN p_sql        LONGTEXT,
    IN p_attributes LONGTEXT,
    IN p_culture    VARCHAR(128)
)
BEGIN
    DECLARE v_attrSql   LONGTEXT;
    DECLARE v_pivotSql  LONGTEXT;
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

    SET v_attrSql = BuildAttributeQuery('CompanyCaseValue.Attributes', p_attributes);

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

-- =============================================================================
-- GetEmployeeCaseChangeValues
-- T-SQL: WHERE EmployeeCaseChange.EmployeeId = @parentId  (not TenantId)
-- Extra join: Employee table for TenantId
-- =============================================================================
DROP PROCEDURE IF EXISTS GetEmployeeCaseChangeValues$$
CREATE PROCEDURE GetEmployeeCaseChangeValues(
    IN p_parentId   INT,
    IN p_sql        LONGTEXT,
    IN p_attributes LONGTEXT,
    IN p_culture    VARCHAR(128)
)
BEGIN
    DECLARE v_attrSql   LONGTEXT;
    DECLARE v_pivotSql  LONGTEXT;
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

    SET v_attrSql = BuildAttributeQuery('EmployeeCaseValue.Attributes', p_attributes);

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

SELECT 'CaseValue pivot stored procedures created (8/8).' AS Result;
