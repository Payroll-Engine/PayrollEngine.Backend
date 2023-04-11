SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

IF EXISTS (
    SELECT *
    FROM sysobjects
    WHERE id = object_id(N'[dbo].[GetDerivedPayrollRegulations]')
      AND OBJECTPROPERTY(id, N'IsProcedure') = 1
    )
BEGIN
  DROP PROCEDURE dbo.[GetDerivedPayrollRegulations]
END
GO

-- =============================================
-- Get the derived regulations of payroll (only active).
-- =============================================
CREATE PROCEDURE dbo.[GetDerivedPayrollRegulations]
  -- the tenant
  @tenantId AS INT,
  -- the payroll
  @payrollId AS INT,
  -- the regulation valid from date
  @regulationDate AS DATETIME2(7),
  -- creation date
  @createdBefore AS DATETIME2(7)
AS
BEGIN
  -- SET NOCOUNT ON added to prevent extra result sets from
  -- interfering with SELECT statements
  SET NOCOUNT ON;

  -- select the derived regulations
  SELECT *
  FROM dbo.[GetDerivedRegulations](@tenantId, @payrollId, @regulationDate, @createdBefore)
  -- sort order
  ORDER BY [Level] DESC,
    [Priority] DESC
END
GO


