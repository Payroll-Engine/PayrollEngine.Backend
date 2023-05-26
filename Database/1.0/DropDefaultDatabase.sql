-- test new database
IF NOT EXISTS ( SELECT name FROM master.dbo.sysdatabases WHERE name = N'PayrollEngine') BEGIN
  RAISERROR( 'Error: Missing database PayrollEngine.', 16, 10 )
  RETURN
END

DROP DATABASE [PayrollEngine]