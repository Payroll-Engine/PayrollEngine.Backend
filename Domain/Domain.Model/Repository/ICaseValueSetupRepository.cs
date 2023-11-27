namespace PayrollEngine.Domain.Model.Repository;

/// <summary>
/// Repository for global case value setups
/// </summary>
public interface IGlobalCaseValueSetupRepository : ICaseValueSetupRepository;

/// <summary>
/// Repository for national case value setups
/// </summary>
public interface INationalCaseValueSetupRepository : ICaseValueSetupRepository;

/// <summary>
/// Repository for company case value setups
/// </summary>
public interface ICompanyCaseValueSetupRepository : ICaseValueSetupRepository;

/// <summary>
/// Repository for national case value setup
/// </summary>
public interface IEmployeeCaseValueSetupRepository : ICaseValueSetupRepository;

/// <summary>
/// Repository for case value setups
/// </summary>
public interface ICaseValueSetupRepository : ICaseValueRepository<CaseValueSetup>;