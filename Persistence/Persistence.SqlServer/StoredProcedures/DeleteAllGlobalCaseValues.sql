SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

IF EXISTS (
    SELECT *
    FROM sysobjects
    WHERE id = object_id(N'[dbo].[DeleteAllGlobalCaseValues]')
      AND OBJECTPROPERTY(id, N'IsProcedure') = 1
    )
BEGIN
  DROP PROCEDURE [dbo].[DeleteAllGlobalCaseValues]
END
GO

-- =============================================
-- Delete all global case values
--	
CREATE PROCEDURE [dbo].[DeleteAllGlobalCaseValues]
AS
BEGIN
  -- SET NOCOUNT ON added to prevent extra result sets from
  -- interfering with SELECT statements
  SET NOCOUNT ON;

  DELETE FROM [dbo].[GlobalCaseValueChange]
  DELETE FROM [dbo].[GlobalCaseDocument]
  DELETE FROM [dbo].[GlobalCaseValue]
  DELETE FROM [dbo].[GlobalCaseChange]

END
GO


