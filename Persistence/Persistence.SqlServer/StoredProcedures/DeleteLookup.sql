SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

IF EXISTS (
    SELECT *
    FROM sysobjects
    WHERE id = object_id(N'[dbo].[DeleteLookup]')
      AND OBJECTPROPERTY(id, N'IsProcedure') = 1
    )
BEGIN
  DROP PROCEDURE [dbo].[DeleteLookup]
END
GO

-- =============================================
-- Detete lookup including the lookup audit
--	
CREATE PROCEDURE [dbo].[DeleteLookup]
  -- the tenant with the lookup
  @tenantId AS INT,
  -- the p to delete
  @lookupId AS INT
AS
BEGIN
  -- SET NOCOUNT ON added to prevent extra result sets from
  -- interfering with SELECT statements
  SET NOCOUNT ON;
  SET XACT_ABORT ON;

  -- transaction start
  BEGIN TRANSACTION;
  SAVE TRANSACTION DeleteLookupTransaction;

  BEGIN TRY

    -- lookup
    DELETE [dbo].[LookupValueAudit]
    FROM [dbo].[LookupValueAudit]
    INNER JOIN [dbo].[LookupValue]
      ON [dbo].[LookupValueAudit].[LookupValueId] = [dbo].[LookupValue].[Id]
    INNER JOIN [dbo].[Lookup]
      ON [dbo].[LookupValue].[LookupId] = [dbo].[Lookup].[Id]
    INNER JOIN [dbo].[Regulation]
      ON [dbo].[Lookup].[RegulationId] = [dbo].[Regulation].[Id]
    WHERE [dbo].[Regulation].[TenantId] = @tenantId AND
          [dbo].[Lookup].Id = @lookupId 

    DELETE [dbo].[LookupValue]
    FROM [dbo].[LookupValue]
    INNER JOIN [dbo].[Lookup]
      ON [dbo].[LookupValue].[LookupId] = [dbo].[Lookup].[Id]
    INNER JOIN [dbo].[Regulation]
      ON [dbo].[Lookup].[RegulationId] = [dbo].[Regulation].[Id]
    WHERE [dbo].[Regulation].[TenantId] = @tenantId AND
          [dbo].[Lookup].Id = @lookupId 

    DELETE [dbo].[LookupAudit]
    FROM [dbo].[LookupAudit]
    INNER JOIN [dbo].[Lookup]
      ON [dbo].[LookupAudit].[LookupId] = [dbo].[Lookup].[Id]
    INNER JOIN [dbo].[Regulation]
      ON [dbo].[Lookup].[RegulationId] = [dbo].[Regulation].[Id]
    WHERE [dbo].[Regulation].[TenantId] = @tenantId AND
          [dbo].[Lookup].Id = @lookupId 

    DELETE [dbo].[Lookup]
    FROM [dbo].[Lookup]
    INNER JOIN [dbo].[Regulation]
      ON [dbo].[Lookup].[RegulationId] = [dbo].[Regulation].[Id]
    WHERE [dbo].[Regulation].[TenantId] = @tenantId AND
          [dbo].[Lookup].Id = @lookupId 

    -- transaction end
    COMMIT TRANSACTION;
    
    -- success
    RETURN 1
  END TRY

  BEGIN CATCH
    IF @@TRANCOUNT > 0
    BEGIN
      ROLLBACK TRANSACTION DeleteLookupTransaction;
    END
    
    -- failure
    RETURN 0
  END CATCH
END
GO


