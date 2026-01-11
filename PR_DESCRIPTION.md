# feat: Async payrun job processing to prevent HTTP timeouts

## Summary
- Implement asynchronous payrun job processing using `Channel<T>` and `BackgroundService`
- Return HTTP 202 Accepted immediately with Location header for status polling
- Prevent HTTP 524 timeout errors when processing large payrolls (500+ employees)

## Problem
When starting a payrun job via `POST /tenants/{tenantId}/payruns/jobs` with a large number of employees, the HTTP request blocks until all payroll calculations are complete. For 1,000 employees at ~0.5-1s each, this results in 8-15+ minutes of blocking time, far exceeding typical gateway timeouts (e.g., Cloudflare's 100s limit results in HTTP 524 errors).

## Solution
Decouple the HTTP response from job completion:
1. Create the PayrunJob record immediately in "Process" status
2. Enqueue the job for background processing via `Channel<T>`
3. Return HTTP 202 Accepted with `Location` header pointing to status endpoint
4. `BackgroundService` processes jobs from the queue asynchronously
5. Clients poll `GET /api/tenants/{id}/payruns/jobs/{jobId}/status` for completion

## Changes

### New files
- `Domain/Domain.Application/Service/IPayrunJobQueue.cs` - Queue interface
- `Domain/Domain.Application/Service/PayrunJobQueueItem.cs` - Queue item DTO
- `Domain/Domain.Application/PayrunJobQueue.cs` - Channel-based queue implementation
- `Backend.Server/PayrunJobWorkerService.cs` - Background worker service
- `Api/Api.Core/AcceptedObjectResult.cs` - HTTP 202 result helper

### Modified files
- `Api/Api.Controller/PayrunJobController.cs` - Async job creation + queue integration
- `Backend.Controller/PayrunJobController.cs` - Updated constructor and HTTP attributes
- `Backend.Server/ProviderStartupExtensions.cs` - DI registration
- `Backend.Server/Startup.cs` - Queue and worker service registration

## Breaking Changes
- `POST /api/tenants/{id}/payruns/jobs` now returns **HTTP 202 Accepted** instead of HTTP 201 Created
- Response includes `Location` header for status polling
- Clients must poll status endpoint to determine job completion
- Job is returned in "Process" status, not "Complete"

## Migration Guide
Clients should update their integration to:

```csharp
// 1. Create job - now returns immediately
var response = await client.PostAsync("/api/tenants/{id}/payruns/jobs", content);
if (response.StatusCode == HttpStatusCode.Accepted)
{
    var statusUrl = response.Headers.Location;
    var job = await response.Content.ReadFromJsonAsync<PayrunJob>();

    // 2. Poll for completion
    while (job.JobStatus == PayrunJobStatus.Process)
    {
        await Task.Delay(TimeSpan.FromSeconds(5));
        job = await client.GetFromJsonAsync<PayrunJob>(statusUrl);
    }

    // 3. Check final status
    if (job.JobStatus == PayrunJobStatus.Complete)
        // Success
    else if (job.JobStatus == PayrunJobStatus.Abort)
        // Check job.ErrorMessage
}
```

## Test plan
- [ ] Build succeeds: `dotnet build PayrollEngine.Backend.sln`
- [ ] Unit tests pass: `dotnet test Domain/Domain.Model.Tests/`
- [ ] POST new payrun job returns HTTP 202 with Location header
- [ ] GET status endpoint returns current job status
- [ ] ProcessedEmployeeCount increments during processing
- [ ] Job status changes to Complete when finished
- [ ] Error scenario: job marked as Abort with ErrorMessage
- [ ] Graceful shutdown: in-progress job marked as Abort
