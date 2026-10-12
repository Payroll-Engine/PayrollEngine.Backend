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
