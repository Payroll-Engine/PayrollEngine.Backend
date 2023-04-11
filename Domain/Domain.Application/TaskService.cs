using PayrollEngine.Domain.Application.Service;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Domain.Application;

public class TaskService : ChildApplicationService<ITaskRepository, Task>, ITaskService
{
    public TaskService(ITaskRepository repository) :
        base(repository)
    {
    }
}