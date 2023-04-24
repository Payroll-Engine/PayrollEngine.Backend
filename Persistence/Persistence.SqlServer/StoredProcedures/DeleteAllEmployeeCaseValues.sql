SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

IF EXISTS (
    SELECT *
    FROM sysobjects
    WHERE id = object_id(N'[dbo].[DeleteAllEmployeeCaseValues]')
      AND OBJECTPROPERTY(id, N'IsProcedure') = 1
    )
BEGIN
  DROP PROCEDURE [dbo].[DeleteAllEmployeeCaseValues]
END
GO

-- =============================================
-- Delete all employee case values
--	
CREATE PROCEDURE [dbo].[DeleteAllEmployeeCaseValues]
AS
BEGIN
  -- SET NOCOUNT ON added to prevent extra result sets from
  -- interfering with SELECT statements
  SET NOCOUNT ON;

  DELETE FROM [dbo].[EmployeeCaseValueChange]
  DELETE FROM [dbo].[EmployeeCaseDocument]
  DELETE FROM [dbo].[EmployeeCaseValue]
  DELETE FROM [dbo].[EmployeeCaseChange]
END
GO


