SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

IF EXISTS (
    SELECT *
    FROM sysobjects
    WHERE id = object_id(N'[dbo].[GetDateAttributeValue]')
      AND type = 'FN'
    )
BEGIN
  DROP FUNCTION [dbo].[GetDateAttributeValue]
END
GO

-- =============================================
-- Get numeric value from JSON attributes
-- =============================================
CREATE FUNCTION [dbo].[GetDateAttributeValue] (
  -- the attributes json
  @attributes AS NVARCHAR(MAX),
  -- the attribute
  @name AS NVARCHAR(MAX)
  )
RETURNS DATETIME2(7)
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

  RETURN IIF(@type = 1, CAST(@value AS DATETIME2(7)), NULL);
END
GO


