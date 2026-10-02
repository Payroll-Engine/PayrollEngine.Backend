-- =============================================================================
-- Create-Model.pg.sql
-- PostgreSQL schema for PayrollEngine 14+ (16 LTS recommended).
-- Schema version: 1.1.0
-- NOTE: All table names are double-quoted for C# SqlKata compatibility.
-- =============================================================================

-- TABLES
-- =============================================================================

-- =============================================================================
-- NOTE: Indexes are defined after all tables in the INDEXES section below.

CREATE TABLE IF NOT EXISTS "Calendar" (
    "Id" INT           NOT NULL GENERATED ALWAYS AS IDENTITY,
    "Status" INT           NOT NULL,
    "Created" TIMESTAMP(6)   NOT NULL,
    "Updated" TIMESTAMP(6)   NOT NULL,
    "TenantId" INT           NOT NULL,
    "Name" VARCHAR(128)  NOT NULL,
    "NameLocalizations" TEXT      NULL,
    "CycleTimeUnit" INT           NOT NULL,
    "PeriodTimeUnit" INT           NOT NULL,
    "TimeMap" INT           NOT NULL,
    "FirstMonthOfYear" INT           NULL,
    "PeriodDayCount" DECIMAL(28,6) NULL,
    "YearWeekRule" INT           NULL,
    "FirstDayOfWeek" INT           NULL,
    "WeekMode" INT           NOT NULL,
    "WorkMonday" BOOLEAN    NULL,
    "WorkTuesday" BOOLEAN    NULL,
    "WorkWednesday" BOOLEAN    NULL,
    "WorkThursday" BOOLEAN    NULL,
    "WorkFriday" BOOLEAN    NULL,
    "WorkSaturday" BOOLEAN    NULL,
    "WorkSunday" BOOLEAN    NULL,
    "Attributes" TEXT      NULL,
    PRIMARY KEY ("Id")
);

CREATE TABLE IF NOT EXISTS "Case" (
    "Id" INT           NOT NULL GENERATED ALWAYS AS IDENTITY,
    "Status" INT           NOT NULL,
    "Created" TIMESTAMP(6)   NOT NULL,
    "Updated" TIMESTAMP(6)   NOT NULL,
    "RegulationId" INT           NOT NULL,
    "CaseType" INT           NOT NULL,
    "Name" VARCHAR(128)  NOT NULL,
    "NameLocalizations" TEXT      NULL,
    "NameSynonyms" TEXT      NULL,
    "Description" TEXT      NULL,
    "DescriptionLocalizations" TEXT      NULL,
    "DefaultReason" TEXT      NULL,
    "DefaultReasonLocalizations" TEXT      NULL,
    "BaseCase" VARCHAR(128)  NULL,
    "BaseCaseFields" TEXT      NULL,
    "OverrideType" INT           NOT NULL,
    "CancellationType" INT           NOT NULL,
    "Hidden" BOOLEAN    NOT NULL,
    "AvailableExpression" TEXT      NULL,
    "BuildExpression" TEXT      NULL,
    "ValidateExpression" TEXT      NULL,
    "Lookups" TEXT      NULL,
    "Slots" TEXT      NULL,
    "Script" TEXT      NULL,
    "ScriptVersion" VARCHAR(128)  NULL,
    "Binary" BYTEA      NULL,
    "ScriptHash" INT           NULL,
    "AvailableActions" TEXT      NULL,
    "BuildActions" TEXT      NULL,
    "ValidateActions" TEXT      NULL,
    "Attributes" TEXT      NULL,
    "Clusters" TEXT      NULL,
    PRIMARY KEY ("Id")
);

CREATE TABLE IF NOT EXISTS "CaseAudit" (
    "Id" INT           NOT NULL GENERATED ALWAYS AS IDENTITY,
    "Status" INT           NOT NULL,
    "Created" TIMESTAMP(6)   NOT NULL,
    "Updated" TIMESTAMP(6)   NOT NULL,
    "CaseId" INT           NOT NULL,
    "CaseChangeId" INT           NULL,
    "CaseType" INT           NOT NULL,
    "Name" VARCHAR(128)  NOT NULL,
    "NameLocalizations" TEXT      NULL,
    "NameSynonyms" TEXT      NULL,
    "Description" TEXT      NULL,
    "DescriptionLocalizations" TEXT      NULL,
    "DefaultReason" TEXT      NULL,
    "DefaultReasonLocalizations" TEXT      NULL,
    "BaseCase" VARCHAR(128)  NULL,
    "BaseCaseFields" TEXT      NULL,
    "OverrideType" INT           NOT NULL,
    "CancellationType" INT           NOT NULL,
    "Hidden" BOOLEAN    NOT NULL,
    "AvailableExpression" TEXT      NULL,
    "BuildExpression" TEXT      NULL,
    "ValidateExpression" TEXT      NULL,
    "Lookups" TEXT      NULL,
    "Slots" TEXT      NULL,
    "Script" TEXT      NULL,
    "ScriptVersion" VARCHAR(128)  NULL,
    "Binary" BYTEA      NULL,
    "ScriptHash" INT           NULL,
    "AvailableActions" TEXT      NULL,
    "BuildActions" TEXT      NULL,
    "ValidateActions" TEXT      NULL,
    "Attributes" TEXT      NULL,
    "Clusters" TEXT      NULL,
    PRIMARY KEY ("Id")
);

CREATE TABLE IF NOT EXISTS "CaseField" (
    "Id" INT           NOT NULL GENERATED ALWAYS AS IDENTITY,
    "Status" INT           NOT NULL,
    "Created" TIMESTAMP(6)   NOT NULL,
    "Updated" TIMESTAMP(6)   NOT NULL,
    "Name" VARCHAR(128)  NOT NULL,
    "NameLocalizations" TEXT      NULL,
    "Description" TEXT      NULL,
    "DescriptionLocalizations" TEXT      NULL,
    "CaseId" INT           NOT NULL,
    "ValueType" INT           NOT NULL,
    "ValueScope" INT           NOT NULL,
    "StartDateType" INT           NOT NULL,
    "EndDateType" INT           NOT NULL,
    "EndMandatory" BOOLEAN    NOT NULL,
    "DefaultStart" VARCHAR(128)  NULL,
    "DefaultEnd" VARCHAR(128)  NULL,
    "DefaultValue" TEXT      NULL,
    "LookupSettings" TEXT      NULL,
    "TimeType" INT           NOT NULL,
    "TimeUnit" INT           NOT NULL,
    "Culture" VARCHAR(128)  NULL,
    "PeriodAggregation" INT           NOT NULL,
    "OverrideType" INT           NOT NULL,
    "CancellationMode" INT           NOT NULL,
    "ValueCreationMode" INT           NOT NULL,
    "ValueMandatory" BOOLEAN    NOT NULL,
    "Order" INT           NOT NULL,
    "Tags" TEXT      NULL,
    "Clusters" TEXT      NULL,
    "Attributes" TEXT      NULL,
    "ValueAttributes" TEXT      NULL,
    PRIMARY KEY ("Id")
);

CREATE TABLE IF NOT EXISTS "CaseFieldAudit" (
    "Id" INT           NOT NULL GENERATED ALWAYS AS IDENTITY,
    "Status" INT           NOT NULL,
    "Created" TIMESTAMP(6)   NOT NULL,
    "Updated" TIMESTAMP(6)   NOT NULL,
    "CaseFieldId" INT           NOT NULL,
    "ValueType" INT           NOT NULL,
    "ValueScope" INT           NOT NULL,
    "Name" VARCHAR(128)  NOT NULL,
    "NameLocalizations" TEXT      NULL,
    "Description" TEXT      NULL,
    "DescriptionLocalizations" TEXT      NULL,
    "StartDateType" INT           NOT NULL,
    "EndDateType" INT           NOT NULL,
    "EndMandatory" BOOLEAN    NOT NULL,
    "DefaultStart" VARCHAR(128)  NULL,
    "DefaultEnd" VARCHAR(128)  NULL,
    "DefaultValue" TEXT      NULL,
    "LookupSettings" TEXT      NULL,
    "TimeType" INT           NOT NULL,
    "TimeUnit" INT           NOT NULL,
    "Culture" VARCHAR(128)  NULL,
    "PeriodAggregation" INT           NOT NULL,
    "OverrideType" INT           NOT NULL,
    "CancellationMode" INT           NOT NULL,
    "ValueCreationMode" INT           NOT NULL,
    "ValueMandatory" BOOLEAN    NOT NULL,
    "Order" INT           NOT NULL,
    "Tags" TEXT      NULL,
    "Clusters" TEXT      NULL,
    "Attributes" TEXT      NULL,
    "ValueAttributes" TEXT      NULL,
    PRIMARY KEY ("Id")
);

CREATE TABLE IF NOT EXISTS "CaseRelation" (
    "Id" INT           NOT NULL GENERATED ALWAYS AS IDENTITY,
    "Status" INT           NOT NULL,
    "Created" TIMESTAMP(6)   NOT NULL,
    "Updated" TIMESTAMP(6)   NOT NULL,
    "RegulationId" INT           NOT NULL,
    "SourceCaseName" VARCHAR(128)  NOT NULL,
    "SourceCaseNameLocalizations" TEXT      NULL,
    "SourceCaseSlot" VARCHAR(128)  NULL,
    "SourceCaseSlotLocalizations" TEXT      NULL,
    "TargetCaseName" VARCHAR(128)  NOT NULL,
    "TargetCaseNameLocalizations" TEXT      NULL,
    "TargetCaseSlot" VARCHAR(128)  NULL,
    "TargetCaseSlotLocalizations" TEXT      NULL,
    "RelationHash" INT           NOT NULL,
    "BuildExpression" TEXT      NULL,
    "ValidateExpression" TEXT      NULL,
    "OverrideType" INT           NOT NULL,
    "Order" INT           NOT NULL,
    "Script" TEXT      NULL,
    "ScriptVersion" VARCHAR(128)  NULL,
    "Binary" BYTEA      NULL,
    "ScriptHash" INT           NULL,
    "BuildActions" TEXT      NULL,
    "ValidateActions" TEXT      NULL,
    "Attributes" TEXT      NULL,
    "Clusters" TEXT      NULL,
    PRIMARY KEY ("Id")
);

CREATE TABLE IF NOT EXISTS "CaseRelationAudit" (
    "Id" INT           NOT NULL GENERATED ALWAYS AS IDENTITY,
    "Status" INT           NOT NULL,
    "Created" TIMESTAMP(6)   NOT NULL,
    "Updated" TIMESTAMP(6)   NOT NULL,
    "CaseRelationId" INT           NOT NULL,
    "SourceCaseName" VARCHAR(128)  NOT NULL,
    "SourceCaseNameLocalizations" TEXT      NULL,
    "SourceCaseSlot" VARCHAR(128)  NULL,
    "SourceCaseSlotLocalizations" TEXT      NULL,
    "TargetCaseName" VARCHAR(128)  NOT NULL,
    "TargetCaseNameLocalizations" TEXT      NULL,
    "TargetCaseSlot" VARCHAR(128)  NULL,
    "TargetCaseSlotLocalizations" TEXT      NULL,
    "RelationHash" INT           NOT NULL,
    "BuildExpression" TEXT      NULL,
    "ValidateExpression" TEXT      NULL,
    "OverrideType" INT           NOT NULL,
    "Order" INT           NOT NULL,
    "Script" TEXT      NULL,
    "ScriptVersion" VARCHAR(128)  NULL,
    "Binary" BYTEA      NULL,
    "ScriptHash" INT           NULL,
    "BuildActions" TEXT      NULL,
    "ValidateActions" TEXT      NULL,
    "Attributes" TEXT      NULL,
    "Clusters" TEXT      NULL,
    PRIMARY KEY ("Id")
);

CREATE TABLE IF NOT EXISTS "Collector" (
    "Id" INT           NOT NULL GENERATED ALWAYS AS IDENTITY,
    "Status" INT           NOT NULL,
    "Created" TIMESTAMP(6)   NOT NULL,
    "Updated" TIMESTAMP(6)   NOT NULL,
    "CollectMode" INT           NOT NULL,
    "Negated" BOOLEAN    NOT NULL,
    "RegulationId" INT           NOT NULL,
    "Name" VARCHAR(128)  NOT NULL,
    "NameLocalizations" TEXT      NULL,
    "OverrideType" INT           NOT NULL,
    "ValueType" INT           NOT NULL,
    "Culture" VARCHAR(128)  NULL,
    "CollectorGroups" TEXT      NULL,
    "StartExpression" TEXT      NULL,
    "ApplyExpression" TEXT      NULL,
    "EndExpression" TEXT      NULL,
    "StartActions" TEXT      NULL,
    "ApplyActions" TEXT      NULL,
    "EndActions" TEXT      NULL,
    "Threshold" DECIMAL(28,6) NULL,
    "MinResult" DECIMAL(28,6) NULL,
    "MaxResult" DECIMAL(28,6) NULL,
    "Script" TEXT      NULL,
    "ScriptVersion" VARCHAR(128)  NULL,
    "Binary" BYTEA      NULL,
    "ScriptHash" INT           NULL,
    "Attributes" TEXT      NULL,
    "Clusters" TEXT      NULL,
    PRIMARY KEY ("Id")
);

CREATE TABLE IF NOT EXISTS "CollectorAudit" (
    "Id" INT           NOT NULL GENERATED ALWAYS AS IDENTITY,
    "Status" INT           NOT NULL,
    "Created" TIMESTAMP(6)   NOT NULL,
    "Updated" TIMESTAMP(6)   NOT NULL,
    "CollectorId" INT           NOT NULL,
    "CollectMode" INT           NOT NULL,
    "Negated" BOOLEAN    NOT NULL,
    "Name" VARCHAR(128)  NOT NULL,
    "NameLocalizations" TEXT      NULL,
    "OverrideType" INT           NOT NULL,
    "ValueType" INT           NOT NULL,
    "Culture" VARCHAR(128)  NULL,
    "CollectorGroups" TEXT      NULL,
    "StartExpression" TEXT      NULL,
    "ApplyExpression" TEXT      NULL,
    "EndExpression" TEXT      NULL,
    "StartActions" TEXT      NULL,
    "ApplyActions" TEXT      NULL,
    "EndActions" TEXT      NULL,
    "Threshold" DECIMAL(28,6) NULL,
    "MinResult" DECIMAL(28,6) NULL,
    "MaxResult" DECIMAL(28,6) NULL,
    "Script" TEXT      NULL,
    "ScriptVersion" VARCHAR(128)  NULL,
    "Binary" BYTEA      NULL,
    "ScriptHash" INT           NULL,
    "Attributes" TEXT      NULL,
    "Clusters" TEXT      NULL,
    PRIMARY KEY ("Id")
);

CREATE TABLE IF NOT EXISTS "CollectorCustomResult" (
    "Id" INT           NOT NULL GENERATED ALWAYS AS IDENTITY,
    "Status" INT           NOT NULL,
    "Created" TIMESTAMP(6)   NOT NULL,
    "Updated" TIMESTAMP(6)   NOT NULL,
    "CollectorResultId" INT           NOT NULL,
    "TenantId" INT           NOT NULL,
    "EmployeeId" INT           NOT NULL,
    "DivisionId" INT           NULL,
    "CollectorName" VARCHAR(128)  NOT NULL,
    "CollectorNameHash" INT           NOT NULL,
    "CollectorNameLocalizations" TEXT      NULL,
    "Source" VARCHAR(128)  NOT NULL,
    "ValueType" INT           NOT NULL,
    "Value" DECIMAL(28,6) NOT NULL,
    "Culture" VARCHAR(128)  NOT NULL,
    "Start" TIMESTAMP(6)   NOT NULL,
    "StartHash" INT           NOT NULL,
    "End" TIMESTAMP(6)   NOT NULL,
    "PayrunJobId" INT           NOT NULL,
    "Forecast" VARCHAR(128)  NULL,
    "ParentJobId" INT           NULL,
    "Tags" TEXT      NULL,
    "Attributes" TEXT      NULL,
    PRIMARY KEY ("Id")
);

CREATE TABLE IF NOT EXISTS "CollectorResult" (
    "Id" INT           NOT NULL GENERATED ALWAYS AS IDENTITY,
    "Status" INT           NOT NULL,
    "Created" TIMESTAMP(6)   NOT NULL,
    "Updated" TIMESTAMP(6)   NOT NULL,
    "PayrollResultId" INT           NOT NULL,
    "TenantId" INT           NOT NULL,
    "EmployeeId" INT           NOT NULL,
    "DivisionId" INT           NULL,
    "CollectorId" INT           NOT NULL,
    "CollectorName" VARCHAR(128)  NOT NULL,
    "CollectorNameHash" INT           NOT NULL,
    "CollectorNameLocalizations" TEXT      NULL,
    "CollectMode" INT           NOT NULL,
    "Negated" BOOLEAN    NOT NULL,
    "ValueType" INT           NOT NULL,
    "Value" DECIMAL(28,6) NOT NULL,
    "Culture" VARCHAR(128)  NOT NULL,
    "Start" TIMESTAMP(6)   NOT NULL,
    "StartHash" INT           NOT NULL,
    "End" TIMESTAMP(6)   NOT NULL,
    "PayrunJobId" INT           NOT NULL,
    "Forecast" VARCHAR(128)  NULL,
    "ParentJobId" INT           NULL,
    "Tags" TEXT      NULL,
    "Attributes" TEXT      NULL,
    PRIMARY KEY ("Id")
);

CREATE TABLE IF NOT EXISTS "CompanyCaseChange" (
    "Id" INT           NOT NULL GENERATED ALWAYS AS IDENTITY,
    "Status" INT           NOT NULL,
    "Created" TIMESTAMP(6)   NOT NULL,
    "Updated" TIMESTAMP(6)   NOT NULL,
    "TenantId" INT           NOT NULL,
    "UserId" INT           NOT NULL,
    "DivisionId" INT           NULL,
    "CancellationType" INT           NOT NULL,
    "CancellationId" INT           NULL,
    "CancellationDate" TIMESTAMP(6)   NULL,
    "Reason" TEXT      NOT NULL,
    "ValidationCaseName" VARCHAR(128)  NULL,
    "Forecast" VARCHAR(128)  NULL,
    PRIMARY KEY ("Id")
);

CREATE TABLE IF NOT EXISTS "CompanyCaseDocument" (
    "Id" INT          NOT NULL GENERATED ALWAYS AS IDENTITY,
    "Status" INT          NOT NULL,
    "Created" TIMESTAMP(6)  NOT NULL,
    "Updated" TIMESTAMP(6)  NOT NULL,
    "CaseValueId" INT          NOT NULL,
    "Name" VARCHAR(256) NOT NULL,
    "Content" TEXT     NOT NULL,
    "ContentType" VARCHAR(128) NOT NULL,
    PRIMARY KEY ("Id")
);

CREATE TABLE IF NOT EXISTS "CompanyCaseValue" (
    "Id" INT           NOT NULL GENERATED ALWAYS AS IDENTITY,
    "Status" INT           NOT NULL,
    "Created" TIMESTAMP(6)   NOT NULL,
    "Updated" TIMESTAMP(6)   NOT NULL,
    "TenantId" INT           NOT NULL,
    "DivisionId" INT           NULL,
    "CaseName" VARCHAR(128)  NOT NULL,
    "CaseNameLocalizations" TEXT      NULL,
    "CaseFieldName" VARCHAR(128)  NOT NULL,
    "CaseFieldNameLocalizations" TEXT      NULL,
    "CaseSlot" VARCHAR(128)  NULL,
    "CaseSlotLocalizations" TEXT      NULL,
    "ValueType" INT           NOT NULL,
    "Value" TEXT      NOT NULL,
    "NumericValue" DECIMAL(28,6) NULL,
    "Culture" VARCHAR(128)  NOT NULL,
    "CaseRelation" TEXT      NULL,
    "CancellationDate" TIMESTAMP(6)   NULL,
    "Start" TIMESTAMP(6)   NULL,
    "End" TIMESTAMP(6)   NULL,
    "Forecast" VARCHAR(128)  NULL,
    "Tags" TEXT      NULL,
    "Attributes" TEXT      NULL,
    PRIMARY KEY ("Id")
);

CREATE TABLE IF NOT EXISTS "CompanyCaseValueChange" (
    "Id" INT         NOT NULL GENERATED ALWAYS AS IDENTITY,
    "Status" INT         NOT NULL,
    "Created" TIMESTAMP(6) NOT NULL,
    "Updated" TIMESTAMP(6) NOT NULL,
    "CaseChangeId" INT         NOT NULL,
    "CaseValueId" INT         NOT NULL,
    PRIMARY KEY ("Id")
);

CREATE TABLE IF NOT EXISTS "Division" (
    "Id" INT          NOT NULL GENERATED ALWAYS AS IDENTITY,
    "Status" INT          NOT NULL,
    "Created" TIMESTAMP(6)  NOT NULL,
    "Updated" TIMESTAMP(6)  NOT NULL,
    "TenantId" INT          NOT NULL,
    "Name" VARCHAR(128) NOT NULL,
    "NameLocalizations" TEXT     NULL,
    "Culture" VARCHAR(128) NULL,
    "Calendar" VARCHAR(128) NULL,
    "Attributes" TEXT     NULL,
    PRIMARY KEY ("Id")
);

CREATE TABLE IF NOT EXISTS "Employee" (
    "Id" INT          NOT NULL GENERATED ALWAYS AS IDENTITY,
    "Status" INT          NOT NULL,
    "Created" TIMESTAMP(6)  NOT NULL,
    "Updated" TIMESTAMP(6)  NOT NULL,
    "TenantId" INT          NOT NULL,
    "Identifier" VARCHAR(128) NOT NULL,
    "FirstName" VARCHAR(128) NOT NULL,
    "LastName" VARCHAR(128) NOT NULL,
    "Culture" VARCHAR(128) NULL,
    "Calendar" VARCHAR(128) NULL,
    "Attributes" TEXT     NULL,
    PRIMARY KEY ("Id")
);

CREATE TABLE IF NOT EXISTS "EmployeeCaseChange" (
    "Id" INT          NOT NULL GENERATED ALWAYS AS IDENTITY,
    "Status" INT          NOT NULL,
    "Created" TIMESTAMP(6)  NOT NULL,
    "Updated" TIMESTAMP(6)  NOT NULL,
    "EmployeeId" INT          NOT NULL,
    "UserId" INT          NOT NULL,
    "DivisionId" INT          NULL,
    "CancellationType" INT          NOT NULL,
    "CancellationId" INT          NULL,
    "CancellationDate" TIMESTAMP(6)  NULL,
    "Reason" TEXT     NOT NULL,
    "ValidationCaseName" VARCHAR(128) NULL,
    "Forecast" VARCHAR(128) NULL,
    PRIMARY KEY ("Id")
);

CREATE TABLE IF NOT EXISTS "EmployeeCaseDocument" (
    "Id" INT          NOT NULL GENERATED ALWAYS AS IDENTITY,
    "Status" INT          NOT NULL,
    "Created" TIMESTAMP(6)  NOT NULL,
    "Updated" TIMESTAMP(6)  NOT NULL,
    "CaseValueId" INT          NOT NULL,
    "Name" VARCHAR(256) NOT NULL,
    "Content" TEXT     NOT NULL,
    "ContentType" VARCHAR(128) NOT NULL,
    PRIMARY KEY ("Id")
);

CREATE TABLE IF NOT EXISTS "EmployeeCaseValue" (
    "Id" INT           NOT NULL GENERATED ALWAYS AS IDENTITY,
    "Status" INT           NOT NULL,
    "Created" TIMESTAMP(6)   NOT NULL,
    "Updated" TIMESTAMP(6)   NOT NULL,
    "EmployeeId" INT           NOT NULL,
    "DivisionId" INT           NULL,
    "CaseName" VARCHAR(128)  NOT NULL,
    "CaseNameLocalizations" TEXT      NULL,
    "CaseFieldName" VARCHAR(128)  NOT NULL,
    "CaseFieldNameLocalizations" TEXT      NULL,
    "CaseSlot" VARCHAR(128)  NULL,
    "CaseSlotLocalizations" TEXT      NULL,
    "ValueType" INT           NOT NULL,
    "Value" TEXT      NOT NULL,
    "NumericValue" DECIMAL(28,6) NULL,
    "Culture" VARCHAR(128)  NOT NULL,
    "CaseRelation" TEXT      NULL,
    "CancellationDate" TIMESTAMP(6)   NULL,
    "Start" TIMESTAMP(6)   NULL,
    "End" TIMESTAMP(6)   NULL,
    "Forecast" VARCHAR(128)  NULL,
    "Tags" TEXT      NULL,
    "Attributes" TEXT      NULL,
    PRIMARY KEY ("Id")
);

CREATE TABLE IF NOT EXISTS "EmployeeCaseValueChange" (
    "Id" INT         NOT NULL GENERATED ALWAYS AS IDENTITY,
    "Status" INT         NOT NULL,
    "Created" TIMESTAMP(6) NOT NULL,
    "Updated" TIMESTAMP(6) NOT NULL,
    "CaseChangeId" INT         NOT NULL,
    "CaseValueId" INT         NOT NULL,
    PRIMARY KEY ("Id")
);

CREATE TABLE IF NOT EXISTS "EmployeeDivision" (
    "Id" INT         NOT NULL GENERATED ALWAYS AS IDENTITY,
    "Status" INT         NOT NULL,
    "Created" TIMESTAMP(6) NOT NULL,
    "Updated" TIMESTAMP(6) NOT NULL,
    "EmployeeId" INT         NOT NULL,
    "DivisionId" INT         NOT NULL,
    PRIMARY KEY ("Id")
);

CREATE TABLE IF NOT EXISTS "GlobalCaseChange" (
    "Id" INT          NOT NULL GENERATED ALWAYS AS IDENTITY,
    "Status" INT          NOT NULL,
    "Created" TIMESTAMP(6)  NOT NULL,
    "Updated" TIMESTAMP(6)  NOT NULL,
    "TenantId" INT          NOT NULL,
    "UserId" INT          NOT NULL,
    "DivisionId" INT          NULL,
    "CancellationType" INT          NOT NULL,
    "CancellationId" INT          NULL,
    "CancellationDate" TIMESTAMP(6)  NULL,
    "Reason" TEXT     NOT NULL,
    "ValidationCaseName" VARCHAR(128) NULL,
    "Forecast" VARCHAR(128) NULL,
    PRIMARY KEY ("Id")
);

CREATE TABLE IF NOT EXISTS "GlobalCaseDocument" (
    "Id" INT          NOT NULL GENERATED ALWAYS AS IDENTITY,
    "Status" INT          NOT NULL,
    "Created" TIMESTAMP(6)  NOT NULL,
    "Updated" TIMESTAMP(6)  NOT NULL,
    "CaseValueId" INT          NOT NULL,
    "Name" VARCHAR(256) NOT NULL,
    "Content" TEXT     NOT NULL,
    "ContentType" VARCHAR(128) NOT NULL,
    PRIMARY KEY ("Id")
);

CREATE TABLE IF NOT EXISTS "GlobalCaseValue" (
    "Id" INT           NOT NULL GENERATED ALWAYS AS IDENTITY,
    "Status" INT           NOT NULL,
    "Created" TIMESTAMP(6)   NOT NULL,
    "Updated" TIMESTAMP(6)   NOT NULL,
    "TenantId" INT           NOT NULL,
    "DivisionId" INT           NULL,
    "CaseName" VARCHAR(128)  NOT NULL,
    "CaseNameLocalizations" TEXT      NULL,
    "CaseFieldName" VARCHAR(128)  NOT NULL,
    "CaseFieldNameLocalizations" TEXT      NULL,
    "CaseSlot" VARCHAR(128)  NULL,
    "CaseSlotLocalizations" TEXT      NULL,
    "ValueType" INT           NOT NULL,
    "Value" TEXT      NOT NULL,
    "NumericValue" DECIMAL(28,6) NULL,
    "Culture" VARCHAR(128)  NOT NULL,
    "CaseRelation" TEXT      NULL,
    "CancellationDate" TIMESTAMP(6)   NULL,
    "Start" TIMESTAMP(6)   NULL,
    "End" TIMESTAMP(6)   NULL,
    "Forecast" VARCHAR(128)  NULL,
    "Tags" TEXT      NULL,
    "Attributes" TEXT      NULL,
    PRIMARY KEY ("Id")
);

CREATE TABLE IF NOT EXISTS "GlobalCaseValueChange" (
    "Id" INT         NOT NULL GENERATED ALWAYS AS IDENTITY,
    "Status" INT         NOT NULL,
    "Created" TIMESTAMP(6) NOT NULL,
    "Updated" TIMESTAMP(6) NOT NULL,
    "CaseChangeId" INT         NOT NULL,
    "CaseValueId" INT         NOT NULL,
    PRIMARY KEY ("Id")
);

CREATE TABLE IF NOT EXISTS "Log" (
    "Id" INT          NOT NULL GENERATED ALWAYS AS IDENTITY,
    "Status" INT          NOT NULL,
    "Created" TIMESTAMP(6)  NOT NULL,
    "Updated" TIMESTAMP(6)  NOT NULL,
    "TenantId" INT          NOT NULL,
    "Level" INT          NOT NULL,
    "Message" TEXT     NOT NULL,
    "User" VARCHAR(128) NOT NULL,
    "Error" TEXT     NULL,
    "Comment" TEXT     NULL,
    "Owner" VARCHAR(128) NULL,
    "OwnerType" VARCHAR(128) NULL,
    PRIMARY KEY ("Id")
);

CREATE TABLE IF NOT EXISTS "Lookup" (
    "Id" INT           NOT NULL GENERATED ALWAYS AS IDENTITY,
    "Status" INT           NOT NULL,
    "Created" TIMESTAMP(6)   NOT NULL,
    "Updated" TIMESTAMP(6)   NOT NULL,
    "RegulationId" INT           NOT NULL,
    "Name" VARCHAR(128)  NOT NULL,
    "NameLocalizations" TEXT      NULL,
    "Description" TEXT      NULL,
    "DescriptionLocalizations" TEXT      NULL,
    "OverrideType" INT           NOT NULL,
    "RangeSize" DECIMAL(28,6) NULL,
    "Attributes" TEXT      NULL,
    "RangeMode" INT           NOT NULL,
    PRIMARY KEY ("Id")
);

CREATE TABLE IF NOT EXISTS "LookupAudit" (
    "Id" INT           NOT NULL GENERATED ALWAYS AS IDENTITY,
    "Status" INT           NOT NULL,
    "Created" TIMESTAMP(6)   NOT NULL,
    "Updated" TIMESTAMP(6)   NOT NULL,
    "LookupId" INT           NOT NULL,
    "Name" VARCHAR(128)  NOT NULL,
    "NameLocalizations" TEXT      NULL,
    "Description" TEXT      NULL,
    "DescriptionLocalizations" TEXT      NULL,
    "OverrideType" INT           NOT NULL,
    "RangeSize" DECIMAL(28,6) NULL,
    "Attributes" TEXT      NULL,
    "RangeMode" INT           NOT NULL,
    PRIMARY KEY ("Id")
);

CREATE TABLE IF NOT EXISTS "LookupValue" (
    "Id" INT           NOT NULL GENERATED ALWAYS AS IDENTITY,
    "Status" INT           NOT NULL,
    "Created" TIMESTAMP(6)   NOT NULL,
    "Updated" TIMESTAMP(6)   NOT NULL,
    "LookupId" INT           NOT NULL,
    "Key" TEXT      NOT NULL,
    "KeyHash" INT           NOT NULL,
    "RangeValue" DECIMAL(28,6) NULL,
    "Value" TEXT      NOT NULL,
    "ValueLocalizations" TEXT      NULL,
    "OverrideType" INT           NOT NULL,
    "LookupHash" INT           NOT NULL,
    PRIMARY KEY ("Id")
);

CREATE TABLE IF NOT EXISTS "LookupValueAudit" (
    "Id" INT           NOT NULL GENERATED ALWAYS AS IDENTITY,
    "Status" INT           NOT NULL,
    "Created" TIMESTAMP(6)   NOT NULL,
    "Updated" TIMESTAMP(6)   NOT NULL,
    "LookupValueId" INT           NOT NULL,
    "Key" TEXT      NOT NULL,
    "KeyHash" INT           NOT NULL,
    "RangeValue" DECIMAL(28,6) NULL,
    "Value" TEXT      NOT NULL,
    "ValueLocalizations" TEXT      NULL,
    "OverrideType" INT           NOT NULL,
    "LookupHash" INT           NOT NULL,
    PRIMARY KEY ("Id")
);

CREATE TABLE IF NOT EXISTS "NationalCaseChange" (
    "Id" INT          NOT NULL GENERATED ALWAYS AS IDENTITY,
    "Status" INT          NOT NULL,
    "Created" TIMESTAMP(6)  NOT NULL,
    "Updated" TIMESTAMP(6)  NOT NULL,
    "TenantId" INT          NOT NULL,
    "UserId" INT          NOT NULL,
    "DivisionId" INT          NULL,
    "CancellationType" INT          NOT NULL,
    "CancellationId" INT          NULL,
    "CancellationDate" TIMESTAMP(6)  NULL,
    "Reason" TEXT     NOT NULL,
    "ValidationCaseName" VARCHAR(128) NULL,
    "Forecast" VARCHAR(128) NULL,
    PRIMARY KEY ("Id")
);

CREATE TABLE IF NOT EXISTS "NationalCaseDocument" (
    "Id" INT          NOT NULL GENERATED ALWAYS AS IDENTITY,
    "Status" INT          NOT NULL,
    "Created" TIMESTAMP(6)  NOT NULL,
    "Updated" TIMESTAMP(6)  NOT NULL,
    "CaseValueId" INT          NOT NULL,
    "Name" VARCHAR(256) NOT NULL,
    "Content" TEXT     NOT NULL,
    "ContentType" VARCHAR(128) NOT NULL,
    PRIMARY KEY ("Id")
);

CREATE TABLE IF NOT EXISTS "NationalCaseValue" (
    "Id" INT           NOT NULL GENERATED ALWAYS AS IDENTITY,
    "Status" INT           NOT NULL,
    "Created" TIMESTAMP(6)   NOT NULL,
    "Updated" TIMESTAMP(6)   NOT NULL,
    "TenantId" INT           NOT NULL,
    "DivisionId" INT           NULL,
    "CaseName" VARCHAR(128)  NOT NULL,
    "CaseNameLocalizations" TEXT      NULL,
    "CaseFieldName" VARCHAR(128)  NOT NULL,
    "CaseFieldNameLocalizations" TEXT      NULL,
    "CaseSlot" VARCHAR(128)  NULL,
    "CaseSlotLocalizations" TEXT      NULL,
    "ValueType" INT           NOT NULL,
    "Value" TEXT      NOT NULL,
    "NumericValue" DECIMAL(28,6) NULL,
    "Culture" VARCHAR(128)  NOT NULL,
    "CaseRelation" TEXT      NULL,
    "CancellationDate" TIMESTAMP(6)   NULL,
    "Start" TIMESTAMP(6)   NULL,
    "End" TIMESTAMP(6)   NULL,
    "Forecast" VARCHAR(128)  NULL,
    "Tags" TEXT      NULL,
    "Attributes" TEXT      NULL,
    PRIMARY KEY ("Id")
);

CREATE TABLE IF NOT EXISTS "NationalCaseValueChange" (
    "Id" INT         NOT NULL GENERATED ALWAYS AS IDENTITY,
    "Status" INT         NOT NULL,
    "Created" TIMESTAMP(6) NOT NULL,
    "Updated" TIMESTAMP(6) NOT NULL,
    "CaseChangeId" INT         NOT NULL,
    "CaseValueId" INT         NOT NULL,
    PRIMARY KEY ("Id")
);

CREATE TABLE IF NOT EXISTS "Payroll" (
    "Id" INT          NOT NULL GENERATED ALWAYS AS IDENTITY,
    "Status" INT          NOT NULL,
    "Created" TIMESTAMP(6)  NOT NULL,
    "Updated" TIMESTAMP(6)  NOT NULL,
    "TenantId" INT          NOT NULL,
    "DivisionId" INT          NOT NULL,
    "Name" VARCHAR(128) NOT NULL,
    "NameLocalizations" TEXT     NULL,
    "Description" TEXT     NULL,
    "DescriptionLocalizations" TEXT     NULL,
    "ClusterSet" TEXT     NULL,
    "ClusterSets" TEXT     NULL,
    "Attributes" TEXT     NULL,
    PRIMARY KEY ("Id")
);

CREATE TABLE IF NOT EXISTS "PayrollLayer" (
    "Id" INT          NOT NULL GENERATED ALWAYS AS IDENTITY,
    "Status" INT          NOT NULL,
    "Created" TIMESTAMP(6)  NOT NULL,
    "Updated" TIMESTAMP(6)  NOT NULL,
    "PayrollId" INT          NOT NULL,
    "RegulationName" VARCHAR(128) NOT NULL,
    "Level" INT          NOT NULL,
    "Priority" INT          NOT NULL,
    "Attributes" TEXT     NULL,
    PRIMARY KEY ("Id")
);

CREATE TABLE IF NOT EXISTS "PayrollResult" (
    "Id" INT          NOT NULL GENERATED ALWAYS AS IDENTITY,
    "Status" INT          NOT NULL,
    "Created" TIMESTAMP(6)  NOT NULL,
    "Updated" TIMESTAMP(6)  NOT NULL,
    "TenantId" INT          NOT NULL,
    "PayrollId" INT          NOT NULL,
    "PayrollName" VARCHAR(128) NULL,
    "PayrunId" INT          NOT NULL,
    "PayrunName" VARCHAR(128) NULL,
    "PayrunJobId" INT          NOT NULL,
    "PayrunJobName" VARCHAR(128) NULL,
    "EmployeeId" INT          NOT NULL,
    "EmployeeIdentifier" VARCHAR(128) NULL,
    "DivisionId" INT          NOT NULL,
    "DivisionName" VARCHAR(128) NULL,
    "CycleName" VARCHAR(128) NOT NULL,
    "CycleStart" TIMESTAMP(6)  NOT NULL,
    "CycleEnd" TIMESTAMP(6)  NOT NULL,
    "PeriodName" VARCHAR(128) NOT NULL,
    "PeriodStart" TIMESTAMP(6)  NOT NULL,
    "PeriodEnd" TIMESTAMP(6)  NOT NULL,
    PRIMARY KEY ("Id")
);

CREATE TABLE IF NOT EXISTS "Payrun" (
    "Id" INT          NOT NULL GENERATED ALWAYS AS IDENTITY,
    "Status" INT          NOT NULL,
    "Created" TIMESTAMP(6)  NOT NULL,
    "Updated" TIMESTAMP(6)  NOT NULL,
    "TenantId" INT          NOT NULL,
    "PayrollId" INT          NOT NULL,
    "Name" VARCHAR(128) NOT NULL,
    "NameLocalizations" TEXT     NULL,
    "DefaultReason" TEXT     NULL,
    "DefaultReasonLocalizations" TEXT     NULL,
    "StartExpression" TEXT     NULL,
    "EmployeeAvailableExpression" TEXT     NULL,
    "EmployeeStartExpression" TEXT     NULL,
    "EmployeeEndExpression" TEXT     NULL,
    "WageTypeAvailableExpression" TEXT     NULL,
    "EndExpression" TEXT     NULL,
    "RetroBackCycles" INT          NOT NULL,
    "Script" TEXT     NULL,
    "ScriptVersion" VARCHAR(128) NULL,
    "Binary" BYTEA     NULL,
    "ScriptHash" INT          NULL,
    PRIMARY KEY ("Id")
);

CREATE TABLE IF NOT EXISTS "PayrunJob" (
    "Id" INT          NOT NULL GENERATED ALWAYS AS IDENTITY,
    "Status" INT          NOT NULL,
    "Created" TIMESTAMP(6)  NOT NULL,
    "Updated" TIMESTAMP(6)  NOT NULL,
    "TenantId" INT          NOT NULL,
    "PayrunId" INT          NOT NULL,
    "PayrollId" INT          NOT NULL,
    "DivisionId" INT          NOT NULL,
    "ParentJobId" INT          NULL,
    "CreatedUserId" INT          NOT NULL,
    "ReleasedUserId" INT          NULL,
    "ProcessedUserId" INT          NULL,
    "FinishedUserId" INT          NULL,
    "RetroPayMode" INT          NOT NULL,
    "JobStatus" INT          NOT NULL,
    "JobResult" INT          NOT NULL,
    "Name" VARCHAR(128) NOT NULL,
    "Owner" VARCHAR(128) NULL,
    "Forecast" VARCHAR(128) NULL,
    "CycleName" VARCHAR(128) NOT NULL,
    "CycleStart" TIMESTAMP(6)  NOT NULL,
    "CycleEnd" TIMESTAMP(6)  NOT NULL,
    "PeriodName" VARCHAR(128) NOT NULL,
    "PeriodStart" TIMESTAMP(6)  NOT NULL,
    "PeriodEnd" TIMESTAMP(6)  NOT NULL,
    "EvaluationDate" TIMESTAMP(6)  NOT NULL,
    "Released" TIMESTAMP(6)  NULL,
    "Processed" TIMESTAMP(6)  NULL,
    "Finished" TIMESTAMP(6)  NULL,
    "CreatedReason" TEXT     NOT NULL,
    "ReleasedReason" TEXT     NULL,
    "ProcessedReason" TEXT     NULL,
    "FinishedReason" TEXT     NULL,
    "TotalEmployeeCount" INT          NOT NULL,
    "ProcessedEmployeeCount" INT          NOT NULL,
    "JobStart" TIMESTAMP(6)  NOT NULL,
    "JobEnd" TIMESTAMP(6)  NULL,
    "Message" TEXT     NULL,
    "ErrorMessage" TEXT     NULL,
    "Tags" TEXT     NULL,
    "Attributes" TEXT     NULL,
    PRIMARY KEY ("Id")
);

CREATE TABLE IF NOT EXISTS "PayrunJobEmployee" (
    "Id" INT         NOT NULL GENERATED ALWAYS AS IDENTITY,
    "Status" INT         NOT NULL,
    "Created" TIMESTAMP(6) NOT NULL,
    "Updated" TIMESTAMP(6) NOT NULL,
    "PayrunJobId" INT         NOT NULL,
    "EmployeeId" INT         NOT NULL,
    PRIMARY KEY ("Id")
);

CREATE TABLE IF NOT EXISTS "PayrunParameter" (
    "Id" INT          NOT NULL GENERATED ALWAYS AS IDENTITY,
    "Status" INT          NOT NULL,
    "Created" TIMESTAMP(6)  NOT NULL,
    "Updated" TIMESTAMP(6)  NOT NULL,
    "PayrunId" INT          NOT NULL,
    "Name" VARCHAR(128) NOT NULL,
    "NameLocalizations" TEXT     NULL,
    "Description" TEXT     NULL,
    "DescriptionLocalizations" TEXT     NULL,
    "Mandatory" BOOLEAN   NOT NULL,
    "Value" TEXT     NULL,
    "ValueType" INT          NOT NULL,
    "Attributes" TEXT     NULL,
    PRIMARY KEY ("Id")
);

CREATE TABLE IF NOT EXISTS "PayrunResult" (
    "Id" INT           NOT NULL GENERATED ALWAYS AS IDENTITY,
    "Status" INT           NOT NULL,
    "Created" TIMESTAMP(6)   NOT NULL,
    "Updated" TIMESTAMP(6)   NOT NULL,
    "PayrollResultId" INT           NOT NULL,
    "TenantId" INT           NOT NULL,
    "EmployeeId" INT           NOT NULL,
    "DivisionId" INT           NULL,
    "Source" VARCHAR(128)  NOT NULL,
    "Name" VARCHAR(128)  NOT NULL,
    "NameLocalizations" TEXT      NULL,
    "Slot" VARCHAR(128)  NULL,
    "ValueType" INT           NOT NULL,
    "Value" TEXT      NULL,
    "NumericValue" DECIMAL(28,6) NULL,
    "Culture" VARCHAR(128)  NOT NULL,
    "Start" TIMESTAMP(6)   NULL,
    "StartHash" INT           NOT NULL,
    "End" TIMESTAMP(6)   NULL,
    "PayrunJobId" INT           NOT NULL,
    "Forecast" VARCHAR(128)  NULL,
    "ParentJobId" INT           NULL,
    "Tags" TEXT      NULL,
    "Attributes" TEXT      NULL,
    PRIMARY KEY ("Id")
);

CREATE TABLE IF NOT EXISTS "PayrunTrace" (
    "Id" INT          NOT NULL GENERATED ALWAYS AS IDENTITY,
    "Status" INT          NOT NULL,
    "Created" TIMESTAMP(6)  NOT NULL,
    "Updated" TIMESTAMP(6)  NOT NULL,
    "PayrollResultId" INT          NOT NULL,
    "TenantId" INT          NOT NULL,
    "EmployeeId" INT          NOT NULL,
    "DivisionId" INT          NULL,
    "Level" INT          NOT NULL,
    "Text" TEXT     NOT NULL,
    "PayrunJobId" INT          NOT NULL,
    "Forecast" VARCHAR(128) NULL,
    "ParentJobId" INT          NULL,
    PRIMARY KEY ("Id")
);

CREATE TABLE IF NOT EXISTS "Regulation" (
    "Id" INT          NOT NULL GENERATED ALWAYS AS IDENTITY,
    "Status" INT          NOT NULL,
    "Created" TIMESTAMP(6)  NOT NULL,
    "Updated" TIMESTAMP(6)  NOT NULL,
    "TenantId" INT          NOT NULL,
    "Name" VARCHAR(128) NOT NULL,
    "NameLocalizations" TEXT     NULL,
    "Namespace" VARCHAR(128) NULL,
    "Version" INT          NOT NULL,
    "SharedRegulation" BOOLEAN   NOT NULL,
    "ValidFrom" TIMESTAMP(6)  NULL,
    "Owner" VARCHAR(128) NULL,
    "Description" TEXT     NULL,
    "DescriptionLocalizations" TEXT     NULL,
    "BaseRegulations" TEXT     NULL,
    "Attributes" TEXT     NULL,
    PRIMARY KEY ("Id")
);

CREATE TABLE IF NOT EXISTS "RegulationShare" (
    "Id" INT         NOT NULL GENERATED ALWAYS AS IDENTITY,
    "Status" INT         NOT NULL,
    "Created" TIMESTAMP(6) NOT NULL,
    "Updated" TIMESTAMP(6) NOT NULL,
    "ProviderTenantId" INT         NOT NULL,
    "ProviderRegulationId" INT         NOT NULL,
    "ConsumerTenantId" INT         NOT NULL,
    "ConsumerDivisionId" INT         NULL,
    "IsolationLevel" INT         NOT NULL DEFAULT 3,
    "Attributes" TEXT    NULL,
    PRIMARY KEY ("Id"),
    CONSTRAINT CK_RegulationShare_IsolationLevel CHECK ("IsolationLevel" IN (0, 1, 2, 3))
);

CREATE TABLE IF NOT EXISTS "Report" (
    "Id" INT          NOT NULL GENERATED ALWAYS AS IDENTITY,
    "Status" INT          NOT NULL,
    "Created" TIMESTAMP(6)  NOT NULL,
    "Updated" TIMESTAMP(6)  NOT NULL,
    "RegulationId" INT          NOT NULL,
    "Name" VARCHAR(128) NOT NULL,
    "NameLocalizations" TEXT     NULL,
    "Description" TEXT     NULL,
    "DescriptionLocalizations" TEXT     NULL,
    "Category" VARCHAR(128) NULL,
    "Queries" TEXT     NULL,
    "Relations" TEXT     NULL,
    "OverrideType" INT          NOT NULL,
    "AttributeMode" INT          NOT NULL,
    "UserType" INT          NOT NULL,
    "ReportIsolation" INT          NOT NULL,
    "BuildExpression" TEXT     NULL,
    "StartExpression" TEXT     NULL,
    "EndExpression" TEXT     NULL,
    "Script" TEXT     NULL,
    "ScriptVersion" VARCHAR(128) NULL,
    "Binary" BYTEA     NULL,
    "ScriptHash" INT          NOT NULL,
    "Attributes" TEXT     NULL,
    "Clusters" TEXT     NULL,
    PRIMARY KEY ("Id")
);

CREATE TABLE IF NOT EXISTS "ReportAudit" (
    "Id" INT          NOT NULL GENERATED ALWAYS AS IDENTITY,
    "ReportId" INT          NOT NULL,
    "Status" INT          NOT NULL,
    "Created" TIMESTAMP(6)  NOT NULL,
    "Updated" TIMESTAMP(6)  NOT NULL,
    "Name" VARCHAR(128) NOT NULL,
    "NameLocalizations" TEXT     NULL,
    "Description" TEXT     NULL,
    "DescriptionLocalizations" TEXT     NULL,
    "Category" VARCHAR(128) NULL,
    "Queries" TEXT     NULL,
    "Relations" TEXT     NULL,
    "AttributeMode" INT          NOT NULL,
    "OverrideType" INT          NOT NULL,
    "UserType" INT          NOT NULL,
    "ReportIsolation" INT          NOT NULL,
    "BuildExpression" TEXT     NULL,
    "StartExpression" TEXT     NULL,
    "EndExpression" TEXT     NULL,
    "Script" TEXT     NULL,
    "ScriptVersion" VARCHAR(128) NULL,
    "Binary" BYTEA     NULL,
    "ScriptHash" INT          NOT NULL,
    "Attributes" TEXT     NULL,
    "Clusters" TEXT     NULL,
    PRIMARY KEY ("Id")
);

CREATE TABLE IF NOT EXISTS "ReportLog" (
    "Id" INT          NOT NULL GENERATED ALWAYS AS IDENTITY,
    "Status" INT          NOT NULL,
    "Created" TIMESTAMP(6)  NOT NULL,
    "Updated" TIMESTAMP(6)  NOT NULL,
    "TenantId" INT          NOT NULL,
    "ReportName" VARCHAR(128) NOT NULL,
    "ReportDate" TIMESTAMP(6)  NOT NULL,
    "Message" TEXT     NULL,
    "Key" VARCHAR(128) NULL,
    "User" VARCHAR(128) NOT NULL,
    PRIMARY KEY ("Id")
);

CREATE TABLE IF NOT EXISTS "ReportParameter" (
    "Id" INT          NOT NULL GENERATED ALWAYS AS IDENTITY,
    "Status" INT          NOT NULL,
    "Created" TIMESTAMP(6)  NOT NULL,
    "Updated" TIMESTAMP(6)  NOT NULL,
    "ReportId" INT          NOT NULL,
    "Name" VARCHAR(128) NOT NULL,
    "NameLocalizations" TEXT     NULL,
    "Description" TEXT     NULL,
    "DescriptionLocalizations" TEXT     NULL,
    "Mandatory" BOOLEAN   NOT NULL,
    "Hidden" BOOLEAN   NOT NULL,
    "Value" TEXT     NULL,
    "ValueType" INT          NOT NULL,
    "ParameterType" INT          NOT NULL,
    "OverrideType" INT          NOT NULL,
    "Attributes" TEXT     NULL,
    PRIMARY KEY ("Id")
);

CREATE TABLE IF NOT EXISTS "ReportParameterAudit" (
    "Id" INT          NOT NULL GENERATED ALWAYS AS IDENTITY,
    "ReportParameterId" INT          NOT NULL,
    "Status" INT          NOT NULL,
    "Created" TIMESTAMP(6)  NOT NULL,
    "Updated" TIMESTAMP(6)  NOT NULL,
    "Name" VARCHAR(128) NOT NULL,
    "NameLocalizations" TEXT     NULL,
    "Description" TEXT     NULL,
    "DescriptionLocalizations" TEXT     NULL,
    "Mandatory" BOOLEAN   NOT NULL,
    "Hidden" BOOLEAN   NOT NULL,
    "Value" TEXT     NULL,
    "ValueType" INT          NOT NULL,
    "ParameterType" INT          NOT NULL,
    "OverrideType" INT          NOT NULL,
    "Attributes" TEXT     NULL,
    PRIMARY KEY ("Id")
);

CREATE TABLE IF NOT EXISTS "ReportTemplate" (
    "Id" INT          NOT NULL GENERATED ALWAYS AS IDENTITY,
    "Status" INT          NOT NULL,
    "Created" TIMESTAMP(6)  NOT NULL,
    "Updated" TIMESTAMP(6)  NOT NULL,
    "ReportId" INT          NOT NULL,
    "Name" VARCHAR(128) NOT NULL,
    "Culture" VARCHAR(128) NOT NULL,
    "Content" TEXT     NOT NULL,
    "ContentType" VARCHAR(128) NULL,
    "Schema" TEXT     NULL,
    "Resource" VARCHAR(256) NULL,
    "OverrideType" INT          NOT NULL,
    "Attributes" TEXT     NULL,
    PRIMARY KEY ("Id")
);

CREATE TABLE IF NOT EXISTS "ReportTemplateAudit" (
    "Id" INT          NOT NULL GENERATED ALWAYS AS IDENTITY,
    "Status" INT          NOT NULL,
    "Created" TIMESTAMP(6)  NOT NULL,
    "Updated" TIMESTAMP(6)  NOT NULL,
    "ReportTemplateId" INT          NOT NULL,
    "Name" VARCHAR(128) NOT NULL,
    "Culture" VARCHAR(128) NOT NULL,
    "Content" TEXT     NOT NULL,
    "ContentType" VARCHAR(128) NULL,
    "Schema" TEXT     NULL,
    "Resource" VARCHAR(256) NULL,
    "OverrideType" INT          NOT NULL,
    "Attributes" TEXT     NULL,
    PRIMARY KEY ("Id")
);

CREATE TABLE IF NOT EXISTS "Script" (
    "Id" INT          NOT NULL GENERATED ALWAYS AS IDENTITY,
    "Status" INT          NOT NULL,
    "Created" TIMESTAMP(6)  NOT NULL,
    "Updated" TIMESTAMP(6)  NOT NULL,
    "RegulationId" INT          NOT NULL,
    "Name" VARCHAR(128) NOT NULL,
    "FunctionTypeMask" BIGINT       NOT NULL,
    "Value" TEXT     NOT NULL,
    "OverrideType" INT          NOT NULL,
    PRIMARY KEY ("Id")
);

CREATE TABLE IF NOT EXISTS "ScriptAudit" (
    "Id" INT          NOT NULL GENERATED ALWAYS AS IDENTITY,
    "Status" INT          NOT NULL,
    "Created" TIMESTAMP(6)  NOT NULL,
    "Updated" TIMESTAMP(6)  NOT NULL,
    "ScriptId" INT          NOT NULL,
    "Name" VARCHAR(128) NOT NULL,
    "FunctionTypeMask" BIGINT       NOT NULL,
    "Value" TEXT     NOT NULL,
    "OverrideType" INT          NOT NULL,
    PRIMARY KEY ("Id")
);

CREATE TABLE IF NOT EXISTS "Task" (
    "Id" INT          NOT NULL GENERATED ALWAYS AS IDENTITY,
    "Status" INT          NOT NULL,
    "Created" TIMESTAMP(6)  NOT NULL,
    "Updated" TIMESTAMP(6)  NOT NULL,
    "TenantId" INT          NOT NULL,
    "Name" VARCHAR(128) NOT NULL,
    "NameLocalizations" TEXT     NULL,
    "Category" VARCHAR(128) NULL,
    "Instruction" TEXT     NOT NULL,
    "ScheduledUserId" INT          NOT NULL,
    "Scheduled" TIMESTAMP(6)  NOT NULL,
    "CompletedUserId" INT          NULL,
    "Completed" TIMESTAMP(6)  NULL,
    "Attributes" TEXT     NULL,
    PRIMARY KEY ("Id")
);

CREATE TABLE IF NOT EXISTS "Tenant" (
    "Id" INT          NOT NULL GENERATED ALWAYS AS IDENTITY,
    "Status" INT          NOT NULL,
    "Created" TIMESTAMP(6)  NOT NULL,
    "Updated" TIMESTAMP(6)  NOT NULL,
    "Identifier" VARCHAR(128) NOT NULL,
    "Culture" VARCHAR(128) NULL,
    "Calendar" VARCHAR(128) NULL,
    "Attributes" TEXT     NULL,
    PRIMARY KEY ("Id")
);

CREATE TABLE IF NOT EXISTS "User" (
    "Id" INT          NOT NULL GENERATED ALWAYS AS IDENTITY,
    "Status" INT          NOT NULL,
    "Created" TIMESTAMP(6)  NOT NULL,
    "Updated" TIMESTAMP(6)  NOT NULL,
    "TenantId" INT          NOT NULL,
    "Identifier" VARCHAR(128) NOT NULL,
    "UserType" INT          NOT NULL,
    "Password" VARCHAR(128) NULL,
    "StoredSalt" BYTEA     NULL,
    "FirstName" VARCHAR(128) NOT NULL,
    "LastName" VARCHAR(128) NOT NULL,
    "Culture" VARCHAR(128) NULL,
    "Attributes" TEXT     NULL,
    PRIMARY KEY ("Id")
);

CREATE TABLE IF NOT EXISTS "Version" (
    "Id" INT          NOT NULL GENERATED ALWAYS AS IDENTITY,
    "Created" TIMESTAMP(6)  NOT NULL,
    "MajorVersion" INT          NOT NULL,
    "MinorVersion" INT          NOT NULL,
    "SubVersion" INT          NOT NULL,
    "Owner" VARCHAR(128) NOT NULL,
    "Description" TEXT     NOT NULL,
    PRIMARY KEY ("Id")
);

CREATE TABLE IF NOT EXISTS "WageType" (
    "Id" INT           NOT NULL GENERATED ALWAYS AS IDENTITY,
    "Status" INT           NOT NULL,
    "Created" TIMESTAMP(6)   NOT NULL,
    "Updated" TIMESTAMP(6)   NOT NULL,
    "RegulationId" INT           NOT NULL,
    "Name" VARCHAR(128)  NOT NULL,
    "NameLocalizations" TEXT      NULL,
    "WageTypeNumber" DECIMAL(28,6) NOT NULL,
    "Description" TEXT      NULL,
    "DescriptionLocalizations" TEXT      NULL,
    "OverrideType" INT           NOT NULL,
    "ValueType" INT           NOT NULL,
    "Calendar" VARCHAR(128)  NULL,
    "Culture" VARCHAR(128)  NULL,
    "Collectors" TEXT      NULL,
    "CollectorGroups" TEXT      NULL,
    "ValueExpression" TEXT      NULL,
    "ResultExpression" TEXT      NULL,
    "ValueActions" TEXT      NULL,
    "ResultActions" TEXT      NULL,
    "Script" TEXT      NULL,
    "ScriptVersion" VARCHAR(128)  NULL,
    "Binary" BYTEA      NULL,
    "ScriptHash" INT           NULL,
    "Attributes" TEXT      NULL,
    "Clusters" TEXT      NULL,
    PRIMARY KEY ("Id")
);

CREATE TABLE IF NOT EXISTS "WageTypeAudit" (
    "Id" INT           NOT NULL GENERATED ALWAYS AS IDENTITY,
    "Status" INT           NOT NULL,
    "Created" TIMESTAMP(6)   NOT NULL,
    "Updated" TIMESTAMP(6)   NOT NULL,
    "WageTypeId" INT           NOT NULL,
    "Name" VARCHAR(128)  NOT NULL,
    "NameLocalizations" TEXT      NULL,
    "WageTypeNumber" DECIMAL(28,6) NOT NULL,
    "Description" TEXT      NULL,
    "DescriptionLocalizations" TEXT      NULL,
    "OverrideType" INT           NOT NULL,
    "ValueType" INT           NOT NULL,
    "Calendar" VARCHAR(128)  NULL,
    "Culture" VARCHAR(128)  NULL,
    "Collectors" TEXT      NULL,
    "CollectorGroups" TEXT      NULL,
    "ValueExpression" TEXT      NULL,
    "ResultExpression" TEXT      NULL,
    "ValueActions" TEXT      NULL,
    "ResultActions" TEXT      NULL,
    "Script" TEXT      NULL,
    "ScriptVersion" VARCHAR(128)  NULL,
    "Binary" BYTEA      NULL,
    "ScriptHash" INT           NULL,
    "Attributes" TEXT      NULL,
    "Clusters" TEXT      NULL,
    PRIMARY KEY ("Id")
);

CREATE TABLE IF NOT EXISTS "WageTypeCustomResult" (
    "Id" INT           NOT NULL GENERATED ALWAYS AS IDENTITY,
    "Status" INT           NOT NULL,
    "Created" TIMESTAMP(6)   NOT NULL,
    "Updated" TIMESTAMP(6)   NOT NULL,
    "WageTypeResultId" INT           NOT NULL,
    "TenantId" INT           NOT NULL,
    "EmployeeId" INT           NOT NULL,
    "DivisionId" INT           NULL,
    "WageTypeNumber" DECIMAL(28,6) NOT NULL,
    "WageTypeName" VARCHAR(128)  NOT NULL,
    "WageTypeNameLocalizations" TEXT      NULL,
    "Source" VARCHAR(128)  NOT NULL,
    "ValueType" INT           NOT NULL,
    "Value" DECIMAL(28,6) NOT NULL,
    "Culture" VARCHAR(128)  NOT NULL,
    "Start" TIMESTAMP(6)   NOT NULL,
    "StartHash" INT           NOT NULL,
    "End" TIMESTAMP(6)   NOT NULL,
    "PayrunJobId" INT           NOT NULL,
    "Forecast" VARCHAR(128)  NULL,
    "ParentJobId" INT           NULL,
    "Tags" TEXT      NULL,
    "Attributes" TEXT      NULL,
    PRIMARY KEY ("Id")
);

CREATE TABLE IF NOT EXISTS "WageTypeResult" (
    "Id" INT           NOT NULL GENERATED ALWAYS AS IDENTITY,
    "Status" INT           NOT NULL,
    "Created" TIMESTAMP(6)   NOT NULL,
    "Updated" TIMESTAMP(6)   NOT NULL,
    "PayrollResultId" INT           NOT NULL,
    "TenantId" INT           NOT NULL,
    "EmployeeId" INT           NOT NULL,
    "DivisionId" INT           NULL,
    "WageTypeId" INT           NOT NULL,
    "WageTypeNumber" DECIMAL(28,6) NOT NULL,
    "WageTypeName" VARCHAR(128)  NOT NULL,
    "WageTypeNameLocalizations" TEXT      NULL,
    "ValueType" INT           NOT NULL,
    "Value" DECIMAL(28,6) NOT NULL,
    "Culture" VARCHAR(128)  NOT NULL,
    "Start" TIMESTAMP(6)   NOT NULL,
    "StartHash" INT           NOT NULL,
    "End" TIMESTAMP(6)   NOT NULL,
    "PayrunJobId" INT           NOT NULL,
    "Forecast" VARCHAR(128)  NULL,
    "ParentJobId" INT           NULL,
    "Tags" TEXT      NULL,
    "Attributes" TEXT      NULL,
    PRIMARY KEY ("Id")
);

CREATE TABLE IF NOT EXISTS "Webhook" (
    "Id" INT          NOT NULL GENERATED ALWAYS AS IDENTITY,
    "Status" INT          NOT NULL,
    "Created" TIMESTAMP(6)  NOT NULL,
    "Updated" TIMESTAMP(6)  NOT NULL,
    "TenantId" INT          NOT NULL,
    "Name" VARCHAR(128) NOT NULL,
    "ReceiverAddress" VARCHAR(128) NOT NULL,
    "Action" INT          NOT NULL,
    "Attributes" TEXT     NULL,
    PRIMARY KEY ("Id")
);

CREATE TABLE IF NOT EXISTS "WebhookMessage" (
    "Id" INT          NOT NULL GENERATED ALWAYS AS IDENTITY,
    "Status" INT          NOT NULL,
    "Created" TIMESTAMP(6)  NOT NULL,
    "Updated" TIMESTAMP(6)  NOT NULL,
    "WebhookId" INT          NOT NULL,
    "ActionName" VARCHAR(128) NOT NULL,
    "ReceiverAddress" VARCHAR(128) NOT NULL,
    "RequestDate" TIMESTAMP(6)  NOT NULL,
    "RequestMessage" TEXT     NULL,
    "RequestOperation" TEXT     NULL,
    "ResponseDate" TIMESTAMP(6)  NULL,
    "ResponseStatus" INT          NULL,
    "ResponseMessage" TEXT     NULL,
    PRIMARY KEY ("Id")
);


-- =============================================================================

-- INDEXES
-- =============================================================================

-- =============================================================================

CREATE UNIQUE INDEX IX_Calendar_UniquePerTenant               ON "Calendar" ("Name", "TenantId");
CREATE UNIQUE INDEX IX_Case_UniqueNamePerRegulation           ON "Case" ("RegulationId", "Name");
CREATE UNIQUE INDEX IX_CaseField_UniqueNamePerCase            ON "CaseField" ("CaseId", "Name");
CREATE        INDEX IX_CaseField_ValueType                    ON "CaseField" ("ValueType");
CREATE        INDEX IX_CaseRelation_SourceCaseName            ON "CaseRelation" ("RegulationId", "SourceCaseName");
CREATE        INDEX IX_CaseRelation_TargetCaseName            ON "CaseRelation" ("RegulationId", "TargetCaseName");
CREATE        INDEX IX_CaseRelation_TargetSlot                ON "CaseRelation" ("RegulationId", "TargetCaseSlot");
CREATE UNIQUE INDEX IX_CaseRelation_UniqueRelation            ON "CaseRelation" ("RegulationId", "RelationHash");
CREATE        INDEX IX_Collector_CollectMode                  ON "Collector" ("CollectMode");
CREATE UNIQUE INDEX IX_Collector_UniquePerReg                 ON "Collector" ("Name", "RegulationId");
CREATE        INDEX IX_CollectorCustomResult_ResultId         ON "CollectorCustomResult" ("CollectorResultId");
-- Covering index: eliminates Key Lookups on the hot per-employee query path.
CREATE        INDEX IX_CollectorCustomResult_Employee_Coll    ON "CollectorCustomResult" ("TenantId", "EmployeeId", "StartHash", "CollectorNameHash")
    INCLUDE ("Start", "Created", "DivisionId", "Forecast", "ParentJobId", "PayrunJobId");
CREATE        INDEX IX_CollectorResult_PayrollResultId        ON "CollectorResult" ("PayrollResultId");
-- Covering index: eliminates Key Lookups on the hot per-employee query path.
CREATE        INDEX IX_CollectorResult_Employee_Collector     ON "CollectorResult" ("TenantId", "EmployeeId", "StartHash", "CollectorNameHash")
    INCLUDE ("Start", "Created", "DivisionId", "Forecast", "ParentJobId", "PayrunJobId");
CREATE UNIQUE INDEX IX_CompanyCaseValue_Unique                ON "CompanyCaseValue" ("TenantId", "DivisionId", "CaseFieldName", "CaseSlot", "Created");
CREATE        INDEX IX_CompanyCaseValue_CaseFieldName         ON "CompanyCaseValue" ("CaseFieldName");
CREATE        INDEX IX_CompanyCaseValue_Slot                  ON "CompanyCaseValue" ("CaseSlot");
-- Covering index: lead key matches SP WHERE filter; INCLUDE avoids Key Lookups on Value/Start/"End".
CREATE        INDEX IX_CompanyCaseValue_TenantId              ON "CompanyCaseValue" ("TenantId", "CaseFieldName")
    INCLUDE ("DivisionId", "Start", "End", "Value", "NumericValue", "CancellationDate", "Forecast", "Created", "Status");
CREATE UNIQUE INDEX IX_CompanyCaseValueChange_Unique          ON "CompanyCaseValueChange" ("CaseValueId", "CaseChangeId");
CREATE UNIQUE INDEX IX_Division_UniquePerTenant               ON "Division" ("Name", "TenantId");
CREATE        INDEX IX_Employee_TenantId                      ON "Employee" ("TenantId", "Status");
CREATE UNIQUE INDEX IX_Employee_UniqueIdentifierPerTenant     ON "Employee" ("Identifier", "TenantId");
-- Covering index: lead key matches per-employee and tenant-wide SP filters.
CREATE        INDEX IX_EmployeeCaseValue_EmployeeId           ON "EmployeeCaseValue" ("EmployeeId", "CaseFieldName")
    INCLUDE ("DivisionId", "Start", "End", "Value", "NumericValue", "CancellationDate", "Forecast", "Created", "Status");
CREATE        INDEX IX_EmployeeCaseValue_CaseFieldName        ON "EmployeeCaseValue" ("CaseFieldName");
CREATE        INDEX IX_EmployeeCaseValue_Slot                 ON "EmployeeCaseValue" ("CaseSlot");
CREATE UNIQUE INDEX IX_EmployeeCaseValue_Unique               ON "EmployeeCaseValue" ("EmployeeId", "DivisionId", "CaseFieldName", "CaseSlot", "Created");
CREATE UNIQUE INDEX IX_EmployeeCaseValueChange_Unique         ON "EmployeeCaseValueChange" ("CaseValueId", "CaseChangeId");
CREATE UNIQUE INDEX IX_EmployeeDivision_UniquePerEmployee     ON "EmployeeDivision" ("EmployeeId", "DivisionId");
-- Covering index: lead key matches SP WHERE filter; INCLUDE avoids Key Lookups on Value/Start/"End".
CREATE        INDEX IX_GlobalCaseValue_TenantId               ON "GlobalCaseValue" ("TenantId", "CaseFieldName")
    INCLUDE ("DivisionId", "Start", "End", "Value", "NumericValue", "CancellationDate", "Forecast", "Created", "Status");
CREATE        INDEX IX_GlobalCaseValue_CaseFieldName          ON "GlobalCaseValue" ("CaseFieldName");
CREATE        INDEX IX_GlobalCaseValue_Slot                   ON "GlobalCaseValue" ("CaseSlot");
CREATE UNIQUE INDEX IX_GlobalCaseValue_Unique                 ON "GlobalCaseValue" ("TenantId", "DivisionId", "CaseFieldName", "CaseSlot", "Created");
CREATE UNIQUE INDEX IX_GlobalCaseValueChange_Unique           ON "GlobalCaseValueChange" ("CaseValueId", "CaseChangeId");
CREATE UNIQUE INDEX IX_Lookup_UniquePerReg                    ON "Lookup" ("Name", "RegulationId");
CREATE        INDEX IX_LookupValue_KeyHash                    ON "LookupValue" ("KeyHash");
CREATE UNIQUE INDEX IX_LookupValue_UniqueValueKeyPerLookup    ON "LookupValue" ("LookupId", "LookupHash");
-- Covering index: lead key matches SP WHERE filter; INCLUDE avoids Key Lookups on Value/Start/"End".
CREATE        INDEX IX_NationalCaseValue_TenantId             ON "NationalCaseValue" ("TenantId", "CaseFieldName")
    INCLUDE ("DivisionId", "Start", "End", "Value", "NumericValue", "CancellationDate", "Forecast", "Created", "Status");
CREATE        INDEX IX_NationalCaseValue_CaseFieldName        ON "NationalCaseValue" ("CaseFieldName");
CREATE        INDEX IX_NationalCaseValue_Slot                 ON "NationalCaseValue" ("CaseSlot");
CREATE UNIQUE INDEX IX_NationalCaseValue_Unique               ON "NationalCaseValue" ("TenantId", "DivisionId", "CaseFieldName", "CaseSlot", "Created");
CREATE UNIQUE INDEX IX_NationalCaseValueChange_Unique         ON "NationalCaseValueChange" ("CaseValueId", "CaseChangeId");
-- Column order: Name is the higher-cardinality discriminator; TenantId narrows the seek.
CREATE UNIQUE INDEX IX_Payroll_UniquePerTenant                ON "Payroll" ("Name", "TenantId");
CREATE        INDEX IX_PayrollLayer_Priority                  ON "PayrollLayer" ("Priority");
CREATE UNIQUE INDEX IX_PayrollLayer_UniqueLevelPriority       ON "PayrollLayer" ("Level", "Priority", "PayrollId");
CREATE UNIQUE INDEX IX_PayrollLayer_UniqueNamePerPayroll      ON "PayrollLayer" ("RegulationName", "PayrollId");
-- Covering index: EmployeeId seek; INCLUDE avoids Key Lookups in GetPayrollResults joins.
CREATE        INDEX IX_PayrollResult_EmployeeId               ON "PayrollResult" ("EmployeeId")
    INCLUDE ("PayrunJobId", "DivisionId", "PayrunId");
CREATE        INDEX IX_PayrollResult_PayrunId                 ON "PayrollResult" ("PayrunId");
CREATE UNIQUE INDEX IX_PayrollResult_UniqueEmployeePerJob     ON "PayrollResult" ("EmployeeId", "PayrunJobId");
-- Unique per payroll (not per tenant): PayrollId scopes the payrun name correctly.
CREATE UNIQUE INDEX IX_Payrun_UniquePerPayroll                ON "Payrun" ("Name", "PayrollId");
CREATE        INDEX IX_PayrunJob_TenantId                     ON "PayrunJob" ("TenantId", "JobStatus");
CREATE        INDEX IX_PayrunJob_PayrunId                     ON "PayrunJob" ("PayrunId");
CREATE        INDEX IX_PayrunJob_ParentJob                    ON "PayrunJob" ("ParentJobId");
CREATE        INDEX IX_PayrunJob_PeriodStart                  ON "PayrunJob" ("PeriodStart" DESC);
-- Column order matches SS reference: EmployeeId is the per-employee seek key.
CREATE UNIQUE INDEX IX_PayrunJobEmployee_Unique               ON "PayrunJobEmployee" ("EmployeeId", "PayrunJobId");
CREATE UNIQUE INDEX IX_PayrunParameter_UniquePerPayrun        ON "PayrunParameter" ("PayrunId", "Name");
CREATE        INDEX IX_PayrunResult_PayrollResultId           ON "PayrunResult" ("PayrollResultId");
-- Covering index: TenantId+EmployeeId+StartHash+Name matches the hot GetPayrunResults filter.
CREATE        INDEX IX_PayrunResult_Employee                  ON "PayrunResult" ("TenantId", "EmployeeId", "StartHash", "Name")
    INCLUDE ("Start", "Created", "DivisionId", "Forecast", "ParentJobId", "PayrunJobId");
CREATE        INDEX IX_PayrunTrace_Employee                   ON "PayrunTrace" ("TenantId", "EmployeeId")
    INCLUDE ("Created", "DivisionId", "Forecast", "ParentJobId", "PayrunJobId");
CREATE        INDEX IX_PayrunTrace_PayrollResultId            ON "PayrunTrace" ("PayrollResultId");
CREATE        INDEX IX_Regulation_TenantId                    ON "Regulation" ("TenantId");
CREATE        INDEX IX_Regulation_Name                        ON "Regulation" ("Name");
-- Regulations are versioned: (Name, ValidFrom, TenantId) enforces per-version uniqueness.
CREATE UNIQUE INDEX IX_Regulation_UniqueValidFrom             ON "Regulation" ("Name", "ValidFrom", "TenantId");
CREATE UNIQUE INDEX IX_RegulationShare_UniqueShare            ON "RegulationShare" ("ProviderTenantId", "ProviderRegulationId", "ConsumerTenantId", "ConsumerDivisionId");
-- Supports GetDerived* isolation check: ConsumerTenantId + ProviderRegulationId + IsolationLevel seek.
CREATE        INDEX IX_RegulationShare_ConsumerProviderLevel  ON "RegulationShare" ("ConsumerTenantId", "ProviderRegulationId", "IsolationLevel");
CREATE        INDEX IX_Report_Category                        ON "Report" ("Category");
CREATE UNIQUE INDEX IX_Report_UniquePerReg                    ON "Report" ("Name", "RegulationId");
CREATE UNIQUE INDEX IX_ReportParameter_UniquePerReport        ON "ReportParameter" ("Name", "ReportId");
-- Two separate UNIQUE constraints matching SS: one per language, one per name.
CREATE UNIQUE INDEX IX_ReportTemplate_UniqueLanguagePerReport ON "ReportTemplate" ("ReportId", "Culture");
CREATE UNIQUE INDEX IX_ReportTemplate_UniqueNamePerReport     ON "ReportTemplate" ("ReportId", "Name");
CREATE UNIQUE INDEX IX_Script_UniquePerReg                    ON "Script" ("Name", "RegulationId");
CREATE UNIQUE INDEX IX_Tenant_Identifier                      ON "Tenant" ("Identifier");
CREATE UNIQUE INDEX IX_User_UniquePerTenant                   ON "User" ("Identifier", "TenantId");
CREATE UNIQUE INDEX IX_Version_Unique                         ON "Version" ("MajorVersion", "MinorVersion", "SubVersion");
CREATE UNIQUE INDEX IX_WageType_UniqueNamePerReg              ON "WageType" ("RegulationId", "Name");
CREATE UNIQUE INDEX IX_WageType_UniqueNumberPerReg            ON "WageType" ("RegulationId", "WageTypeNumber");
CREATE        INDEX IX_WageTypeCustomResult_ResultId          ON "WageTypeCustomResult" ("WageTypeResultId");
-- Covering index: eliminates Key Lookups on the hot per-employee query path.
CREATE        INDEX IX_WageTypeCustomResult_Employee_WT       ON "WageTypeCustomResult" ("TenantId", "EmployeeId", "StartHash", "WageTypeNumber")
    INCLUDE ("Start", "Created", "DivisionId", "Forecast", "ParentJobId", "PayrunJobId");
CREATE        INDEX IX_WageTypeResult_PayrollResultId         ON "WageTypeResult" ("PayrollResultId");
-- Covering index: eliminates Key Lookups on the hot per-employee query path.
CREATE        INDEX IX_WageTypeResult_Employee_WT             ON "WageTypeResult" ("TenantId", "EmployeeId", "StartHash", "WageTypeNumber")
    INCLUDE ("Start", "Created", "DivisionId", "Forecast", "ParentJobId", "PayrunJobId");
CREATE UNIQUE INDEX IX_Webhook_UniquePerTenant                ON "Webhook" ("Name", "TenantId");


-- =============================================================================

-- FUNCTIONS (7)
-- =============================================================================

-- BuildAttributeQuery.pg.sql
-- ----------------------------------------------------------------------
-- =============================================================================
-- BuildAttributeQuery
-- Builds a SQL fragment for dynamic attribute column projection.
-- Used by CaseValue pivot SPs and GetPayrollResultValues.
--
-- T-SQL: imperative WHILE + OPENJSON + string concatenation
-- MySQL: JSON_TABLE with FOR ORDINALITY + GROUP_CONCAT (order preserved)
-- PostgreSQL: jsonb_array_elements_text WITH ORDINALITY + STRING_AGG
--
-- Output: '' if empty, ',' + fragment + newline if attributes present
--
-- Attribute prefix convention:
--   TA_ -> GetTextAttributeValue(field, 'name') AS TA_xxx
--   NA_ -> GetNumericAttributeValue(field, 'name') AS NA_xxx
--   DA_ -> GetDateAttributeValue(field, 'name') AS DA_xxx
--   NULL field -> NULL AS xxx  ("PayrunResult" has no attribute field)
--
-- NOTE: Attribute JSON keys are plain names ("City"), not prefixed ("TA_City").
-- The TA_/NA_/DA_ prefix is the output column alias only.
-- =============================================================================

CREATE OR REPLACE FUNCTION BuildAttributeQuery(
    p_attributeField TEXT,
    p_attributes     TEXT
)
RETURNS TEXT
LANGUAGE plpgsql
STABLE
AS $$
DECLARE
    v_parts TEXT;
    v_sql   TEXT DEFAULT '';
BEGIN
    IF p_attributes IS NULL THEN
        RETURN v_sql;
    END IF;

    IF jsonb_array_length(p_attributes::jsonb) = 0 THEN
        RETURN v_sql;
    END IF;

    SELECT STRING_AGG(
        CASE
            WHEN p_attributeField IS NULL THEN
                'NULL AS ' || j.val
            WHEN LEFT(j.val, 3) = 'TA_' THEN
                'GetTextAttributeValue(' || p_attributeField || ', ''' || SUBSTRING(j.val, 4) || ''') AS ' || j.val
            WHEN LEFT(j.val, 3) = 'DA_' THEN
                'GetDateAttributeValue(' || p_attributeField || ', ''' || SUBSTRING(j.val, 4) || ''') AS ' || j.val
            WHEN LEFT(j.val, 3) = 'NA_' THEN
                'GetNumericAttributeValue(' || p_attributeField || ', ''' || SUBSTRING(j.val, 4) || ''') AS ' || j.val
            ELSE NULL
        END
        , ', ' ORDER BY j.idx
    )
    INTO v_parts
    FROM jsonb_array_elements_text(p_attributes::jsonb) WITH ORDINALITY AS j(val, idx)
    WHERE LENGTH(TRIM(j.val)) > 0;

    IF v_parts IS NOT NULL AND LENGTH(v_parts) > 0 THEN
        v_sql := ',' || v_parts || E'\n        ';
    END IF;

    RETURN v_sql;
END;
$$;

-- GetAttributeNames.pg.sql
-- ----------------------------------------------------------------------
-- =============================================================================
-- GetAttributeNames
-- Builds a comma-separated list of attribute names from a JSON array.
-- Used by GetPayrollResultValues for the outer SELECT projection.
--
-- T-SQL: imperative WHILE + OPENJSON + string concatenation
-- MySQL: JSON_TABLE with FOR ORDINALITY + GROUP_CONCAT
-- PostgreSQL: jsonb_array_elements_text WITH ORDINALITY + STRING_AGG
--
-- Output: '' if empty, ',' + names + newline if non-empty
-- =============================================================================

CREATE OR REPLACE FUNCTION GetAttributeNames(
    p_attributes TEXT
)
RETURNS TEXT
LANGUAGE plpgsql
STABLE
AS $$
DECLARE
    v_parts TEXT;
    v_sql   TEXT DEFAULT '';
BEGIN
    IF p_attributes IS NULL THEN
        RETURN v_sql;
    END IF;

    IF jsonb_array_length(p_attributes::jsonb) = 0 THEN
        RETURN v_sql;
    END IF;

    SELECT STRING_AGG(j.val, ', ' ORDER BY j.idx)
    INTO v_parts
    FROM jsonb_array_elements_text(p_attributes::jsonb) WITH ORDINALITY AS j(val, idx)
    WHERE LENGTH(TRIM(j.val)) > 0;

    IF v_parts IS NOT NULL AND LENGTH(v_parts) > 0 THEN
        v_sql := ',' || v_parts || E'\n        ';
    END IF;

    RETURN v_sql;
END;
$$;

-- GetDateAttributeValue.pg.sql
-- ----------------------------------------------------------------------
-- =============================================================================
-- GetDateAttributeValue
-- Returns TIMESTAMP(6) from a JSON attribute stored as ISO 8601 string.
-- NULL if attribute is not a string or not parseable as timestamp.
--
-- T-SQL: RETURN IIF(@type = 1, CAST(@value AS DATETIME2(7)), NULL)
-- MySQL: JSON_TYPE='STRING' + CAST AS DATETIME(6)
-- PostgreSQL: jsonb_typeof='string' + CAST AS TIMESTAMP(6)
-- =============================================================================

CREATE OR REPLACE FUNCTION GetDateAttributeValue(
    p_attributes TEXT,
    p_name       VARCHAR(255)
)
RETURNS TIMESTAMP(6)
LANGUAGE plpgsql
STABLE
AS $$
DECLARE
    v_type TEXT;
    v_raw  VARCHAR(50);
BEGIN
    IF p_attributes IS NULL OR p_name IS NULL THEN
        RETURN NULL;
    END IF;

    v_type := jsonb_typeof(p_attributes::jsonb -> p_name);

    IF v_type = 'string' THEN
        v_raw := p_attributes::jsonb ->> p_name;
        RETURN v_raw::TIMESTAMP(6);
    END IF;

    RETURN NULL;
END;
$$;

-- GetLocalizedValue.pg.sql
-- ----------------------------------------------------------------------
-- =============================================================================
-- GetLocalizedValue
-- Returns the value for the given culture from a JSON localizations object.
-- Falls back to p_fallback if culture key not found.
--
-- T-SQL: SELECT @value = value FROM OPENJSON(@localizations) WHERE [key] = @culture
-- MySQL: JSON_EXTRACT with quoted key syntax $."de-CH" (handles hyphens)
-- PostgreSQL: jsonb ->> key operator (handles any key including hyphens)
--
-- IMPORTANT: Culture codes like "de-CH" contain a hyphen.
-- MySQL requires quoted path $."de-CH". PostgreSQL's ->> operator handles
-- any key natively without quoting concerns.
-- =============================================================================

CREATE OR REPLACE FUNCTION GetLocalizedValue(
    p_localizations TEXT,
    p_culture       VARCHAR(128),
    p_fallback      TEXT
)
RETURNS TEXT
LANGUAGE plpgsql
STABLE
AS $$
DECLARE
    v_value TEXT;
BEGIN
    IF p_localizations IS NULL OR p_culture IS NULL THEN
        RETURN p_fallback;
    END IF;

    v_value := p_localizations::jsonb ->> p_culture;

    IF v_value IS NULL THEN
        RETURN p_fallback;
    END IF;

    RETURN v_value;
END;
$$;

-- GetNumericAttributeValue.pg.sql
-- ----------------------------------------------------------------------
-- =============================================================================
-- GetNumericAttributeValue
-- Returns NUMERIC(28,6) value of a JSON attribute, NULL if not numeric.
--
-- T-SQL: RETURN IIF(@type = 2, CAST(@value AS DECIMAL(28,6)), NULL)
-- MySQL: JSON_TYPE checks for 'INTEGER' or 'DOUBLE'
-- PostgreSQL: jsonb_typeof checks for 'number'
-- =============================================================================

CREATE OR REPLACE FUNCTION GetNumericAttributeValue(
    p_attributes TEXT,
    p_name       VARCHAR(255)
)
RETURNS NUMERIC(28,6)
LANGUAGE plpgsql
STABLE
AS $$
DECLARE
    v_type TEXT;
BEGIN
    IF p_attributes IS NULL OR p_name IS NULL THEN
        RETURN NULL;
    END IF;

    v_type := jsonb_typeof(p_attributes::jsonb -> p_name);

    IF v_type = 'number' THEN
        RETURN (p_attributes::jsonb ->> p_name)::NUMERIC(28,6);
    END IF;

    RETURN NULL;
END;
$$;

-- GetTextAttributeValue.pg.sql
-- ----------------------------------------------------------------------
-- =============================================================================
-- GetTextAttributeValue
-- Returns the string value of a JSON attribute key, NULL if type is not string.
--
-- T-SQL: RETURN IIF(@type = 1, @value, NULL)  -- type 1 = string in OPENJSON
-- MySQL: JSON_EXTRACT + JSON_TYPE check for 'STRING'
-- PostgreSQL: jsonb_typeof check for 'string'
-- =============================================================================

CREATE OR REPLACE FUNCTION GetTextAttributeValue(
    p_attributes TEXT,
    p_name       VARCHAR(255)
)
RETURNS TEXT
LANGUAGE plpgsql
STABLE
AS $$
DECLARE
    v_type TEXT;
BEGIN
    IF p_attributes IS NULL OR p_name IS NULL THEN
        RETURN NULL;
    END IF;

    v_type := jsonb_typeof(p_attributes::jsonb -> p_name);

    IF v_type = 'string' THEN
        RETURN p_attributes::jsonb ->> p_name;
    END IF;

    RETURN NULL;
END;
$$;

-- IsMatchingCluster.pg.sql
-- ----------------------------------------------------------------------
-- =============================================================================
-- IsMatchingCluster
-- Tests include/exclude cluster filters against a test cluster array.
-- All arrays are JSON arrays of VARCHAR(128).
--
-- T-SQL: imperative WHILE loop over OPENJSON
-- MySQL: set-based JSON_TABLE + EXISTS / NOT EXISTS
-- PostgreSQL: jsonb_array_elements_text + EXISTS / NOT EXISTS
--
-- Logic:
--   include: every cluster in includeClusters must appear in testClusters
--   exclude: no cluster in excludeClusters may appear in testClusters
--   returns 1 (match) or 0 (no match)
-- =============================================================================

CREATE OR REPLACE FUNCTION IsMatchingCluster(
    p_includeClusters VARCHAR(4000),
    p_excludeClusters VARCHAR(4000),
    p_testClusters    VARCHAR(4000)
)
RETURNS INTEGER
LANGUAGE plpgsql
STABLE
AS $$
DECLARE
    v_testClusters VARCHAR(4000);
BEGIN
    v_testClusters := COALESCE(p_testClusters, '[]');

    IF p_includeClusters IS NOT NULL AND jsonb_array_length(p_includeClusters::jsonb) > 0 THEN
        IF EXISTS (
            SELECT 1
            FROM jsonb_array_elements_text(p_includeClusters::jsonb) AS inc(val)
            WHERE LENGTH(TRIM(inc.val)) > 0
              AND NOT EXISTS (
                SELECT 1
                FROM jsonb_array_elements_text(v_testClusters::jsonb) AS tst(val)
                WHERE tst.val = inc.val)
        ) THEN
            RETURN 0;
        END IF;
    END IF;

    IF p_excludeClusters IS NOT NULL AND jsonb_array_length(p_excludeClusters::jsonb) > 0 THEN
        IF EXISTS (
            SELECT 1
            FROM jsonb_array_elements_text(p_excludeClusters::jsonb) AS exc(val)
            WHERE LENGTH(TRIM(exc.val)) > 0
              AND EXISTS (
                SELECT 1
                FROM jsonb_array_elements_text(v_testClusters::jsonb) AS tst(val)
                WHERE tst.val = exc.val)
        ) THEN
            RETURN 0;
        END IF;
    END IF;

    RETURN 1;
END;
$$;



-- STORED PROCEDURES (44)
-- =============================================================================

-- DeleteAllCaseValues.pg.sql
-- ----------------------------------------------------------------------
-- =============================================================================
-- DeleteAllCaseValues
-- Delegates to the four scope-specific procedures.
-- =============================================================================

CREATE OR REPLACE PROCEDURE DeleteAllCaseValues()
LANGUAGE plpgsql
AS $$
BEGIN
    CALL DeleteAllGlobalCaseValues();
    CALL DeleteAllNationalCaseValues();
    CALL DeleteAllCompanyCaseValues();
    CALL DeleteAllEmployeeCaseValues();
END;
$$;

-- DeleteAllCompanyCaseValues.pg.sql
-- ----------------------------------------------------------------------
-- =============================================================================
-- DeleteAllCompanyCaseValues
-- =============================================================================

CREATE OR REPLACE PROCEDURE DeleteAllCompanyCaseValues()
LANGUAGE plpgsql
AS $$
BEGIN
    DELETE FROM "CompanyCaseValueChange";
    DELETE FROM "CompanyCaseDocument";
    DELETE FROM "CompanyCaseValue";
    DELETE FROM "CompanyCaseChange";
END;
$$;

-- DeleteAllEmployeeCaseValues.pg.sql
-- ----------------------------------------------------------------------
-- =============================================================================
-- DeleteAllEmployeeCaseValues
-- =============================================================================

CREATE OR REPLACE PROCEDURE DeleteAllEmployeeCaseValues()
LANGUAGE plpgsql
AS $$
BEGIN
    DELETE FROM "EmployeeCaseValueChange";
    DELETE FROM "EmployeeCaseDocument";
    DELETE FROM "EmployeeCaseValue";
    DELETE FROM "EmployeeCaseChange";
END;
$$;

-- DeleteAllGlobalCaseValues.pg.sql
-- ----------------------------------------------------------------------
-- =============================================================================
-- DeleteAllGlobalCaseValues
-- =============================================================================

CREATE OR REPLACE PROCEDURE DeleteAllGlobalCaseValues()
LANGUAGE plpgsql
AS $$
BEGIN
    DELETE FROM "GlobalCaseValueChange";
    DELETE FROM "GlobalCaseDocument";
    DELETE FROM "GlobalCaseValue";
    DELETE FROM "GlobalCaseChange";
END;
$$;

-- DeleteAllNationalCaseValues.pg.sql
-- ----------------------------------------------------------------------
-- =============================================================================
-- DeleteAllNationalCaseValues
-- =============================================================================

CREATE OR REPLACE PROCEDURE DeleteAllNationalCaseValues()
LANGUAGE plpgsql
AS $$
BEGIN
    DELETE FROM "NationalCaseValueChange";
    DELETE FROM "NationalCaseDocument";
    DELETE FROM "NationalCaseValue";
    DELETE FROM "NationalCaseChange";
END;
$$;

-- DeleteEmployee.pg.sql
-- ----------------------------------------------------------------------
-- =============================================================================
-- DeleteEmployee
-- MySQL: DELETE t FROM t INNER JOIN -> PG: DELETE FROM t USING ...
-- =============================================================================

CREATE OR REPLACE PROCEDURE DeleteEmployee(
    IN p_tenantId   INTEGER,
    IN p_employeeId INTEGER
)
LANGUAGE plpgsql
AS $$
BEGIN
    DELETE FROM "PayrunResult" pr
    USING "PayrollResult" prl
    WHERE pr."PayrollResultId" = prl."Id"
      AND prl."TenantId" = p_tenantId AND prl."EmployeeId" = p_employeeId;

    DELETE FROM "WageTypeCustomResult" wtcr
    USING "WageTypeResult" wtr, "PayrollResult" prl
    WHERE wtcr."WageTypeResultId" = wtr."Id"
      AND wtr."PayrollResultId" = prl."Id"
      AND prl."TenantId" = p_tenantId AND prl."EmployeeId" = p_employeeId;

    DELETE FROM "WageTypeResult" wtr
    USING "PayrollResult" prl
    WHERE wtr."PayrollResultId" = prl."Id"
      AND prl."TenantId" = p_tenantId AND prl."EmployeeId" = p_employeeId;

    DELETE FROM "CollectorCustomResult" ccr
    USING "CollectorResult" cr, "PayrollResult" prl
    WHERE ccr."CollectorResultId" = cr."Id"
      AND cr."PayrollResultId" = prl."Id"
      AND prl."TenantId" = p_tenantId AND prl."EmployeeId" = p_employeeId;

    DELETE FROM "CollectorResult" cr
    USING "PayrollResult" prl
    WHERE cr."PayrollResultId" = prl."Id"
      AND prl."TenantId" = p_tenantId AND prl."EmployeeId" = p_employeeId;

    DELETE FROM "PayrollResult" WHERE "TenantId" = p_tenantId AND "EmployeeId" = p_employeeId;

    DELETE FROM "PayrunJobEmployee" pje
    USING "PayrunJob" pj
    WHERE pje."PayrunJobId" = pj."Id"
      AND pj."TenantId" = p_tenantId AND pje."EmployeeId" = p_employeeId;

    DELETE FROM "EmployeeCaseValueChange" ecvc
    USING "EmployeeCaseChange" ecc, "Employee" e
    WHERE ecvc."CaseChangeId" = ecc."Id"
      AND ecc."EmployeeId" = e."Id"
      AND e."TenantId" = p_tenantId AND e."Id" = p_employeeId;

    DELETE FROM "EmployeeCaseChange" ecc
    USING "Employee" e
    WHERE ecc."EmployeeId" = e."Id"
      AND e."TenantId" = p_tenantId AND e."Id" = p_employeeId;

    DELETE FROM "EmployeeCaseDocument" ecd
    USING "EmployeeCaseValue" ecv, "Employee" e
    WHERE ecd."CaseValueId" = ecv."Id"
      AND ecv."EmployeeId" = e."Id"
      AND e."TenantId" = p_tenantId AND e."Id" = p_employeeId;

    DELETE FROM "EmployeeCaseValue" ecv
    USING "Employee" e
    WHERE ecv."EmployeeId" = e."Id"
      AND e."TenantId" = p_tenantId AND e."Id" = p_employeeId;

    DELETE FROM "EmployeeDivision" ed
    USING "Employee" e
    WHERE ed."EmployeeId" = e."Id"
      AND e."TenantId" = p_tenantId AND e."Id" = p_employeeId;

    DELETE FROM "Employee" WHERE "TenantId" = p_tenantId AND "Id" = p_employeeId;
END;
$$;

-- DeleteLookup.pg.sql
-- ----------------------------------------------------------------------
-- =============================================================================
-- DeleteLookup
-- =============================================================================

CREATE OR REPLACE PROCEDURE DeleteLookup(
    IN p_tenantId INTEGER,
    IN p_lookupId INTEGER
)
LANGUAGE plpgsql
AS $$
BEGIN
    DELETE FROM "LookupValueAudit" lva
    USING "LookupValue" lv, "Lookup" lk, "Regulation" r
    WHERE lva."LookupValueId" = lv."Id"
      AND lv."LookupId" = lk."Id"
      AND lk."RegulationId" = r."Id"
      AND r."TenantId" = p_tenantId AND lk."Id" = p_lookupId;

    DELETE FROM "LookupValue" lv
    USING "Lookup" lk, "Regulation" r
    WHERE lv."LookupId" = lk."Id"
      AND lk."RegulationId" = r."Id"
      AND r."TenantId" = p_tenantId AND lk."Id" = p_lookupId;

    DELETE FROM "LookupAudit" la
    USING "Lookup" lk, "Regulation" r
    WHERE la."LookupId" = lk."Id"
      AND lk."RegulationId" = r."Id"
      AND r."TenantId" = p_tenantId AND lk."Id" = p_lookupId;

    DELETE FROM "Lookup" lk
    USING "Regulation" r
    WHERE lk."RegulationId" = r."Id"
      AND r."TenantId" = p_tenantId AND lk."Id" = p_lookupId;
END;
$$;

-- DeletePayrunJob.pg.sql
-- ----------------------------------------------------------------------
-- =============================================================================
-- DeletePayrunJob
-- =============================================================================

CREATE OR REPLACE PROCEDURE DeletePayrunJob(
    IN p_tenantId    INTEGER,
    IN p_payrunJobId INTEGER
)
LANGUAGE plpgsql
AS $$
BEGIN
    DELETE FROM "PayrunResult" pr
    USING "PayrollResult" prl
    WHERE pr."PayrollResultId" = prl."Id"
      AND prl."TenantId" = p_tenantId AND prl."PayrunJobId" = p_payrunJobId;

    DELETE FROM "WageTypeCustomResult" wtcr
    USING "WageTypeResult" wtr, "PayrollResult" prl
    WHERE wtcr."WageTypeResultId" = wtr."Id"
      AND wtr."PayrollResultId" = prl."Id"
      AND prl."TenantId" = p_tenantId AND prl."PayrunJobId" = p_payrunJobId;

    DELETE FROM "WageTypeResult" wtr
    USING "PayrollResult" prl
    WHERE wtr."PayrollResultId" = prl."Id"
      AND prl."TenantId" = p_tenantId AND prl."PayrunJobId" = p_payrunJobId;

    DELETE FROM "CollectorCustomResult" ccr
    USING "CollectorResult" cr, "PayrollResult" prl
    WHERE ccr."CollectorResultId" = cr."Id"
      AND cr."PayrollResultId" = prl."Id"
      AND prl."TenantId" = p_tenantId AND prl."PayrunJobId" = p_payrunJobId;

    DELETE FROM "CollectorResult" cr
    USING "PayrollResult" prl
    WHERE cr."PayrollResultId" = prl."Id"
      AND prl."TenantId" = p_tenantId AND prl."PayrunJobId" = p_payrunJobId;

    DELETE FROM "PayrollResult" WHERE "TenantId" = p_tenantId AND "PayrunJobId" = p_payrunJobId;

    DELETE FROM "PayrunJobEmployee" pje
    USING "PayrunJob" pj
    WHERE pje."PayrunJobId" = pj."Id"
      AND pj."TenantId" = p_tenantId AND pje."PayrunJobId" = p_payrunJobId;

    DELETE FROM "PayrunJob" WHERE "TenantId" = p_tenantId AND "Id" = p_payrunJobId;
END;
$$;

-- DeleteTenant.pg.sql
-- ----------------------------------------------------------------------
-- =============================================================================
-- DeleteTenant
-- PostgreSQL: DELETE FROM t USING ... (all joins in USING clause)
-- =============================================================================

CREATE OR REPLACE PROCEDURE DeleteTenant(
    IN p_tenantId INTEGER
)
LANGUAGE plpgsql
AS $$
BEGIN
    DELETE FROM "PayrunResult" pr
    USING "PayrollResult" prl
    WHERE pr."PayrollResultId" = prl."Id"
      AND prl."TenantId" = p_tenantId;

    DELETE FROM "WageTypeCustomResult" wtcr
    USING "WageTypeResult" wtr, "PayrollResult" prl
    WHERE wtcr."WageTypeResultId" = wtr."Id"
      AND wtr."PayrollResultId" = prl."Id"
      AND prl."TenantId" = p_tenantId;

    DELETE FROM "WageTypeResult" wtr
    USING "PayrollResult" prl
    WHERE wtr."PayrollResultId" = prl."Id"
      AND prl."TenantId" = p_tenantId;

    DELETE FROM "CollectorCustomResult" ccr
    USING "CollectorResult" cr, "PayrollResult" prl
    WHERE ccr."CollectorResultId" = cr."Id"
      AND cr."PayrollResultId" = prl."Id"
      AND prl."TenantId" = p_tenantId;

    DELETE FROM "CollectorResult" cr
    USING "PayrollResult" prl
    WHERE cr."PayrollResultId" = prl."Id"
      AND prl."TenantId" = p_tenantId;

    DELETE FROM "PayrollResult" WHERE "TenantId" = p_tenantId;

    DELETE FROM "PayrunJobEmployee" pje
    USING "PayrunJob" pj
    WHERE pje."PayrunJobId" = pj."Id"
      AND pj."TenantId" = p_tenantId;

    DELETE FROM "PayrunJob" WHERE "TenantId" = p_tenantId;

    DELETE FROM "PayrunParameter" pp
    USING "Payrun" pay
    WHERE pp."PayrunId" = pay."Id"
      AND pay."TenantId" = p_tenantId;

    DELETE FROM "Payrun" WHERE "TenantId" = p_tenantId;

    DELETE FROM "PayrollLayer" pl
    USING "Payroll" pay
    WHERE pl."PayrollId" = pay."Id"
      AND pay."TenantId" = p_tenantId;

    DELETE FROM "Payroll" WHERE "TenantId" = p_tenantId;

    DELETE FROM "RegulationShare" WHERE "ProviderTenantId" = p_tenantId OR "ConsumerTenantId" = p_tenantId;

    DELETE FROM "ReportTemplateAudit" rta
    USING "ReportTemplate" rt, "Report" rp, "Regulation" r
    WHERE rta."ReportTemplateId" = rt."Id"
      AND rt."ReportId" = rp."Id"
      AND rp."RegulationId" = r."Id"
      AND r."TenantId" = p_tenantId;

    DELETE FROM "ReportTemplate" rt
    USING "Report" rp, "Regulation" r
    WHERE rt."ReportId" = rp."Id"
      AND rp."RegulationId" = r."Id"
      AND r."TenantId" = p_tenantId;

    DELETE FROM "ReportParameterAudit" rpa
    USING "ReportParameter" rpar, "Report" rp, "Regulation" r
    WHERE rpa."ReportParameterId" = rpar."Id"
      AND rpar."ReportId" = rp."Id"
      AND rp."RegulationId" = r."Id"
      AND r."TenantId" = p_tenantId;

    DELETE FROM "ReportParameter" rpar
    USING "Report" rp, "Regulation" r
    WHERE rpar."ReportId" = rp."Id"
      AND rp."RegulationId" = r."Id"
      AND r."TenantId" = p_tenantId;

    DELETE FROM "ReportAudit" ra
    USING "Report" rp, "Regulation" r
    WHERE ra."ReportId" = rp."Id"
      AND rp."RegulationId" = r."Id"
      AND r."TenantId" = p_tenantId;

    DELETE FROM "Report" rp
    USING "Regulation" r
    WHERE rp."RegulationId" = r."Id"
      AND r."TenantId" = p_tenantId;

    DELETE FROM "ScriptAudit" sa
    USING "Script" s, "Regulation" r
    WHERE sa."ScriptId" = s."Id"
      AND s."RegulationId" = r."Id"
      AND r."TenantId" = p_tenantId;

    DELETE FROM "Script" s
    USING "Regulation" r
    WHERE s."RegulationId" = r."Id"
      AND r."TenantId" = p_tenantId;

    DELETE FROM "LookupValueAudit" lva
    USING "LookupValue" lv, "Lookup" lk, "Regulation" r
    WHERE lva."LookupValueId" = lv."Id"
      AND lv."LookupId" = lk."Id"
      AND lk."RegulationId" = r."Id"
      AND r."TenantId" = p_tenantId;

    DELETE FROM "LookupValue" lv
    USING "Lookup" lk, "Regulation" r
    WHERE lv."LookupId" = lk."Id"
      AND lk."RegulationId" = r."Id"
      AND r."TenantId" = p_tenantId;

    DELETE FROM "LookupAudit" la
    USING "Lookup" lk, "Regulation" r
    WHERE la."LookupId" = lk."Id"
      AND lk."RegulationId" = r."Id"
      AND r."TenantId" = p_tenantId;

    DELETE FROM "Lookup" lk
    USING "Regulation" r
    WHERE lk."RegulationId" = r."Id"
      AND r."TenantId" = p_tenantId;

    DELETE FROM "CollectorAudit" coa
    USING "Collector" co, "Regulation" r
    WHERE coa."CollectorId" = co."Id"
      AND co."RegulationId" = r."Id"
      AND r."TenantId" = p_tenantId;

    DELETE FROM "Collector" co
    USING "Regulation" r
    WHERE co."RegulationId" = r."Id"
      AND r."TenantId" = p_tenantId;

    DELETE FROM "WageTypeAudit" wta
    USING "WageType" wt, "Regulation" r
    WHERE wta."WageTypeId" = wt."Id"
      AND wt."RegulationId" = r."Id"
      AND r."TenantId" = p_tenantId;

    DELETE FROM "WageType" wt
    USING "Regulation" r
    WHERE wt."RegulationId" = r."Id"
      AND r."TenantId" = p_tenantId;

    DELETE FROM "CaseRelationAudit" cra
    USING "CaseRelation" cr, "Regulation" r
    WHERE cra."CaseRelationId" = cr."Id"
      AND cr."RegulationId" = r."Id"
      AND r."TenantId" = p_tenantId;

    DELETE FROM "CaseRelation" cr
    USING "Regulation" r
    WHERE cr."RegulationId" = r."Id"
      AND r."TenantId" = p_tenantId;

    DELETE FROM "CaseFieldAudit" cfa
    USING "CaseField" cf, "Case" c, "Regulation" r
    WHERE cfa."CaseFieldId" = cf."Id"
      AND cf."CaseId" = c."Id"
      AND c."RegulationId" = r."Id"
      AND r."TenantId" = p_tenantId;

    DELETE FROM "CaseField" cf
    USING "Case" c, "Regulation" r
    WHERE cf."CaseId" = c."Id"
      AND c."RegulationId" = r."Id"
      AND r."TenantId" = p_tenantId;

    DELETE FROM "CaseAudit" ca
    USING "Case" c, "Regulation" r
    WHERE ca."CaseId" = c."Id"
      AND c."RegulationId" = r."Id"
      AND r."TenantId" = p_tenantId;

    DELETE FROM "Case" c
    USING "Regulation" r
    WHERE c."RegulationId" = r."Id"
      AND r."TenantId" = p_tenantId;

    DELETE FROM "Regulation" WHERE "TenantId" = p_tenantId;

    DELETE FROM "EmployeeCaseValueChange" ecvc
    USING "EmployeeCaseChange" ecc, "Employee" e
    WHERE ecvc."CaseChangeId" = ecc."Id"
      AND ecc."EmployeeId" = e."Id"
      AND e."TenantId" = p_tenantId;

    DELETE FROM "EmployeeCaseChange" ecc
    USING "Employee" e
    WHERE ecc."EmployeeId" = e."Id"
      AND e."TenantId" = p_tenantId;

    DELETE FROM "EmployeeCaseDocument" ecd
    USING "EmployeeCaseValue" ecv, "Employee" e
    WHERE ecd."CaseValueId" = ecv."Id"
      AND ecv."EmployeeId" = e."Id"
      AND e."TenantId" = p_tenantId;

    DELETE FROM "EmployeeCaseValue" ecv
    USING "Employee" e
    WHERE ecv."EmployeeId" = e."Id"
      AND e."TenantId" = p_tenantId;

    DELETE FROM "EmployeeDivision" ed
    USING "Employee" e
    WHERE ed."EmployeeId" = e."Id"
      AND e."TenantId" = p_tenantId;

    DELETE FROM "Employee" WHERE "TenantId" = p_tenantId;

    DELETE FROM "CompanyCaseValueChange" ccvc
    USING "CompanyCaseChange" ccc
    WHERE ccvc."CaseChangeId" = ccc."Id"
      AND ccc."TenantId" = p_tenantId;

    DELETE FROM "CompanyCaseChange" WHERE "TenantId" = p_tenantId;

    DELETE FROM "CompanyCaseDocument" ccd
    USING "CompanyCaseValue" ccv
    WHERE ccd."CaseValueId" = ccv."Id"
      AND ccv."TenantId" = p_tenantId;

    DELETE FROM "CompanyCaseValue" WHERE "TenantId" = p_tenantId;

    DELETE FROM "NationalCaseValueChange" ncvc
    USING "NationalCaseChange" ncc
    WHERE ncvc."CaseChangeId" = ncc."Id"
      AND ncc."TenantId" = p_tenantId;

    DELETE FROM "NationalCaseChange" WHERE "TenantId" = p_tenantId;

    DELETE FROM "NationalCaseDocument" ncd
    USING "NationalCaseValue" ncv
    WHERE ncd."CaseValueId" = ncv."Id"
      AND ncv."TenantId" = p_tenantId;

    DELETE FROM "NationalCaseValue" WHERE "TenantId" = p_tenantId;

    DELETE FROM "GlobalCaseValueChange" gcvc
    USING "GlobalCaseChange" gcc
    WHERE gcvc."CaseChangeId" = gcc."Id"
      AND gcc."TenantId" = p_tenantId;

    DELETE FROM "GlobalCaseChange" WHERE "TenantId" = p_tenantId;

    DELETE FROM "GlobalCaseDocument" gcd
    USING "GlobalCaseValue" gcv
    WHERE gcd."CaseValueId" = gcv."Id"
      AND gcv."TenantId" = p_tenantId;

    DELETE FROM "GlobalCaseValue" WHERE "TenantId" = p_tenantId;

    DELETE FROM "WebhookMessage" wm
    USING "Webhook" wh
    WHERE wm."WebhookId" = wh."Id"
      AND wh."TenantId" = p_tenantId;

    DELETE FROM "Webhook" WHERE "TenantId" = p_tenantId;
    DELETE FROM "Task" WHERE "TenantId" = p_tenantId;
    DELETE FROM "Log" WHERE "TenantId" = p_tenantId;
    DELETE FROM "ReportLog" WHERE "TenantId" = p_tenantId;
    DELETE FROM "User" WHERE "TenantId" = p_tenantId;
    DELETE FROM "Division" WHERE "TenantId" = p_tenantId;
    DELETE FROM "Calendar" WHERE "TenantId" = p_tenantId;
    DELETE FROM "Tenant" WHERE "Id" = p_tenantId;
END;
$$;

-- GetCollectorCustomResults.pg.sql
-- ----------------------------------------------------------------------
-- =============================================================================
-- GetCollectorCustomResults
-- =============================================================================

CREATE OR REPLACE FUNCTION "GetCollectorCustomResults"(
    IN "tenantId"            INT,
    IN "employeeId"          INT,
    IN "divisionId"          INT,
    IN "payrunJobId"         INT,
    IN "parentPayrunJobId"   INT,
    IN "collectorNameHashes" TEXT,
    IN "periodStart"         TIMESTAMP(6),
    IN "periodEnd"           TIMESTAMP(6),
    IN "jobStatus"           INT,
    IN "forecast"            TEXT,
    IN "evaluationDate"      TIMESTAMP(6)
)
RETURNS TABLE (
    "Id"                          INT,
    "Status"                      INT,
    "Created"                     TIMESTAMP(6),
    "Updated"                     TIMESTAMP(6),
    "CollectorResultId"           INT,
    "TenantId"                    INT,
    "EmployeeId"                  INT,
    "DivisionId"                  INT,
    "CollectorName"               VARCHAR(128),
    "CollectorNameHash"           INT,
    "CollectorNameLocalizations"  TEXT,
    "Source"                      VARCHAR(128),
    "ValueType"                   INT,
    "Value"                       DECIMAL(28,6),
    "Culture"                     VARCHAR(128),
    "Start"                       TIMESTAMP(6),
    "StartHash"                   INT,
    "End"                         TIMESTAMP(6),
    "PayrunJobId"                 INT,
    "Forecast"                    VARCHAR(128),
    "ParentJobId"                 INT,
    "Tags"                        TEXT,
    "Attributes"                  TEXT
)
LANGUAGE plpgsql STABLE
AS $$
DECLARE
    v_collectorNameHash INT;
    v_collectorCount    INT;
BEGIN
    v_collectorCount := CASE WHEN "collectorNameHashes" IS NULL THEN 0
                             ELSE jsonb_array_length("collectorNameHashes"::jsonb) END;

    IF v_collectorCount = 1 THEN
        SELECT CAST(jt.val AS INT) INTO v_collectorNameHash
        FROM jsonb_array_elements_text("collectorNameHashes"::jsonb) AS jt(val)
        LIMIT 1;
    END IF;

    RETURN QUERY
    SELECT "ccr".*
    FROM "CollectorCustomResult" "ccr"
    WHERE "ccr"."TenantId" = "tenantId"
      AND "ccr"."EmployeeId" = "employeeId"
      AND ("divisionId" IS NULL        OR "ccr"."DivisionId" = "divisionId")
      AND ("payrunJobId" IS NULL       OR "ccr"."PayrunJobId" = "payrunJobId")
      AND ("parentPayrunJobId" IS NULL OR "ccr"."ParentJobId" = "parentPayrunJobId")
      AND ("collectorNameHashes" IS NULL OR v_collectorCount = 0
           OR (v_collectorCount = 1 AND "ccr"."CollectorNameHash" = v_collectorNameHash)
           OR (v_collectorCount > 1 AND "ccr"."CollectorNameHash" IN (
               SELECT CAST(jt.val AS INT)
               FROM jsonb_array_elements_text("collectorNameHashes"::jsonb) AS jt(val))))
      AND ("periodStart" IS NULL OR "ccr"."Start" BETWEEN "periodStart" AND "periodEnd")
      AND ("jobStatus" IS NULL OR "ccr"."PayrunJobId" IN (
               SELECT "pj"."Id" FROM "PayrunJob" "pj"
               WHERE "pj"."Id" = "ccr"."PayrunJobId"
                 AND "pj"."JobStatus" = "jobStatus"))
      AND ("ccr"."Forecast" IS NULL OR "ccr"."Forecast" = "forecast")
      AND ("evaluationDate" IS NULL OR "ccr"."Created" <= "evaluationDate")
    ORDER BY "ccr"."Created";
END;
$$;

-- GetCollectorResults.pg.sql
-- ----------------------------------------------------------------------
-- =============================================================================
-- GetCollectorResults
-- =============================================================================

CREATE OR REPLACE FUNCTION "GetCollectorResults"(
    IN "tenantId"            INT,
    IN "employeeId"          INT,
    IN "divisionId"          INT,
    IN "payrunJobId"         INT,
    IN "parentPayrunJobId"   INT,
    IN "collectorNameHashes" TEXT,
    IN "periodStart"         TIMESTAMP(6),
    IN "periodEnd"           TIMESTAMP(6),
    IN "jobStatus"           INT,
    IN "forecast"            TEXT,
    IN "evaluationDate"      TIMESTAMP(6)
)
RETURNS TABLE (
    "Id"                          INT,
    "Status"                      INT,
    "Created"                     TIMESTAMP(6),
    "Updated"                     TIMESTAMP(6),
    "PayrollResultId"             INT,
    "TenantId"                    INT,
    "EmployeeId"                  INT,
    "DivisionId"                  INT,
    "CollectorId"                 INT,
    "CollectorName"               VARCHAR(128),
    "CollectorNameHash"           INT,
    "CollectorNameLocalizations"  TEXT,
    "CollectMode"                 INT,
    "Negated"                     BOOLEAN,
    "ValueType"                   INT,
    "Value"                       DECIMAL(28,6),
    "Culture"                     VARCHAR(128),
    "Start"                       TIMESTAMP(6),
    "StartHash"                   INT,
    "End"                         TIMESTAMP(6),
    "PayrunJobId"                 INT,
    "Forecast"                    VARCHAR(128),
    "ParentJobId"                 INT,
    "Tags"                        TEXT,
    "Attributes"                  TEXT
)
LANGUAGE plpgsql STABLE
AS $$
DECLARE
    v_collectorNameHash INT;
    v_collectorCount    INT;
BEGIN
    v_collectorCount := CASE WHEN "collectorNameHashes" IS NULL THEN 0
                             ELSE jsonb_array_length("collectorNameHashes"::jsonb) END;

    IF v_collectorCount = 1 THEN
        SELECT CAST(jt.val AS INT) INTO v_collectorNameHash
        FROM jsonb_array_elements_text("collectorNameHashes"::jsonb) AS jt(val)
        LIMIT 1;
    END IF;

    RETURN QUERY
    SELECT "cr".*
    FROM "CollectorResult" "cr"
    WHERE "cr"."TenantId" = "tenantId"
      AND "cr"."EmployeeId" = "employeeId"
      AND ("divisionId" IS NULL        OR "cr"."DivisionId" = "divisionId")
      AND ("payrunJobId" IS NULL       OR "cr"."PayrunJobId" = "payrunJobId")
      AND ("parentPayrunJobId" IS NULL OR "cr"."ParentJobId" = "parentPayrunJobId")
      AND ("collectorNameHashes" IS NULL OR v_collectorCount = 0
           OR (v_collectorCount = 1 AND "cr"."CollectorNameHash" = v_collectorNameHash)
           OR (v_collectorCount > 1 AND "cr"."CollectorNameHash" IN (
               SELECT CAST(jt.val AS INT)
               FROM jsonb_array_elements_text("collectorNameHashes"::jsonb) AS jt(val))))
      AND ("periodStart" IS NULL OR "cr"."Start" BETWEEN "periodStart" AND "periodEnd")
      AND ("jobStatus" IS NULL OR "cr"."PayrunJobId" IN (
               SELECT "pj"."Id" FROM "PayrunJob" "pj"
               WHERE "pj"."Id" = "cr"."PayrunJobId"
                 AND "pj"."JobStatus" = "jobStatus"))
      AND ("cr"."Forecast" IS NULL OR "cr"."Forecast" = "forecast")
      AND ("evaluationDate" IS NULL OR "cr"."Created" <= "evaluationDate")
    ORDER BY "cr"."Created";
END;
$$;

-- GetCompanyCaseChangeValues.pg.sql
-- ----------------------------------------------------------------------
-- =============================================================================
-- GetCompanyCaseChangeValues
-- Creates temp pivot table joining CaseChange + CaseValue + User, with
-- optional culture-based name localization and attribute columns.
-- Count queries (sql ILIKE '%COUNT(*)%') are routed via EXECUTE INTO to
-- return the count value in the first column ("TenantId") for Dapper long mapping.
-- =============================================================================

DROP PROCEDURE IF EXISTS GetCompanyCaseChangeValues;

CREATE OR REPLACE FUNCTION "GetCompanyCaseChangeValues"(
    IN "parentId"   INTEGER,
    IN "employeeId" INTEGER,
    IN "divisionId" INTEGER,
    IN "sql"        TEXT,
    IN "attributes" TEXT,
    IN "culture"    TEXT
)
RETURNS TABLE(
    "TenantId"           INTEGER,
    "CaseChangeId"       INTEGER,
    "CaseChangeCreated"  TIMESTAMP(6),
    "Reason"             TEXT,
    "ValidationCaseName" VARCHAR(128),
    "CancellationType"   INTEGER,
    "CancellationId"     INTEGER,
    "CancellationDate"   TIMESTAMP(6),
    "EmployeeId"         INTEGER,
    "UserId"             INTEGER,
    "UserIdentifier"     VARCHAR(128),
    "DivisionId"         INTEGER,
    "Id"                 INTEGER,
    "Created"            TIMESTAMP(6),
    "Updated"            TIMESTAMP(6),
    "Status"             INTEGER,
    "CaseName"           VARCHAR(128),
    "CaseFieldName"      VARCHAR(128),
    "CaseSlot"           VARCHAR(128),
    "CaseRelation"       TEXT,
    "ValueType"          INTEGER,
    "Value"              TEXT,
    "NumericValue"       DECIMAL(28,6),
    "Culture"            VARCHAR(128),
    "Start"              TIMESTAMP(6),
    "End"                TIMESTAMP(6),
    "Forecast"           VARCHAR(128),
    "Tags"               TEXT,
    "Attributes"         TEXT,
    "Documents"          BIGINT
)
LANGUAGE plpgsql VOLATILE AS $$
DECLARE
    v_attrSql       TEXT;
    v_pivotSql      TEXT;
    v_caseName      TEXT;
    v_caseFieldName TEXT;
    v_caseSlot      TEXT;
    v_count         BIGINT;
BEGIN
    IF "culture" IS NULL THEN
        v_caseName      := '"CompanyCaseValue"."CaseName"';
        v_caseFieldName := '"CompanyCaseValue"."CaseFieldName"';
        v_caseSlot      := '"CompanyCaseValue"."CaseSlot"';
    ELSE
        v_caseName      := 'GetLocalizedValue("CompanyCaseValue"."CaseNameLocalizations", ''' || "culture" || ''', "CompanyCaseValue"."CaseName")';
        v_caseFieldName := 'GetLocalizedValue("CompanyCaseValue"."CaseFieldNameLocalizations", ''' || "culture" || ''', "CompanyCaseValue"."CaseFieldName")';
        v_caseSlot      := 'GetLocalizedValue("CompanyCaseValue"."CaseSlotLocalizations", ''' || "culture" || ''', "CompanyCaseValue"."CaseSlot")';
    END IF;

    v_attrSql  := BuildAttributeQuery('"CompanyCaseValue"."Attributes"', "attributes");
    v_pivotSql := 'CREATE TEMP TABLE "##CompanyCaseChangeValuePivot" AS SELECT'
        || ' "CompanyCaseChange"."TenantId",'
        || ' "CompanyCaseChange"."Id" AS "CaseChangeId",'
        || ' "CompanyCaseChange"."Created" AS "CaseChangeCreated",'
        || ' "CompanyCaseChange"."Reason",'
        || ' "CompanyCaseChange"."ValidationCaseName",'
        || ' "CompanyCaseChange"."CancellationType",'
        || ' "CompanyCaseChange"."CancellationId",'
        || ' "CompanyCaseChange"."CancellationDate",'
        || ' NULL AS "EmployeeId",'
        || ' "CompanyCaseChange"."UserId",'
        || ' "User"."Identifier" AS "UserIdentifier",'
        || ' "CompanyCaseChange"."DivisionId",'
        || ' "CompanyCaseValue"."Id",'
        || ' "CompanyCaseValue"."Created",'
        || ' "CompanyCaseValue"."Updated",'
        || ' "CompanyCaseValue"."Status",'
        || ' ' || v_caseName      || ' AS "CaseName",'
        || ' ' || v_caseFieldName || ' AS "CaseFieldName",'
        || ' ' || v_caseSlot      || ' AS "CaseSlot",'
        || ' "CompanyCaseValue"."CaseRelation",'
        || ' "CompanyCaseValue"."ValueType",'
        || ' "CompanyCaseValue"."Value",'
        || ' "CompanyCaseValue"."NumericValue",'
        || ' "CompanyCaseValue"."Culture",'
        || ' "CompanyCaseValue"."Start",'
        || ' "CompanyCaseValue"."End",'
        || ' "CompanyCaseValue"."Forecast",'
        || ' "CompanyCaseValue"."Tags",'
        || ' "CompanyCaseValue"."Attributes",'
        || ' (SELECT COUNT(*) FROM "CompanyCaseDocument" WHERE "CaseValueId" = "CompanyCaseValue"."Id") AS "Documents"'
        || v_attrSql
        || ' FROM "CompanyCaseValue"'
        || ' LEFT JOIN "CompanyCaseValueChange" ON "CompanyCaseValue"."Id" = "CompanyCaseValueChange"."CaseValueId"'
        || ' LEFT JOIN "CompanyCaseChange" ON "CompanyCaseValueChange"."CaseChangeId" = "CompanyCaseChange"."Id"'
        || ' LEFT JOIN "User" ON "User"."Id" = "CompanyCaseChange"."UserId"'
        || ' WHERE "CompanyCaseChange"."TenantId" = ' || "parentId"::TEXT;

    DROP TABLE IF EXISTS "##CompanyCaseChangeValuePivot";
    EXECUTE v_pivotSql;

    IF "sql" ILIKE '%COUNT(*)%' OR "sql" ILIKE '%COUNT(0)%' THEN
        EXECUTE "sql" INTO v_count;
        "TenantId" := v_count::INTEGER;
        RETURN NEXT;
    ELSE
        RETURN QUERY EXECUTE "sql";
    END IF;

    DROP TABLE IF EXISTS "##CompanyCaseChangeValuePivot";
EXCEPTION WHEN OTHERS THEN
    DROP TABLE IF EXISTS "##CompanyCaseChangeValuePivot";
    RAISE;
END;
$$;

-- GetCompanyCaseValues.pg.sql
-- ----------------------------------------------------------------------
-- =============================================================================
-- GetCompanyCaseValues
-- Creates temp pivot table with optional attribute columns, then executes
-- the caller-supplied SQL against it.
-- Count queries (sql ILIKE '%COUNT(*)%') are routed via EXECUTE INTO to
-- return the count value in the first column ("Id") for Dapper long mapping.
-- =============================================================================

DROP PROCEDURE IF EXISTS GetCompanyCaseValues;

CREATE OR REPLACE FUNCTION "GetCompanyCaseValues"(
    IN "parentId"   INTEGER,
    IN "employeeId" INTEGER,
    IN "divisionId" INTEGER,
    IN "sql"        TEXT,
    IN "attributes" TEXT,
    IN "culture"    TEXT
)
RETURNS TABLE(
    "Id"                         INTEGER,
    "Status"                     INTEGER,
    "Created"                    TIMESTAMP(6),
    "Updated"                    TIMESTAMP(6),
    "TenantId"                   INTEGER,
    "DivisionId"                 INTEGER,
    "CaseName"                   VARCHAR(128),
    "CaseNameLocalizations"      TEXT,
    "CaseFieldName"              VARCHAR(128),
    "CaseFieldNameLocalizations" TEXT,
    "CaseSlot"                   VARCHAR(128),
    "CaseSlotLocalizations"      TEXT,
    "ValueType"                  INTEGER,
    "Value"                      TEXT,
    "NumericValue"               DECIMAL(28,6),
    "Culture"                    VARCHAR(128),
    "CaseRelation"               TEXT,
    "CancellationDate"           TIMESTAMP(6),
    "Start"                      TIMESTAMP(6),
    "End"                        TIMESTAMP(6),
    "Forecast"                   VARCHAR(128),
    "Tags"                       TEXT,
    "Attributes"                 TEXT
)
LANGUAGE plpgsql VOLATILE AS $$
DECLARE
    v_attrSql  TEXT;
    v_pivotSql TEXT;
    v_count    BIGINT;
BEGIN
    v_attrSql  := BuildAttributeQuery('"CompanyCaseValue"."Attributes"', "attributes");
    v_pivotSql := 'CREATE TEMP TABLE "##CompanyCaseValuePivot" AS'
        || ' SELECT "CompanyCaseValue".*'
        || v_attrSql
        || ' FROM "CompanyCaseValue"'
        || ' WHERE "CompanyCaseValue"."TenantId" = ' || "parentId"::TEXT;

    DROP TABLE IF EXISTS "##CompanyCaseValuePivot";
    EXECUTE v_pivotSql;

    IF "sql" ILIKE '%COUNT(*)%' OR "sql" ILIKE '%COUNT(0)%' THEN
        EXECUTE "sql" INTO v_count;
        "Id" := v_count::INTEGER;
        RETURN NEXT;
    ELSE
        RETURN QUERY EXECUTE "sql";
    END IF;

    DROP TABLE IF EXISTS "##CompanyCaseValuePivot";
EXCEPTION WHEN OTHERS THEN
    DROP TABLE IF EXISTS "##CompanyCaseValuePivot";
    RAISE;
END;
$$;

-- GetConsolidatedCollectorCustomResults.pg.sql
-- ----------------------------------------------------------------------
-- =============================================================================
-- GetConsolidatedCollectorCustomResults
-- =============================================================================

DROP PROCEDURE IF EXISTS GetConsolidatedCollectorCustomResults;

CREATE OR REPLACE FUNCTION "GetConsolidatedCollectorCustomResults"(
    IN "tenantId"            INTEGER,
    IN "employeeId"          INTEGER,
    IN "divisionId"          INTEGER,
    IN "collectorNameHashes" TEXT,
    IN "periodStartHashes"   TEXT,
    IN "jobStatus"           INTEGER,
    IN "forecast"            TEXT,
    IN "evaluationDate"      TIMESTAMP(6),
    IN "noRetro"             BOOLEAN,
    IN "excludeParentJobId"  INTEGER
)
RETURNS TABLE(
    "Id"                          INT,
    "Status"                      INT,
    "Created"                     TIMESTAMP(6),
    "Updated"                     TIMESTAMP(6),
    "CollectorResultId"           INT,
    "TenantId"                    INT,
    "EmployeeId"                  INT,
    "DivisionId"                  INT,
    "CollectorName"               VARCHAR(128),
    "CollectorNameHash"           INT,
    "CollectorNameLocalizations"  TEXT,
    "Source"                      VARCHAR(128),
    "ValueType"                   INT,
    "Value"                       DECIMAL(28,6),
    "Culture"                     VARCHAR(128),
    "Start"                       TIMESTAMP(6),
    "StartHash"                   INT,
    "End"                         TIMESTAMP(6),
    "PayrunJobId"                 INT,
    "Forecast"                    VARCHAR(128),
    "ParentJobId"                 INT,
    "Tags"                        TEXT,
    "Attributes"                  TEXT
)
LANGUAGE plpgsql STABLE
AS $$
BEGIN
    IF "periodStartHashes" IS NULL THEN
        RETURN;
    END IF;
    RETURN QUERY
    WITH "Winners" AS (
        SELECT r."Id",
            ROW_NUMBER() OVER (
                PARTITION BY r."CollectorNameHash", r."Start"
                ORDER BY r."Created" DESC, r."Id" DESC
            ) AS "RowNumber"
        FROM "CollectorCustomResult" r
        WHERE r."TenantId" = "tenantId"
          AND r."EmployeeId" = "employeeId"
          AND r."StartHash" = ANY(ARRAY(
                SELECT (v::bigint)::int
                FROM jsonb_array_elements_text("periodStartHashes"::jsonb) v))
          AND ("divisionId" IS NULL OR r."DivisionId" = "divisionId")
          AND ("collectorNameHashes" IS NULL OR r."CollectorNameHash" = ANY(
                ARRAY(SELECT CAST(v AS INTEGER)
                      FROM jsonb_array_elements_text("collectorNameHashes"::jsonb) v)))
          AND ("evaluationDate" IS NULL OR r."Created" <= "evaluationDate")
          AND ("jobStatus" IS NULL OR r."PayrunJobId" IN (
                SELECT pj."Id" FROM "PayrunJob" pj
                WHERE pj."JobStatus" = "jobStatus"))
          AND (r."Forecast" IS NULL OR r."Forecast" = "forecast")
          AND (COALESCE("noRetro", FALSE) = FALSE OR r."ParentJobId" IS NULL)
          AND ("excludeParentJobId" IS NULL OR r."ParentJobId" IS NULL
               OR r."ParentJobId" <> "excludeParentJobId")
    )
    SELECT r.*
    FROM "CollectorCustomResult" r
    INNER JOIN "Winners" w ON w."Id" = r."Id"
    WHERE w."RowNumber" = 1;
END;
$$;

-- GetConsolidatedCollectorResults.pg.sql
-- ----------------------------------------------------------------------
-- =============================================================================
-- GetConsolidatedCollectorResults
-- =============================================================================

DROP PROCEDURE IF EXISTS GetConsolidatedCollectorResults;

CREATE OR REPLACE FUNCTION "GetConsolidatedCollectorResults"(
    IN "tenantId"            INTEGER,
    IN "employeeId"          INTEGER,
    IN "divisionId"          INTEGER,
    IN "collectorNameHashes" TEXT,
    IN "periodStartHashes"   TEXT,
    IN "jobStatus"           INTEGER,
    IN "forecast"            TEXT,
    IN "evaluationDate"      TIMESTAMP(6),
    IN "noRetro"             BOOLEAN,
    IN "excludeParentJobId"  INTEGER
)
RETURNS TABLE(
    "Id"                          INT,
    "Status"                      INT,
    "Created"                     TIMESTAMP(6),
    "Updated"                     TIMESTAMP(6),
    "PayrollResultId"             INT,
    "TenantId"                    INT,
    "EmployeeId"                  INT,
    "DivisionId"                  INT,
    "CollectorId"                 INT,
    "CollectorName"               VARCHAR(128),
    "CollectorNameHash"           INT,
    "CollectorNameLocalizations"  TEXT,
    "CollectMode"                 INT,
    "Negated"                     BOOLEAN,
    "ValueType"                   INT,
    "Value"                       DECIMAL(28,6),
    "Culture"                     VARCHAR(128),
    "Start"                       TIMESTAMP(6),
    "StartHash"                   INT,
    "End"                         TIMESTAMP(6),
    "PayrunJobId"                 INT,
    "Forecast"                    VARCHAR(128),
    "ParentJobId"                 INT,
    "Tags"                        TEXT,
    "Attributes"                  TEXT
)
LANGUAGE plpgsql STABLE
AS $$
BEGIN
    IF "periodStartHashes" IS NULL THEN
        RETURN;
    END IF;
    RETURN QUERY
    WITH "Winners" AS (
        SELECT r."Id",
            ROW_NUMBER() OVER (
                PARTITION BY r."CollectorNameHash", r."Start"
                ORDER BY r."Created" DESC, r."Id" DESC
            ) AS "RowNumber"
        FROM "CollectorResult" r
        WHERE r."TenantId" = "tenantId"
          AND r."EmployeeId" = "employeeId"
          AND r."StartHash" = ANY(ARRAY(
                SELECT (v::bigint)::int
                FROM jsonb_array_elements_text("periodStartHashes"::jsonb) v))
          AND ("divisionId" IS NULL OR r."DivisionId" = "divisionId")
          AND ("collectorNameHashes" IS NULL OR r."CollectorNameHash" = ANY(
                ARRAY(SELECT CAST(v AS INTEGER)
                      FROM jsonb_array_elements_text("collectorNameHashes"::jsonb) v)))
          AND ("evaluationDate" IS NULL OR r."Created" <= "evaluationDate")
          AND ("jobStatus" IS NULL OR r."PayrunJobId" IN (
                SELECT pj."Id" FROM "PayrunJob" pj
                WHERE pj."JobStatus" = "jobStatus"))
          AND (r."Forecast" IS NULL OR r."Forecast" = "forecast")
          AND (COALESCE("noRetro", FALSE) = FALSE OR r."ParentJobId" IS NULL)
          AND ("excludeParentJobId" IS NULL OR r."ParentJobId" IS NULL
               OR r."ParentJobId" <> "excludeParentJobId")
    )
    SELECT r.*
    FROM "CollectorResult" r
    INNER JOIN "Winners" w ON w."Id" = r."Id"
    WHERE w."RowNumber" = 1;
END;
$$;

-- GetConsolidatedPayrunResults.pg.sql
-- ----------------------------------------------------------------------
-- =============================================================================
-- GetConsolidatedPayrunResults
-- =============================================================================

DROP PROCEDURE IF EXISTS GetConsolidatedPayrunResults;

CREATE OR REPLACE FUNCTION "GetConsolidatedPayrunResults"(
    IN "tenantId"            INTEGER,
    IN "employeeId"          INTEGER,
    IN "divisionId"          INTEGER,
    IN "names"               TEXT,
    IN "periodStartHashes"   TEXT,
    IN "jobStatus"           INTEGER,
    IN "forecast"            TEXT,
    IN "evaluationDate"      TIMESTAMP(6),
    IN "noRetro"             BOOLEAN,
    IN "excludeParentJobId"  INTEGER
)
RETURNS TABLE(
    "Id"                  INT,
    "Status"              INT,
    "Created"             TIMESTAMP(6),
    "Updated"             TIMESTAMP(6),
    "PayrollResultId"     INT,
    "TenantId"            INT,
    "EmployeeId"          INT,
    "DivisionId"          INT,
    "Source"              VARCHAR(128),
    "Name"                VARCHAR(128),
    "NameLocalizations"   TEXT,
    "Slot"                VARCHAR(128),
    "ValueType"           INT,
    "Value"               TEXT,
    "NumericValue"        DECIMAL(28,6),
    "Culture"             VARCHAR(128),
    "Start"               TIMESTAMP(6),
    "StartHash"           INT,
    "End"                 TIMESTAMP(6),
    "PayrunJobId"         INT,
    "Forecast"            VARCHAR(128),
    "ParentJobId"         INT,
    "Tags"                TEXT,
    "Attributes"          TEXT
)
LANGUAGE plpgsql STABLE
AS $$
BEGIN
    IF "periodStartHashes" IS NULL THEN
        RETURN;
    END IF;
    RETURN QUERY
    WITH "Winners" AS (
        SELECT r."Id",
            ROW_NUMBER() OVER (
                PARTITION BY r."Name", r."Start"
                ORDER BY r."Created" DESC, r."Id" DESC
            ) AS "RowNumber"
        FROM "PayrunResult" r
        WHERE r."TenantId" = "tenantId"
          AND r."EmployeeId" = "employeeId"
          AND r."StartHash" = ANY(ARRAY(
                SELECT (v::bigint)::int
                FROM jsonb_array_elements_text("periodStartHashes"::jsonb) v))
          AND ("divisionId" IS NULL OR r."DivisionId" = "divisionId")
          AND ("names" IS NULL OR r."Name" = ANY(
                ARRAY(SELECT v
                      FROM jsonb_array_elements_text("names"::jsonb) v)))
          AND ("evaluationDate" IS NULL OR r."Created" <= "evaluationDate")
          AND ("jobStatus" IS NULL OR r."PayrunJobId" IN (
                SELECT pj."Id" FROM "PayrunJob" pj
                WHERE pj."JobStatus" = "jobStatus"))
          AND (r."Forecast" IS NULL OR r."Forecast" = "forecast")
          AND (COALESCE("noRetro", FALSE) = FALSE OR r."ParentJobId" IS NULL)
          AND ("excludeParentJobId" IS NULL OR r."ParentJobId" IS NULL
               OR r."ParentJobId" <> "excludeParentJobId")
    )
    SELECT r.*
    FROM "PayrunResult" r
    INNER JOIN "Winners" w ON w."Id" = r."Id"
    WHERE w."RowNumber" = 1;
END;
$$;

-- GetConsolidatedWageTypeCustomResults.pg.sql
-- ----------------------------------------------------------------------
-- =============================================================================
-- GetConsolidatedWageTypeCustomResults
-- =============================================================================

DROP PROCEDURE IF EXISTS GetConsolidatedWageTypeCustomResults;

CREATE OR REPLACE FUNCTION "GetConsolidatedWageTypeCustomResults"(
    IN "tenantId"            INTEGER,
    IN "employeeId"          INTEGER,
    IN "divisionId"          INTEGER,
    IN "wageTypeNumbers"     TEXT,
    IN "periodStartHashes"   TEXT,
    IN "jobStatus"           INTEGER,
    IN "forecast"            TEXT,
    IN "evaluationDate"      TIMESTAMP(6),
    IN "noRetro"             BOOLEAN,
    IN "excludeParentJobId"  INTEGER
)
RETURNS TABLE(
    "Id"                          INT,
    "Status"                      INT,
    "Created"                     TIMESTAMP(6),
    "Updated"                     TIMESTAMP(6),
    "WageTypeResultId"            INT,
    "TenantId"                    INT,
    "EmployeeId"                  INT,
    "DivisionId"                  INT,
    "WageTypeNumber"              DECIMAL(28,6),
    "WageTypeName"                VARCHAR(128),
    "WageTypeNameLocalizations"   TEXT,
    "Source"                      VARCHAR(128),
    "ValueType"                   INT,
    "Value"                       DECIMAL(28,6),
    "Culture"                     VARCHAR(128),
    "Start"                       TIMESTAMP(6),
    "StartHash"                   INT,
    "End"                         TIMESTAMP(6),
    "PayrunJobId"                 INT,
    "Forecast"                    VARCHAR(128),
    "ParentJobId"                 INT,
    "Tags"                        TEXT,
    "Attributes"                  TEXT
)
LANGUAGE plpgsql STABLE
AS $$
BEGIN
    IF "periodStartHashes" IS NULL THEN
        RETURN;
    END IF;
    RETURN QUERY
    WITH "Winners" AS (
        SELECT r."Id",
            ROW_NUMBER() OVER (
                PARTITION BY r."WageTypeNumber", r."Start"
                ORDER BY r."Created" DESC, r."Id" DESC
            ) AS "RowNumber"
        FROM "WageTypeCustomResult" r
        WHERE r."TenantId" = "tenantId"
          AND r."EmployeeId" = "employeeId"
          AND r."StartHash" = ANY(ARRAY(
                SELECT (v::bigint)::int
                FROM jsonb_array_elements_text("periodStartHashes"::jsonb) v))
          AND ("divisionId" IS NULL OR r."DivisionId" = "divisionId")
          AND ("wageTypeNumbers" IS NULL OR r."WageTypeNumber" = ANY(
                ARRAY(SELECT CAST(v AS DECIMAL(28,6))
                      FROM jsonb_array_elements_text("wageTypeNumbers"::jsonb) v)))
          AND ("evaluationDate" IS NULL OR r."Created" <= "evaluationDate")
          AND ("jobStatus" IS NULL OR r."PayrunJobId" IN (
                SELECT pj."Id" FROM "PayrunJob" pj
                WHERE pj."JobStatus" = "jobStatus"))
          AND (r."Forecast" IS NULL OR r."Forecast" = "forecast")
          AND (COALESCE("noRetro", FALSE) = FALSE OR r."ParentJobId" IS NULL)
          AND ("excludeParentJobId" IS NULL OR r."ParentJobId" IS NULL
               OR r."ParentJobId" <> "excludeParentJobId")
    )
    SELECT r.*
    FROM "WageTypeCustomResult" r
    INNER JOIN "Winners" w ON w."Id" = r."Id"
    WHERE w."RowNumber" = 1;
END;
$$;

-- GetConsolidatedWageTypeResults.pg.sql
-- ----------------------------------------------------------------------
-- =============================================================================
-- GetConsolidatedWageTypeResults
-- =============================================================================

DROP PROCEDURE IF EXISTS GetConsolidatedWageTypeResults;

CREATE OR REPLACE FUNCTION "GetConsolidatedWageTypeResults"(
    IN "tenantId"            INTEGER,
    IN "employeeId"          INTEGER,
    IN "divisionId"          INTEGER,
    IN "wageTypeNumbers"     TEXT,
    IN "periodStartHashes"   TEXT,
    IN "jobStatus"           INTEGER,
    IN "forecast"            TEXT,
    IN "evaluationDate"      TIMESTAMP(6),
    IN "noRetro"             BOOLEAN,
    IN "excludeParentJobId"  INTEGER
)
RETURNS TABLE(
    "Id"                          INT,
    "Status"                      INT,
    "Created"                     TIMESTAMP(6),
    "Updated"                     TIMESTAMP(6),
    "PayrollResultId"             INT,
    "TenantId"                    INT,
    "EmployeeId"                  INT,
    "DivisionId"                  INT,
    "WageTypeId"                  INT,
    "WageTypeNumber"              DECIMAL(28,6),
    "WageTypeName"                VARCHAR(128),
    "WageTypeNameLocalizations"   TEXT,
    "ValueType"                   INT,
    "Value"                       DECIMAL(28,6),
    "Culture"                     VARCHAR(128),
    "Start"                       TIMESTAMP(6),
    "StartHash"                   INT,
    "End"                         TIMESTAMP(6),
    "PayrunJobId"                 INT,
    "Forecast"                    VARCHAR(128),
    "ParentJobId"                 INT,
    "Tags"                        TEXT,
    "Attributes"                  TEXT
)
LANGUAGE plpgsql STABLE
AS $$
BEGIN
    IF "periodStartHashes" IS NULL THEN
        RETURN;
    END IF;
    RETURN QUERY
    WITH "Winners" AS (
        SELECT r."Id",
            ROW_NUMBER() OVER (
                PARTITION BY r."WageTypeNumber", r."Start"
                ORDER BY r."Created" DESC, r."Id" DESC
            ) AS "RowNumber"
        FROM "WageTypeResult" r
        WHERE r."TenantId" = "tenantId"
          AND r."EmployeeId" = "employeeId"
          AND r."StartHash" = ANY(ARRAY(
                SELECT (v::bigint)::int
                FROM jsonb_array_elements_text("periodStartHashes"::jsonb) v))
          AND ("divisionId" IS NULL OR r."DivisionId" = "divisionId")
          AND ("wageTypeNumbers" IS NULL OR r."WageTypeNumber" = ANY(
                ARRAY(SELECT CAST(v AS DECIMAL(28,6))
                      FROM jsonb_array_elements_text("wageTypeNumbers"::jsonb) v)))
          AND ("evaluationDate" IS NULL OR r."Created" <= "evaluationDate")
          AND ("jobStatus" IS NULL OR r."PayrunJobId" IN (
                SELECT pj."Id" FROM "PayrunJob" pj
                WHERE pj."JobStatus" = "jobStatus"))
          AND (r."Forecast" IS NULL OR r."Forecast" = "forecast")
          AND (COALESCE("noRetro", FALSE) = FALSE OR r."ParentJobId" IS NULL)
          AND ("excludeParentJobId" IS NULL OR r."ParentJobId" IS NULL
               OR r."ParentJobId" <> "excludeParentJobId")
    )
    SELECT r.*
    FROM "WageTypeResult" r
    INNER JOIN "Winners" w ON w."Id" = r."Id"
    WHERE w."RowNumber" = 1;
END;
$$;

-- GetDerivedCaseFields.pg.sql
-- ----------------------------------------------------------------------
-- =============================================================================
-- GetDerivedCaseFields
-- Filtered by case field names.
-- =============================================================================

CREATE OR REPLACE FUNCTION GetDerivedCaseFields(
    IN "tenantId" INTEGER, IN "payrollId" INTEGER,
    IN "regulationDate" TIMESTAMP(6), IN "createdBefore" TIMESTAMP(6),
    IN "caseFieldNames" TEXT, IN "includeClusters" TEXT, IN "excludeClusters" TEXT
)
RETURNS TABLE(
    "RegulationId" INT, "Level" INT, "Priority" INT,
    "CaseId" INT, "CaseType" INT,
    "Id" INT, "Status" INT, "Created" TIMESTAMPTZ, "Updated" TIMESTAMPTZ,
    "Name" TEXT, "NameLocalizations" TEXT, "Description" TEXT, "DescriptionLocalizations" TEXT,
    "ValueType" INT, "ValueScope" INT, "StartDateType" INT, "EndDateType" INT,
    "EndMandatory" BOOLEAN, "DefaultStart" TEXT, "DefaultEnd" TEXT, "DefaultValue" TEXT,
    "LookupSettings" TEXT, "TimeType" INT, "TimeUnit" INT, "Culture" TEXT,
    "PeriodAggregation" INT, "OverrideType" INT, "CancellationMode" INT, "ValueCreationMode" INT,
    "ValueMandatory" BOOLEAN, "Order" INT, "Tags" TEXT, "Clusters" TEXT, "Attributes" TEXT, "ValueAttributes" TEXT
)
LANGUAGE sql STABLE AS $$
    WITH DerivedRegulations AS (
        SELECT r."Id", pl."Level", pl."Priority",
            ROW_NUMBER() OVER (PARTITION BY pl."Id", r."Name" ORDER BY r."ValidFrom" DESC NULLS LAST, r."Created" DESC) AS "RowNumber"
        FROM "PayrollLayer" pl
        INNER JOIN "Regulation" r ON pl."RegulationName" = r."Name"
        WHERE r."Status" = 0
          AND (r."TenantId" = "tenantId"
            OR (r."SharedRegulation" = true
              AND EXISTS (SELECT 1 FROM "RegulationShare" rs
                          INNER JOIN "Regulation" rp ON rs."ProviderRegulationId" = rp."Id"
                          WHERE rp."Name" = r."Name" AND rs."ConsumerTenantId" = "tenantId" AND rs."IsolationLevel" >= 3)))
          AND r."Created" <= "createdBefore"
          AND (r."ValidFrom" IS NULL OR r."ValidFrom" <= "regulationDate")
          AND pl."Status" = 0 AND pl."PayrollId" = "payrollId"
    ),
    Regulations AS (SELECT "Id", "Level", "Priority" FROM DerivedRegulations WHERE "RowNumber" = 1)
    SELECT
        reg."Id" AS "RegulationId", reg."Level", reg."Priority",
        cf."CaseId", c."CaseType",
        cf."Id", cf."Status", cf."Created", cf."Updated",
        cf."Name", cf."NameLocalizations", cf."Description", cf."DescriptionLocalizations",
        cf."ValueType", cf."ValueScope", cf."StartDateType", cf."EndDateType",
        cf."EndMandatory", cf."DefaultStart", cf."DefaultEnd", cf."DefaultValue",
        cf."LookupSettings", cf."TimeType", cf."TimeUnit", cf."Culture",
        cf."PeriodAggregation", cf."OverrideType", cf."CancellationMode", cf."ValueCreationMode",
        cf."ValueMandatory", cf."Order", cf."Tags", cf."Clusters", cf."Attributes", cf."ValueAttributes"
    FROM "CaseField" cf
    INNER JOIN "Case" c ON cf."CaseId" = c."Id"
    INNER JOIN Regulations reg ON c."RegulationId" = reg."Id"
    WHERE cf."Status" = 0
      AND cf."Created" <= "createdBefore"
      AND (("includeClusters" IS NULL AND "excludeClusters" IS NULL)
           OR IsMatchingCluster("includeClusters", "excludeClusters", cf."Clusters") = 1)
      AND ("caseFieldNames" IS NULL
           OR LOWER(cf."Name") IN (
               SELECT LOWER(jt.val)
               FROM jsonb_array_elements_text("caseFieldNames"::jsonb) AS jt(val)))
    ORDER BY reg."Level" DESC, reg."Priority" DESC;
$$;

-- GetDerivedCaseFieldsOfCase.pg.sql
-- ----------------------------------------------------------------------
-- =============================================================================
-- GetDerivedCaseFieldsOfCase
-- Filtered by case names (not field names).
-- =============================================================================

CREATE OR REPLACE FUNCTION GetDerivedCaseFieldsOfCase(
    IN "tenantId" INTEGER, IN "payrollId" INTEGER,
    IN "regulationDate" TIMESTAMP(6), IN "createdBefore" TIMESTAMP(6),
    IN "caseNames" TEXT, IN "includeClusters" TEXT, IN "excludeClusters" TEXT
)
RETURNS TABLE(
    "RegulationId" INT, "Level" INT, "Priority" INT,
    "CaseId" INT, "CaseType" INT,
    "Id" INT, "Status" INT, "Created" TIMESTAMPTZ, "Updated" TIMESTAMPTZ,
    "Name" TEXT, "NameLocalizations" TEXT, "Description" TEXT, "DescriptionLocalizations" TEXT,
    "ValueType" INT, "ValueScope" INT, "StartDateType" INT, "EndDateType" INT,
    "EndMandatory" BOOLEAN, "DefaultStart" TEXT, "DefaultEnd" TEXT, "DefaultValue" TEXT,
    "LookupSettings" TEXT, "TimeType" INT, "TimeUnit" INT, "Culture" TEXT,
    "PeriodAggregation" INT, "OverrideType" INT, "CancellationMode" INT, "ValueCreationMode" INT,
    "ValueMandatory" BOOLEAN, "Order" INT, "Tags" TEXT, "Clusters" TEXT, "Attributes" TEXT, "ValueAttributes" TEXT
)
LANGUAGE sql STABLE AS $$
    WITH DerivedRegulations AS (
        SELECT r."Id", pl."Level", pl."Priority",
            ROW_NUMBER() OVER (PARTITION BY pl."Id", r."Name" ORDER BY r."ValidFrom" DESC NULLS LAST, r."Created" DESC) AS "RowNumber"
        FROM "PayrollLayer" pl
        INNER JOIN "Regulation" r ON pl."RegulationName" = r."Name"
        WHERE r."Status" = 0
          AND (r."TenantId" = "tenantId"
            OR (r."SharedRegulation" = true
              AND EXISTS (SELECT 1 FROM "RegulationShare" rs
                          INNER JOIN "Regulation" rp ON rs."ProviderRegulationId" = rp."Id"
                          WHERE rp."Name" = r."Name" AND rs."ConsumerTenantId" = "tenantId" AND rs."IsolationLevel" >= 3)))
          AND r."Created" <= "createdBefore"
          AND (r."ValidFrom" IS NULL OR r."ValidFrom" <= "regulationDate")
          AND pl."Status" = 0 AND pl."PayrollId" = "payrollId"
    ),
    Regulations AS (SELECT "Id", "Level", "Priority" FROM DerivedRegulations WHERE "RowNumber" = 1)
    SELECT
        reg."Id" AS "RegulationId", reg."Level", reg."Priority",
        cf."CaseId", c."CaseType",
        cf."Id", cf."Status", cf."Created", cf."Updated",
        cf."Name", cf."NameLocalizations", cf."Description", cf."DescriptionLocalizations",
        cf."ValueType", cf."ValueScope", cf."StartDateType", cf."EndDateType",
        cf."EndMandatory", cf."DefaultStart", cf."DefaultEnd", cf."DefaultValue",
        cf."LookupSettings", cf."TimeType", cf."TimeUnit", cf."Culture",
        cf."PeriodAggregation", cf."OverrideType", cf."CancellationMode", cf."ValueCreationMode",
        cf."ValueMandatory", cf."Order", cf."Tags", cf."Clusters", cf."Attributes", cf."ValueAttributes"
    FROM "CaseField" cf
    INNER JOIN "Case" c ON cf."CaseId" = c."Id"
    INNER JOIN Regulations reg ON c."RegulationId" = reg."Id"
    WHERE cf."Status" = 0
      AND cf."Created" <= "createdBefore"
      AND (("includeClusters" IS NULL AND "excludeClusters" IS NULL)
           OR IsMatchingCluster("includeClusters", "excludeClusters", cf."Clusters") = 1)
      AND ("caseNames" IS NULL
           OR LOWER(c."Name") IN (
               SELECT LOWER(jt.val)
               FROM jsonb_array_elements_text("caseNames"::jsonb) AS jt(val)))
    ORDER BY reg."Level" DESC, reg."Priority" DESC;
$$;

-- GetDerivedCaseRelations.pg.sql
-- ----------------------------------------------------------------------
-- =============================================================================
-- GetDerivedCaseRelations
-- cr."Order" double-quoted (reserved keyword in PG)
-- Excludes Binary, Script, ScriptVersion (performance hint)
-- =============================================================================

CREATE OR REPLACE FUNCTION GetDerivedCaseRelations(
    IN "tenantId" INTEGER, IN "payrollId" INTEGER,
    IN "regulationDate" TIMESTAMP(6), IN "createdBefore" TIMESTAMP(6),
    IN "sourceCaseName" TEXT, IN "targetCaseName" TEXT,
    IN "includeClusters" TEXT, IN "excludeClusters" TEXT
)
RETURNS TABLE(
    "RegulationId" INT, "Level" INT, "Priority" INT,
    "Id" INT, "Status" INT, "Created" TIMESTAMPTZ, "Updated" TIMESTAMPTZ, "CrRegulationId" INT,
    "SourceCaseName" TEXT, "SourceCaseNameLocalizations" TEXT,
    "SourceCaseSlot" TEXT, "SourceCaseSlotLocalizations" TEXT,
    "TargetCaseName" TEXT, "TargetCaseNameLocalizations" TEXT,
    "TargetCaseSlot" TEXT, "TargetCaseSlotLocalizations" TEXT,
    "RelationHash" INT, "BuildExpression" TEXT, "ValidateExpression" TEXT,
    "OverrideType" INT, "Order" INT,
    "ScriptHash" INT, "Attributes" TEXT, "Clusters" TEXT,
    "BuildActions" TEXT, "ValidateActions" TEXT
)
LANGUAGE sql STABLE AS $$
    WITH DerivedRegulations AS (
        SELECT r."Id", pl."Level", pl."Priority",
            ROW_NUMBER() OVER (PARTITION BY pl."Id", r."Name" ORDER BY r."ValidFrom" DESC NULLS LAST, r."Created" DESC) AS "RowNumber"
        FROM "PayrollLayer" pl
        INNER JOIN "Regulation" r ON pl."RegulationName" = r."Name"
        WHERE r."Status" = 0
          AND (r."TenantId" = "tenantId"
            OR (r."SharedRegulation" = true
              AND EXISTS (SELECT 1 FROM "RegulationShare" rs
                          INNER JOIN "Regulation" rp ON rs."ProviderRegulationId" = rp."Id"
                          WHERE rp."Name" = r."Name" AND rs."ConsumerTenantId" = "tenantId" AND rs."IsolationLevel" >= 3)))
          AND r."Created" <= "createdBefore"
          AND (r."ValidFrom" IS NULL OR r."ValidFrom" <= "regulationDate")
          AND pl."Status" = 0 AND pl."PayrollId" = "payrollId"
    ),
    Regulations AS (SELECT "Id", "Level", "Priority" FROM DerivedRegulations WHERE "RowNumber" = 1)
    SELECT
        reg."Id" AS "RegulationId", reg."Level", reg."Priority",
        cr."Id", cr."Status", cr."Created", cr."Updated", cr."RegulationId" AS "CrRegulationId",
        cr."SourceCaseName", cr."SourceCaseNameLocalizations",
        cr."SourceCaseSlot", cr."SourceCaseSlotLocalizations",
        cr."TargetCaseName", cr."TargetCaseNameLocalizations",
        cr."TargetCaseSlot", cr."TargetCaseSlotLocalizations",
        cr."RelationHash", cr."BuildExpression", cr."ValidateExpression",
        cr."OverrideType", cr."Order",
        cr."ScriptHash", cr."Attributes", cr."Clusters",
        cr."BuildActions", cr."ValidateActions"
    FROM "CaseRelation" cr
    INNER JOIN Regulations reg ON cr."RegulationId" = reg."Id"
    WHERE cr."Status" = 0
      AND cr."Created" <= "createdBefore"
      AND ("sourceCaseName" IS NULL
           OR LOWER(cr."SourceCaseName") = LOWER("sourceCaseName"))
      AND ("targetCaseName" IS NULL
           OR LOWER(cr."TargetCaseName") = LOWER("targetCaseName"))
      AND (("includeClusters" IS NULL AND "excludeClusters" IS NULL)
           OR IsMatchingCluster("includeClusters", "excludeClusters", cr."Clusters") = 1)
    ORDER BY cr."SourceCaseName", cr."TargetCaseName", reg."Level" DESC, reg."Priority" DESC;
$$;

-- GetDerivedCases.pg.sql
-- ----------------------------------------------------------------------
-- =============================================================================
-- GetDerivedCases
-- Excludes Binary, Script, ScriptVersion, Hidden (performance hint identical to T-SQL)
-- =============================================================================

CREATE OR REPLACE FUNCTION GetDerivedCases(
    IN "tenantId" INTEGER, IN "payrollId" INTEGER,
    IN "regulationDate" TIMESTAMP(6), IN "createdBefore" TIMESTAMP(6),
    IN "caseType" INTEGER, IN "caseNames" TEXT,
    IN "includeClusters" TEXT, IN "excludeClusters" TEXT, IN "hidden" BOOLEAN
)
RETURNS TABLE(
    "RegulationId" INT, "Level" INT, "Priority" INT,
    "Id" INT, "Status" INT, "Created" TIMESTAMPTZ, "Updated" TIMESTAMPTZ, "CsRegulationId" INT,
    "CaseType" INT, "Name" TEXT, "NameLocalizations" TEXT, "NameSynonyms" TEXT,
    "Description" TEXT, "DescriptionLocalizations" TEXT,
    "DefaultReason" TEXT, "DefaultReasonLocalizations" TEXT,
    "BaseCase" TEXT, "BaseCaseFields" TEXT,
    "OverrideType" INT, "CancellationType" INT,
    "AvailableExpression" TEXT, "BuildExpression" TEXT, "ValidateExpression" TEXT,
    "Lookups" TEXT, "Slots" TEXT,
    "ScriptHash" INT, "Attributes" TEXT, "Clusters" TEXT,
    "AvailableActions" TEXT, "BuildActions" TEXT, "ValidateActions" TEXT
)
LANGUAGE sql STABLE AS $$
    WITH DerivedRegulations AS (
        SELECT r."Id", pl."Level", pl."Priority",
            ROW_NUMBER() OVER (PARTITION BY pl."Id", r."Name" ORDER BY r."ValidFrom" DESC NULLS LAST, r."Created" DESC) AS "RowNumber"
        FROM "PayrollLayer" pl
        INNER JOIN "Regulation" r ON pl."RegulationName" = r."Name"
        WHERE r."Status" = 0
          AND (r."TenantId" = "tenantId"
            OR (r."SharedRegulation" = true
              AND EXISTS (SELECT 1 FROM "RegulationShare" rs
                          INNER JOIN "Regulation" rp ON rs."ProviderRegulationId" = rp."Id"
                          WHERE rp."Name" = r."Name" AND rs."ConsumerTenantId" = "tenantId" AND rs."IsolationLevel" >= 3)))
          AND r."Created" <= "createdBefore"
          AND (r."ValidFrom" IS NULL OR r."ValidFrom" <= "regulationDate")
          AND pl."Status" = 0 AND pl."PayrollId" = "payrollId"
    ),
    Regulations AS (SELECT "Id", "Level", "Priority" FROM DerivedRegulations WHERE "RowNumber" = 1)
    SELECT
        reg."Id" AS "RegulationId", reg."Level", reg."Priority",
        c."Id", c."Status", c."Created", c."Updated", c."RegulationId" AS "CsRegulationId",
        c."CaseType", c."Name", c."NameLocalizations", c."NameSynonyms",
        c."Description", c."DescriptionLocalizations",
        c."DefaultReason", c."DefaultReasonLocalizations",
        c."BaseCase", c."BaseCaseFields",
        c."OverrideType", c."CancellationType",
        c."AvailableExpression", c."BuildExpression", c."ValidateExpression",
        c."Lookups", c."Slots",
        c."ScriptHash", c."Attributes", c."Clusters",
        c."AvailableActions", c."BuildActions", c."ValidateActions"
    FROM "Case" c
    INNER JOIN Regulations reg ON c."RegulationId" = reg."Id"
    WHERE c."Status" = 0
      AND c."Created" <= "createdBefore"
      AND ("hidden" IS NULL OR c."Hidden" = "hidden")
      AND ("caseType" IS NULL OR c."CaseType" = "caseType")
      AND (("includeClusters" IS NULL AND "excludeClusters" IS NULL)
           OR IsMatchingCluster("includeClusters", "excludeClusters", c."Clusters") = 1)
      AND ("caseNames" IS NULL
           OR LOWER(c."Name") IN (
               SELECT LOWER(jt.val)
               FROM jsonb_array_elements_text("caseNames"::jsonb) AS jt(val)))
    ORDER BY reg."Level" DESC, reg."Priority" DESC;
$$;

-- GetDerivedCollectors.pg.sql
-- ----------------------------------------------------------------------
CREATE OR REPLACE FUNCTION GetDerivedCollectors(
    IN "tenantId" INTEGER, IN "payrollId" INTEGER,
    IN "regulationDate" TIMESTAMP(6), IN "createdBefore" TIMESTAMP(6),
    IN "includeClusters" TEXT, IN "excludeClusters" TEXT, IN "collectorNames" TEXT
)
RETURNS TABLE(
    "RegulationId" INT, "Level" INT, "Priority" INT,
    "Id" INT, "Status" INT, "Created" TIMESTAMPTZ, "Updated" TIMESTAMPTZ, "CoRegulationId" INT,
    "Name" TEXT, "NameLocalizations" TEXT,
    "CollectMode" INT, "Negated" BOOLEAN, "OverrideType" INT, "ValueType" INT,
    "Culture" TEXT, "CollectorGroups" TEXT,
    "StartExpression" TEXT, "ApplyExpression" TEXT, "EndExpression" TEXT,
    "StartActions" TEXT, "ApplyActions" TEXT, "EndActions" TEXT,
    "Threshold" NUMERIC, "MinResult" NUMERIC, "MaxResult" NUMERIC,
    "ScriptHash" TEXT, "Attributes" TEXT, "Clusters" TEXT
)
LANGUAGE sql STABLE AS $$
        WITH DerivedRegulations AS (
        SELECT r."Id", pl."Level", pl."Priority",
            ROW_NUMBER() OVER (PARTITION BY pl."Id", r."Name" ORDER BY r."ValidFrom" DESC NULLS LAST, r."Created" DESC) AS "RowNumber"
        FROM "PayrollLayer" pl
        INNER JOIN "Regulation" r ON pl."RegulationName" = r."Name"
        WHERE r."Status" = 0
          AND (r."TenantId" = "tenantId"
            OR (r."SharedRegulation" = true
              AND EXISTS (SELECT 1 FROM "RegulationShare" rs
                          INNER JOIN "Regulation" rp ON rs."ProviderRegulationId" = rp."Id"
                          WHERE rp."Name" = r."Name" AND rs."ConsumerTenantId" = "tenantId" AND rs."IsolationLevel" >= 3)))
          AND r."Created" <= "createdBefore"
          AND (r."ValidFrom" IS NULL OR r."ValidFrom" <= "regulationDate")
          AND pl."Status" = 0 AND pl."PayrollId" = "payrollId"
    ),
    Regulations AS (SELECT "Id", "Level", "Priority" FROM DerivedRegulations WHERE "RowNumber" = 1)
    SELECT reg."Id", reg."Level", reg."Priority",
        co."Id", co."Status", co."Created", co."Updated", co."RegulationId",
        co."Name", co."NameLocalizations",
        co."CollectMode", co."Negated", co."OverrideType", co."ValueType",
        co."Culture", co."CollectorGroups",
        co."StartExpression", co."ApplyExpression", co."EndExpression",
        co."StartActions", co."ApplyActions", co."EndActions",
        co."Threshold", co."MinResult", co."MaxResult",
        co."ScriptHash", co."Attributes", co."Clusters"
    FROM "Collector" co
    INNER JOIN Regulations reg ON co."RegulationId" = reg."Id"
    WHERE co."Status" = 0 AND co."Created" <= "createdBefore"
    ORDER BY co."Name", reg."Level" DESC, reg."Priority" DESC;
$$;

-- GetDerivedLookupValues.pg.sql
-- ----------------------------------------------------------------------
-- =============================================================================
-- GetDerivedLookupValues
-- lv."Key" double-quoted (reserved keyword in PG)
-- Case-sensitive key filter (no LOWER(), identical to T-SQL)
-- =============================================================================

CREATE OR REPLACE FUNCTION GetDerivedLookupValues(
    IN "tenantId" INTEGER, IN "payrollId" INTEGER,
    IN "regulationDate" TIMESTAMP(6), IN "createdBefore" TIMESTAMP(6),
    IN "lookupNames" TEXT, IN "lookupKeys" TEXT
)
RETURNS TABLE(
    "RegulationId" INT, "Level" INT, "Priority" INT,
    "Id" INT, "Status" INT, "Created" TIMESTAMPTZ, "Updated" TIMESTAMPTZ, "LookupId" INT,
    "Key" TEXT, "KeyHash" INT, "RangeValue" NUMERIC, "Value" TEXT, "ValueLocalizations" TEXT,
    "OverrideType" INT, "LookupHash" INT
)
LANGUAGE sql STABLE AS $$
    WITH DerivedRegulations AS (
        SELECT r."Id", pl."Level", pl."Priority",
            ROW_NUMBER() OVER (PARTITION BY pl."Id", r."Name" ORDER BY r."ValidFrom" DESC NULLS LAST, r."Created" DESC) AS "RowNumber"
        FROM "PayrollLayer" pl
        INNER JOIN "Regulation" r ON pl."RegulationName" = r."Name"
        WHERE r."Status" = 0
          AND (r."TenantId" = "tenantId"
            OR (r."SharedRegulation" = true
              AND EXISTS (SELECT 1 FROM "RegulationShare" rs
                          INNER JOIN "Regulation" rp ON rs."ProviderRegulationId" = rp."Id"
                          WHERE rp."Name" = r."Name" AND rs."ConsumerTenantId" = "tenantId" AND rs."IsolationLevel" >= 3)))
          AND r."Created" <= "createdBefore"
          AND (r."ValidFrom" IS NULL OR r."ValidFrom" <= "regulationDate")
          AND pl."Status" = 0 AND pl."PayrollId" = "payrollId"
    ),
    Regulations AS (SELECT "Id", "Level", "Priority" FROM DerivedRegulations WHERE "RowNumber" = 1)
    SELECT
        reg."Id" AS "RegulationId", reg."Level", reg."Priority",
        lv."Id", lv."Status", lv."Created", lv."Updated", lv."LookupId",
        lv."Key", lv."KeyHash", lv."RangeValue", lv."Value", lv."ValueLocalizations",
        lv."OverrideType", lv."LookupHash"
    FROM "LookupValue" lv
    INNER JOIN "Lookup" lk ON lv."LookupId" = lk."Id"
    INNER JOIN Regulations reg ON lk."RegulationId" = reg."Id"
    WHERE lv."Status" = 0
      AND lv."Created" <= "createdBefore"
      AND ("lookupNames" IS NULL
           OR LOWER(lk."Name") IN (
               SELECT LOWER(jt.val)
               FROM jsonb_array_elements_text("lookupNames"::jsonb) AS jt(val)))
      AND ("lookupKeys" IS NULL
           OR lv."Key" IN (
               SELECT jt.val
               FROM jsonb_array_elements_text("lookupKeys"::jsonb) AS jt(val)))
    ORDER BY reg."Level" DESC, reg."Priority" DESC;
$$;

-- GetDerivedLookups.pg.sql
-- ----------------------------------------------------------------------
-- =============================================================================
-- GetDerivedLookups
-- =============================================================================

CREATE OR REPLACE FUNCTION GetDerivedLookups(
    IN "tenantId" INTEGER, IN "payrollId" INTEGER,
    IN "regulationDate" TIMESTAMP(6), IN "createdBefore" TIMESTAMP(6),
    IN "lookupNames" TEXT
)
RETURNS TABLE(
    "RegulationId" INT, "Level" INT, "Priority" INT,
    "Id" INT, "Status" INT, "Created" TIMESTAMPTZ, "Updated" TIMESTAMPTZ, "LkRegulationId" INT,
    "Name" TEXT, "NameLocalizations" TEXT, "Description" TEXT, "DescriptionLocalizations" TEXT,
    "OverrideType" INT, "RangeSize" NUMERIC, "Attributes" TEXT, "RangeMode" INT
)
LANGUAGE sql STABLE AS $$
    WITH DerivedRegulations AS (
        SELECT r."Id", pl."Level", pl."Priority",
            ROW_NUMBER() OVER (PARTITION BY pl."Id", r."Name" ORDER BY r."ValidFrom" DESC NULLS LAST, r."Created" DESC) AS "RowNumber"
        FROM "PayrollLayer" pl
        INNER JOIN "Regulation" r ON pl."RegulationName" = r."Name"
        WHERE r."Status" = 0
          AND (r."TenantId" = "tenantId"
            OR (r."SharedRegulation" = true
              AND EXISTS (SELECT 1 FROM "RegulationShare" rs
                          INNER JOIN "Regulation" rp ON rs."ProviderRegulationId" = rp."Id"
                          WHERE rp."Name" = r."Name" AND rs."ConsumerTenantId" = "tenantId" AND rs."IsolationLevel" >= 3)))
          AND r."Created" <= "createdBefore"
          AND (r."ValidFrom" IS NULL OR r."ValidFrom" <= "regulationDate")
          AND pl."Status" = 0 AND pl."PayrollId" = "payrollId"
    ),
    Regulations AS (SELECT "Id", "Level", "Priority" FROM DerivedRegulations WHERE "RowNumber" = 1)
    SELECT
        reg."Id" AS "RegulationId", reg."Level", reg."Priority",
        lk."Id", lk."Status", lk."Created", lk."Updated", lk."RegulationId" AS "LkRegulationId",
        lk."Name", lk."NameLocalizations", lk."Description", lk."DescriptionLocalizations",
        lk."OverrideType", lk."RangeSize", lk."Attributes", lk."RangeMode"
    FROM "Lookup" lk
    INNER JOIN Regulations reg ON lk."RegulationId" = reg."Id"
    WHERE lk."Status" = 0
      AND lk."Created" <= "createdBefore"
      AND ("lookupNames" IS NULL
           OR LOWER(lk."Name") IN (
               SELECT LOWER(jt.val)
               FROM jsonb_array_elements_text("lookupNames"::jsonb) AS jt(val)))
    ORDER BY reg."Level" DESC, reg."Priority" DESC;
$$;

-- GetDerivedPayrollRegulations.pg.sql
-- ----------------------------------------------------------------------
CREATE OR REPLACE FUNCTION GetDerivedPayrollRegulations(
    IN "tenantId" INTEGER, IN "payrollId" INTEGER,
    IN "regulationDate" TIMESTAMP(6), IN "createdBefore" TIMESTAMP(6)
)
RETURNS TABLE(
    "Id" INT, "Status" INT, "Created" TIMESTAMPTZ, "Updated" TIMESTAMPTZ,
    "Name" TEXT, "NameLocalizations" TEXT,
    "ValidFrom" TIMESTAMP, "Owner" TEXT, "SharedRegulation" BOOLEAN,
    "TenantId" INT, "Attributes" TEXT,
    "Level" INT, "Priority" INT
)
LANGUAGE sql STABLE AS $$
        WITH DerivedRegulations AS (
        SELECT r."Id", pl."Level", pl."Priority",
            ROW_NUMBER() OVER (PARTITION BY pl."Id", r."Name" ORDER BY r."ValidFrom" DESC NULLS LAST, r."Created" DESC) AS "RowNumber"
        FROM "PayrollLayer" pl
        INNER JOIN "Regulation" r ON pl."RegulationName" = r."Name"
        WHERE r."Status" = 0
          AND (r."TenantId" = "tenantId"
            OR (r."SharedRegulation" = true
              AND EXISTS (SELECT 1 FROM "RegulationShare" rs
                          INNER JOIN "Regulation" rp ON rs."ProviderRegulationId" = rp."Id"
                          WHERE rp."Name" = r."Name" AND rs."ConsumerTenantId" = "tenantId" AND rs."IsolationLevel" >= 3)))
          AND r."Created" <= "createdBefore"
          AND (r."ValidFrom" IS NULL OR r."ValidFrom" <= "regulationDate")
          AND pl."Status" = 0 AND pl."PayrollId" = "payrollId"
    ),
    Regulations AS (SELECT "Id", "Level", "Priority" FROM DerivedRegulations WHERE "RowNumber" = 1)
    SELECT r."Id", r."Status", r."Created", r."Updated",
        r."Name", r."NameLocalizations",
        r."ValidFrom", r."Owner", r."SharedRegulation",
        r."TenantId", r."Attributes",
        reg."Level", reg."Priority"
    FROM "Regulation" r
    INNER JOIN Regulations reg ON r."Id" = reg."Id"
    ORDER BY reg."Level" DESC, reg."Priority" DESC;
$$;

-- GetDerivedReportParameters.pg.sql
-- ----------------------------------------------------------------------
-- =============================================================================
-- GetDerivedReportParameters
-- =============================================================================

CREATE OR REPLACE FUNCTION GetDerivedReportParameters(
    IN "tenantId" INTEGER, IN "payrollId" INTEGER,
    IN "regulationDate" TIMESTAMP(6), IN "createdBefore" TIMESTAMP(6),
    IN "reportNames" TEXT
)
RETURNS TABLE(
    "RegulationId" INT, "Level" INT, "Priority" INT,
    "Id" INT, "Status" INT, "Created" TIMESTAMPTZ, "Updated" TIMESTAMPTZ, "ReportId" INT,
    "Name" TEXT, "NameLocalizations" TEXT, "Description" TEXT, "DescriptionLocalizations" TEXT,
    "Mandatory" BOOLEAN, "Hidden" BOOLEAN, "Value" TEXT, "ValueType" INT, "ParameterType" INT,
    "OverrideType" INT, "Attributes" TEXT
)
LANGUAGE sql STABLE AS $$
    WITH DerivedRegulations AS (
        SELECT r."Id", pl."Level", pl."Priority",
            ROW_NUMBER() OVER (PARTITION BY pl."Id", r."Name" ORDER BY r."ValidFrom" DESC NULLS LAST, r."Created" DESC) AS "RowNumber"
        FROM "PayrollLayer" pl
        INNER JOIN "Regulation" r ON pl."RegulationName" = r."Name"
        WHERE r."Status" = 0
          AND (r."TenantId" = "tenantId"
            OR (r."SharedRegulation" = true
              AND EXISTS (SELECT 1 FROM "RegulationShare" rs
                          INNER JOIN "Regulation" rp ON rs."ProviderRegulationId" = rp."Id"
                          WHERE rp."Name" = r."Name" AND rs."ConsumerTenantId" = "tenantId" AND rs."IsolationLevel" >= 3)))
          AND r."Created" <= "createdBefore"
          AND (r."ValidFrom" IS NULL OR r."ValidFrom" <= "regulationDate")
          AND pl."Status" = 0 AND pl."PayrollId" = "payrollId"
    ),
    Regulations AS (SELECT "Id", "Level", "Priority" FROM DerivedRegulations WHERE "RowNumber" = 1)
    SELECT
        reg."Id" AS "RegulationId", reg."Level", reg."Priority",
        rpar."Id", rpar."Status", rpar."Created", rpar."Updated", rpar."ReportId",
        rpar."Name", rpar."NameLocalizations", rpar."Description", rpar."DescriptionLocalizations",
        rpar."Mandatory", rpar."Hidden", rpar."Value", rpar."ValueType", rpar."ParameterType",
        rpar."OverrideType", rpar."Attributes"
    FROM "ReportParameter" rpar
    INNER JOIN "Report" rp ON rpar."ReportId" = rp."Id"
    INNER JOIN Regulations reg ON rp."RegulationId" = reg."Id"
    WHERE rpar."Status" = 0
      AND rpar."Created" <= "createdBefore"
      AND ("reportNames" IS NULL
           OR LOWER(rp."Name") IN (
               SELECT LOWER(jt.val)
               FROM jsonb_array_elements_text("reportNames"::jsonb) AS jt(val)))
    ORDER BY reg."Level" DESC, reg."Priority" DESC;
$$;

-- GetDerivedReportTemplates.pg.sql
-- ----------------------------------------------------------------------
-- =============================================================================
-- GetDerivedReportTemplates
-- rt."Schema" double-quoted (reserved keyword in PG)
-- =============================================================================

CREATE OR REPLACE FUNCTION GetDerivedReportTemplates(
    IN "tenantId" INTEGER, IN "payrollId" INTEGER,
    IN "regulationDate" TIMESTAMP(6), IN "createdBefore" TIMESTAMP(6),
    IN "reportNames" TEXT, IN "culture" TEXT
)
RETURNS TABLE(
    "RegulationId" INT, "Level" INT, "Priority" INT,
    "Id" INT, "Status" INT, "Created" TIMESTAMPTZ, "Updated" TIMESTAMPTZ, "ReportId" INT,
    "Name" TEXT, "Culture" TEXT, "Content" TEXT, "ContentType" TEXT,
    "Schema" TEXT, "Resource" TEXT, "OverrideType" INT, "Attributes" TEXT
)
LANGUAGE sql STABLE AS $$
    WITH DerivedRegulations AS (
        SELECT r."Id", pl."Level", pl."Priority",
            ROW_NUMBER() OVER (PARTITION BY pl."Id", r."Name" ORDER BY r."ValidFrom" DESC NULLS LAST, r."Created" DESC) AS "RowNumber"
        FROM "PayrollLayer" pl
        INNER JOIN "Regulation" r ON pl."RegulationName" = r."Name"
        WHERE r."Status" = 0
          AND (r."TenantId" = "tenantId"
            OR (r."SharedRegulation" = true
              AND EXISTS (SELECT 1 FROM "RegulationShare" rs
                          INNER JOIN "Regulation" rp ON rs."ProviderRegulationId" = rp."Id"
                          WHERE rp."Name" = r."Name" AND rs."ConsumerTenantId" = "tenantId" AND rs."IsolationLevel" >= 3)))
          AND r."Created" <= "createdBefore"
          AND (r."ValidFrom" IS NULL OR r."ValidFrom" <= "regulationDate")
          AND pl."Status" = 0 AND pl."PayrollId" = "payrollId"
    ),
    Regulations AS (SELECT "Id", "Level", "Priority" FROM DerivedRegulations WHERE "RowNumber" = 1)
    SELECT
        reg."Id" AS "RegulationId", reg."Level", reg."Priority",
        rt."Id", rt."Status", rt."Created", rt."Updated", rt."ReportId",
        rt."Name", rt."Culture", rt."Content", rt."ContentType",
        rt."Schema", rt."Resource", rt."OverrideType", rt."Attributes"
    FROM "ReportTemplate" rt
    INNER JOIN "Report" rp ON rt."ReportId" = rp."Id"
    INNER JOIN Regulations reg ON rp."RegulationId" = reg."Id"
    WHERE rt."Status" = 0
      AND rt."Created" <= "createdBefore"
      AND ("reportNames" IS NULL
           OR LOWER(rp."Name") IN (
               SELECT LOWER(jt.val)
               FROM jsonb_array_elements_text("reportNames"::jsonb) AS jt(val)))
      AND ("culture" IS NULL OR rt."Culture" = "culture")
    ORDER BY reg."Level" DESC, reg."Priority" DESC;
$$;

-- GetDerivedReports.pg.sql
-- ----------------------------------------------------------------------
-- =============================================================================
-- GetDerivedReports
-- Excludes Binary, Script, ScriptVersion, OverrideType (performance hint)
-- =============================================================================

CREATE OR REPLACE FUNCTION GetDerivedReports(
    IN "tenantId" INTEGER, IN "payrollId" INTEGER,
    IN "regulationDate" TIMESTAMP(6), IN "createdBefore" TIMESTAMP(6),
    IN "userType" INTEGER, IN "reportNames" TEXT,
    IN "includeClusters" TEXT, IN "excludeClusters" TEXT
)
RETURNS TABLE(
    "RegulationId" INT, "Level" INT, "Priority" INT,
    "Id" INT, "Status" INT, "Created" TIMESTAMPTZ, "Updated" TIMESTAMPTZ, "RpRegulationId" INT,
    "Name" TEXT, "NameLocalizations" TEXT, "Description" TEXT, "DescriptionLocalizations" TEXT,
    "Category" TEXT, "Queries" TEXT, "Relations" TEXT,
    "AttributeMode" INT, "UserType" INT, "ReportIsolation" INT,
    "BuildExpression" TEXT, "StartExpression" TEXT, "EndExpression" TEXT,
    "ScriptHash" INT, "Attributes" TEXT, "Clusters" TEXT
)
LANGUAGE sql STABLE AS $$
    WITH DerivedRegulations AS (
        SELECT r."Id", pl."Level", pl."Priority",
            ROW_NUMBER() OVER (PARTITION BY pl."Id", r."Name" ORDER BY r."ValidFrom" DESC NULLS LAST, r."Created" DESC) AS "RowNumber"
        FROM "PayrollLayer" pl
        INNER JOIN "Regulation" r ON pl."RegulationName" = r."Name"
        WHERE r."Status" = 0
          AND (r."TenantId" = "tenantId"
            OR (r."SharedRegulation" = true
              AND EXISTS (SELECT 1 FROM "RegulationShare" rs
                          INNER JOIN "Regulation" rp ON rs."ProviderRegulationId" = rp."Id"
                          WHERE rp."Name" = r."Name" AND rs."ConsumerTenantId" = "tenantId" AND rs."IsolationLevel" >= 3)))
          AND r."Created" <= "createdBefore"
          AND (r."ValidFrom" IS NULL OR r."ValidFrom" <= "regulationDate")
          AND pl."Status" = 0 AND pl."PayrollId" = "payrollId"
    ),
    Regulations AS (SELECT "Id", "Level", "Priority" FROM DerivedRegulations WHERE "RowNumber" = 1)
    SELECT
        reg."Id" AS "RegulationId", reg."Level", reg."Priority",
        rp."Id", rp."Status", rp."Created", rp."Updated", rp."RegulationId" AS "RpRegulationId",
        rp."Name", rp."NameLocalizations",
        rp."Description", rp."DescriptionLocalizations",
        rp."Category", rp."Queries", rp."Relations",
        rp."AttributeMode", rp."UserType", rp."ReportIsolation",
        rp."BuildExpression", rp."StartExpression", rp."EndExpression",
        rp."ScriptHash", rp."Attributes", rp."Clusters"
    FROM "Report" rp
    INNER JOIN Regulations reg ON rp."RegulationId" = reg."Id"
    WHERE rp."Status" = 0
      AND rp."Created" <= "createdBefore"
      AND ("userType" IS NULL OR rp."UserType" <= "userType")
      AND (("includeClusters" IS NULL AND "excludeClusters" IS NULL)
           OR IsMatchingCluster("includeClusters", "excludeClusters", rp."Clusters") = 1)
      AND ("reportNames" IS NULL
           OR LOWER(rp."Name") IN (
               SELECT LOWER(jt.val)
               FROM jsonb_array_elements_text("reportNames"::jsonb) AS jt(val)))
    ORDER BY reg."Level" DESC, reg."Priority" DESC;
$$;

-- GetDerivedScripts.pg.sql
-- ----------------------------------------------------------------------
-- =============================================================================
-- GetDerivedScripts
-- OverrideType excluded from SELECT (matches T-SQL explicit column list)
-- =============================================================================

CREATE OR REPLACE FUNCTION GetDerivedScripts(
    IN "tenantId" INTEGER, IN "payrollId" INTEGER,
    IN "regulationDate" TIMESTAMP(6), IN "createdBefore" TIMESTAMP(6),
    IN "scriptNames" TEXT
)
RETURNS TABLE(
    "RegulationId" INT, "Level" INT, "Priority" INT,
    "Id" INT, "Status" INT, "Created" TIMESTAMPTZ, "Updated" TIMESTAMPTZ, "ScRegulationId" INT,
    "Name" TEXT, "FunctionTypeMask" BIGINT, "Value" TEXT
)
LANGUAGE sql STABLE AS $$
    WITH DerivedRegulations AS (
        SELECT r."Id", pl."Level", pl."Priority",
            ROW_NUMBER() OVER (PARTITION BY pl."Id", r."Name" ORDER BY r."ValidFrom" DESC NULLS LAST, r."Created" DESC) AS "RowNumber"
        FROM "PayrollLayer" pl
        INNER JOIN "Regulation" r ON pl."RegulationName" = r."Name"
        WHERE r."Status" = 0
          AND (r."TenantId" = "tenantId"
            OR (r."SharedRegulation" = true
              AND EXISTS (SELECT 1 FROM "RegulationShare" rs
                          INNER JOIN "Regulation" rp ON rs."ProviderRegulationId" = rp."Id"
                          WHERE rp."Name" = r."Name" AND rs."ConsumerTenantId" = "tenantId" AND rs."IsolationLevel" >= 3)))
          AND r."Created" <= "createdBefore"
          AND (r."ValidFrom" IS NULL OR r."ValidFrom" <= "regulationDate")
          AND pl."Status" = 0 AND pl."PayrollId" = "payrollId"
    ),
    Regulations AS (SELECT "Id", "Level", "Priority" FROM DerivedRegulations WHERE "RowNumber" = 1)
    SELECT
        reg."Id" AS "RegulationId", reg."Level", reg."Priority",
        s."Id", s."Status", s."Created", s."Updated", s."RegulationId" AS "ScRegulationId",
        s."Name", s."FunctionTypeMask", s."Value"
    FROM "Script" s
    INNER JOIN Regulations reg ON s."RegulationId" = reg."Id"
    WHERE s."Status" = 0
      AND s."Created" <= "createdBefore"
      AND ("scriptNames" IS NULL
           OR LOWER(s."Name") IN (
               SELECT LOWER(jt.val)
               FROM jsonb_array_elements_text("scriptNames"::jsonb) AS jt(val)))
    ORDER BY reg."Level" DESC, reg."Priority" DESC;
$$;

-- GetDerivedWageTypes.pg.sql
-- ----------------------------------------------------------------------
CREATE OR REPLACE FUNCTION GetDerivedWageTypes(
    IN "tenantId" INTEGER, IN "payrollId" INTEGER,
    IN "regulationDate" TIMESTAMP(6), IN "createdBefore" TIMESTAMP(6),
    IN "wageTypeNumbers" TEXT, IN "includeClusters" TEXT, IN "excludeClusters" TEXT
)
RETURNS TABLE("RegulationId" INT, "Level" INT, "Priority" INT,
    "Id" INT, "Status" INT, "Created" TIMESTAMPTZ, "Updated" TIMESTAMPTZ, "WtRegulationId" INT,
    "Name" TEXT, "NameLocalizations" TEXT, "WageTypeNumber" NUMERIC,
    "Description" TEXT, "DescriptionLocalizations" TEXT,
    "OverrideType" INT, "ValueType" INT, "Calendar" TEXT, "Culture" TEXT,
    "Collectors" TEXT, "CollectorGroups" TEXT,
    "ValueExpression" TEXT, "ResultExpression" TEXT,
    "ValueActions" TEXT, "ResultActions" TEXT,
    "ScriptHash" TEXT, "Attributes" TEXT, "Clusters" TEXT
)
LANGUAGE sql STABLE AS $$
    WITH DerivedRegulations AS (
        SELECT r."Id", pl."Level", pl."Priority",
            ROW_NUMBER() OVER (PARTITION BY pl."Id", r."Name" ORDER BY r."ValidFrom" DESC NULLS LAST, r."Created" DESC) AS "RowNumber"
        FROM "PayrollLayer" pl
        INNER JOIN "Regulation" r ON pl."RegulationName" = r."Name"
        WHERE r."Status" = 0
          AND (r."TenantId" = "tenantId"
            OR (r."SharedRegulation" = true
              AND EXISTS (SELECT 1 FROM "RegulationShare" rs
                          INNER JOIN "Regulation" rp ON rs."ProviderRegulationId" = rp."Id"
                          WHERE rp."Name" = r."Name" AND rs."ConsumerTenantId" = "tenantId" AND rs."IsolationLevel" >= 3)))
          AND r."Created" <= "createdBefore"
          AND (r."ValidFrom" IS NULL OR r."ValidFrom" <= "regulationDate")
          AND pl."Status" = 0 AND pl."PayrollId" = "payrollId"
    ),
    Regulations AS (SELECT "Id", "Level", "Priority" FROM DerivedRegulations WHERE "RowNumber" = 1)
    SELECT reg."Id", reg."Level", reg."Priority",
        wt."Id", wt."Status", wt."Created", wt."Updated", wt."RegulationId",
        wt."Name", wt."NameLocalizations", wt."WageTypeNumber",
        wt."Description", wt."DescriptionLocalizations",
        wt."OverrideType", wt."ValueType", wt."Calendar", wt."Culture",
        wt."Collectors", wt."CollectorGroups",
        wt."ValueExpression", wt."ResultExpression",
        wt."ValueActions", wt."ResultActions",
        wt."ScriptHash", wt."Attributes", wt."Clusters"
    FROM "WageType" wt
    INNER JOIN Regulations reg ON wt."RegulationId" = reg."Id"
    WHERE wt."Status" = 0 AND wt."Created" <= "createdBefore"
    ORDER BY wt."WageTypeNumber", reg."Level" DESC, reg."Priority" DESC;
$$;

-- GetEmployeeCaseChangeValues.pg.sql
-- ----------------------------------------------------------------------
-- =============================================================================
-- GetEmployeeCaseChangeValues
-- Filter is EmployeeId (not TenantId); extra JOIN to Employee for TenantId.
-- Creates temp pivot table joining CaseChange + CaseValue + User + Employee,
-- with optional culture-based name localization and attribute columns.
-- Count queries (sql ILIKE '%COUNT(*)%') are routed via EXECUTE INTO to
-- return the count value in the first column ("TenantId") for Dapper long mapping.
-- =============================================================================

DROP PROCEDURE IF EXISTS GetEmployeeCaseChangeValues;

CREATE OR REPLACE FUNCTION "GetEmployeeCaseChangeValues"(
    IN "parentId"   INTEGER,
    IN "employeeId" INTEGER,
    IN "divisionId" INTEGER,
    IN "sql"        TEXT,
    IN "attributes" TEXT,
    IN "culture"    TEXT
)
RETURNS TABLE(
    "TenantId"           INTEGER,
    "CaseChangeId"       INTEGER,
    "CaseChangeCreated"  TIMESTAMP(6),
    "Reason"             TEXT,
    "ValidationCaseName" VARCHAR(128),
    "CancellationType"   INTEGER,
    "CancellationId"     INTEGER,
    "CancellationDate"   TIMESTAMP(6),
    "EmployeeId"         INTEGER,
    "UserId"             INTEGER,
    "UserIdentifier"     VARCHAR(128),
    "DivisionId"         INTEGER,
    "Id"                 INTEGER,
    "Created"            TIMESTAMP(6),
    "Updated"            TIMESTAMP(6),
    "Status"             INTEGER,
    "CaseName"           VARCHAR(128),
    "CaseFieldName"      VARCHAR(128),
    "CaseSlot"           VARCHAR(128),
    "CaseRelation"       TEXT,
    "ValueType"          INTEGER,
    "Value"              TEXT,
    "NumericValue"       DECIMAL(28,6),
    "Culture"            VARCHAR(128),
    "Start"              TIMESTAMP(6),
    "End"                TIMESTAMP(6),
    "Forecast"           VARCHAR(128),
    "Tags"               TEXT,
    "Attributes"         TEXT,
    "Documents"          BIGINT
)
LANGUAGE plpgsql VOLATILE AS $$
DECLARE
    v_attrSql       TEXT;
    v_pivotSql      TEXT;
    v_caseName      TEXT;
    v_caseFieldName TEXT;
    v_caseSlot      TEXT;
    v_count         BIGINT;
BEGIN
    IF "culture" IS NULL THEN
        v_caseName      := '"EmployeeCaseValue"."CaseName"';
        v_caseFieldName := '"EmployeeCaseValue"."CaseFieldName"';
        v_caseSlot      := '"EmployeeCaseValue"."CaseSlot"';
    ELSE
        v_caseName      := 'GetLocalizedValue("EmployeeCaseValue"."CaseNameLocalizations", ''' || "culture" || ''', "EmployeeCaseValue"."CaseName")';
        v_caseFieldName := 'GetLocalizedValue("EmployeeCaseValue"."CaseFieldNameLocalizations", ''' || "culture" || ''', "EmployeeCaseValue"."CaseFieldName")';
        v_caseSlot      := 'GetLocalizedValue("EmployeeCaseValue"."CaseSlotLocalizations", ''' || "culture" || ''', "EmployeeCaseValue"."CaseSlot")';
    END IF;

    v_attrSql  := BuildAttributeQuery('"EmployeeCaseValue"."Attributes"', "attributes");
    v_pivotSql := 'CREATE TEMP TABLE "##EmployeeCaseChangeValuePivot" AS SELECT'
        || ' "Employee"."TenantId",'
        || ' "EmployeeCaseChange"."Id" AS "CaseChangeId",'
        || ' "EmployeeCaseChange"."Created" AS "CaseChangeCreated",'
        || ' "EmployeeCaseChange"."Reason",'
        || ' "EmployeeCaseChange"."ValidationCaseName",'
        || ' "EmployeeCaseChange"."CancellationType",'
        || ' "EmployeeCaseChange"."CancellationId",'
        || ' "EmployeeCaseChange"."CancellationDate",'
        || ' "EmployeeCaseChange"."EmployeeId",'
        || ' "EmployeeCaseChange"."UserId",'
        || ' "User"."Identifier" AS "UserIdentifier",'
        || ' "EmployeeCaseChange"."DivisionId",'
        || ' "EmployeeCaseValue"."Id",'
        || ' "EmployeeCaseValue"."Created",'
        || ' "EmployeeCaseValue"."Updated",'
        || ' "EmployeeCaseValue"."Status",'
        || ' ' || v_caseName      || ' AS "CaseName",'
        || ' ' || v_caseFieldName || ' AS "CaseFieldName",'
        || ' ' || v_caseSlot      || ' AS "CaseSlot",'
        || ' "EmployeeCaseValue"."CaseRelation",'
        || ' "EmployeeCaseValue"."ValueType",'
        || ' "EmployeeCaseValue"."Value",'
        || ' "EmployeeCaseValue"."NumericValue",'
        || ' "EmployeeCaseValue"."Culture",'
        || ' "EmployeeCaseValue"."Start",'
        || ' "EmployeeCaseValue"."End",'
        || ' "EmployeeCaseValue"."Forecast",'
        || ' "EmployeeCaseValue"."Tags",'
        || ' "EmployeeCaseValue"."Attributes",'
        || ' (SELECT COUNT(*) FROM "EmployeeCaseDocument" WHERE "CaseValueId" = "EmployeeCaseValue"."Id") AS "Documents"'
        || v_attrSql
        || ' FROM "EmployeeCaseValue"'
        || ' LEFT JOIN "EmployeeCaseValueChange" ON "EmployeeCaseValue"."Id" = "EmployeeCaseValueChange"."CaseValueId"'
        || ' LEFT JOIN "EmployeeCaseChange" ON "EmployeeCaseValueChange"."CaseChangeId" = "EmployeeCaseChange"."Id"'
        || ' LEFT JOIN "User" ON "User"."Id" = "EmployeeCaseChange"."UserId"'
        || ' LEFT JOIN "Employee" ON "Employee"."Id" = "EmployeeCaseChange"."EmployeeId"'
        || ' WHERE "EmployeeCaseChange"."EmployeeId" = ' || "parentId"::TEXT;

    DROP TABLE IF EXISTS "##EmployeeCaseChangeValuePivot";
    EXECUTE v_pivotSql;

    IF "sql" ILIKE '%COUNT(*)%' OR "sql" ILIKE '%COUNT(0)%' THEN
        EXECUTE "sql" INTO v_count;
        "TenantId" := v_count::INTEGER;
        RETURN NEXT;
    ELSE
        RETURN QUERY EXECUTE "sql";
    END IF;

    DROP TABLE IF EXISTS "##EmployeeCaseChangeValuePivot";
EXCEPTION WHEN OTHERS THEN
    DROP TABLE IF EXISTS "##EmployeeCaseChangeValuePivot";
    RAISE;
END;
$$;

-- GetEmployeeCaseValues.pg.sql
-- ----------------------------------------------------------------------
-- =============================================================================
-- GetEmployeeCaseValues
-- Filter is EmployeeId (not TenantId) -- employee-scoped pivot.
-- Count queries (sql ILIKE '%COUNT(*)%') are routed via EXECUTE INTO to
-- return the count value in the first column ("Id") for Dapper long mapping.
-- =============================================================================

DROP PROCEDURE IF EXISTS GetEmployeeCaseValues;

CREATE OR REPLACE FUNCTION "GetEmployeeCaseValues"(
    IN "parentId"   INTEGER,
    IN "employeeId" INTEGER,
    IN "divisionId" INTEGER,
    IN "sql"        TEXT,
    IN "attributes" TEXT,
    IN "culture"    TEXT
)
RETURNS TABLE(
    "Id"                         INTEGER,
    "Status"                     INTEGER,
    "Created"                    TIMESTAMP(6),
    "Updated"                    TIMESTAMP(6),
    "EmployeeId"                 INTEGER,
    "DivisionId"                 INTEGER,
    "CaseName"                   VARCHAR(128),
    "CaseNameLocalizations"      TEXT,
    "CaseFieldName"              VARCHAR(128),
    "CaseFieldNameLocalizations" TEXT,
    "CaseSlot"                   VARCHAR(128),
    "CaseSlotLocalizations"      TEXT,
    "ValueType"                  INTEGER,
    "Value"                      TEXT,
    "NumericValue"               DECIMAL(28,6),
    "Culture"                    VARCHAR(128),
    "CaseRelation"               TEXT,
    "CancellationDate"           TIMESTAMP(6),
    "Start"                      TIMESTAMP(6),
    "End"                        TIMESTAMP(6),
    "Forecast"                   VARCHAR(128),
    "Tags"                       TEXT,
    "Attributes"                 TEXT
)
LANGUAGE plpgsql VOLATILE AS $$
DECLARE
    v_attrSql  TEXT;
    v_pivotSql TEXT;
    v_count    BIGINT;
BEGIN
    v_attrSql  := BuildAttributeQuery('"EmployeeCaseValue"."Attributes"', "attributes");
    v_pivotSql := 'CREATE TEMP TABLE "##EmployeeCaseValuePivot" AS'
        || ' SELECT "EmployeeCaseValue".*'
        || v_attrSql
        || ' FROM "EmployeeCaseValue"'
        || ' WHERE "EmployeeCaseValue"."EmployeeId" = ' || "parentId"::TEXT;

    DROP TABLE IF EXISTS "##EmployeeCaseValuePivot";
    EXECUTE v_pivotSql;

    IF "sql" ILIKE '%COUNT(*)%' OR "sql" ILIKE '%COUNT(0)%' THEN
        EXECUTE "sql" INTO v_count;
        "Id" := v_count::INTEGER;
        RETURN NEXT;
    ELSE
        RETURN QUERY EXECUTE "sql";
    END IF;

    DROP TABLE IF EXISTS "##EmployeeCaseValuePivot";
EXCEPTION WHEN OTHERS THEN
    DROP TABLE IF EXISTS "##EmployeeCaseValuePivot";
    RAISE;
END;
$$;

-- GetEmployeeCaseValuesByTenant.pg.sql
-- ----------------------------------------------------------------------
-- =============================================================================
-- GetEmployeeCaseValuesByTenant
-- Direct JOIN query -- no pivot, no temp table needed.
-- Returns active, non-cancelled employee case values for a tenant filtered
-- by optional value date window, evaluation date, field names, and forecast.
-- =============================================================================

DROP PROCEDURE IF EXISTS GetEmployeeCaseValuesByTenant;

CREATE OR REPLACE FUNCTION "GetEmployeeCaseValuesByTenant"(
    IN "tenantId"       INTEGER,
    IN "valueDate"      TIMESTAMP(6),
    IN "evaluationDate" TIMESTAMP(6),
    IN "fieldNames"     TEXT,
    IN "forecast"       TEXT
)
RETURNS TABLE(
    "Id"                         INTEGER,
    "Status"                     INTEGER,
    "Created"                    TIMESTAMP(6),
    "Updated"                    TIMESTAMP(6),
    "EmployeeId"                 INTEGER,
    "DivisionId"                 INTEGER,
    "CaseName"                   VARCHAR(128),
    "CaseNameLocalizations"      TEXT,
    "CaseFieldName"              VARCHAR(128),
    "CaseFieldNameLocalizations" TEXT,
    "CaseSlot"                   VARCHAR(128),
    "CaseSlotLocalizations"      TEXT,
    "ValueType"                  INTEGER,
    "Value"                      TEXT,
    "NumericValue"               DECIMAL(28,6),
    "Culture"                    VARCHAR(128),
    "CaseRelation"               TEXT,
    "CancellationDate"           TIMESTAMP(6),
    "Start"                      TIMESTAMP(6),
    "End"                        TIMESTAMP(6),
    "Forecast"                   VARCHAR(128),
    "Tags"                       TEXT,
    "Attributes"                 TEXT
)
LANGUAGE plpgsql STABLE AS $$
BEGIN
    RETURN QUERY
    SELECT
        ecv."Id", ecv."Status", ecv."Created", ecv."Updated",
        ecv."EmployeeId", ecv."DivisionId",
        ecv."CaseName", ecv."CaseNameLocalizations",
        ecv."CaseFieldName", ecv."CaseFieldNameLocalizations",
        ecv."CaseSlot", ecv."CaseSlotLocalizations",
        ecv."ValueType", ecv."Value", ecv."NumericValue", ecv."Culture",
        ecv."CaseRelation", ecv."CancellationDate", ecv."Start", ecv."End",
        ecv."Forecast", ecv."Tags", ecv."Attributes"
    FROM "EmployeeCaseValue" ecv
    INNER JOIN "Employee" e ON e."Id" = ecv."EmployeeId"
    WHERE e."TenantId" = "tenantId"
      AND e."Status" = 0
      AND ecv."CancellationDate" IS NULL
      AND ("evaluationDate" IS NULL OR ecv."Created" <= "evaluationDate")
      AND ("valueDate" IS NULL OR ecv."Start" IS NULL OR ecv."Start" <= "valueDate")
      AND ("valueDate" IS NULL OR ecv."End"   IS NULL OR ecv."End"   >  "valueDate")
      AND (
          ("forecast" IS NULL     AND ecv."Forecast" IS NULL)
          OR ("forecast" IS NOT NULL AND (ecv."Forecast" IS NULL OR ecv."Forecast" = "forecast"))
      )
      AND (
          "fieldNames" IS NULL
          OR ecv."CaseFieldName" IN (
              SELECT jt.val
              FROM jsonb_array_elements_text("fieldNames"::jsonb) AS jt(val)
          )
      )
    ORDER BY ecv."EmployeeId" ASC, ecv."CaseFieldName" ASC, ecv."Created" DESC;
END;
$$;

-- GetGlobalCaseChangeValues.pg.sql
-- ----------------------------------------------------------------------
-- =============================================================================
-- GetGlobalCaseChangeValues
-- Creates temp pivot table joining CaseChange + CaseValue + User, with
-- optional culture-based name localization and attribute columns.
-- Count queries (sql ILIKE '%COUNT(*)%') are routed via EXECUTE INTO to
-- return the count value in the first column ("TenantId") for Dapper long mapping.
-- =============================================================================

DROP PROCEDURE IF EXISTS GetGlobalCaseChangeValues;

CREATE OR REPLACE FUNCTION "GetGlobalCaseChangeValues"(
    IN "parentId"   INTEGER,
    IN "employeeId" INTEGER,
    IN "divisionId" INTEGER,
    IN "sql"        TEXT,
    IN "attributes" TEXT,
    IN "culture"    TEXT
)
RETURNS TABLE(
    "TenantId"           INTEGER,
    "CaseChangeId"       INTEGER,
    "CaseChangeCreated"  TIMESTAMP(6),
    "Reason"             TEXT,
    "ValidationCaseName" VARCHAR(128),
    "CancellationType"   INTEGER,
    "CancellationId"     INTEGER,
    "CancellationDate"   TIMESTAMP(6),
    "EmployeeId"         INTEGER,
    "UserId"             INTEGER,
    "UserIdentifier"     VARCHAR(128),
    "DivisionId"         INTEGER,
    "Id"                 INTEGER,
    "Created"            TIMESTAMP(6),
    "Updated"            TIMESTAMP(6),
    "Status"             INTEGER,
    "CaseName"           VARCHAR(128),
    "CaseFieldName"      VARCHAR(128),
    "CaseSlot"           VARCHAR(128),
    "CaseRelation"       TEXT,
    "ValueType"          INTEGER,
    "Value"              TEXT,
    "NumericValue"       DECIMAL(28,6),
    "Culture"            VARCHAR(128),
    "Start"              TIMESTAMP(6),
    "End"                TIMESTAMP(6),
    "Forecast"           VARCHAR(128),
    "Tags"               TEXT,
    "Attributes"         TEXT,
    "Documents"          BIGINT
)
LANGUAGE plpgsql VOLATILE AS $$
DECLARE
    v_attrSql       TEXT;
    v_pivotSql      TEXT;
    v_caseName      TEXT;
    v_caseFieldName TEXT;
    v_caseSlot      TEXT;
    v_count         BIGINT;
BEGIN
    IF "culture" IS NULL THEN
        v_caseName      := '"GlobalCaseValue"."CaseName"';
        v_caseFieldName := '"GlobalCaseValue"."CaseFieldName"';
        v_caseSlot      := '"GlobalCaseValue"."CaseSlot"';
    ELSE
        v_caseName      := 'GetLocalizedValue("GlobalCaseValue"."CaseNameLocalizations", ''' || "culture" || ''', "GlobalCaseValue"."CaseName")';
        v_caseFieldName := 'GetLocalizedValue("GlobalCaseValue"."CaseFieldNameLocalizations", ''' || "culture" || ''', "GlobalCaseValue"."CaseFieldName")';
        v_caseSlot      := 'GetLocalizedValue("GlobalCaseValue"."CaseSlotLocalizations", ''' || "culture" || ''', "GlobalCaseValue"."CaseSlot")';
    END IF;

    v_attrSql  := BuildAttributeQuery('"GlobalCaseValue"."Attributes"', "attributes");
    v_pivotSql := 'CREATE TEMP TABLE "##GlobalCaseChangeValuePivot" AS SELECT'
        || ' "GlobalCaseChange"."TenantId",'
        || ' "GlobalCaseChange"."Id" AS "CaseChangeId",'
        || ' "GlobalCaseChange"."Created" AS "CaseChangeCreated",'
        || ' "GlobalCaseChange"."Reason",'
        || ' "GlobalCaseChange"."ValidationCaseName",'
        || ' "GlobalCaseChange"."CancellationType",'
        || ' "GlobalCaseChange"."CancellationId",'
        || ' "GlobalCaseChange"."CancellationDate",'
        || ' NULL AS "EmployeeId",'
        || ' "GlobalCaseChange"."UserId",'
        || ' "User"."Identifier" AS "UserIdentifier",'
        || ' "GlobalCaseChange"."DivisionId",'
        || ' "GlobalCaseValue"."Id",'
        || ' "GlobalCaseValue"."Created",'
        || ' "GlobalCaseValue"."Updated",'
        || ' "GlobalCaseValue"."Status",'
        || ' ' || v_caseName      || ' AS "CaseName",'
        || ' ' || v_caseFieldName || ' AS "CaseFieldName",'
        || ' ' || v_caseSlot      || ' AS "CaseSlot",'
        || ' "GlobalCaseValue"."CaseRelation",'
        || ' "GlobalCaseValue"."ValueType",'
        || ' "GlobalCaseValue"."Value",'
        || ' "GlobalCaseValue"."NumericValue",'
        || ' "GlobalCaseValue"."Culture",'
        || ' "GlobalCaseValue"."Start",'
        || ' "GlobalCaseValue"."End",'
        || ' "GlobalCaseValue"."Forecast",'
        || ' "GlobalCaseValue"."Tags",'
        || ' "GlobalCaseValue"."Attributes",'
        || ' (SELECT COUNT(*) FROM "GlobalCaseDocument" WHERE "CaseValueId" = "GlobalCaseValue"."Id") AS "Documents"'
        || v_attrSql
        || ' FROM "GlobalCaseValue"'
        || ' LEFT JOIN "GlobalCaseValueChange" ON "GlobalCaseValue"."Id" = "GlobalCaseValueChange"."CaseValueId"'
        || ' LEFT JOIN "GlobalCaseChange" ON "GlobalCaseValueChange"."CaseChangeId" = "GlobalCaseChange"."Id"'
        || ' LEFT JOIN "User" ON "User"."Id" = "GlobalCaseChange"."UserId"'
        || ' WHERE "GlobalCaseChange"."TenantId" = ' || "parentId"::TEXT;

    DROP TABLE IF EXISTS "##GlobalCaseChangeValuePivot";
    EXECUTE v_pivotSql;

    IF "sql" ILIKE '%COUNT(*)%' OR "sql" ILIKE '%COUNT(0)%' THEN
        EXECUTE "sql" INTO v_count;
        "TenantId" := v_count::INTEGER;
        RETURN NEXT;
    ELSE
        RETURN QUERY EXECUTE "sql";
    END IF;

    DROP TABLE IF EXISTS "##GlobalCaseChangeValuePivot";
EXCEPTION WHEN OTHERS THEN
    DROP TABLE IF EXISTS "##GlobalCaseChangeValuePivot";
    RAISE;
END;
$$;

-- GetGlobalCaseValues.pg.sql
-- ----------------------------------------------------------------------
-- =============================================================================
-- GetGlobalCaseValues
-- Creates temp pivot table with optional attribute columns, then executes
-- the caller-supplied SQL against it.
-- Count queries (sql ILIKE '%COUNT(*)%') are routed via EXECUTE INTO to
-- return the count value in the first column ("Id") for Dapper long mapping.
-- =============================================================================

DROP PROCEDURE IF EXISTS GetGlobalCaseValues;

CREATE OR REPLACE FUNCTION "GetGlobalCaseValues"(
    IN "parentId"   INTEGER,
    IN "employeeId" INTEGER,
    IN "divisionId" INTEGER,
    IN "sql"        TEXT,
    IN "attributes" TEXT,
    IN "culture"    TEXT
)
RETURNS TABLE(
    "Id"                         INTEGER,
    "Status"                     INTEGER,
    "Created"                    TIMESTAMP(6),
    "Updated"                    TIMESTAMP(6),
    "TenantId"                   INTEGER,
    "DivisionId"                 INTEGER,
    "CaseName"                   VARCHAR(128),
    "CaseNameLocalizations"      TEXT,
    "CaseFieldName"              VARCHAR(128),
    "CaseFieldNameLocalizations" TEXT,
    "CaseSlot"                   VARCHAR(128),
    "CaseSlotLocalizations"      TEXT,
    "ValueType"                  INTEGER,
    "Value"                      TEXT,
    "NumericValue"               DECIMAL(28,6),
    "Culture"                    VARCHAR(128),
    "CaseRelation"               TEXT,
    "CancellationDate"           TIMESTAMP(6),
    "Start"                      TIMESTAMP(6),
    "End"                        TIMESTAMP(6),
    "Forecast"                   VARCHAR(128),
    "Tags"                       TEXT,
    "Attributes"                 TEXT
)
LANGUAGE plpgsql VOLATILE AS $$
DECLARE
    v_attrSql  TEXT;
    v_pivotSql TEXT;
    v_count    BIGINT;
BEGIN
    v_attrSql  := BuildAttributeQuery('"GlobalCaseValue"."Attributes"', "attributes");
    v_pivotSql := 'CREATE TEMP TABLE "##GlobalCaseValuePivot" AS'
        || ' SELECT "GlobalCaseValue".*'
        || v_attrSql
        || ' FROM "GlobalCaseValue"'
        || ' WHERE "GlobalCaseValue"."TenantId" = ' || "parentId"::TEXT;

    DROP TABLE IF EXISTS "##GlobalCaseValuePivot";
    EXECUTE v_pivotSql;

    IF "sql" ILIKE '%COUNT(*)%' OR "sql" ILIKE '%COUNT(0)%' THEN
        EXECUTE "sql" INTO v_count;
        "Id" := v_count::INTEGER;
        RETURN NEXT;
    ELSE
        RETURN QUERY EXECUTE "sql";
    END IF;

    DROP TABLE IF EXISTS "##GlobalCaseValuePivot";
EXCEPTION WHEN OTHERS THEN
    DROP TABLE IF EXISTS "##GlobalCaseValuePivot";
    RAISE;
END;
$$;

-- GetLookupRangeValue.pg.sql
-- ----------------------------------------------------------------------
-- =============================================================================
-- GetLookupRangeValue
-- Returns the single LookupValue row whose RangeValue <= p_rangeValue
-- (highest match), optionally filtered by KeyHash.
-- Returns an empty set when rangeValue is outside [min, max+rangeSize].
-- =============================================================================

DROP PROCEDURE IF EXISTS GetLookupRangeValue;

CREATE OR REPLACE FUNCTION "GetLookupRangeValue"(
    IN "lookupId"   INTEGER,
    IN "rangeValue" DECIMAL(28,6),
    IN "keyHash"    INTEGER
)
RETURNS TABLE(
    "Id"                 INTEGER,
    "Status"             INTEGER,
    "Created"            TIMESTAMP(6),
    "Updated"            TIMESTAMP(6),
    "LookupId"           INTEGER,
    "Key"                TEXT,
    "KeyHash"            INTEGER,
    "RangeValue"         DECIMAL(28,6),
    "Value"              TEXT,
    "ValueLocalizations" TEXT,
    "OverrideType"       INTEGER,
    "LookupHash"         INTEGER
)
LANGUAGE plpgsql STABLE AS $$
DECLARE
    v_rangeSize DECIMAL(28,6) DEFAULT 0.0;
    v_minValue  DECIMAL(28,6);
    v_maxValue  DECIMAL(28,6);
BEGIN
    SELECT COALESCE(lk."RangeSize", 0.0) INTO v_rangeSize
    FROM "Lookup" lk WHERE lk."Id" = "lookupId";

    SELECT MIN(lv."RangeValue"), MAX(lv."RangeValue") + v_rangeSize
    INTO v_minValue, v_maxValue
    FROM "LookupValue" lv
    INNER JOIN "Lookup" lk ON lv."LookupId" = lk."Id"
    WHERE lk."Id" = "lookupId";

    IF v_minValue IS NULL
       OR "rangeValue" < v_minValue
       OR "rangeValue" > v_maxValue THEN
        RETURN;
    ELSE
        RETURN QUERY
        SELECT
            lv."Id",
            lv."Status",
            lv."Created",
            lv."Updated",
            lv."LookupId",
            lv."Key",
            lv."KeyHash",
            lv."RangeValue",
            lv."Value",
            lv."ValueLocalizations",
            lv."OverrideType",
            lv."LookupHash"
        FROM "LookupValue" lv
        INNER JOIN "Lookup" lk ON lv."LookupId" = lk."Id"
        WHERE lk."Id" = "lookupId"
          AND lv."RangeValue" <= "rangeValue"
          AND ("keyHash" IS NULL OR lv."KeyHash" = "keyHash")
        ORDER BY lv."RangeValue" DESC
        LIMIT 1;
    END IF;
END;
$$;

-- GetNationalCaseChangeValues.pg.sql
-- ----------------------------------------------------------------------
-- =============================================================================
-- GetNationalCaseChangeValues
-- Creates temp pivot table joining CaseChange + CaseValue + User, with
-- optional culture-based name localization and attribute columns.
-- Count queries (sql ILIKE '%COUNT(*)%') are routed via EXECUTE INTO to
-- return the count value in the first column ("TenantId") for Dapper long mapping.
-- =============================================================================

DROP PROCEDURE IF EXISTS GetNationalCaseChangeValues;

CREATE OR REPLACE FUNCTION "GetNationalCaseChangeValues"(
    IN "parentId"   INTEGER,
    IN "employeeId" INTEGER,
    IN "divisionId" INTEGER,
    IN "sql"        TEXT,
    IN "attributes" TEXT,
    IN "culture"    TEXT
)
RETURNS TABLE(
    "TenantId"           INTEGER,
    "CaseChangeId"       INTEGER,
    "CaseChangeCreated"  TIMESTAMP(6),
    "Reason"             TEXT,
    "ValidationCaseName" VARCHAR(128),
    "CancellationType"   INTEGER,
    "CancellationId"     INTEGER,
    "CancellationDate"   TIMESTAMP(6),
    "EmployeeId"         INTEGER,
    "UserId"             INTEGER,
    "UserIdentifier"     VARCHAR(128),
    "DivisionId"         INTEGER,
    "Id"                 INTEGER,
    "Created"            TIMESTAMP(6),
    "Updated"            TIMESTAMP(6),
    "Status"             INTEGER,
    "CaseName"           VARCHAR(128),
    "CaseFieldName"      VARCHAR(128),
    "CaseSlot"           VARCHAR(128),
    "CaseRelation"       TEXT,
    "ValueType"          INTEGER,
    "Value"              TEXT,
    "NumericValue"       DECIMAL(28,6),
    "Culture"            VARCHAR(128),
    "Start"              TIMESTAMP(6),
    "End"                TIMESTAMP(6),
    "Forecast"           VARCHAR(128),
    "Tags"               TEXT,
    "Attributes"         TEXT,
    "Documents"          BIGINT
)
LANGUAGE plpgsql VOLATILE AS $$
DECLARE
    v_attrSql       TEXT;
    v_pivotSql      TEXT;
    v_caseName      TEXT;
    v_caseFieldName TEXT;
    v_caseSlot      TEXT;
    v_count         BIGINT;
BEGIN
    IF "culture" IS NULL THEN
        v_caseName      := '"NationalCaseValue"."CaseName"';
        v_caseFieldName := '"NationalCaseValue"."CaseFieldName"';
        v_caseSlot      := '"NationalCaseValue"."CaseSlot"';
    ELSE
        v_caseName      := 'GetLocalizedValue("NationalCaseValue"."CaseNameLocalizations", ''' || "culture" || ''', "NationalCaseValue"."CaseName")';
        v_caseFieldName := 'GetLocalizedValue("NationalCaseValue"."CaseFieldNameLocalizations", ''' || "culture" || ''', "NationalCaseValue"."CaseFieldName")';
        v_caseSlot      := 'GetLocalizedValue("NationalCaseValue"."CaseSlotLocalizations", ''' || "culture" || ''', "NationalCaseValue"."CaseSlot")';
    END IF;

    v_attrSql  := BuildAttributeQuery('"NationalCaseValue"."Attributes"', "attributes");
    v_pivotSql := 'CREATE TEMP TABLE "##NationalCaseChangeValuePivot" AS SELECT'
        || ' "NationalCaseChange"."TenantId",'
        || ' "NationalCaseChange"."Id" AS "CaseChangeId",'
        || ' "NationalCaseChange"."Created" AS "CaseChangeCreated",'
        || ' "NationalCaseChange"."Reason",'
        || ' "NationalCaseChange"."ValidationCaseName",'
        || ' "NationalCaseChange"."CancellationType",'
        || ' "NationalCaseChange"."CancellationId",'
        || ' "NationalCaseChange"."CancellationDate",'
        || ' NULL AS "EmployeeId",'
        || ' "NationalCaseChange"."UserId",'
        || ' "User"."Identifier" AS "UserIdentifier",'
        || ' "NationalCaseChange"."DivisionId",'
        || ' "NationalCaseValue"."Id",'
        || ' "NationalCaseValue"."Created",'
        || ' "NationalCaseValue"."Updated",'
        || ' "NationalCaseValue"."Status",'
        || ' ' || v_caseName      || ' AS "CaseName",'
        || ' ' || v_caseFieldName || ' AS "CaseFieldName",'
        || ' ' || v_caseSlot      || ' AS "CaseSlot",'
        || ' "NationalCaseValue"."CaseRelation",'
        || ' "NationalCaseValue"."ValueType",'
        || ' "NationalCaseValue"."Value",'
        || ' "NationalCaseValue"."NumericValue",'
        || ' "NationalCaseValue"."Culture",'
        || ' "NationalCaseValue"."Start",'
        || ' "NationalCaseValue"."End",'
        || ' "NationalCaseValue"."Forecast",'
        || ' "NationalCaseValue"."Tags",'
        || ' "NationalCaseValue"."Attributes",'
        || ' (SELECT COUNT(*) FROM "NationalCaseDocument" WHERE "CaseValueId" = "NationalCaseValue"."Id") AS "Documents"'
        || v_attrSql
        || ' FROM "NationalCaseValue"'
        || ' LEFT JOIN "NationalCaseValueChange" ON "NationalCaseValue"."Id" = "NationalCaseValueChange"."CaseValueId"'
        || ' LEFT JOIN "NationalCaseChange" ON "NationalCaseValueChange"."CaseChangeId" = "NationalCaseChange"."Id"'
        || ' LEFT JOIN "User" ON "User"."Id" = "NationalCaseChange"."UserId"'
        || ' WHERE "NationalCaseChange"."TenantId" = ' || "parentId"::TEXT;

    DROP TABLE IF EXISTS "##NationalCaseChangeValuePivot";
    EXECUTE v_pivotSql;

    IF "sql" ILIKE '%COUNT(*)%' OR "sql" ILIKE '%COUNT(0)%' THEN
        EXECUTE "sql" INTO v_count;
        "TenantId" := v_count::INTEGER;
        RETURN NEXT;
    ELSE
        RETURN QUERY EXECUTE "sql";
    END IF;

    DROP TABLE IF EXISTS "##NationalCaseChangeValuePivot";
EXCEPTION WHEN OTHERS THEN
    DROP TABLE IF EXISTS "##NationalCaseChangeValuePivot";
    RAISE;
END;
$$;

-- GetNationalCaseValues.pg.sql
-- ----------------------------------------------------------------------
-- =============================================================================
-- GetNationalCaseValues
-- Creates temp pivot table with optional attribute columns, then executes
-- the caller-supplied SQL against it.
-- Count queries (sql ILIKE '%COUNT(*)%') are routed via EXECUTE INTO to
-- return the count value in the first column ("Id") for Dapper long mapping.
-- =============================================================================

DROP PROCEDURE IF EXISTS GetNationalCaseValues;

CREATE OR REPLACE FUNCTION "GetNationalCaseValues"(
    IN "parentId"   INTEGER,
    IN "employeeId" INTEGER,
    IN "divisionId" INTEGER,
    IN "sql"        TEXT,
    IN "attributes" TEXT,
    IN "culture"    TEXT
)
RETURNS TABLE(
    "Id"                         INTEGER,
    "Status"                     INTEGER,
    "Created"                    TIMESTAMP(6),
    "Updated"                    TIMESTAMP(6),
    "TenantId"                   INTEGER,
    "DivisionId"                 INTEGER,
    "CaseName"                   VARCHAR(128),
    "CaseNameLocalizations"      TEXT,
    "CaseFieldName"              VARCHAR(128),
    "CaseFieldNameLocalizations" TEXT,
    "CaseSlot"                   VARCHAR(128),
    "CaseSlotLocalizations"      TEXT,
    "ValueType"                  INTEGER,
    "Value"                      TEXT,
    "NumericValue"               DECIMAL(28,6),
    "Culture"                    VARCHAR(128),
    "CaseRelation"               TEXT,
    "CancellationDate"           TIMESTAMP(6),
    "Start"                      TIMESTAMP(6),
    "End"                        TIMESTAMP(6),
    "Forecast"                   VARCHAR(128),
    "Tags"                       TEXT,
    "Attributes"                 TEXT
)
LANGUAGE plpgsql VOLATILE AS $$
DECLARE
    v_attrSql  TEXT;
    v_pivotSql TEXT;
    v_count    BIGINT;
BEGIN
    v_attrSql  := BuildAttributeQuery('"NationalCaseValue"."Attributes"', "attributes");
    v_pivotSql := 'CREATE TEMP TABLE "##NationalCaseValuePivot" AS'
        || ' SELECT "NationalCaseValue".*'
        || v_attrSql
        || ' FROM "NationalCaseValue"'
        || ' WHERE "NationalCaseValue"."TenantId" = ' || "parentId"::TEXT;

    DROP TABLE IF EXISTS "##NationalCaseValuePivot";
    EXECUTE v_pivotSql;

    IF "sql" ILIKE '%COUNT(*)%' OR "sql" ILIKE '%COUNT(0)%' THEN
        EXECUTE "sql" INTO v_count;
        "Id" := v_count::INTEGER;
        RETURN NEXT;
    ELSE
        RETURN QUERY EXECUTE "sql";
    END IF;

    DROP TABLE IF EXISTS "##NationalCaseValuePivot";
EXCEPTION WHEN OTHERS THEN
    DROP TABLE IF EXISTS "##NationalCaseValuePivot";
    RAISE;
END;
$$;

-- GetPayrollResultValues.pg.sql
-- ----------------------------------------------------------------------
-- =============================================================================
-- GetPayrollResultValues
-- 5-way UNION ALL pivot of all result types.
-- Parameter order matches C# positional call (CaseValueExtendedParameters=true):
--   parentId, employeeId, divisionId, sql, attributes
-- p_sql from C# references "##PayrollResultPivot"; the inner pivot is exposed
-- as a CTE under that exact name so the caller query resolves it correctly.
-- =============================================================================

CREATE OR REPLACE FUNCTION "GetPayrollResultValues"(
    IN "parentId"   INT,
    IN "employeeId" INT,
    IN "divisionId" INT,
    IN "sql"        TEXT,
    IN "attributes" TEXT
)
RETURNS TABLE (
    "TenantId"           INT,
    "PayrollResultId"    INT,
    "Created"            TIMESTAMP(6),
    "ResultKind"         INT,
    "ResultId"           INT,
    "ResultParentId"     INT,
    "ResultNumber"       DECIMAL(28,6),
    "KindName"           VARCHAR(128),
    "ResultCreated"      TIMESTAMP(6),
    "ResultStart"        TIMESTAMP(6),
    "ResultEnd"          TIMESTAMP(6),
    "ResultType"         INT,
    "ResultValue"        TEXT,
    "ResultNumericValue" DECIMAL(28,6),
    "ResultCulture"      VARCHAR(128),
    "ResultTags"         TEXT,
    "Attributes"         TEXT,
    "JobId"              INT,
    "JobName"            VARCHAR(128),
    "JobReason"          TEXT,
    "Forecast"           VARCHAR(128),
    "JobStatus"          INT,
    "CycleName"          VARCHAR(128),
    "PeriodName"         VARCHAR(128),
    "PeriodStart"        TIMESTAMP(6),
    "PeriodEnd"          TIMESTAMP(6),
    "PayrunId"           INT,
    "PayrunName"         VARCHAR(128),
    "PayrollId"          INT,
    "PayrollName"        VARCHAR(128),
    "DivisionId"         INT,
    "DivisionName"       VARCHAR(128),
    "Culture"            VARCHAR(128),
    "UserId"             INT,
    "UserIdentifier"     VARCHAR(128),
    "EmployeeId"         INT,
    "EmployeeIdentifier" VARCHAR(128)
)
LANGUAGE plpgsql STABLE
AS $$
DECLARE
    v_attrNames TEXT;
    v_innerSql  TEXT;
    v_where     TEXT;
    v_fullSql   TEXT;
BEGIN
    v_attrNames := GetAttributeNames("attributes");

    -- Build optional WHERE clause (employee / division pre-filter inside the pivot)
    v_where := '';
    IF "employeeId" IS NOT NULL OR "divisionId" IS NOT NULL THEN
        v_where := ' WHERE ';
        IF "employeeId" IS NOT NULL THEN
            v_where := v_where || '"Employee"."Id" = ' || "employeeId"::TEXT;
        END IF;
        IF "employeeId" IS NOT NULL AND "divisionId" IS NOT NULL THEN
            v_where := v_where || ' AND ';
        END IF;
        IF "divisionId" IS NOT NULL THEN
            v_where := v_where || '"Division"."Id" = ' || "divisionId"::TEXT;
        END IF;
    END IF;

    -- Build inner pivot SELECT (fixed columns + optional dynamic attribute columns)
    v_innerSql :=
        'SELECT'
        || ' "PayrollResult"."TenantId",'
        || ' "PayrollResult"."Id" AS "PayrollResultId",'
        || ' "PayrollResult"."Created",'
        || ' "PayrollValue"."ResultKind",'
        || ' "PayrollValue"."ResultId",'
        || ' "PayrollValue"."ResultParentId",'
        || ' "PayrollValue"."ResultNumber",'
        || ' "PayrollValue"."KindName",'
        || ' "PayrollValue"."ResultCreated",'
        || ' "PayrollValue"."ResultStart",'
        || ' "PayrollValue"."ResultEnd",'
        || ' "PayrollValue"."ResultType",'
        || ' "PayrollValue"."ResultValue",'
        || ' "PayrollValue"."ResultNumericValue",'
        || ' "PayrollValue"."ResultCulture",'
        || ' "PayrollValue"."ResultTags",'
        || ' "PayrollValue"."Attributes",'
        || ' "PayrunJob"."Id" AS "JobId",'
        || ' "PayrunJob"."Name" AS "JobName",'
        || ' "PayrunJob"."CreatedReason" AS "JobReason",'
        || ' "PayrunJob"."Forecast",'
        || ' "PayrunJob"."JobStatus",'
        || ' "PayrunJob"."CycleName",'
        || ' "PayrunJob"."PeriodName",'
        || ' "PayrunJob"."PeriodStart",'
        || ' "PayrunJob"."PeriodEnd",'
        || ' "Payrun"."Id" AS "PayrunId",'
        || ' "Payrun"."Name" AS "PayrunName",'
        || ' "Payroll"."Id" AS "PayrollId",'
        || ' "Payroll"."Name" AS "PayrollName",'
        || ' "Division"."Id" AS "DivisionId",'
        || ' "Division"."Name" AS "DivisionName",'
        || ' "Division"."Culture",'
        || ' "User"."Id" AS "UserId",'
        || ' "User"."Identifier" AS "UserIdentifier",'
        || ' "Employee"."Id" AS "EmployeeId",'
        || ' "Employee"."Identifier" AS "EmployeeIdentifier"'
        || v_attrNames
        || ' FROM ('
        -- CollectorResult (kind=10)
        || ' SELECT 10 AS "ResultKind",'
        || ' "CollectorResult"."PayrollResultId",'
        || ' "CollectorResult"."Id" AS "ResultId",'
        || ' "CollectorResult"."PayrollResultId" AS "ResultParentId",'
        || ' "CollectorResult"."CollectorName" AS "KindName",'
        || ' CAST(0 AS DECIMAL(28,6)) AS "ResultNumber",'
        || ' "CollectorResult"."Created" AS "ResultCreated",'
        || ' "CollectorResult"."Start" AS "ResultStart",'
        || ' "CollectorResult"."End" AS "ResultEnd",'
        || ' "CollectorResult"."Tags" AS "ResultTags",'
        || ' "CollectorResult"."Attributes",'
        || ' "CollectorResult"."ValueType" AS "ResultType",'
        || ' to_char("CollectorResult"."Value", ''FM999999999999999999990.00'') AS "ResultValue",'
        || ' "CollectorResult"."Value" AS "ResultNumericValue",'
        || ' "CollectorResult"."Culture" AS "ResultCulture"'
        || BuildAttributeQuery('"CollectorResult"."Attributes"', "attributes")
        || ' FROM "CollectorResult"'
        || ' UNION ALL'
        -- CollectorCustomResult (kind=11)
        || ' SELECT 11 AS "ResultKind",'
        || ' "CollectorResult"."PayrollResultId",'
        || ' "CollectorCustomResult"."Id" AS "ResultId",'
        || ' "CollectorResult"."Id" AS "ResultParentId",'
        || ' "CollectorCustomResult"."Source" AS "KindName",'
        || ' CAST(0 AS DECIMAL(28,6)) AS "ResultNumber",'
        || ' "CollectorCustomResult"."Created" AS "ResultCreated",'
        || ' "CollectorCustomResult"."Start" AS "ResultStart",'
        || ' "CollectorCustomResult"."End" AS "ResultEnd",'
        || ' "CollectorCustomResult"."Tags" AS "ResultTags",'
        || ' "CollectorCustomResult"."Attributes",'
        || ' "CollectorCustomResult"."ValueType" AS "ResultType",'
        || ' to_char("CollectorCustomResult"."Value", ''FM999999999999999999990.00'') AS "ResultValue",'
        || ' "CollectorCustomResult"."Value" AS "ResultNumericValue",'
        || ' "CollectorCustomResult"."Culture" AS "ResultCulture"'
        || BuildAttributeQuery('"CollectorCustomResult"."Attributes"', "attributes")
        || ' FROM "CollectorResult"'
        || ' INNER JOIN "CollectorCustomResult" ON "CollectorResult"."Id" = "CollectorCustomResult"."CollectorResultId"'
        || ' UNION ALL'
        -- WageTypeResult (kind=20)
        || ' SELECT 20 AS "ResultKind",'
        || ' "WageTypeResult"."PayrollResultId",'
        || ' "WageTypeResult"."Id" AS "ResultId",'
        || ' "WageTypeResult"."PayrollResultId" AS "ResultParentId",'
        || ' "WageTypeResult"."WageTypeName" AS "KindName",'
        || ' "WageTypeResult"."WageTypeNumber" AS "ResultNumber",'
        || ' "WageTypeResult"."Created" AS "ResultCreated",'
        || ' "WageTypeResult"."Start" AS "ResultStart",'
        || ' "WageTypeResult"."End" AS "ResultEnd",'
        || ' "WageTypeResult"."Tags" AS "ResultTags",'
        || ' "WageTypeResult"."Attributes",'
        || ' "WageTypeResult"."ValueType" AS "ResultType",'
        || ' to_char("WageTypeResult"."Value", ''FM999999999999999999990.00'') AS "ResultValue",'
        || ' "WageTypeResult"."Value" AS "ResultNumericValue",'
        || ' "WageTypeResult"."Culture" AS "ResultCulture"'
        || BuildAttributeQuery('"WageTypeResult"."Attributes"', "attributes")
        || ' FROM "WageTypeResult"'
        || ' UNION ALL'
        -- WageTypeCustomResult (kind=21)
        || ' SELECT 21 AS "ResultKind",'
        || ' "WageTypeResult"."PayrollResultId",'
        || ' "WageTypeCustomResult"."Id" AS "ResultId",'
        || ' "WageTypeResult"."Id" AS "ResultParentId",'
        || ' "WageTypeCustomResult"."Source" AS "KindName",'
        || ' CAST(0 AS DECIMAL(28,6)) AS "ResultNumber",'
        || ' "WageTypeCustomResult"."Created" AS "ResultCreated",'
        || ' "WageTypeCustomResult"."Start" AS "ResultStart",'
        || ' "WageTypeCustomResult"."End" AS "ResultEnd",'
        || ' "WageTypeCustomResult"."Tags" AS "ResultTags",'
        || ' "WageTypeCustomResult"."Attributes",'
        || ' "WageTypeCustomResult"."ValueType" AS "ResultType",'
        || ' to_char("WageTypeCustomResult"."Value", ''FM999999999999999999990.00'') AS "ResultValue",'
        || ' "WageTypeCustomResult"."Value" AS "ResultNumericValue",'
        || ' "WageTypeCustomResult"."Culture" AS "ResultCulture"'
        || BuildAttributeQuery('"WageTypeCustomResult"."Attributes"', "attributes")
        || ' FROM "WageTypeResult"'
        || ' INNER JOIN "WageTypeCustomResult" ON "WageTypeResult"."Id" = "WageTypeCustomResult"."WageTypeResultId"'
        || ' UNION ALL'
        -- PayrunResult (kind=30)
        || ' SELECT 30 AS "ResultKind",'
        || ' "PayrunResult"."PayrollResultId",'
        || ' "PayrunResult"."Id" AS "ResultId",'
        || ' "PayrunResult"."PayrollResultId" AS "ResultParentId",'
        || ' "PayrunResult"."Name" AS "KindName",'
        || ' CAST(0 AS DECIMAL(28,6)) AS "ResultNumber",'
        || ' "PayrunResult"."Created" AS "ResultCreated",'
        || ' "PayrunResult"."Start" AS "ResultStart",'
        || ' "PayrunResult"."End" AS "ResultEnd",'
        || ' "PayrunResult"."Tags" AS "ResultTags",'
        || ' "PayrunResult"."Attributes",'
        || ' "PayrunResult"."ValueType" AS "ResultType",'
        || ' LTRIM("PayrunResult"."Value") AS "ResultValue",'
        || ' "PayrunResult"."NumericValue" AS "ResultNumericValue",'
        || ' "PayrunResult"."Culture" AS "ResultCulture"'
        || BuildAttributeQuery(NULL, "attributes")
        || ' FROM "PayrunResult"'
        || ') "PayrollValue"'
        || ' LEFT JOIN "PayrollResult" ON "PayrollResult"."Id" = "PayrollValue"."PayrollResultId"'
        || ' LEFT JOIN "PayrunJob" ON "PayrollResult"."PayrunJobId" = "PayrunJob"."Id"'
        || ' LEFT JOIN "Payrun" ON "PayrunJob"."PayrunId" = "Payrun"."Id"'
        || ' LEFT JOIN "Employee" ON "PayrollResult"."EmployeeId" = "Employee"."Id"'
        || ' LEFT JOIN "Payroll" ON "PayrollResult"."PayrollId" = "Payroll"."Id"'
        || ' LEFT JOIN "Division" ON "Payroll"."DivisionId" = "Division"."Id"'
        || ' LEFT JOIN "User" ON "PayrunJob"."CreatedUserId" = "User"."Id"'
        || v_where;

    -- Wrap pivot as CTE under "##PayrollResultPivot" so the caller sql resolves it
    v_fullSql := 'WITH "##PayrollResultPivot" AS (' || v_innerSql || ') ' || "sql";

    RETURN QUERY EXECUTE v_fullSql;
END;
$$;

-- GetWageTypeCustomResults.pg.sql
-- ----------------------------------------------------------------------
-- =============================================================================
-- GetWageTypeCustomResults
-- =============================================================================

CREATE OR REPLACE FUNCTION "GetWageTypeCustomResults"(
    IN "tenantId"          INT,
    IN "employeeId"        INT,
    IN "divisionId"        INT,
    IN "payrunJobId"       INT,
    IN "parentPayrunJobId" INT,
    IN "wageTypeNumbers"   TEXT,
    IN "periodStart"       TIMESTAMP(6),
    IN "periodEnd"         TIMESTAMP(6),
    IN "jobStatus"         INT,
    IN "forecast"          TEXT,
    IN "evaluationDate"    TIMESTAMP(6)
)
RETURNS TABLE (
    "Id"                         INT,
    "Status"                     INT,
    "Created"                    TIMESTAMP(6),
    "Updated"                    TIMESTAMP(6),
    "WageTypeResultId"           INT,
    "TenantId"                   INT,
    "EmployeeId"                 INT,
    "DivisionId"                 INT,
    "WageTypeNumber"             DECIMAL(28,6),
    "WageTypeName"               VARCHAR(128),
    "WageTypeNameLocalizations"  TEXT,
    "Source"                     VARCHAR(128),
    "ValueType"                  INT,
    "Value"                      DECIMAL(28,6),
    "Culture"                    VARCHAR(128),
    "Start"                      TIMESTAMP(6),
    "StartHash"                  INT,
    "End"                        TIMESTAMP(6),
    "PayrunJobId"                INT,
    "Forecast"                   VARCHAR(128),
    "ParentJobId"                INT,
    "Tags"                       TEXT,
    "Attributes"                 TEXT
)
LANGUAGE plpgsql STABLE
AS $$
DECLARE
    v_wageTypeNumber DECIMAL(28,6);
    v_wageTypeCount  INT;
BEGIN
    v_wageTypeCount := CASE WHEN "wageTypeNumbers" IS NULL THEN 0
                            ELSE jsonb_array_length("wageTypeNumbers"::jsonb) END;

    IF v_wageTypeCount = 1 THEN
        SELECT CAST(jt.val AS DECIMAL(28,6)) INTO v_wageTypeNumber
        FROM jsonb_array_elements_text("wageTypeNumbers"::jsonb) AS jt(val)
        LIMIT 1;
    END IF;

    RETURN QUERY
    SELECT "wtcr".*
    FROM "WageTypeCustomResult" "wtcr"
    WHERE "wtcr"."TenantId" = "tenantId"
      AND "wtcr"."EmployeeId" = "employeeId"
      AND ("divisionId" IS NULL        OR "wtcr"."DivisionId" = "divisionId")
      AND ("payrunJobId" IS NULL       OR "wtcr"."PayrunJobId" = "payrunJobId")
      AND ("parentPayrunJobId" IS NULL OR "wtcr"."ParentJobId" = "parentPayrunJobId")
      AND ("wageTypeNumbers" IS NULL OR v_wageTypeCount = 0
           OR (v_wageTypeCount = 1 AND "wtcr"."WageTypeNumber" = v_wageTypeNumber)
           OR (v_wageTypeCount > 1 AND "wtcr"."WageTypeNumber" IN (
               SELECT CAST(jt.val AS DECIMAL(28,6))
               FROM jsonb_array_elements_text("wageTypeNumbers"::jsonb) AS jt(val))))
      AND ("periodStart" IS NULL OR "wtcr"."Start" BETWEEN "periodStart" AND "periodEnd")
      AND ("jobStatus" IS NULL OR "wtcr"."PayrunJobId" IN (
               SELECT "pj"."Id" FROM "PayrunJob" "pj"
               WHERE "pj"."Id" = "wtcr"."PayrunJobId"
                 AND "pj"."JobStatus" = "jobStatus"))
      AND ("wtcr"."Forecast" IS NULL OR "wtcr"."Forecast" = "forecast")
      AND ("evaluationDate" IS NULL OR "wtcr"."Created" <= "evaluationDate")
    ORDER BY "wtcr"."Created";
END;
$$;

-- GetWageTypeResults.pg.sql
-- ----------------------------------------------------------------------
-- =============================================================================
-- GetWageTypeResults
-- =============================================================================

CREATE OR REPLACE FUNCTION "GetWageTypeResults"(
    IN "tenantId"          INT,
    IN "employeeId"        INT,
    IN "divisionId"        INT,
    IN "payrunJobId"       INT,
    IN "parentPayrunJobId" INT,
    IN "wageTypeNumbers"   TEXT,
    IN "periodStart"       TIMESTAMP(6),
    IN "periodEnd"         TIMESTAMP(6),
    IN "jobStatus"         INT,
    IN "forecast"          TEXT,
    IN "evaluationDate"    TIMESTAMP(6)
)
RETURNS TABLE (
    "Id"                         INT,
    "Status"                     INT,
    "Created"                    TIMESTAMP(6),
    "Updated"                    TIMESTAMP(6),
    "PayrollResultId"            INT,
    "TenantId"                   INT,
    "EmployeeId"                 INT,
    "DivisionId"                 INT,
    "WageTypeId"                 INT,
    "WageTypeNumber"             DECIMAL(28,6),
    "WageTypeName"               VARCHAR(128),
    "WageTypeNameLocalizations"  TEXT,
    "ValueType"                  INT,
    "Value"                      DECIMAL(28,6),
    "Culture"                    VARCHAR(128),
    "Start"                      TIMESTAMP(6),
    "StartHash"                  INT,
    "End"                        TIMESTAMP(6),
    "PayrunJobId"                INT,
    "Forecast"                   VARCHAR(128),
    "ParentJobId"                INT,
    "Tags"                       TEXT,
    "Attributes"                 TEXT
)
LANGUAGE plpgsql STABLE
AS $$
DECLARE
    v_wageTypeNumber DECIMAL(28,6);
    v_wageTypeCount  INT;
BEGIN
    v_wageTypeCount := CASE WHEN "wageTypeNumbers" IS NULL THEN 0
                            ELSE jsonb_array_length("wageTypeNumbers"::jsonb) END;

    IF v_wageTypeCount = 1 THEN
        SELECT CAST(jt.val AS DECIMAL(28,6)) INTO v_wageTypeNumber
        FROM jsonb_array_elements_text("wageTypeNumbers"::jsonb) AS jt(val)
        LIMIT 1;
    END IF;

    RETURN QUERY
    SELECT "wtr".*
    FROM "WageTypeResult" "wtr"
    WHERE "wtr"."TenantId" = "tenantId"
      AND "wtr"."EmployeeId" = "employeeId"
      AND ("divisionId" IS NULL        OR "wtr"."DivisionId" = "divisionId")
      AND ("payrunJobId" IS NULL       OR "wtr"."PayrunJobId" = "payrunJobId")
      AND ("parentPayrunJobId" IS NULL OR "wtr"."ParentJobId" = "parentPayrunJobId")
      AND ("wageTypeNumbers" IS NULL OR v_wageTypeCount = 0
           OR (v_wageTypeCount = 1 AND "wtr"."WageTypeNumber" = v_wageTypeNumber)
           OR (v_wageTypeCount > 1 AND "wtr"."WageTypeNumber" IN (
               SELECT CAST(jt.val AS DECIMAL(28,6))
               FROM jsonb_array_elements_text("wageTypeNumbers"::jsonb) AS jt(val))))
      AND ("periodStart" IS NULL OR "wtr"."Start" BETWEEN "periodStart" AND "periodEnd")
      AND ("jobStatus" IS NULL OR "wtr"."PayrunJobId" IN (
               SELECT "pj"."Id" FROM "PayrunJob" "pj"
               WHERE "pj"."Id" = "wtr"."PayrunJobId"
                 AND "pj"."JobStatus" = "jobStatus"))
      AND ("wtr"."Forecast" IS NULL OR "wtr"."Forecast" = "forecast")
      AND ("evaluationDate" IS NULL OR "wtr"."Created" <= "evaluationDate")
    ORDER BY "wtr"."Created";
END;
$$;

-- UpdateStatistics.pg.sql
-- ----------------------------------------------------------------------
-- =============================================================================
-- UpdateStatistics
-- T-SQL: UPDATE STATISTICS ... WITH FULLSCAN -> MySQL: ANALYZE TABLE -> PG: ANALYZE
-- =============================================================================

CREATE OR REPLACE PROCEDURE UpdateStatistics()
LANGUAGE plpgsql
AS $$
DECLARE
    v_table TEXT;
BEGIN
    FOR v_table IN
        SELECT tablename FROM pg_catalog.pg_tables
        WHERE schemaname = 'public'
    LOOP
        EXECUTE format('ANALYZE %I', v_table);
    END LOOP;
END;
$$;

-- UpdateStatisticsTargeted.pg.sql
-- ----------------------------------------------------------------------
-- =============================================================================
-- UpdateStatisticsTargeted
-- T-SQL: UPDATE STATISTICS ... WITH FULLSCAN -> MySQL: ANALYZE TABLE -> PG: ANALYZE
-- =============================================================================

CREATE OR REPLACE PROCEDURE UpdateStatisticsTargeted()
LANGUAGE plpgsql
AS $$
BEGIN
    ANALYZE "LookupValue";
    ANALYZE "PayrollResult";
    ANALYZE "WageTypeResult";
    ANALYZE "WageTypeCustomResult";
    ANALYZE "CollectorResult";
    ANALYZE "CollectorCustomResult";
    ANALYZE "PayrunResult";
    ANALYZE "GlobalCaseValue";
    ANALYZE "NationalCaseValue";
    ANALYZE "CompanyCaseValue";
    ANALYZE "EmployeeCaseValue";
END;
$$;

-- =============================================================================
-- VERSION RECORD
-- =============================================================================

INSERT INTO "Version" ("Created", "MajorVersion", "MinorVersion", "SubVersion", "Owner", "Description")
VALUES (NOW(), 1, 1, 0, CURRENT_USER, 'Payroll Engine: Full setup v1.1.0 (PostgreSQL)');
