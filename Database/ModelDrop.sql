USE [PayrollEngine]
GO

/****** Object:  StoredProcedure [dbo].[GetWageTypeResults]    Script Date: 06.01.2026 17:05:52 ******/
DROP PROCEDURE [dbo].[GetWageTypeResults]
GO

/****** Object:  StoredProcedure [dbo].[GetWageTypeCustomResults]    Script Date: 06.01.2026 17:05:53 ******/
DROP PROCEDURE [dbo].[GetWageTypeCustomResults]
GO

/****** Object:  StoredProcedure [dbo].[GetPayrollResultValues]    Script Date: 06.01.2026 17:05:53 ******/
DROP PROCEDURE [dbo].[GetPayrollResultValues]
GO

/****** Object:  StoredProcedure [dbo].[GetNationalCaseValues]    Script Date: 06.01.2026 17:05:53 ******/
DROP PROCEDURE [dbo].[GetNationalCaseValues]
GO

/****** Object:  StoredProcedure [dbo].[GetNationalCaseChangeValues]    Script Date: 06.01.2026 17:05:53 ******/
DROP PROCEDURE [dbo].[GetNationalCaseChangeValues]
GO

/****** Object:  StoredProcedure [dbo].[GetLookupRangeValue]    Script Date: 06.01.2026 17:05:53 ******/
DROP PROCEDURE [dbo].[GetLookupRangeValue]
GO

/****** Object:  StoredProcedure [dbo].[GetGlobalCaseValues]    Script Date: 06.01.2026 17:05:53 ******/
DROP PROCEDURE [dbo].[GetGlobalCaseValues]
GO

/****** Object:  StoredProcedure [dbo].[GetGlobalCaseChangeValues]    Script Date: 06.01.2026 17:05:53 ******/
DROP PROCEDURE [dbo].[GetGlobalCaseChangeValues]
GO

/****** Object:  StoredProcedure [dbo].[GetEmployeeCaseValues]    Script Date: 06.01.2026 17:05:53 ******/
DROP PROCEDURE [dbo].[GetEmployeeCaseValues]
GO

/****** Object:  StoredProcedure [dbo].[GetEmployeeCaseChangeValues]    Script Date: 06.01.2026 17:05:53 ******/
DROP PROCEDURE [dbo].[GetEmployeeCaseChangeValues]
GO

/****** Object:  StoredProcedure [dbo].[GetDerivedWageTypes]    Script Date: 06.01.2026 17:05:53 ******/
DROP PROCEDURE [dbo].[GetDerivedWageTypes]
GO

/****** Object:  StoredProcedure [dbo].[GetDerivedScripts]    Script Date: 06.01.2026 17:05:53 ******/
DROP PROCEDURE [dbo].[GetDerivedScripts]
GO

/****** Object:  StoredProcedure [dbo].[GetDerivedReportTemplates]    Script Date: 06.01.2026 17:05:53 ******/
DROP PROCEDURE [dbo].[GetDerivedReportTemplates]
GO

/****** Object:  StoredProcedure [dbo].[GetDerivedReports]    Script Date: 06.01.2026 17:05:53 ******/
DROP PROCEDURE [dbo].[GetDerivedReports]
GO

/****** Object:  StoredProcedure [dbo].[GetDerivedReportParameters]    Script Date: 06.01.2026 17:05:53 ******/
DROP PROCEDURE [dbo].[GetDerivedReportParameters]
GO

/****** Object:  StoredProcedure [dbo].[GetDerivedPayrollRegulations]    Script Date: 06.01.2026 17:05:53 ******/
DROP PROCEDURE [dbo].[GetDerivedPayrollRegulations]
GO

/****** Object:  StoredProcedure [dbo].[GetDerivedLookupValues]    Script Date: 06.01.2026 17:05:53 ******/
DROP PROCEDURE [dbo].[GetDerivedLookupValues]
GO

/****** Object:  StoredProcedure [dbo].[GetDerivedLookups]    Script Date: 06.01.2026 17:05:53 ******/
DROP PROCEDURE [dbo].[GetDerivedLookups]
GO

/****** Object:  StoredProcedure [dbo].[GetDerivedCollectors]    Script Date: 06.01.2026 17:05:53 ******/
DROP PROCEDURE [dbo].[GetDerivedCollectors]
GO

/****** Object:  StoredProcedure [dbo].[GetDerivedCases]    Script Date: 06.01.2026 17:05:53 ******/
DROP PROCEDURE [dbo].[GetDerivedCases]
GO

/****** Object:  StoredProcedure [dbo].[GetDerivedCaseRelations]    Script Date: 06.01.2026 17:05:53 ******/
DROP PROCEDURE [dbo].[GetDerivedCaseRelations]
GO

/****** Object:  StoredProcedure [dbo].[GetDerivedCaseFieldsOfCase]    Script Date: 06.01.2026 17:05:53 ******/
DROP PROCEDURE [dbo].[GetDerivedCaseFieldsOfCase]
GO

/****** Object:  StoredProcedure [dbo].[GetDerivedCaseFields]    Script Date: 06.01.2026 17:05:53 ******/
DROP PROCEDURE [dbo].[GetDerivedCaseFields]
GO

/****** Object:  StoredProcedure [dbo].[GetConsolidatedWageTypeResults]    Script Date: 06.01.2026 17:05:53 ******/
DROP PROCEDURE [dbo].[GetConsolidatedWageTypeResults]
GO

/****** Object:  StoredProcedure [dbo].[GetConsolidatedWageTypeCustomResults]    Script Date: 06.01.2026 17:05:53 ******/
DROP PROCEDURE [dbo].[GetConsolidatedWageTypeCustomResults]
GO

/****** Object:  StoredProcedure [dbo].[GetConsolidatedPayrunResults]    Script Date: 06.01.2026 17:05:53 ******/
DROP PROCEDURE [dbo].[GetConsolidatedPayrunResults]
GO

/****** Object:  StoredProcedure [dbo].[GetConsolidatedCollectorResults]    Script Date: 06.01.2026 17:05:53 ******/
DROP PROCEDURE [dbo].[GetConsolidatedCollectorResults]
GO

/****** Object:  StoredProcedure [dbo].[GetConsolidatedCollectorCustomResults]    Script Date: 06.01.2026 17:05:53 ******/
DROP PROCEDURE [dbo].[GetConsolidatedCollectorCustomResults]
GO

/****** Object:  StoredProcedure [dbo].[GetCompanyCaseValues]    Script Date: 06.01.2026 17:05:53 ******/
DROP PROCEDURE [dbo].[GetCompanyCaseValues]
GO

/****** Object:  StoredProcedure [dbo].[GetCompanyCaseChangeValues]    Script Date: 06.01.2026 17:05:53 ******/
DROP PROCEDURE [dbo].[GetCompanyCaseChangeValues]
GO

/****** Object:  StoredProcedure [dbo].[GetCollectorResults]    Script Date: 06.01.2026 17:05:53 ******/
DROP PROCEDURE [dbo].[GetCollectorResults]
GO

/****** Object:  StoredProcedure [dbo].[GetCollectorCustomResults]    Script Date: 06.01.2026 17:05:53 ******/
DROP PROCEDURE [dbo].[GetCollectorCustomResults]
GO

/****** Object:  StoredProcedure [dbo].[DeleteTenant]    Script Date: 06.01.2026 17:05:53 ******/
DROP PROCEDURE [dbo].[DeleteTenant]
GO

/****** Object:  StoredProcedure [dbo].[DeletePayrunJob]    Script Date: 06.01.2026 17:05:53 ******/
DROP PROCEDURE [dbo].[DeletePayrunJob]
GO

/****** Object:  StoredProcedure [dbo].[DeleteLookup]    Script Date: 06.01.2026 17:05:53 ******/
DROP PROCEDURE [dbo].[DeleteLookup]
GO

/****** Object:  StoredProcedure [dbo].[DeleteEmployee]    Script Date: 06.01.2026 17:05:53 ******/
DROP PROCEDURE [dbo].[DeleteEmployee]
GO

/****** Object:  StoredProcedure [dbo].[DeleteAllNationalCaseValues]    Script Date: 06.01.2026 17:05:53 ******/
DROP PROCEDURE [dbo].[DeleteAllNationalCaseValues]
GO

/****** Object:  StoredProcedure [dbo].[DeleteAllGlobalCaseValues]    Script Date: 06.01.2026 17:05:53 ******/
DROP PROCEDURE [dbo].[DeleteAllGlobalCaseValues]
GO

/****** Object:  StoredProcedure [dbo].[DeleteAllEmployeeCaseValues]    Script Date: 06.01.2026 17:05:53 ******/
DROP PROCEDURE [dbo].[DeleteAllEmployeeCaseValues]
GO

/****** Object:  StoredProcedure [dbo].[DeleteAllCompanyCaseValues]    Script Date: 06.01.2026 17:05:53 ******/
DROP PROCEDURE [dbo].[DeleteAllCompanyCaseValues]
GO

/****** Object:  StoredProcedure [dbo].[DeleteAllCaseValues]    Script Date: 06.01.2026 17:05:53 ******/
DROP PROCEDURE [dbo].[DeleteAllCaseValues]
GO

ALTER TABLE [dbo].[WebhookMessage]

DROP CONSTRAINT [FK_WebhookMessage_Webhook]
GO

ALTER TABLE [dbo].[Webhook]

DROP CONSTRAINT [FK_Webhook_Tenant]
GO

ALTER TABLE [dbo].[WageTypeResult]

DROP CONSTRAINT [FK_WageTypeResult_PayrollResult]
GO

ALTER TABLE [dbo].[WageTypeCustomResult]

DROP CONSTRAINT [FK_WageTypeCustomResult_WageTypeResult]
GO

ALTER TABLE [dbo].[WageTypeAudit]

DROP CONSTRAINT [FK_Regulation.WageTypeAudit_WageType]
GO

ALTER TABLE [dbo].[WageType]

DROP CONSTRAINT [FK_WageType_Regulation]
GO

ALTER TABLE [dbo].[User]

DROP CONSTRAINT [FK_User_Tenant]
GO

ALTER TABLE [dbo].[Task]

DROP CONSTRAINT [FK_Task_User1]
GO

ALTER TABLE [dbo].[Task]

DROP CONSTRAINT [FK_Task_User]
GO

ALTER TABLE [dbo].[Task]

DROP CONSTRAINT [FK_Task_Tenant]
GO

ALTER TABLE [dbo].[Script]

DROP CONSTRAINT [FK_Script_Regulation]
GO

ALTER TABLE [dbo].[ReportTemplate]

DROP CONSTRAINT [FK_ReportTemplate_Report]
GO

ALTER TABLE [dbo].[ReportParameter]

DROP CONSTRAINT [FK_ReportParameter_Report]
GO

ALTER TABLE [dbo].[ReportLog]

DROP CONSTRAINT [FK_ReportLog_Tenant]
GO

ALTER TABLE [dbo].[Report]

DROP CONSTRAINT [FK_Report_Regulation]
GO

ALTER TABLE [dbo].[RegulationShare]

DROP CONSTRAINT [FK_RegulationShare_ProviderTenant]
GO

ALTER TABLE [dbo].[RegulationShare]

DROP CONSTRAINT [FK_RegulationShare_ProviderRegulation]
GO

ALTER TABLE [dbo].[RegulationShare]

DROP CONSTRAINT [FK_RegulationShare_PermissionTenant]
GO

ALTER TABLE [dbo].[RegulationShare]

DROP CONSTRAINT [FK_RegulationShare_ConsumerDivision]
GO

ALTER TABLE [dbo].[Regulation]

DROP CONSTRAINT [FK_Regulation_Tenant]
GO

ALTER TABLE [dbo].[PayrunTrace]

DROP CONSTRAINT [FK_PayrunTrace_PayrollResult]
GO

ALTER TABLE [dbo].[PayrunResult]

DROP CONSTRAINT [FK_PayrunResult_PayrollResult]
GO

ALTER TABLE [dbo].[PayrunParameter]

DROP CONSTRAINT [FK_PayrunParameter_Payrun]
GO

ALTER TABLE [dbo].[PayrunJobEmployee]

DROP CONSTRAINT [FK_PayrunJobEmployee_PayrunJob]
GO

ALTER TABLE [dbo].[PayrunJobEmployee]

DROP CONSTRAINT [FK_PayrunJobEmployee_Employee]
GO

ALTER TABLE [dbo].[PayrunJob]

DROP CONSTRAINT [FK_PayrunJob_User]
GO

ALTER TABLE [dbo].[PayrunJob]

DROP CONSTRAINT [FK_PayrunJob_Tenant]
GO

ALTER TABLE [dbo].[PayrunJob]

DROP CONSTRAINT [FK_PayrunJob_ReleaseUser]
GO

ALTER TABLE [dbo].[PayrunJob]

DROP CONSTRAINT [FK_PayrunJob_ProcessUser]
GO

ALTER TABLE [dbo].[PayrunJob]

DROP CONSTRAINT [FK_PayrunJob_Payrun]
GO

ALTER TABLE [dbo].[PayrunJob]

DROP CONSTRAINT [FK_PayrunJob_Payroll]
GO

ALTER TABLE [dbo].[PayrunJob]

DROP CONSTRAINT [FK_PayrunJob_ParentPayrunJob]
GO

ALTER TABLE [dbo].[PayrunJob]

DROP CONSTRAINT [FK_PayrunJob_FinishUser]
GO

ALTER TABLE [dbo].[PayrunJob]

DROP CONSTRAINT [FK_PayrunJob_Division]
GO

ALTER TABLE [dbo].[Payrun]

DROP CONSTRAINT [FK_Payrun_Tenant]
GO

ALTER TABLE [dbo].[Payrun]

DROP CONSTRAINT [FK_Payrun_Payroll]
GO

ALTER TABLE [dbo].[PayrollResult]

DROP CONSTRAINT [FK_PayrollResult_PayrunJob]
GO

ALTER TABLE [dbo].[PayrollResult]

DROP CONSTRAINT [FK_PayrollResult_Payroll]
GO

ALTER TABLE [dbo].[PayrollResult]

DROP CONSTRAINT [FK_PayrollResult_Employee]
GO

ALTER TABLE [dbo].[PayrollResult]

DROP CONSTRAINT [FK_PayrollResult_Division]
GO

ALTER TABLE [dbo].[PayrollLayer]

DROP CONSTRAINT [FK_PayrollLayer_Payroll]
GO

ALTER TABLE [dbo].[Payroll]

DROP CONSTRAINT [FK_Payroll_Tenant]
GO

ALTER TABLE [dbo].[Payroll]

DROP CONSTRAINT [FK_Payroll_Division]
GO

ALTER TABLE [dbo].[NationalCaseValueChange]

DROP CONSTRAINT [FK_NationalCaseValueChange_NationalCaseValue]
GO

ALTER TABLE [dbo].[NationalCaseValueChange]

DROP CONSTRAINT [FK_NationalCaseValueChange_NationalCaseChange]
GO

ALTER TABLE [dbo].[NationalCaseValue]

DROP CONSTRAINT [FK_NationalCaseValue_Tenant]
GO

ALTER TABLE [dbo].[NationalCaseValue]

DROP CONSTRAINT [FK_NationalCaseValue_Division]
GO

ALTER TABLE [dbo].[NationalCaseDocument]

DROP CONSTRAINT [FK_NationalCaseDocument_NationalCaseValue]
GO

ALTER TABLE [dbo].[NationalCaseChange]

DROP CONSTRAINT [FK_NationalCaseChange_Tenant]
GO

ALTER TABLE [dbo].[NationalCaseChange]

DROP CONSTRAINT [FK_NationalCaseChange_Division]
GO

ALTER TABLE [dbo].[NationalCaseChange]

DROP CONSTRAINT [FK_NationalCaseChange_CancellationCaseChange]
GO

ALTER TABLE [dbo].[NationalCaseChange]

DROP CONSTRAINT [FK_CaseChange_User]
GO

ALTER TABLE [dbo].[LookupValueAudit]

DROP CONSTRAINT [FK_LookupValueAudit_LookupValue]
GO

ALTER TABLE [dbo].[LookupValue]

DROP CONSTRAINT [FK_LookupValue_Lookup]
GO

ALTER TABLE [dbo].[LookupAudit]

DROP CONSTRAINT [FK_LookupAudit_Lookup]
GO

ALTER TABLE [dbo].[Lookup]

DROP CONSTRAINT [FK_Lookup_Regulation]
GO

ALTER TABLE [dbo].[Log]

DROP CONSTRAINT [FK_Log_Tenant]
GO

ALTER TABLE [dbo].[GlobalCaseValueChange]

DROP CONSTRAINT [FK_GlobalCaseValueChange_GlobalCaseValue]
GO

ALTER TABLE [dbo].[GlobalCaseValueChange]

DROP CONSTRAINT [FK_GlobalCaseValueChange_GlobalCaseChange]
GO

ALTER TABLE [dbo].[GlobalCaseValue]

DROP CONSTRAINT [FK_GlobalCaseValue_Tenant]
GO

ALTER TABLE [dbo].[GlobalCaseValue]

DROP CONSTRAINT [FK_GlobalCaseValue_Division]
GO

ALTER TABLE [dbo].[GlobalCaseDocument]

DROP CONSTRAINT [FK_GlobalCaseDocument_GlobalCaseValue]
GO

ALTER TABLE [dbo].[GlobalCaseChange]

DROP CONSTRAINT [FK_GlobalCaseChange_User]
GO

ALTER TABLE [dbo].[GlobalCaseChange]

DROP CONSTRAINT [FK_GlobalCaseChange_Tenant]
GO

ALTER TABLE [dbo].[GlobalCaseChange]

DROP CONSTRAINT [FK_GlobalCaseChange_Division]
GO

ALTER TABLE [dbo].[GlobalCaseChange]

DROP CONSTRAINT [FK_GlobalCaseChange_CancellationGlobalCaseChange]
GO

ALTER TABLE [dbo].[EmployeeDivision]

DROP CONSTRAINT [FK_EmployeeDivision_Employee]
GO

ALTER TABLE [dbo].[EmployeeDivision]

DROP CONSTRAINT [FK_EmployeeDivision_Division]
GO

ALTER TABLE [dbo].[EmployeeCaseValueChange]

DROP CONSTRAINT [FK_EmployeeCaseValueChange_EmployeeCaseValue]
GO

ALTER TABLE [dbo].[EmployeeCaseValueChange]

DROP CONSTRAINT [FK_EmployeeCaseValueChange_EmployeeCaseChange]
GO

ALTER TABLE [dbo].[EmployeeCaseValue]

DROP CONSTRAINT [FK_EmployeeCaseValue_Employee]
GO

ALTER TABLE [dbo].[EmployeeCaseValue]

DROP CONSTRAINT [FK_EmployeeCaseValue_Division]
GO

ALTER TABLE [dbo].[EmployeeCaseDocument]

DROP CONSTRAINT [FK_EmployeeCaseDocument_EmployeeCaseValue]
GO

ALTER TABLE [dbo].[EmployeeCaseChange]

DROP CONSTRAINT [FK_EmployeeCaseChange_User]
GO

ALTER TABLE [dbo].[EmployeeCaseChange]

DROP CONSTRAINT [FK_EmployeeCaseChange_Employee]
GO

ALTER TABLE [dbo].[EmployeeCaseChange]

DROP CONSTRAINT [FK_EmployeeCaseChange_Division]
GO

ALTER TABLE [dbo].[EmployeeCaseChange]

DROP CONSTRAINT [FK_EmployeeCaseChange_CancellationCaseChange]
GO

ALTER TABLE [dbo].[Employee]

DROP CONSTRAINT [FK_Employee_Tenant]
GO

ALTER TABLE [dbo].[Division]

DROP CONSTRAINT [FK_Division_Tenant]
GO

ALTER TABLE [dbo].[CompanyCaseValueChange]

DROP CONSTRAINT [FK_IX_CompanyCaseValueChange_CompanyCaseChange]
GO

ALTER TABLE [dbo].[CompanyCaseValueChange]

DROP CONSTRAINT [FK_CompanyCaseValueChange_CompanyCaseValue]
GO

ALTER TABLE [dbo].[CompanyCaseValue]

DROP CONSTRAINT [FK_CompanyCaseValue_Tenant]
GO

ALTER TABLE [dbo].[CompanyCaseValue]

DROP CONSTRAINT [FK_CompanyCaseValue_Division]
GO

ALTER TABLE [dbo].[CompanyCaseDocument]

DROP CONSTRAINT [FK_CompanyCaseDocument_CompanyCaseValue]
GO

ALTER TABLE [dbo].[CompanyCaseChange]

DROP CONSTRAINT [FK_CompanyCaseChange_User]
GO

ALTER TABLE [dbo].[CompanyCaseChange]

DROP CONSTRAINT [FK_CompanyCaseChange_Tenant]
GO

ALTER TABLE [dbo].[CompanyCaseChange]

DROP CONSTRAINT [FK_CompanyCaseChange_Division]
GO

ALTER TABLE [dbo].[CompanyCaseChange]

DROP CONSTRAINT [FK_CompanyCaseChange_CancellationCaseChange]
GO

ALTER TABLE [dbo].[CollectorResult]

DROP CONSTRAINT [FK_CollectorResult_PayrollResult]
GO

ALTER TABLE [dbo].[CollectorCustomResult]

DROP CONSTRAINT [FK_CollectorCustomResult_CollectorResult]
GO

ALTER TABLE [dbo].[CollectorAudit]

DROP CONSTRAINT [FK_Regulation.CollectorAudit_Collector]
GO

ALTER TABLE [dbo].[Collector]

DROP CONSTRAINT [FK_Collector_Regulation]
GO

ALTER TABLE [dbo].[CaseRelationAudit]

DROP CONSTRAINT [FK_CaseRelationAudit_CaseRelation]
GO

ALTER TABLE [dbo].[CaseRelation]

DROP CONSTRAINT [FK_CaseRelation_Regulation]
GO

ALTER TABLE [dbo].[CaseFieldAudit]

DROP CONSTRAINT [FK_CaseFieldAudit_CaseField]
GO

ALTER TABLE [dbo].[CaseField]

DROP CONSTRAINT [FK_CaseField_Case]
GO

ALTER TABLE [dbo].[CaseAudit]

DROP CONSTRAINT [FK_Case.CaseAudit_CaseChange]
GO

ALTER TABLE [dbo].[CaseAudit]

DROP CONSTRAINT [FK_Case.CaseAudit_Case]
GO

ALTER TABLE [dbo].[Case]

DROP CONSTRAINT [FK_Case_Regulation]
GO

ALTER TABLE [dbo].[Calendar]

DROP CONSTRAINT [FK_Calendar_Tenant]
GO

/****** Object:  Table [dbo].[WebhookMessage]    Script Date: 06.01.2026 17:05:53 ******/
DROP TABLE [dbo].[WebhookMessage]
GO

/****** Object:  Table [dbo].[Webhook]    Script Date: 06.01.2026 17:05:53 ******/
DROP TABLE [dbo].[Webhook]
GO

/****** Object:  Table [dbo].[WageTypeResult]    Script Date: 06.01.2026 17:05:53 ******/
DROP TABLE [dbo].[WageTypeResult]
GO

/****** Object:  Table [dbo].[WageTypeCustomResult]    Script Date: 06.01.2026 17:05:53 ******/
DROP TABLE [dbo].[WageTypeCustomResult]
GO

/****** Object:  Table [dbo].[WageTypeAudit]    Script Date: 06.01.2026 17:05:53 ******/
DROP TABLE [dbo].[WageTypeAudit]
GO

/****** Object:  Table [dbo].[WageType]    Script Date: 06.01.2026 17:05:53 ******/
DROP TABLE [dbo].[WageType]
GO

/****** Object:  Table [dbo].[Version]    Script Date: 06.01.2026 17:05:53 ******/
DROP TABLE [dbo].[Version]
GO

/****** Object:  Table [dbo].[User]    Script Date: 06.01.2026 17:05:53 ******/
DROP TABLE [dbo].[User]
GO

/****** Object:  Table [dbo].[Tenant]    Script Date: 06.01.2026 17:05:53 ******/
DROP TABLE [dbo].[Tenant]
GO

/****** Object:  Table [dbo].[Task]    Script Date: 06.01.2026 17:05:53 ******/
DROP TABLE [dbo].[Task]
GO

/****** Object:  Table [dbo].[ScriptAudit]    Script Date: 06.01.2026 17:05:53 ******/
DROP TABLE [dbo].[ScriptAudit]
GO

/****** Object:  Table [dbo].[Script]    Script Date: 06.01.2026 17:05:53 ******/
DROP TABLE [dbo].[Script]
GO

/****** Object:  Table [dbo].[ReportTemplateAudit]    Script Date: 06.01.2026 17:05:53 ******/
DROP TABLE [dbo].[ReportTemplateAudit]
GO

/****** Object:  Table [dbo].[ReportTemplate]    Script Date: 06.01.2026 17:05:53 ******/
DROP TABLE [dbo].[ReportTemplate]
GO

/****** Object:  Table [dbo].[ReportParameterAudit]    Script Date: 06.01.2026 17:05:53 ******/
DROP TABLE [dbo].[ReportParameterAudit]
GO

/****** Object:  Table [dbo].[ReportParameter]    Script Date: 06.01.2026 17:05:53 ******/
DROP TABLE [dbo].[ReportParameter]
GO

/****** Object:  Table [dbo].[ReportLog]    Script Date: 06.01.2026 17:05:53 ******/
DROP TABLE [dbo].[ReportLog]
GO

/****** Object:  Table [dbo].[ReportAudit]    Script Date: 06.01.2026 17:05:53 ******/
DROP TABLE [dbo].[ReportAudit]
GO

/****** Object:  Table [dbo].[Report]    Script Date: 06.01.2026 17:05:53 ******/
DROP TABLE [dbo].[Report]
GO

/****** Object:  Table [dbo].[RegulationShare]    Script Date: 06.01.2026 17:05:53 ******/
DROP TABLE [dbo].[RegulationShare]
GO

/****** Object:  Table [dbo].[PayrunTrace]    Script Date: 06.01.2026 17:05:53 ******/
DROP TABLE [dbo].[PayrunTrace]
GO

/****** Object:  Table [dbo].[PayrunResult]    Script Date: 06.01.2026 17:05:53 ******/
DROP TABLE [dbo].[PayrunResult]
GO

/****** Object:  Table [dbo].[PayrunParameter]    Script Date: 06.01.2026 17:05:53 ******/
DROP TABLE [dbo].[PayrunParameter]
GO

/****** Object:  Table [dbo].[PayrunJobEmployee]    Script Date: 06.01.2026 17:05:53 ******/
DROP TABLE [dbo].[PayrunJobEmployee]
GO

/****** Object:  Table [dbo].[PayrunJob]    Script Date: 06.01.2026 17:05:53 ******/
DROP TABLE [dbo].[PayrunJob]
GO

/****** Object:  Table [dbo].[Payrun]    Script Date: 06.01.2026 17:05:53 ******/
DROP TABLE [dbo].[Payrun]
GO

/****** Object:  Table [dbo].[PayrollResult]    Script Date: 06.01.2026 17:05:53 ******/
DROP TABLE [dbo].[PayrollResult]
GO

/****** Object:  Table [dbo].[Payroll]    Script Date: 06.01.2026 17:05:53 ******/
DROP TABLE [dbo].[Payroll]
GO

/****** Object:  Table [dbo].[NationalCaseValueChange]    Script Date: 06.01.2026 17:05:53 ******/
DROP TABLE [dbo].[NationalCaseValueChange]
GO

/****** Object:  Table [dbo].[NationalCaseValue]    Script Date: 06.01.2026 17:05:53 ******/
DROP TABLE [dbo].[NationalCaseValue]
GO

/****** Object:  Table [dbo].[NationalCaseDocument]    Script Date: 06.01.2026 17:05:53 ******/
DROP TABLE [dbo].[NationalCaseDocument]
GO

/****** Object:  Table [dbo].[NationalCaseChange]    Script Date: 06.01.2026 17:05:53 ******/
DROP TABLE [dbo].[NationalCaseChange]
GO

/****** Object:  Table [dbo].[LookupValueAudit]    Script Date: 06.01.2026 17:05:53 ******/
DROP TABLE [dbo].[LookupValueAudit]
GO

/****** Object:  Table [dbo].[LookupValue]    Script Date: 06.01.2026 17:05:53 ******/
DROP TABLE [dbo].[LookupValue]
GO

/****** Object:  Table [dbo].[LookupAudit]    Script Date: 06.01.2026 17:05:53 ******/
DROP TABLE [dbo].[LookupAudit]
GO

/****** Object:  Table [dbo].[Lookup]    Script Date: 06.01.2026 17:05:53 ******/
DROP TABLE [dbo].[Lookup]
GO

/****** Object:  Table [dbo].[Log]    Script Date: 06.01.2026 17:05:53 ******/
DROP TABLE [dbo].[Log]
GO

/****** Object:  Table [dbo].[GlobalCaseValueChange]    Script Date: 06.01.2026 17:05:53 ******/
DROP TABLE [dbo].[GlobalCaseValueChange]
GO

/****** Object:  Table [dbo].[GlobalCaseValue]    Script Date: 06.01.2026 17:05:53 ******/
DROP TABLE [dbo].[GlobalCaseValue]
GO

/****** Object:  Table [dbo].[GlobalCaseDocument]    Script Date: 06.01.2026 17:05:53 ******/
DROP TABLE [dbo].[GlobalCaseDocument]
GO

/****** Object:  Table [dbo].[GlobalCaseChange]    Script Date: 06.01.2026 17:05:53 ******/
DROP TABLE [dbo].[GlobalCaseChange]
GO

/****** Object:  Table [dbo].[EmployeeDivision]    Script Date: 06.01.2026 17:05:53 ******/
DROP TABLE [dbo].[EmployeeDivision]
GO

/****** Object:  Table [dbo].[EmployeeCaseValueChange]    Script Date: 06.01.2026 17:05:53 ******/
DROP TABLE [dbo].[EmployeeCaseValueChange]
GO

/****** Object:  Table [dbo].[EmployeeCaseValue]    Script Date: 06.01.2026 17:05:53 ******/
DROP TABLE [dbo].[EmployeeCaseValue]
GO

/****** Object:  Table [dbo].[EmployeeCaseDocument]    Script Date: 06.01.2026 17:05:53 ******/
DROP TABLE [dbo].[EmployeeCaseDocument]
GO

/****** Object:  Table [dbo].[EmployeeCaseChange]    Script Date: 06.01.2026 17:05:53 ******/
DROP TABLE [dbo].[EmployeeCaseChange]
GO

/****** Object:  Table [dbo].[Employee]    Script Date: 06.01.2026 17:05:53 ******/
DROP TABLE [dbo].[Employee]
GO

/****** Object:  Table [dbo].[Division]    Script Date: 06.01.2026 17:05:53 ******/
DROP TABLE [dbo].[Division]
GO

/****** Object:  Table [dbo].[CompanyCaseValueChange]    Script Date: 06.01.2026 17:05:53 ******/
DROP TABLE [dbo].[CompanyCaseValueChange]
GO

/****** Object:  Table [dbo].[CompanyCaseValue]    Script Date: 06.01.2026 17:05:53 ******/
DROP TABLE [dbo].[CompanyCaseValue]
GO

/****** Object:  Table [dbo].[CompanyCaseDocument]    Script Date: 06.01.2026 17:05:53 ******/
DROP TABLE [dbo].[CompanyCaseDocument]
GO

/****** Object:  Table [dbo].[CompanyCaseChange]    Script Date: 06.01.2026 17:05:53 ******/
DROP TABLE [dbo].[CompanyCaseChange]
GO

/****** Object:  Table [dbo].[CollectorResult]    Script Date: 06.01.2026 17:05:53 ******/
DROP TABLE [dbo].[CollectorResult]
GO

/****** Object:  Table [dbo].[CollectorCustomResult]    Script Date: 06.01.2026 17:05:53 ******/
DROP TABLE [dbo].[CollectorCustomResult]
GO

/****** Object:  Table [dbo].[CollectorAudit]    Script Date: 06.01.2026 17:05:53 ******/
DROP TABLE [dbo].[CollectorAudit]
GO

/****** Object:  Table [dbo].[Collector]    Script Date: 06.01.2026 17:05:53 ******/
DROP TABLE [dbo].[Collector]
GO

/****** Object:  Table [dbo].[CaseRelationAudit]    Script Date: 06.01.2026 17:05:53 ******/
DROP TABLE [dbo].[CaseRelationAudit]
GO

/****** Object:  Table [dbo].[CaseRelation]    Script Date: 06.01.2026 17:05:53 ******/
DROP TABLE [dbo].[CaseRelation]
GO

/****** Object:  Table [dbo].[CaseFieldAudit]    Script Date: 06.01.2026 17:05:53 ******/
DROP TABLE [dbo].[CaseFieldAudit]
GO

/****** Object:  Table [dbo].[CaseField]    Script Date: 06.01.2026 17:05:53 ******/
DROP TABLE [dbo].[CaseField]
GO

/****** Object:  Table [dbo].[CaseAudit]    Script Date: 06.01.2026 17:05:53 ******/
DROP TABLE [dbo].[CaseAudit]
GO

/****** Object:  Table [dbo].[Case]    Script Date: 06.01.2026 17:05:53 ******/
DROP TABLE [dbo].[Case]
GO

/****** Object:  Table [dbo].[Calendar]    Script Date: 06.01.2026 17:05:53 ******/
DROP TABLE [dbo].[Calendar]
GO

/****** Object:  UserDefinedFunction [dbo].[GetDerivedRegulations]    Script Date: 06.01.2026 17:05:53 ******/
DROP FUNCTION [dbo].[GetDerivedRegulations]
GO

/****** Object:  Table [dbo].[Regulation]    Script Date: 06.01.2026 17:05:53 ******/
DROP TABLE [dbo].[Regulation]
GO

/****** Object:  Table [dbo].[PayrollLayer]    Script Date: 06.01.2026 17:05:53 ******/
DROP TABLE [dbo].[PayrollLayer]
GO

/****** Object:  UserDefinedFunction [dbo].[IsMatchingCluster]    Script Date: 06.01.2026 17:05:53 ******/
DROP FUNCTION [dbo].[IsMatchingCluster]
GO

/****** Object:  UserDefinedFunction [dbo].[GetTextAttributeValue]    Script Date: 06.01.2026 17:05:53 ******/
DROP FUNCTION [dbo].[GetTextAttributeValue]
GO

/****** Object:  UserDefinedFunction [dbo].[GetNumericAttributeValue]    Script Date: 06.01.2026 17:05:53 ******/
DROP FUNCTION [dbo].[GetNumericAttributeValue]
GO

/****** Object:  UserDefinedFunction [dbo].[GetLocalizedValue]    Script Date: 06.01.2026 17:05:53 ******/
DROP FUNCTION [dbo].[GetLocalizedValue]
GO

/****** Object:  UserDefinedFunction [dbo].[GetDateAttributeValue]    Script Date: 06.01.2026 17:05:53 ******/
DROP FUNCTION [dbo].[GetDateAttributeValue]
GO

/****** Object:  UserDefinedFunction [dbo].[GetAttributeNames]    Script Date: 06.01.2026 17:05:53 ******/
DROP FUNCTION [dbo].[GetAttributeNames]
GO

/****** Object:  UserDefinedFunction [dbo].[BuildAttributeQuery]    Script Date: 06.01.2026 17:05:53 ******/
DROP FUNCTION [dbo].[BuildAttributeQuery]
GO

USE [master]
GO

/****** Object:  Database [PayrollEngine]    Script Date: 06.01.2026 17:05:53 ******/
DROP DATABASE [PayrollEngine]
GO


