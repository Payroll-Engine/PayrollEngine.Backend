SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

IF EXISTS (
    SELECT *
    FROM sysobjects
    WHERE id = object_id(N'[dbo].[GetDerivedScripts]')
      AND OBJECTPROPERTY(id, N'IsProcedure') = 1
    )
BEGIN
  DROP PROCEDURE dbo.[GetDerivedScripts]
END
GO

-- =============================================
-- Get the topmost derived scripts (only active).
-- =============================================
CREATE PROCEDURE dbo.[GetDerivedScripts]
  -- the tenant
  @tenantId AS INT,
  -- the payroll
  @payrollId AS INT,
  -- the regulation valid from date
  @regulationDate AS DATETIME2(7),
  -- creation date
  @createdBefore AS DATETIME2(7),
  -- the script names: JSON array of VARCHAR(128)
  @scriptNames AS VARCHAR(MAX) = NULL
AS
BEGIN
  -- SET NOCOUNT ON added to prevent extra result sets from
  -- interfering with SELECT statements
  SET NOCOUNT ON;

  -- select all active scripts, using the order of script name and payroll level
  SELECT [Regulations].[Id] AS [RegulationId],
    [Regulations].[Level] AS [Level],
    [Regulations].[Priority] AS [Priority],
    -- perfomance hint: don't use [Script].*
    dbo.[Script].[Id],
    dbo.[Script].[Status],
    dbo.[Script].[Created],
    dbo.[Script].[Updated],
    dbo.[Script].[RegulationId],
    dbo.[Script].[Name],
    dbo.[Script].[FunctionTypeMask],
    dbo.[Script].[Value]
  FROM dbo.[Script]
  INNER JOIN dbo.GetDerivedRegulations(@tenantId, @payrollId, @regulationDate, @createdBefore) AS [Regulations]
    ON dbo.[Script].[RegulationId] = [Regulations].[Id]
  -- active scripts only
  WHERE dbo.[Script].[Status] = 0
    AND dbo.[Script].[Created] < @createdBefore
    AND (
      @scriptNames IS NULL
      OR dbo.[Script].[Name] IN (
        SELECT value
        FROM OPENJSON(@scriptNames)
        )
      )
  -- derived order by sort order
  ORDER BY [Level] DESC,
    [Priority] DESC
END
GO


