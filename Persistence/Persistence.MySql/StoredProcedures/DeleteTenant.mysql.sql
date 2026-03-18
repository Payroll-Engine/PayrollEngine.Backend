-- =============================================================================
-- DeleteTenant
-- DELETE [ReportLog] WHERE -> DELETE FROM ReportLog WHERE (T-SQL alias quirk)
-- =============================================================================

USE PayrollEngine;

DELIMITER $$

DROP PROCEDURE IF EXISTS DeleteTenant$$
CREATE PROCEDURE DeleteTenant(
    IN p_tenantId INT
)
BEGIN
    DELETE pr FROM PayrunResult pr
    INNER JOIN PayrollResult prl ON pr.PayrollResultId = prl.Id
    WHERE prl.TenantId = p_tenantId;

    DELETE wtcr FROM WageTypeCustomResult wtcr
    INNER JOIN WageTypeResult wtr ON wtcr.WageTypeResultId = wtr.Id
    INNER JOIN PayrollResult prl ON wtr.PayrollResultId = prl.Id
    WHERE prl.TenantId = p_tenantId;

    DELETE wtr FROM WageTypeResult wtr
    INNER JOIN PayrollResult prl ON wtr.PayrollResultId = prl.Id
    WHERE prl.TenantId = p_tenantId;

    DELETE ccr FROM CollectorCustomResult ccr
    INNER JOIN CollectorResult cr ON ccr.CollectorResultId = cr.Id
    INNER JOIN PayrollResult prl ON cr.PayrollResultId = prl.Id
    WHERE prl.TenantId = p_tenantId;

    DELETE cr FROM CollectorResult cr
    INNER JOIN PayrollResult prl ON cr.PayrollResultId = prl.Id
    WHERE prl.TenantId = p_tenantId;

    DELETE FROM PayrollResult WHERE TenantId = p_tenantId;

    DELETE pje FROM PayrunJobEmployee pje
    INNER JOIN PayrunJob pj ON pje.PayrunJobId = pj.Id
    WHERE pj.TenantId = p_tenantId;

    DELETE FROM PayrunJob WHERE TenantId = p_tenantId;

    DELETE pp FROM PayrunParameter pp
    INNER JOIN Payrun pay ON pp.PayrunId = pay.Id
    WHERE pay.TenantId = p_tenantId;

    DELETE FROM Payrun WHERE TenantId = p_tenantId;

    DELETE pl FROM PayrollLayer pl
    INNER JOIN Payroll pay ON pl.PayrollId = pay.Id
    WHERE pay.TenantId = p_tenantId;

    DELETE FROM Payroll WHERE TenantId = p_tenantId;

    DELETE FROM RegulationShare WHERE ProviderTenantId = p_tenantId OR ConsumerTenantId = p_tenantId;

    DELETE rta FROM ReportTemplateAudit rta
    INNER JOIN ReportTemplate rt ON rta.ReportTemplateId = rt.Id
    INNER JOIN Report rp ON rt.ReportId = rp.Id
    INNER JOIN Regulation r ON rp.RegulationId = r.Id
    WHERE r.TenantId = p_tenantId;

    DELETE rt FROM ReportTemplate rt
    INNER JOIN Report rp ON rt.ReportId = rp.Id
    INNER JOIN Regulation r ON rp.RegulationId = r.Id
    WHERE r.TenantId = p_tenantId;

    DELETE rpa FROM ReportParameterAudit rpa
    INNER JOIN ReportParameter rpar ON rpa.ReportParameterId = rpar.Id
    INNER JOIN Report rp ON rpar.ReportId = rp.Id
    INNER JOIN Regulation r ON rp.RegulationId = r.Id
    WHERE r.TenantId = p_tenantId;

    DELETE rpar FROM ReportParameter rpar
    INNER JOIN Report rp ON rpar.ReportId = rp.Id
    INNER JOIN Regulation r ON rp.RegulationId = r.Id
    WHERE r.TenantId = p_tenantId;

    DELETE ra FROM ReportAudit ra
    INNER JOIN Report rp ON ra.ReportId = rp.Id
    INNER JOIN Regulation r ON rp.RegulationId = r.Id
    WHERE r.TenantId = p_tenantId;

    DELETE rp FROM Report rp
    INNER JOIN Regulation r ON rp.RegulationId = r.Id
    WHERE r.TenantId = p_tenantId;

    DELETE sa FROM ScriptAudit sa
    INNER JOIN Script s ON sa.ScriptId = s.Id
    INNER JOIN Regulation r ON s.RegulationId = r.Id
    WHERE r.TenantId = p_tenantId;

    DELETE s FROM Script s
    INNER JOIN Regulation r ON s.RegulationId = r.Id
    WHERE r.TenantId = p_tenantId;

    DELETE lva FROM LookupValueAudit lva
    INNER JOIN LookupValue lv ON lva.LookupValueId = lv.Id
    INNER JOIN Lookup lk ON lv.LookupId = lk.Id
    INNER JOIN Regulation r ON lk.RegulationId = r.Id
    WHERE r.TenantId = p_tenantId;

    DELETE lv FROM LookupValue lv
    INNER JOIN Lookup lk ON lv.LookupId = lk.Id
    INNER JOIN Regulation r ON lk.RegulationId = r.Id
    WHERE r.TenantId = p_tenantId;

    DELETE la FROM LookupAudit la
    INNER JOIN Lookup lk ON la.LookupId = lk.Id
    INNER JOIN Regulation r ON lk.RegulationId = r.Id
    WHERE r.TenantId = p_tenantId;

    DELETE lk FROM Lookup lk
    INNER JOIN Regulation r ON lk.RegulationId = r.Id
    WHERE r.TenantId = p_tenantId;

    DELETE coa FROM CollectorAudit coa
    INNER JOIN Collector co ON coa.CollectorId = co.Id
    INNER JOIN Regulation r ON co.RegulationId = r.Id
    WHERE r.TenantId = p_tenantId;

    DELETE co FROM Collector co
    INNER JOIN Regulation r ON co.RegulationId = r.Id
    WHERE r.TenantId = p_tenantId;

    DELETE wta FROM WageTypeAudit wta
    INNER JOIN WageType wt ON wta.WageTypeId = wt.Id
    INNER JOIN Regulation r ON wt.RegulationId = r.Id
    WHERE r.TenantId = p_tenantId;

    DELETE wt FROM WageType wt
    INNER JOIN Regulation r ON wt.RegulationId = r.Id
    WHERE r.TenantId = p_tenantId;

    DELETE cra FROM CaseRelationAudit cra
    INNER JOIN CaseRelation cr ON cra.CaseRelationId = cr.Id
    INNER JOIN Regulation r ON cr.RegulationId = r.Id
    WHERE r.TenantId = p_tenantId;

    DELETE cr FROM CaseRelation cr
    INNER JOIN Regulation r ON cr.RegulationId = r.Id
    WHERE r.TenantId = p_tenantId;

    DELETE cfa FROM CaseFieldAudit cfa
    INNER JOIN CaseField cf ON cfa.CaseFieldId = cf.Id
    INNER JOIN `Case` c ON cf.CaseId = c.Id
    INNER JOIN Regulation r ON c.RegulationId = r.Id
    WHERE r.TenantId = p_tenantId;

    DELETE cf FROM CaseField cf
    INNER JOIN `Case` c ON cf.CaseId = c.Id
    INNER JOIN Regulation r ON c.RegulationId = r.Id
    WHERE r.TenantId = p_tenantId;

    DELETE ca FROM CaseAudit ca
    INNER JOIN `Case` c ON ca.CaseId = c.Id
    INNER JOIN Regulation r ON c.RegulationId = r.Id
    WHERE r.TenantId = p_tenantId;

    DELETE c FROM `Case` c
    INNER JOIN Regulation r ON c.RegulationId = r.Id
    WHERE r.TenantId = p_tenantId;

    DELETE FROM Regulation WHERE TenantId = p_tenantId;

    DELETE ecvc FROM EmployeeCaseValueChange ecvc
    INNER JOIN EmployeeCaseChange ecc ON ecvc.CaseChangeId = ecc.Id
    INNER JOIN Employee e ON ecc.EmployeeId = e.Id
    WHERE e.TenantId = p_tenantId;

    DELETE ecc FROM EmployeeCaseChange ecc
    INNER JOIN Employee e ON ecc.EmployeeId = e.Id
    WHERE e.TenantId = p_tenantId;

    DELETE ecd FROM EmployeeCaseDocument ecd
    INNER JOIN EmployeeCaseValue ecv ON ecd.CaseValueId = ecv.Id
    INNER JOIN Employee e ON ecv.EmployeeId = e.Id
    WHERE e.TenantId = p_tenantId;

    DELETE ecv FROM EmployeeCaseValue ecv
    INNER JOIN Employee e ON ecv.EmployeeId = e.Id
    WHERE e.TenantId = p_tenantId;

    DELETE ed FROM EmployeeDivision ed
    INNER JOIN Employee e ON ed.EmployeeId = e.Id
    WHERE e.TenantId = p_tenantId;

    DELETE FROM Employee WHERE TenantId = p_tenantId;

    DELETE ccvc FROM CompanyCaseValueChange ccvc
    INNER JOIN CompanyCaseChange ccc ON ccvc.CaseChangeId = ccc.Id
    WHERE ccc.TenantId = p_tenantId;

    DELETE FROM CompanyCaseChange WHERE TenantId = p_tenantId;

    DELETE ccd FROM CompanyCaseDocument ccd
    INNER JOIN CompanyCaseValue ccv ON ccd.CaseValueId = ccv.Id
    WHERE ccv.TenantId = p_tenantId;

    DELETE FROM CompanyCaseValue WHERE TenantId = p_tenantId;

    DELETE ncvc FROM NationalCaseValueChange ncvc
    INNER JOIN NationalCaseChange ncc ON ncvc.CaseChangeId = ncc.Id
    WHERE ncc.TenantId = p_tenantId;

    DELETE FROM NationalCaseChange WHERE TenantId = p_tenantId;

    DELETE ncd FROM NationalCaseDocument ncd
    INNER JOIN NationalCaseValue ncv ON ncd.CaseValueId = ncv.Id
    WHERE ncv.TenantId = p_tenantId;

    DELETE FROM NationalCaseValue WHERE TenantId = p_tenantId;

    DELETE gcvc FROM GlobalCaseValueChange gcvc
    INNER JOIN GlobalCaseChange gcc ON gcvc.CaseChangeId = gcc.Id
    WHERE gcc.TenantId = p_tenantId;

    DELETE FROM GlobalCaseChange WHERE TenantId = p_tenantId;

    DELETE gcd FROM GlobalCaseDocument gcd
    INNER JOIN GlobalCaseValue gcv ON gcd.CaseValueId = gcv.Id
    WHERE gcv.TenantId = p_tenantId;

    DELETE FROM GlobalCaseValue WHERE TenantId = p_tenantId;

    DELETE wm FROM WebhookMessage wm
    INNER JOIN Webhook wh ON wm.WebhookId = wh.Id
    WHERE wh.TenantId = p_tenantId;

    DELETE FROM Webhook WHERE TenantId = p_tenantId;
    DELETE FROM Task WHERE TenantId = p_tenantId;
    DELETE FROM Log WHERE TenantId = p_tenantId;
    DELETE FROM ReportLog WHERE TenantId = p_tenantId;
    DELETE FROM `User` WHERE TenantId = p_tenantId;
    DELETE FROM Division WHERE TenantId = p_tenantId;
    DELETE FROM Calendar WHERE TenantId = p_tenantId;
    DELETE FROM Tenant WHERE Id = p_tenantId;
END$$

DELIMITER ;
