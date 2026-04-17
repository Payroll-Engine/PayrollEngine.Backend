-- =============================================================================
-- Update-Model.mysql.sql
-- Migration: PayrollEngine v0.9.7 → v1.0.0 (MySQL)
-- =============================================================================

-- =============================================================================
-- VERSION CHECK
-- Guard: abort if the schema is not at version 0.9.7
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

    IF v_major <> 0 OR v_minor <> 9 OR v_sub <> 7 THEN
        SET v_msg = CONCAT('Version mismatch: expected 0.9.7, found ',
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

-- Payroll: consolidate individual ClusterSetXxx columns into single ClusterSet JSON column
ALTER TABLE Payroll ADD COLUMN ClusterSet JSON NULL;

-- migrate existing data into the new JSON column
UPDATE Payroll
SET ClusterSet = JSON_OBJECT(
    'ClusterSetCase',           ClusterSetCase,
    'ClusterSetCaseField',      ClusterSetCaseField,
    'ClusterSetCollector',      ClusterSetCollector,
    'ClusterSetCollectorRetro', ClusterSetCollectorRetro,
    'ClusterSetWageType',       ClusterSetWageType,
    'ClusterSetWageTypeRetro',  ClusterSetWageTypeRetro,
    'ClusterSetCaseValue',      ClusterSetCaseValue,
    'ClusterSetWageTypePeriod', ClusterSetWageTypePeriod,
    'ClusterSetWageTypeLookup', ClusterSetWageTypeLookup
)
WHERE ClusterSetCase           IS NOT NULL
   OR ClusterSetCaseField      IS NOT NULL
   OR ClusterSetCollector      IS NOT NULL
   OR ClusterSetCollectorRetro IS NOT NULL
   OR ClusterSetWageType       IS NOT NULL
   OR ClusterSetWageTypeRetro  IS NOT NULL
   OR ClusterSetCaseValue      IS NOT NULL
   OR ClusterSetWageTypePeriod IS NOT NULL
   OR ClusterSetWageTypeLookup IS NOT NULL;

-- drop individual columns (now superseded by ClusterSet JSON)
ALTER TABLE Payroll
    DROP COLUMN ClusterSetCase,
    DROP COLUMN ClusterSetCaseField,
    DROP COLUMN ClusterSetCollector,
    DROP COLUMN ClusterSetCollectorRetro,
    DROP COLUMN ClusterSetWageType,
    DROP COLUMN ClusterSetWageTypeRetro,
    DROP COLUMN ClusterSetCaseValue,
    DROP COLUMN ClusterSetWageTypePeriod,
    DROP COLUMN ClusterSetWageTypeLookup;

-- =============================================================================
-- VERSION SET
-- =============================================================================

INSERT INTO `Version` (Created, MajorVersion, MinorVersion, SubVersion, Owner, Description)
VALUES (NOW(6), 1, 0, 0, CURRENT_USER(), 'Payroll Engine: Migration v0.9.7 -> v1.0.0 (MySQL)');

SELECT CONCAT('PayrollEngine MySQL schema updated to v1.0.0 successfully.') AS Result;
