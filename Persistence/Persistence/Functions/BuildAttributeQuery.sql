SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

IF EXISTS (
    SELECT *
    FROM sysobjects
    WHERE id = object_id(N'[dbo].[BuildAttributeQuery]')
      AND type = 'FN'
    )
BEGIN
  DROP FUNCTION [dbo].[BuildAttributeQuery]
END
GO

-- =============================================
-- Buld the attribute query
-- =============================================
CREATE FUNCTION [dbo].[BuildAttributeQuery] (
  -- the attribute field name, NULL is supported
  @attributeField AS NVARCHAR(MAX),
  -- the attribute name as JSON
  @attributes AS NVARCHAR(MAX) = NULL
  )
RETURNS NVARCHAR(MAX)
AS
BEGIN
    -- the query sql
    DECLARE @sql AS NVARCHAR(MAX);
    DECLARE @attributeSql AS NVARCHAR(MAX);
    DECLARE @attributeName VARCHAR(128);
    DECLARE @index INT;
    DECLARE @count INT;

    SET @sql = '';
    IF (@attributes IS NULL)
    BEGIN
        RETURN @sql;
    END

    SELECT @count = COUNT(*) FROM OPENJSON(@attributes);
    IF (@count= 0)
    BEGIN
        RETURN @sql;
    END

    -- foreach attribute
    SELECT @index = 0;
    WHILE (@index < @count)
    BEGIN
        SELECT
            @attributeName = value
        FROM 
            OPENJSON(@attributes)
        WHERE
            [key] = @index;

        IF (@attributeField IS NULL)
        BEGIN
            SET @attributeSql = '
            NULL AS ' + @attributeName;
        END
        ELSE IF (LEN(@attributeName) > 0)
        BEGIN
            -- text attribute sql
            IF (SUBSTRING(@attributeName, 1, 3) = 'TA_')
            BEGIN
                SET @attributeSql = '
                dbo.GetTextAttributeValue(' + @attributeField + ', ''' +
                    REPLACE(@attributeName, N'TA_', '') + ''') AS ' + @attributeName;
            END
            -- date attribute sql
            IF (SUBSTRING(@attributeName, 1, 3) = 'DA_')
            BEGIN
                SET @attributeSql = '
                dbo.GetDateAttributeValue(' + @attributeField + ', ''' +
                    REPLACE(@attributeName, N'DA_', '') + ''') AS ' + @attributeName;
            END
            -- numeric attribute sql
            ELSE IF (SUBSTRING(@attributeName, 1, 3) = 'NA_')
            BEGIN
                SET @attributeSql = '
                dbo.GetNumericAttributeValue(' + @attributeField + ', ''' +
                    REPLACE(@attributeName, N'NA_', '') + ''') AS ' + @attributeName;
            END
        END

        -- concat sql statement
        SET @sql = @sql + @attributeSql;

        -- separater between multiple attributes
        IF (@index <> @count - 1)
        BEGIN
            SET @sql = @sql + ', ';
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


