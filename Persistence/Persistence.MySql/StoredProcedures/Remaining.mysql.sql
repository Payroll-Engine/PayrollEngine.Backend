-- =============================================================================
-- PayrollEngine MySQL -- Remaining Stored Procedures (Phases 5, 6, 7)
--
-- Phase 5: GetPayrollResultValues   (1 SP, ##Temp + PREPARE/EXECUTE + FORMAT)
-- Phase 7: Delete*, GetLookupRangeValue, GetEmployeeCaseValuesByTenant,
--           UpdateStatistics, UpdateStatisticsTargeted (9 SPs)
--
-- Key translations:
--   FORMAT(value, 'N2') -> FORMAT(value, 2)   (MySQL FORMAT has different signature)
--   LTRIM()             -> LTRIM()             (same)
--   ##PayrollResultPivot-> TEMPORARY TABLE PayrollResultPivot
--   sp_executesql       -> PREPARE/EXECUTE
--   IIF(@x IS NULL,...) -> IF(p_x IS NULL,...)
--   DELETE t FROM t INNER JOIN -> DELETE t USING t INNER JOIN  (MySQL multi-table DELETE)
--   SELECT TOP 1 *      -> SELECT * ... LIMIT 1
--   RETURN NULL (in SP) -> no equivalent; SP just returns empty result set
--   DELETE [ReportLog] WHERE -> DELETE FROM ReportLog WHERE  (T-SQL alias quirk)
-- =============================================================================

USE PayrollEngine;

DELIMITER $$

-- =============================================================================
-- PHASE 5: GetPayrollResultValues
-- Most complex SP: 5-way UNION ALL pivot of all result types + PREPARE/EXECUTE
-- FORMAT(value, 'N2') -> MySQL: FORMAT(value, 2)  (format_mask differs)
-- LTRIM([PayrunResult].[Value]) -> LTRIM(PayrunResult.Value)
-- GetAttributeNames() / BuildAttributeQuery() -> MySQL functions from Phase 2
-- =============================================================================
DROP PROCEDURE IF EXISTS GetPayrollResultValues$$
CREATE PROCEDURE GetPayrollResultValues(
    IN p_parentId   INT,
    IN p_sql        LONGTEXT,
    IN p_employeeId INT,
    IN p_attributes LONGTEXT
)
BEGIN
    DECLARE v_attrNames LONGTEXT;
    DECLARE v_pivotSql  LONGTEXT;

    DECLARE EXIT HANDLER FOR SQLEXCEPTION
    BEGIN
        DROP TEMPORARY TABLE IF EXISTS PayrollResultPivot;
        RESIGNAL;
    END;

    -- Attribute names fragment for outer SELECT (empty string if none)
    SET v_attrNames = GetAttributeNames(p_attributes);

    -- Build the large pivot SQL
    -- Part 1: outer SELECT + payroll context columns
    SET v_pivotSql = CONCAT(
        'CREATE TEMPORARY TABLE PayrollResultPivot AS SELECT',
        ' PayrollResult.TenantId,',
        ' PayrollResult.Id AS PayrollResultId,',
        ' PayrollResult.Created,',
        ' PayrollValue.ResultKind,',
        ' PayrollValue.ResultId,',
        ' PayrollValue.ResultParentId,',
        ' PayrollValue.ResultNumber,',
        ' PayrollValue.KindName,',
        ' PayrollValue.ResultCreated,',
        ' PayrollValue.ResultStart,',
        ' PayrollValue.ResultEnd,',
        ' PayrollValue.ResultType,',
        ' PayrollValue.ResultValue,',
        ' PayrollValue.ResultNumericValue,',
        ' PayrollValue.ResultCulture,',
        ' PayrollValue.ResultTags,',
        ' PayrollValue.Attributes,',
        ' PayrunJob.Id AS JobId,',
        ' PayrunJob.Name AS JobName,',
        ' PayrunJob.CreatedReason AS JobReason,',
        ' PayrunJob.Forecast,',
        ' PayrunJob.JobStatus,',
        ' PayrunJob.CycleName,',
        ' PayrunJob.PeriodName,',
        ' PayrunJob.PeriodStart,',
        ' PayrunJob.PeriodEnd,',
        ' Payrun.Id AS PayrunId,',
        ' Payrun.Name AS PayrunName,',
        ' Payroll.Id AS PayrollId,',
        ' Payroll.Name AS PayrollName,',
        ' Division.Id AS DivisionId,',
        ' Division.Name AS DivisionName,',
        ' Division.Culture,',
        ' `User`.Id AS UserId,',
        ' `User`.Identifier AS UserIdentifier,',
        ' Employee.Id AS EmployeeId,',
        ' Employee.Identifier AS EmployeeIdentifier',
        v_attrNames,

        -- Part 2: UNION ALL of 5 result types
        ' FROM (',

        -- 2a: CollectorResult (kind=10)
        ' SELECT 10 AS ResultKind,',
        ' CollectorResult.PayrollResultId,',
        ' CollectorResult.Id AS ResultId,',
        ' CollectorResult.PayrollResultId AS ResultParentId,',
        ' CollectorResult.CollectorName AS KindName,',
        ' 0 AS ResultNumber,',
        ' CollectorResult.Created AS ResultCreated,',
        ' CollectorResult.Start AS ResultStart,',
        ' CollectorResult.End AS ResultEnd,',
        ' CollectorResult.Tags AS ResultTags,',
        ' CollectorResult.Attributes,',
        ' CollectorResult.ValueType AS ResultType,',
        ' FORMAT(CollectorResult.Value, 2) AS ResultValue,',
        ' CollectorResult.Value AS ResultNumericValue,',
        ' CollectorResult.Culture AS ResultCulture',
        BuildAttributeQuery('CollectorResult.Attributes', p_attributes),
        ' FROM CollectorResult',

        ' UNION ALL',

        -- 2b: CollectorCustomResult (kind=11)
        ' SELECT 11 AS ResultKind,',
        ' CollectorResult.PayrollResultId,',
        ' CollectorCustomResult.Id AS ResultId,',
        ' CollectorResult.Id AS ResultParentId,',
        ' CollectorCustomResult.Source AS KindName,',
        ' 0 AS ResultNumber,',
        ' CollectorCustomResult.Created AS ResultCreated,',
        ' CollectorCustomResult.Start AS ResultStart,',
        ' CollectorCustomResult.End AS ResultEnd,',
        ' CollectorCustomResult.Tags AS ResultTags,',
        ' CollectorCustomResult.Attributes,',
        ' CollectorCustomResult.ValueType AS ResultType,',
        ' FORMAT(CollectorCustomResult.Value, 2) AS ResultValue,',
        ' CollectorCustomResult.Value AS ResultNumericValue,',
        ' CollectorCustomResult.Culture AS ResultCulture',
        BuildAttributeQuery('CollectorCustomResult.Attributes', p_attributes),
        ' FROM CollectorResult',
        ' INNER JOIN CollectorCustomResult ON CollectorResult.Id = CollectorCustomResult.CollectorResultId',

        ' UNION ALL',

        -- 2c: WageTypeResult (kind=20)
        ' SELECT 20 AS ResultKind,',
        ' WageTypeResult.PayrollResultId,',
        ' WageTypeResult.Id AS ResultId,',
        ' WageTypeResult.PayrollResultId AS ResultParentId,',
        ' WageTypeResult.WageTypeName AS KindName,',
        ' WageTypeResult.WageTypeNumber AS ResultNumber,',
        ' WageTypeResult.Created AS ResultCreated,',
        ' WageTypeResult.Start AS ResultStart,',
        ' WageTypeResult.End AS ResultEnd,',
        ' WageTypeResult.Tags AS ResultTags,',
        ' WageTypeResult.Attributes,',
        ' WageTypeResult.ValueType AS ResultType,',
        ' FORMAT(WageTypeResult.Value, 2) AS ResultValue,',
        ' WageTypeResult.Value AS ResultNumericValue,',
        ' WageTypeResult.Culture AS ResultCulture',
        BuildAttributeQuery('WageTypeResult.Attributes', p_attributes),
        ' FROM WageTypeResult',

        ' UNION ALL',

        -- 2d: WageTypeCustomResult (kind=21)
        ' SELECT 21 AS ResultKind,',
        ' WageTypeResult.PayrollResultId,',
        ' WageTypeCustomResult.Id AS ResultId,',
        ' WageTypeResult.Id AS ResultParentId,',
        ' WageTypeCustomResult.Source AS KindName,',
        ' 0 AS ResultNumber,',
        ' WageTypeCustomResult.Created AS ResultCreated,',
        ' WageTypeCustomResult.Start AS ResultStart,',
        ' WageTypeCustomResult.End AS ResultEnd,',
        ' WageTypeCustomResult.Tags AS ResultTags,',
        ' WageTypeCustomResult.Attributes,',
        ' WageTypeCustomResult.ValueType AS ResultType,',
        ' FORMAT(WageTypeCustomResult.Value, 2) AS ResultValue,',
        ' WageTypeCustomResult.Value AS ResultNumericValue,',
        ' WageTypeCustomResult.Culture AS ResultCulture',
        BuildAttributeQuery('WageTypeCustomResult.Attributes', p_attributes),
        ' FROM WageTypeResult',
        ' INNER JOIN WageTypeCustomResult ON WageTypeResult.Id = WageTypeCustomResult.WageTypeResultId',

        ' UNION ALL',

        -- 2e: PayrunResult (kind=30)
        -- LTRIM identical in MySQL; BuildAttributeQuery(NULL,...) -> NULL placeholders
        ' SELECT 30 AS ResultKind,',
        ' PayrunResult.PayrollResultId,',
        ' PayrunResult.Id AS ResultId,',
        ' PayrunResult.PayrollResultId AS ResultParentId,',
        ' PayrunResult.Name AS KindName,',
        ' 0 AS ResultNumber,',
        ' PayrunResult.Created AS ResultCreated,',
        ' PayrunResult.Start AS ResultStart,',
        ' PayrunResult.End AS ResultEnd,',
        ' PayrunResult.Tags AS ResultTags,',
        ' PayrunResult.Attributes,',
        ' PayrunResult.ValueType AS ResultType,',
        ' LTRIM(PayrunResult.Value) AS ResultValue,',
        ' PayrunResult.NumericValue AS ResultNumericValue,',
        ' PayrunResult.Culture AS ResultCulture',
        BuildAttributeQuery(NULL, p_attributes),
        ' FROM PayrunResult',

        -- Part 3: outer JOINs
        ') PayrollValue',
        ' LEFT JOIN PayrollResult ON PayrollResult.Id = PayrollValue.PayrollResultId',
        ' LEFT JOIN PayrunJob ON PayrollResult.PayrunJobId = PayrunJob.Id',
        ' LEFT JOIN Payrun ON PayrunJob.PayrunId = Payrun.Id',
        ' LEFT JOIN Employee ON PayrollResult.EmployeeId = Employee.Id',
        ' LEFT JOIN Payroll ON PayrollResult.PayrollId = Payroll.Id',
        ' LEFT JOIN Division ON Payroll.DivisionId = Division.Id',
        ' LEFT JOIN `User` ON PayrunJob.CreatedUserId = `User`.Id',
        IF(p_employeeId IS NULL, '', CONCAT(' WHERE Employee.Id = ', CAST(p_employeeId AS CHAR))));

    DROP TEMPORARY TABLE IF EXISTS PayrollResultPivot;

    SET @_pivot_sql = v_pivotSql;
    PREPARE _pivot_stmt FROM @_pivot_sql;
    EXECUTE _pivot_stmt;
    DEALLOCATE PREPARE _pivot_stmt;

    SET @_query_sql = p_sql;
    PREPARE _query_stmt FROM @_query_sql;
    EXECUTE _query_stmt;
    DEALLOCATE PREPARE _query_stmt;

    DROP TEMPORARY TABLE IF EXISTS PayrollResultPivot;
END$$

-- =============================================================================
-- PHASE 7: GetLookupRangeValue
-- T-SQL: SELECT TOP 1 * -> MySQL: SELECT * ... LIMIT 1
-- T-SQL: RETURN NULL (no result) -> MySQL SP: just returns empty set
-- =============================================================================
DROP PROCEDURE IF EXISTS GetLookupRangeValue$
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

    -- IF/ELSE instead of LEAVE (avoids label syntax issues)
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
END$

-- =============================================================================
-- PHASE 7: GetEmployeeCaseValuesByTenant
-- Direct JOIN query -- no pivot, no temp table needed.
-- OPENJSON(@fieldNames) -> JSON_TABLE(p_fieldNames, '$[*]' COLUMNS ...)
-- =============================================================================
DROP PROCEDURE IF EXISTS GetEmployeeCaseValuesByTenant$$
CREATE PROCEDURE GetEmployeeCaseValuesByTenant(
    IN p_tenantId       INT,
    IN p_valueDate      DATETIME(6),
    IN p_evaluationDate DATETIME(6),
    IN p_fieldNames     LONGTEXT,
    IN p_forecast       VARCHAR(128)
)
BEGIN
    SELECT
        ecv.Id, ecv.Status, ecv.Created, ecv.Updated,
        ecv.EmployeeId, ecv.DivisionId,
        ecv.CaseName, ecv.CaseNameLocalizations,
        ecv.CaseFieldName, ecv.CaseFieldNameLocalizations,
        ecv.CaseSlot, ecv.CaseSlotLocalizations,
        ecv.ValueType, ecv.Value, ecv.NumericValue, ecv.Culture,
        ecv.CaseRelation, ecv.CancellationDate, ecv.Start, ecv.End,
        ecv.Forecast, ecv.Tags, ecv.Attributes
    FROM EmployeeCaseValue ecv
    INNER JOIN Employee e ON e.Id = ecv.EmployeeId
    WHERE e.TenantId = p_tenantId
      AND e.Status = 0
      AND ecv.CancellationDate IS NULL
      AND (p_evaluationDate IS NULL OR ecv.Created <= p_evaluationDate)
      AND (p_valueDate IS NULL OR ecv.Start IS NULL OR ecv.Start <= p_valueDate)
      AND (p_valueDate IS NULL OR ecv.End   IS NULL OR ecv.End   >  p_valueDate)
      AND (
          (p_forecast IS NULL     AND ecv.Forecast IS NULL)
          OR (p_forecast IS NOT NULL AND (ecv.Forecast IS NULL OR ecv.Forecast = p_forecast))
      )
      AND (
          p_fieldNames IS NULL
          OR ecv.CaseFieldName IN (
              SELECT jt.val
              FROM JSON_TABLE(p_fieldNames, '$[*]' COLUMNS (val VARCHAR(128) PATH '$')) AS jt)
      )
    ORDER BY ecv.EmployeeId ASC, ecv.CaseFieldName ASC, ecv.Created DESC;
END$$

-- =============================================================================
-- PHASE 7: DeleteEmployee
-- T-SQL: DELETE t FROM t INNER JOIN ... -> MySQL: DELETE t USING/FROM t INNER JOIN
-- MySQL multi-table DELETE syntax: DELETE ecvc FROM EmployeeCaseValueChange ecvc INNER JOIN ...
-- =============================================================================
DROP PROCEDURE IF EXISTS DeleteEmployee$$
CREATE PROCEDURE DeleteEmployee(
    IN p_tenantId  INT,
    IN p_employeeId INT
)
BEGIN
    -- payroll results (via PayrollResult join)
    DELETE pr FROM PayrunResult pr
    INNER JOIN PayrollResult prl ON pr.PayrollResultId = prl.Id
    WHERE prl.TenantId = p_tenantId AND prl.EmployeeId = p_employeeId;

    DELETE wtcr FROM WageTypeCustomResult wtcr
    INNER JOIN WageTypeResult wtr ON wtcr.WageTypeResultId = wtr.Id
    INNER JOIN PayrollResult prl ON wtr.PayrollResultId = prl.Id
    WHERE prl.TenantId = p_tenantId AND prl.EmployeeId = p_employeeId;

    DELETE wtr FROM WageTypeResult wtr
    INNER JOIN PayrollResult prl ON wtr.PayrollResultId = prl.Id
    WHERE prl.TenantId = p_tenantId AND prl.EmployeeId = p_employeeId;

    DELETE ccr FROM CollectorCustomResult ccr
    INNER JOIN CollectorResult cr ON ccr.CollectorResultId = cr.Id
    INNER JOIN PayrollResult prl ON cr.PayrollResultId = prl.Id
    WHERE prl.TenantId = p_tenantId AND prl.EmployeeId = p_employeeId;

    DELETE cr FROM CollectorResult cr
    INNER JOIN PayrollResult prl ON cr.PayrollResultId = prl.Id
    WHERE prl.TenantId = p_tenantId AND prl.EmployeeId = p_employeeId;

    DELETE FROM PayrollResult
    WHERE TenantId = p_tenantId AND EmployeeId = p_employeeId;

    -- payrun job employee
    DELETE pje FROM PayrunJobEmployee pje
    INNER JOIN PayrunJob pj ON pje.PayrunJobId = pj.Id
    WHERE pj.TenantId = p_tenantId AND pje.EmployeeId = p_employeeId;

    -- employee case data
    DELETE ecvc FROM EmployeeCaseValueChange ecvc
    INNER JOIN EmployeeCaseChange ecc ON ecvc.CaseChangeId = ecc.Id
    INNER JOIN Employee e ON ecc.EmployeeId = e.Id
    WHERE e.TenantId = p_tenantId AND e.Id = p_employeeId;

    DELETE ecc FROM EmployeeCaseChange ecc
    INNER JOIN Employee e ON ecc.EmployeeId = e.Id
    WHERE e.TenantId = p_tenantId AND e.Id = p_employeeId;

    DELETE ecd FROM EmployeeCaseDocument ecd
    INNER JOIN EmployeeCaseValue ecv ON ecd.CaseValueId = ecv.Id
    INNER JOIN Employee e ON ecv.EmployeeId = e.Id
    WHERE e.TenantId = p_tenantId AND e.Id = p_employeeId;

    DELETE ecv FROM EmployeeCaseValue ecv
    INNER JOIN Employee e ON ecv.EmployeeId = e.Id
    WHERE e.TenantId = p_tenantId AND e.Id = p_employeeId;

    DELETE ed FROM EmployeeDivision ed
    INNER JOIN Employee e ON ed.EmployeeId = e.Id
    WHERE e.TenantId = p_tenantId AND e.Id = p_employeeId;

    DELETE FROM Employee WHERE TenantId = p_tenantId AND Id = p_employeeId;
END$$

-- =============================================================================
-- PHASE 7: DeleteLookup
-- =============================================================================
DROP PROCEDURE IF EXISTS DeleteLookup$$
CREATE PROCEDURE DeleteLookup(
    IN p_tenantId INT,
    IN p_lookupId INT
)
BEGIN
    DELETE lva FROM LookupValueAudit lva
    INNER JOIN LookupValue lv ON lva.LookupValueId = lv.Id
    INNER JOIN Lookup lk ON lv.LookupId = lk.Id
    INNER JOIN Regulation r ON lk.RegulationId = r.Id
    WHERE r.TenantId = p_tenantId AND lk.Id = p_lookupId;

    DELETE lv FROM LookupValue lv
    INNER JOIN Lookup lk ON lv.LookupId = lk.Id
    INNER JOIN Regulation r ON lk.RegulationId = r.Id
    WHERE r.TenantId = p_tenantId AND lk.Id = p_lookupId;

    DELETE la FROM LookupAudit la
    INNER JOIN Lookup lk ON la.LookupId = lk.Id
    INNER JOIN Regulation r ON lk.RegulationId = r.Id
    WHERE r.TenantId = p_tenantId AND lk.Id = p_lookupId;

    DELETE lk FROM Lookup lk
    INNER JOIN Regulation r ON lk.RegulationId = r.Id
    WHERE r.TenantId = p_tenantId AND lk.Id = p_lookupId;
END$$

-- =============================================================================
-- PHASE 7: DeletePayrunJob
-- =============================================================================
DROP PROCEDURE IF EXISTS DeletePayrunJob$$
CREATE PROCEDURE DeletePayrunJob(
    IN p_tenantId    INT,
    IN p_payrunJobId INT
)
BEGIN
    DELETE pr FROM PayrunResult pr
    INNER JOIN PayrollResult prl ON pr.PayrollResultId = prl.Id
    WHERE prl.TenantId = p_tenantId AND prl.PayrunJobId = p_payrunJobId;

    DELETE wtcr FROM WageTypeCustomResult wtcr
    INNER JOIN WageTypeResult wtr ON wtcr.WageTypeResultId = wtr.Id
    INNER JOIN PayrollResult prl ON wtr.PayrollResultId = prl.Id
    WHERE prl.TenantId = p_tenantId AND prl.PayrunJobId = p_payrunJobId;

    DELETE wtr FROM WageTypeResult wtr
    INNER JOIN PayrollResult prl ON wtr.PayrollResultId = prl.Id
    WHERE prl.TenantId = p_tenantId AND prl.PayrunJobId = p_payrunJobId;

    DELETE ccr FROM CollectorCustomResult ccr
    INNER JOIN CollectorResult cr ON ccr.CollectorResultId = cr.Id
    INNER JOIN PayrollResult prl ON cr.PayrollResultId = prl.Id
    WHERE prl.TenantId = p_tenantId AND prl.PayrunJobId = p_payrunJobId;

    DELETE cr FROM CollectorResult cr
    INNER JOIN PayrollResult prl ON cr.PayrollResultId = prl.Id
    WHERE prl.TenantId = p_tenantId AND prl.PayrunJobId = p_payrunJobId;

    DELETE FROM PayrollResult
    WHERE TenantId = p_tenantId AND PayrunJobId = p_payrunJobId;

    DELETE pje FROM PayrunJobEmployee pje
    INNER JOIN PayrunJob pj ON pje.PayrunJobId = pj.Id
    WHERE pj.TenantId = p_tenantId AND pje.PayrunJobId = p_payrunJobId;

    DELETE FROM PayrunJob
    WHERE TenantId = p_tenantId AND Id = p_payrunJobId;
END$$

-- =============================================================================
-- PHASE 7: DeleteTenant
-- T-SQL: DELETE [ReportLog] WHERE ... -> MySQL: DELETE FROM ReportLog WHERE ...
-- =============================================================================
DROP PROCEDURE IF EXISTS DeleteTenant$$
CREATE PROCEDURE DeleteTenant(
    IN p_tenantId INT
)
BEGIN
    -- payroll results
    DELETE pr FROM PayrunResult pr
    INNER JOIN PayrollResult prl ON pr.PayrollResultId = prl.Id
    WHERE prl.TenantId = p_tenantId;

    DELETE wtcr FROM WageTypeCustomResult wtcr
    INNER JOIN WageTypeResult wtr ON wtcr.WageTypeResultId = wtr.Id
    INNER JOIN PayrollResult prl ON wtr.PayrollResultId = prl.Id
    WHERE prl.TenantId = p_tenantId;

    DELETE wtr FROM WageTypeResult wtr
    INNER JOIN PayrollResult prl ON wtr.PayrollResultId = prl.Id
    WHERE prl.TenantId = p_tenantId;

    DELETE ccr FROM CollectorCustomResult ccr
    INNER JOIN CollectorResult cr ON ccr.CollectorResultId = cr.Id
    INNER JOIN PayrollResult prl ON cr.PayrollResultId = prl.Id
    WHERE prl.TenantId = p_tenantId;

    DELETE cr FROM CollectorResult cr
    INNER JOIN PayrollResult prl ON cr.PayrollResultId = prl.Id
    WHERE prl.TenantId = p_tenantId;

    DELETE FROM PayrollResult WHERE TenantId = p_tenantId;

    -- payrun
    DELETE pje FROM PayrunJobEmployee pje
    INNER JOIN PayrunJob pj ON pje.PayrunJobId = pj.Id
    WHERE pj.TenantId = p_tenantId;

    DELETE FROM PayrunJob WHERE TenantId = p_tenantId;

    DELETE pp FROM PayrunParameter pp
    INNER JOIN Payrun pay ON pp.PayrunId = pay.Id
    WHERE pay.TenantId = p_tenantId;

    DELETE FROM Payrun WHERE TenantId = p_tenantId;

    -- payroll
    DELETE pl FROM PayrollLayer pl
    INNER JOIN Payroll pay ON pl.PayrollId = pay.Id
    WHERE pay.TenantId = p_tenantId;

    DELETE FROM Payroll WHERE TenantId = p_tenantId;

    -- regulation shares
    DELETE FROM RegulationShare
    WHERE ProviderTenantId = p_tenantId OR ConsumerTenantId = p_tenantId;

    -- regulation objects (leaf to root)
    DELETE rta FROM ReportTemplateAudit rta
    INNER JOIN ReportTemplate rt ON rta.ReportTemplateId = rt.Id
    INNER JOIN Report rp ON rt.ReportId = rp.Id
    INNER JOIN Regulation r ON rp.RegulationId = r.Id
    WHERE r.TenantId = p_tenantId;

    DELETE rt FROM ReportTemplate rt
    INNER JOIN Report rp ON rt.ReportId = rp.Id
    INNER JOIN Regulation r ON rp.RegulationId = r.Id
    WHERE r.TenantId = p_tenantId;

    DELETE rpa FROM ReportParameterAudit rpa
    INNER JOIN ReportParameter rpar ON rpa.ReportParameterId = rpar.Id
    INNER JOIN Report rp ON rpar.ReportId = rp.Id
    INNER JOIN Regulation r ON rp.RegulationId = r.Id
    WHERE r.TenantId = p_tenantId;

    DELETE rpar FROM ReportParameter rpar
    INNER JOIN Report rp ON rpar.ReportId = rp.Id
    INNER JOIN Regulation r ON rp.RegulationId = r.Id
    WHERE r.TenantId = p_tenantId;

    DELETE ra FROM ReportAudit ra
    INNER JOIN Report rp ON ra.ReportId = rp.Id
    INNER JOIN Regulation r ON rp.RegulationId = r.Id
    WHERE r.TenantId = p_tenantId;

    DELETE rp FROM Report rp
    INNER JOIN Regulation r ON rp.RegulationId = r.Id
    WHERE r.TenantId = p_tenantId;

    DELETE sa FROM ScriptAudit sa
    INNER JOIN Script s ON sa.ScriptId = s.Id
    INNER JOIN Regulation r ON s.RegulationId = r.Id
    WHERE r.TenantId = p_tenantId;

    DELETE s FROM Script s
    INNER JOIN Regulation r ON s.RegulationId = r.Id
    WHERE r.TenantId = p_tenantId;

    DELETE lva FROM LookupValueAudit lva
    INNER JOIN LookupValue lv ON lva.LookupValueId = lv.Id
    INNER JOIN Lookup lk ON lv.LookupId = lk.Id
    INNER JOIN Regulation r ON lk.RegulationId = r.Id
    WHERE r.TenantId = p_tenantId;

    DELETE lv FROM LookupValue lv
    INNER JOIN Lookup lk ON lv.LookupId = lk.Id
    INNER JOIN Regulation r ON lk.RegulationId = r.Id
    WHERE r.TenantId = p_tenantId;

    DELETE la FROM LookupAudit la
    INNER JOIN Lookup lk ON la.LookupId = lk.Id
    INNER JOIN Regulation r ON lk.RegulationId = r.Id
    WHERE r.TenantId = p_tenantId;

    DELETE lk FROM Lookup lk
    INNER JOIN Regulation r ON lk.RegulationId = r.Id
    WHERE r.TenantId = p_tenantId;

    DELETE coa FROM CollectorAudit coa
    INNER JOIN Collector co ON coa.CollectorId = co.Id
    INNER JOIN Regulation r ON co.RegulationId = r.Id
    WHERE r.TenantId = p_tenantId;

    DELETE co FROM Collector co
    INNER JOIN Regulation r ON co.RegulationId = r.Id
    WHERE r.TenantId = p_tenantId;

    DELETE wta FROM WageTypeAudit wta
    INNER JOIN WageType wt ON wta.WageTypeId = wt.Id
    INNER JOIN Regulation r ON wt.RegulationId = r.Id
    WHERE r.TenantId = p_tenantId;

    DELETE wt FROM WageType wt
    INNER JOIN Regulation r ON wt.RegulationId = r.Id
    WHERE r.TenantId = p_tenantId;

    DELETE cra FROM CaseRelationAudit cra
    INNER JOIN CaseRelation cr ON cra.CaseRelationId = cr.Id
    INNER JOIN Regulation r ON cr.RegulationId = r.Id
    WHERE r.TenantId = p_tenantId;

    DELETE cr FROM CaseRelation cr
    INNER JOIN Regulation r ON cr.RegulationId = r.Id
    WHERE r.TenantId = p_tenantId;

    DELETE cfa FROM CaseFieldAudit cfa
    INNER JOIN CaseField cf ON cfa.CaseFieldId = cf.Id
    INNER JOIN `Case` c ON cf.CaseId = c.Id
    INNER JOIN Regulation r ON c.RegulationId = r.Id
    WHERE r.TenantId = p_tenantId;

    DELETE cf FROM CaseField cf
    INNER JOIN `Case` c ON cf.CaseId = c.Id
    INNER JOIN Regulation r ON c.RegulationId = r.Id
    WHERE r.TenantId = p_tenantId;

    DELETE ca FROM CaseAudit ca
    INNER JOIN `Case` c ON ca.CaseId = c.Id
    INNER JOIN Regulation r ON c.RegulationId = r.Id
    WHERE r.TenantId = p_tenantId;

    DELETE c FROM `Case` c
    INNER JOIN Regulation r ON c.RegulationId = r.Id
    WHERE r.TenantId = p_tenantId;

    DELETE FROM Regulation WHERE TenantId = p_tenantId;

    -- employee
    DELETE ecvc FROM EmployeeCaseValueChange ecvc
    INNER JOIN EmployeeCaseChange ecc ON ecvc.CaseChangeId = ecc.Id
    INNER JOIN Employee e ON ecc.EmployeeId = e.Id
    WHERE e.TenantId = p_tenantId;

    DELETE ecc FROM EmployeeCaseChange ecc
    INNER JOIN Employee e ON ecc.EmployeeId = e.Id
    WHERE e.TenantId = p_tenantId;

    DELETE ecd FROM EmployeeCaseDocument ecd
    INNER JOIN EmployeeCaseValue ecv ON ecd.CaseValueId = ecv.Id
    INNER JOIN Employee e ON ecv.EmployeeId = e.Id
    WHERE e.TenantId = p_tenantId;

    DELETE ecv FROM EmployeeCaseValue ecv
    INNER JOIN Employee e ON ecv.EmployeeId = e.Id
    WHERE e.TenantId = p_tenantId;

    DELETE ed FROM EmployeeDivision ed
    INNER JOIN Employee e ON ed.EmployeeId = e.Id
    WHERE e.TenantId = p_tenantId;

    DELETE FROM Employee WHERE TenantId = p_tenantId;

    -- company
    DELETE ccvc FROM CompanyCaseValueChange ccvc
    INNER JOIN CompanyCaseChange ccc ON ccvc.CaseChangeId = ccc.Id
    WHERE ccc.TenantId = p_tenantId;

    DELETE FROM CompanyCaseChange WHERE TenantId = p_tenantId;

    DELETE ccd FROM CompanyCaseDocument ccd
    INNER JOIN CompanyCaseValue ccv ON ccd.CaseValueId = ccv.Id
    WHERE ccv.TenantId = p_tenantId;

    DELETE FROM CompanyCaseValue WHERE TenantId = p_tenantId;

    -- national
    DELETE ncvc FROM NationalCaseValueChange ncvc
    INNER JOIN NationalCaseChange ncc ON ncvc.CaseChangeId = ncc.Id
    WHERE ncc.TenantId = p_tenantId;

    DELETE FROM NationalCaseChange WHERE TenantId = p_tenantId;

    DELETE ncd FROM NationalCaseDocument ncd
    INNER JOIN NationalCaseValue ncv ON ncd.CaseValueId = ncv.Id
    WHERE ncv.TenantId = p_tenantId;

    DELETE FROM NationalCaseValue WHERE TenantId = p_tenantId;

    -- global
    DELETE gcvc FROM GlobalCaseValueChange gcvc
    INNER JOIN GlobalCaseChange gcc ON gcvc.CaseChangeId = gcc.Id
    WHERE gcc.TenantId = p_tenantId;

    DELETE FROM GlobalCaseChange WHERE TenantId = p_tenantId;

    DELETE gcd FROM GlobalCaseDocument gcd
    INNER JOIN GlobalCaseValue gcv ON gcd.CaseValueId = gcv.Id
    WHERE gcv.TenantId = p_tenantId;

    DELETE FROM GlobalCaseValue WHERE TenantId = p_tenantId;

    -- webhook
    DELETE wm FROM WebhookMessage wm
    INNER JOIN Webhook wh ON wm.WebhookId = wh.Id
    WHERE wh.TenantId = p_tenantId;

    DELETE FROM Webhook WHERE TenantId = p_tenantId;

    DELETE FROM Task WHERE TenantId = p_tenantId;
    DELETE FROM Log WHERE TenantId = p_tenantId;
    DELETE FROM ReportLog WHERE TenantId = p_tenantId;
    DELETE FROM `User` WHERE TenantId = p_tenantId;
    DELETE FROM Division WHERE TenantId = p_tenantId;
    DELETE FROM Calendar WHERE TenantId = p_tenantId;
    DELETE FROM Tenant WHERE Id = p_tenantId;
END$$

-- =============================================================================
-- PHASE 7: UpdateStatistics + UpdateStatisticsTargeted
-- T-SQL: UPDATE STATISTICS ... WITH FULLSCAN -> MySQL: ANALYZE TABLE
-- =============================================================================
DROP PROCEDURE IF EXISTS UpdateStatistics$$
CREATE PROCEDURE UpdateStatistics()
BEGIN
    DECLARE v_table VARCHAR(128);
    DECLARE v_sql   LONGTEXT;
    DECLARE done    INT DEFAULT 0;
    DECLARE cur CURSOR FOR
        SELECT TABLE_NAME FROM information_schema.TABLES
        WHERE TABLE_SCHEMA = DATABASE() AND TABLE_TYPE = 'BASE TABLE';
    DECLARE CONTINUE HANDLER FOR NOT FOUND SET done = 1;

    OPEN cur;
    read_loop: LOOP
        FETCH cur INTO v_table;
        IF done THEN LEAVE read_loop; END IF;
        SET v_sql = CONCAT('ANALYZE TABLE `', v_table, '`');
        SET @_stmt = v_sql;
        PREPARE _s FROM @_stmt;
        EXECUTE _s;
        DEALLOCATE PREPARE _s;
    END LOOP;
    CLOSE cur;
END$$

DROP PROCEDURE IF EXISTS UpdateStatisticsTargeted$$
CREATE PROCEDURE UpdateStatisticsTargeted()
BEGIN
    ANALYZE TABLE LookupValue;
    ANALYZE TABLE PayrollResult;
    ANALYZE TABLE WageTypeResult;
    ANALYZE TABLE WageTypeCustomResult;
    ANALYZE TABLE CollectorResult;
    ANALYZE TABLE CollectorCustomResult;
    ANALYZE TABLE PayrunResult;
    ANALYZE TABLE GlobalCaseValue;
    ANALYZE TABLE NationalCaseValue;
    ANALYZE TABLE CompanyCaseValue;
    ANALYZE TABLE EmployeeCaseValue;
END$$

DELIMITER ;

SELECT 'Remaining stored procedures created (Phases 5+7: 8 SPs).' AS Result;
