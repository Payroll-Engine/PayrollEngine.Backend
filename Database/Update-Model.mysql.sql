-- =============================================================================
-- Update-Model.mysql.sql
-- Migration: PayrollEngine v0.9.6 → v0.9.7 (MySQL)
-- =============================================================================

-- =============================================================================
-- VERSION CHECK
-- Guard: abort if the schema is not at version 0.9.6
-- =============================================================================

DROP PROCEDURE IF EXISTS _PE_VersionCheck;

DELIMITER $$

CREATE PROCEDURE _PE_VersionCheck()
BEGIN
    DECLARE v_major INT DEFAULT NULL;
    DECLARE v_minor INT DEFAULT NULL;
    DECLARE v_sub   INT DEFAULT NULL;
    DECLARE v_msg   VARCHAR(200);

    IF NOT EXISTS (
        SELECT 1 FROM information_schema.TABLES
        WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'Version'
    ) THEN
        SIGNAL SQLSTATE '45000'
            SET MESSAGE_TEXT = 'Schema not found: Version table does not exist. Run Create-Model.mysql.sql first.';
    END IF;

    SELECT MajorVersion, MinorVersion, SubVersion
    INTO v_major, v_minor, v_sub
    FROM `Version`
    ORDER BY MajorVersion DESC, MinorVersion DESC, SubVersion DESC
    LIMIT 1;

    IF v_major <> 0 OR v_minor <> 9 OR v_sub <> 6 THEN
        SET v_msg = CONCAT('Version mismatch: expected 0.9.6, found ',
                           IFNULL(v_major, -1), '.', IFNULL(v_minor, -1), '.', IFNULL(v_sub, -1));
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = v_msg;
    END IF;
END$$

DELIMITER ;

CALL _PE_VersionCheck();
DROP PROCEDURE IF EXISTS _PE_VersionCheck;

-- =============================================================================
-- TABLE CHANGES
-- =============================================================================

-- Payrun: RetroTimeType (enum) → RetroBackCycles (int)
-- -1 = unlimited (was: Anytime = 0), 0 = current cycle (was: Cycle = 1)
ALTER TABLE Payrun RENAME COLUMN RetroTimeType TO RetroBackCycles;
UPDATE Payrun SET RetroBackCycles = -1 WHERE RetroBackCycles = 0; -- Anytime → unlimited
UPDATE Payrun SET RetroBackCycles = 0  WHERE RetroBackCycles = 1; -- Cycle → current cycle

-- PayrollResult: new denormalized name fields
ALTER TABLE PayrollResult ADD COLUMN PayrollName        VARCHAR(128) NULL AFTER PayrollId;
ALTER TABLE PayrollResult ADD COLUMN PayrunName         VARCHAR(128) NULL AFTER PayrunId;
ALTER TABLE PayrollResult ADD COLUMN PayrunJobName      VARCHAR(128) NULL AFTER PayrunJobId;
ALTER TABLE PayrollResult ADD COLUMN EmployeeIdentifier VARCHAR(128) NULL AFTER EmployeeId;
ALTER TABLE PayrollResult ADD COLUMN DivisionName       VARCHAR(128) NULL AFTER DivisionId;

-- RegulationShare: new IsolationLevel column (DEFAULT 3 = TenantIsolationLevel.Write)
ALTER TABLE RegulationShare ADD COLUMN IsolationLevel INT NOT NULL DEFAULT 3;

-- =============================================================================
-- INDEX CHANGES
-- =============================================================================

CREATE INDEX IX_RegulationShare_Consumer_Provider_Level
ON RegulationShare (ConsumerTenantId ASC, ProviderRegulationId ASC, IsolationLevel ASC);

-- =============================================================================
-- STORED PROCEDURE CHANGES
-- =============================================================================

-- GetConsolidatedWageTypeResults: rewritten with StartHash-based index seek

DELIMITER $$

DROP PROCEDURE IF EXISTS GetConsolidatedWageTypeResults$$
CREATE PROCEDURE GetConsolidatedWageTypeResults(
    IN p_tenantId           INT,
    IN p_employeeId         INT,
    IN p_divisionId         INT,
    IN p_wageTypeNumbers    VARCHAR(4000),
    IN p_periodStartHashes  VARCHAR(4000),
    IN p_jobStatus          INT,
    IN p_forecast           VARCHAR(128),
    IN p_evaluationDate     DATETIME(6),
    IN p_noRetro            TINYINT(1),
    IN p_excludeParentJobId INT
)
BEGIN
    DECLARE v_wageTypeNumber  DECIMAL(28,6);
    DECLARE v_wageTypeCount   INT;
    DECLARE v_startHash       INT;
    DECLARE v_startHashCount  INT;

    SET v_wageTypeCount  = IF(p_wageTypeNumbers IS NULL,   0, JSON_LENGTH(p_wageTypeNumbers));
    SET v_startHashCount = IF(p_periodStartHashes IS NULL, 0, JSON_LENGTH(p_periodStartHashes));

    IF v_wageTypeCount = 1 THEN
        SELECT CAST(jt.val AS DECIMAL(28,6)) INTO v_wageTypeNumber
        FROM JSON_TABLE(p_wageTypeNumbers, '$[*]' COLUMNS (val VARCHAR(50) PATH '$')) AS jt LIMIT 1;
    END IF;

    IF v_startHashCount = 1 THEN
        SELECT CAST(jt.val AS SIGNED) INTO v_startHash
        FROM JSON_TABLE(p_periodStartHashes, '$[*]' COLUMNS (val VARCHAR(20) PATH '$')) AS jt LIMIT 1;
    END IF;

    WITH Winners AS (
        SELECT r.Id,
            ROW_NUMBER() OVER (
                PARTITION BY r.WageTypeNumber, r.Start
                ORDER BY r.Created DESC, r.Id DESC
            ) AS RowNumber
        FROM WageTypeResult r
        WHERE r.TenantId = p_tenantId
          AND r.EmployeeId = p_employeeId
          AND (v_startHashCount = 0 OR
               (v_startHashCount = 1 AND r.StartHash = v_startHash) OR
               (v_startHashCount > 1 AND r.StartHash IN (
                   SELECT CAST(jt.val AS SIGNED)
                   FROM JSON_TABLE(p_periodStartHashes, '$[*]' COLUMNS (val VARCHAR(20) PATH '$')) AS jt)))
          AND (p_divisionId IS NULL OR r.DivisionId = p_divisionId)
          AND (p_wageTypeNumbers IS NULL OR v_wageTypeCount = 0
               OR (v_wageTypeCount = 1 AND r.WageTypeNumber = v_wageTypeNumber)
               OR (v_wageTypeCount > 1 AND r.WageTypeNumber IN (
                   SELECT CAST(jt.val AS DECIMAL(28,6))
                   FROM JSON_TABLE(p_wageTypeNumbers, '$[*]' COLUMNS (val VARCHAR(50) PATH '$')) AS jt)))
          AND (p_evaluationDate IS NULL OR r.Created <= p_evaluationDate)
          AND (p_jobStatus IS NULL OR r.PayrunJobId IN (
                   SELECT pj.Id FROM PayrunJob pj WHERE (pj.JobStatus & p_jobStatus) = pj.JobStatus))
          AND (r.Forecast IS NULL OR r.Forecast = p_forecast)
          AND (p_noRetro = 0 OR r.ParentJobId IS NULL)
          AND (p_excludeParentJobId IS NULL OR r.ParentJobId IS NULL
               OR r.ParentJobId <> p_excludeParentJobId)
    )
    SELECT r.*
    FROM WageTypeResult r
    INNER JOIN Winners w ON w.Id = r.Id
    WHERE w.RowNumber = 1;
END$$

-- GetDerivedReports: added ReportIsolation column to SELECT

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
            ROW_NUMBER() OVER (
                PARTITION BY pl.Id, r.Name
                ORDER BY r.ValidFrom DESC, r.Created DESC
            ) AS RowNumber
        FROM PayrollLayer pl
        INNER JOIN Regulation r ON pl.RegulationName = r.Name
        WHERE r.Status = 0
          AND (r.TenantId = p_tenantId OR (
              r.SharedRegulation = 1 AND EXISTS (
                  SELECT 1 FROM RegulationShare rs
                  WHERE rs.ProviderRegulationId = r.Id
                    AND rs.ConsumerTenantId = p_tenantId
                    AND rs.IsolationLevel >= 3
              )))
          AND r.Created <= p_createdBefore
          AND (r.ValidFrom IS NULL OR r.ValidFrom <= p_regulationDate)
          AND pl.Status = 0 AND pl.PayrollId = p_payrollId
    ),
    Regulations AS (SELECT Id, Level, Priority FROM DerivedRegulations WHERE RowNumber = 1)
    SELECT
        reg.Id AS RegulationId, reg.Level, reg.Priority,
        rp.Id, rp.Status, rp.Created, rp.Updated, rp.RegulationId,
        rp.Name, rp.NameLocalizations,
        rp.Description, rp.DescriptionLocalizations,
        rp.Category, rp.Queries, rp.Relations,
        rp.AttributeMode, rp.UserType, rp.ReportIsolation,
        rp.BuildExpression, rp.StartExpression, rp.EndExpression,
        rp.ScriptHash, rp.Attributes, rp.Clusters
    FROM Report rp
    INNER JOIN Regulations reg ON rp.RegulationId = reg.Id
    WHERE rp.Status = 0
      AND rp.Created <= p_createdBefore
      AND (p_userType IS NULL OR rp.UserType <= p_userType)
      AND ((p_includeClusters IS NULL AND p_excludeClusters IS NULL)
           OR IsMatchingCluster(p_includeClusters, p_excludeClusters, rp.Clusters) = 1)
      AND (p_reportNames IS NULL
           OR LOWER(rp.Name) IN (
               SELECT LOWER(jt.val)
               FROM JSON_TABLE(p_reportNames, '$[*]' COLUMNS (val VARCHAR(128) PATH '$')) AS jt))
    ORDER BY reg.Level DESC, reg.Priority DESC;
END$$

DELIMITER ;

-- =============================================================================
-- VERSION SET
-- =============================================================================

INSERT INTO `Version` (Created, MajorVersion, MinorVersion, SubVersion, Owner, Description)
VALUES (NOW(6), 0, 9, 7, CURRENT_USER(), 'Payroll Engine: Migration v0.9.6 -> v0.9.7 (MySQL)');

SELECT CONCAT('PayrollEngine MySQL schema updated to v0.9.7 successfully.') AS Result;
