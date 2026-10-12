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
