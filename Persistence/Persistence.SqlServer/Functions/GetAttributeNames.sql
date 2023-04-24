SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

IF EXISTS (
    SELECT *
    FROM sysobjects
    WHERE id = object_id(N'[dbo].[GetAttributeNames]')
      AND type = 'FN'
    )
BEGIN
  DROP FUNCTION [dbo].[GetAttributeNames]
END
GO

-- =============================================
-- Get the attribute names from JSON
-- =============================================
CREATE FUNCTION [dbo].[GetAttributeNames] (
  -- the attributes
  @attributes AS NVARCHAR(MAX)
  )
RETURNS NVARCHAR(MAX)
AS
BEGIN
    DECLARE @sql AS NVARCHAR(MAX);
    DECLARE @value VARCHAR(128);
    DECLARE @index INT;
    DECLARE @count INT;

    SET @sql = '';
    SELECT @count = COUNT(*) FROM OPENJSON(@attributes);
    IF (@count = 0)
    BEGIN
        RETURN @sql;
    END

    -- foreach attribute
    SET @index = 0;
    WHILE (@index < @count)
    BEGIN
        SELECT
            @value = value
        FROM
            OPENJSON(@attributes)
        WHERE
            [key] = @index;

        IF (LEN(@value) > 0)
        BEGIN
            -- concat attribute names
            SET @sql = @sql + @value;

            -- separater between multiple attributes
            IF (@index <> @count - 1)
            BEGIN
                SET @sql = @sql + ', ';
            END
        END

        SET @index = @index + 1
    END

    IF (LEN(@sql) > 0)
    BEGIN
        SET @sql = ',' + @sql + '
        ';
    END

    RETURN @sql;
END
GO


