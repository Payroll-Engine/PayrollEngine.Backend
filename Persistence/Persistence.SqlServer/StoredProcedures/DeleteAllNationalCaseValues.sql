SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

IF EXISTS (
    SELECT *
    FROM sysobjects
    WHERE id = object_id(N'[dbo].[DeleteAllNationalCaseValues]')
      AND OBJECTPROPERTY(id, N'IsProcedure') = 1
    )
BEGIN
  DROP PROCEDURE [dbo].[DeleteAllNationalCaseValues]
END
GO

-- =============================================
-- Delete all national case values
--	
CREATE PROCEDURE [dbo].[DeleteAllNationalCaseValues]
AS
BEGIN
  -- SET NOCOUNT ON added to prevent extra result sets from
  -- interfering with SELECT statements
  SET NOCOUNT ON;

  DELETE FROM [dbo].[NationalCaseValueChange]
  DELETE FROM [dbo].[NationalCaseDocument]
  DELETE FROM [dbo].[NationalCaseValue]
  DELETE FROM [dbo].[NationalCaseChange]

END
GO


