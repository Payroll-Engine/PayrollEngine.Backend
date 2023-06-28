SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

IF EXISTS (
    SELECT *
    FROM sysobjects
    WHERE id = object_id(N'[dbo].[GetLocalizedValue]')
      AND type = 'FN'
    )
BEGIN
  DROP FUNCTION [dbo].[GetLocalizedValue]
END
GO

-- =============================================
-- Get the localized value from JSON localizations
-- =============================================
CREATE FUNCTION [dbo].[GetLocalizedValue] (
  -- the localizations
  @localizations AS NVARCHAR(MAX),
  -- the cultue
  @culture AS NVARCHAR(128),
  -- the fallback value
  @fallback AS NVARCHAR(MAX)
  )
RETURNS NVARCHAR(MAX)
AS
BEGIN
  DECLARE @value VARCHAR(MAX);

  SELECT @value = value
  FROM OPENJSON(@localizations)
  WHERE [key] = @culture;

  RETURN IIF(@value IS NULL, @fallback, @value);
END
GO


