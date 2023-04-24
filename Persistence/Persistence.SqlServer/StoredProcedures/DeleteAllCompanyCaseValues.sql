SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

IF EXISTS (
    SELECT *
    FROM sysobjects
    WHERE id = object_id(N'[dbo].[DeleteAllCompanyCaseValues]')
      AND OBJECTPROPERTY(id, N'IsProcedure') = 1
    )
BEGIN
  DROP PROCEDURE [dbo].[DeleteAllCompanyCaseValues]
END
GO

-- =============================================
-- Delete all company case values
--	
CREATE PROCEDURE [dbo].[DeleteAllCompanyCaseValues]
AS
BEGIN
  -- SET NOCOUNT ON added to prevent extra result sets from
  -- interfering with SELECT statements
  SET NOCOUNT ON;

  DELETE FROM [dbo].[CompanyCaseValueChange]
  DELETE FROM [dbo].[CompanyCaseDocument]
  DELETE FROM [dbo].[CompanyCaseValue]
  DELETE FROM [dbo].[CompanyCaseChange]
END
GO


