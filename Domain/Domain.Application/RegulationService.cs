﻿using PayrollEngine.Domain.Application.Service;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Domain.Application;

public class RegulationService : ChildApplicationService<IRegulationRepository, Regulation>, IRegulationService
{
    public RegulationService(IRegulationRepository repository) :
        base(repository)
    {
    }
}