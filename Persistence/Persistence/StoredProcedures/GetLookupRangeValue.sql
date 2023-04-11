SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

IF EXISTS (
    SELECT *
    FROM sysobjects
    WHERE id = object_id(N'[dbo].[GetLookupRangeValue]')
      AND OBJECTPROPERTY(id, N'IsProcedure') = 1
    )
BEGIN
  DROP PROCEDURE [dbo].[GetLookupRangeValue]
END
GO

-- =============================================
-- Get all base payroll ids, starting from the derived regulation until the root regulation
--	
CREATE PROCEDURE [dbo].[GetLookupRangeValue]
  -- the lookup
  @lookupId AS INT,
  -- the range value
  @rangeValue AS DECIMAL(28, 6),
  -- the lookup value key hash
  @keyHash AS INT = NULL
AS
BEGIN
  -- SET NOCOUNT ON added to prevent extra result sets from
  -- interfering with SELECT statements
  SET NOCOUNT ON;

  DECLARE @rangeSize DECIMAL(28, 6)
  DECLARE @minValue DECIMAL(28, 6)
  DECLARE @maxValue DECIMAL(28, 6)

  -- get range data
  SELECT @rangeSize = [RangeSize]
  FROM dbo.[Lookup]
  WHERE dbo.[Lookup].[Id] = @lookupId

  IF (@rangeSize IS NULL)
  BEGIN
    SET @rangeSize = 0.0
  END

  -- get min/max values
  SELECT @minValue = MIN([RangeValue]),
    @maxValue = MAX([RangeValue]) + @rangeSize
  FROM dbo.[LookupValue]
  INNER JOIN [dbo].[Lookup]
    ON [dbo].[LookupValue].[LookupId] = [dbo].[Lookup].[Id]
  WHERE dbo.[Lookup].[Id] = @lookupId

  -- out of boundaries
  IF (@minValue IS NULL)
    OR (@rangeValue < @minValue)
    OR (@rangeValue > @maxValue)
  BEGIN
    RETURN NULL
  END

  -- select lookup value with the next smaller range value
  SELECT TOP 1 *
  FROM dbo.[LookupValue]
  INNER JOIN [dbo].[Lookup]
    ON [dbo].[LookupValue].[LookupId] = [dbo].[Lookup].[Id]
  WHERE dbo.[Lookup].[Id] = @lookupId
    AND dbo.[LookupValue].[RangeValue] <= @rangeValue
    AND (
      @keyHash IS NULL
      OR dbo.[LookupValue].[KeyHash] = @keyHash
      )
  ORDER BY dbo.[LookupValue].[RangeValue] DESC
END
GO


