SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

IF EXISTS (
    SELECT *
    FROM sysobjects
    WHERE id = object_id(N'[dbo].[DeletePayrunJob]')
      AND OBJECTPROPERTY(id, N'IsProcedure') = 1
    )
BEGIN
  DROP PROCEDURE [dbo].[DeletePayrunJob]
END
GO

-- =============================================
-- Detete payrun job including all his related objects
--	
CREATE PROCEDURE [dbo].[DeletePayrunJob]
  -- the tenant
  @tenantId AS INT,
  -- the payrun job to delete
  @payrunJobId AS INT
AS
BEGIN
  -- SET NOCOUNT ON added to prevent extra result sets from
  -- interfering with SELECT statements
  SET NOCOUNT ON;

  -- transaction start
  BEGIN TRANSACTION;

  SAVE TRANSACTION DeletePayrunJobTransaction;

  BEGIN TRY

    -- payrun results
    DELETE [dbo].[PayrunResult]
    FROM [dbo].[PayrunResult]
    INNER JOIN [dbo].[PayrollResult]
      ON [dbo].[PayrunResult].[PayrollResultId] = [dbo].[PayrollResult].[Id]
    WHERE
        @tenantId = @tenantId 
      AND
        [dbo].[PayrollResult].[PayrunJobId] = @payrunJobId

    -- wage type custom results
    DELETE [dbo].[WageTypeCustomResult]
    FROM [dbo].[WageTypeCustomResult]
    INNER JOIN [dbo].[WageTypeResult]
      ON [dbo].[WageTypeCustomResult].[WageTypeResultId] = [dbo].[WageTypeResult].[Id]
    INNER JOIN [dbo].[PayrollResult]
      ON [dbo].[WageTypeResult].[PayrollResultId] = [dbo].[PayrollResult].[Id]
    WHERE
        @tenantId = @tenantId 
      AND
        [dbo].[PayrollResult].[PayrunJobId] = @payrunJobId

    -- wage type results
    DELETE [dbo].[WageTypeResult]
    FROM [dbo].[WageTypeResult]
    INNER JOIN [dbo].[PayrollResult]
      ON [dbo].[WageTypeResult].[PayrollResultId] = [dbo].[PayrollResult].[Id]
    WHERE
        @tenantId = @tenantId 
      AND
        [dbo].[PayrollResult].[PayrunJobId] = @payrunJobId

    -- collector custom results
    DELETE [dbo].[CollectorCustomResult]
    FROM [dbo].[CollectorCustomResult]
    INNER JOIN [dbo].[CollectorResult]
      ON [dbo].[CollectorCustomResult].[CollectorResultId] = [dbo].[CollectorResult].[Id]
    INNER JOIN [dbo].[PayrollResult]
      ON [dbo].[CollectorResult].[PayrollResultId] = [dbo].[PayrollResult].[Id]
    WHERE
        @tenantId = @tenantId 
      AND
        [dbo].[PayrollResult].[PayrunJobId] = @payrunJobId

    -- collector results
    DELETE [dbo].[CollectorResult]
    FROM [dbo].[CollectorResult]
    INNER JOIN [dbo].[PayrollResult]
      ON [dbo].[CollectorResult].[PayrollResultId] = [dbo].[PayrollResult].[Id]
    WHERE
        @tenantId = @tenantId 
      AND
        [dbo].[PayrollResult].[PayrunJobId] = @payrunJobId

    -- payroll results
    DELETE
    FROM [dbo].[PayrollResult]
    WHERE
        @tenantId = @tenantId 
      AND
        [dbo].[PayrollResult].[PayrunJobId] = @payrunJobId

    -- payrun job emplyoee
    DELETE [dbo].[PayrunJobEmployee]
    FROM [dbo].[PayrunJobEmployee]
    WHERE
        @tenantId = @tenantId 
      AND
        [dbo].[PayrunJobEmployee].[PayrunJobId] = @payrunJobId

    -- payrun job
    DELETE [dbo].[PayrunJob]
    FROM [dbo].[PayrunJob]
    WHERE
        @tenantId = @tenantId 
      AND
        [dbo].[PayrunJob].[Id] = @payrunJobId

    -- transaction end
    COMMIT TRANSACTION;

    -- success
    RETURN 1
  END TRY

  BEGIN CATCH
    IF @@TRANCOUNT > 0
    BEGIN
      ROLLBACK TRANSACTION DeletePayrunJobTransaction;
    END

    -- failure
    RETURN 0
  END CATCH
END
GO


