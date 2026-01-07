PRINT '**********************************************'
PRINT 'Payroll Engine Database Update 0.9.4 to 0.9.5.'
PRINT '**********************************************'

-- ****************************** update check ******************************
-- test version
DECLARE @major int
DECLARE @minor int
DECLARE @sub int
-- latest version
select TOP 1 
  @major = MajorVersion,
  @minor = MinorVersion,
  @sub = SubVersion
from (
  select MajorVersion, MinorVersion, SubVersion, 
         row_number() over (partition by Id order by MajorVersion desc, MinorVersion desc, MinorVersion desc) as rowNumber
  from [PayrollEngine].[dbo].[Version]
) AS lv
where rowNumber = 1
ORDER BY MajorVersion desc, MinorVersion desc, SubVersion desc
-- min version check
IF (@major <> 0 OR @minor <> 9 OR @sub <> 4) BEGIN
	RAISERROR('Invalid database version. Required version is 0.9.4.', 16 ,1);
END


-- ****************************** regulation namespace ******************************
ALTER TABLE [dbo].[Regulation] ADD 
  [Namespace] [nvarchar](128) NULL;
GO

-- ****************************** case field actions ******************************
ALTER TABLE [dbo].[CaseField] DROP COLUMN 
  [BuildActions],
  [ValidateActions];
GO

ALTER TABLE [dbo].[CaseFieldAudit] DROP COLUMN 
  [BuildActions], 
  [ValidateActions];
GO

-- ****************************** collector actions ******************************
ALTER TABLE [dbo].[Collector] ADD 
  [StartActions] [nvarchar](max) NULL,
  [ApplyActions] [nvarchar](max) NULL,
  [EndActions] [nvarchar](max) NULL;
GO

ALTER TABLE [dbo].[CollectorAudit] ADD
  [StartActions] [nvarchar](max) NULL,
  [ApplyActions] [nvarchar](max) NULL,
  [EndActions] [nvarchar](max) NULL;
GO


-- ****************************** derived collectors ******************************
DROP PROCEDURE [dbo].[GetDerivedCollectors]
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Get the topmost derived collectors (only active).
-- =============================================
CREATE PROCEDURE [dbo].[GetDerivedCollectors]
  -- the tenant
  @tenantId AS INT,
  -- the payroll
  @payrollId AS INT,
  -- the regulation valid from date
  @regulationDate AS DATETIME2(7),
  -- creation date
  @createdBefore AS DATETIME2(7),
  -- the collector names: JSON array of VARCHAR(128)
  @collectorNames AS VARCHAR(MAX) = NULL,
  -- the include clusters: JSON array of cluster names VARCHAR(128)
  @includeClusters AS VARCHAR(MAX) = NULL,
  -- the exclude clusters: JSON array of cluster names VARCHAR(128)
  @excludeClusters AS VARCHAR(MAX) = NULL
AS
BEGIN
  -- SET NOCOUNT ON added to prevent extra result sets from
  -- interfering with SELECT statements
  SET NOCOUNT ON;

  -- select all active collectors, using the order of collector name and payroll level
  SELECT [Regulations].[Id] AS [RegulationId],
    [Regulations].[Level] AS [Level],
    [Regulations].[Priority] AS [Priority],
    -- perfomance hint: don't use [Collector].*
    dbo.[Collector].[Id],
    dbo.[Collector].[Status],
    dbo.[Collector].[Created],
    dbo.[Collector].[Updated],
    dbo.[Collector].[RegulationId],
    dbo.[Collector].[Name],
    dbo.[Collector].[NameLocalizations],
    dbo.[Collector].[CollectMode],
    dbo.[Collector].[Negated],
    dbo.[Collector].[OverrideType],
    dbo.[Collector].[Culture],
    dbo.[Collector].[CollectorGroups],
    dbo.[Collector].[StartExpression],
    dbo.[Collector].[ApplyExpression],
    dbo.[Collector].[EndExpression],
    dbo.[Collector].[StartActions],
    dbo.[Collector].[ApplyActions],
    dbo.[Collector].[EndActions],
    dbo.[Collector].[Threshold],
    dbo.[Collector].[MinResult],
    dbo.[Collector].[MaxResult],
    --   dbo.[Collector].[Binary],
    dbo.[Collector].[ScriptHash],
    dbo.[Collector].[Attributes],
    dbo.[Collector].[Clusters]
  -- excluded columns
  --dbo.[Collector].[Script],
  --dbo.[Collector].[ScriptVersion]
  FROM dbo.[Collector]
  INNER JOIN dbo.GetDerivedRegulations(@tenantId, @payrollId, @regulationDate, @createdBefore) AS [Regulations]
    ON dbo.[Collector].[RegulationId] = [Regulations].[Id]
  -- active collectors only
  WHERE dbo.[Collector].[Status] = 0
    AND dbo.[Collector].[Created] < @createdBefore
    AND (
      (
        @includeClusters IS NULL
        AND @excludeClusters IS NULL
        )
      OR dbo.IsMatchingCluster(@includeClusters, @excludeClusters, dbo.[Collector].[Clusters]) = 1
      )
    AND (
      @collectorNames IS NULL
      OR LOWER(dbo.[Collector].[Name]) IN (
        SELECT LOWER(value)
        FROM OPENJSON(@collectorNames)
        )
      )
  -- derived order by collector name
  ORDER BY dbo.[Collector].[Name],
    [Level] DESC,
    [Priority] DESC
END
GO

-- ****************************** wage type actions ******************************
ALTER TABLE [dbo].[WageType] ADD
  [ValueActions] [nvarchar](max) NULL,
  [ResultActions] [nvarchar](max) NULL;
GO

ALTER TABLE [dbo].[WageTypeAudit] ADD
  [ValueActions] [nvarchar](max) NULL,
  [ResultActions] [nvarchar](max) NULL;
GO

-- ****************************** derived wage types ******************************
DROP PROCEDURE [dbo].[GetDerivedWageTypes]
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Get derived wage types (only active).
-- =============================================
CREATE PROCEDURE [dbo].[GetDerivedWageTypes]
  -- the tenant
  @tenantId AS INT,
  -- the payroll,
  @payrollId AS INT,
  -- the regulation valid from date
  @regulationDate AS DATETIME2(7),
  -- creation date
  @createdBefore AS DATETIME2(7),
  -- the wage type numbers: JSON array of DECIMAL(28, 6)
  @wageTypeNumbers AS VARCHAR(MAX) = NULL,
  -- the include clusters: JSON array of cluster names VARCHAR(128)
  @includeClusters AS VARCHAR(MAX) = NULL,
  -- the exclude clusters: JSON array of cluster names VARCHAR(128)
  @excludeClusters AS VARCHAR(MAX) = NULL
AS
BEGIN
  -- SET NOCOUNT ON added to prevent extra result sets from
  -- interfering with SELECT statements
  SET NOCOUNT ON;

  -- select using the wage type number and payroll level
  SELECT [Regulations].[Id] AS [RegulationId],
    [Regulations].[Level] AS [Level],
    [Regulations].[Priority] AS [Priority],
    -- perfomance hint: don't use [WageType].*
    dbo.[WageType].[Id],
    dbo.[WageType].[Status],
    dbo.[WageType].[Created],
    dbo.[WageType].[Updated],
    dbo.[WageType].[RegulationId],
    dbo.[WageType].[Name],
    dbo.[WageType].[NameLocalizations],
    dbo.[WageType].[WageTypeNumber],
    dbo.[WageType].[Description],
    dbo.[WageType].[DescriptionLocalizations],
    dbo.[WageType].[OverrideType],
    dbo.[WageType].[Calendar],
    dbo.[WageType].[Culture],
    dbo.[WageType].[Collectors],
    dbo.[WageType].[CollectorGroups],
    dbo.[WageType].[ValueExpression],
    dbo.[WageType].[ResultExpression],
    dbo.[WageType].[ValueActions],
    dbo.[WageType].[ResultActions],
    --  dbo.[WageType].[Binary],
    dbo.[WageType].[ScriptHash],
    dbo.[WageType].[Attributes],
    dbo.[WageType].[Clusters]
  -- excluded columns
  -- dbo.[WageType].[Script],
  -- dbo.[WageType].[ScriptVersion],
  FROM dbo.[WageType]
  INNER JOIN dbo.GetDerivedRegulations(@tenantId, @payrollId, @regulationDate, @createdBefore) AS [Regulations]
    ON dbo.[WageType].[RegulationId] = [Regulations].[Id]
  -- active wage types only
  WHERE dbo.[WageType].[Status] = 0
    AND dbo.[WageType].[Created] < @createdBefore
    AND (
      (
        @includeClusters IS NULL
        AND @excludeClusters IS NULL
        )
      OR dbo.IsMatchingCluster(@includeClusters, @excludeClusters, dbo.[WageType].[Clusters]) = 1
      )
    AND (
      @wageTypeNumbers IS NULL
      OR dbo.[WageType].[WageTypeNumber] IN (
        SELECT CAST(value AS DECIMAL(28, 6))
        FROM OPENJSON(@wageTypeNumbers)
        )
      )
  -- derived order by wage type number
  ORDER BY dbo.[WageType].[WageTypeNumber],
    [Level] DESC,
    [Priority] DESC
END
GO

-- ****************************** delete lookup ******************************
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
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
    WHERE [dbo].[Regulation].[TenantId] = @tenantId
      AND [dbo].[Lookup].Id = @lookupId

    DELETE [dbo].[LookupValue]
    FROM [dbo].[LookupValue]
    INNER JOIN [dbo].[Lookup]
      ON [dbo].[LookupValue].[LookupId] = [dbo].[Lookup].[Id]
    INNER JOIN [dbo].[Regulation]
      ON [dbo].[Lookup].[RegulationId] = [dbo].[Regulation].[Id]
    WHERE [dbo].[Regulation].[TenantId] = @tenantId
      AND [dbo].[Lookup].Id = @lookupId

    DELETE [dbo].[LookupAudit]
    FROM [dbo].[LookupAudit]
    INNER JOIN [dbo].[Lookup]
      ON [dbo].[LookupAudit].[LookupId] = [dbo].[Lookup].[Id]
    INNER JOIN [dbo].[Regulation]
      ON [dbo].[Lookup].[RegulationId] = [dbo].[Regulation].[Id]
    WHERE [dbo].[Regulation].[TenantId] = @tenantId
      AND [dbo].[Lookup].Id = @lookupId

    DELETE [dbo].[Lookup]
    FROM [dbo].[Lookup]
    INNER JOIN [dbo].[Regulation]
      ON [dbo].[Lookup].[RegulationId] = [dbo].[Regulation].[Id]
    WHERE [dbo].[Regulation].[TenantId] = @tenantId
      AND [dbo].[Lookup].Id = @lookupId

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

-- ****************************** version ******************************
CREATE UNIQUE NONCLUSTERED INDEX [IX_UniqueVersion] ON [dbo].[Version] (
  [MajorVersion] ASC,
  [MinorVersion] ASC,
  [SubVersion] ASC
  )
  WITH (
      PAD_INDEX = OFF,
      STATISTICS_NORECOMPUTE = OFF,
      SORT_IN_TEMPDB = OFF,
      IGNORE_DUP_KEY = OFF,
      DROP_EXISTING = OFF,
      ONLINE = OFF,
      ALLOW_ROW_LOCKS = ON,
      ALLOW_PAGE_LOCKS = ON
      ) ON [PRIMARY]
GO

-- update database version
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
	5,
	CURRENT_USER,
	'Payroll Engine: Update v0.9.4 to v0.9.5' )
SET @errorID = @@ERROR
IF ( @errorID <> 0 ) BEGIN
	PRINT 'Error while updating the Payroll Engine database version.'
END
ELSE BEGIN
	PRINT 'Payroll Engine database version successfully updated to release 0.9.5'
END
