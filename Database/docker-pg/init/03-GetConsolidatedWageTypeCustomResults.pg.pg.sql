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
