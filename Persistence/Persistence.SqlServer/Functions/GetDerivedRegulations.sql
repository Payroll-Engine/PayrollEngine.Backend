SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

IF EXISTS (
    SELECT *
    FROM sysobjects
    WHERE id = object_id(N'[dbo].[GetDerivedRegulations]')
      AND OBJECTPROPERTY(id, N'IsInlineFunction') = 1
    )
BEGIN
  DROP FUNCTION [dbo].[GetDerivedRegulations]
END
GO

-- =============================================
-- Get all active derived regulation ids from the payroll.
-- IsolationLevel < Write (< 3) means Consolidation-only — not a payroll layer.
-- Only shares with IsolationLevel >= Write (3) are eligible as payroll layers.
-- =============================================
CREATE FUNCTION [dbo].[GetDerivedRegulations] (
  -- the tenant
  @tenantId AS INT,
  -- the payroll
  @payrollId AS INT,
  -- the regulation valid from date
  @regulationDate AS DATETIME2(7),
  -- the creation date
  @createdBefore AS DATETIME2(7)
  )
RETURNS TABLE
AS
RETURN (
    WITH GroupRegulation AS (
        SELECT [Regulation].[Id],
          [PayrollLayer].[Level],
          [PayrollLayer].[Priority],
          ROW_NUMBER() OVER (
            -- group by regulation name within a payroll layer
            PARTITION BY [PayrollLayer].[Id],
            [Regulation].[Name] ORDER BY [Regulation].[ValidFrom] DESC,
              -- use latest created in case of same valid from
              [Regulation].[Created] DESC
            ) AS RowNumber
        FROM [PayrollLayer]
        INNER JOIN [Regulation]
          ON [PayrollLayer].[RegulationName] = [Regulation].[Name]
        -- active payroll layers and regulations only
        WHERE [Regulation].[Status] = 0
          -- own regulation (always a valid layer)
          -- shared regulation: IsolationLevel must be >= Write to act as payroll layer.
          -- Write = 3 (TenantIsolationLevel enum, PayrollEngine.Core).
          -- IMPORTANT: if TenantIsolationLevel enum values change, this literal must
          -- be updated in sync. The CK_RegulationShare_IsolationLevel check constraint
          -- enforces the allowed set and will fail on INSERT if the enum is extended
          -- without a corresponding schema migration.
          AND (
            [Regulation].[TenantId] = @tenantId
            OR (
              [Regulation].[SharedRegulation] = 1
              AND EXISTS (
                SELECT 1 FROM [dbo].[RegulationShare] rs
                WHERE rs.[ProviderRegulationId] = [Regulation].[Id]
                  AND rs.[ConsumerTenantId]     = @tenantId
                  AND rs.[IsolationLevel]       >= 3  -- TenantIsolationLevel.Write
              )
            )
          )
          AND [Regulation].[Created] <= @createdBefore
          AND (
            [Regulation].[ValidFrom] IS NULL
            OR [Regulation].[ValidFrom] <= @regulationDate
            )
          AND [PayrollLayer].[Status] = 0
          AND [PayrollLayer].[PayrollId] = @payrollId
        )
    SELECT *
    FROM GroupRegulation
    WHERE RowNumber = 1
    )
GO
