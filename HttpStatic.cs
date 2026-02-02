using System.Net;
using Grpc.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Net.Mime;

namespace Company.Function;

public class HttpStatic{
    private readonly ILogger<HttpStatic> _logger;
    public HttpStatic(ILogger<HttpStatic> logger)
    {
        _logger = logger;
    }

    private static string ContentType(string path){
        string ext = Path.GetExtension(path);
        return ext switch
        {
            ".js" => "text/javascript",
            ".html" => "text/html",
            ".css" => "text/css",
            _ => "text/plain",
        };
    }

    [Function(nameof(HttpStatic))]
    public static async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "static/{*path:file}")] HttpRequestData req,
        FunctionContext executionContext,
        string path)
    {
        var logger = executionContext.GetLogger(nameof(HttpStatic));
        logger.LogInformation($"HttpStatic: GET {path}");

        var response = req.CreateResponse(HttpStatusCode.OK);

        response.Headers.Add("Content-Type", $"{ContentType(path)}; charset=utf-8");

        if(path.Contains("..")) // block path traversal
        {
            return req.CreateResponse(HttpStatusCode.Forbidden);
        }
        string content;

        try{
            content = await File.ReadAllTextAsync($"wwwroot/{path}");
        }
        catch{
            return req.CreateResponse(HttpStatusCode.NotFound);
        }
        await response.WriteStringAsync(content);
        return response;
    }
}
