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
        return new OkObjectResult(response);
    }
}