SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

IF EXISTS (
    SELECT *
    FROM sysobjects
    WHERE id = object_id(N'[dbo].[GetEmployeeCaseValuesByTenant]')
      AND OBJECTPROPERTY(id, N'IsProcedure') = 1
    )
BEGIN
  DROP PROCEDURE dbo.[GetEmployeeCaseValuesByTenant]
END
GO

-- =============================================
-- Get employee case values for all active employees of a tenant at a point in time.
-- Single-pass JOIN query - no Global Temp Table, no N+1 per-employee calls.
-- Supports valueDate (Start/End filter), evaluationDate, fieldNames filter, forecast.
-- Uses: IX_Employee.TenantId + IX_EmployeeCaseValue.EmployeeId_Cover
-- =============================================
CREATE PROCEDURE dbo.[GetEmployeeCaseValuesByTenant]
  -- the tenant id
  @tenantId       AS INT,
  -- the value date: only values active at this date are returned (Start <= valueDate < End)
  @valueDate      AS DATETIME2(7) = NULL,
  -- the evaluation date: only values created on or before this date are returned
  @evaluationDate AS DATETIME2(7) = NULL,
  -- the case field names filter: JSON array of NVARCHAR(128), NULL = all fields
  @fieldNames     AS NVARCHAR(MAX) = NULL,
  -- the forecast name: NULL = real values only, name = real + forecast values
  @forecast       AS NVARCHAR(128) = NULL
AS
BEGIN
  SET NOCOUNT ON;

  SELECT
    ecv.[Id],
    ecv.[Status],
    ecv.[Created],
    ecv.[Updated],
    ecv.[EmployeeId],
    ecv.[DivisionId],
    ecv.[CaseName],
    ecv.[CaseNameLocalizations],
    ecv.[CaseFieldName],
    ecv.[CaseFieldNameLocalizations],
    ecv.[CaseSlot],
    ecv.[CaseSlotLocalizations],
    ecv.[ValueType],
    ecv.[Value],
    ecv.[NumericValue],
    ecv.[Culture],
    ecv.[CaseRelation],
    ecv.[CancellationDate],
    ecv.[Start],
    ecv.[End],
    ecv.[Forecast],
    ecv.[Tags],
    ecv.[Attributes]
  FROM dbo.[EmployeeCaseValue] ecv
  INNER JOIN dbo.[Employee] e
    ON e.[Id] = ecv.[EmployeeId]
  WHERE
    e.[TenantId] = @tenantId
    AND e.[Status] = 0
    AND ecv.[CancellationDate] IS NULL
    AND (@evaluationDate IS NULL OR ecv.[Created] <= @evaluationDate)
    AND (@valueDate IS NULL OR ecv.[Start] IS NULL OR ecv.[Start] <= @valueDate)
    AND (@valueDate IS NULL OR ecv.[End]   IS NULL OR ecv.[End]   >  @valueDate)
    AND (
      (@forecast IS NULL     AND ecv.[Forecast] IS NULL)
      OR (@forecast IS NOT NULL AND (ecv.[Forecast] IS NULL OR ecv.[Forecast] = @forecast))
    )
    AND (
      @fieldNames IS NULL
      OR ecv.[CaseFieldName] IN (SELECT [value] FROM OPENJSON(@fieldNames))
    )
  ORDER BY
    ecv.[EmployeeId] ASC,
    ecv.[CaseFieldName] ASC,
    ecv.[Created] DESC
END
GO
