-- --------------------------------------------------------------------------------
-- Version.sql
-- Update Payroll Engine Database Version
-- --------------------------------------------------------------------------------

IF DB_NAME() <> 'PayrollEngine' BEGIN
  RAISERROR( 'Error: Wrong database, expecting PayrollEngine.', 16, 10 )
  RETURN
END

-- database version
DECLARE @errorID int
INSERT INTO [Version] (
	MajorVersion,
	MinorVersion,
	SubVersion,
	[Owner],
	[Description] )
VALUES (
	0,
	5,
	1,
	CURRENT_USER,
	'Payroll Engine: initial database setup' )
SET @errorID = @@ERROR
IF ( @errorID <> 0 ) BEGIN
	PRINT 'Error while updating the Payroll Engine database version.'
END
ELSE BEGIN
	PRINT 'Payroll Engine database version successfully updated to release 0.5.1'
END

