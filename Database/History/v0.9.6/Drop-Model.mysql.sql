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

-- Drop stored procedures
DROP PROCEDURE IF EXISTS DeleteEmployee;
DROP PROCEDURE IF EXISTS DeleteLookup;
DROP PROCEDURE IF EXISTS DeletePayrunJob;
DROP PROCEDURE IF EXISTS DeleteTenant;
DROP PROCEDURE IF EXISTS GetCollectorCustomResults;
DROP PROCEDURE IF EXISTS GetCollectorResults;
DROP PROCEDURE IF EXISTS GetCompanyCaseChangeValues;
DROP PROCEDURE IF EXISTS GetCompanyCaseValues;
DROP PROCEDURE IF EXISTS GetConsolidatedCollectorCustomResults;
DROP PROCEDURE IF EXISTS GetConsolidatedCollectorResults;
DROP PROCEDURE IF EXISTS GetConsolidatedPayrunResults;
DROP PROCEDURE IF EXISTS GetConsolidatedWageTypeCustomResults;
DROP PROCEDURE IF EXISTS GetConsolidatedWageTypeResults;
DROP PROCEDURE IF EXISTS GetDerivedCaseFields;
DROP PROCEDURE IF EXISTS GetDerivedCaseFieldsOfCase;
DROP PROCEDURE IF EXISTS GetDerivedCaseRelations;
DROP PROCEDURE IF EXISTS GetDerivedCases;
DROP PROCEDURE IF EXISTS GetDerivedCollectors;
DROP PROCEDURE IF EXISTS GetDerivedLookups;
DROP PROCEDURE IF EXISTS GetDerivedLookupValues;
DROP PROCEDURE IF EXISTS GetDerivedPayrollRegulations;
DROP PROCEDURE IF EXISTS GetDerivedReportParameters;
DROP PROCEDURE IF EXISTS GetDerivedReports;
DROP PROCEDURE IF EXISTS GetDerivedReportTemplates;
DROP PROCEDURE IF EXISTS GetDerivedScripts;
DROP PROCEDURE IF EXISTS GetDerivedWageTypes;
DROP PROCEDURE IF EXISTS GetEmployeeCaseChangeValues;
DROP PROCEDURE IF EXISTS GetEmployeeCaseValues;
DROP PROCEDURE IF EXISTS GetEmployeeCaseValuesByTenant;
DROP PROCEDURE IF EXISTS GetGlobalCaseChangeValues;
DROP PROCEDURE IF EXISTS GetGlobalCaseValues;
DROP PROCEDURE IF EXISTS GetLookupRangeValue;
DROP PROCEDURE IF EXISTS GetNationalCaseChangeValues;
DROP PROCEDURE IF EXISTS GetNationalCaseValues;
DROP PROCEDURE IF EXISTS GetPayrollResultValues;
DROP PROCEDURE IF EXISTS GetWageTypeCustomResults;
DROP PROCEDURE IF EXISTS GetWageTypeResults;
DROP PROCEDURE IF EXISTS UpdateStatistics;
DROP PROCEDURE IF EXISTS UpdateStatisticsTargeted;

-- Drop functions
DROP FUNCTION IF EXISTS BuildAttributeQuery;
DROP FUNCTION IF EXISTS GetAttributeNames;
DROP FUNCTION IF EXISTS GetDateAttributeValue;
DROP FUNCTION IF EXISTS GetLocalizedValue;
DROP FUNCTION IF EXISTS GetNumericAttributeValue;
DROP FUNCTION IF EXISTS GetTextAttributeValue;
DROP FUNCTION IF EXISTS IsMatchingCluster;

SELECT 'PayrollEngine MySQL schema dropped.' AS Result;