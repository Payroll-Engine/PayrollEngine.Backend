SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

IF EXISTS (
    SELECT *
    FROM sysobjects
    WHERE id = object_id(N'[dbo].[GetNumericAttributeValue]')
      AND type = 'FN'
    )
BEGIN
  DROP FUNCTION [dbo].[GetNumericAttributeValue]
END
GO

-- =============================================
-- Get numeric value from JSON attributes
-- =============================================
CREATE FUNCTION [dbo].[GetNumericAttributeValue] (
  -- the attributes json
  @attributes AS NVARCHAR(MAX),
  -- the attribute
  @name AS NVARCHAR(MAX)
  )
RETURNS DECIMAL(28, 6)
AS
BEGIN
  DECLARE @type INT;
  DECLARE @value VARCHAR(MAX);

  SELECT
    @value = value,
    @type = type
  FROM 
    OPENJSON(@attributes)
  WHERE
    [key] = @name;

  RETURN IIF(@type = 2, CAST(@value AS DECIMAL(28, 6)), NULL);
END
GO


