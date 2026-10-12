-- =============================================================================
-- Update-Model.pg.sql
-- Migration: PayrollEngine v1.1.0 -> v1.2.0 (PostgreSQL)
-- =============================================================================

BEGIN;

-- =============================================================================
-- VERSION CHECK
-- Guard: abort if the schema is not at version 1.1.0
-- =============================================================================

DO $$
DECLARE
    v_major INT;
    v_minor INT;
    v_sub   INT;
BEGIN
    IF to_regclass('"Version"') IS NULL THEN
        RAISE EXCEPTION 'Schema not found: Version table does not exist. Run Create-Model.pg.sql first.';
    END IF;

    SELECT "MajorVersion", "MinorVersion", "SubVersion"
    INTO v_major, v_minor, v_sub
    FROM "Version"
    ORDER BY "MajorVersion" DESC, "MinorVersion" DESC, "SubVersion" DESC
    LIMIT 1;

    IF v_major IS DISTINCT FROM 1 OR v_minor IS DISTINCT FROM 1 OR v_sub IS DISTINCT FROM 0 THEN
        RAISE EXCEPTION 'Version mismatch: expected 1.1.0, found %.%.%',
            COALESCE(v_major, -1), COALESCE(v_minor, -1), COALESCE(v_sub, -1);
    END IF;
END;
$$;

-- =============================================================================
-- TABLE CHANGES
-- =============================================================================

-- Calendar: anchored week/bi-week periods and pay date offset
ALTER TABLE "Calendar"
    ADD COLUMN "PeriodAnchorDate" TIMESTAMP(6) NULL,
    ADD COLUMN "PayDateOffset" INT NULL;

-- PayrunJob: pay date (existing jobs: period end date)
ALTER TABLE "PayrunJob" ADD COLUMN "PayDate" TIMESTAMP(6) NULL;
UPDATE "PayrunJob" SET "PayDate" = date_trunc('day', "PeriodEnd");
ALTER TABLE "PayrunJob" ALTER COLUMN "PayDate" SET NOT NULL;

-- Results: denormalized payrun job pay date (last column, matches the result functions)
ALTER TABLE "WageTypeResult" ADD COLUMN "PayDate" TIMESTAMP(6) NULL;
ALTER TABLE "WageTypeCustomResult" ADD COLUMN "PayDate" TIMESTAMP(6) NULL;
ALTER TABLE "CollectorResult" ADD COLUMN "PayDate" TIMESTAMP(6) NULL;
ALTER TABLE "CollectorCustomResult" ADD COLUMN "PayDate" TIMESTAMP(6) NULL;
ALTER TABLE "PayrunResult" ADD COLUMN "PayDate" TIMESTAMP(6) NULL;

UPDATE "WageTypeResult" r SET "PayDate" = j."PayDate" FROM "PayrunJob" j WHERE j."Id" = r."PayrunJobId";
UPDATE "WageTypeCustomResult" r SET "PayDate" = j."PayDate" FROM "PayrunJob" j WHERE j."Id" = r."PayrunJobId";
UPDATE "CollectorResult" r SET "PayDate" = j."PayDate" FROM "PayrunJob" j WHERE j."Id" = r."PayrunJobId";
UPDATE "CollectorCustomResult" r SET "PayDate" = j."PayDate" FROM "PayrunJob" j WHERE j."Id" = r."PayrunJobId";
UPDATE "PayrunResult" r SET "PayDate" = j."PayDate" FROM "PayrunJob" j WHERE j."Id" = r."PayrunJobId";

-- results without job: result end date
UPDATE "WageTypeResult" SET "PayDate" = date_trunc('day', "End") WHERE "PayDate" IS NULL;
UPDATE "WageTypeCustomResult" SET "PayDate" = date_trunc('day', "End") WHERE "PayDate" IS NULL;
UPDATE "CollectorResult" SET "PayDate" = date_trunc('day', "End") WHERE "PayDate" IS NULL;
UPDATE "CollectorCustomResult" SET "PayDate" = date_trunc('day', "End") WHERE "PayDate" IS NULL;
UPDATE "PayrunResult" SET "PayDate" = date_trunc('day', "End") WHERE "PayDate" IS NULL;

ALTER TABLE "WageTypeResult" ALTER COLUMN "PayDate" SET NOT NULL;
ALTER TABLE "WageTypeCustomResult" ALTER COLUMN "PayDate" SET NOT NULL;
ALTER TABLE "CollectorResult" ALTER COLUMN "PayDate" SET NOT NULL;
ALTER TABLE "CollectorCustomResult" ALTER COLUMN "PayDate" SET NOT NULL;
ALTER TABLE "PayrunResult" ALTER COLUMN "PayDate" SET NOT NULL;

-- =============================================================================
-- INDEX CHANGES
-- =============================================================================

-- (none in this release)

-- =============================================================================
-- FUNCTION CHANGES
-- =============================================================================

-- Result functions: new result column PayDate (changed return type requires drop)
DROP FUNCTION IF EXISTS "GetWageTypeResults";
DROP FUNCTION IF EXISTS "GetWageTypeCustomResults";
DROP FUNCTION IF EXISTS "GetCollectorResults";
DROP FUNCTION IF EXISTS "GetCollectorCustomResults";
DROP FUNCTION IF EXISTS "GetConsolidatedWageTypeResults";
DROP FUNCTION IF EXISTS "GetConsolidatedWageTypeCustomResults";
DROP FUNCTION IF EXISTS "GetConsolidatedCollectorResults";
DROP FUNCTION IF EXISTS "GetConsolidatedCollectorCustomResults";
DROP FUNCTION IF EXISTS "GetConsolidatedPayrunResults";

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
    "Attributes"                 TEXT,
    "PayDate"                    TIMESTAMP(6)
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
    "Attributes"                 TEXT,
    "PayDate"                    TIMESTAMP(6)
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
    "Attributes"                  TEXT,
    "PayDate"                     TIMESTAMP(6)
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
    "Attributes"                  TEXT,
    "PayDate"                     TIMESTAMP(6)
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
    "Attributes"                  TEXT,
    "PayDate"                     TIMESTAMP(6)
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
    "Attributes"                  TEXT,
    "PayDate"                     TIMESTAMP(6)
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
    "Attributes"                  TEXT,
    "PayDate"                     TIMESTAMP(6)
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
    "Attributes"                  TEXT,
    "PayDate"                     TIMESTAMP(6)
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
    "Attributes"          TEXT,
    "PayDate"             TIMESTAMP(6)
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

-- =============================================================================
-- VERSION SET
-- =============================================================================

INSERT INTO "Version" ("Created", "MajorVersion", "MinorVersion", "SubVersion", "Owner", "Description")
VALUES (NOW(), 1, 2, 0, CURRENT_USER, 'Payroll Engine: Migration v1.1.0 -> v1.2.0 (PostgreSQL)');

COMMIT;
