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
-- Get all active derived regulation ids from the payroll
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
          -- working tenant or shared regulation 
          AND (
            [Regulation].[TenantId] = @tenantId
            OR [Regulation].[SharedRegulation] = 1
            )
          AND [Regulation].[Created] < @createdBefore
          AND (
            [Regulation].[ValidFrom] IS NULL
            OR [Regulation].[ValidFrom] < @regulationDate
            )
          AND [PayrollLayer].[Status] = 0
          AND [PayrollLayer].[PayrollId] = @payrollId
        )
    SELECT *
    FROM GroupRegulation
    WHERE RowNumber = 1
    )
GO


