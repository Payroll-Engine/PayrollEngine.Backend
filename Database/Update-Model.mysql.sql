-- =============================================================================
-- Update-Model.mysql.sql
-- Migration: PayrollEngine v1.1.0 -> v1.2.0 (MySQL)
-- =============================================================================

-- =============================================================================
-- VERSION CHECK
-- Guard: abort if the schema is not at version 1.1.0
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

    IF v_major <> 1 OR v_minor <> 1 OR v_sub <> 0 THEN
        SET v_msg = CONCAT('Version mismatch: expected 1.1.0, found ',
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

-- Calendar: anchored week/bi-week periods, pay date offset and cycle assignment
ALTER TABLE Calendar
    ADD COLUMN PeriodAnchorDate DATETIME(6) NULL AFTER FirstDayOfWeek,
    ADD COLUMN PayDateOffset    INT         NULL AFTER PeriodAnchorDate,
    ADD COLUMN CycleAssignment  INT         NULL AFTER PayDateOffset;

-- PayrunJob: pay date (existing jobs: period end date)
ALTER TABLE PayrunJob ADD COLUMN PayDate DATETIME(6) NULL AFTER EvaluationDate;
UPDATE PayrunJob SET PayDate = CAST(DATE(PeriodEnd) AS DATETIME(6));
ALTER TABLE PayrunJob MODIFY COLUMN PayDate DATETIME(6) NOT NULL;

-- Results: denormalized payrun job pay date
ALTER TABLE WageTypeResult        ADD COLUMN PayDate DATETIME(6) NULL AFTER Attributes;
ALTER TABLE WageTypeCustomResult  ADD COLUMN PayDate DATETIME(6) NULL AFTER Attributes;
ALTER TABLE CollectorResult       ADD COLUMN PayDate DATETIME(6) NULL AFTER Attributes;
ALTER TABLE CollectorCustomResult ADD COLUMN PayDate DATETIME(6) NULL AFTER Attributes;
ALTER TABLE PayrunResult          ADD COLUMN PayDate DATETIME(6) NULL AFTER Attributes;

UPDATE WageTypeResult r        INNER JOIN PayrunJob j ON j.Id = r.PayrunJobId SET r.PayDate = j.PayDate;
UPDATE WageTypeCustomResult r  INNER JOIN PayrunJob j ON j.Id = r.PayrunJobId SET r.PayDate = j.PayDate;
UPDATE CollectorResult r       INNER JOIN PayrunJob j ON j.Id = r.PayrunJobId SET r.PayDate = j.PayDate;
UPDATE CollectorCustomResult r INNER JOIN PayrunJob j ON j.Id = r.PayrunJobId SET r.PayDate = j.PayDate;
UPDATE PayrunResult r          INNER JOIN PayrunJob j ON j.Id = r.PayrunJobId SET r.PayDate = j.PayDate;

-- results without job: result end date
UPDATE WageTypeResult        SET PayDate = CAST(DATE(`End`) AS DATETIME(6)) WHERE PayDate IS NULL;
UPDATE WageTypeCustomResult  SET PayDate = CAST(DATE(`End`) AS DATETIME(6)) WHERE PayDate IS NULL;
UPDATE CollectorResult       SET PayDate = CAST(DATE(`End`) AS DATETIME(6)) WHERE PayDate IS NULL;
UPDATE CollectorCustomResult SET PayDate = CAST(DATE(`End`) AS DATETIME(6)) WHERE PayDate IS NULL;
UPDATE PayrunResult          SET PayDate = CAST(DATE(`End`) AS DATETIME(6)) WHERE PayDate IS NULL;

ALTER TABLE WageTypeResult        MODIFY COLUMN PayDate DATETIME(6) NOT NULL;
ALTER TABLE WageTypeCustomResult  MODIFY COLUMN PayDate DATETIME(6) NOT NULL;
ALTER TABLE CollectorResult       MODIFY COLUMN PayDate DATETIME(6) NOT NULL;
ALTER TABLE CollectorCustomResult MODIFY COLUMN PayDate DATETIME(6) NOT NULL;
ALTER TABLE PayrunResult          MODIFY COLUMN PayDate DATETIME(6) NOT NULL;

-- Webhook: signing secret (stored encrypted)
ALTER TABLE Webhook ADD COLUMN Secret VARCHAR(1024) NULL AFTER Attributes;

-- =============================================================================
-- INDEX CHANGES
-- =============================================================================

-- (none in this release)

-- =============================================================================
-- STORED PROCEDURE CHANGES
-- =============================================================================

-- (none in this release: result procedures select all result columns)

-- =============================================================================
-- VERSION SET
-- =============================================================================

INSERT INTO `Version` (Created, MajorVersion, MinorVersion, SubVersion, Owner, Description)
VALUES (NOW(6), 1, 2, 0, CURRENT_USER(), 'Payroll Engine: Migration v1.1.0 -> v1.2.0 (MySQL)');

SELECT CONCAT('PayrollEngine MySQL schema updated to v1.2.0 successfully.') AS Result;
