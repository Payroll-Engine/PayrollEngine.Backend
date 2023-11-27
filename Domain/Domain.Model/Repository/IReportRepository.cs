namespace PayrollEngine.Domain.Model.Repository;

/// <summary>
/// Repository for reports
/// </summary>
public interface IReportRepository : IScriptTrackDomainObjectRepository<Report, ReportAudit>;

/// <summary>
/// Repository for reports
/// </summary>
public interface IReportRepository<T> : ITrackChildDomainRepository<T, ReportAudit>
    where T : TrackDomainObject<ReportAudit>, new();