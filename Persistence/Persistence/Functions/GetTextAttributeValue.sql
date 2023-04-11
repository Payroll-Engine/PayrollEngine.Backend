SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

IF EXISTS (
    SELECT *
    FROM sysobjects
    WHERE id = object_id(N'[dbo].[GetTextAttributeValue]')
      AND type = 'FN'
    )
BEGIN
  DROP FUNCTION [dbo].[GetTextAttributeValue]
END
GO

-- =============================================
-- Get a text value from JSON attributes
-- =============================================
CREATE FUNCTION [dbo].[GetTextAttributeValue] (
  -- the attributes json
  @attributes AS NVARCHAR(MAX),
  -- the attribute
  @name AS NVARCHAR(MAX)
  )
RETURNS NVARCHAR(MAX)
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

  RETURN IIF(@type = 1, @value, NULL);
END
GO


