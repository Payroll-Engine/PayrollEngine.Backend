-- =============================================================================
-- Create-Model.mysql.sql
-- Creates the PayrollEngine database for MySQL 8.0+ (8.4 LTS recommended).
--
-- Schema version: 0.9.6
--
-- Includes: CREATE DATABASE, all tables, 37 indexes, 7 functions, 44 stored procedures
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
-- Reserved word quoting: User, Key, Order, Schema, Binary, Case
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
-- NOTE: Indexes are defined after all tables in the INDEXES section below.

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

CREATE TABLE IF NOT EXISTS CompanyCaseValueChange (
    Id           INT         NOT NULL AUTO_INCREMENT,
    Status       INT         NOT NULL,
    Created      DATETIME(6) NOT NULL,
    Updated      DATETIME(6) NOT NULL,
    CaseChangeId INT         NOT NULL,
    CaseValueId  INT         NOT NULL,
    PRIMARY KEY (Id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

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

CREATE TABLE IF NOT EXISTS EmployeeCaseValueChange (
    Id           INT         NOT NULL AUTO_INCREMENT,
    Status       INT         NOT NULL,
    Created      DATETIME(6) NOT NULL,
    Updated      DATETIME(6) NOT NULL,
    CaseChangeId INT         NOT NULL,
    CaseValueId  INT         NOT NULL,
    PRIMARY KEY (Id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS EmployeeDivision (
    Id         INT         NOT NULL AUTO_INCREMENT,
    Status     INT         NOT NULL,
    Created    DATETIME(6) NOT NULL,
    Updated    DATETIME(6) NOT NULL,
    EmployeeId INT         NOT NULL,
    DivisionId INT         NOT NULL,
    PRIMARY KEY (Id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

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

CREATE TABLE IF NOT EXISTS GlobalCaseValueChange (
    Id           INT         NOT NULL AUTO_INCREMENT,
    Status       INT         NOT NULL,
    Created      DATETIME(6) NOT NULL,
    Updated      DATETIME(6) NOT NULL,
    CaseChangeId INT         NOT NULL,
    CaseValueId  INT         NOT NULL,
    PRIMARY KEY (Id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

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

CREATE TABLE IF NOT EXISTS NationalCaseValueChange (
    Id           INT         NOT NULL AUTO_INCREMENT,
    Status       INT         NOT NULL,
    Created      DATETIME(6) NOT NULL,
    Updated      DATETIME(6) NOT NULL,
    CaseChangeId INT         NOT NULL,
    CaseValueId  INT         NOT NULL,
    PRIMARY KEY (Id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

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

CREATE TABLE IF NOT EXISTS PayrollResult (
    Id                   INT          NOT NULL AUTO_INCREMENT,
    Status               INT          NOT NULL,
    Created              DATETIME(6)  NOT NULL,
    Updated              DATETIME(6)  NOT NULL,
    TenantId             INT          NOT NULL,
    PayrollId            INT          NOT NULL,
    PayrollName          VARCHAR(128) NULL,
    PayrunId             INT          NOT NULL,
    PayrunName           VARCHAR(128) NULL,
    PayrunJobId          INT          NOT NULL,
    PayrunJobName        VARCHAR(128) NULL,
    EmployeeId           INT          NOT NULL,
    EmployeeIdentifier   VARCHAR(128) NULL,
    DivisionId           INT          NOT NULL,
    DivisionName         VARCHAR(128) NULL,
    CycleName            VARCHAR(128) NOT NULL,
    CycleStart  DATETIME(6)  NOT NULL,
    CycleEnd    DATETIME(6)  NOT NULL,
    PeriodName  VARCHAR(128) NOT NULL,
    PeriodStart DATETIME(6)  NOT NULL,
    PeriodEnd   DATETIME(6)  NOT NULL,
    PRIMARY KEY (Id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

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
    RetroBackCycles                 INT          NOT NULL,
    Script                          LONGTEXT     NULL,
    ScriptVersion                   VARCHAR(128) NULL,
    `Binary`                        LONGBLOB     NULL,
    ScriptHash                      INT          NULL,
    PRIMARY KEY (Id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

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

CREATE TABLE IF NOT EXISTS PayrunJobEmployee (
    Id          INT         NOT NULL AUTO_INCREMENT,
    Status      INT         NOT NULL,
    Created     DATETIME(6) NOT NULL,
    Updated     DATETIME(6) NOT NULL,
    PayrunJobId INT         NOT NULL,
    EmployeeId  INT         NOT NULL,
    PRIMARY KEY (Id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

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

CREATE TABLE IF NOT EXISTS RegulationShare (
    Id                    INT         NOT NULL AUTO_INCREMENT,
    Status                INT         NOT NULL,
    Created               DATETIME(6) NOT NULL,
    Updated               DATETIME(6) NOT NULL,
    ProviderTenantId      INT         NOT NULL,
    ProviderRegulationId  INT         NOT NULL,
    ConsumerTenantId      INT         NOT NULL,
    ConsumerDivisionId    INT         NULL,
    IsolationLevel        INT         NOT NULL DEFAULT 3,
    Attributes            LONGTEXT    NULL,
    PRIMARY KEY (Id),
    CONSTRAINT CK_RegulationShare_IsolationLevel CHECK (IsolationLevel IN (0, 1, 2, 3))
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

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

CREATE TABLE IF NOT EXISTS Task (
    Id                INT          NOT NULL AUTO_INCREMENT,
    Status            INT          NOT NULL,
    Created           DATETIME(6)  NOT NULL,
    Updated           DATETIME(6)  NOT NULL,
    TenantId          INT          NOT NULL,
    Name              VARCHAR(128) NOT NULL,
    NameLocalizations LONGTEXT     NULL,
    Category          VARCHAR(128) NULL,
    Instruction       LONGTEXT     NOT NULL,
    ScheduledUserId   INT          NOT NULL,
    Scheduled         DATETIME(6)  NOT NULL,
    CompletedUserId   INT          NULL,
    Completed         DATETIME(6)  NULL,
    Attributes        LONGTEXT     NULL,
    PRIMARY KEY (Id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

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

CREATE UNIQUE INDEX IX_Calendar_UniquePerTenant             ON Calendar (Name, TenantId);
CREATE UNIQUE INDEX IX_Case_UniqueNamePerRegulation         ON `Case` (RegulationId, Name);
CREATE UNIQUE INDEX IX_CaseField_UniqueNamePerCase          ON CaseField (CaseId, Name);
CREATE        INDEX IX_CaseField_ValueType                  ON CaseField (ValueType);
CREATE        INDEX IX_CaseRelation_SourceCaseName          ON CaseRelation (RegulationId, SourceCaseName);
CREATE        INDEX IX_CaseRelation_TargetCaseName          ON CaseRelation (RegulationId, TargetCaseName);
CREATE        INDEX IX_CaseRelation_TargetSlot              ON CaseRelation (RegulationId, TargetCaseSlot);
CREATE UNIQUE INDEX IX_CaseRelation_UniqueRelation          ON CaseRelation (RegulationId, RelationHash);
CREATE        INDEX IX_Collector_CollectMode                ON Collector (CollectMode);
CREATE UNIQUE INDEX IX_Collector_UniquePerReg               ON Collector (Name, RegulationId);
CREATE        INDEX IX_CollectorCustomResult_ResultId       ON CollectorCustomResult (CollectorResultId);
CREATE        INDEX IX_CollectorCustomResult_Employee_Coll  ON CollectorCustomResult (TenantId, EmployeeId, StartHash, CollectorNameHash);
CREATE        INDEX IX_CollectorResult_PayrollResultId      ON CollectorResult (PayrollResultId);
CREATE        INDEX IX_CollectorResult_Employee_Collector   ON CollectorResult (TenantId, EmployeeId, StartHash, CollectorNameHash);
CREATE UNIQUE INDEX IX_CompanyCaseValue_Unique              ON CompanyCaseValue (TenantId, DivisionId, CaseFieldName, CaseSlot, Created);
CREATE        INDEX IX_CompanyCaseValue_CaseFieldName       ON CompanyCaseValue (CaseFieldName);
CREATE        INDEX IX_CompanyCaseValue_Slot                ON CompanyCaseValue (CaseSlot);
CREATE        INDEX IX_CompanyCaseValue_TenantId            ON CompanyCaseValue (TenantId, CaseFieldName);
CREATE UNIQUE INDEX IX_CompanyCaseValueChange_Unique        ON CompanyCaseValueChange (CaseValueId, CaseChangeId);
CREATE UNIQUE INDEX IX_Division_UniquePerTenant             ON Division (Name, TenantId);
CREATE        INDEX IX_Employee_TenantId                    ON Employee (TenantId, Status);
CREATE        INDEX IX_EmployeeCaseValue_EmployeeId         ON EmployeeCaseValue (EmployeeId);
CREATE        INDEX IX_EmployeeCaseValue_CaseFieldName      ON EmployeeCaseValue (CaseFieldName);
CREATE        INDEX IX_EmployeeCaseValue_Slot               ON EmployeeCaseValue (CaseSlot);
CREATE UNIQUE INDEX IX_EmployeeCaseValue_Unique             ON EmployeeCaseValue (EmployeeId, DivisionId, CaseFieldName, CaseSlot, Created);
CREATE UNIQUE INDEX IX_EmployeeCaseValueChange_Unique       ON EmployeeCaseValueChange (CaseValueId, CaseChangeId);
CREATE        INDEX IX_GlobalCaseValue_TenantId             ON GlobalCaseValue (TenantId);
CREATE        INDEX IX_GlobalCaseValue_CaseFieldName        ON GlobalCaseValue (CaseFieldName);
CREATE        INDEX IX_GlobalCaseValue_Slot                 ON GlobalCaseValue (CaseSlot);
CREATE UNIQUE INDEX IX_GlobalCaseValue_Unique               ON GlobalCaseValue (TenantId, DivisionId, CaseFieldName, CaseSlot, Created);
CREATE UNIQUE INDEX IX_GlobalCaseValueChange_Unique         ON GlobalCaseValueChange (CaseValueId, CaseChangeId);
CREATE UNIQUE INDEX IX_Lookup_UniquePerReg                  ON Lookup (Name, RegulationId);
CREATE UNIQUE INDEX IX_LookupValue_UniqueValueKeyPerLookup  ON LookupValue (LookupId, LookupHash);
CREATE        INDEX IX_NationalCaseValue_TenantId           ON NationalCaseValue (TenantId);
CREATE        INDEX IX_NationalCaseValue_CaseFieldName      ON NationalCaseValue (CaseFieldName);
CREATE        INDEX IX_NationalCaseValue_Slot               ON NationalCaseValue (CaseSlot);
CREATE UNIQUE INDEX IX_NationalCaseValue_Unique             ON NationalCaseValue (TenantId, DivisionId, CaseFieldName, CaseSlot, Created);
CREATE UNIQUE INDEX IX_NationalCaseValueChange_Unique       ON NationalCaseValueChange (CaseValueId, CaseChangeId);
CREATE UNIQUE INDEX IX_Payroll_UniquePerTenant              ON Payroll (TenantId, Name);
CREATE        INDEX IX_PayrollLayer_PayrollId               ON PayrollLayer (PayrollId);
CREATE        INDEX IX_PayrollLayer_RegName                 ON PayrollLayer (RegulationName);
CREATE        INDEX IX_PayrollResult_TenantId               ON PayrollResult (TenantId, EmployeeId);
CREATE        INDEX IX_PayrollResult_PayrunJobId            ON PayrollResult (PayrunJobId);
CREATE UNIQUE INDEX IX_Payrun_UniquePerTenant               ON Payrun (TenantId, Name);
CREATE        INDEX IX_PayrunJob_TenantId                   ON PayrunJob (TenantId, JobStatus);
CREATE        INDEX IX_PayrunJob_PayrunId                   ON PayrunJob (PayrunId);
CREATE        INDEX IX_PayrunJob_PeriodStart                ON PayrunJob (PeriodStart);
CREATE UNIQUE INDEX IX_PayrunJobEmployee_Unique             ON PayrunJobEmployee (PayrunJobId, EmployeeId);
CREATE        INDEX IX_PayrunResult_PayrollResultId         ON PayrunResult (PayrollResultId);
CREATE        INDEX IX_PayrunResult_Employee                ON PayrunResult (TenantId, EmployeeId);
CREATE        INDEX IX_Regulation_TenantId                  ON Regulation (TenantId);
CREATE        INDEX IX_Regulation_Name                      ON Regulation (Name);
CREATE UNIQUE INDEX IX_Report_UniquePerReg                  ON Report (RegulationId, Name);
CREATE UNIQUE INDEX IX_ReportParameter_UniquePerReport      ON ReportParameter (ReportId, Name);
CREATE UNIQUE INDEX IX_ReportTemplate_UniquePerReport       ON ReportTemplate (ReportId, Name, Culture);
CREATE UNIQUE INDEX IX_Script_UniquePerReg                  ON Script (RegulationId, Name);
CREATE UNIQUE INDEX IX_Tenant_Identifier                    ON Tenant (Identifier);
CREATE UNIQUE INDEX IX_User_UniquePerTenant                 ON `User` (TenantId, Identifier);
CREATE UNIQUE INDEX IX_WageType_UniqueNumberPerReg          ON WageType (RegulationId, WageTypeNumber);
CREATE        INDEX IX_WageTypeCustomResult_ResultId        ON WageTypeCustomResult (WageTypeResultId);
CREATE        INDEX IX_WageTypeCustomResult_Employee_WT     ON WageTypeCustomResult (TenantId, EmployeeId, StartHash, WageTypeNumber);
CREATE        INDEX IX_WageTypeResult_PayrollResultId       ON WageTypeResult (PayrollResultId);
CREATE        INDEX IX_WageTypeResult_Employee_WT           ON WageTypeResult (TenantId, EmployeeId, StartHash, WageTypeNumber);
CREATE UNIQUE INDEX IX_Webhook_UniquePerTenant              ON Webhook (TenantId, Name);


-- =============================================================================
-- FUNCTIONS (7)
-- DELIMITER $$ avoids conflicts with $ in JSON PATH expressions
-- =============================================================================

-- BuildAttributeQuery.mysql.sql
-- =============================================================================
-- BuildAttributeQuery
-- Builds a SQL fragment for dynamic attribute column projection.
-- Used by CaseValue pivot SPs and GetPayrollResultValues.
--
-- T-SQL: imperative WHILE + OPENJSON + string concatenation
-- MySQL: JSON_TABLE with FOR ORDINALITY + GROUP_CONCAT (order preserved)
--
-- Output: '' if empty, ',' + fragment + newline if attributes present
--
-- Attribute prefix convention:
--   TA_ -> GetTextAttributeValue(field, 'name') AS TA_xxx
--   NA_ -> GetNumericAttributeValue(field, 'name') AS NA_xxx
--   DA_ -> GetDateAttributeValue(field, 'name') AS DA_xxx
--   NULL field -> NULL AS xxx  (PayrunResult has no attribute field)
--
-- NOTE: Attribute JSON keys are plain names ("City"), not prefixed ("TA_City").
-- The TA_/NA_/DA_ prefix is the output column alias only.
-- =============================================================================

USE PayrollEngine;

SET GLOBAL log_bin_trust_function_creators = 1;

-- =============================================================================
-- FUNCTIONS (7)
-- DELIMITER $$ avoids conflicts with $ in JSON PATH expressions
-- =============================================================================

-- BuildAttributeQuery.mysql.sql
-- =============================================================================
-- BuildAttributeQuery
-- Builds a SQL fragment for dynamic attribute column projection.
-- Used by CaseValue pivot SPs and GetPayrollResultValues.
--
-- T-SQL: imperative WHILE + OPENJSON + string concatenation
-- MySQL: JSON_TABLE with FOR ORDINALITY + GROUP_CONCAT (order preserved)
--
-- Output: '' if empty, ',' + fragment + newline if attributes present
--
-- Attribute prefix convention:
--   TA_ -> GetTextAttributeValue(field, 'name') AS TA_xxx
--   NA_ -> GetNumericAttributeValue(field, 'name') AS NA_xxx
--   DA_ -> GetDateAttributeValue(field, 'name') AS DA_xxx
--   NULL field -> NULL AS xxx  (PayrunResult has no attribute field)
--
-- NOTE: Attribute JSON keys are plain names ("City"), not prefixed ("TA_City").
-- The TA_/NA_/DA_ prefix is the output column alias only.
-- =============================================================================

USE PayrollEngine;

SET GLOBAL log_bin_trust_function_creators = 1;

DELIMITER $$

DROP FUNCTION IF EXISTS BuildAttributeQuery$$
CREATE FUNCTION BuildAttributeQuery(
    p_attributeField LONGTEXT,
    p_attributes     LONGTEXT
)
RETURNS LONGTEXT
DETERMINISTIC
READS SQL DATA
BEGIN
    DECLARE v_parts LONGTEXT;
    DECLARE v_sql   LONGTEXT DEFAULT '';

    IF p_attributes IS NULL THEN
        RETURN v_sql;
    END IF;

    IF JSON_LENGTH(p_attributes) = 0 THEN
        RETURN v_sql;
    END IF;

    SELECT GROUP_CONCAT(
        CASE
            WHEN p_attributeField IS NULL THEN
                CONCAT('NULL AS ', j.val)
            WHEN LEFT(j.val, 3) = 'TA_' THEN
                CONCAT('GetTextAttributeValue(', p_attributeField, ', ''', SUBSTRING(j.val, 4), ''') AS ', j.val)
            WHEN LEFT(j.val, 3) = 'DA_' THEN
                CONCAT('GetDateAttributeValue(', p_attributeField, ', ''', SUBSTRING(j.val, 4), ''') AS ', j.val)
            WHEN LEFT(j.val, 3) = 'NA_' THEN
                CONCAT('GetNumericAttributeValue(', p_attributeField, ', ''', SUBSTRING(j.val, 4), ''') AS ', j.val)
            ELSE NULL
        END
        ORDER BY j.idx
        SEPARATOR ', '
    )
    INTO v_parts
    FROM JSON_TABLE(
        p_attributes,
        '$[*]' COLUMNS (idx FOR ORDINALITY, val VARCHAR(128) PATH '$')
    ) AS j
    WHERE LENGTH(TRIM(j.val)) > 0;

    IF v_parts IS NOT NULL AND LENGTH(v_parts) > 0 THEN
        SET v_sql = CONCAT(',', v_parts, '\n        ');
    END IF;

    RETURN v_sql;
END$$

DELIMITER ;

-- GetAttributeNames.mysql.sql
-- =============================================================================
-- GetAttributeNames
-- Builds a comma-separated list of attribute names from a JSON array.
-- Used by GetPayrollResultValues for the outer SELECT projection.
--
-- T-SQL: imperative WHILE + OPENJSON + string concatenation
-- MySQL: JSON_TABLE with FOR ORDINALITY + GROUP_CONCAT
--
-- Output: '' if empty, ',' + names + newline if non-empty
-- =============================================================================

USE PayrollEngine;

SET GLOBAL log_bin_trust_function_creators = 1;

DELIMITER $$

DROP FUNCTION IF EXISTS GetAttributeNames$$
CREATE FUNCTION GetAttributeNames(
    p_attributes LONGTEXT
)
RETURNS LONGTEXT
DETERMINISTIC
READS SQL DATA
BEGIN
    DECLARE v_parts LONGTEXT;
    DECLARE v_sql   LONGTEXT DEFAULT '';

    IF p_attributes IS NULL THEN
        RETURN v_sql;
    END IF;

    IF JSON_LENGTH(p_attributes) = 0 THEN
        RETURN v_sql;
    END IF;

    SELECT GROUP_CONCAT(j.val ORDER BY j.idx SEPARATOR ', ')
    INTO v_parts
    FROM JSON_TABLE(
        p_attributes,
        '$[*]' COLUMNS (idx FOR ORDINALITY, val VARCHAR(128) PATH '$')
    ) AS j
    WHERE LENGTH(TRIM(j.val)) > 0;

    IF v_parts IS NOT NULL AND LENGTH(v_parts) > 0 THEN
        SET v_sql = CONCAT(',', v_parts, '\n        ');
    END IF;

    RETURN v_sql;
END$$

DELIMITER ;

-- GetDateAttributeValue.mysql.sql
-- =============================================================================
-- GetDateAttributeValue
-- Returns DATETIME(6) from a JSON attribute stored as ISO 8601 string.
-- NULL if attribute is not a string or not parseable as datetime.
--
-- T-SQL: RETURN IIF(@type = 1, CAST(@value AS DATETIME2(7)), NULL)
-- MySQL: JSON_TYPE='STRING' + CAST AS DATETIME(6)
-- =============================================================================

USE PayrollEngine;

SET GLOBAL log_bin_trust_function_creators = 1;

DELIMITER $$

DROP FUNCTION IF EXISTS GetDateAttributeValue$$
CREATE FUNCTION GetDateAttributeValue(
    p_attributes LONGTEXT,
    p_name       VARCHAR(255)
)
RETURNS DATETIME(6)
DETERMINISTIC
READS SQL DATA
BEGIN
    DECLARE v_path  VARCHAR(512);
    DECLARE v_type  VARCHAR(20);
    DECLARE v_raw   VARCHAR(50);

    IF p_attributes IS NULL OR p_name IS NULL THEN
        RETURN NULL;
    END IF;

    SET v_path = CONCAT('$.', p_name);
    SET v_type = JSON_TYPE(JSON_EXTRACT(p_attributes, v_path));

    IF v_type = 'STRING' THEN
        SET v_raw = JSON_UNQUOTE(JSON_EXTRACT(p_attributes, v_path));
        RETURN CAST(v_raw AS DATETIME(6));
    END IF;

    RETURN NULL;
END$$

DELIMITER ;

-- GetLocalizedValue.mysql.sql
-- =============================================================================
-- GetLocalizedValue
-- Returns the value for the given culture from a JSON localizations object.
-- Falls back to p_fallback if culture key not found.
--
-- T-SQL: SELECT @value = value FROM OPENJSON(@localizations) WHERE [key] = @culture
-- MySQL: JSON_EXTRACT with quoted key syntax $."de-CH" (handles hyphens)
--
-- IMPORTANT: Culture codes like "de-CH" contain a hyphen.
-- Unquoted path $.de-CH fails with ERROR 3143. Use $."de-CH" instead.
-- =============================================================================

USE PayrollEngine;

SET GLOBAL log_bin_trust_function_creators = 1;

DELIMITER $$

DROP FUNCTION IF EXISTS GetLocalizedValue$$
CREATE FUNCTION GetLocalizedValue(
    p_localizations LONGTEXT,
    p_culture       VARCHAR(128),
    p_fallback      LONGTEXT
)
RETURNS LONGTEXT
DETERMINISTIC
READS SQL DATA
BEGIN
    DECLARE v_path  VARCHAR(512);
    DECLARE v_value LONGTEXT;

    IF p_localizations IS NULL OR p_culture IS NULL THEN
        RETURN p_fallback;
    END IF;

    SET v_path  = CONCAT('$."', p_culture, '"');
    SET v_value = JSON_UNQUOTE(JSON_EXTRACT(p_localizations, v_path));

    IF v_value IS NULL THEN
        RETURN p_fallback;
    END IF;

    RETURN v_value;
END$$

DELIMITER ;

-- GetNumericAttributeValue.mysql.sql
-- =============================================================================
-- GetNumericAttributeValue
-- Returns DECIMAL(28,6) value of a JSON attribute, NULL if not numeric.
--
-- T-SQL: RETURN IIF(@type = 2, CAST(@value AS DECIMAL(28,6)), NULL)
-- MySQL: JSON_TYPE checks for 'INTEGER' or 'DOUBLE'
-- =============================================================================

USE PayrollEngine;

SET GLOBAL log_bin_trust_function_creators = 1;

DELIMITER $$

DROP FUNCTION IF EXISTS GetNumericAttributeValue$$
CREATE FUNCTION GetNumericAttributeValue(
    p_attributes LONGTEXT,
    p_name       VARCHAR(255)
)
RETURNS DECIMAL(28,6)
DETERMINISTIC
READS SQL DATA
BEGIN
    DECLARE v_path  VARCHAR(512);
    DECLARE v_type  VARCHAR(20);

    IF p_attributes IS NULL OR p_name IS NULL THEN
        RETURN NULL;
    END IF;

    SET v_path = CONCAT('$.', p_name);
    SET v_type = JSON_TYPE(JSON_EXTRACT(p_attributes, v_path));

    IF v_type IN ('INTEGER', 'DOUBLE') THEN
        RETURN CAST(JSON_EXTRACT(p_attributes, v_path) AS DECIMAL(28,6));
    END IF;

    RETURN NULL;
END$$

DELIMITER ;

-- GetTextAttributeValue.mysql.sql
-- =============================================================================
-- GetTextAttributeValue
-- Returns the string value of a JSON attribute key, NULL if type is not string.
--
-- T-SQL: RETURN IIF(@type = 1, @value, NULL)  -- type 1 = string in OPENJSON
-- MySQL: JSON_EXTRACT + JSON_TYPE check for 'STRING'
-- =============================================================================

USE PayrollEngine;

SET GLOBAL log_bin_trust_function_creators = 1;

DELIMITER $$

DROP FUNCTION IF EXISTS GetTextAttributeValue$$
CREATE FUNCTION GetTextAttributeValue(
    p_attributes LONGTEXT,
    p_name       VARCHAR(255)
)
RETURNS LONGTEXT
DETERMINISTIC
READS SQL DATA
BEGIN
    DECLARE v_path  VARCHAR(512);
    DECLARE v_type  VARCHAR(20);

    IF p_attributes IS NULL OR p_name IS NULL THEN
        RETURN NULL;
    END IF;

    SET v_path = CONCAT('$.', p_name);
    SET v_type = JSON_TYPE(JSON_EXTRACT(p_attributes, v_path));

    IF v_type = 'STRING' THEN
        RETURN JSON_UNQUOTE(JSON_EXTRACT(p_attributes, v_path));
    END IF;

    RETURN NULL;
END$$

DELIMITER ;

-- IsMatchingCluster.mysql.sql
-- =============================================================================
-- IsMatchingCluster
-- Tests include/exclude cluster filters against a test cluster array.
-- All arrays are JSON arrays of VARCHAR(128).
--
-- T-SQL: imperative WHILE loop over OPENJSON
-- MySQL: set-based JSON_TABLE + EXISTS / NOT EXISTS
--
-- Logic:
--   include: every cluster in includeClusters must appear in testClusters
--   exclude: no cluster in excludeClusters may appear in testClusters
--   returns 1 (match) or 0 (no match)
-- =============================================================================

USE PayrollEngine;

SET GLOBAL log_bin_trust_function_creators = 1;

DELIMITER $$

DROP FUNCTION IF EXISTS IsMatchingCluster$$
CREATE FUNCTION IsMatchingCluster(
    p_includeClusters VARCHAR(4000),
    p_excludeClusters VARCHAR(4000),
    p_testClusters    VARCHAR(4000)
)
RETURNS TINYINT(1)
DETERMINISTIC
READS SQL DATA
BEGIN
    IF p_testClusters IS NULL THEN
        SET p_testClusters = '[]';
    END IF;

    IF p_includeClusters IS NOT NULL AND JSON_LENGTH(p_includeClusters) > 0 THEN
        IF EXISTS (
            SELECT 1
            FROM JSON_TABLE(p_includeClusters, '$[*]' COLUMNS (val VARCHAR(128) PATH '$')) AS inc
            WHERE LENGTH(TRIM(inc.val)) > 0
              AND NOT EXISTS (
                SELECT 1
                FROM JSON_TABLE(p_testClusters, '$[*]' COLUMNS (val VARCHAR(128) PATH '$')) AS tst
                WHERE tst.val = inc.val)
        ) THEN
            RETURN 0;
        END IF;
    END IF;

    IF p_excludeClusters IS NOT NULL AND JSON_LENGTH(p_excludeClusters) > 0 THEN
        IF EXISTS (
            SELECT 1
            FROM JSON_TABLE(p_excludeClusters, '$[*]' COLUMNS (val VARCHAR(128) PATH '$')) AS exc
            WHERE LENGTH(TRIM(exc.val)) > 0
              AND EXISTS (
                SELECT 1
                FROM JSON_TABLE(p_testClusters, '$[*]' COLUMNS (val VARCHAR(128) PATH '$')) AS tst
                WHERE tst.val = exc.val)
        ) THEN
            RETURN 0;
        END IF;
    END IF;

    RETURN 1;
END$$

DELIMITER ;


-- =============================================================================
-- STORED PROCEDURES (44)
-- =============================================================================

-- DeleteAllCompanyCaseValues.mysql.sql
-- =============================================================================
-- DeleteAllCompanyCaseValues
-- =============================================================================

USE PayrollEngine;

DELIMITER $$

DROP PROCEDURE IF EXISTS DeleteAllCompanyCaseValues$$
CREATE PROCEDURE DeleteAllCompanyCaseValues()
BEGIN
    DELETE FROM CompanyCaseValueChange;
    DELETE FROM CompanyCaseDocument;
    DELETE FROM CompanyCaseValue;
    DELETE FROM CompanyCaseChange;
END$$

DELIMITER ;

-- DeleteAllEmployeeCaseValues.mysql.sql
-- =============================================================================
-- DeleteAllEmployeeCaseValues
-- =============================================================================

USE PayrollEngine;

DELIMITER $$

DROP PROCEDURE IF EXISTS DeleteAllEmployeeCaseValues$$
CREATE PROCEDURE DeleteAllEmployeeCaseValues()
BEGIN
    DELETE FROM EmployeeCaseValueChange;
    DELETE FROM EmployeeCaseDocument;
    DELETE FROM EmployeeCaseValue;
    DELETE FROM EmployeeCaseChange;
END$$

DELIMITER ;

-- DeleteAllGlobalCaseValues.mysql.sql
-- =============================================================================
-- DeleteAllGlobalCaseValues
-- =============================================================================

USE PayrollEngine;

DELIMITER $$

DROP PROCEDURE IF EXISTS DeleteAllGlobalCaseValues$$
CREATE PROCEDURE DeleteAllGlobalCaseValues()
BEGIN
    DELETE FROM GlobalCaseValueChange;
    DELETE FROM GlobalCaseDocument;
    DELETE FROM GlobalCaseValue;
    DELETE FROM GlobalCaseChange;
END$$

DELIMITER ;

-- DeleteAllNationalCaseValues.mysql.sql
-- =============================================================================
-- DeleteAllNationalCaseValues
-- =============================================================================

USE PayrollEngine;

DELIMITER $$

DROP PROCEDURE IF EXISTS DeleteAllNationalCaseValues$$
CREATE PROCEDURE DeleteAllNationalCaseValues()
BEGIN
    DELETE FROM NationalCaseValueChange;
    DELETE FROM NationalCaseDocument;
    DELETE FROM NationalCaseValue;
    DELETE FROM NationalCaseChange;
END$$

DELIMITER ;

-- DeleteAllCaseValues.mysql.sql
-- =============================================================================
-- DeleteAllCaseValues
-- Delegates to the four scope-specific procedures.
-- =============================================================================

USE PayrollEngine;

DELIMITER $$

DROP PROCEDURE IF EXISTS DeleteAllCaseValues$$
CREATE PROCEDURE DeleteAllCaseValues()
BEGIN
    CALL DeleteAllGlobalCaseValues();
    CALL DeleteAllNationalCaseValues();
    CALL DeleteAllCompanyCaseValues();
    CALL DeleteAllEmployeeCaseValues();
END$$

DELIMITER ;

-- DeleteEmployee.mysql.sql
-- =============================================================================
-- DeleteEmployee
-- DELETE t FROM t INNER JOIN -> DELETE t USING t INNER JOIN (MySQL syntax)
-- =============================================================================

USE PayrollEngine;

DELIMITER $$

DROP PROCEDURE IF EXISTS DeleteEmployee$$
CREATE PROCEDURE DeleteEmployee(
    IN p_tenantId   INT,
    IN p_employeeId INT
)
BEGIN
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

    DELETE FROM PayrollResult WHERE TenantId = p_tenantId AND EmployeeId = p_employeeId;

    DELETE pje FROM PayrunJobEmployee pje
    INNER JOIN PayrunJob pj ON pje.PayrunJobId = pj.Id
    WHERE pj.TenantId = p_tenantId AND pje.EmployeeId = p_employeeId;

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

DELIMITER ;

-- DeleteLookup.mysql.sql
-- =============================================================================
-- DeleteLookup
-- =============================================================================

USE PayrollEngine;

DELIMITER $$

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

DELIMITER ;

-- DeletePayrunJob.mysql.sql
-- =============================================================================
-- DeletePayrunJob
-- =============================================================================

USE PayrollEngine;

DELIMITER $$

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

    DELETE FROM PayrollResult WHERE TenantId = p_tenantId AND PayrunJobId = p_payrunJobId;

    DELETE pje FROM PayrunJobEmployee pje
    INNER JOIN PayrunJob pj ON pje.PayrunJobId = pj.Id
    WHERE pj.TenantId = p_tenantId AND pje.PayrunJobId = p_payrunJobId;

    DELETE FROM PayrunJob WHERE TenantId = p_tenantId AND Id = p_payrunJobId;
END$$

DELIMITER ;

-- DeleteTenant.mysql.sql
-- =============================================================================
-- DeleteTenant
-- DELETE [ReportLog] WHERE -> DELETE FROM ReportLog WHERE (T-SQL alias quirk)
-- =============================================================================

USE PayrollEngine;

DELIMITER $$

DROP PROCEDURE IF EXISTS DeleteTenant$$
CREATE PROCEDURE DeleteTenant(
    IN p_tenantId INT
)
BEGIN
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

    DELETE pje FROM PayrunJobEmployee pje
    INNER JOIN PayrunJob pj ON pje.PayrunJobId = pj.Id
    WHERE pj.TenantId = p_tenantId;

    DELETE FROM PayrunJob WHERE TenantId = p_tenantId;

    DELETE pp FROM PayrunParameter pp
    INNER JOIN Payrun pay ON pp.PayrunId = pay.Id
    WHERE pay.TenantId = p_tenantId;

    DELETE FROM Payrun WHERE TenantId = p_tenantId;

    DELETE pl FROM PayrollLayer pl
    INNER JOIN Payroll pay ON pl.PayrollId = pay.Id
    WHERE pay.TenantId = p_tenantId;

    DELETE FROM Payroll WHERE TenantId = p_tenantId;

    DELETE FROM RegulationShare WHERE ProviderTenantId = p_tenantId OR ConsumerTenantId = p_tenantId;

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

    DELETE ccvc FROM CompanyCaseValueChange ccvc
    INNER JOIN CompanyCaseChange ccc ON ccvc.CaseChangeId = ccc.Id
    WHERE ccc.TenantId = p_tenantId;

    DELETE FROM CompanyCaseChange WHERE TenantId = p_tenantId;

    DELETE ccd FROM CompanyCaseDocument ccd
    INNER JOIN CompanyCaseValue ccv ON ccd.CaseValueId = ccv.Id
    WHERE ccv.TenantId = p_tenantId;

    DELETE FROM CompanyCaseValue WHERE TenantId = p_tenantId;

    DELETE ncvc FROM NationalCaseValueChange ncvc
    INNER JOIN NationalCaseChange ncc ON ncvc.CaseChangeId = ncc.Id
    WHERE ncc.TenantId = p_tenantId;

    DELETE FROM NationalCaseChange WHERE TenantId = p_tenantId;

    DELETE ncd FROM NationalCaseDocument ncd
    INNER JOIN NationalCaseValue ncv ON ncd.CaseValueId = ncv.Id
    WHERE ncv.TenantId = p_tenantId;

    DELETE FROM NationalCaseValue WHERE TenantId = p_tenantId;

    DELETE gcvc FROM GlobalCaseValueChange gcvc
    INNER JOIN GlobalCaseChange gcc ON gcvc.CaseChangeId = gcc.Id
    WHERE gcc.TenantId = p_tenantId;

    DELETE FROM GlobalCaseChange WHERE TenantId = p_tenantId;

    DELETE gcd FROM GlobalCaseDocument gcd
    INNER JOIN GlobalCaseValue gcv ON gcd.CaseValueId = gcv.Id
    WHERE gcv.TenantId = p_tenantId;

    DELETE FROM GlobalCaseValue WHERE TenantId = p_tenantId;

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

DELIMITER ;

-- GetCollectorCustomResults.mysql.sql
-- =============================================================================
-- GetCollectorCustomResults
-- =============================================================================

USE PayrollEngine;

DELIMITER $$

DROP PROCEDURE IF EXISTS GetCollectorCustomResults$$
CREATE PROCEDURE GetCollectorCustomResults(
    IN p_tenantId            INT,
    IN p_employeeId          INT,
    IN p_divisionId          INT,
    IN p_payrunJobId         INT,
    IN p_parentPayrunJobId   INT,
    IN p_collectorNameHashes VARCHAR(4000),
    IN p_periodStart         DATETIME(6),
    IN p_periodEnd           DATETIME(6),
    IN p_jobStatus           INT,
    IN p_forecast            VARCHAR(128),
    IN p_evaluationDate      DATETIME(6)
)
BEGIN
    DECLARE v_collectorNameHash INT;
    DECLARE v_collectorCount    INT;

    SET v_collectorCount = IF(p_collectorNameHashes IS NULL, 0, JSON_LENGTH(p_collectorNameHashes));

    IF v_collectorCount = 1 THEN
        SELECT CAST(jt.val AS SIGNED) INTO v_collectorNameHash
        FROM JSON_TABLE(p_collectorNameHashes, '$[*]' COLUMNS (val VARCHAR(20) PATH '$')) AS jt
        LIMIT 1;
    END IF;

    SELECT ccr.*
    FROM CollectorCustomResult ccr
    WHERE ccr.TenantId = p_tenantId
      AND ccr.EmployeeId = p_employeeId
      AND (p_divisionId IS NULL        OR ccr.DivisionId = p_divisionId)
      AND (p_payrunJobId IS NULL       OR ccr.PayrunJobId = p_payrunJobId)
      AND (p_parentPayrunJobId IS NULL OR ccr.ParentJobId = p_parentPayrunJobId)
      AND (p_collectorNameHashes IS NULL OR v_collectorCount = 0
           OR (v_collectorCount = 1 AND ccr.CollectorNameHash = v_collectorNameHash)
           OR (v_collectorCount > 1 AND ccr.CollectorNameHash IN (
               SELECT CAST(jt.val AS SIGNED)
               FROM JSON_TABLE(p_collectorNameHashes, '$[*]' COLUMNS (val VARCHAR(20) PATH '$')) AS jt)))
      AND (p_periodStart IS NULL OR ccr.Start BETWEEN p_periodStart AND p_periodEnd)
      AND (p_jobStatus IS NULL OR ccr.PayrunJobId IN (
               SELECT pj.Id FROM PayrunJob pj
               WHERE pj.Id = ccr.PayrunJobId
                 AND (pj.JobStatus & p_jobStatus) = pj.JobStatus))
      AND (ccr.Forecast IS NULL OR ccr.Forecast = p_forecast)
      AND (p_evaluationDate IS NULL OR ccr.Created <= p_evaluationDate)
    ORDER BY ccr.Created;
END$$

DELIMITER ;

-- GetCollectorResults.mysql.sql
-- =============================================================================
-- GetCollectorResults
-- =============================================================================

USE PayrollEngine;

DELIMITER $$

DROP PROCEDURE IF EXISTS GetCollectorResults$$
CREATE PROCEDURE GetCollectorResults(
    IN p_tenantId            INT,
    IN p_employeeId          INT,
    IN p_divisionId          INT,
    IN p_payrunJobId         INT,
    IN p_parentPayrunJobId   INT,
    IN p_collectorNameHashes VARCHAR(4000),
    IN p_periodStart         DATETIME(6),
    IN p_periodEnd           DATETIME(6),
    IN p_jobStatus           INT,
    IN p_forecast            VARCHAR(128),
    IN p_evaluationDate      DATETIME(6)
)
BEGIN
    DECLARE v_collectorNameHash INT;
    DECLARE v_collectorCount    INT;

    SET v_collectorCount = IF(p_collectorNameHashes IS NULL, 0, JSON_LENGTH(p_collectorNameHashes));

    IF v_collectorCount = 1 THEN
        SELECT CAST(jt.val AS SIGNED) INTO v_collectorNameHash
        FROM JSON_TABLE(p_collectorNameHashes, '$[*]' COLUMNS (val VARCHAR(20) PATH '$')) AS jt
        LIMIT 1;
    END IF;

    SELECT cr.*
    FROM CollectorResult cr
    WHERE cr.TenantId = p_tenantId
      AND cr.EmployeeId = p_employeeId
      AND (p_divisionId IS NULL        OR cr.DivisionId = p_divisionId)
      AND (p_payrunJobId IS NULL       OR cr.PayrunJobId = p_payrunJobId)
      AND (p_parentPayrunJobId IS NULL OR cr.ParentJobId = p_parentPayrunJobId)
      AND (p_collectorNameHashes IS NULL OR v_collectorCount = 0
           OR (v_collectorCount = 1 AND cr.CollectorNameHash = v_collectorNameHash)
           OR (v_collectorCount > 1 AND cr.CollectorNameHash IN (
               SELECT CAST(jt.val AS SIGNED)
               FROM JSON_TABLE(p_collectorNameHashes, '$[*]' COLUMNS (val VARCHAR(20) PATH '$')) AS jt)))
      AND (p_periodStart IS NULL OR cr.Start BETWEEN p_periodStart AND p_periodEnd)
      AND (p_jobStatus IS NULL OR cr.PayrunJobId IN (
               SELECT pj.Id FROM PayrunJob pj
               WHERE pj.Id = cr.PayrunJobId
                 AND (pj.JobStatus & p_jobStatus) = pj.JobStatus))
      AND (cr.Forecast IS NULL OR cr.Forecast = p_forecast)
      AND (p_evaluationDate IS NULL OR cr.Created <= p_evaluationDate)
    ORDER BY cr.Created;
END$$

DELIMITER ;

-- GetCompanyCaseChangeValues.mysql.sql
-- =============================================================================
-- GetCompanyCaseChangeValues
-- =============================================================================

USE PayrollEngine;

DELIMITER $$

DROP PROCEDURE IF EXISTS GetCompanyCaseChangeValues$$
CREATE PROCEDURE GetCompanyCaseChangeValues(
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

    SET v_attrSql  = BuildAttributeQuery('CompanyCaseValue.Attributes', p_attributes);
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

DELIMITER ;

-- GetCompanyCaseValues.mysql.sql
-- =============================================================================
-- GetCompanyCaseValues
-- =============================================================================

USE PayrollEngine;

DELIMITER $$

DROP PROCEDURE IF EXISTS GetCompanyCaseValues$$
CREATE PROCEDURE GetCompanyCaseValues(
    IN p_parentId   INT,
    IN p_sql        LONGTEXT,
    IN p_attributes LONGTEXT
)
BEGIN
    DECLARE v_attrSql  LONGTEXT;
    DECLARE v_pivotSql LONGTEXT;

    DECLARE EXIT HANDLER FOR SQLEXCEPTION
    BEGIN
        DROP TEMPORARY TABLE IF EXISTS CompanyCaseValuePivot;
        RESIGNAL;
    END;

    SET v_attrSql  = BuildAttributeQuery('CompanyCaseValue.Attributes', p_attributes);
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

DELIMITER ;

-- GetConsolidatedCollectorCustomResults.mysql.sql
-- =============================================================================
-- GetConsolidatedCollectorCustomResults
-- =============================================================================

USE PayrollEngine;

DELIMITER $$

DROP PROCEDURE IF EXISTS GetConsolidatedCollectorCustomResults$$
CREATE PROCEDURE GetConsolidatedCollectorCustomResults(
    IN p_tenantId            INT,
    IN p_employeeId          INT,
    IN p_divisionId          INT,
    IN p_collectorNameHashes VARCHAR(4000),
    IN p_periodStartHashes   VARCHAR(4000),
    IN p_jobStatus           INT,
    IN p_forecast            VARCHAR(128),
    IN p_evaluationDate      DATETIME(6),
    IN p_noRetro             TINYINT(1),
    IN p_excludeParentJobId  INT
)
BEGIN
    DECLARE v_collectorNameHash INT;
    DECLARE v_collectorCount    INT;
    DECLARE v_startHash         INT;
    DECLARE v_startHashCount    INT;

    SET v_collectorCount = IF(p_collectorNameHashes IS NULL, 0, JSON_LENGTH(p_collectorNameHashes));
    SET v_startHashCount = IF(p_periodStartHashes IS NULL,   0, JSON_LENGTH(p_periodStartHashes));

    IF v_collectorCount = 1 THEN
        SELECT CAST(jt.val AS SIGNED) INTO v_collectorNameHash
        FROM JSON_TABLE(p_collectorNameHashes, '$[*]' COLUMNS (val VARCHAR(20) PATH '$')) AS jt LIMIT 1;
    END IF;

    -- single-hash fast path: equality seek on StartHash
    IF v_startHashCount = 1 THEN
        SELECT CAST(jt.val AS SIGNED) INTO v_startHash
        FROM JSON_TABLE(p_periodStartHashes, '$[*]' COLUMNS (val VARCHAR(20) PATH '$')) AS jt LIMIT 1;
    END IF;

    -- Phase 1: select winning IDs via index-only scan
    -- Index key order: (TenantId, EmployeeId, StartHash, CollectorNameHash)
    -- → seeks directly to the period, constant cost regardless of history
    WITH Winners AS (
        SELECT r.Id,
            ROW_NUMBER() OVER (
                PARTITION BY r.CollectorNameHash, r.Start
                ORDER BY r.Created DESC, r.Id DESC
            ) AS RowNumber
        FROM CollectorCustomResult r
        WHERE r.TenantId = p_tenantId
          AND r.EmployeeId = p_employeeId
          -- period filter: single hash → equality seek; multiple → IN list
          AND (v_startHashCount = 0 OR
               (v_startHashCount = 1 AND r.StartHash = v_startHash) OR
               (v_startHashCount > 1 AND r.StartHash IN (
                   SELECT CAST(jt.val AS SIGNED)
                   FROM JSON_TABLE(p_periodStartHashes, '$[*]' COLUMNS (val VARCHAR(20) PATH '$')) AS jt)))
          AND (p_divisionId IS NULL OR r.DivisionId = p_divisionId)
          AND (p_collectorNameHashes IS NULL OR v_collectorCount = 0
               OR (v_collectorCount = 1 AND r.CollectorNameHash = v_collectorNameHash)
               OR (v_collectorCount > 1 AND r.CollectorNameHash IN (
                   SELECT CAST(jt.val AS SIGNED)
                   FROM JSON_TABLE(p_collectorNameHashes, '$[*]' COLUMNS (val VARCHAR(20) PATH '$')) AS jt)))
          AND (p_evaluationDate IS NULL OR r.Created <= p_evaluationDate)
          AND (p_jobStatus IS NULL OR r.PayrunJobId IN (
                   SELECT pj.Id FROM PayrunJob pj WHERE (pj.JobStatus & p_jobStatus) = pj.JobStatus))
          AND (r.Forecast IS NULL OR r.Forecast = p_forecast)
          AND (p_noRetro = 0 OR r.ParentJobId IS NULL)
          AND (p_excludeParentJobId IS NULL OR r.ParentJobId IS NULL
               OR r.ParentJobId <> p_excludeParentJobId)
    )
    -- Phase 2: key lookup only for winning rows
    SELECT r.*
    FROM CollectorCustomResult r
    INNER JOIN Winners w ON w.Id = r.Id
    WHERE w.RowNumber = 1;
END$$

DELIMITER ;

-- GetConsolidatedCollectorResults.mysql.sql
-- =============================================================================
-- GetConsolidatedCollectorResults
-- =============================================================================

USE PayrollEngine;

DELIMITER $$

DROP PROCEDURE IF EXISTS GetConsolidatedCollectorResults$$
CREATE PROCEDURE GetConsolidatedCollectorResults(
    IN p_tenantId            INT,
    IN p_employeeId          INT,
    IN p_divisionId          INT,
    IN p_collectorNameHashes VARCHAR(4000),
    IN p_periodStartHashes   VARCHAR(4000),
    IN p_jobStatus           INT,
    IN p_forecast            VARCHAR(128),
    IN p_evaluationDate      DATETIME(6),
    IN p_noRetro             TINYINT(1),
    IN p_excludeParentJobId  INT
)
BEGIN
    DECLARE v_collectorNameHash INT;
    DECLARE v_collectorCount    INT;
    DECLARE v_startHash         INT;
    DECLARE v_startHashCount    INT;

    SET v_collectorCount = IF(p_collectorNameHashes IS NULL, 0, JSON_LENGTH(p_collectorNameHashes));
    SET v_startHashCount = IF(p_periodStartHashes IS NULL,   0, JSON_LENGTH(p_periodStartHashes));

    IF v_collectorCount = 1 THEN
        SELECT CAST(jt.val AS SIGNED) INTO v_collectorNameHash
        FROM JSON_TABLE(p_collectorNameHashes, '$[*]' COLUMNS (val VARCHAR(20) PATH '$')) AS jt LIMIT 1;
    END IF;

    -- single-hash fast path: equality seek on StartHash
    IF v_startHashCount = 1 THEN
        SELECT CAST(jt.val AS SIGNED) INTO v_startHash
        FROM JSON_TABLE(p_periodStartHashes, '$[*]' COLUMNS (val VARCHAR(20) PATH '$')) AS jt LIMIT 1;
    END IF;

    -- Phase 1: select winning IDs via index-only scan
    -- Index key order: (TenantId, EmployeeId, StartHash, CollectorNameHash)
    -- → seeks directly to the period, constant cost regardless of history
    WITH Winners AS (
        SELECT r.Id,
            ROW_NUMBER() OVER (
                PARTITION BY r.CollectorNameHash, r.Start
                ORDER BY r.Created DESC, r.Id DESC
            ) AS RowNumber
        FROM CollectorResult r
        WHERE r.TenantId = p_tenantId
          AND r.EmployeeId = p_employeeId
          -- period filter: single hash → equality seek; multiple → IN list
          AND (v_startHashCount = 0 OR
               (v_startHashCount = 1 AND r.StartHash = v_startHash) OR
               (v_startHashCount > 1 AND r.StartHash IN (
                   SELECT CAST(jt.val AS SIGNED)
                   FROM JSON_TABLE(p_periodStartHashes, '$[*]' COLUMNS (val VARCHAR(20) PATH '$')) AS jt)))
          AND (p_divisionId IS NULL OR r.DivisionId = p_divisionId)
          AND (p_collectorNameHashes IS NULL OR v_collectorCount = 0
               OR (v_collectorCount = 1 AND r.CollectorNameHash = v_collectorNameHash)
               OR (v_collectorCount > 1 AND r.CollectorNameHash IN (
                   SELECT CAST(jt.val AS SIGNED)
                   FROM JSON_TABLE(p_collectorNameHashes, '$[*]' COLUMNS (val VARCHAR(20) PATH '$')) AS jt)))
          AND (p_evaluationDate IS NULL OR r.Created <= p_evaluationDate)
          AND (p_jobStatus IS NULL OR r.PayrunJobId IN (
                   SELECT pj.Id FROM PayrunJob pj WHERE (pj.JobStatus & p_jobStatus) = pj.JobStatus))
          AND (r.Forecast IS NULL OR r.Forecast = p_forecast)
          AND (p_noRetro = 0 OR r.ParentJobId IS NULL)
          AND (p_excludeParentJobId IS NULL OR r.ParentJobId IS NULL
               OR r.ParentJobId <> p_excludeParentJobId)
    )
    -- Phase 2: key lookup only for winning rows
    SELECT r.*
    FROM CollectorResult r
    INNER JOIN Winners w ON w.Id = r.Id
    WHERE w.RowNumber = 1;
END$$

DELIMITER ;

-- GetConsolidatedPayrunResults.mysql.sql
-- =============================================================================
-- GetConsolidatedPayrunResults
-- =============================================================================

USE PayrollEngine;

DELIMITER $$

DROP PROCEDURE IF EXISTS GetConsolidatedPayrunResults$$
CREATE PROCEDURE GetConsolidatedPayrunResults(
    IN p_tenantId           INT,
    IN p_employeeId         INT,
    IN p_divisionId         INT,
    IN p_names              VARCHAR(4000),
    IN p_periodStartHashes  VARCHAR(4000),
    IN p_jobStatus          INT,
    IN p_forecast           VARCHAR(128),
    IN p_evaluationDate     DATETIME(6),
    IN p_noRetro            TINYINT(1),
    IN p_excludeParentJobId INT
)
BEGIN
    DECLARE v_name           VARCHAR(128);
    DECLARE v_nameCount      INT;
    DECLARE v_startHash      INT;
    DECLARE v_startHashCount INT;

    SET v_nameCount      = IF(p_names IS NULL,             0, JSON_LENGTH(p_names));
    SET v_startHashCount = IF(p_periodStartHashes IS NULL, 0, JSON_LENGTH(p_periodStartHashes));

    IF v_nameCount = 1 THEN
        SELECT jt.val INTO v_name
        FROM JSON_TABLE(p_names, '$[*]' COLUMNS (val VARCHAR(128) PATH '$')) AS jt LIMIT 1;
    END IF;

    -- single-hash fast path: equality seek on StartHash
    IF v_startHashCount = 1 THEN
        SELECT CAST(jt.val AS SIGNED) INTO v_startHash
        FROM JSON_TABLE(p_periodStartHashes, '$[*]' COLUMNS (val VARCHAR(20) PATH '$')) AS jt LIMIT 1;
    END IF;

    -- Phase 1: select winning IDs via index-only scan
    -- Index key order: (TenantId, EmployeeId, StartHash, Name)
    -- → seeks directly to the period, constant cost regardless of history
    WITH Winners AS (
        SELECT r.Id,
            ROW_NUMBER() OVER (
                PARTITION BY r.Name, r.Start
                ORDER BY r.Created DESC, r.Id DESC
            ) AS RowNumber
        FROM PayrunResult r
        WHERE r.TenantId = p_tenantId
          AND r.EmployeeId = p_employeeId
          -- period filter: single hash → equality seek; multiple → IN list
          AND (v_startHashCount = 0 OR
               (v_startHashCount = 1 AND r.StartHash = v_startHash) OR
               (v_startHashCount > 1 AND r.StartHash IN (
                   SELECT CAST(jt.val AS SIGNED)
                   FROM JSON_TABLE(p_periodStartHashes, '$[*]' COLUMNS (val VARCHAR(20) PATH '$')) AS jt)))
          AND (p_divisionId IS NULL OR r.DivisionId = p_divisionId)
          AND (p_names IS NULL OR v_nameCount = 0
               OR (v_nameCount = 1 AND r.Name = v_name)
               OR (v_nameCount > 1 AND r.Name IN (
                   SELECT jt.val
                   FROM JSON_TABLE(p_names, '$[*]' COLUMNS (val VARCHAR(128) PATH '$')) AS jt)))
          AND (p_evaluationDate IS NULL OR r.Created <= p_evaluationDate)
          AND (p_jobStatus IS NULL OR r.PayrunJobId IN (
                   SELECT pj.Id FROM PayrunJob pj WHERE (pj.JobStatus & p_jobStatus) = pj.JobStatus))
          AND (r.Forecast IS NULL OR r.Forecast = p_forecast)
          AND (p_noRetro = 0 OR r.ParentJobId IS NULL)
          AND (p_excludeParentJobId IS NULL OR r.ParentJobId IS NULL
               OR r.ParentJobId <> p_excludeParentJobId)
    )
    -- Phase 2: key lookup only for winning rows
    SELECT r.*
    FROM PayrunResult r
    INNER JOIN Winners w ON w.Id = r.Id
    WHERE w.RowNumber = 1;
END$$

DELIMITER ;

-- GetConsolidatedWageTypeCustomResults.mysql.sql
-- =============================================================================
-- GetConsolidatedWageTypeCustomResults
-- =============================================================================

USE PayrollEngine;

DELIMITER $$

DROP PROCEDURE IF EXISTS GetConsolidatedWageTypeCustomResults$$
CREATE PROCEDURE GetConsolidatedWageTypeCustomResults(
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

    -- single-hash fast path: equality seek on StartHash
    IF v_startHashCount = 1 THEN
        SELECT CAST(jt.val AS SIGNED) INTO v_startHash
        FROM JSON_TABLE(p_periodStartHashes, '$[*]' COLUMNS (val VARCHAR(20) PATH '$')) AS jt LIMIT 1;
    END IF;

    -- Phase 1: select winning IDs via index-only scan
    -- Index key order: (TenantId, EmployeeId, StartHash, WageTypeNumber)
    -- → seeks directly to the period, constant cost regardless of history
    WITH Winners AS (
        SELECT r.Id,
            ROW_NUMBER() OVER (
                PARTITION BY r.WageTypeNumber, r.Start
                ORDER BY r.Created DESC, r.Id DESC
            ) AS RowNumber
        FROM WageTypeCustomResult r
        WHERE r.TenantId = p_tenantId
          AND r.EmployeeId = p_employeeId
          -- period filter: single hash → equality seek; multiple → IN list
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
    -- Phase 2: key lookup only for winning rows
    SELECT r.*
    FROM WageTypeCustomResult r
    INNER JOIN Winners w ON w.Id = r.Id
    WHERE w.RowNumber = 1;
END$$

DELIMITER ;

-- GetConsolidatedWageTypeResults.mysql.sql
-- =============================================================================
-- GetConsolidatedWageTypeResults
-- ;WITH Winners AS -> WITH Winners AS (MySQL 8.0+ supports CTEs in SPs)
-- OPTION (RECOMPILE) -> removed
-- =============================================================================

USE PayrollEngine;

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

    -- single-hash fast path: equality seek on StartHash
    IF v_startHashCount = 1 THEN
        SELECT CAST(jt.val AS SIGNED) INTO v_startHash
        FROM JSON_TABLE(p_periodStartHashes, '$[*]' COLUMNS (val VARCHAR(20) PATH '$')) AS jt LIMIT 1;
    END IF;

    -- Phase 1: select winning IDs via index-only scan
    -- Index key order: (TenantId, EmployeeId, StartHash, WageTypeNumber)
    -- → seeks directly to the period, constant cost regardless of history
    WITH Winners AS (
        SELECT r.Id,
            ROW_NUMBER() OVER (
                PARTITION BY r.WageTypeNumber, r.Start
                ORDER BY r.Created DESC, r.Id DESC
            ) AS RowNumber
        FROM WageTypeResult r
        WHERE r.TenantId = p_tenantId
          AND r.EmployeeId = p_employeeId
          -- period filter: single hash → equality seek; multiple → IN list
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
    -- Phase 2: key lookup only for winning rows
    SELECT r.*
    FROM WageTypeResult r
    INNER JOIN Winners w ON w.Id = r.Id
    WHERE w.RowNumber = 1;
END$$

DELIMITER ;

-- GetDerivedCaseFields.mysql.sql
-- =============================================================================
-- GetDerivedCaseFields
-- Filtered by case field names.
-- =============================================================================

USE PayrollEngine;

DELIMITER $$

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
        reg.Id AS RegulationId, reg.Level, reg.Priority,
        c.Id AS CaseId, c.CaseType,
        cf.*
    FROM CaseField cf
    INNER JOIN `Case` c ON cf.CaseId = c.Id
    INNER JOIN Regulations reg ON c.RegulationId = reg.Id
    WHERE cf.Status = 0
      AND cf.Created <= p_createdBefore
      AND ((p_includeClusters IS NULL AND p_excludeClusters IS NULL)
           OR IsMatchingCluster(p_includeClusters, p_excludeClusters, cf.Clusters) = 1)
      AND (p_caseFieldNames IS NULL
           OR LOWER(cf.Name) IN (
               SELECT LOWER(jt.val)
               FROM JSON_TABLE(p_caseFieldNames, '$[*]' COLUMNS (val VARCHAR(128) PATH '$')) AS jt))
    ORDER BY reg.Level DESC, reg.Priority DESC;
END$$

DELIMITER ;

-- GetDerivedCaseFieldsOfCase.mysql.sql
-- =============================================================================
-- GetDerivedCaseFieldsOfCase
-- Filtered by case names (not field names).
-- =============================================================================

USE PayrollEngine;

DELIMITER $$

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
        reg.Id AS RegulationId, reg.Level, reg.Priority,
        c.Id AS CaseId, c.CaseType,
        cf.*
    FROM CaseField cf
    INNER JOIN `Case` c ON cf.CaseId = c.Id
    INNER JOIN Regulations reg ON c.RegulationId = reg.Id
    WHERE cf.Status = 0
      AND cf.Created <= p_createdBefore
      AND ((p_includeClusters IS NULL AND p_excludeClusters IS NULL)
           OR IsMatchingCluster(p_includeClusters, p_excludeClusters, cf.Clusters) = 1)
      AND (p_caseNames IS NULL
           OR LOWER(c.Name) IN (
               SELECT LOWER(jt.val)
               FROM JSON_TABLE(p_caseNames, '$[*]' COLUMNS (val VARCHAR(128) PATH '$')) AS jt))
    ORDER BY reg.Level DESC, reg.Priority DESC;
END$$

DELIMITER ;

-- GetDerivedCaseRelations.mysql.sql
-- =============================================================================
-- GetDerivedCaseRelations
-- cr.`Order` backtick-quoted (reserved keyword in MySQL)
-- Excludes Binary, Script, ScriptVersion (performance hint)
-- =============================================================================

USE PayrollEngine;

DELIMITER $$

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
        reg.Id AS RegulationId, reg.Level, reg.Priority,
        cr.Id, cr.Status, cr.Created, cr.Updated, cr.RegulationId,
        cr.SourceCaseName, cr.SourceCaseNameLocalizations,
        cr.SourceCaseSlot, cr.SourceCaseSlotLocalizations,
        cr.TargetCaseName, cr.TargetCaseNameLocalizations,
        cr.TargetCaseSlot, cr.TargetCaseSlotLocalizations,
        cr.RelationHash, cr.BuildExpression, cr.ValidateExpression,
        cr.OverrideType, cr.`Order`,
        cr.ScriptHash, cr.Attributes, cr.Clusters,
        cr.BuildActions, cr.ValidateActions
    FROM CaseRelation cr
    INNER JOIN Regulations reg ON cr.RegulationId = reg.Id
    WHERE cr.Status = 0
      AND cr.Created <= p_createdBefore
      AND (p_sourceCaseName IS NULL
           OR LOWER(cr.SourceCaseName) = LOWER(p_sourceCaseName))
      AND (p_targetCaseName IS NULL
           OR LOWER(cr.TargetCaseName) = LOWER(p_targetCaseName))
      AND ((p_includeClusters IS NULL AND p_excludeClusters IS NULL)
           OR IsMatchingCluster(p_includeClusters, p_excludeClusters, cr.Clusters) = 1)
    ORDER BY cr.SourceCaseName, cr.TargetCaseName, reg.Level DESC, reg.Priority DESC;
END$$

DELIMITER ;

-- GetDerivedCases.mysql.sql
-- =============================================================================
-- GetDerivedCases
-- Excludes Binary, Script, ScriptVersion (performance hint identical to T-SQL)
-- =============================================================================

USE PayrollEngine;

DELIMITER $$

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
        reg.Id AS RegulationId, reg.Level, reg.Priority,
        c.Id, c.Status, c.Created, c.Updated, c.RegulationId,
        c.CaseType, c.Name, c.NameLocalizations, c.NameSynonyms,
        c.Description, c.DescriptionLocalizations,
        c.DefaultReason, c.DefaultReasonLocalizations,
        c.BaseCase, c.BaseCaseFields,
        c.OverrideType, c.CancellationType,
        c.AvailableExpression, c.BuildExpression, c.ValidateExpression,
        c.Lookups, c.Slots,
        c.ScriptHash, c.Attributes, c.Clusters,
        c.AvailableActions, c.BuildActions, c.ValidateActions
    FROM `Case` c
    INNER JOIN Regulations reg ON c.RegulationId = reg.Id
    WHERE c.Status = 0
      AND c.Created <= p_createdBefore
      AND (p_hidden IS NULL OR c.Hidden = p_hidden)
      AND (p_caseType IS NULL OR c.CaseType = p_caseType)
      AND ((p_includeClusters IS NULL AND p_excludeClusters IS NULL)
           OR IsMatchingCluster(p_includeClusters, p_excludeClusters, c.Clusters) = 1)
      AND (p_caseNames IS NULL
           OR LOWER(c.Name) IN (
               SELECT LOWER(jt.val)
               FROM JSON_TABLE(p_caseNames, '$[*]' COLUMNS (val VARCHAR(128) PATH '$')) AS jt))
    ORDER BY reg.Level DESC, reg.Priority DESC;
END$$

DELIMITER ;

-- GetDerivedCollectors.mysql.sql
-- =============================================================================
-- GetDerivedCollectors
-- Excludes Binary, Script, ScriptVersion (performance hint)
-- =============================================================================

USE PayrollEngine;

DELIMITER $$

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
        reg.Id AS RegulationId, reg.Level, reg.Priority,
        co.Id, co.Status, co.Created, co.Updated, co.RegulationId,
        co.Name, co.NameLocalizations,
        co.CollectMode, co.Negated, co.OverrideType, co.ValueType,
        co.Culture, co.CollectorGroups,
        co.StartExpression, co.ApplyExpression, co.EndExpression,
        co.StartActions, co.ApplyActions, co.EndActions,
        co.Threshold, co.MinResult, co.MaxResult,
        co.ScriptHash, co.Attributes, co.Clusters
    FROM Collector co
    INNER JOIN Regulations reg ON co.RegulationId = reg.Id
    WHERE co.Status = 0
      AND co.Created <= p_createdBefore
      AND ((p_includeClusters IS NULL AND p_excludeClusters IS NULL)
           OR IsMatchingCluster(p_includeClusters, p_excludeClusters, co.Clusters) = 1)
      AND (p_collectorNames IS NULL
           OR LOWER(co.Name) IN (
               SELECT LOWER(jt.val)
               FROM JSON_TABLE(p_collectorNames, '$[*]' COLUMNS (val VARCHAR(128) PATH '$')) AS jt))
    ORDER BY co.Name, reg.Level DESC, reg.Priority DESC;
END$$

DELIMITER ;

-- GetDerivedLookups.mysql.sql
-- =============================================================================
-- GetDerivedLookups
-- =============================================================================

USE PayrollEngine;

DELIMITER $$

DROP PROCEDURE IF EXISTS GetDerivedLookups$$
CREATE PROCEDURE GetDerivedLookups(
    IN p_tenantId       INT,
    IN p_payrollId      INT,
    IN p_regulationDate DATETIME(6),
    IN p_createdBefore  DATETIME(6),
    IN p_lookupNames    VARCHAR(4000)
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
        reg.Id AS RegulationId, reg.Level, reg.Priority,
        lk.*
    FROM Lookup lk
    INNER JOIN Regulations reg ON lk.RegulationId = reg.Id
    WHERE lk.Status = 0
      AND lk.Created <= p_createdBefore
      AND (p_lookupNames IS NULL
           OR LOWER(lk.Name) IN (
               SELECT LOWER(jt.val)
               FROM JSON_TABLE(p_lookupNames, '$[*]' COLUMNS (val VARCHAR(128) PATH '$')) AS jt))
    ORDER BY reg.Level DESC, reg.Priority DESC;
END$$

DELIMITER ;

-- GetDerivedLookupValues.mysql.sql
-- =============================================================================
-- GetDerivedLookupValues
-- lv.`Key` backtick-quoted (reserved keyword in MySQL)
-- Case-sensitive key filter (no LOWER(), identical to T-SQL)
-- =============================================================================

USE PayrollEngine;

DELIMITER $$

DROP PROCEDURE IF EXISTS GetDerivedLookupValues$$
CREATE PROCEDURE GetDerivedLookupValues(
    IN p_tenantId       INT,
    IN p_payrollId      INT,
    IN p_regulationDate DATETIME(6),
    IN p_createdBefore  DATETIME(6),
    IN p_lookupNames    VARCHAR(4000),
    IN p_lookupKeys     VARCHAR(4000)
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
        reg.Id AS RegulationId, reg.Level, reg.Priority,
        lv.*
    FROM LookupValue lv
    INNER JOIN Lookup lk ON lv.LookupId = lk.Id
    INNER JOIN Regulations reg ON lk.RegulationId = reg.Id
    WHERE lv.Status = 0
      AND lv.Created <= p_createdBefore
      AND (p_lookupNames IS NULL
           OR LOWER(lk.Name) IN (
               SELECT LOWER(jt.val)
               FROM JSON_TABLE(p_lookupNames, '$[*]' COLUMNS (val VARCHAR(128) PATH '$')) AS jt))
      AND (p_lookupKeys IS NULL
           OR lv.`Key` IN (
               SELECT jt.val
               FROM JSON_TABLE(p_lookupKeys, '$[*]' COLUMNS (val VARCHAR(128) PATH '$')) AS jt))
    ORDER BY reg.Level DESC, reg.Priority DESC;
END$$

DELIMITER ;

-- GetDerivedPayrollRegulations.mysql.sql
-- =============================================================================
-- GetDerivedPayrollRegulations
-- T-SQL: SELECT * FROM dbo.GetDerivedRegulations(...)
-- MySQL: GetDerivedRegulations eliminated -- inlined as CTE
-- IsolationLevel >= 3 (Write) required for shared regulations as payroll layers;
-- Consolidation (1) and Read (2) shares are excluded from layer resolution.
-- =============================================================================

USE PayrollEngine;

DELIMITER $$

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
            ROW_NUMBER() OVER (
                PARTITION BY pl.Id, r.Name
                ORDER BY r.ValidFrom DESC, r.Created DESC
            ) AS RowNumber
        FROM PayrollLayer pl
        INNER JOIN Regulation r ON pl.RegulationName = r.Name
        WHERE r.Status = 0
          AND (
            r.TenantId = p_tenantId
            -- shared regulation: IsolationLevel must be >= Write to act as payroll layer.
            -- Write = 3 (TenantIsolationLevel enum, PayrollEngine.Core).
            -- IMPORTANT: if TenantIsolationLevel enum values change, this literal must
            -- be updated in sync. The CK_RegulationShare_IsolationLevel check constraint
            -- enforces the allowed set and will fail on INSERT if the enum is extended.
            OR (
              r.SharedRegulation = 1
              AND EXISTS (
                SELECT 1 FROM RegulationShare rs
                WHERE rs.ProviderRegulationId = r.Id
                  AND rs.ConsumerTenantId     = p_tenantId
                  AND rs.IsolationLevel       >= 3  -- TenantIsolationLevel.Write
              )
            )
          )
          AND r.Created <= p_createdBefore
          AND (r.ValidFrom IS NULL OR r.ValidFrom <= p_regulationDate)
          AND pl.Status = 0 AND pl.PayrollId = p_payrollId
    ),
    Regulations AS (SELECT Id, Level, Priority FROM DerivedRegulations WHERE RowNumber = 1)
    SELECT r.*, reg.Level, reg.Priority
    FROM Regulation r
    INNER JOIN Regulations reg ON r.Id = reg.Id
    ORDER BY reg.Level DESC, reg.Priority DESC;
END$$

DELIMITER ;

-- GetDerivedReportParameters.mysql.sql
-- =============================================================================
-- GetDerivedReportParameters
-- =============================================================================

USE PayrollEngine;

DELIMITER $$

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
        reg.Id AS RegulationId, reg.Level, reg.Priority,
        rpar.*
    FROM ReportParameter rpar
    INNER JOIN Report rp ON rpar.ReportId = rp.Id
    INNER JOIN Regulations reg ON rp.RegulationId = reg.Id
    WHERE rpar.Status = 0
      AND rpar.Created <= p_createdBefore
      AND (p_reportNames IS NULL
           OR LOWER(rp.Name) IN (
               SELECT LOWER(jt.val)
               FROM JSON_TABLE(p_reportNames, '$[*]' COLUMNS (val VARCHAR(128) PATH '$')) AS jt))
    ORDER BY reg.Level DESC, reg.Priority DESC;
END$$

DELIMITER ;

-- GetDerivedReports.mysql.sql
-- =============================================================================
-- GetDerivedReports
-- Excludes Binary, Script, ScriptVersion (performance hint)
-- =============================================================================

USE PayrollEngine;

DELIMITER $$

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
          AND (r.TenantId = p_tenantId OR r.SharedRegulation = 1)
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

-- GetDerivedReportTemplates.mysql.sql
-- =============================================================================
-- GetDerivedReportTemplates
-- `Schema` column is backtick-quoted (reserved word in MySQL)
-- SELECT rt.* expands safely
-- =============================================================================

USE PayrollEngine;

DELIMITER $$

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
        reg.Id AS RegulationId, reg.Level, reg.Priority,
        rt.*
    FROM ReportTemplate rt
    INNER JOIN Report rp ON rt.ReportId = rp.Id
    INNER JOIN Regulations reg ON rp.RegulationId = reg.Id
    WHERE rt.Status = 0
      AND rt.Created <= p_createdBefore
      AND (p_reportNames IS NULL
           OR LOWER(rp.Name) IN (
               SELECT LOWER(jt.val)
               FROM JSON_TABLE(p_reportNames, '$[*]' COLUMNS (val VARCHAR(128) PATH '$')) AS jt))
      AND (p_culture IS NULL OR rt.Culture = p_culture)
    ORDER BY reg.Level DESC, reg.Priority DESC;
END$$

DELIMITER ;

-- GetDerivedScripts.mysql.sql
-- =============================================================================
-- GetDerivedScripts
-- OverrideType excluded from SELECT (matches T-SQL explicit column list)
-- =============================================================================

USE PayrollEngine;

DELIMITER $$

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
        reg.Id AS RegulationId, reg.Level, reg.Priority,
        s.Id, s.Status, s.Created, s.Updated, s.RegulationId,
        s.Name, s.FunctionTypeMask, s.Value
    FROM Script s
    INNER JOIN Regulations reg ON s.RegulationId = reg.Id
    WHERE s.Status = 0
      AND s.Created <= p_createdBefore
      AND (p_scriptNames IS NULL
           OR LOWER(s.Name) IN (
               SELECT LOWER(jt.val)
               FROM JSON_TABLE(p_scriptNames, '$[*]' COLUMNS (val VARCHAR(128) PATH '$')) AS jt))
    ORDER BY reg.Level DESC, reg.Priority DESC;
END$$

DELIMITER ;

-- GetDerivedWageTypes.mysql.sql
-- =============================================================================
-- GetDerivedWageTypes
-- Excludes Binary, Script, ScriptVersion (performance hint identical to T-SQL)
-- =============================================================================

USE PayrollEngine;

DELIMITER $$

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
        reg.Id AS RegulationId, reg.Level, reg.Priority,
        wt.Id, wt.Status, wt.Created, wt.Updated, wt.RegulationId,
        wt.Name, wt.NameLocalizations, wt.WageTypeNumber,
        wt.Description, wt.DescriptionLocalizations,
        wt.OverrideType, wt.ValueType, wt.Calendar, wt.Culture,
        wt.Collectors, wt.CollectorGroups,
        wt.ValueExpression, wt.ResultExpression,
        wt.ValueActions, wt.ResultActions,
        wt.ScriptHash, wt.Attributes, wt.Clusters
    FROM WageType wt
    INNER JOIN Regulations reg ON wt.RegulationId = reg.Id
    WHERE wt.Status = 0
      AND wt.Created <= p_createdBefore
      AND ((p_includeClusters IS NULL AND p_excludeClusters IS NULL)
           OR IsMatchingCluster(p_includeClusters, p_excludeClusters, wt.Clusters) = 1)
      AND (p_wageTypeNumbers IS NULL
           OR wt.WageTypeNumber IN (
               SELECT CAST(jt.val AS DECIMAL(28,6))
               FROM JSON_TABLE(p_wageTypeNumbers, '$[*]' COLUMNS (val VARCHAR(50) PATH '$')) AS jt))
    ORDER BY wt.WageTypeNumber, reg.Level DESC, reg.Priority DESC;
END$$

DELIMITER ;

-- GetEmployeeCaseChangeValues.mysql.sql
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

-- GetEmployeeCaseValues.mysql.sql
-- =============================================================================
-- GetEmployeeCaseValues
-- Filter is EmployeeId (not TenantId) -- employee-scoped pivot
-- =============================================================================

USE PayrollEngine;

DELIMITER $$

DROP PROCEDURE IF EXISTS GetEmployeeCaseValues$$
CREATE PROCEDURE GetEmployeeCaseValues(
    IN p_parentId   INT,
    IN p_sql        LONGTEXT,
    IN p_attributes LONGTEXT
)
BEGIN
    DECLARE v_attrSql  LONGTEXT;
    DECLARE v_pivotSql LONGTEXT;

    DECLARE EXIT HANDLER FOR SQLEXCEPTION
    BEGIN
        DROP TEMPORARY TABLE IF EXISTS EmployeeCaseValuePivot;
        RESIGNAL;
    END;

    SET v_attrSql  = BuildAttributeQuery('EmployeeCaseValue.Attributes', p_attributes);
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

DELIMITER ;

-- GetEmployeeCaseValuesByTenant.mysql.sql
-- =============================================================================
-- GetEmployeeCaseValuesByTenant
-- Direct JOIN query -- no pivot, no temp table needed.
-- OPENJSON(@fieldNames) -> JSON_TABLE(p_fieldNames, '$[*]' COLUMNS ...)
-- =============================================================================

USE PayrollEngine;

DELIMITER $$

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

DELIMITER ;

-- GetGlobalCaseChangeValues.mysql.sql
-- =============================================================================
-- GetGlobalCaseChangeValues
-- IIF(@culture IS NULL,...) -> IF(p_culture IS NULL,...)
-- `User` backtick-quoted (reserved word in MySQL)
-- =============================================================================

USE PayrollEngine;

DELIMITER $$

DROP PROCEDURE IF EXISTS GetGlobalCaseChangeValues$$
CREATE PROCEDURE GetGlobalCaseChangeValues(
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
        DROP TEMPORARY TABLE IF EXISTS GlobalCaseChangeValuePivot;
        RESIGNAL;
    END;

    IF p_culture IS NULL THEN
        SET v_caseName      = 'GlobalCaseValue.CaseName';
        SET v_caseFieldName = 'GlobalCaseValue.CaseFieldName';
        SET v_caseSlot      = 'GlobalCaseValue.CaseSlot';
    ELSE
        SET v_caseName      = CONCAT('GetLocalizedValue(GlobalCaseValue.CaseNameLocalizations, ''', p_culture, ''', GlobalCaseValue.CaseName)');
        SET v_caseFieldName = CONCAT('GetLocalizedValue(GlobalCaseValue.CaseFieldNameLocalizations, ''', p_culture, ''', GlobalCaseValue.CaseFieldName)');
        SET v_caseSlot      = CONCAT('GetLocalizedValue(GlobalCaseValue.CaseSlotLocalizations, ''', p_culture, ''', GlobalCaseValue.CaseSlot)');
    END IF;

    SET v_attrSql  = BuildAttributeQuery('GlobalCaseValue.Attributes', p_attributes);
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

DELIMITER ;

-- GetGlobalCaseValues.mysql.sql
-- =============================================================================
-- GetGlobalCaseValues
-- Creates TEMPORARY TABLE pivot + executes caller query against it.
-- PREPARE requires user variable (@), not local variable (DECLARE v_...).
-- =============================================================================

USE PayrollEngine;

DELIMITER $$

DROP PROCEDURE IF EXISTS GetGlobalCaseValues$$
CREATE PROCEDURE GetGlobalCaseValues(
    IN p_parentId   INT,
    IN p_sql        LONGTEXT,
    IN p_attributes LONGTEXT
)
BEGIN
    DECLARE v_attrSql  LONGTEXT;
    DECLARE v_pivotSql LONGTEXT;

    DECLARE EXIT HANDLER FOR SQLEXCEPTION
    BEGIN
        DROP TEMPORARY TABLE IF EXISTS GlobalCaseValuePivot;
        RESIGNAL;
    END;

    SET v_attrSql  = BuildAttributeQuery('GlobalCaseValue.Attributes', p_attributes);
    SET v_pivotSql = CONCAT(
        'CREATE TEMPORARY TABLE GlobalCaseValuePivot AS SELECT GlobalCaseValue.*',
        v_attrSql,
        ' FROM GlobalCaseValue WHERE GlobalCaseValue.TenantId = ',
        CAST(p_parentId AS CHAR));

    DROP TEMPORARY TABLE IF EXISTS GlobalCaseValuePivot;

    SET @_pivot_sql = v_pivotSql;
    PREPARE _pivot_stmt FROM @_pivot_sql;
    EXECUTE _pivot_stmt;
    DEALLOCATE PREPARE _pivot_stmt;

    SET @_query_sql = p_sql;
    PREPARE _query_stmt FROM @_query_sql;
    EXECUTE _query_stmt;
    DEALLOCATE PREPARE _query_stmt;

    DROP TEMPORARY TABLE IF EXISTS GlobalCaseValuePivot;
END$$

DELIMITER ;

-- GetLookupRangeValue.mysql.sql
-- =============================================================================
-- GetLookupRangeValue
-- T-SQL: SELECT TOP 1 * -> MySQL: SELECT * ... LIMIT 1
-- T-SQL: RETURN NULL   -> MySQL: returns empty result set
-- =============================================================================

USE PayrollEngine;

DELIMITER $$

DROP PROCEDURE IF EXISTS GetLookupRangeValue$$
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
END$$

DELIMITER ;

-- GetNationalCaseChangeValues.mysql.sql
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

-- GetNationalCaseValues.mysql.sql
-- =============================================================================
-- GetNationalCaseValues
-- =============================================================================

USE PayrollEngine;

DELIMITER $$

DROP PROCEDURE IF EXISTS GetNationalCaseValues$$
CREATE PROCEDURE GetNationalCaseValues(
    IN p_parentId   INT,
    IN p_sql        LONGTEXT,
    IN p_attributes LONGTEXT
)
BEGIN
    DECLARE v_attrSql  LONGTEXT;
    DECLARE v_pivotSql LONGTEXT;

    DECLARE EXIT HANDLER FOR SQLEXCEPTION
    BEGIN
        DROP TEMPORARY TABLE IF EXISTS NationalCaseValuePivot;
        RESIGNAL;
    END;

    SET v_attrSql  = BuildAttributeQuery('NationalCaseValue.Attributes', p_attributes);
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

DELIMITER ;

-- GetPayrollResultValues.mysql.sql
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

-- GetWageTypeCustomResults.mysql.sql
-- =============================================================================
-- GetWageTypeCustomResults
-- =============================================================================

USE PayrollEngine;

DELIMITER $$

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
    DECLARE v_wageTypeNumber DECIMAL(28,6);
    DECLARE v_wageTypeCount  INT;

    SET v_wageTypeCount = IF(p_wageTypeNumbers IS NULL, 0, JSON_LENGTH(p_wageTypeNumbers));

    IF v_wageTypeCount = 1 THEN
        SELECT CAST(jt.val AS DECIMAL(28,6)) INTO v_wageTypeNumber
        FROM JSON_TABLE(p_wageTypeNumbers, '$[*]' COLUMNS (val VARCHAR(50) PATH '$')) AS jt
        LIMIT 1;
    END IF;

    SELECT wtcr.*
    FROM WageTypeCustomResult wtcr
    WHERE wtcr.TenantId = p_tenantId
      AND wtcr.EmployeeId = p_employeeId
      AND (p_divisionId IS NULL        OR wtcr.DivisionId = p_divisionId)
      AND (p_payrunJobId IS NULL       OR wtcr.PayrunJobId = p_payrunJobId)
      AND (p_parentPayrunJobId IS NULL OR wtcr.ParentJobId = p_parentPayrunJobId)
      AND (p_wageTypeNumbers IS NULL OR v_wageTypeCount = 0
           OR (v_wageTypeCount = 1 AND wtcr.WageTypeNumber = v_wageTypeNumber)
           OR (v_wageTypeCount > 1 AND wtcr.WageTypeNumber IN (
               SELECT CAST(jt.val AS DECIMAL(28,6))
               FROM JSON_TABLE(p_wageTypeNumbers, '$[*]' COLUMNS (val VARCHAR(50) PATH '$')) AS jt)))
      AND (p_periodStart IS NULL OR wtcr.Start BETWEEN p_periodStart AND p_periodEnd)
      AND (p_jobStatus IS NULL OR wtcr.PayrunJobId IN (
               SELECT pj.Id FROM PayrunJob pj
               WHERE pj.Id = wtcr.PayrunJobId
                 AND (pj.JobStatus & p_jobStatus) = pj.JobStatus))
      AND (wtcr.Forecast IS NULL OR wtcr.Forecast = p_forecast)
      AND (p_evaluationDate IS NULL OR wtcr.Created <= p_evaluationDate)
    ORDER BY wtcr.Created;
END$$

DELIMITER ;

-- GetWageTypeResults.mysql.sql
-- =============================================================================
-- GetWageTypeResults
-- OPENJSON(@wageTypeNumbers) -> JSON_TABLE + JSON_LENGTH
-- [JobStatus] & @jobStatus = [JobStatus] -> (pj.JobStatus & p_jobStatus) = pj.JobStatus
-- TOP (100) PERCENT ... ORDER BY -> ORDER BY (no TOP in MySQL)
-- =============================================================================

USE PayrollEngine;

DELIMITER $$

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
    DECLARE v_wageTypeNumber DECIMAL(28,6);
    DECLARE v_wageTypeCount  INT;

    SET v_wageTypeCount = IF(p_wageTypeNumbers IS NULL, 0, JSON_LENGTH(p_wageTypeNumbers));

    IF v_wageTypeCount = 1 THEN
        SELECT CAST(jt.val AS DECIMAL(28,6)) INTO v_wageTypeNumber
        FROM JSON_TABLE(p_wageTypeNumbers, '$[*]' COLUMNS (val VARCHAR(50) PATH '$')) AS jt
        LIMIT 1;
    END IF;

    SELECT wtr.*
    FROM WageTypeResult wtr
    WHERE wtr.TenantId = p_tenantId
      AND wtr.EmployeeId = p_employeeId
      AND (p_divisionId IS NULL        OR wtr.DivisionId = p_divisionId)
      AND (p_payrunJobId IS NULL       OR wtr.PayrunJobId = p_payrunJobId)
      AND (p_parentPayrunJobId IS NULL OR wtr.ParentJobId = p_parentPayrunJobId)
      AND (p_wageTypeNumbers IS NULL OR v_wageTypeCount = 0
           OR (v_wageTypeCount = 1 AND wtr.WageTypeNumber = v_wageTypeNumber)
           OR (v_wageTypeCount > 1 AND wtr.WageTypeNumber IN (
               SELECT CAST(jt.val AS DECIMAL(28,6))
               FROM JSON_TABLE(p_wageTypeNumbers, '$[*]' COLUMNS (val VARCHAR(50) PATH '$')) AS jt)))
      AND (p_periodStart IS NULL OR wtr.Start BETWEEN p_periodStart AND p_periodEnd)
      AND (p_jobStatus IS NULL OR wtr.PayrunJobId IN (
               SELECT pj.Id FROM PayrunJob pj
               WHERE pj.Id = wtr.PayrunJobId
                 AND (pj.JobStatus & p_jobStatus) = pj.JobStatus))
      AND (wtr.Forecast IS NULL OR wtr.Forecast = p_forecast)
      AND (p_evaluationDate IS NULL OR wtr.Created <= p_evaluationDate)
    ORDER BY wtr.Created;
END$$

DELIMITER ;

-- UpdateStatistics.mysql.sql
-- =============================================================================
-- UpdateStatistics
-- T-SQL: UPDATE STATISTICS ... WITH FULLSCAN -> MySQL: ANALYZE TABLE
-- =============================================================================

USE PayrollEngine;

DELIMITER $$

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

DELIMITER ;

-- UpdateStatisticsTargeted.mysql.sql
-- =============================================================================
-- UpdateStatisticsTargeted
-- T-SQL: UPDATE STATISTICS ... WITH FULLSCAN -> MySQL: ANALYZE TABLE
-- =============================================================================

USE PayrollEngine;

DELIMITER $$

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


-- =============================================================================

INSERT INTO `Version` (Created, MajorVersion, MinorVersion, SubVersion, Owner, Description)
VALUES (NOW(6), 0, 9, 7, CURRENT_USER(), 'Payroll Engine: Full setup v0.9.7 (MySQL)');

SELECT 'PayrollEngine MySQL schema v0.9.7 created successfully.' AS Result;
