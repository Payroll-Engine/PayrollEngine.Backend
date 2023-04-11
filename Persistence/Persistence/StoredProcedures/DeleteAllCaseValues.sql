SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

IF EXISTS (
    SELECT *
    FROM sysobjects
    WHERE id = object_id(N'[dbo].[DeleteAllCaseValues]')
      AND OBJECTPROPERTY(id, N'IsProcedure') = 1
    )
BEGIN
  DROP PROCEDURE [dbo].[DeleteAllCaseValues]
END
GO

-- =============================================
-- Delete all case values
--	
CREATE PROCEDURE [dbo].[DeleteAllCaseValues]
AS
BEGIN
  -- SET NOCOUNT ON added to prevent extra result sets from
  -- interfering with SELECT statements
  SET NOCOUNT ON;

  EXEC [dbo].[DeleteAllGlobalCaseValues]

  EXEC [dbo].[DeleteAllNationalCaseValues]

  EXEC [dbo].[DeleteAllCompanyCaseValues]

  EXEC [dbo].[DeleteAllEmployeeCaseValues]
END
GO


