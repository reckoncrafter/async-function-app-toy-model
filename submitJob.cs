using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Company.Models;
using Grpc.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Company.Function;

public class submitJob
{
    private readonly ILogger<submitJob> _logger;

    public submitJob(ILogger<submitJob> logger)
    {
        _logger = logger;
    }

    [Function("submitJob")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "submitJob")] 
        HttpRequest req)
    {

        _logger.LogInformation("Job submission recieved.");
        string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

        _logger.LogInformation($"x-functions-key: [{req.Headers["x-functions-key"]}]");
        _logger.LogInformation($"x-notification-email: [{req.Headers["x-notification-email"]}]");
        _logger.LogInformation(requestBody);

        JsonNode ? jobJson;
        try{
            jobJson = JsonNode.Parse(requestBody);
        }catch(JsonException){
            return new ObjectResult("Malformed request."){
                StatusCode = 400
            };
        }

        if(jobJson == null){
            return new ObjectResult("Unexpected Error."){
                StatusCode = 500
            };
        }

        JobData jobData;
        try{
            jobData = new(){
                name = (string)jobJson["name"],
                data = jobJson["data"]
            };
        }catch{
            return new BadRequestObjectResult("Malformed JSON.");
        }


        string jobGuid = BackgroundTaskHandler.RequestNewBackgroundTask(jobData, req.Headers["x-notification-email"]);
        
        var response = new Dictionary<string, string>(){
            {"jobId", jobGuid},
            {"jobStatus", BackgroundTaskHandler.GetJobStatus(jobGuid)}
        };

        return new OkObjectResult(JsonSerializer.Serialize(response));
    }
}