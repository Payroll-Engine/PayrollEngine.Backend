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
