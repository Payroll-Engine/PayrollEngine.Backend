using System.Data;
using Task = PayrollEngine.Domain.Model.Task;
using PayrollEngine.Serialization;
using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Persistence;

public class TaskRepository() : ChildDomainRepository<Task>(DbSchema.Tables.Task, DbSchema.TaskColumn.TenantId),
    ITaskRepository
{
    protected override void GetObjectData(Task task, DbParameterCollection parameters)
    {
        parameters.Add(nameof(task.Name), task.Name);
        parameters.Add(nameof(task.NameLocalizations), JsonSerializer.SerializeNamedDictionary(task.NameLocalizations));
        parameters.Add(nameof(task.Category), task.Category);
        parameters.Add(nameof(task.Instruction), task.Instruction);
        parameters.Add(nameof(task.ScheduledUserId), task.ScheduledUserId, DbType.Int32);
        parameters.Add(nameof(task.Scheduled), task.Scheduled, DbType.DateTime2);
        parameters.Add(nameof(task.CompletedUserId), task.CompletedUserId, DbType.Int32);
        parameters.Add(nameof(task.Completed), task.Completed, DbType.DateTime2);
        parameters.Add(nameof(task.Attributes), JsonSerializer.SerializeNamedDictionary(task.Attributes));
        base.GetObjectData(task, parameters);
    }
}