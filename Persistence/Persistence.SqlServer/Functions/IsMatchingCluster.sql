SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

IF EXISTS (
    SELECT *
    FROM sysobjects
    WHERE id = object_id(N'[dbo].[IsMatchingCluster]')
      AND type = 'FN'
    )
BEGIN
  DROP FUNCTION [dbo].[IsMatchingCluster]
END
GO

-- =============================================
-- Test for matching include/exclude clusters
-- =============================================
CREATE FUNCTION [dbo].[IsMatchingCluster] (
  -- the include clusters: JSON array of clusters VARCHAR(128)
  @includeClusters AS VARCHAR(MAX) = NULL,
  -- the exclude clusters: JSON array of clusters VARCHAR(128)
  @excludeClusters AS VARCHAR(MAX) = NULL,
  -- the test clusters: JSON array of clusters VARCHAR(128)
  @testClusters AS VARCHAR(MAX) = NULL
  )
RETURNS bit
AS
BEGIN
    DECLARE @value VARCHAR(128);
    DECLARE @index INT;
    DECLARE @count INT;

    DECLARE @testCount INT;
    SELECT @testCount = COUNT(*) FROM OPENJSON(@testClusters);

    IF (@includeClusters IS NOT NULL OR @excludeClusters IS NOT NULL)
    BEGIN
        -- include clusters
        DECLARE @includeCount INT;
        SELECT @includeCount = COUNT(*) FROM OPENJSON(@includeClusters);
        IF (@includeCount > 0)
        BEGIN
            SET @index = 0;
            WHILE (@index < @includeCount)
            BEGIN
                SELECT
                    @value = value
                FROM
                    OPENJSON(@includeClusters)
                WHERE
                    [key] = @index;

                IF (LEN(@value) > 0)
                BEGIN
                     SELECT @count = COUNT(*)
                        FROM OPENJSON(@testClusters)
                        WHERE value = @value;
                    IF (@count = 0)
                    BEGIN
                        -- missing include cluster
                        RETURN 0;
                    END
                END
                SET @index = @index + 1
            END
        END

        -- exclude clusters
        DECLARE @excludeCount INT;
        SELECT @excludeCount = COUNT(*) FROM OPENJSON(@excludeClusters);
        IF (@excludeCount > 0)
        BEGIN
            SET @index = 0;
            WHILE (@index < @excludeCount)
            BEGIN
                SELECT
                    @value = value
                FROM
                    OPENJSON(@excludeClusters)
                WHERE
                    [key] = @index;

                IF (LEN(@value) > 0)
                BEGIN
                     SELECT @count = COUNT(*)
                        FROM OPENJSON(@testClusters)
                        WHERE value = @value;
                    IF (@count > 0)
                    BEGIN
                        -- present exclude cluster
                        RETURN 0;
                    END
                END
                SET @index = @index + 1
            END
        END
    END

    RETURN 1;
END
GO


