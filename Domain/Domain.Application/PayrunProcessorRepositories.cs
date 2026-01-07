using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PayrollEngine.Domain.Model;

namespace PayrollEngine.Domain.Application;

internal sealed class PayrunProcessorRepositories
{
    private PayrunProcessorSettings Settings { get; }
    private Tenant Tenant { get; }

    internal PayrunProcessorRepositories(PayrunProcessorSettings settings, Tenant tenant)
    {
        Settings = settings ?? throw new ArgumentNullException(nameof(settings));
        Tenant = tenant ?? throw new ArgumentNullException(nameof(tenant));
    }

    internal async Task<Payroll> LoadPayrollAsync(int payrollId)
    {
        var payroll = await Settings.PayrollRepository.GetAsync(Settings.DbContext, Tenant.Id, payrollId);
        if (payroll == null)
        {
            throw new PayrunException($"Unknown payroll with id {payrollId}");
        }
        return payroll;
    }

    internal async Task<List<Regulation>> LoadDerivedRegulationsAsync(int payrollId,
        DateTime regulationDate, DateTime evaluationDate)
    {
        var regulations = (await Settings.PayrollRepository.GetDerivedRegulationsAsync(Settings.DbContext,
            new()
            {
                TenantId = Tenant.Id,
                PayrollId = payrollId,
                RegulationDate = regulationDate,
                EvaluationDate = evaluationDate
            })).ToList();
        if (regulations == null)
        {
            throw new PayrunException($"Unknown payroll with id {payrollId}");
        }
        return regulations;
    }

    internal async Task<string> ValidatePayrollAsync(Payroll payroll, Division division,
        DatePeriod period, DateTime evaluationDate)
    {
        // validate regulations
        var regulations = (await Settings.PayrollRepository.GetDerivedRegulationsAsync(Settings.DbContext,
            new()
            {
                TenantId = Tenant.Id,
                PayrollId = payroll.Id,
                RegulationDate = period.Start,
                EvaluationDate = evaluationDate
            })).ToList();
        if (!regulations.Any())
        {
            return $"Missing regulations for payroll {payroll.Name} (#{payroll.Id}): period={period}, evaluation={evaluationDate}";
        }

        // validate shared regulations
        foreach (var regulation in regulations)
        {
            var regulationTenantId = await Settings.RegulationRepository.GetParentIdAsync(Settings.DbContext, regulation.Id);
            if (!regulationTenantId.HasValue)
            {
                return $"Unknown regulation {regulation.Name} with id {regulation.Id}";
            }
            // local regulation
            if (regulationTenantId == Tenant.Id)
            {
                continue;
            }

            // test regulation sharing state
            if (!regulation.SharedRegulation)
            {
                return $"Invalid access to regulation {regulation.Name} with id {regulation.Id}";
            }

            // test regulation shares
            var shares = await Settings.RegulationShareRepository.GetAsync(Settings.DbContext, regulationTenantId.Value, regulation.Id,
                Tenant.Id, division?.Id);
            if (shares == null)
            {
                return $"Access denied to regulation {regulation.Name} with id {regulation.Id}";
            }
        }

        // valid
        return null;
    }

    internal async Task<Division> LoadDivisionAsync(int divisionId)
    {
        var division = await Settings.DivisionRepository.GetAsync(Settings.DbContext, Tenant.Id, divisionId);
        if (division == null)
        {
            throw new PayrunException($"Unknown division with id {divisionId}");
        }
        return division;
    }

    internal async Task<User> LoadUserAsync(int userId)
    {
        var user = await Settings.UserRepository.GetAsync(Settings.DbContext, Tenant.Id, userId);
        if (user == null)
        {
            throw new PayrunException($"Unknown user with id {userId}");
        }
        return user;
    }

    internal async Task<PayrunJob> LoadPayrunJobAsync(int payrunJobId)
    {
        var payrunJob = await Settings.PayrunJobRepository.GetAsync(Settings.DbContext, Tenant.Id, payrunJobId);
        if (payrunJob == null)
        {
            throw new PayrunException($"Unknown payrun job with id {payrunJobId}");
        }
        return payrunJob;
    }

    internal async Task<Payrun> LoadPayrunAsync(int payrunId)
    {
        var payrun = await Settings.PayrunRepository.GetAsync(Settings.DbContext, Tenant.Id, payrunId);
        if (payrun == null)
        {
            throw new PayrunException($"Unknown payrun with id {payrunId}");
        }
        return payrun;
    }
}