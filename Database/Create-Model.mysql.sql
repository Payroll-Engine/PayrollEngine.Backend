-- =============================================================================
-- Create-Model.mysql.sql
-- Creates the PayrollEngine database for MySQL 8.0+ (8.4 LTS recommended).
--
-- Schema version: 0.9.6
--
-- Type mapping from T-SQL:
--   [nvarchar](N)   -> VARCHAR(N)
--   [nvarchar](max) -> LONGTEXT
--   [varbinary](max)-> LONGBLOB
--   [datetime2](7)  -> DATETIME(6)
--   IDENTITY(1,1)   -> AUTO_INCREMENT
--   [bit]           -> TINYINT(1)
--   [decimal](28,6) -> DECIMAL(28,6)
--   [bigint]        -> BIGINT
--
-- Reserved word quoting: User, Key, Order, Schema, Binary
-- Note: Binary is a reserved keyword in MySQL 8.4 -- must be backtick-quoted
-- =============================================================================

CREATE DATABASE IF NOT EXISTS PayrollEngine
    CHARACTER SET utf8mb4
    COLLATE utf8mb4_unicode_ci;

USE PayrollEngine;

SET GLOBAL log_bin_trust_function_creators = 1;

-- =============================================================================
-- TABLES (alphabetical order, dependencies first)
-- =============================================================================

-- -----------------------------------------------------------------------------
-- Calendar
-- -----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS Calendar (
    Id                  INT           NOT NULL AUTO_INCREMENT,
    Status              INT           NOT NULL,
    Created             DATETIME(6)   NOT NULL,
    Updated             DATETIME(6)   NOT NULL,
    TenantId            INT           NOT NULL,
    Name                VARCHAR(128)  NOT NULL,
    NameLocalizations   LONGTEXT      NULL,
    CycleTimeUnit       INT           NOT NULL,
    PeriodTimeUnit      INT           NOT NULL,
    TimeMap             INT           NOT NULL,
    FirstMonthOfYear    INT           NULL,
    PeriodDayCount      DECIMAL(28,6) NULL,
    YearWeekRule        INT           NULL,
    FirstDayOfWeek      INT           NULL,
    WeekMode            INT           NOT NULL,
    WorkMonday          TINYINT(1)    NULL,
    WorkTuesday         TINYINT(1)    NULL,
    WorkWednesday       TINYINT(1)    NULL,
    WorkThursday        TINYINT(1)    NULL,
    WorkFriday          TINYINT(1)    NULL,
    WorkSaturday        TINYINT(1)    NULL,
    WorkSunday          TINYINT(1)    NULL,
    Attributes          LONGTEXT      NULL,
    PRIMARY KEY (Id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- -----------------------------------------------------------------------------
-- Case
-- -----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS `Case` (
    Id                          INT           NOT NULL AUTO_INCREMENT,
    Status                      INT           NOT NULL,
    Created                     DATETIME(6)   NOT NULL,
    Updated                     DATETIME(6)   NOT NULL,
    RegulationId                INT           NOT NULL,
    CaseType                    INT           NOT NULL,
    Name                        VARCHAR(128)  NOT NULL,
    NameLocalizations           LONGTEXT      NULL,
    NameSynonyms                LONGTEXT      NULL,
    Description                 LONGTEXT      NULL,
    DescriptionLocalizations    LONGTEXT      NULL,
    DefaultReason               LONGTEXT      NULL,
    DefaultReasonLocalizations  LONGTEXT      NULL,
    BaseCase                    VARCHAR(128)  NULL,
    BaseCaseFields              LONGTEXT      NULL,
    OverrideType                INT           NOT NULL,
    CancellationType            INT           NOT NULL,
    Hidden                      TINYINT(1)    NOT NULL,
    AvailableExpression         LONGTEXT      NULL,
    BuildExpression             LONGTEXT      NULL,
    ValidateExpression          LONGTEXT      NULL,
    Lookups                     LONGTEXT      NULL,
    Slots                       LONGTEXT      NULL,
    Script                      LONGTEXT      NULL,
    ScriptVersion               VARCHAR(128)  NULL,
    `Binary`                    LONGBLOB      NULL,
    ScriptHash                  INT           NULL,
    AvailableActions            LONGTEXT      NULL,
    BuildActions                LONGTEXT      NULL,
    ValidateActions             LONGTEXT      NULL,
    Attributes                  LONGTEXT      NULL,
    Clusters                    LONGTEXT      NULL,
    PRIMARY KEY (Id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- -----------------------------------------------------------------------------
-- CaseAudit
-- -----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS CaseAudit (
    Id                          INT           NOT NULL AUTO_INCREMENT,
    Status                      INT           NOT NULL,
    Created                     DATETIME(6)   NOT NULL,
    Updated                     DATETIME(6)   NOT NULL,
    CaseId                      INT           NOT NULL,
    CaseChangeId                INT           NULL,
    CaseType                    INT           NOT NULL,
    Name                        VARCHAR(128)  NOT NULL,
    NameLocalizations           LONGTEXT      NULL,
    NameSynonyms                LONGTEXT      NULL,
    Description                 LONGTEXT      NULL,
    DescriptionLocalizations    LONGTEXT      NULL,
    DefaultReason               LONGTEXT      NULL,
    DefaultReasonLocalizations  LONGTEXT      NULL,
    BaseCase                    VARCHAR(128)  NULL,
    BaseCaseFields              LONGTEXT      NULL,
    OverrideType                INT           NOT NULL,
    CancellationType            INT           NOT NULL,
    Hidden                      TINYINT(1)    NOT NULL,
    AvailableExpression         LONGTEXT      NULL,
    BuildExpression             LONGTEXT      NULL,
    ValidateExpression          LONGTEXT      NULL,
    Lookups                     LONGTEXT      NULL,
    Slots                       LONGTEXT      NULL,
    Script                      LONGTEXT      NULL,
    ScriptVersion               VARCHAR(128)  NULL,
    `Binary`                    LONGBLOB      NULL,
    ScriptHash                  INT           NULL,
    AvailableActions            LONGTEXT      NULL,
    BuildActions                LONGTEXT      NULL,
    ValidateActions             LONGTEXT      NULL,
    Attributes                  LONGTEXT      NULL,
    Clusters                    LONGTEXT      NULL,
    PRIMARY KEY (Id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- -----------------------------------------------------------------------------
-- CaseField
-- -----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS CaseField (
    Id                       INT           NOT NULL AUTO_INCREMENT,
    Status                   INT           NOT NULL,
    Created                  DATETIME(6)   NOT NULL,
    Updated                  DATETIME(6)   NOT NULL,
    Name                     VARCHAR(128)  NOT NULL,
    NameLocalizations        LONGTEXT      NULL,
    Description              LONGTEXT      NULL,
    DescriptionLocalizations LONGTEXT      NULL,
    CaseId                   INT           NOT NULL,
    ValueType                INT           NOT NULL,
    ValueScope               INT           NOT NULL,
    StartDateType            INT           NOT NULL,
    EndDateType              INT           NOT NULL,
    EndMandatory             TINYINT(1)    NOT NULL,
    DefaultStart             VARCHAR(128)  NULL,
    DefaultEnd               VARCHAR(128)  NULL,
    DefaultValue             LONGTEXT      NULL,
    LookupSettings           LONGTEXT      NULL,
    TimeType                 INT           NOT NULL,
    TimeUnit                 INT           NOT NULL,
    Culture                  VARCHAR(128)  NULL,
    PeriodAggregation        INT           NOT NULL,
    OverrideType             INT           NOT NULL,
    CancellationMode         INT           NOT NULL,
    ValueCreationMode        INT           NOT NULL,
    ValueMandatory           TINYINT(1)    NOT NULL,
    `Order`                  INT           NOT NULL,
    Tags                     LONGTEXT      NULL,
    Clusters                 LONGTEXT      NULL,
    Attributes               LONGTEXT      NULL,
    ValueAttributes          LONGTEXT      NULL,
    PRIMARY KEY (Id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- -----------------------------------------------------------------------------
-- CaseFieldAudit
-- -----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS CaseFieldAudit (
    Id                       INT           NOT NULL AUTO_INCREMENT,
    Status                   INT           NOT NULL,
    Created                  DATETIME(6)   NOT NULL,
    Updated                  DATETIME(6)   NOT NULL,
    CaseFieldId              INT           NOT NULL,
    ValueType                INT           NOT NULL,
    ValueScope               INT           NOT NULL,
    Name                     VARCHAR(128)  NOT NULL,
    NameLocalizations        LONGTEXT      NULL,
    Description              LONGTEXT      NULL,
    DescriptionLocalizations LONGTEXT      NULL,
    StartDateType            INT           NOT NULL,
    EndDateType              INT           NOT NULL,
    EndMandatory             TINYINT(1)    NOT NULL,
    DefaultStart             VARCHAR(128)  NULL,
    DefaultEnd               VARCHAR(128)  NULL,
    DefaultValue             LONGTEXT      NULL,
    LookupSettings           LONGTEXT      NULL,
    TimeType                 INT           NOT NULL,
    TimeUnit                 INT           NOT NULL,
    Culture                  VARCHAR(128)  NULL,
    PeriodAggregation        INT           NOT NULL,
    OverrideType             INT           NOT NULL,
    CancellationMode         INT           NOT NULL,
    ValueCreationMode        INT           NOT NULL,
    ValueMandatory           TINYINT(1)    NOT NULL,
    `Order`                  INT           NOT NULL,
    Tags                     LONGTEXT      NULL,
    Clusters                 LONGTEXT      NULL,
    Attributes               LONGTEXT      NULL,
    ValueAttributes          LONGTEXT      NULL,
    PRIMARY KEY (Id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- -----------------------------------------------------------------------------
-- CaseRelation
-- -----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS CaseRelation (
    Id                              INT           NOT NULL AUTO_INCREMENT,
    Status                          INT           NOT NULL,
    Created                         DATETIME(6)   NOT NULL,
    Updated                         DATETIME(6)   NOT NULL,
    RegulationId                    INT           NOT NULL,
    SourceCaseName                  VARCHAR(128)  NOT NULL,
    SourceCaseNameLocalizations     LONGTEXT      NULL,
    SourceCaseSlot                  VARCHAR(128)  NULL,
    SourceCaseSlotLocalizations     LONGTEXT      NULL,
    TargetCaseName                  VARCHAR(128)  NOT NULL,
    TargetCaseNameLocalizations     LONGTEXT      NULL,
    TargetCaseSlot                  VARCHAR(128)  NULL,
    TargetCaseSlotLocalizations     LONGTEXT      NULL,
    RelationHash                    INT           NOT NULL,
    BuildExpression                 LONGTEXT      NULL,
    ValidateExpression              LONGTEXT      NULL,
    OverrideType                    INT           NOT NULL,
    `Order`                         INT           NOT NULL,
    Script                          LONGTEXT      NULL,
    ScriptVersion                   VARCHAR(128)  NULL,
    `Binary`                        LONGBLOB      NULL,
    ScriptHash                      INT           NULL,
    BuildActions                    LONGTEXT      NULL,
    ValidateActions                 LONGTEXT      NULL,
    Attributes                      LONGTEXT      NULL,
    Clusters                        LONGTEXT      NULL,
    PRIMARY KEY (Id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- -----------------------------------------------------------------------------
-- CaseRelationAudit
-- -----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS CaseRelationAudit (
    Id                              INT           NOT NULL AUTO_INCREMENT,
    Status                          INT           NOT NULL,
    Created                         DATETIME(6)   NOT NULL,
    Updated                         DATETIME(6)   NOT NULL,
    CaseRelationId                  INT           NOT NULL,
    SourceCaseName                  VARCHAR(128)  NOT NULL,
    SourceCaseNameLocalizations     LONGTEXT      NULL,
    SourceCaseSlot                  VARCHAR(128)  NULL,
    SourceCaseSlotLocalizations     LONGTEXT      NULL,
    TargetCaseName                  VARCHAR(128)  NOT NULL,
    TargetCaseNameLocalizations     LONGTEXT      NULL,
    TargetCaseSlot                  VARCHAR(128)  NULL,
    TargetCaseSlotLocalizations     LONGTEXT      NULL,
    RelationHash                    INT           NOT NULL,
    BuildExpression                 LONGTEXT      NULL,
    ValidateExpression              LONGTEXT      NULL,
    OverrideType                    INT           NOT NULL,
    `Order`                         INT           NOT NULL,
    Script                          LONGTEXT      NULL,
    ScriptVersion                   VARCHAR(128)  NULL,
    `Binary`                        LONGBLOB      NULL,
    ScriptHash                      INT           NULL,
    BuildActions                    LONGTEXT      NULL,
    ValidateActions                 LONGTEXT      NULL,
    Attributes                      LONGTEXT      NULL,
    Clusters                        LONGTEXT      NULL,
    PRIMARY KEY (Id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- -----------------------------------------------------------------------------
-- Collector
-- -----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS Collector (
    Id                  INT           NOT NULL AUTO_INCREMENT,
    Status              INT           NOT NULL,
    Created             DATETIME(6)   NOT NULL,
    Updated             DATETIME(6)   NOT NULL,
    CollectMode         INT           NOT NULL,
    Negated             TINYINT(1)    NOT NULL,
    RegulationId        INT           NOT NULL,
    Name                VARCHAR(128)  NOT NULL,
    NameLocalizations   LONGTEXT      NULL,
    OverrideType        INT           NOT NULL,
    ValueType           INT           NOT NULL,
    Culture             VARCHAR(128)  NULL,
    CollectorGroups     LONGTEXT      NULL,
    StartExpression     LONGTEXT      NULL,
    ApplyExpression     LONGTEXT      NULL,
    EndExpression       LONGTEXT      NULL,
    StartActions        LONGTEXT      NULL,
    ApplyActions        LONGTEXT      NULL,
    EndActions          LONGTEXT      NULL,
    Threshold           DECIMAL(28,6) NULL,
    MinResult           DECIMAL(28,6) NULL,
    MaxResult           DECIMAL(28,6) NULL,
    Script              LONGTEXT      NULL,
    ScriptVersion       VARCHAR(128)  NULL,
    `Binary`            LONGBLOB      NULL,
    ScriptHash          INT           NULL,
    Attributes          LONGTEXT      NULL,
    Clusters            LONGTEXT      NULL,
    PRIMARY KEY (Id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- -----------------------------------------------------------------------------
-- CollectorAudit
-- -----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS CollectorAudit (
    Id                  INT           NOT NULL AUTO_INCREMENT,
    Status              INT           NOT NULL,
    Created             DATETIME(6)   NOT NULL,
    Updated             DATETIME(6)   NOT NULL,
    CollectorId         INT           NOT NULL,
    CollectMode         INT           NOT NULL,
    Negated             TINYINT(1)    NOT NULL,
    Name                VARCHAR(128)  NOT NULL,
    NameLocalizations   LONGTEXT      NULL,
    OverrideType        INT           NOT NULL,
    ValueType           INT           NOT NULL,
    Culture             VARCHAR(128)  NULL,
    CollectorGroups     LONGTEXT      NULL,
    StartExpression     LONGTEXT      NULL,
    ApplyExpression     LONGTEXT      NULL,
    EndExpression       LONGTEXT      NULL,
    StartActions        LONGTEXT      NULL,
    ApplyActions        LONGTEXT      NULL,
    EndActions          LONGTEXT      NULL,
    Threshold           DECIMAL(28,6) NULL,
    MinResult           DECIMAL(28,6) NULL,
    MaxResult           DECIMAL(28,6) NULL,
    Script              LONGTEXT      NULL,
    ScriptVersion       VARCHAR(128)  NULL,
    `Binary`            LONGBLOB      NULL,
    ScriptHash          INT           NULL,
    Attributes          LONGTEXT      NULL,
    Clusters            LONGTEXT      NULL,
    PRIMARY KEY (Id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- -----------------------------------------------------------------------------
-- CollectorCustomResult
-- -----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS CollectorCustomResult (
    Id                          INT           NOT NULL AUTO_INCREMENT,
    Status                      INT           NOT NULL,
    Created                     DATETIME(6)   NOT NULL,
    Updated                     DATETIME(6)   NOT NULL,
    CollectorResultId           INT           NOT NULL,
    TenantId                    INT           NOT NULL,
    EmployeeId                  INT           NOT NULL,
    DivisionId                  INT           NULL,
    CollectorName               VARCHAR(128)  NOT NULL,
    CollectorNameHash           INT           NOT NULL,
    CollectorNameLocalizations  LONGTEXT      NULL,
    Source                      VARCHAR(128)  NOT NULL,
    ValueType                   INT           NOT NULL,
    Value                       DECIMAL(28,6) NOT NULL,
    Culture                     VARCHAR(128)  NOT NULL,
    Start                       DATETIME(6)   NOT NULL,
    StartHash                   INT           NOT NULL,
    End                         DATETIME(6)   NOT NULL,
    PayrunJobId                 INT           NOT NULL,
    Forecast                    VARCHAR(128)  NULL,
    ParentJobId                 INT           NULL,
    Tags                        LONGTEXT      NULL,
    Attributes                  LONGTEXT      NULL,
    PRIMARY KEY (Id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- -----------------------------------------------------------------------------
-- CollectorResult
-- -----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS CollectorResult (
    Id                          INT           NOT NULL AUTO_INCREMENT,
    Status                      INT           NOT NULL,
    Created                     DATETIME(6)   NOT NULL,
    Updated                     DATETIME(6)   NOT NULL,
    PayrollResultId             INT           NOT NULL,
    TenantId                    INT           NOT NULL,
    EmployeeId                  INT           NOT NULL,
    DivisionId                  INT           NULL,
    CollectorId                 INT           NOT NULL,
    CollectorName               VARCHAR(128)  NOT NULL,
    CollectorNameHash           INT           NOT NULL,
    CollectorNameLocalizations  LONGTEXT      NULL,
    CollectMode                 INT           NOT NULL,
    Negated                     TINYINT(1)    NOT NULL,
    ValueType                   INT           NOT NULL,
    Value                       DECIMAL(28,6) NOT NULL,
    Culture                     VARCHAR(128)  NOT NULL,
    Start                       DATETIME(6)   NOT NULL,
    StartHash                   INT           NOT NULL,
    End                         DATETIME(6)   NOT NULL,
    PayrunJobId                 INT           NOT NULL,
    Forecast                    VARCHAR(128)  NULL,
    ParentJobId                 INT           NULL,
    Tags                        LONGTEXT      NULL,
    Attributes                  LONGTEXT      NULL,
    PRIMARY KEY (Id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- -----------------------------------------------------------------------------
-- CompanyCaseChange
-- -----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS CompanyCaseChange (
    Id                   INT           NOT NULL AUTO_INCREMENT,
    Status               INT           NOT NULL,
    Created              DATETIME(6)   NOT NULL,
    Updated              DATETIME(6)   NOT NULL,
    TenantId             INT           NOT NULL,
    UserId               INT           NOT NULL,
    DivisionId           INT           NULL,
    CancellationType     INT           NOT NULL,
    CancellationId       INT           NULL,
    CancellationDate     DATETIME(6)   NULL,
    Reason               LONGTEXT      NOT NULL,
    ValidationCaseName   VARCHAR(128)  NULL,
    Forecast             VARCHAR(128)  NULL,
    PRIMARY KEY (Id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- -----------------------------------------------------------------------------
-- CompanyCaseDocument
-- -----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS CompanyCaseDocument (
    Id          INT          NOT NULL AUTO_INCREMENT,
    Status      INT          NOT NULL,
    Created     DATETIME(6)  NOT NULL,
    Updated     DATETIME(6)  NOT NULL,
    CaseValueId INT          NOT NULL,
    Name        VARCHAR(256) NOT NULL,
    Content     LONGTEXT     NOT NULL,
    ContentType VARCHAR(128) NOT NULL,
    PRIMARY KEY (Id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- -----------------------------------------------------------------------------
-- CompanyCaseValue
-- -----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS CompanyCaseValue (
    Id                          INT           NOT NULL AUTO_INCREMENT,
    Status                      INT           NOT NULL,
    Created                     DATETIME(6)   NOT NULL,
    Updated                     DATETIME(6)   NOT NULL,
    TenantId                    INT           NOT NULL,
    DivisionId                  INT           NULL,
    CaseName                    VARCHAR(128)  NOT NULL,
    CaseNameLocalizations       LONGTEXT      NULL,
    CaseFieldName               VARCHAR(128)  NOT NULL,
    CaseFieldNameLocalizations  LONGTEXT      NULL,
    CaseSlot                    VARCHAR(128)  NULL,
    CaseSlotLocalizations       LONGTEXT      NULL,
    ValueType                   INT           NOT NULL,
    Value                       LONGTEXT      NOT NULL,
    NumericValue                DECIMAL(28,6) NULL,
    Culture                     VARCHAR(128)  NOT NULL,
    CaseRelation                LONGTEXT      NULL,
    CancellationDate            DATETIME(6)   NULL,
    Start                       DATETIME(6)   NULL,
    End                         DATETIME(6)   NULL,
    Forecast                    VARCHAR(128)  NULL,
    Tags                        LONGTEXT      NULL,
    Attributes                  LONGTEXT      NULL,
    PRIMARY KEY (Id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- -----------------------------------------------------------------------------
-- CompanyCaseValueChange
-- -----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS CompanyCaseValueChange (
    Id           INT         NOT NULL AUTO_INCREMENT,
    Status       INT         NOT NULL,
    Created      DATETIME(6) NOT NULL,
    Updated      DATETIME(6) NOT NULL,
    CaseChangeId INT         NOT NULL,
    CaseValueId  INT         NOT NULL,
    PRIMARY KEY (Id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- -----------------------------------------------------------------------------
-- Division
-- -----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS Division (
    Id                  INT          NOT NULL AUTO_INCREMENT,
    Status              INT          NOT NULL,
    Created             DATETIME(6)  NOT NULL,
    Updated             DATETIME(6)  NOT NULL,
    TenantId            INT          NOT NULL,
    Name                VARCHAR(128) NOT NULL,
    NameLocalizations   LONGTEXT     NULL,
    Culture             VARCHAR(128) NULL,
    Calendar            VARCHAR(128) NULL,
    Attributes          LONGTEXT     NULL,
    PRIMARY KEY (Id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- -----------------------------------------------------------------------------
-- Employee
-- -----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS Employee (
    Id          INT          NOT NULL AUTO_INCREMENT,
    Status      INT          NOT NULL,
    Created     DATETIME(6)  NOT NULL,
    Updated     DATETIME(6)  NOT NULL,
    TenantId    INT          NOT NULL,
    Identifier  VARCHAR(128) NOT NULL,
    FirstName   VARCHAR(128) NOT NULL,
    LastName    VARCHAR(128) NOT NULL,
    Culture     VARCHAR(128) NULL,
    Calendar    VARCHAR(128) NULL,
    Attributes  LONGTEXT     NULL,
    PRIMARY KEY (Id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- -----------------------------------------------------------------------------
-- EmployeeCaseChange
-- -----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS EmployeeCaseChange (
    Id                   INT          NOT NULL AUTO_INCREMENT,
    Status               INT          NOT NULL,
    Created              DATETIME(6)  NOT NULL,
    Updated              DATETIME(6)  NOT NULL,
    EmployeeId           INT          NOT NULL,
    UserId               INT          NOT NULL,
    DivisionId           INT          NULL,
    CancellationType     INT          NOT NULL,
    CancellationId       INT          NULL,
    CancellationDate     DATETIME(6)  NULL,
    Reason               LONGTEXT     NOT NULL,
    ValidationCaseName   VARCHAR(128) NULL,
    Forecast             VARCHAR(128) NULL,
    PRIMARY KEY (Id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- -----------------------------------------------------------------------------
-- EmployeeCaseDocument
-- -----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS EmployeeCaseDocument (
    Id          INT          NOT NULL AUTO_INCREMENT,
    Status      INT          NOT NULL,
    Created     DATETIME(6)  NOT NULL,
    Updated     DATETIME(6)  NOT NULL,
    CaseValueId INT          NOT NULL,
    Name        VARCHAR(256) NOT NULL,
    Content     LONGTEXT     NOT NULL,
    ContentType VARCHAR(128) NOT NULL,
    PRIMARY KEY (Id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- -----------------------------------------------------------------------------
-- EmployeeCaseValue
-- -----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS EmployeeCaseValue (
    Id                          INT           NOT NULL AUTO_INCREMENT,
    Status                      INT           NOT NULL,
    Created                     DATETIME(6)   NOT NULL,
    Updated                     DATETIME(6)   NOT NULL,
    EmployeeId                  INT           NOT NULL,
    DivisionId                  INT           NULL,
    CaseName                    VARCHAR(128)  NOT NULL,
    CaseNameLocalizations       LONGTEXT      NULL,
    CaseFieldName               VARCHAR(128)  NOT NULL,
    CaseFieldNameLocalizations  LONGTEXT      NULL,
    CaseSlot                    VARCHAR(128)  NULL,
    CaseSlotLocalizations       LONGTEXT      NULL,
    ValueType                   INT           NOT NULL,
    Value                       LONGTEXT      NOT NULL,
    NumericValue                DECIMAL(28,6) NULL,
    Culture                     VARCHAR(128)  NOT NULL,
    CaseRelation                LONGTEXT      NULL,
    CancellationDate            DATETIME(6)   NULL,
    Start                       DATETIME(6)   NULL,
    End                         DATETIME(6)   NULL,
    Forecast                    VARCHAR(128)  NULL,
    Tags                        LONGTEXT      NULL,
    Attributes                  LONGTEXT      NULL,
    PRIMARY KEY (Id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- -----------------------------------------------------------------------------
-- EmployeeCaseValueChange
-- -----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS EmployeeCaseValueChange (
    Id           INT         NOT NULL AUTO_INCREMENT,
    Status       INT         NOT NULL,
    Created      DATETIME(6) NOT NULL,
    Updated      DATETIME(6) NOT NULL,
    CaseChangeId INT         NOT NULL,
    CaseValueId  INT         NOT NULL,
    PRIMARY KEY (Id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- -----------------------------------------------------------------------------
-- EmployeeDivision
-- -----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS EmployeeDivision (
    Id         INT         NOT NULL AUTO_INCREMENT,
    Status     INT         NOT NULL,
    Created    DATETIME(6) NOT NULL,
    Updated    DATETIME(6) NOT NULL,
    EmployeeId INT         NOT NULL,
    DivisionId INT         NOT NULL,
    PRIMARY KEY (Id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- -----------------------------------------------------------------------------
-- GlobalCaseChange
-- -----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS GlobalCaseChange (
    Id                   INT          NOT NULL AUTO_INCREMENT,
    Status               INT          NOT NULL,
    Created              DATETIME(6)  NOT NULL,
    Updated              DATETIME(6)  NOT NULL,
    TenantId             INT          NOT NULL,
    UserId               INT          NOT NULL,
    DivisionId           INT          NULL,
    CancellationType     INT          NOT NULL,
    CancellationId       INT          NULL,
    CancellationDate     DATETIME(6)  NULL,
    Reason               LONGTEXT     NOT NULL,
    ValidationCaseName   VARCHAR(128) NULL,
    Forecast             VARCHAR(128) NULL,
    PRIMARY KEY (Id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- -----------------------------------------------------------------------------
-- GlobalCaseDocument
-- -----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS GlobalCaseDocument (
    Id          INT          NOT NULL AUTO_INCREMENT,
    Status      INT          NOT NULL,
    Created     DATETIME(6)  NOT NULL,
    Updated     DATETIME(6)  NOT NULL,
    CaseValueId INT          NOT NULL,
    Name        VARCHAR(256) NOT NULL,
    Content     LONGTEXT     NOT NULL,
    ContentType VARCHAR(128) NOT NULL,
    PRIMARY KEY (Id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- -----------------------------------------------------------------------------
-- GlobalCaseValue
-- -----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS GlobalCaseValue (
    Id                          INT           NOT NULL AUTO_INCREMENT,
    Status                      INT           NOT NULL,
    Created                     DATETIME(6)   NOT NULL,
    Updated                     DATETIME(6)   NOT NULL,
    TenantId                    INT           NOT NULL,
    DivisionId                  INT           NULL,
    CaseName                    VARCHAR(128)  NOT NULL,
    CaseNameLocalizations       LONGTEXT      NULL,
    CaseFieldName               VARCHAR(128)  NOT NULL,
    CaseFieldNameLocalizations  LONGTEXT      NULL,
    CaseSlot                    VARCHAR(128)  NULL,
    CaseSlotLocalizations       LONGTEXT      NULL,
    ValueType                   INT           NOT NULL,
    Value                       LONGTEXT      NOT NULL,
    NumericValue                DECIMAL(28,6) NULL,
    Culture                     VARCHAR(128)  NOT NULL,
    CaseRelation                LONGTEXT      NULL,
    CancellationDate            DATETIME(6)   NULL,
    Start                       DATETIME(6)   NULL,
    End                         DATETIME(6)   NULL,
    Forecast                    VARCHAR(128)  NULL,
    Tags                        LONGTEXT      NULL,
    Attributes                  LONGTEXT      NULL,
    PRIMARY KEY (Id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- -----------------------------------------------------------------------------
-- GlobalCaseValueChange
-- -----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS GlobalCaseValueChange (
    Id           INT         NOT NULL AUTO_INCREMENT,
    Status       INT         NOT NULL,
    Created      DATETIME(6) NOT NULL,
    Updated      DATETIME(6) NOT NULL,
    CaseChangeId INT         NOT NULL,
    CaseValueId  INT         NOT NULL,
    PRIMARY KEY (Id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- -----------------------------------------------------------------------------
-- Log
-- -----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS Log (
    Id        INT          NOT NULL AUTO_INCREMENT,
    Status    INT          NOT NULL,
    Created   DATETIME(6)  NOT NULL,
    Updated   DATETIME(6)  NOT NULL,
    TenantId  INT          NOT NULL,
    Level     INT          NOT NULL,
    Message   LONGTEXT     NOT NULL,
    `User`    VARCHAR(128) NOT NULL,
    Error     LONGTEXT     NULL,
    Comment   LONGTEXT     NULL,
    Owner     VARCHAR(128) NULL,
    OwnerType VARCHAR(128) NULL,
    PRIMARY KEY (Id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- -----------------------------------------------------------------------------
-- Lookup
-- -----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS Lookup (
    Id                       INT           NOT NULL AUTO_INCREMENT,
    Status                   INT           NOT NULL,
    Created                  DATETIME(6)   NOT NULL,
    Updated                  DATETIME(6)   NOT NULL,
    RegulationId             INT           NOT NULL,
    Name                     VARCHAR(128)  NOT NULL,
    NameLocalizations        LONGTEXT      NULL,
    Description              LONGTEXT      NULL,
    DescriptionLocalizations LONGTEXT      NULL,
    OverrideType             INT           NOT NULL,
    RangeSize                DECIMAL(28,6) NULL,
    Attributes               LONGTEXT      NULL,
    RangeMode                INT           NOT NULL,
    PRIMARY KEY (Id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- -----------------------------------------------------------------------------
-- LookupAudit
-- -----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS LookupAudit (
    Id                       INT           NOT NULL AUTO_INCREMENT,
    Status                   INT           NOT NULL,
    Created                  DATETIME(6)   NOT NULL,
    Updated                  DATETIME(6)   NOT NULL,
    LookupId                 INT           NOT NULL,
    Name                     VARCHAR(128)  NOT NULL,
    NameLocalizations        LONGTEXT      NULL,
    Description              LONGTEXT      NULL,
    DescriptionLocalizations LONGTEXT      NULL,
    OverrideType             INT           NOT NULL,
    RangeSize                DECIMAL(28,6) NULL,
    Attributes               LONGTEXT      NULL,
    RangeMode                INT           NOT NULL,
    PRIMARY KEY (Id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- -----------------------------------------------------------------------------
-- LookupValue
-- RangeValue is nullable -> partial unique index via functional expression
-- -----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS LookupValue (
    Id                  INT           NOT NULL AUTO_INCREMENT,
    Status              INT           NOT NULL,
    Created             DATETIME(6)   NOT NULL,
    Updated             DATETIME(6)   NOT NULL,
    LookupId            INT           NOT NULL,
    `Key`               LONGTEXT      NOT NULL,
    KeyHash             INT           NOT NULL,
    RangeValue          DECIMAL(28,6) NULL,
    Value               LONGTEXT      NOT NULL,
    ValueLocalizations  LONGTEXT      NULL,
    OverrideType        INT           NOT NULL,
    LookupHash          INT           NOT NULL,
    PRIMARY KEY (Id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- -----------------------------------------------------------------------------
-- LookupValueAudit
-- -----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS LookupValueAudit (
    Id                  INT           NOT NULL AUTO_INCREMENT,
    Status              INT           NOT NULL,
    Created             DATETIME(6)   NOT NULL,
    Updated             DATETIME(6)   NOT NULL,
    LookupValueId       INT           NOT NULL,
    `Key`               LONGTEXT      NOT NULL,
    KeyHash             INT           NOT NULL,
    RangeValue          DECIMAL(28,6) NULL,
    Value               LONGTEXT      NOT NULL,
    ValueLocalizations  LONGTEXT      NULL,
    OverrideType        INT           NOT NULL,
    LookupHash          INT           NOT NULL,
    PRIMARY KEY (Id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- -----------------------------------------------------------------------------
-- NationalCaseChange
-- -----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS NationalCaseChange (
    Id                   INT          NOT NULL AUTO_INCREMENT,
    Status               INT          NOT NULL,
    Created              DATETIME(6)  NOT NULL,
    Updated              DATETIME(6)  NOT NULL,
    TenantId             INT          NOT NULL,
    UserId               INT          NOT NULL,
    DivisionId           INT          NULL,
    CancellationType     INT          NOT NULL,
    CancellationId       INT          NULL,
    CancellationDate     DATETIME(6)  NULL,
    Reason               LONGTEXT     NOT NULL,
    ValidationCaseName   VARCHAR(128) NULL,
    Forecast             VARCHAR(128) NULL,
    PRIMARY KEY (Id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- -----------------------------------------------------------------------------
-- NationalCaseDocument
-- -----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS NationalCaseDocument (
    Id          INT          NOT NULL AUTO_INCREMENT,
    Status      INT          NOT NULL,
    Created     DATETIME(6)  NOT NULL,
    Updated     DATETIME(6)  NOT NULL,
    CaseValueId INT          NOT NULL,
    Name        VARCHAR(256) NOT NULL,
    Content     LONGTEXT     NOT NULL,
    ContentType VARCHAR(128) NOT NULL,
    PRIMARY KEY (Id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- -----------------------------------------------------------------------------
-- NationalCaseValue
-- -----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS NationalCaseValue (
    Id                          INT           NOT NULL AUTO_INCREMENT,
    Status                      INT           NOT NULL,
    Created                     DATETIME(6)   NOT NULL,
    Updated                     DATETIME(6)   NOT NULL,
    TenantId                    INT           NOT NULL,
    DivisionId                  INT           NULL,
    CaseName                    VARCHAR(128)  NOT NULL,
    CaseNameLocalizations       LONGTEXT      NULL,
    CaseFieldName               VARCHAR(128)  NOT NULL,
    CaseFieldNameLocalizations  LONGTEXT      NULL,
    CaseSlot                    VARCHAR(128)  NULL,
    CaseSlotLocalizations       LONGTEXT      NULL,
    ValueType                   INT           NOT NULL,
    Value                       LONGTEXT      NOT NULL,
    NumericValue                DECIMAL(28,6) NULL,
    Culture                     VARCHAR(128)  NOT NULL,
    CaseRelation                LONGTEXT      NULL,
    CancellationDate            DATETIME(6)   NULL,
    Start                       DATETIME(6)   NULL,
    End                         DATETIME(6)   NULL,
    Forecast                    VARCHAR(128)  NULL,
    Tags                        LONGTEXT      NULL,
    Attributes                  LONGTEXT      NULL,
    PRIMARY KEY (Id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- -----------------------------------------------------------------------------
-- NationalCaseValueChange
-- -----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS NationalCaseValueChange (
    Id           INT         NOT NULL AUTO_INCREMENT,
    Status       INT         NOT NULL,
    Created      DATETIME(6) NOT NULL,
    Updated      DATETIME(6) NOT NULL,
    CaseChangeId INT         NOT NULL,
    CaseValueId  INT         NOT NULL,
    PRIMARY KEY (Id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- -----------------------------------------------------------------------------
-- Payroll
-- -----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS Payroll (
    Id                          INT          NOT NULL AUTO_INCREMENT,
    Status                      INT          NOT NULL,
    Created                     DATETIME(6)  NOT NULL,
    Updated                     DATETIME(6)  NOT NULL,
    TenantId                    INT          NOT NULL,
    DivisionId                  INT          NOT NULL,
    Name                        VARCHAR(128) NOT NULL,
    NameLocalizations           LONGTEXT     NULL,
    Description                 LONGTEXT     NULL,
    DescriptionLocalizations    LONGTEXT     NULL,
    ClusterSetCase              VARCHAR(128) NULL,
    ClusterSetCaseField         VARCHAR(128) NULL,
    ClusterSetCollector         VARCHAR(128) NULL,
    ClusterSetCollectorRetro    VARCHAR(128) NULL,
    ClusterSetWageType          VARCHAR(128) NULL,
    ClusterSetWageTypeRetro     VARCHAR(128) NULL,
    ClusterSetCaseValue         VARCHAR(128) NULL,
    ClusterSetWageTypePeriod    VARCHAR(128) NULL,
    ClusterSetWageTypeLookup    VARCHAR(128) NULL,
    ClusterSets                 LONGTEXT     NULL,
    Attributes                  LONGTEXT     NULL,
    PRIMARY KEY (Id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- -----------------------------------------------------------------------------
-- PayrollLayer
-- -----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS PayrollLayer (
    Id              INT          NOT NULL AUTO_INCREMENT,
    Status          INT          NOT NULL,
    Created         DATETIME(6)  NOT NULL,
    Updated         DATETIME(6)  NOT NULL,
    PayrollId       INT          NOT NULL,
    RegulationName  VARCHAR(128) NOT NULL,
    Level           INT          NOT NULL,
    Priority        INT          NOT NULL,
    Attributes      LONGTEXT     NULL,
    PRIMARY KEY (Id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- -----------------------------------------------------------------------------
-- PayrollResult
-- -----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS PayrollResult (
    Id          INT          NOT NULL AUTO_INCREMENT,
    Status      INT          NOT NULL,
    Created     DATETIME(6)  NOT NULL,
    Updated     DATETIME(6)  NOT NULL,
    TenantId    INT          NOT NULL,
    PayrollId   INT          NOT NULL,
    PayrunId    INT          NOT NULL,
    PayrunJobId INT          NOT NULL,
    EmployeeId  INT          NOT NULL,
    DivisionId  INT          NOT NULL,
    CycleName   VARCHAR(128) NOT NULL,
    CycleStart  DATETIME(6)  NOT NULL,
    CycleEnd    DATETIME(6)  NOT NULL,
    PeriodName  VARCHAR(128) NOT NULL,
    PeriodStart DATETIME(6)  NOT NULL,
    PeriodEnd   DATETIME(6)  NOT NULL,
    PRIMARY KEY (Id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- -----------------------------------------------------------------------------
-- Payrun
-- -----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS Payrun (
    Id                              INT          NOT NULL AUTO_INCREMENT,
    Status                          INT          NOT NULL,
    Created                         DATETIME(6)  NOT NULL,
    Updated                         DATETIME(6)  NOT NULL,
    TenantId                        INT          NOT NULL,
    PayrollId                       INT          NOT NULL,
    Name                            VARCHAR(128) NOT NULL,
    NameLocalizations               LONGTEXT     NULL,
    DefaultReason                   LONGTEXT     NULL,
    DefaultReasonLocalizations      LONGTEXT     NULL,
    StartExpression                 LONGTEXT     NULL,
    EmployeeAvailableExpression     LONGTEXT     NULL,
    EmployeeStartExpression         LONGTEXT     NULL,
    EmployeeEndExpression           LONGTEXT     NULL,
    WageTypeAvailableExpression     LONGTEXT     NULL,
    EndExpression                   LONGTEXT     NULL,
    RetroTimeType                   INT          NOT NULL,
    Script                          LONGTEXT     NULL,
    ScriptVersion                   VARCHAR(128) NULL,
    `Binary`                        LONGBLOB     NULL,
    ScriptHash                      INT          NULL,
    PRIMARY KEY (Id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- -----------------------------------------------------------------------------
-- PayrunJob
-- -----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS PayrunJob (
    Id                      INT          NOT NULL AUTO_INCREMENT,
    Status                  INT          NOT NULL,
    Created                 DATETIME(6)  NOT NULL,
    Updated                 DATETIME(6)  NOT NULL,
    TenantId                INT          NOT NULL,
    PayrunId                INT          NOT NULL,
    PayrollId               INT          NOT NULL,
    DivisionId              INT          NOT NULL,
    ParentJobId             INT          NULL,
    CreatedUserId           INT          NOT NULL,
    ReleasedUserId          INT          NULL,
    ProcessedUserId         INT          NULL,
    FinishedUserId          INT          NULL,
    RetroPayMode            INT          NOT NULL,
    JobStatus               INT          NOT NULL,
    JobResult               INT          NOT NULL,
    Name                    VARCHAR(128) NOT NULL,
    Owner                   VARCHAR(128) NULL,
    Forecast                VARCHAR(128) NULL,
    CycleName               VARCHAR(128) NOT NULL,
    CycleStart              DATETIME(6)  NOT NULL,
    CycleEnd                DATETIME(6)  NOT NULL,
    PeriodName              VARCHAR(128) NOT NULL,
    PeriodStart             DATETIME(6)  NOT NULL,
    PeriodEnd               DATETIME(6)  NOT NULL,
    EvaluationDate          DATETIME(6)  NOT NULL,
    Released                DATETIME(6)  NULL,
    Processed               DATETIME(6)  NULL,
    Finished                DATETIME(6)  NULL,
    CreatedReason           LONGTEXT     NOT NULL,
    ReleasedReason          LONGTEXT     NULL,
    ProcessedReason         LONGTEXT     NULL,
    FinishedReason          LONGTEXT     NULL,
    TotalEmployeeCount      INT          NOT NULL,
    ProcessedEmployeeCount  INT          NOT NULL,
    JobStart                DATETIME(6)  NOT NULL,
    JobEnd                  DATETIME(6)  NULL,
    Message                 LONGTEXT     NULL,
    ErrorMessage            LONGTEXT     NULL,
    Tags                    LONGTEXT     NULL,
    Attributes              LONGTEXT     NULL,
    PRIMARY KEY (Id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- -----------------------------------------------------------------------------
-- PayrunJobEmployee
-- -----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS PayrunJobEmployee (
    Id          INT         NOT NULL AUTO_INCREMENT,
    Status      INT         NOT NULL,
    Created     DATETIME(6) NOT NULL,
    Updated     DATETIME(6) NOT NULL,
    PayrunJobId INT         NOT NULL,
    EmployeeId  INT         NOT NULL,
    PRIMARY KEY (Id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- -----------------------------------------------------------------------------
-- PayrunParameter
-- -----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS PayrunParameter (
    Id                       INT          NOT NULL AUTO_INCREMENT,
    Status                   INT          NOT NULL,
    Created                  DATETIME(6)  NOT NULL,
    Updated                  DATETIME(6)  NOT NULL,
    PayrunId                 INT          NOT NULL,
    Name                     VARCHAR(128) NOT NULL,
    NameLocalizations        LONGTEXT     NULL,
    Description              LONGTEXT     NULL,
    DescriptionLocalizations LONGTEXT     NULL,
    Mandatory                TINYINT(1)   NOT NULL,
    Value                    LONGTEXT     NULL,
    ValueType                INT          NOT NULL,
    Attributes               LONGTEXT     NULL,
    PRIMARY KEY (Id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- -----------------------------------------------------------------------------
-- PayrunResult
-- -----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS PayrunResult (
    Id                  INT           NOT NULL AUTO_INCREMENT,
    Status              INT           NOT NULL,
    Created             DATETIME(6)   NOT NULL,
    Updated             DATETIME(6)   NOT NULL,
    PayrollResultId     INT           NOT NULL,
    TenantId            INT           NOT NULL,
    EmployeeId          INT           NOT NULL,
    DivisionId          INT           NULL,
    Source              VARCHAR(128)  NOT NULL,
    Name                VARCHAR(128)  NOT NULL,
    NameLocalizations   LONGTEXT      NULL,
    Slot                VARCHAR(128)  NULL,
    ValueType           INT           NOT NULL,
    Value               LONGTEXT      NULL,
    NumericValue        DECIMAL(28,6) NULL,
    Culture             VARCHAR(128)  NOT NULL,
    Start               DATETIME(6)   NULL,
    StartHash           INT           NOT NULL,
    End                 DATETIME(6)   NULL,
    PayrunJobId         INT           NOT NULL,
    Forecast            VARCHAR(128)  NULL,
    ParentJobId         INT           NULL,
    Tags                LONGTEXT      NULL,
    Attributes          LONGTEXT      NULL,
    PRIMARY KEY (Id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- -----------------------------------------------------------------------------
-- PayrunTrace
-- -----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS PayrunTrace (
    Id              INT          NOT NULL AUTO_INCREMENT,
    Status          INT          NOT NULL,
    Created         DATETIME(6)  NOT NULL,
    Updated         DATETIME(6)  NOT NULL,
    PayrollResultId INT          NOT NULL,
    TenantId        INT          NOT NULL,
    EmployeeId      INT          NOT NULL,
    DivisionId      INT          NULL,
    Level           INT          NOT NULL,
    Text            LONGTEXT     NOT NULL,
    PayrunJobId     INT          NOT NULL,
    Forecast        VARCHAR(128) NULL,
    ParentJobId     INT          NULL,
    PRIMARY KEY (Id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- -----------------------------------------------------------------------------
-- Regulation
-- -----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS Regulation (
    Id                       INT          NOT NULL AUTO_INCREMENT,
    Status                   INT          NOT NULL,
    Created                  DATETIME(6)  NOT NULL,
    Updated                  DATETIME(6)  NOT NULL,
    TenantId                 INT          NOT NULL,
    Name                     VARCHAR(128) NOT NULL,
    NameLocalizations        LONGTEXT     NULL,
    Namespace                VARCHAR(128) NULL,
    Version                  INT          NOT NULL,
    SharedRegulation         TINYINT(1)   NOT NULL,
    ValidFrom                DATETIME(6)  NULL,
    Owner                    VARCHAR(128) NULL,
    Description              LONGTEXT     NULL,
    DescriptionLocalizations LONGTEXT     NULL,
    BaseRegulations          LONGTEXT     NULL,
    Attributes               LONGTEXT     NULL,
    PRIMARY KEY (Id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- -----------------------------------------------------------------------------
-- RegulationShare
-- -----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS RegulationShare (
    Id                    INT         NOT NULL AUTO_INCREMENT,
    Status                INT         NOT NULL,
    Created               DATETIME(6) NOT NULL,
    Updated               DATETIME(6) NOT NULL,
    ProviderTenantId      INT         NOT NULL,
    ProviderRegulationId  INT         NOT NULL,
    ConsumerTenantId      INT         NOT NULL,
    ConsumerDivisionId    INT         NULL,
    Attributes            LONGTEXT    NULL,
    PRIMARY KEY (Id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- -----------------------------------------------------------------------------
-- Report
-- -----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS Report (
    Id                       INT          NOT NULL AUTO_INCREMENT,
    Status                   INT          NOT NULL,
    Created                  DATETIME(6)  NOT NULL,
    Updated                  DATETIME(6)  NOT NULL,
    RegulationId             INT          NOT NULL,
    Name                     VARCHAR(128) NOT NULL,
    NameLocalizations        LONGTEXT     NULL,
    Description              LONGTEXT     NULL,
    DescriptionLocalizations LONGTEXT     NULL,
    Category                 VARCHAR(128) NULL,
    Queries                  LONGTEXT     NULL,
    Relations                LONGTEXT     NULL,
    OverrideType             INT          NOT NULL,
    AttributeMode            INT          NOT NULL,
    UserType                 INT          NOT NULL,
    ReportIsolation          INT          NOT NULL,
    BuildExpression          LONGTEXT     NULL,
    StartExpression          LONGTEXT     NULL,
    EndExpression            LONGTEXT     NULL,
    Script                   LONGTEXT     NULL,
    ScriptVersion            VARCHAR(128) NULL,
    `Binary`                 LONGBLOB     NULL,
    ScriptHash               INT          NOT NULL,
    Attributes               LONGTEXT     NULL,
    Clusters                 LONGTEXT     NULL,
    PRIMARY KEY (Id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- -----------------------------------------------------------------------------
-- ReportAudit
-- -----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS ReportAudit (
    Id                       INT          NOT NULL AUTO_INCREMENT,
    ReportId                 INT          NOT NULL,
    Status                   INT          NOT NULL,
    Created                  DATETIME(6)  NOT NULL,
    Updated                  DATETIME(6)  NOT NULL,
    Name                     VARCHAR(128) NOT NULL,
    NameLocalizations        LONGTEXT     NULL,
    Description              LONGTEXT     NULL,
    DescriptionLocalizations LONGTEXT     NULL,
    Category                 VARCHAR(128) NULL,
    Queries                  LONGTEXT     NULL,
    Relations                LONGTEXT     NULL,
    AttributeMode            INT          NOT NULL,
    OverrideType             INT          NOT NULL,
    UserType                 INT          NOT NULL,
    ReportIsolation          INT          NOT NULL,
    BuildExpression          LONGTEXT     NULL,
    StartExpression          LONGTEXT     NULL,
    EndExpression            LONGTEXT     NULL,
    Script                   LONGTEXT     NULL,
    ScriptVersion            VARCHAR(128) NULL,
    `Binary`                 LONGBLOB     NULL,
    ScriptHash               INT          NOT NULL,
    Attributes               LONGTEXT     NULL,
    Clusters                 LONGTEXT     NULL,
    PRIMARY KEY (Id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- -----------------------------------------------------------------------------
-- ReportLog
-- -----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS ReportLog (
    Id          INT          NOT NULL AUTO_INCREMENT,
    Status      INT          NOT NULL,
    Created     DATETIME(6)  NOT NULL,
    Updated     DATETIME(6)  NOT NULL,
    TenantId    INT          NOT NULL,
    ReportName  VARCHAR(128) NOT NULL,
    ReportDate  DATETIME(6)  NOT NULL,
    Message     LONGTEXT     NULL,
    `Key`       VARCHAR(128) NULL,
    `User`      VARCHAR(128) NOT NULL,
    PRIMARY KEY (Id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- -----------------------------------------------------------------------------
-- ReportParameter
-- -----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS ReportParameter (
    Id                       INT          NOT NULL AUTO_INCREMENT,
    Status                   INT          NOT NULL,
    Created                  DATETIME(6)  NOT NULL,
    Updated                  DATETIME(6)  NOT NULL,
    ReportId                 INT          NOT NULL,
    Name                     VARCHAR(128) NOT NULL,
    NameLocalizations        LONGTEXT     NULL,
    Description              LONGTEXT     NULL,
    DescriptionLocalizations LONGTEXT     NULL,
    Mandatory                TINYINT(1)   NOT NULL,
    Hidden                   TINYINT(1)   NOT NULL,
    Value                    LONGTEXT     NULL,
    ValueType                INT          NOT NULL,
    ParameterType            INT          NOT NULL,
    OverrideType             INT          NOT NULL,
    Attributes               LONGTEXT     NULL,
    PRIMARY KEY (Id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- -----------------------------------------------------------------------------
-- ReportParameterAudit
-- -----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS ReportParameterAudit (
    Id                       INT          NOT NULL AUTO_INCREMENT,
    ReportParameterId        INT          NOT NULL,
    Status                   INT          NOT NULL,
    Created                  DATETIME(6)  NOT NULL,
    Updated                  DATETIME(6)  NOT NULL,
    Name                     VARCHAR(128) NOT NULL,
    NameLocalizations        LONGTEXT     NULL,
    Description              LONGTEXT     NULL,
    DescriptionLocalizations LONGTEXT     NULL,
    Mandatory                TINYINT(1)   NOT NULL,
    Hidden                   TINYINT(1)   NOT NULL,
    Value                    LONGTEXT     NULL,
    ValueType                INT          NOT NULL,
    ParameterType            INT          NOT NULL,
    OverrideType             INT          NOT NULL,
    Attributes               LONGTEXT     NULL,
    PRIMARY KEY (Id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- -----------------------------------------------------------------------------
-- ReportTemplate
-- -----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS ReportTemplate (
    Id           INT          NOT NULL AUTO_INCREMENT,
    Status       INT          NOT NULL,
    Created      DATETIME(6)  NOT NULL,
    Updated      DATETIME(6)  NOT NULL,
    ReportId     INT          NOT NULL,
    Name         VARCHAR(128) NOT NULL,
    Culture      VARCHAR(128) NOT NULL,
    Content      LONGTEXT     NOT NULL,
    ContentType  VARCHAR(128) NULL,
    `Schema`     LONGTEXT     NULL,
    Resource     VARCHAR(256) NULL,
    OverrideType INT          NOT NULL,
    Attributes   LONGTEXT     NULL,
    PRIMARY KEY (Id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- -----------------------------------------------------------------------------
-- ReportTemplateAudit
-- -----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS ReportTemplateAudit (
    Id               INT          NOT NULL AUTO_INCREMENT,
    Status           INT          NOT NULL,
    Created          DATETIME(6)  NOT NULL,
    Updated          DATETIME(6)  NOT NULL,
    ReportTemplateId INT          NOT NULL,
    Name             VARCHAR(128) NOT NULL,
    Culture          VARCHAR(128) NOT NULL,
    Content          LONGTEXT     NOT NULL,
    ContentType      VARCHAR(128) NULL,
    `Schema`         LONGTEXT     NULL,
    Resource         VARCHAR(256) NULL,
    OverrideType     INT          NOT NULL,
    Attributes       LONGTEXT     NULL,
    PRIMARY KEY (Id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- -----------------------------------------------------------------------------
-- Script
-- -----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS Script (
    Id               INT          NOT NULL AUTO_INCREMENT,
    Status           INT          NOT NULL,
    Created          DATETIME(6)  NOT NULL,
    Updated          DATETIME(6)  NOT NULL,
    RegulationId     INT          NOT NULL,
    Name             VARCHAR(128) NOT NULL,
    FunctionTypeMask BIGINT       NOT NULL,
    Value            LONGTEXT     NOT NULL,
    OverrideType     INT          NOT NULL,
    PRIMARY KEY (Id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- -----------------------------------------------------------------------------
-- ScriptAudit
-- -----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS ScriptAudit (
    Id               INT          NOT NULL AUTO_INCREMENT,
    Status           INT          NOT NULL,
    Created          DATETIME(6)  NOT NULL,
    Updated          DATETIME(6)  NOT NULL,
    ScriptId         INT          NOT NULL,
    Name             VARCHAR(128) NOT NULL,
    FunctionTypeMask BIGINT       NOT NULL,
    Value            LONGTEXT     NOT NULL,
    OverrideType     INT          NOT NULL,
    PRIMARY KEY (Id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- -----------------------------------------------------------------------------
-- Task
-- -----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS Task (
    Id               INT          NOT NULL AUTO_INCREMENT,
    Status           INT          NOT NULL,
    Created          DATETIME(6)  NOT NULL,
    Updated          DATETIME(6)  NOT NULL,
    TenantId         INT          NOT NULL,
    Name             VARCHAR(128) NOT NULL,
    NameLocalizations LONGTEXT    NULL,
    Category         VARCHAR(128) NULL,
    Instruction      LONGTEXT     NOT NULL,
    ScheduledUserId  INT          NOT NULL,
    Scheduled        DATETIME(6)  NOT NULL,
    CompletedUserId  INT          NULL,
    Completed        DATETIME(6)  NULL,
    Attributes       LONGTEXT     NULL,
    PRIMARY KEY (Id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- -----------------------------------------------------------------------------
-- Tenant
-- -----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS Tenant (
    Id         INT          NOT NULL AUTO_INCREMENT,
    Status     INT          NOT NULL,
    Created    DATETIME(6)  NOT NULL,
    Updated    DATETIME(6)  NOT NULL,
    Identifier VARCHAR(128) NOT NULL,
    Culture    VARCHAR(128) NULL,
    Calendar   VARCHAR(128) NULL,
    Attributes LONGTEXT     NULL,
    PRIMARY KEY (Id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- -----------------------------------------------------------------------------
-- User
-- (backtick-quoted: USER is a reserved word in MySQL)
-- -----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS `User` (
    Id          INT          NOT NULL AUTO_INCREMENT,
    Status      INT          NOT NULL,
    Created     DATETIME(6)  NOT NULL,
    Updated     DATETIME(6)  NOT NULL,
    TenantId    INT          NOT NULL,
    Identifier  VARCHAR(128) NOT NULL,
    UserType    INT          NOT NULL,
    Password    VARCHAR(128) NULL,
    StoredSalt  LONGBLOB     NULL,
    FirstName   VARCHAR(128) NOT NULL,
    LastName    VARCHAR(128) NOT NULL,
    Culture     VARCHAR(128) NULL,
    Attributes  LONGTEXT     NULL,
    PRIMARY KEY (Id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- -----------------------------------------------------------------------------
-- Version
-- (backtick-quoted: VERSION is a function in MySQL, not reserved but safer)
-- -----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS `Version` (
    Id           INT          NOT NULL AUTO_INCREMENT,
    Created      DATETIME(6)  NOT NULL,
    MajorVersion INT          NOT NULL,
    MinorVersion INT          NOT NULL,
    SubVersion   INT          NOT NULL,
    Owner        VARCHAR(128) NOT NULL,
    Description  LONGTEXT     NOT NULL,
    PRIMARY KEY (Id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- -----------------------------------------------------------------------------
-- WageType
-- -----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS WageType (
    Id                       INT           NOT NULL AUTO_INCREMENT,
    Status                   INT           NOT NULL,
    Created                  DATETIME(6)   NOT NULL,
    Updated                  DATETIME(6)   NOT NULL,
    RegulationId             INT           NOT NULL,
    Name                     VARCHAR(128)  NOT NULL,
    NameLocalizations        LONGTEXT      NULL,
    WageTypeNumber           DECIMAL(28,6) NOT NULL,
    Description              LONGTEXT      NULL,
    DescriptionLocalizations LONGTEXT      NULL,
    OverrideType             INT           NOT NULL,
    ValueType                INT           NOT NULL,
    Calendar                 VARCHAR(128)  NULL,
    Culture                  VARCHAR(128)  NULL,
    Collectors               LONGTEXT      NULL,
    CollectorGroups          LONGTEXT      NULL,
    ValueExpression          LONGTEXT      NULL,
    ResultExpression         LONGTEXT      NULL,
    ValueActions             LONGTEXT      NULL,
    ResultActions            LONGTEXT      NULL,
    Script                   LONGTEXT      NULL,
    ScriptVersion            VARCHAR(128)  NULL,
    `Binary`                 LONGBLOB      NULL,
    ScriptHash               INT           NULL,
    Attributes               LONGTEXT      NULL,
    Clusters                 LONGTEXT      NULL,
    PRIMARY KEY (Id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- -----------------------------------------------------------------------------
-- WageTypeAudit
-- -----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS WageTypeAudit (
    Id                       INT           NOT NULL AUTO_INCREMENT,
    Status                   INT           NOT NULL,
    Created                  DATETIME(6)   NOT NULL,
    Updated                  DATETIME(6)   NOT NULL,
    WageTypeId               INT           NOT NULL,
    Name                     VARCHAR(128)  NOT NULL,
    NameLocalizations        LONGTEXT      NULL,
    WageTypeNumber           DECIMAL(28,6) NOT NULL,
    Description              LONGTEXT      NULL,
    DescriptionLocalizations LONGTEXT      NULL,
    OverrideType             INT           NOT NULL,
    ValueType                INT           NOT NULL,
    Calendar                 VARCHAR(128)  NULL,
    Culture                  VARCHAR(128)  NULL,
    Collectors               LONGTEXT      NULL,
    CollectorGroups          LONGTEXT      NULL,
    ValueExpression          LONGTEXT      NULL,
    ResultExpression         LONGTEXT      NULL,
    ValueActions             LONGTEXT      NULL,
    ResultActions            LONGTEXT      NULL,
    Script                   LONGTEXT      NULL,
    ScriptVersion            VARCHAR(128)  NULL,
    `Binary`                 LONGBLOB      NULL,
    ScriptHash               INT           NULL,
    Attributes               LONGTEXT      NULL,
    Clusters                 LONGTEXT      NULL,
    PRIMARY KEY (Id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- -----------------------------------------------------------------------------
-- WageTypeCustomResult
-- -----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS WageTypeCustomResult (
    Id                          INT           NOT NULL AUTO_INCREMENT,
    Status                      INT           NOT NULL,
    Created                     DATETIME(6)   NOT NULL,
    Updated                     DATETIME(6)   NOT NULL,
    WageTypeResultId            INT           NOT NULL,
    TenantId                    INT           NOT NULL,
    EmployeeId                  INT           NOT NULL,
    DivisionId                  INT           NULL,
    WageTypeNumber              DECIMAL(28,6) NOT NULL,
    WageTypeName                VARCHAR(128)  NOT NULL,
    WageTypeNameLocalizations   LONGTEXT      NULL,
    Source                      VARCHAR(128)  NOT NULL,
    ValueType                   INT           NOT NULL,
    Value                       DECIMAL(28,6) NOT NULL,
    Culture                     VARCHAR(128)  NOT NULL,
    Start                       DATETIME(6)   NOT NULL,
    StartHash                   INT           NOT NULL,
    End                         DATETIME(6)   NOT NULL,
    PayrunJobId                 INT           NOT NULL,
    Forecast                    VARCHAR(128)  NULL,
    ParentJobId                 INT           NULL,
    Tags                        LONGTEXT      NULL,
    Attributes                  LONGTEXT      NULL,
    PRIMARY KEY (Id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- -----------------------------------------------------------------------------
-- WageTypeResult
-- -----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS WageTypeResult (
    Id                          INT           NOT NULL AUTO_INCREMENT,
    Status                      INT           NOT NULL,
    Created                     DATETIME(6)   NOT NULL,
    Updated                     DATETIME(6)   NOT NULL,
    PayrollResultId             INT           NOT NULL,
    TenantId                    INT           NOT NULL,
    EmployeeId                  INT           NOT NULL,
    DivisionId                  INT           NULL,
    WageTypeId                  INT           NOT NULL,
    WageTypeNumber              DECIMAL(28,6) NOT NULL,
    WageTypeName                VARCHAR(128)  NOT NULL,
    WageTypeNameLocalizations   LONGTEXT      NULL,
    ValueType                   INT           NOT NULL,
    Value                       DECIMAL(28,6) NOT NULL,
    Culture                     VARCHAR(128)  NOT NULL,
    Start                       DATETIME(6)   NOT NULL,
    StartHash                   INT           NOT NULL,
    End                         DATETIME(6)   NOT NULL,
    PayrunJobId                 INT           NOT NULL,
    Forecast                    VARCHAR(128)  NULL,
    ParentJobId                 INT           NULL,
    Tags                        LONGTEXT      NULL,
    Attributes                  LONGTEXT      NULL,
    PRIMARY KEY (Id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- -----------------------------------------------------------------------------
-- Webhook
-- -----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS Webhook (
    Id              INT          NOT NULL AUTO_INCREMENT,
    Status          INT          NOT NULL,
    Created         DATETIME(6)  NOT NULL,
    Updated         DATETIME(6)  NOT NULL,
    TenantId        INT          NOT NULL,
    Name            VARCHAR(128) NOT NULL,
    ReceiverAddress VARCHAR(128) NOT NULL,
    Action          INT          NOT NULL,
    Attributes      LONGTEXT     NULL,
    PRIMARY KEY (Id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- -----------------------------------------------------------------------------
-- WebhookMessage
-- -----------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS WebhookMessage (
    Id               INT          NOT NULL AUTO_INCREMENT,
    Status           INT          NOT NULL,
    Created          DATETIME(6)  NOT NULL,
    Updated          DATETIME(6)  NOT NULL,
    WebhookId        INT          NOT NULL,
    ActionName       VARCHAR(128) NOT NULL,
    ReceiverAddress  VARCHAR(128) NOT NULL,
    RequestDate      DATETIME(6)  NOT NULL,
    RequestMessage   LONGTEXT     NULL,
    RequestOperation LONGTEXT     NULL,
    ResponseDate     DATETIME(6)  NULL,
    ResponseStatus   INT          NULL,
    ResponseMessage  LONGTEXT     NULL,
    PRIMARY KEY (Id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;


-- =============================================================================
-- INDEXES
-- =============================================================================

-- Calendar
CREATE UNIQUE INDEX IX_Calendar_UniquePerTenant ON Calendar (Name, TenantId);

-- Case
CREATE UNIQUE INDEX IX_Case_UniqueNamePerRegulation ON `Case` (RegulationId, Name);

-- CaseField
CREATE UNIQUE INDEX IX_CaseField_UniqueNamePerCase ON CaseField (CaseId, Name);
CREATE INDEX IX_CaseField_ValueType ON CaseField (ValueType);

-- CaseRelation
CREATE INDEX  IX_CaseRelation_SourceCaseName      ON CaseRelation (RegulationId, SourceCaseName);
CREATE INDEX  IX_CaseRelation_TargetCaseName      ON CaseRelation (RegulationId, TargetCaseName);
CREATE INDEX  IX_CaseRelation_TargetSlot          ON CaseRelation (RegulationId, TargetCaseSlot);
CREATE UNIQUE INDEX IX_CaseRelation_UniqueRelation ON CaseRelation (RegulationId, RelationHash);

-- Collector
CREATE INDEX  IX_Collector_CollectMode          ON Collector (CollectMode);
CREATE UNIQUE INDEX IX_Collector_UniquePerReg   ON Collector (Name, RegulationId);

-- CollectorCustomResult
CREATE INDEX IX_CollectorCustomResult_ResultId       ON CollectorCustomResult (CollectorResultId);
CREATE INDEX IX_CollectorCustomResult_Employee_Coll  ON CollectorCustomResult
    (TenantId, EmployeeId, StartHash, CollectorNameHash);

-- CollectorResult
CREATE INDEX IX_CollectorResult_PayrollResultId    ON CollectorResult (PayrollResultId);
CREATE INDEX IX_CollectorResult_Employee_Collector ON CollectorResult
    (TenantId, EmployeeId, StartHash, CollectorNameHash);

-- CompanyCaseValue
CREATE UNIQUE INDEX IX_CompanyCaseValue_Unique ON CompanyCaseValue
    (TenantId, DivisionId, CaseFieldName, CaseSlot, Created);
CREATE INDEX IX_CompanyCaseValue_CaseFieldName ON CompanyCaseValue (CaseFieldName);
CREATE INDEX IX_CompanyCaseValue_Slot          ON CompanyCaseValue (CaseSlot);
CREATE INDEX IX_CompanyCaseValue_TenantId      ON CompanyCaseValue (TenantId, CaseFieldName);

-- CompanyCaseValueChange
CREATE UNIQUE INDEX IX_CompanyCaseValueChange_Unique ON CompanyCaseValueChange
    (CaseValueId, CaseChangeId);

-- Division
CREATE UNIQUE INDEX IX_Division_UniquePerTenant ON Division (Name, TenantId);

-- Employee
CREATE INDEX IX_Employee_TenantId ON Employee (TenantId, Status);

-- EmployeeCaseValue
CREATE INDEX IX_EmployeeCaseValue_EmployeeId   ON EmployeeCaseValue (EmployeeId);
CREATE INDEX IX_EmployeeCaseValue_CaseFieldName ON EmployeeCaseValue (CaseFieldName);
CREATE INDEX IX_EmployeeCaseValue_Slot          ON EmployeeCaseValue (CaseSlot);
CREATE UNIQUE INDEX IX_EmployeeCaseValue_Unique ON EmployeeCaseValue
    (EmployeeId, DivisionId, CaseFieldName, CaseSlot, Created);

-- EmployeeCaseValueChange
CREATE UNIQUE INDEX IX_EmployeeCaseValueChange_Unique ON EmployeeCaseValueChange
    (CaseValueId, CaseChangeId);

-- GlobalCaseValue
CREATE INDEX IX_GlobalCaseValue_TenantId      ON GlobalCaseValue (TenantId);
CREATE INDEX IX_GlobalCaseValue_CaseFieldName ON GlobalCaseValue (CaseFieldName);
CREATE INDEX IX_GlobalCaseValue_Slot          ON GlobalCaseValue (CaseSlot);
CREATE UNIQUE INDEX IX_GlobalCaseValue_Unique ON GlobalCaseValue
    (TenantId, DivisionId, CaseFieldName, CaseSlot, Created);

-- GlobalCaseValueChange
CREATE UNIQUE INDEX IX_GlobalCaseValueChange_Unique ON GlobalCaseValueChange
    (CaseValueId, CaseChangeId);

-- Lookup
CREATE UNIQUE INDEX IX_Lookup_UniquePerReg ON Lookup (Name, RegulationId);

-- LookupValue
-- Unique constraint is on (LookupId, LookupHash) -- identical to SQL Server OPT-1 index.
-- Multiple entries with the same RangeValue but different keys (KeyHash) are allowed.
CREATE UNIQUE INDEX IX_LookupValue_UniqueValueKeyPerLookup ON LookupValue (LookupId, LookupHash);

-- NationalCaseValue
CREATE INDEX IX_NationalCaseValue_TenantId      ON NationalCaseValue (TenantId);
CREATE INDEX IX_NationalCaseValue_CaseFieldName ON NationalCaseValue (CaseFieldName);
CREATE INDEX IX_NationalCaseValue_Slot          ON NationalCaseValue (CaseSlot);
CREATE UNIQUE INDEX IX_NationalCaseValue_Unique ON NationalCaseValue
    (TenantId, DivisionId, CaseFieldName, CaseSlot, Created);

-- NationalCaseValueChange
CREATE UNIQUE INDEX IX_NationalCaseValueChange_Unique ON NationalCaseValueChange
    (CaseValueId, CaseChangeId);

-- Payroll
CREATE UNIQUE INDEX IX_Payroll_UniquePerTenant ON Payroll (TenantId, Name);

-- PayrollLayer
CREATE INDEX IX_PayrollLayer_PayrollId ON PayrollLayer (PayrollId);
CREATE INDEX IX_PayrollLayer_RegName   ON PayrollLayer (RegulationName);

-- PayrollResult
CREATE INDEX IX_PayrollResult_TenantId    ON PayrollResult (TenantId, EmployeeId);
CREATE INDEX IX_PayrollResult_PayrunJobId ON PayrollResult (PayrunJobId);

-- Payrun
CREATE UNIQUE INDEX IX_Payrun_UniquePerTenant ON Payrun (TenantId, Name);

-- PayrunJob
CREATE INDEX IX_PayrunJob_TenantId   ON PayrunJob (TenantId, JobStatus);
CREATE INDEX IX_PayrunJob_PayrunId   ON PayrunJob (PayrunId);
CREATE INDEX IX_PayrunJob_PeriodStart ON PayrunJob (PeriodStart);

-- PayrunJobEmployee
CREATE UNIQUE INDEX IX_PayrunJobEmployee_Unique ON PayrunJobEmployee (PayrunJobId, EmployeeId);

-- PayrunResult
CREATE INDEX IX_PayrunResult_PayrollResultId ON PayrunResult (PayrollResultId);
CREATE INDEX IX_PayrunResult_Employee        ON PayrunResult (TenantId, EmployeeId);

-- Regulation
CREATE INDEX IX_Regulation_TenantId ON Regulation (TenantId);
CREATE INDEX IX_Regulation_Name     ON Regulation (Name);

-- Report
CREATE UNIQUE INDEX IX_Report_UniquePerReg ON Report (RegulationId, Name);

-- ReportParameter
CREATE UNIQUE INDEX IX_ReportParameter_UniquePerReport ON ReportParameter (ReportId, Name);

-- ReportTemplate
CREATE UNIQUE INDEX IX_ReportTemplate_UniquePerReport ON ReportTemplate (ReportId, Name, Culture);

-- Script
CREATE UNIQUE INDEX IX_Script_UniquePerReg ON Script (RegulationId, Name);

-- Tenant
CREATE UNIQUE INDEX IX_Tenant_Identifier ON Tenant (Identifier);

-- User
CREATE UNIQUE INDEX IX_User_UniquePerTenant ON `User` (TenantId, Identifier);

-- WageType
CREATE UNIQUE INDEX IX_WageType_UniqueNumberPerReg ON WageType (RegulationId, WageTypeNumber);

-- WageTypeCustomResult
CREATE INDEX IX_WageTypeCustomResult_ResultId      ON WageTypeCustomResult (WageTypeResultId);
CREATE INDEX IX_WageTypeCustomResult_Employee_WT   ON WageTypeCustomResult
    (TenantId, EmployeeId, StartHash, WageTypeNumber);

-- WageTypeResult
CREATE INDEX IX_WageTypeResult_PayrollResultId ON WageTypeResult (PayrollResultId);
CREATE INDEX IX_WageTypeResult_Employee_WT     ON WageTypeResult
    (TenantId, EmployeeId, StartHash, WageTypeNumber);

-- Webhook
CREATE UNIQUE INDEX IX_Webhook_UniquePerTenant ON Webhook (TenantId, Name);


-- =============================================================================
-- VERSION RECORD
-- =============================================================================

INSERT INTO `Version` (Created, MajorVersion, MinorVersion, SubVersion, Owner, Description)
VALUES (NOW(6), 0, 9, 6, CURRENT_USER(), 'Payroll Engine: Full setup v0.9.6 (MySQL)');

SELECT 'PayrollEngine MySQL schema v0.9.6 created successfully.' AS Result;
