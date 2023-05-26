-- --------------------------------------------------------------------------------
-- CreateDefaultDatabase.sql
-- --------------------------------------------------------------------------------

-- select root
USE master
GO

-- check database
DECLARE @dbname nvarchar(128)
SET @dbname = N'PayrollEngine'
IF EXISTS ( SELECT name FROM master.dbo.sysdatabases WHERE name = N'PayrollEngine') BEGIN
  RAISERROR( 'Error: Database PayrollEngine already exists.', 16, 10 )
  RETURN
END

-- data file path
declare @defaultData nvarchar(512)
exec master.dbo.xp_instance_regread N'HKEY_LOCAL_MACHINE', N'Software\Microsoft\MSSQLServer\MSSQLServer', N'DefaultData', @defaultData output
declare @masterData nvarchar(512)
exec master.dbo.xp_instance_regread N'HKEY_LOCAL_MACHINE', N'Software\Microsoft\MSSQLServer\MSSQLServer\Parameters', N'SqlArg0', @masterData output
select @masterData=substring(@masterData, 3, 255)
select @masterData=substring(@masterData, 1, len(@masterData) - charindex('\', reverse(@masterData)))
declare @dataFileName nvarchar(MAX)
SET @dataFileName = CONCAT(ISNULL(@defaultData, @masterData), '\PayrollEngine.mdb')
print @dataFileName

-- log file path
declare @defaultLog nvarchar(512)
exec master.dbo.xp_instance_regread N'HKEY_LOCAL_MACHINE', N'Software\Microsoft\MSSQLServer\MSSQLServer', N'DefaultLog', @defaultLog output
declare @masterLog nvarchar(512)
exec master.dbo.xp_instance_regread N'HKEY_LOCAL_MACHINE', N'Software\Microsoft\MSSQLServer\MSSQLServer\Parameters', N'SqlArg2', @masterLog output
select @masterLog=substring(@masterLog, 3, 255)
select @masterLog=substring(@masterLog, 1, len(@masterLog) - charindex('\', reverse(@masterLog)))
declare @logFileName nvarchar(MAX)
SET @logFileName = CONCAT(ISNULL(@defaultLog, @masterLog), '\PayrollEngine_log.ldf')
print @logFileName

-- create database
DECLARE @sql nvarchar(MAX)
DECLARE @error int
SET @error = -1;
SELECT @sql = 'CREATE DATABASE [PayrollEngine] ON PRIMARY ( ' +
			  'NAME = PayrollEngineData, ' +
              'FILENAME = N''' + @dataFileName + '''' +
			  ') LOG ON ( ' +
			  'NAME = PayrollEngineLog, ' +
			  'FILENAME = N''' + @logFileName + '''' +
			  ') ' + 
			  'COLLATE SQL_Latin1_General_CP1_CS_AS'
EXEC (@sql)
PRINT @sql
SET @error = @@ERROR
IF ( @error <> 0 ) BEGIN
	PRINT 'Error while updating the database version.'
END
ELSE BEGIN
	PRINT 'Database version successfully updated to release 1.0.9'
END

GO

-- test new database
IF NOT EXISTS ( SELECT name FROM master.dbo.sysdatabases WHERE name = N'PayrollEngine') BEGIN
  RAISERROR( 'Error: Database PayrollEngine not created.', 16, 10 )
  RETURN
END

-- use the new database
USE [PayrollEngine]

