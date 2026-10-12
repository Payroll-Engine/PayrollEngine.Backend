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
