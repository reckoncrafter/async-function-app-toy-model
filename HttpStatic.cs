using System.Net;
using Grpc.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Company.Function;

public class HttpStatic{
    private readonly ILogger<HttpStatic> _logger;
    public HttpStatic(ILogger<HttpStatic> logger)
    {
        _logger = logger;
    }

    [Function(nameof(HttpStatic))]
    public static async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "{*path}")] HttpRequestData req,
        FunctionContext executionContext,
        string path = "index.html")
    {
        var logger = executionContext.GetLogger(nameof(HttpStatic));
        logger.LogInformation($"HttpStatic: GET {path}");

        var response = req.CreateResponse(HttpStatusCode.OK);
        response.Headers.Add("Content-Type", "text/html; charset=utf-8");

        if(path.Contains("..")) // block path traversal
        {
            return req.CreateResponse(HttpStatusCode.Forbidden);
        }

        string content = await File.ReadAllTextAsync($"wwwroot/{path}");
        await response.WriteStringAsync(content);
        return response;
    }
}