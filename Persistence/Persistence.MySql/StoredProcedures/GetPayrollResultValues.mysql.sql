-- =============================================================================
-- GetPayrollResultValues
-- 5-way UNION ALL pivot of all result types + PREPARE/EXECUTE
-- FORMAT(value, 'N2') -> MySQL: FORMAT(value, 2)
-- ##PayrollResultPivot -> TEMPORARY TABLE PayrollResultPivot
-- =============================================================================

USE PayrollEngine;

DELIMITER $$

DROP PROCEDURE IF EXISTS GetPayrollResultValues$$
CREATE PROCEDURE GetPayrollResultValues(
    IN p_parentId   INT,
    IN p_sql        LONGTEXT,
    IN p_employeeId INT,
    IN p_divisionId INT,
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

    SET v_attrNames = GetAttributeNames(p_attributes);

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
        ' FROM (',
        -- CollectorResult (kind=10)
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
        -- CollectorCustomResult (kind=11)
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
        -- WageTypeResult (kind=20)
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
        -- WageTypeCustomResult (kind=21)
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
        -- PayrunResult (kind=30)
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
        ') PayrollValue',
        ' LEFT JOIN PayrollResult ON PayrollResult.Id = PayrollValue.PayrollResultId',
        ' LEFT JOIN PayrunJob ON PayrollResult.PayrunJobId = PayrunJob.Id',
        ' LEFT JOIN Payrun ON PayrunJob.PayrunId = Payrun.Id',
        ' LEFT JOIN Employee ON PayrollResult.EmployeeId = Employee.Id',
        ' LEFT JOIN Payroll ON PayrollResult.PayrollId = Payroll.Id',
        ' LEFT JOIN Division ON Payroll.DivisionId = Division.Id',
        ' LEFT JOIN `User` ON PayrunJob.CreatedUserId = `User`.Id',
        IF(p_employeeId IS NULL AND p_divisionId IS NULL, '',
            CONCAT(' WHERE ',
                IF(p_employeeId IS NULL, '', CONCAT('Employee.Id = ', CAST(p_employeeId AS CHAR))),
                IF(p_employeeId IS NOT NULL AND p_divisionId IS NOT NULL, ' AND ', ''),
                IF(p_divisionId IS NULL, '', CONCAT('Division.Id = ', CAST(p_divisionId AS CHAR)))
            )
        ));

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

DELIMITER ;
