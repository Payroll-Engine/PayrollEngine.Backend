using PayrollEngine.Domain.Model.Repository;
using PayrollEngine.Serialization;
using Task = PayrollEngine.Domain.Model.Task;

namespace PayrollEngine.Persistence;

public class TaskRepository : ChildDomainRepository<Task>, ITaskRepository
{
    public TaskRepository(IDbContext context) :
        base(DbSchema.Tables.Task, DbSchema.TaskColumn.TenantId, context)
    {
    }

    protected override void GetObjectData(Task task, DbParameterCollection parameters)
    {
        parameters.Add(nameof(task.Name), task.Name);
        parameters.Add(nameof(task.NameLocalizations), JsonSerializer.SerializeNamedDictionary(task.NameLocalizations));
        parameters.Add(nameof(task.Category), task.Category);
        parameters.Add(nameof(task.Instruction), task.Instruction);
        parameters.Add(nameof(task.ScheduledUserId), task.ScheduledUserId);
        parameters.Add(nameof(task.Scheduled), task.Scheduled);
        parameters.Add(nameof(task.CompletedUserId), task.CompletedUserId);
        parameters.Add(nameof(task.Completed), task.Completed);
        parameters.Add(nameof(task.Attributes), JsonSerializer.SerializeNamedDictionary(task.Attributes));
        base.GetObjectData(task, parameters);
    }
}