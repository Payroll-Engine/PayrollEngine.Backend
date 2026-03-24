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
