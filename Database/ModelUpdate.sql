PRINT '**********************************************'
PRINT 'Payroll Engine Database Update 0.9.3 to 0.9.4.'
PRINT '**********************************************'

-- test version
DECLARE @major int
DECLARE @minor int
DECLARE @sub int
SELECT @major = MajorVersion,
  @minor = MinorVersion,
  @sub = SubVersion
FROM [Version]
IF (@major <> 0 OR @minor <> 9 OR @sub <> 3) BEGIN
	RAISERROR('Invalid database version. Required version is 0.9.3.', 16 ,1);
END

-- lookup range mode

/****** Object:  Table [dbo].[Lookup]    Script Date: 04.11.2025 09:42:19 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER TABLE [dbo].[Lookup]
  ADD [RangeMode] [int] NOT NULL DEFAULT 0

GO

/****** Object:  Table [dbo].[LookupAudit]    Script Date: 04.11.2025 09:42:19 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER TABLE [dbo].[LookupAudit]
  ADD [RangeMode] [int] NOT NULL DEFAULT 0
GO

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
	9,
	4,
	CURRENT_USER,
	'Payroll Engine: Full setup v0.9.4' )
SET @errorID = @@ERROR
IF ( @errorID <> 0 ) BEGIN
	PRINT 'Error while updating the Payroll Engine database version.'
END
ELSE BEGIN
	PRINT 'Payroll Engine database version successfully updated to release 0.9.4'
END
