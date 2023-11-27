using PayrollEngine.Domain.Application.Service;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Domain.Application;

public class TaskService(ITaskRepository repository) : ChildApplicationService<ITaskRepository, Task>(repository),
    ITaskService;