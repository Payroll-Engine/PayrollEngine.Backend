SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

IF EXISTS (
    SELECT *
    FROM sysobjects
    WHERE id = object_id(N'[dbo].[DeleteTenant]')
      AND OBJECTPROPERTY(id, N'IsProcedure') = 1
    )
BEGIN
  DROP PROCEDURE [dbo].[DeleteTenant]
END
GO

-- =============================================
-- Detete tenant including all his related objects
--	
CREATE PROCEDURE [dbo].[DeleteTenant]
  -- the tenant to delete
  @tenantId AS INT
AS
BEGIN
  -- SET NOCOUNT ON added to prevent extra result sets from
  -- interfering with SELECT statements
  SET NOCOUNT ON;
  SET XACT_ABORT ON;

  -- transaction start
  BEGIN TRANSACTION;
  SAVE TRANSACTION DeleteTenantTransaction;

  BEGIN TRY
    -- payroll results
    DELETE [dbo].[PayrunResult]
    FROM [dbo].[PayrunResult]
    INNER JOIN [dbo].[PayrollResult]
      ON [dbo].[PayrunResult].[PayrollResultId] = [dbo].[PayrollResult].[Id]
    WHERE [dbo].[PayrollResult].[TenantId] = @tenantId

    DELETE [dbo].[WageTypeCustomResult]
    FROM [dbo].[WageTypeCustomResult]
    INNER JOIN [dbo].[WageTypeResult]
      ON [dbo].[WageTypeCustomResult].[WageTypeResultId] = [dbo].[WageTypeResult].[Id]
    INNER JOIN [dbo].[PayrollResult]
      ON [dbo].[WageTypeResult].[PayrollResultId] = [dbo].[PayrollResult].[Id]
    WHERE [dbo].[PayrollResult].[TenantId] = @tenantId

    DELETE [dbo].[WageTypeResult]
    FROM [dbo].[WageTypeResult]
    INNER JOIN [dbo].[PayrollResult]
      ON [dbo].[WageTypeResult].[PayrollResultId] = [dbo].[PayrollResult].[Id]
    WHERE [dbo].[PayrollResult].[TenantId] = @tenantId

    DELETE [dbo].[CollectorCustomResult]
    FROM [dbo].[CollectorCustomResult]
    INNER JOIN [dbo].[CollectorResult]
      ON [dbo].[CollectorCustomResult].[CollectorResultId] = [dbo].[CollectorResult].[Id]
    INNER JOIN [dbo].[PayrollResult]
      ON [dbo].[CollectorResult].[PayrollResultId] = [dbo].[PayrollResult].[Id]
    WHERE [dbo].[PayrollResult].[TenantId] = @tenantId

    DELETE [dbo].[CollectorResult]
    FROM [dbo].[CollectorResult]
    INNER JOIN [dbo].[PayrollResult]
      ON [dbo].[CollectorResult].[PayrollResultId] = [dbo].[PayrollResult].[Id]
    WHERE [dbo].[PayrollResult].[TenantId] = @tenantId

    DELETE
    FROM [dbo].[PayrollResult]
    WHERE [TenantId] = @tenantId

    -- payrun with jobs
    DELETE [dbo].[PayrunJobEmployee]
    FROM [dbo].[PayrunJobEmployee]
    INNER JOIN [dbo].[PayrunJob]
      ON [dbo].[PayrunJobEmployee].[PayrunJobId] = [dbo].[PayrunJob].[Id]
    WHERE [dbo].[PayrunJob].[TenantId] = @tenantId

    DELETE
    FROM [dbo].[PayrunJob]
    WHERE [TenantId] = @tenantId

    DELETE [dbo].[PayrunParameter]
    FROM [dbo].[PayrunParameter]
    INNER JOIN [dbo].[Payrun]
      ON [dbo].[PayrunParameter].[PayrunId] = [dbo].[Payrun].[Id]
    WHERE [dbo].[Payrun].[TenantId] = @tenantId

    DELETE
    FROM [dbo].[Payrun]
    WHERE [TenantId] = @tenantId

    -- payroll with payroll layers
    DELETE [dbo].[PayrollLayer]
    FROM [dbo].[PayrollLayer]
    INNER JOIN [dbo].[Payroll]
      ON [dbo].[PayrollLayer].[PayrollId] = [dbo].[Payroll].[Id]
    WHERE [dbo].[Payroll].[TenantId] = @tenantId

    DELETE
    FROM [dbo].[Payroll]
    WHERE [TenantId] = @tenantId

    -- regulation permissions
    DELETE [dbo].[RegulationPermission]
    FROM [dbo].[RegulationPermission]
    WHERE [dbo].[RegulationPermission].[PermissionTenantId] = @tenantId
      OR [dbo].[RegulationPermission].[TenantId] = @tenantId

    -- regulation
    DELETE [dbo].[ReportTemplateAudit]
    FROM [dbo].[ReportTemplateAudit]
    INNER JOIN [dbo].[ReportTemplate]
      ON [dbo].[ReportTemplateAudit].[ReportTemplateId] = [dbo].[ReportTemplate].[Id]
    INNER JOIN [dbo].[Report]
      ON [dbo].[ReportTemplate].[ReportId] = [dbo].[Report].[Id]
    INNER JOIN [dbo].[Regulation]
      ON [dbo].[Report].[RegulationId] = [dbo].[Regulation].[Id]
    WHERE [dbo].[Regulation].[TenantId] = @tenantId

    DELETE [dbo].[ReportTemplate]
    FROM [dbo].[ReportTemplate]
    INNER JOIN [dbo].[Report]
      ON [dbo].[ReportTemplate].[ReportId] = [dbo].[Report].[Id]
    INNER JOIN [dbo].[Regulation]
      ON [dbo].[Report].[RegulationId] = [dbo].[Regulation].[Id]
    WHERE [dbo].[Regulation].[TenantId] = @tenantId

    DELETE [dbo].[ReportParameterAudit]
    FROM [dbo].[ReportParameterAudit]
    INNER JOIN [dbo].[ReportParameter]
      ON [dbo].[ReportParameterAudit].[ReportParameterId] = [dbo].[ReportParameter].[Id]
    INNER JOIN [dbo].[Report]
      ON [dbo].[ReportParameter].[ReportId] = [dbo].[Report].[Id]
    INNER JOIN [dbo].[Regulation]
      ON [dbo].[Report].[RegulationId] = [dbo].[Regulation].[Id]
    WHERE [dbo].[Regulation].[TenantId] = @tenantId

    DELETE [dbo].[ReportParameter]
    FROM [dbo].[ReportParameter]
    INNER JOIN [dbo].[Report]
      ON [dbo].[ReportParameter].[ReportId] = [dbo].[Report].[Id]
    INNER JOIN [dbo].[Regulation]
      ON [dbo].[Report].[RegulationId] = [dbo].[Regulation].[Id]
    WHERE [dbo].[Regulation].[TenantId] = @tenantId

    DELETE [dbo].[ReportAudit]
    FROM [dbo].[ReportAudit]
    INNER JOIN [dbo].[Report]
      ON [dbo].[ReportAudit].[ReportId] = [dbo].[Report].[Id]
    INNER JOIN [dbo].[Regulation]
      ON [dbo].[Report].[RegulationId] = [dbo].[Regulation].[Id]
    WHERE [dbo].[Regulation].[TenantId] = @tenantId

    DELETE [dbo].[Report]
    FROM [dbo].[Report]
    INNER JOIN [dbo].[Regulation]
      ON [dbo].[Report].[RegulationId] = [dbo].[Regulation].[Id]
    WHERE [dbo].[Regulation].[TenantId] = @tenantId

    DELETE [dbo].[ScriptAudit]
    FROM [dbo].[ScriptAudit]
    INNER JOIN [dbo].[Script]
      ON [dbo].[ScriptAudit].[ScriptId] = [dbo].[Script].[Id]
    INNER JOIN [dbo].[Regulation]
      ON [dbo].[Script].[RegulationId] = [dbo].[Regulation].[Id]
    WHERE [dbo].[Regulation].[TenantId] = @tenantId

    DELETE [dbo].[Script]
    FROM [dbo].[Script]
    INNER JOIN [dbo].[Regulation]
      ON [dbo].[Script].[RegulationId] = [dbo].[Regulation].[Id]
    WHERE [dbo].[Regulation].[TenantId] = @tenantId

    DELETE [dbo].[LookupValueAudit]
    FROM [dbo].[LookupValueAudit]
    INNER JOIN [dbo].[LookupValue]
      ON [dbo].[LookupValueAudit].[LookupValueId] = [dbo].[LookupValue].[Id]
    INNER JOIN [dbo].[Lookup]
      ON [dbo].[LookupValue].[LookupId] = [dbo].[Lookup].[Id]
    INNER JOIN [dbo].[Regulation]
      ON [dbo].[Lookup].[RegulationId] = [dbo].[Regulation].[Id]
    WHERE [dbo].[Regulation].[TenantId] = @tenantId

    DELETE [dbo].[LookupValue]
    FROM [dbo].[LookupValue]
    INNER JOIN [dbo].[Lookup]
      ON [dbo].[LookupValue].[LookupId] = [dbo].[Lookup].[Id]
    INNER JOIN [dbo].[Regulation]
      ON [dbo].[Lookup].[RegulationId] = [dbo].[Regulation].[Id]
    WHERE [dbo].[Regulation].[TenantId] = @tenantId

    DELETE [dbo].[LookupAudit]
    FROM [dbo].[LookupAudit]
    INNER JOIN [dbo].[Lookup]
      ON [dbo].[LookupAudit].[LookupId] = [dbo].[Lookup].[Id]
    INNER JOIN [dbo].[Regulation]
      ON [dbo].[Lookup].[RegulationId] = [dbo].[Regulation].[Id]
    WHERE [dbo].[Regulation].[TenantId] = @tenantId

    DELETE [dbo].[Lookup]
    FROM [dbo].[Lookup]
    INNER JOIN [dbo].[Regulation]
      ON [dbo].[Lookup].[RegulationId] = [dbo].[Regulation].[Id]
    WHERE [dbo].[Regulation].[TenantId] = @tenantId

    DELETE [dbo].[CollectorAudit]
    FROM [dbo].[CollectorAudit]
    INNER JOIN [dbo].[Collector]
      ON [dbo].[CollectorAudit].[CollectorId] = [dbo].[Collector].[Id]
    INNER JOIN [dbo].[Regulation]
      ON [dbo].[Collector].[RegulationId] = [dbo].[Regulation].[Id]
    WHERE [dbo].[Regulation].[TenantId] = @tenantId

    DELETE [dbo].[Collector]
    FROM [dbo].[Collector]
    INNER JOIN [dbo].[Regulation]
      ON [dbo].[Collector].[RegulationId] = [dbo].[Regulation].[Id]
    WHERE [dbo].[Regulation].[TenantId] = @tenantId

    DELETE [dbo].[WageTypeAudit]
    FROM [dbo].[WageTypeAudit]
    INNER JOIN [dbo].[WageType]
      ON [dbo].[WageTypeAudit].[WageTypeId] = [dbo].[WageType].[Id]
    INNER JOIN [dbo].[Regulation]
      ON [dbo].[WageType].[RegulationId] = [dbo].[Regulation].[Id]
    WHERE [dbo].[Regulation].[TenantId] = @tenantId

    DELETE [dbo].[WageType]
    FROM [dbo].[WageType]
    INNER JOIN [dbo].[Regulation]
      ON [dbo].[WageType].[RegulationId] = [dbo].[Regulation].[Id]
    WHERE [dbo].[Regulation].[TenantId] = @tenantId

    DELETE [dbo].[CaseRelationAudit]
    FROM [dbo].[CaseRelationAudit]
    INNER JOIN [dbo].[CaseRelation]
      ON [dbo].[CaseRelationAudit].[CaseRelationId] = [dbo].[CaseRelation].[Id]
    INNER JOIN [dbo].[Regulation]
      ON [dbo].[CaseRelation].[RegulationId] = [dbo].[Regulation].[Id]
    WHERE [dbo].[Regulation].[TenantId] = @tenantId

    DELETE [dbo].[CaseRelation]
    FROM [dbo].[CaseRelation]
    INNER JOIN [dbo].[Regulation]
      ON [dbo].[CaseRelation].[RegulationId] = [dbo].[Regulation].[Id]
    WHERE [dbo].[Regulation].[TenantId] = @tenantId

    DELETE [dbo].[CaseFieldAudit]
    FROM [dbo].[CaseFieldAudit]
    INNER JOIN [dbo].[CaseField]
      ON [dbo].[CaseFieldAudit].[CaseFieldId] = [dbo].[CaseField].[Id]
    INNER JOIN [dbo].[Case]
      ON [dbo].[CaseField].[CaseId] = [dbo].[Case].[Id]
    INNER JOIN [dbo].[Regulation]
      ON [dbo].[Case].[RegulationId] = [dbo].[Regulation].[Id]
    WHERE [dbo].[Regulation].[TenantId] = @tenantId

    DELETE [dbo].[CaseField]
    FROM [dbo].[CaseField]
    INNER JOIN [dbo].[Case]
      ON [dbo].[CaseField].[CaseId] = [dbo].[Case].[Id]
    INNER JOIN [dbo].[Regulation]
      ON [dbo].[Case].[RegulationId] = [dbo].[Regulation].[Id]
    WHERE [dbo].[Regulation].[TenantId] = @tenantId

    DELETE [dbo].[CaseAudit]
    FROM [dbo].[CaseAudit]
    INNER JOIN [dbo].[Case]
      ON [dbo].[CaseAudit].[CaseId] = [dbo].[Case].[Id]
    INNER JOIN [dbo].[Regulation]
      ON [dbo].[Case].[RegulationId] = [dbo].[Regulation].[Id]
    WHERE [dbo].[Regulation].[TenantId] = @tenantId

    DELETE [dbo].[Case]
    FROM [dbo].[Case]
    INNER JOIN [dbo].[Regulation]
      ON [dbo].[Case].[RegulationId] = [dbo].[Regulation].[Id]
    WHERE [dbo].[Regulation].[TenantId] = @tenantId

    DELETE
    FROM [dbo].[Regulation]
    WHERE [TenantId] = @tenantId

    -- employee
    DELETE [dbo].[EmployeeCaseValueChange]
    FROM [dbo].[EmployeeCaseValueChange]
    INNER JOIN [dbo].[EmployeeCaseChange]
      ON [dbo].[EmployeeCaseValueChange].[CaseChangeId] = [dbo].[EmployeeCaseChange].[Id]
    INNER JOIN [dbo].[Employee]
      ON [dbo].[EmployeeCaseChange].[EmployeeId] = [dbo].[Employee].[Id]
    WHERE [dbo].[Employee].[TenantId] = @tenantId

    DELETE [dbo].[EmployeeCaseChange]
    FROM [dbo].[EmployeeCaseChange]
    INNER JOIN [dbo].[Employee]
      ON [dbo].[EmployeeCaseChange].[EmployeeId] = [dbo].[Employee].[Id]
    WHERE [dbo].[Employee].[TenantId] = @tenantId

    DELETE [dbo].[EmployeeCaseDocument]
    FROM [dbo].[EmployeeCaseDocument]
    INNER JOIN [dbo].[EmployeeCaseValue]
      ON [dbo].[EmployeeCaseDocument].[CaseValueId] = [dbo].[EmployeeCaseValue].[Id]
    INNER JOIN [dbo].[Employee]
      ON [dbo].[EmployeeCaseValue].[EmployeeId] = [dbo].[Employee].[Id]
    WHERE [dbo].[Employee].[TenantId] = @tenantId

    DELETE [dbo].[EmployeeCaseValue]
    FROM [dbo].[EmployeeCaseValue]
    INNER JOIN [dbo].[Employee]
      ON [dbo].[EmployeeCaseValue].[EmployeeId] = [dbo].[Employee].[Id]
    WHERE [dbo].[Employee].[TenantId] = @tenantId

    DELETE [dbo].[EmployeeDivision]
    FROM [dbo].[EmployeeDivision]
    INNER JOIN [dbo].[Employee]
      ON [dbo].[EmployeeDivision].[EmployeeId] = [dbo].[Employee].[Id]
    WHERE [dbo].[Employee].[TenantId] = @tenantId

    DELETE
    FROM [dbo].[Employee]
    WHERE [TenantId] = @tenantId

    -- company
    DELETE [dbo].[CompanyCaseValueChange]
    FROM [dbo].[CompanyCaseValueChange]
    INNER JOIN [dbo].[CompanyCaseChange]
      ON [dbo].[CompanyCaseValueChange].[CaseChangeId] = [dbo].[CompanyCaseChange].[Id]
    WHERE [dbo].[CompanyCaseChange].[TenantId] = @tenantId

    DELETE
    FROM [dbo].[CompanyCaseChange]
    WHERE [TenantId] = @tenantId

    DELETE [dbo].[CompanyCaseDocument]
    FROM [dbo].[CompanyCaseDocument]
    INNER JOIN [dbo].[CompanyCaseValue]
      ON [dbo].[CompanyCaseDocument].[CaseValueId] = [dbo].[CompanyCaseValue].[Id]
    WHERE [dbo].[CompanyCaseValue].[TenantId] = @tenantId

    DELETE
    FROM [dbo].[CompanyCaseValue]
    WHERE [TenantId] = @tenantId

    -- national
    DELETE [dbo].[NationalCaseValueChange]
    FROM [dbo].[NationalCaseValueChange]
    INNER JOIN [dbo].[NationalCaseChange]
      ON [dbo].[NationalCaseValueChange].[CaseChangeId] = [dbo].[NationalCaseChange].[Id]
    WHERE [dbo].[NationalCaseChange].[TenantId] = @tenantId

    DELETE
    FROM [dbo].[NationalCaseChange]
    WHERE [TenantId] = @tenantId

    DELETE [dbo].[NationalCaseDocument]
    FROM [dbo].[NationalCaseDocument]
    INNER JOIN [dbo].[NationalCaseValue]
      ON [dbo].[NationalCaseDocument].[CaseValueId] = [dbo].[NationalCaseValue].[Id]
    WHERE [dbo].[NationalCaseValue].[TenantId] = @tenantId

    DELETE
    FROM [dbo].[NationalCaseValue]
    WHERE [TenantId] = @tenantId

    -- Global
    DELETE [dbo].[GlobalCaseValueChange]
    FROM [dbo].[GlobalCaseValueChange]
    INNER JOIN [dbo].[GlobalCaseChange]
      ON [dbo].[GlobalCaseValueChange].[CaseChangeId] = [dbo].[GlobalCaseChange].[Id]
    WHERE [dbo].[GlobalCaseChange].[TenantId] = @tenantId

    DELETE
    FROM [dbo].[GlobalCaseChange]
    WHERE [TenantId] = @tenantId

    DELETE [dbo].[GlobalCaseDocument]
    FROM [dbo].[GlobalCaseDocument]
    INNER JOIN [dbo].[GlobalCaseValue]
      ON [dbo].[GlobalCaseDocument].[CaseValueId] = [dbo].[GlobalCaseValue].[Id]
    WHERE [dbo].[GlobalCaseValue].[TenantId] = @tenantId

    DELETE
    FROM [dbo].[GlobalCaseValue]
    WHERE [TenantId] = @tenantId

    -- webhook
    DELETE [dbo].[WebhookMessage]
    FROM [dbo].[WebhookMessage]
    INNER JOIN [dbo].[Webhook]
      ON [dbo].[WebhookMessage].[WebhookId] = [dbo].[Webhook].[Id]
    WHERE [dbo].[Webhook].[TenantId] = @tenantId

    DELETE
    FROM [dbo].[Webhook]
    WHERE [TenantId] = @tenantId

    -- task
    DELETE
    FROM [dbo].[Task]
    WHERE [TenantId] = @tenantId

    -- log
    DELETE
    FROM [dbo].[Log]
    WHERE [TenantId] = @tenantId

    -- report log
    DELETE [dbo].[ReportLog]
    WHERE [dbo].[ReportLog].[TenantId] = @tenantId

    -- user
    DELETE
    FROM [dbo].[User]
    WHERE [TenantId] = @tenantId

    -- division
    DELETE
    FROM [dbo].[Division]
    WHERE [TenantId] = @tenantId

    -- tenant
    DELETE
    FROM [dbo].[Tenant]
    WHERE [Id] = @tenantId

    -- transaction end
    COMMIT TRANSACTION;
  END TRY

  BEGIN CATCH
    IF @@TRANCOUNT > 0
    BEGIN
      ROLLBACK TRANSACTION DeleteTenantTransaction;
    END
  END CATCH
END
GO


