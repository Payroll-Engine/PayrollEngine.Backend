-- =============================================================================
-- Drop-Model.mysql.sql
-- Drops all PayrollEngine tables for MySQL 8.0+
-- Schema version: 0.9.6
-- =============================================================================

USE PayrollEngine;

SET FOREIGN_KEY_CHECKS = 0;

DROP TABLE IF EXISTS WebhookMessage;
DROP TABLE IF EXISTS Webhook;
DROP TABLE IF EXISTS WageTypeCustomResult;
DROP TABLE IF EXISTS WageTypeResult;
DROP TABLE IF EXISTS WageTypeAudit;
DROP TABLE IF EXISTS WageType;
DROP TABLE IF EXISTS Task;
DROP TABLE IF EXISTS ScriptAudit;
DROP TABLE IF EXISTS Script;
DROP TABLE IF EXISTS ReportTemplateAudit;
DROP TABLE IF EXISTS ReportTemplate;
DROP TABLE IF EXISTS ReportParameterAudit;
DROP TABLE IF EXISTS ReportParameter;
DROP TABLE IF EXISTS ReportLog;
DROP TABLE IF EXISTS ReportAudit;
DROP TABLE IF EXISTS Report;
DROP TABLE IF EXISTS RegulationShare;
DROP TABLE IF EXISTS Regulation;
DROP TABLE IF EXISTS PayrunTrace;
DROP TABLE IF EXISTS PayrunResult;
DROP TABLE IF EXISTS PayrunParameter;
DROP TABLE IF EXISTS PayrunJobEmployee;
DROP TABLE IF EXISTS PayrunJob;
DROP TABLE IF EXISTS Payrun;
DROP TABLE IF EXISTS PayrollResult;
DROP TABLE IF EXISTS PayrollLayer;
DROP TABLE IF EXISTS Payroll;
DROP TABLE IF EXISTS NationalCaseValueChange;
DROP TABLE IF EXISTS NationalCaseValue;
DROP TABLE IF EXISTS NationalCaseDocument;
DROP TABLE IF EXISTS NationalCaseChange;
DROP TABLE IF EXISTS LookupValueAudit;
DROP TABLE IF EXISTS LookupValue;
DROP TABLE IF EXISTS LookupAudit;
DROP TABLE IF EXISTS Lookup;
DROP TABLE IF EXISTS Log;
DROP TABLE IF EXISTS GlobalCaseValueChange;
DROP TABLE IF EXISTS GlobalCaseValue;
DROP TABLE IF EXISTS GlobalCaseDocument;
DROP TABLE IF EXISTS GlobalCaseChange;
DROP TABLE IF EXISTS EmployeeDivision;
DROP TABLE IF EXISTS EmployeeCaseValueChange;
DROP TABLE IF EXISTS EmployeeCaseValue;
DROP TABLE IF EXISTS EmployeeCaseDocument;
DROP TABLE IF EXISTS EmployeeCaseChange;
DROP TABLE IF EXISTS Employee;
DROP TABLE IF EXISTS Division;
DROP TABLE IF EXISTS CompanyCaseValueChange;
DROP TABLE IF EXISTS CompanyCaseValue;
DROP TABLE IF EXISTS CompanyCaseDocument;
DROP TABLE IF EXISTS CompanyCaseChange;
DROP TABLE IF EXISTS CollectorResult;
DROP TABLE IF EXISTS CollectorCustomResult;
DROP TABLE IF EXISTS CollectorAudit;
DROP TABLE IF EXISTS Collector;
DROP TABLE IF EXISTS CaseRelationAudit;
DROP TABLE IF EXISTS CaseRelation;
DROP TABLE IF EXISTS CaseFieldAudit;
DROP TABLE IF EXISTS CaseField;
DROP TABLE IF EXISTS CaseAudit;
DROP TABLE IF EXISTS `Case`;
DROP TABLE IF EXISTS Calendar;
DROP TABLE IF EXISTS `User`;
DROP TABLE IF EXISTS Tenant;
DROP TABLE IF EXISTS `Version`;

SET FOREIGN_KEY_CHECKS = 1;

SELECT 'PayrollEngine MySQL schema dropped.' AS Result;
