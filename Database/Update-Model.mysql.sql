-- =============================================================================
-- Update-Model.mysql.sql
-- Migrates PayrollEngine MySQL schema from 0.9.5 to 0.9.6
--
-- Changes:
--   - Denormalized columns on WageTypeResult, PayrunTrace, PayrunResult,
--     CollectorResult, CollectorCustomResult, WageTypeCustomResult
--   - New SP: UpdateStatisticsTargeted
--   - New SP: GetEmployeeCaseValuesByTenant
--   - Recreate all GetDerived*, GetConsolidated*, Delete*, GetWageType*,
--     GetCollector* SPs (MySQL port of T-SQL equivalents)
--   - New covering indexes for employee/case value queries
-- =============================================================================

USE PayrollEngine;

DELIMITER $$

-- =============================================================================
-- VERSION CHECK
-- Expected: 0.9.5
-- =============================================================================
DROP PROCEDURE IF EXISTS _CheckVersion$$
CREATE PROCEDURE _CheckVersion()
BEGIN
    DECLARE v_major INT;
    DECLARE v_minor INT;
    DECLARE v_sub INT;

    SELECT MajorVersion, MinorVersion, SubVersion
    INTO v_major, v_minor, v_sub
    FROM `Version`
    ORDER BY MajorVersion DESC, MinorVersion DESC, SubVersion DESC
    LIMIT 1;

    IF v_major <> 0 OR v_minor <> 9 OR v_sub <> 5 THEN
        SIGNAL SQLSTATE '45000'
            SET MESSAGE_TEXT = CONCAT('Version mismatch: expected 0.9.5, found ',
                v_major, '.', v_minor, '.', v_sub);
    END IF;
END$$

CALL _CheckVersion()$$
DROP PROCEDURE _CheckVersion$$

DELIMITER ;

-- =============================================================================
-- SCHEMA CHANGES: Add denormalized columns (idempotent)
-- MySQL 8.0: ADD COLUMN IF NOT EXISTS
-- =============================================================================

-- WageTypeResult
ALTER TABLE WageTypeResult
    ADD COLUMN IF NOT EXISTS TenantId   INT          NOT NULL DEFAULT 0,
    ADD COLUMN IF NOT EXISTS EmployeeId INT          NOT NULL DEFAULT 0,
    ADD COLUMN IF NOT EXISTS DivisionId INT          NULL,
    ADD COLUMN IF NOT EXISTS PayrunJobId INT         NOT NULL DEFAULT 0,
    ADD COLUMN IF NOT EXISTS Forecast   VARCHAR(128) NULL,
    ADD COLUMN IF NOT EXISTS ParentJobId INT         NULL;

-- PayrunTrace
ALTER TABLE PayrunTrace
    ADD COLUMN IF NOT EXISTS TenantId   INT          NOT NULL DEFAULT 0,
    ADD COLUMN IF NOT EXISTS EmployeeId INT          NOT NULL DEFAULT 0,
    ADD COLUMN IF NOT EXISTS DivisionId INT          NULL,
    ADD COLUMN IF NOT EXISTS PayrunJobId INT         NOT NULL DEFAULT 0,
    ADD COLUMN IF NOT EXISTS Forecast   VARCHAR(128) NULL,
    ADD COLUMN IF NOT EXISTS ParentJobId INT         NULL;

-- PayrunResult
ALTER TABLE PayrunResult
    ADD COLUMN IF NOT EXISTS TenantId   INT          NOT NULL DEFAULT 0,
    ADD COLUMN IF NOT EXISTS EmployeeId INT          NOT NULL DEFAULT 0,
    ADD COLUMN IF NOT EXISTS DivisionId INT          NULL,
    ADD COLUMN IF NOT EXISTS PayrunJobId INT         NOT NULL DEFAULT 0,
    ADD COLUMN IF NOT EXISTS Forecast   VARCHAR(128) NULL,
    ADD COLUMN IF NOT EXISTS ParentJobId INT         NULL;

-- CollectorResult
ALTER TABLE CollectorResult
    ADD COLUMN IF NOT EXISTS TenantId   INT          NOT NULL DEFAULT 0,
    ADD COLUMN IF NOT EXISTS EmployeeId INT          NOT NULL DEFAULT 0,
    ADD COLUMN IF NOT EXISTS DivisionId INT          NULL,
    ADD COLUMN IF NOT EXISTS PayrunJobId INT         NOT NULL DEFAULT 0,
    ADD COLUMN IF NOT EXISTS Forecast   VARCHAR(128) NULL,
    ADD COLUMN IF NOT EXISTS ParentJobId INT         NULL;

-- CollectorCustomResult
ALTER TABLE CollectorCustomResult
    ADD COLUMN IF NOT EXISTS TenantId   INT          NOT NULL DEFAULT 0,
    ADD COLUMN IF NOT EXISTS EmployeeId INT          NOT NULL DEFAULT 0,
    ADD COLUMN IF NOT EXISTS DivisionId INT          NULL,
    ADD COLUMN IF NOT EXISTS PayrunJobId INT         NOT NULL DEFAULT 0,
    ADD COLUMN IF NOT EXISTS Forecast   VARCHAR(128) NULL,
    ADD COLUMN IF NOT EXISTS ParentJobId INT         NULL;

-- WageTypeCustomResult
ALTER TABLE WageTypeCustomResult
    ADD COLUMN IF NOT EXISTS TenantId   INT          NOT NULL DEFAULT 0,
    ADD COLUMN IF NOT EXISTS EmployeeId INT          NOT NULL DEFAULT 0,
    ADD COLUMN IF NOT EXISTS DivisionId INT          NULL,
    ADD COLUMN IF NOT EXISTS PayrunJobId INT         NOT NULL DEFAULT 0,
    ADD COLUMN IF NOT EXISTS Forecast   VARCHAR(128) NULL,
    ADD COLUMN IF NOT EXISTS ParentJobId INT         NULL;

-- =============================================================================
-- NEW INDEXES (idempotent via CREATE INDEX IF NOT EXISTS -- MySQL 8.0.29+)
-- =============================================================================

CREATE INDEX IF NOT EXISTS IX_Employee_TenantId
    ON Employee (TenantId, Status);

CREATE INDEX IF NOT EXISTS IX_EmployeeCaseValue_Cover
    ON EmployeeCaseValue (EmployeeId, CaseFieldName);

CREATE INDEX IF NOT EXISTS IX_GlobalCaseValue_Cover
    ON GlobalCaseValue (TenantId, CaseFieldName);

CREATE INDEX IF NOT EXISTS IX_NationalCaseValue_Cover
    ON NationalCaseValue (TenantId, CaseFieldName);

CREATE INDEX IF NOT EXISTS IX_CompanyCaseValue_Cover
    ON CompanyCaseValue (TenantId, CaseFieldName);

-- =============================================================================
-- STORED PROCEDURES (drop + recreate)
-- All GetDerived* SPs inline the GetDerivedRegulations CTE (Blocker 1 pattern)
-- =============================================================================

DELIMITER $$

-- -----------------------------------------------------------------------------
-- UpdateStatisticsTargeted (new in 0.9.6)
-- MySQL: ANALYZE TABLE instead of UPDATE STATISTICS ... WITH FULLSCAN
-- -----------------------------------------------------------------------------
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

-- -----------------------------------------------------------------------------
-- UpdateStatistics
-- MySQL: dynamically ANALYZE all tables
-- -----------------------------------------------------------------------------
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

-- -----------------------------------------------------------------------------
-- GetDerivedWageTypes
-- TVF replaced by inline CTE (Blocker 1)
-- -----------------------------------------------------------------------------
DROP PROCEDURE IF EXISTS GetDerivedWageTypes$$
CREATE PROCEDURE GetDerivedWageTypes(
    IN p_tenantId        INT,
    IN p_payrollId       INT,
    IN p_regulationDate  DATETIME(6),
    IN p_createdBefore   DATETIME(6),
    IN p_wageTypeNumbers VARCHAR(4000),
    IN p_includeClusters VARCHAR(4000),
    IN p_excludeClusters VARCHAR(4000)
)
BEGIN
    WITH DerivedRegulations AS (
        SELECT r.Id, pl.Level, pl.Priority,
            ROW_NUMBER() OVER (
                PARTITION BY pl.Id, r.Name
                ORDER BY r.ValidFrom DESC, r.Created DESC
            ) AS RowNumber
        FROM PayrollLayer pl
        INNER JOIN Regulation r ON pl.RegulationName = r.Name
        WHERE r.Status = 0
          AND (r.TenantId = p_tenantId OR r.SharedRegulation = 1)
          AND r.Created <= p_createdBefore
          AND (r.ValidFrom IS NULL OR r.ValidFrom <= p_regulationDate)
          AND pl.Status = 0 AND pl.PayrollId = p_payrollId
    ),
    Regulations AS (SELECT Id, Level, Priority FROM DerivedRegulations WHERE RowNumber = 1)
    SELECT
        Regulations.Id AS RegulationId, Regulations.Level, Regulations.Priority,
        wt.Id, wt.Status, wt.Created, wt.Updated, wt.RegulationId,
        wt.Name, wt.NameLocalizations, wt.WageTypeNumber,
        wt.Description, wt.DescriptionLocalizations, wt.OverrideType,
        wt.Calendar, wt.Culture, wt.Collectors, wt.CollectorGroups,
        wt.ValueExpression, wt.ResultExpression, wt.ValueActions, wt.ResultActions,
        wt.ScriptHash, wt.Attributes, wt.Clusters
    FROM WageType wt
    INNER JOIN Regulations ON wt.RegulationId = Regulations.Id
    WHERE wt.Status = 0
      AND wt.Created <= p_createdBefore
      AND ((p_includeClusters IS NULL AND p_excludeClusters IS NULL)
           OR IsMatchingCluster(p_includeClusters, p_excludeClusters, wt.Clusters) = 1)
      AND (p_wageTypeNumbers IS NULL
           OR wt.WageTypeNumber IN (
               SELECT CAST(jt.val AS DECIMAL(28,6))
               FROM JSON_TABLE(p_wageTypeNumbers, '$[*]' COLUMNS (val VARCHAR(50) PATH '$')) AS jt))
    ORDER BY wt.WageTypeNumber, Regulations.Level DESC, Regulations.Priority DESC;
END$$

-- -----------------------------------------------------------------------------
-- GetDerivedCases
-- -----------------------------------------------------------------------------
DROP PROCEDURE IF EXISTS GetDerivedCases$$
CREATE PROCEDURE GetDerivedCases(
    IN p_tenantId        INT,
    IN p_payrollId       INT,
    IN p_regulationDate  DATETIME(6),
    IN p_createdBefore   DATETIME(6),
    IN p_caseType        INT,
    IN p_caseNames       VARCHAR(4000),
    IN p_includeClusters VARCHAR(4000),
    IN p_excludeClusters VARCHAR(4000),
    IN p_hidden          TINYINT(1)
)
BEGIN
    WITH DerivedRegulations AS (
        SELECT r.Id, pl.Level, pl.Priority,
            ROW_NUMBER() OVER (PARTITION BY pl.Id, r.Name ORDER BY r.ValidFrom DESC, r.Created DESC) AS RowNumber
        FROM PayrollLayer pl INNER JOIN Regulation r ON pl.RegulationName = r.Name
        WHERE r.Status=0 AND (r.TenantId=p_tenantId OR r.SharedRegulation=1)
          AND r.Created<=p_createdBefore AND (r.ValidFrom IS NULL OR r.ValidFrom<=p_regulationDate)
          AND pl.Status=0 AND pl.PayrollId=p_payrollId
    ), Regulations AS (SELECT Id,Level,Priority FROM DerivedRegulations WHERE RowNumber=1)
    SELECT Regulations.Id AS RegulationId, Regulations.Level, Regulations.Priority,
        c.Id, c.Status, c.Created, c.Updated, c.RegulationId, c.CaseType,
        c.Name, c.NameLocalizations, c.NameSynonyms, c.Description, c.DescriptionLocalizations,
        c.DefaultReason, c.DefaultReasonLocalizations, c.BaseCase, c.BaseCaseFields,
        c.OverrideType, c.CancellationType, c.AvailableExpression, c.BuildExpression,
        c.ValidateExpression, c.Lookups, c.Slots, c.ScriptHash, c.Attributes, c.Clusters,
        c.AvailableActions, c.BuildActions, c.ValidateActions
    FROM `Case` c
    INNER JOIN Regulations ON c.RegulationId = Regulations.Id
    WHERE c.Status=0 AND c.Created<=p_createdBefore
      AND (p_hidden IS NULL OR c.Hidden=p_hidden)
      AND (p_caseType IS NULL OR c.CaseType=p_caseType)
      AND ((p_includeClusters IS NULL AND p_excludeClusters IS NULL)
           OR IsMatchingCluster(p_includeClusters, p_excludeClusters, c.Clusters)=1)
      AND (p_caseNames IS NULL OR LOWER(c.Name) IN (
           SELECT LOWER(jt.val) FROM JSON_TABLE(p_caseNames,'$[*]' COLUMNS (val VARCHAR(128) PATH '$')) AS jt))
    ORDER BY Regulations.Level DESC, Regulations.Priority DESC;
END$$

-- -----------------------------------------------------------------------------
-- GetDerivedCaseFields
-- -----------------------------------------------------------------------------
DROP PROCEDURE IF EXISTS GetDerivedCaseFields$$
CREATE PROCEDURE GetDerivedCaseFields(
    IN p_tenantId        INT,
    IN p_payrollId       INT,
    IN p_regulationDate  DATETIME(6),
    IN p_createdBefore   DATETIME(6),
    IN p_caseFieldNames  VARCHAR(4000),
    IN p_includeClusters VARCHAR(4000),
    IN p_excludeClusters VARCHAR(4000)
)
BEGIN
    WITH DerivedRegulations AS (
        SELECT r.Id, pl.Level, pl.Priority,
            ROW_NUMBER() OVER (PARTITION BY pl.Id, r.Name ORDER BY r.ValidFrom DESC, r.Created DESC) AS RowNumber
        FROM PayrollLayer pl INNER JOIN Regulation r ON pl.RegulationName=r.Name
        WHERE r.Status=0 AND (r.TenantId=p_tenantId OR r.SharedRegulation=1)
          AND r.Created<=p_createdBefore AND (r.ValidFrom IS NULL OR r.ValidFrom<=p_regulationDate)
          AND pl.Status=0 AND pl.PayrollId=p_payrollId
    ), Regulations AS (SELECT Id,Level,Priority FROM DerivedRegulations WHERE RowNumber=1)
    SELECT Regulations.Id AS RegulationId, Regulations.Level, Regulations.Priority,
        c.Id AS CaseId, c.CaseType, cf.*
    FROM CaseField cf
    INNER JOIN `Case` c ON cf.CaseId = c.Id
    INNER JOIN Regulations ON c.RegulationId = Regulations.Id
    WHERE cf.Status=0 AND cf.Created<=p_createdBefore
      AND ((p_includeClusters IS NULL AND p_excludeClusters IS NULL)
           OR IsMatchingCluster(p_includeClusters, p_excludeClusters, cf.Clusters)=1)
      AND (p_caseFieldNames IS NULL OR LOWER(cf.Name) IN (
           SELECT LOWER(jt.val) FROM JSON_TABLE(p_caseFieldNames,'$[*]' COLUMNS (val VARCHAR(128) PATH '$')) AS jt))
    ORDER BY Regulations.Level DESC, Regulations.Priority DESC;
END$$

-- -----------------------------------------------------------------------------
-- GetDerivedCaseFieldsOfCase
-- -----------------------------------------------------------------------------
DROP PROCEDURE IF EXISTS GetDerivedCaseFieldsOfCase$$
CREATE PROCEDURE GetDerivedCaseFieldsOfCase(
    IN p_tenantId        INT,
    IN p_payrollId       INT,
    IN p_regulationDate  DATETIME(6),
    IN p_createdBefore   DATETIME(6),
    IN p_caseNames       VARCHAR(4000),
    IN p_includeClusters VARCHAR(4000),
    IN p_excludeClusters VARCHAR(4000)
)
BEGIN
    WITH DerivedRegulations AS (
        SELECT r.Id, pl.Level, pl.Priority,
            ROW_NUMBER() OVER (PARTITION BY pl.Id, r.Name ORDER BY r.ValidFrom DESC, r.Created DESC) AS RowNumber
        FROM PayrollLayer pl INNER JOIN Regulation r ON pl.RegulationName=r.Name
        WHERE r.Status=0 AND (r.TenantId=p_tenantId OR r.SharedRegulation=1)
          AND r.Created<=p_createdBefore AND (r.ValidFrom IS NULL OR r.ValidFrom<=p_regulationDate)
          AND pl.Status=0 AND pl.PayrollId=p_payrollId
    ), Regulations AS (SELECT Id,Level,Priority FROM DerivedRegulations WHERE RowNumber=1)
    SELECT Regulations.Id AS RegulationId, Regulations.Level, Regulations.Priority,
        c.Id AS CaseId, c.CaseType, cf.*
    FROM CaseField cf
    INNER JOIN `Case` c ON cf.CaseId = c.Id
    INNER JOIN Regulations ON c.RegulationId = Regulations.Id
    WHERE cf.Status=0 AND cf.Created<=p_createdBefore
      AND ((p_includeClusters IS NULL AND p_excludeClusters IS NULL)
           OR IsMatchingCluster(p_includeClusters, p_excludeClusters, cf.Clusters)=1)
      AND (p_caseNames IS NULL OR LOWER(c.Name) IN (
           SELECT LOWER(jt.val) FROM JSON_TABLE(p_caseNames,'$[*]' COLUMNS (val VARCHAR(128) PATH '$')) AS jt))
    ORDER BY Regulations.Level DESC, Regulations.Priority DESC;
END$$

-- -----------------------------------------------------------------------------
-- GetDerivedCaseRelations
-- -----------------------------------------------------------------------------
DROP PROCEDURE IF EXISTS GetDerivedCaseRelations$$
CREATE PROCEDURE GetDerivedCaseRelations(
    IN p_tenantId        INT,
    IN p_payrollId       INT,
    IN p_regulationDate  DATETIME(6),
    IN p_createdBefore   DATETIME(6),
    IN p_sourceCaseName  VARCHAR(128),
    IN p_targetCaseName  VARCHAR(128),
    IN p_includeClusters VARCHAR(4000),
    IN p_excludeClusters VARCHAR(4000)
)
BEGIN
    WITH DerivedRegulations AS (
        SELECT r.Id, pl.Level, pl.Priority,
            ROW_NUMBER() OVER (PARTITION BY pl.Id, r.Name ORDER BY r.ValidFrom DESC, r.Created DESC) AS RowNumber
        FROM PayrollLayer pl INNER JOIN Regulation r ON pl.RegulationName=r.Name
        WHERE r.Status=0 AND (r.TenantId=p_tenantId OR r.SharedRegulation=1)
          AND r.Created<=p_createdBefore AND (r.ValidFrom IS NULL OR r.ValidFrom<=p_regulationDate)
          AND pl.Status=0 AND pl.PayrollId=p_payrollId
    ), Regulations AS (SELECT Id,Level,Priority FROM DerivedRegulations WHERE RowNumber=1)
    SELECT Regulations.Id AS RegulationId, Regulations.Level, Regulations.Priority,
        cr.Id, cr.Status, cr.Created, cr.Updated, cr.RegulationId,
        cr.SourceCaseName, cr.SourceCaseNameLocalizations, cr.SourceCaseSlot, cr.SourceCaseSlotLocalizations,
        cr.TargetCaseName, cr.TargetCaseNameLocalizations, cr.TargetCaseSlot, cr.TargetCaseSlotLocalizations,
        cr.RelationHash, cr.BuildExpression, cr.ValidateExpression, cr.OverrideType, cr.`Order`,
        cr.ScriptHash, cr.Attributes, cr.Clusters, cr.BuildActions, cr.ValidateActions
    FROM CaseRelation cr
    INNER JOIN Regulations ON cr.RegulationId = Regulations.Id
    WHERE cr.Status=0 AND cr.Created<=p_createdBefore
      AND (p_sourceCaseName IS NULL OR LOWER(cr.SourceCaseName)=LOWER(p_sourceCaseName))
      AND (p_targetCaseName IS NULL OR LOWER(cr.TargetCaseName)=LOWER(p_targetCaseName))
      AND ((p_includeClusters IS NULL AND p_excludeClusters IS NULL)
           OR IsMatchingCluster(p_includeClusters, p_excludeClusters, cr.Clusters)=1)
    ORDER BY cr.SourceCaseName, cr.TargetCaseName, Regulations.Level DESC, Regulations.Priority DESC;
END$$

-- -----------------------------------------------------------------------------
-- GetDerivedCollectors
-- -----------------------------------------------------------------------------
DROP PROCEDURE IF EXISTS GetDerivedCollectors$$
CREATE PROCEDURE GetDerivedCollectors(
    IN p_tenantId        INT,
    IN p_payrollId       INT,
    IN p_regulationDate  DATETIME(6),
    IN p_createdBefore   DATETIME(6),
    IN p_collectorNames  VARCHAR(4000),
    IN p_includeClusters VARCHAR(4000),
    IN p_excludeClusters VARCHAR(4000)
)
BEGIN
    WITH DerivedRegulations AS (
        SELECT r.Id, pl.Level, pl.Priority,
            ROW_NUMBER() OVER (PARTITION BY pl.Id, r.Name ORDER BY r.ValidFrom DESC, r.Created DESC) AS RowNumber
        FROM PayrollLayer pl INNER JOIN Regulation r ON pl.RegulationName=r.Name
        WHERE r.Status=0 AND (r.TenantId=p_tenantId OR r.SharedRegulation=1)
          AND r.Created<=p_createdBefore AND (r.ValidFrom IS NULL OR r.ValidFrom<=p_regulationDate)
          AND pl.Status=0 AND pl.PayrollId=p_payrollId
    ), Regulations AS (SELECT Id,Level,Priority FROM DerivedRegulations WHERE RowNumber=1)
    SELECT Regulations.Id AS RegulationId, Regulations.Level, Regulations.Priority,
        co.Id, co.Status, co.Created, co.Updated, co.RegulationId,
        co.Name, co.NameLocalizations, co.CollectMode, co.Negated, co.OverrideType,
        co.ValueType, co.Culture, co.CollectorGroups,
        co.StartExpression, co.ApplyExpression, co.EndExpression,
        co.StartActions, co.ApplyActions, co.EndActions,
        co.Threshold, co.MinResult, co.MaxResult,
        co.ScriptHash, co.Attributes, co.Clusters
    FROM Collector co
    INNER JOIN Regulations ON co.RegulationId = Regulations.Id
    WHERE co.Status=0 AND co.Created<=p_createdBefore
      AND ((p_includeClusters IS NULL AND p_excludeClusters IS NULL)
           OR IsMatchingCluster(p_includeClusters, p_excludeClusters, co.Clusters)=1)
      AND (p_collectorNames IS NULL OR LOWER(co.Name) IN (
           SELECT LOWER(jt.val) FROM JSON_TABLE(p_collectorNames,'$[*]' COLUMNS (val VARCHAR(128) PATH '$')) AS jt))
    ORDER BY co.Name, Regulations.Level DESC, Regulations.Priority DESC;
END$$

-- -----------------------------------------------------------------------------
-- GetDerivedLookups
-- -----------------------------------------------------------------------------
DROP PROCEDURE IF EXISTS GetDerivedLookups$$
CREATE PROCEDURE GetDerivedLookups(
    IN p_tenantId        INT,
    IN p_payrollId       INT,
    IN p_regulationDate  DATETIME(6),
    IN p_createdBefore   DATETIME(6),
    IN p_lookupNames     VARCHAR(4000)
)
BEGIN
    WITH DerivedRegulations AS (
        SELECT r.Id, pl.Level, pl.Priority,
            ROW_NUMBER() OVER (PARTITION BY pl.Id, r.Name ORDER BY r.ValidFrom DESC, r.Created DESC) AS RowNumber
        FROM PayrollLayer pl INNER JOIN Regulation r ON pl.RegulationName=r.Name
        WHERE r.Status=0 AND (r.TenantId=p_tenantId OR r.SharedRegulation=1)
          AND r.Created<=p_createdBefore AND (r.ValidFrom IS NULL OR r.ValidFrom<=p_regulationDate)
          AND pl.Status=0 AND pl.PayrollId=p_payrollId
    ), Regulations AS (SELECT Id,Level,Priority FROM DerivedRegulations WHERE RowNumber=1)
    SELECT Regulations.Id AS RegulationId, Regulations.Level, Regulations.Priority, lk.*
    FROM Lookup lk
    INNER JOIN Regulations ON lk.RegulationId = Regulations.Id
    WHERE lk.Status=0 AND lk.Created<=p_createdBefore
      AND (p_lookupNames IS NULL OR LOWER(lk.Name) IN (
           SELECT LOWER(jt.val) FROM JSON_TABLE(p_lookupNames,'$[*]' COLUMNS (val VARCHAR(128) PATH '$')) AS jt))
    ORDER BY Regulations.Level DESC, Regulations.Priority DESC;
END$$

-- -----------------------------------------------------------------------------
-- GetDerivedLookupValues
-- -----------------------------------------------------------------------------
DROP PROCEDURE IF EXISTS GetDerivedLookupValues$$
CREATE PROCEDURE GetDerivedLookupValues(
    IN p_tenantId        INT,
    IN p_payrollId       INT,
    IN p_regulationDate  DATETIME(6),
    IN p_createdBefore   DATETIME(6),
    IN p_lookupNames     VARCHAR(4000),
    IN p_lookupKeys      VARCHAR(4000)
)
BEGIN
    WITH DerivedRegulations AS (
        SELECT r.Id, pl.Level, pl.Priority,
            ROW_NUMBER() OVER (PARTITION BY pl.Id, r.Name ORDER BY r.ValidFrom DESC, r.Created DESC) AS RowNumber
        FROM PayrollLayer pl INNER JOIN Regulation r ON pl.RegulationName=r.Name
        WHERE r.Status=0 AND (r.TenantId=p_tenantId OR r.SharedRegulation=1)
          AND r.Created<=p_createdBefore AND (r.ValidFrom IS NULL OR r.ValidFrom<=p_regulationDate)
          AND pl.Status=0 AND pl.PayrollId=p_payrollId
    ), Regulations AS (SELECT Id,Level,Priority FROM DerivedRegulations WHERE RowNumber=1)
    SELECT Regulations.Id AS RegulationId, Regulations.Level, Regulations.Priority, lv.*
    FROM LookupValue lv
    INNER JOIN Lookup lk ON lv.LookupId = lk.Id
    INNER JOIN Regulations ON lk.RegulationId = Regulations.Id
    WHERE lv.Status=0 AND lv.Created<=p_createdBefore
      AND (p_lookupNames IS NULL OR LOWER(lk.Name) IN (
           SELECT LOWER(jt.val) FROM JSON_TABLE(p_lookupNames,'$[*]' COLUMNS (val VARCHAR(128) PATH '$')) AS jt))
      AND (p_lookupKeys IS NULL OR lv.`Key` IN (
           SELECT jt.val FROM JSON_TABLE(p_lookupKeys,'$[*]' COLUMNS (val VARCHAR(128) PATH '$')) AS jt))
    ORDER BY Regulations.Level DESC, Regulations.Priority DESC;
END$$

-- -----------------------------------------------------------------------------
-- GetDerivedReports
-- -----------------------------------------------------------------------------
DROP PROCEDURE IF EXISTS GetDerivedReports$$
CREATE PROCEDURE GetDerivedReports(
    IN p_tenantId        INT,
    IN p_payrollId       INT,
    IN p_regulationDate  DATETIME(6),
    IN p_createdBefore   DATETIME(6),
    IN p_userType        INT,
    IN p_reportNames     VARCHAR(4000),
    IN p_includeClusters VARCHAR(4000),
    IN p_excludeClusters VARCHAR(4000)
)
BEGIN
    WITH DerivedRegulations AS (
        SELECT r.Id, pl.Level, pl.Priority,
            ROW_NUMBER() OVER (PARTITION BY pl.Id, r.Name ORDER BY r.ValidFrom DESC, r.Created DESC) AS RowNumber
        FROM PayrollLayer pl INNER JOIN Regulation r ON pl.RegulationName=r.Name
        WHERE r.Status=0 AND (r.TenantId=p_tenantId OR r.SharedRegulation=1)
          AND r.Created<=p_createdBefore AND (r.ValidFrom IS NULL OR r.ValidFrom<=p_regulationDate)
          AND pl.Status=0 AND pl.PayrollId=p_payrollId
    ), Regulations AS (SELECT Id,Level,Priority FROM DerivedRegulations WHERE RowNumber=1)
    SELECT Regulations.Id AS RegulationId, Regulations.Level, Regulations.Priority,
        rp.Id, rp.Status, rp.Created, rp.Updated, rp.RegulationId,
        rp.Name, rp.NameLocalizations, rp.Description, rp.DescriptionLocalizations,
        rp.Category, rp.Queries, rp.Relations, rp.AttributeMode, rp.UserType, rp.ReportIsolation,
        rp.BuildExpression, rp.StartExpression, rp.EndExpression,
        rp.ScriptHash, rp.Attributes, rp.Clusters
    FROM Report rp
    INNER JOIN Regulations ON rp.RegulationId = Regulations.Id
    WHERE rp.Status=0 AND rp.Created<=p_createdBefore
      AND (p_userType IS NULL OR rp.UserType<=p_userType)
      AND ((p_includeClusters IS NULL AND p_excludeClusters IS NULL)
           OR IsMatchingCluster(p_includeClusters, p_excludeClusters, rp.Clusters)=1)
      AND (p_reportNames IS NULL OR LOWER(rp.Name) IN (
           SELECT LOWER(jt.val) FROM JSON_TABLE(p_reportNames,'$[*]' COLUMNS (val VARCHAR(128) PATH '$')) AS jt))
    ORDER BY Regulations.Level DESC, Regulations.Priority DESC;
END$$

-- -----------------------------------------------------------------------------
-- GetDerivedReportParameters
-- -----------------------------------------------------------------------------
DROP PROCEDURE IF EXISTS GetDerivedReportParameters$$
CREATE PROCEDURE GetDerivedReportParameters(
    IN p_tenantId       INT,
    IN p_payrollId      INT,
    IN p_regulationDate DATETIME(6),
    IN p_createdBefore  DATETIME(6),
    IN p_reportNames    VARCHAR(4000)
)
BEGIN
    WITH DerivedRegulations AS (
        SELECT r.Id, pl.Level, pl.Priority,
            ROW_NUMBER() OVER (PARTITION BY pl.Id, r.Name ORDER BY r.ValidFrom DESC, r.Created DESC) AS RowNumber
        FROM PayrollLayer pl INNER JOIN Regulation r ON pl.RegulationName=r.Name
        WHERE r.Status=0 AND (r.TenantId=p_tenantId OR r.SharedRegulation=1)
          AND r.Created<=p_createdBefore AND (r.ValidFrom IS NULL OR r.ValidFrom<=p_regulationDate)
          AND pl.Status=0 AND pl.PayrollId=p_payrollId
    ), Regulations AS (SELECT Id,Level,Priority FROM DerivedRegulations WHERE RowNumber=1)
    SELECT Regulations.Id AS RegulationId, Regulations.Level, Regulations.Priority, rp.*
    FROM ReportParameter rp
    INNER JOIN Report r ON rp.ReportId=r.Id
    INNER JOIN Regulations ON r.RegulationId=Regulations.Id
    WHERE rp.Status=0 AND rp.Created<=p_createdBefore
      AND (p_reportNames IS NULL OR LOWER(r.Name) IN (
           SELECT LOWER(jt.val) FROM JSON_TABLE(p_reportNames,'$[*]' COLUMNS (val VARCHAR(128) PATH '$')) AS jt))
    ORDER BY Regulations.Level DESC, Regulations.Priority DESC;
END$$

-- -----------------------------------------------------------------------------
-- GetDerivedReportTemplates
-- -----------------------------------------------------------------------------
DROP PROCEDURE IF EXISTS GetDerivedReportTemplates$$
CREATE PROCEDURE GetDerivedReportTemplates(
    IN p_tenantId       INT,
    IN p_payrollId      INT,
    IN p_regulationDate DATETIME(6),
    IN p_createdBefore  DATETIME(6),
    IN p_reportNames    VARCHAR(4000),
    IN p_culture        VARCHAR(128)
)
BEGIN
    WITH DerivedRegulations AS (
        SELECT r.Id, pl.Level, pl.Priority,
            ROW_NUMBER() OVER (PARTITION BY pl.Id, r.Name ORDER BY r.ValidFrom DESC, r.Created DESC) AS RowNumber
        FROM PayrollLayer pl INNER JOIN Regulation r ON pl.RegulationName=r.Name
        WHERE r.Status=0 AND (r.TenantId=p_tenantId OR r.SharedRegulation=1)
          AND r.Created<=p_createdBefore AND (r.ValidFrom IS NULL OR r.ValidFrom<=p_regulationDate)
          AND pl.Status=0 AND pl.PayrollId=p_payrollId
    ), Regulations AS (SELECT Id,Level,Priority FROM DerivedRegulations WHERE RowNumber=1)
    SELECT Regulations.Id AS RegulationId, Regulations.Level, Regulations.Priority, rt.*
    FROM ReportTemplate rt
    INNER JOIN Report r ON rt.ReportId=r.Id
    INNER JOIN Regulations ON r.RegulationId=Regulations.Id
    WHERE rt.Status=0 AND rt.Created<=p_createdBefore
      AND (p_reportNames IS NULL OR LOWER(r.Name) IN (
           SELECT LOWER(jt.val) FROM JSON_TABLE(p_reportNames,'$[*]' COLUMNS (val VARCHAR(128) PATH '$')) AS jt))
      AND (p_culture IS NULL OR rt.Culture=p_culture)
    ORDER BY Regulations.Level DESC, Regulations.Priority DESC;
END$$

-- -----------------------------------------------------------------------------
-- GetDerivedScripts
-- -----------------------------------------------------------------------------
DROP PROCEDURE IF EXISTS GetDerivedScripts$$
CREATE PROCEDURE GetDerivedScripts(
    IN p_tenantId       INT,
    IN p_payrollId      INT,
    IN p_regulationDate DATETIME(6),
    IN p_createdBefore  DATETIME(6),
    IN p_scriptNames    VARCHAR(4000)
)
BEGIN
    WITH DerivedRegulations AS (
        SELECT r.Id, pl.Level, pl.Priority,
            ROW_NUMBER() OVER (PARTITION BY pl.Id, r.Name ORDER BY r.ValidFrom DESC, r.Created DESC) AS RowNumber
        FROM PayrollLayer pl INNER JOIN Regulation r ON pl.RegulationName=r.Name
        WHERE r.Status=0 AND (r.TenantId=p_tenantId OR r.SharedRegulation=1)
          AND r.Created<=p_createdBefore AND (r.ValidFrom IS NULL OR r.ValidFrom<=p_regulationDate)
          AND pl.Status=0 AND pl.PayrollId=p_payrollId
    ), Regulations AS (SELECT Id,Level,Priority FROM DerivedRegulations WHERE RowNumber=1)
    SELECT Regulations.Id AS RegulationId, Regulations.Level, Regulations.Priority,
        s.Id, s.Status, s.Created, s.Updated, s.RegulationId,
        s.Name, s.FunctionTypeMask, s.Value
    FROM Script s
    INNER JOIN Regulations ON s.RegulationId=Regulations.Id
    WHERE s.Status=0 AND s.Created<=p_createdBefore
      AND (p_scriptNames IS NULL OR LOWER(s.Name) IN (
           SELECT LOWER(jt.val) FROM JSON_TABLE(p_scriptNames,'$[*]' COLUMNS (val VARCHAR(128) PATH '$')) AS jt))
    ORDER BY Regulations.Level DESC, Regulations.Priority DESC;
END$$

-- -----------------------------------------------------------------------------
-- GetDerivedPayrollRegulations
-- -----------------------------------------------------------------------------
DROP PROCEDURE IF EXISTS GetDerivedPayrollRegulations$$
CREATE PROCEDURE GetDerivedPayrollRegulations(
    IN p_tenantId       INT,
    IN p_payrollId      INT,
    IN p_regulationDate DATETIME(6),
    IN p_createdBefore  DATETIME(6)
)
BEGIN
    WITH DerivedRegulations AS (
        SELECT r.Id, pl.Level, pl.Priority,
            ROW_NUMBER() OVER (PARTITION BY pl.Id, r.Name ORDER BY r.ValidFrom DESC, r.Created DESC) AS RowNumber
        FROM PayrollLayer pl INNER JOIN Regulation r ON pl.RegulationName=r.Name
        WHERE r.Status=0 AND (r.TenantId=p_tenantId OR r.SharedRegulation=1)
          AND r.Created<=p_createdBefore AND (r.ValidFrom IS NULL OR r.ValidFrom<=p_regulationDate)
          AND pl.Status=0 AND pl.PayrollId=p_payrollId
    )
    SELECT r.*
    FROM Regulation r
    INNER JOIN DerivedRegulations dr ON r.Id=dr.Id
    WHERE dr.RowNumber=1
    ORDER BY dr.Level DESC, dr.Priority DESC;
END$$

-- -----------------------------------------------------------------------------
-- GetConsolidatedWageTypeResults
-- OPTION(RECOMPILE) -> removed (no MySQL equivalent needed)
-- -----------------------------------------------------------------------------
DROP PROCEDURE IF EXISTS GetConsolidatedWageTypeResults$$
CREATE PROCEDURE GetConsolidatedWageTypeResults(
    IN p_tenantId          INT,
    IN p_employeeId        INT,
    IN p_divisionId        INT,
    IN p_wageTypeNumbers   VARCHAR(4000),
    IN p_periodStartHashes VARCHAR(4000),
    IN p_jobStatus         INT,
    IN p_forecast          VARCHAR(128),
    IN p_evaluationDate    DATETIME(6),
    IN p_noRetro           TINYINT(1),
    IN p_excludeParentJobId INT
)
BEGIN
    DECLARE v_wageTypeNumber   DECIMAL(28,6);
    DECLARE v_wageTypeCount    INT;
    DECLARE v_startHash        INT;
    DECLARE v_startHashCount   INT;

    SELECT COUNT(*) INTO v_wageTypeCount  FROM JSON_TABLE(p_wageTypeNumbers,  '$[*]' COLUMNS (val VARCHAR(50) PATH '$')) AS jt;
    SELECT COUNT(*) INTO v_startHashCount FROM JSON_TABLE(p_periodStartHashes,'$[*]' COLUMNS (val VARCHAR(20) PATH '$')) AS jt;

    IF v_wageTypeCount = 1 THEN
        SELECT CAST(jt.val AS DECIMAL(28,6)) INTO v_wageTypeNumber
        FROM JSON_TABLE(p_wageTypeNumbers,'$[*]' COLUMNS (val VARCHAR(50) PATH '$')) AS jt LIMIT 1;
    END IF;
    IF v_startHashCount = 1 THEN
        SELECT CAST(jt.val AS INT) INTO v_startHash
        FROM JSON_TABLE(p_periodStartHashes,'$[*]' COLUMNS (val VARCHAR(20) PATH '$')) AS jt LIMIT 1;
    END IF;

    WITH Winners AS (
        SELECT wtr.Id,
            ROW_NUMBER() OVER (
                PARTITION BY wtr.WageTypeNumber, wtr.Start
                ORDER BY wtr.Created DESC, wtr.Id DESC
            ) AS RowNumber
        FROM WageTypeResult wtr
        WHERE wtr.TenantId=p_tenantId AND wtr.EmployeeId=p_employeeId
          AND (
              (v_startHashCount=1 AND wtr.StartHash=v_startHash)
              OR (v_startHashCount>1 AND wtr.StartHash IN (
                  SELECT CAST(jt.val AS INT) FROM JSON_TABLE(p_periodStartHashes,'$[*]' COLUMNS (val VARCHAR(20) PATH '$')) AS jt))
          )
          AND (p_divisionId IS NULL OR wtr.DivisionId=p_divisionId)
          AND (p_wageTypeNumbers IS NULL OR v_wageTypeCount=0
               OR (v_wageTypeCount=1 AND wtr.WageTypeNumber=v_wageTypeNumber)
               OR (v_wageTypeCount>1 AND wtr.WageTypeNumber IN (
                   SELECT CAST(jt.val AS DECIMAL(28,6)) FROM JSON_TABLE(p_wageTypeNumbers,'$[*]' COLUMNS (val VARCHAR(50) PATH '$')) AS jt)))
          AND (p_evaluationDate IS NULL OR wtr.Created<=p_evaluationDate)
          AND (p_jobStatus IS NULL OR wtr.PayrunJobId IN (
              SELECT pj.Id FROM PayrunJob pj WHERE pj.JobStatus & p_jobStatus = pj.JobStatus))
          AND (wtr.Forecast IS NULL OR wtr.Forecast=p_forecast)
          AND (p_noRetro=0 OR wtr.ParentJobId IS NULL)
          AND (p_excludeParentJobId IS NULL OR wtr.ParentJobId IS NULL OR wtr.ParentJobId<>p_excludeParentJobId)
    )
    SELECT wtr.*
    FROM WageTypeResult wtr
    INNER JOIN Winners w ON w.Id=wtr.Id
    WHERE w.RowNumber=1;
END$$

-- -----------------------------------------------------------------------------
-- GetConsolidatedCollectorResults
-- -----------------------------------------------------------------------------
DROP PROCEDURE IF EXISTS GetConsolidatedCollectorResults$$
CREATE PROCEDURE GetConsolidatedCollectorResults(
    IN p_tenantId             INT,
    IN p_employeeId           INT,
    IN p_divisionId           INT,
    IN p_collectorNameHashes  VARCHAR(4000),
    IN p_periodStartHashes    VARCHAR(4000),
    IN p_jobStatus            INT,
    IN p_forecast             VARCHAR(128),
    IN p_evaluationDate       DATETIME(6),
    IN p_noRetro              TINYINT(1),
    IN p_excludeParentJobId   INT
)
BEGIN
    DECLARE v_hash INT; DECLARE v_hashCount INT;
    DECLARE v_startHash INT; DECLARE v_startHashCount INT;
    SELECT COUNT(*) INTO v_hashCount      FROM JSON_TABLE(p_collectorNameHashes,'$[*]' COLUMNS (val VARCHAR(20) PATH '$')) AS jt;
    SELECT COUNT(*) INTO v_startHashCount FROM JSON_TABLE(p_periodStartHashes,  '$[*]' COLUMNS (val VARCHAR(20) PATH '$')) AS jt;
    IF v_hashCount=1      THEN SELECT CAST(jt.val AS INT) INTO v_hash      FROM JSON_TABLE(p_collectorNameHashes,'$[*]' COLUMNS (val VARCHAR(20) PATH '$')) AS jt LIMIT 1; END IF;
    IF v_startHashCount=1 THEN SELECT CAST(jt.val AS INT) INTO v_startHash FROM JSON_TABLE(p_periodStartHashes,'$[*]' COLUMNS (val VARCHAR(20) PATH '$'))  AS jt LIMIT 1; END IF;

    WITH Winners AS (
        SELECT r.Id, ROW_NUMBER() OVER (PARTITION BY r.CollectorNameHash, r.Start ORDER BY r.Created DESC, r.Id DESC) AS RowNumber
        FROM CollectorResult r
        WHERE r.TenantId=p_tenantId AND r.EmployeeId=p_employeeId
          AND ((v_startHashCount=1 AND r.StartHash=v_startHash) OR (v_startHashCount>1 AND r.StartHash IN (SELECT CAST(jt.val AS INT) FROM JSON_TABLE(p_periodStartHashes,'$[*]' COLUMNS (val VARCHAR(20) PATH '$')) AS jt)))
          AND (p_divisionId IS NULL OR r.DivisionId=p_divisionId)
          AND (p_collectorNameHashes IS NULL OR v_hashCount=0 OR (v_hashCount=1 AND r.CollectorNameHash=v_hash) OR (v_hashCount>1 AND r.CollectorNameHash IN (SELECT CAST(jt.val AS INT) FROM JSON_TABLE(p_collectorNameHashes,'$[*]' COLUMNS (val VARCHAR(20) PATH '$')) AS jt)))
          AND (p_evaluationDate IS NULL OR r.Created<=p_evaluationDate)
          AND (p_jobStatus IS NULL OR r.PayrunJobId IN (SELECT pj.Id FROM PayrunJob pj WHERE pj.JobStatus & p_jobStatus=pj.JobStatus))
          AND (r.Forecast IS NULL OR r.Forecast=p_forecast)
          AND (p_noRetro=0 OR r.ParentJobId IS NULL)
          AND (p_excludeParentJobId IS NULL OR r.ParentJobId IS NULL OR r.ParentJobId<>p_excludeParentJobId)
    )
    SELECT r.* FROM CollectorResult r INNER JOIN Winners w ON w.Id=r.Id WHERE w.RowNumber=1;
END$$

-- -----------------------------------------------------------------------------
-- GetConsolidatedCollectorCustomResults
-- -----------------------------------------------------------------------------
DROP PROCEDURE IF EXISTS GetConsolidatedCollectorCustomResults$$
CREATE PROCEDURE GetConsolidatedCollectorCustomResults(
    IN p_tenantId             INT,
    IN p_employeeId           INT,
    IN p_divisionId           INT,
    IN p_collectorNameHashes  VARCHAR(4000),
    IN p_periodStartHashes    VARCHAR(4000),
    IN p_jobStatus            INT,
    IN p_forecast             VARCHAR(128),
    IN p_evaluationDate       DATETIME(6),
    IN p_noRetro              TINYINT(1),
    IN p_excludeParentJobId   INT
)
BEGIN
    DECLARE v_hash INT; DECLARE v_hashCount INT;
    DECLARE v_startHash INT; DECLARE v_startHashCount INT;
    SELECT COUNT(*) INTO v_hashCount      FROM JSON_TABLE(p_collectorNameHashes,'$[*]' COLUMNS (val VARCHAR(20) PATH '$')) AS jt;
    SELECT COUNT(*) INTO v_startHashCount FROM JSON_TABLE(p_periodStartHashes,  '$[*]' COLUMNS (val VARCHAR(20) PATH '$')) AS jt;
    IF v_hashCount=1      THEN SELECT CAST(jt.val AS INT) INTO v_hash      FROM JSON_TABLE(p_collectorNameHashes,'$[*]' COLUMNS (val VARCHAR(20) PATH '$')) AS jt LIMIT 1; END IF;
    IF v_startHashCount=1 THEN SELECT CAST(jt.val AS INT) INTO v_startHash FROM JSON_TABLE(p_periodStartHashes,'$[*]' COLUMNS (val VARCHAR(20) PATH '$'))  AS jt LIMIT 1; END IF;

    WITH Winners AS (
        SELECT r.Id, ROW_NUMBER() OVER (PARTITION BY r.CollectorNameHash, r.Start ORDER BY r.Created DESC, r.Id DESC) AS RowNumber
        FROM CollectorCustomResult r
        WHERE r.TenantId=p_tenantId AND r.EmployeeId=p_employeeId
          AND ((v_startHashCount=1 AND r.StartHash=v_startHash) OR (v_startHashCount>1 AND r.StartHash IN (SELECT CAST(jt.val AS INT) FROM JSON_TABLE(p_periodStartHashes,'$[*]' COLUMNS (val VARCHAR(20) PATH '$')) AS jt)))
          AND (p_divisionId IS NULL OR r.DivisionId=p_divisionId)
          AND (p_collectorNameHashes IS NULL OR v_hashCount=0 OR (v_hashCount=1 AND r.CollectorNameHash=v_hash) OR (v_hashCount>1 AND r.CollectorNameHash IN (SELECT CAST(jt.val AS INT) FROM JSON_TABLE(p_collectorNameHashes,'$[*]' COLUMNS (val VARCHAR(20) PATH '$')) AS jt)))
          AND (p_evaluationDate IS NULL OR r.Created<=p_evaluationDate)
          AND (p_jobStatus IS NULL OR r.PayrunJobId IN (SELECT pj.Id FROM PayrunJob pj WHERE pj.JobStatus & p_jobStatus=pj.JobStatus))
          AND (r.Forecast IS NULL OR r.Forecast=p_forecast)
          AND (p_noRetro=0 OR r.ParentJobId IS NULL)
          AND (p_excludeParentJobId IS NULL OR r.ParentJobId IS NULL OR r.ParentJobId<>p_excludeParentJobId)
    )
    SELECT r.* FROM CollectorCustomResult r INNER JOIN Winners w ON w.Id=r.Id WHERE w.RowNumber=1;
END$$

-- -----------------------------------------------------------------------------
-- GetConsolidatedWageTypeCustomResults
-- -----------------------------------------------------------------------------
DROP PROCEDURE IF EXISTS GetConsolidatedWageTypeCustomResults$$
CREATE PROCEDURE GetConsolidatedWageTypeCustomResults(
    IN p_tenantId          INT,
    IN p_employeeId        INT,
    IN p_divisionId        INT,
    IN p_wageTypeNumbers   VARCHAR(4000),
    IN p_periodStartHashes VARCHAR(4000),
    IN p_jobStatus         INT,
    IN p_forecast          VARCHAR(128),
    IN p_evaluationDate    DATETIME(6),
    IN p_noRetro           TINYINT(1),
    IN p_excludeParentJobId INT
)
BEGIN
    DECLARE v_wt DECIMAL(28,6); DECLARE v_wtCount INT;
    DECLARE v_startHash INT;    DECLARE v_startHashCount INT;
    SELECT COUNT(*) INTO v_wtCount        FROM JSON_TABLE(p_wageTypeNumbers,  '$[*]' COLUMNS (val VARCHAR(50) PATH '$')) AS jt;
    SELECT COUNT(*) INTO v_startHashCount FROM JSON_TABLE(p_periodStartHashes,'$[*]' COLUMNS (val VARCHAR(20) PATH '$')) AS jt;
    IF v_wtCount=1        THEN SELECT CAST(jt.val AS DECIMAL(28,6)) INTO v_wt       FROM JSON_TABLE(p_wageTypeNumbers,'$[*]' COLUMNS (val VARCHAR(50) PATH '$'))  AS jt LIMIT 1; END IF;
    IF v_startHashCount=1 THEN SELECT CAST(jt.val AS INT)           INTO v_startHash FROM JSON_TABLE(p_periodStartHashes,'$[*]' COLUMNS (val VARCHAR(20) PATH '$')) AS jt LIMIT 1; END IF;

    WITH Winners AS (
        SELECT r.Id, ROW_NUMBER() OVER (PARTITION BY r.WageTypeNumber, r.Start ORDER BY r.Created DESC, r.Id DESC) AS RowNumber
        FROM WageTypeCustomResult r
        WHERE r.TenantId=p_tenantId AND r.EmployeeId=p_employeeId
          AND ((v_startHashCount=1 AND r.StartHash=v_startHash) OR (v_startHashCount>1 AND r.StartHash IN (SELECT CAST(jt.val AS INT) FROM JSON_TABLE(p_periodStartHashes,'$[*]' COLUMNS (val VARCHAR(20) PATH '$')) AS jt)))
          AND (p_divisionId IS NULL OR r.DivisionId=p_divisionId)
          AND (p_wageTypeNumbers IS NULL OR v_wtCount=0 OR (v_wtCount=1 AND r.WageTypeNumber=v_wt) OR (v_wtCount>1 AND r.WageTypeNumber IN (SELECT CAST(jt.val AS DECIMAL(28,6)) FROM JSON_TABLE(p_wageTypeNumbers,'$[*]' COLUMNS (val VARCHAR(50) PATH '$')) AS jt)))
          AND (p_evaluationDate IS NULL OR r.Created<=p_evaluationDate)
          AND (p_jobStatus IS NULL OR r.PayrunJobId IN (SELECT pj.Id FROM PayrunJob pj WHERE pj.JobStatus & p_jobStatus=pj.JobStatus))
          AND (r.Forecast IS NULL OR r.Forecast=p_forecast)
          AND (p_noRetro=0 OR r.ParentJobId IS NULL)
          AND (p_excludeParentJobId IS NULL OR r.ParentJobId IS NULL OR r.ParentJobId<>p_excludeParentJobId)
    )
    SELECT r.* FROM WageTypeCustomResult r INNER JOIN Winners w ON w.Id=r.Id WHERE w.RowNumber=1;
END$$

-- -----------------------------------------------------------------------------
-- GetConsolidatedPayrunResults
-- -----------------------------------------------------------------------------
DROP PROCEDURE IF EXISTS GetConsolidatedPayrunResults$$
CREATE PROCEDURE GetConsolidatedPayrunResults(
    IN p_tenantId          INT,
    IN p_employeeId        INT,
    IN p_divisionId        INT,
    IN p_names             VARCHAR(4000),
    IN p_periodStartHashes VARCHAR(4000),
    IN p_jobStatus         INT,
    IN p_forecast          VARCHAR(128),
    IN p_evaluationDate    DATETIME(6),
    IN p_noRetro           TINYINT(1),
    IN p_excludeParentJobId INT
)
BEGIN
    DECLARE v_name VARCHAR(128); DECLARE v_nameCount INT;
    DECLARE v_startHash INT;     DECLARE v_startHashCount INT;
    SELECT COUNT(*) INTO v_nameCount      FROM JSON_TABLE(p_names,            '$[*]' COLUMNS (val VARCHAR(128) PATH '$')) AS jt;
    SELECT COUNT(*) INTO v_startHashCount FROM JSON_TABLE(p_periodStartHashes,'$[*]' COLUMNS (val VARCHAR(20)  PATH '$')) AS jt;
    IF v_nameCount=1      THEN SELECT jt.val          INTO v_name      FROM JSON_TABLE(p_names,'$[*]' COLUMNS (val VARCHAR(128) PATH '$'))  AS jt LIMIT 1; END IF;
    IF v_startHashCount=1 THEN SELECT CAST(jt.val AS INT) INTO v_startHash FROM JSON_TABLE(p_periodStartHashes,'$[*]' COLUMNS (val VARCHAR(20) PATH '$')) AS jt LIMIT 1; END IF;

    WITH Winners AS (
        SELECT r.Id, ROW_NUMBER() OVER (PARTITION BY r.Name, r.Start ORDER BY r.Created DESC, r.Id DESC) AS RowNumber
        FROM PayrunResult r
        WHERE r.TenantId=p_tenantId AND r.EmployeeId=p_employeeId
          AND ((v_startHashCount=1 AND r.StartHash=v_startHash) OR (v_startHashCount>1 AND r.StartHash IN (SELECT CAST(jt.val AS INT) FROM JSON_TABLE(p_periodStartHashes,'$[*]' COLUMNS (val VARCHAR(20) PATH '$')) AS jt)))
          AND (p_divisionId IS NULL OR r.DivisionId=p_divisionId)
          AND (p_names IS NULL OR v_nameCount=0 OR (v_nameCount=1 AND r.Name=v_name) OR (v_nameCount>1 AND r.Name IN (SELECT jt.val FROM JSON_TABLE(p_names,'$[*]' COLUMNS (val VARCHAR(128) PATH '$')) AS jt)))
          AND (p_evaluationDate IS NULL OR r.Created<=p_evaluationDate)
          AND (p_jobStatus IS NULL OR r.PayrunJobId IN (SELECT pj.Id FROM PayrunJob pj WHERE pj.JobStatus & p_jobStatus=pj.JobStatus))
          AND (r.Forecast IS NULL OR r.Forecast=p_forecast)
          AND (p_noRetro=0 OR r.ParentJobId IS NULL)
          AND (p_excludeParentJobId IS NULL OR r.ParentJobId IS NULL OR r.ParentJobId<>p_excludeParentJobId)
    )
    SELECT r.* FROM PayrunResult r INNER JOIN Winners w ON w.Id=r.Id WHERE w.RowNumber=1;
END$$

-- -----------------------------------------------------------------------------
-- GetWageTypeResults
-- TOP(100) PERCENT ORDER BY -> MySQL: ORDER BY without LIMIT (optimizer hint removed)
-- -----------------------------------------------------------------------------
DROP PROCEDURE IF EXISTS GetWageTypeResults$$
CREATE PROCEDURE GetWageTypeResults(
    IN p_tenantId          INT,
    IN p_employeeId        INT,
    IN p_divisionId        INT,
    IN p_payrunJobId       INT,
    IN p_parentPayrunJobId INT,
    IN p_wageTypeNumbers   VARCHAR(4000),
    IN p_periodStart       DATETIME(6),
    IN p_periodEnd         DATETIME(6),
    IN p_jobStatus         INT,
    IN p_forecast          VARCHAR(128),
    IN p_evaluationDate    DATETIME(6)
)
BEGIN
    DECLARE v_wt DECIMAL(28,6); DECLARE v_wtCount INT;
    SELECT COUNT(*) INTO v_wtCount FROM JSON_TABLE(p_wageTypeNumbers,'$[*]' COLUMNS (val VARCHAR(50) PATH '$')) AS jt;
    IF v_wtCount=1 THEN SELECT CAST(jt.val AS DECIMAL(28,6)) INTO v_wt FROM JSON_TABLE(p_wageTypeNumbers,'$[*]' COLUMNS (val VARCHAR(50) PATH '$')) AS jt LIMIT 1; END IF;

    SELECT wtr.*
    FROM WageTypeResult wtr
    WHERE wtr.TenantId=p_tenantId AND wtr.EmployeeId=p_employeeId
      AND (p_divisionId IS NULL OR wtr.DivisionId=p_divisionId)
      AND (p_payrunJobId IS NULL OR wtr.PayrunJobId=p_payrunJobId)
      AND (p_parentPayrunJobId IS NULL OR wtr.ParentJobId=p_parentPayrunJobId)
      AND (p_wageTypeNumbers IS NULL OR (v_wtCount=1 AND wtr.WageTypeNumber=v_wt)
           OR (v_wtCount>1 AND wtr.WageTypeNumber IN (SELECT CAST(jt.val AS DECIMAL(28,6)) FROM JSON_TABLE(p_wageTypeNumbers,'$[*]' COLUMNS (val VARCHAR(50) PATH '$')) AS jt)))
      AND (p_periodStart IS NULL OR wtr.Start BETWEEN p_periodStart AND p_periodEnd)
      AND (p_jobStatus IS NULL OR wtr.PayrunJobId IN (SELECT pj.Id FROM PayrunJob pj WHERE pj.JobStatus & p_jobStatus=pj.JobStatus))
      AND (wtr.Forecast IS NULL OR wtr.Forecast=p_forecast)
      AND (p_evaluationDate IS NULL OR wtr.Created<=p_evaluationDate)
    ORDER BY wtr.Created;
END$$

-- -----------------------------------------------------------------------------
-- GetWageTypeCustomResults
-- -----------------------------------------------------------------------------
DROP PROCEDURE IF EXISTS GetWageTypeCustomResults$$
CREATE PROCEDURE GetWageTypeCustomResults(
    IN p_tenantId          INT,
    IN p_employeeId        INT,
    IN p_divisionId        INT,
    IN p_payrunJobId       INT,
    IN p_parentPayrunJobId INT,
    IN p_wageTypeNumbers   VARCHAR(4000),
    IN p_periodStart       DATETIME(6),
    IN p_periodEnd         DATETIME(6),
    IN p_jobStatus         INT,
    IN p_forecast          VARCHAR(128),
    IN p_evaluationDate    DATETIME(6)
)
BEGIN
    DECLARE v_wt DECIMAL(28,6); DECLARE v_wtCount INT;
    SELECT COUNT(*) INTO v_wtCount FROM JSON_TABLE(p_wageTypeNumbers,'$[*]' COLUMNS (val VARCHAR(50) PATH '$')) AS jt;
    IF v_wtCount=1 THEN SELECT CAST(jt.val AS DECIMAL(28,6)) INTO v_wt FROM JSON_TABLE(p_wageTypeNumbers,'$[*]' COLUMNS (val VARCHAR(50) PATH '$')) AS jt LIMIT 1; END IF;

    SELECT wtcr.*
    FROM WageTypeCustomResult wtcr
    WHERE wtcr.TenantId=p_tenantId AND wtcr.EmployeeId=p_employeeId
      AND (p_divisionId IS NULL OR wtcr.DivisionId=p_divisionId)
      AND (p_payrunJobId IS NULL OR wtcr.PayrunJobId=p_payrunJobId)
      AND (p_parentPayrunJobId IS NULL OR wtcr.ParentJobId=p_parentPayrunJobId)
      AND (p_wageTypeNumbers IS NULL OR (v_wtCount=1 AND wtcr.WageTypeNumber=v_wt)
           OR (v_wtCount>1 AND wtcr.WageTypeNumber IN (SELECT CAST(jt.val AS DECIMAL(28,6)) FROM JSON_TABLE(p_wageTypeNumbers,'$[*]' COLUMNS (val VARCHAR(50) PATH '$')) AS jt)))
      AND (p_periodStart IS NULL OR wtcr.Start BETWEEN p_periodStart AND p_periodEnd)
      AND (p_jobStatus IS NULL OR wtcr.PayrunJobId IN (SELECT pj.Id FROM PayrunJob pj WHERE pj.JobStatus & p_jobStatus=pj.JobStatus))
      AND (wtcr.Forecast IS NULL OR wtcr.Forecast=p_forecast)
      AND (p_evaluationDate IS NULL OR wtcr.Created<=p_evaluationDate)
    ORDER BY wtcr.Created;
END$$

-- -----------------------------------------------------------------------------
-- GetCollectorResults
-- -----------------------------------------------------------------------------
DROP PROCEDURE IF EXISTS GetCollectorResults$$
CREATE PROCEDURE GetCollectorResults(
    IN p_tenantId             INT,
    IN p_employeeId           INT,
    IN p_divisionId           INT,
    IN p_payrunJobId          INT,
    IN p_parentPayrunJobId    INT,
    IN p_collectorNameHashes  VARCHAR(4000),
    IN p_periodStart          DATETIME(6),
    IN p_periodEnd            DATETIME(6),
    IN p_jobStatus            INT,
    IN p_forecast             VARCHAR(128),
    IN p_evaluationDate       DATETIME(6)
)
BEGIN
    DECLARE v_hash INT; DECLARE v_hashCount INT;
    SELECT COUNT(*) INTO v_hashCount FROM JSON_TABLE(p_collectorNameHashes,'$[*]' COLUMNS (val VARCHAR(20) PATH '$')) AS jt;
    IF v_hashCount=1 THEN SELECT CAST(jt.val AS INT) INTO v_hash FROM JSON_TABLE(p_collectorNameHashes,'$[*]' COLUMNS (val VARCHAR(20) PATH '$')) AS jt LIMIT 1; END IF;

    SELECT cr.*
    FROM CollectorResult cr
    WHERE cr.TenantId=p_tenantId AND cr.EmployeeId=p_employeeId
      AND (p_divisionId IS NULL OR cr.DivisionId=p_divisionId)
      AND (p_payrunJobId IS NULL OR cr.PayrunJobId=p_payrunJobId)
      AND (p_parentPayrunJobId IS NULL OR cr.ParentJobId=p_parentPayrunJobId)
      AND (p_collectorNameHashes IS NULL OR (v_hashCount=1 AND cr.CollectorNameHash=v_hash)
           OR (v_hashCount>1 AND cr.CollectorNameHash IN (SELECT CAST(jt.val AS INT) FROM JSON_TABLE(p_collectorNameHashes,'$[*]' COLUMNS (val VARCHAR(20) PATH '$')) AS jt)))
      AND (p_periodStart IS NULL OR cr.Start BETWEEN p_periodStart AND p_periodEnd)
      AND (p_jobStatus IS NULL OR cr.PayrunJobId IN (SELECT pj.Id FROM PayrunJob pj WHERE pj.JobStatus & p_jobStatus=pj.JobStatus))
      AND (cr.Forecast IS NULL OR cr.Forecast=p_forecast)
      AND (p_evaluationDate IS NULL OR cr.Created<=p_evaluationDate)
    ORDER BY cr.Created;
END$$

-- -----------------------------------------------------------------------------
-- GetCollectorCustomResults
-- -----------------------------------------------------------------------------
DROP PROCEDURE IF EXISTS GetCollectorCustomResults$$
CREATE PROCEDURE GetCollectorCustomResults(
    IN p_tenantId             INT,
    IN p_employeeId           INT,
    IN p_divisionId           INT,
    IN p_payrunJobId          INT,
    IN p_parentPayrunJobId    INT,
    IN p_collectorNameHashes  VARCHAR(4000),
    IN p_periodStart          DATETIME(6),
    IN p_periodEnd            DATETIME(6),
    IN p_jobStatus            INT,
    IN p_forecast             VARCHAR(128),
    IN p_evaluationDate       DATETIME(6)
)
BEGIN
    DECLARE v_hash INT; DECLARE v_hashCount INT;
    SELECT COUNT(*) INTO v_hashCount FROM JSON_TABLE(p_collectorNameHashes,'$[*]' COLUMNS (val VARCHAR(20) PATH '$')) AS jt;
    IF v_hashCount=1 THEN SELECT CAST(jt.val AS INT) INTO v_hash FROM JSON_TABLE(p_collectorNameHashes,'$[*]' COLUMNS (val VARCHAR(20) PATH '$')) AS jt LIMIT 1; END IF;

    SELECT ccr.*
    FROM CollectorCustomResult ccr
    WHERE ccr.TenantId=p_tenantId AND ccr.EmployeeId=p_employeeId
      AND (p_divisionId IS NULL OR ccr.DivisionId=p_divisionId)
      AND (p_payrunJobId IS NULL OR ccr.PayrunJobId=p_payrunJobId)
      AND (p_parentPayrunJobId IS NULL OR ccr.ParentJobId=p_parentPayrunJobId)
      AND (p_collectorNameHashes IS NULL OR (v_hashCount=1 AND ccr.CollectorNameHash=v_hash)
           OR (v_hashCount>1 AND ccr.CollectorNameHash IN (SELECT CAST(jt.val AS INT) FROM JSON_TABLE(p_collectorNameHashes,'$[*]' COLUMNS (val VARCHAR(20) PATH '$')) AS jt)))
      AND (p_periodStart IS NULL OR ccr.Start BETWEEN p_periodStart AND p_periodEnd)
      AND (p_jobStatus IS NULL OR ccr.PayrunJobId IN (SELECT pj.Id FROM PayrunJob pj WHERE pj.JobStatus & p_jobStatus=pj.JobStatus))
      AND (ccr.Forecast IS NULL OR ccr.Forecast=p_forecast)
      AND (p_evaluationDate IS NULL OR ccr.Created<=p_evaluationDate)
    ORDER BY ccr.Created;
END$$

-- -----------------------------------------------------------------------------
-- GetEmployeeCaseValuesByTenant (new in 0.9.6)
-- -----------------------------------------------------------------------------
DROP PROCEDURE IF EXISTS GetEmployeeCaseValuesByTenant$$
CREATE PROCEDURE GetEmployeeCaseValuesByTenant(
    IN p_tenantId       INT,
    IN p_valueDate      DATETIME(6),
    IN p_evaluationDate DATETIME(6),
    IN p_fieldNames     LONGTEXT,
    IN p_forecast       VARCHAR(128)
)
BEGIN
    SELECT ecv.Id, ecv.Status, ecv.Created, ecv.Updated,
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
              SELECT jt.val FROM JSON_TABLE(p_fieldNames,'$[*]' COLUMNS (val VARCHAR(128) PATH '$')) AS jt
          )
      )
    ORDER BY ecv.EmployeeId ASC, ecv.CaseFieldName ASC, ecv.Created DESC;
END$$

DELIMITER ;

-- =============================================================================
-- VERSION RECORD
-- =============================================================================

INSERT INTO `Version` (Created, MajorVersion, MinorVersion, SubVersion, Owner, Description)
VALUES (NOW(6), 0, 9, 6, CURRENT_USER(), 'Payroll Engine: Migration v0.9.5 -> v0.9.6 (MySQL)');

SELECT 'PayrollEngine MySQL schema updated to v0.9.6 successfully.' AS Result;
