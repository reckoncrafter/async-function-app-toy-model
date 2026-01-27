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

        _logger.LogInformation(requestBody);

        JsonNode ? jobJson = JsonNode.Parse(requestBody); 
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


        string jobGuid = BackgroundTaskHandler.RequestNewBackgroundTask(jobData);
        
        var response = new Dictionary<string, string>(){
            {"jobId", jobGuid},
            {"jobStatus", BackgroundTaskHandler.GetJobStatus(jobGuid)}
        };

        return new OkObjectResult(JsonSerializer.Serialize(response));
    }
}