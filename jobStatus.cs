using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using Company.Models;

namespace Company.Function;

public class jobStatus
{
    private readonly ILogger<jobStatus> _logger;

    public jobStatus(ILogger<jobStatus> logger)
    {
        _logger = logger;
    }

    [Function("jobStatus")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "jobStatus/{guid}")] 
        HttpRequest req,
        string guid)
    {
        string response = BackgroundTaskHandler.GetJobStatus(guid);
       _logger.LogInformation($"x-functions-key: [{req.Headers["x-functions-key"]}]");
        return new OkObjectResult(response);
    }
}

public class purgeDatabase
{
    private readonly ILogger<purgeDatabase> _logger;

    public purgeDatabase(ILogger<purgeDatabase> logger)
    {
        _logger = logger;
    }

    [Function("purgeDatabase")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Admin, "delete", Route = "purgeDatabase")] 
        HttpRequest req)
    {
        BackgroundTaskHandler.PurgeDatabase();
        return new OkObjectResult("Database Purged");
    }
}