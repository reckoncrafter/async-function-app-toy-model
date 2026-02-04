namespace Company.Function;
using Company.Models;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;
using System.Text.Json;
using System.Text.Json.Nodes;

using BackgroundTask = Func<System.Text.Json.Nodes.JsonNode, string, BackgroundTaskResult>;

public enum BackgroundTaskResult {
    Success,
    Failure
};

public class StatusObject {
    public string message {get; set;} = "";
    public bool running {get; set;} = false;
    public bool success {get; set;} = false;
}

public static class BackgroundTaskHandler{
    static Dictionary<string, StatusObject> JobStatus = new();
    private static readonly MailService mailService = new();
    static BackgroundTaskResult ShortTask(JsonNode data, string guid)
    {
        Console.WriteLine($"ShortTask recieved: {data}");
        Task.Delay(1000).Wait();
        return BackgroundTaskResult.Success;
    }

    static BackgroundTaskResult MediumTask(JsonNode data, string guid)
    {
        Console.WriteLine($"MediumTask recieved: {data}");
        Task.Delay(5000).Wait();
        return BackgroundTaskResult.Success;
    }

    static BackgroundTaskResult LongTask(JsonNode data, string guid)
    {
        Console.WriteLine($"LongTask recieved: {data}");
        JobStatus[guid].message = "Collecting monoids...";
        Console.WriteLine(JobStatus[guid]);
        Task.Delay(10000).Wait();
        JobStatus[guid].message = "Sheafifying endofunctors...";
        Console.WriteLine(JobStatus[guid]);
        Task.Delay(10000).Wait();
        JobStatus[guid].message = "Calculating rectilinear tensors...";
        Console.WriteLine(JobStatus[guid]);
        Task.Delay(10000).Wait();
        return BackgroundTaskResult.Success;
    }

    static BackgroundTaskResult FailTask(JsonNode data, string guid)
    {
        Console.WriteLine($"FailTask recieved: {data}");
        JobStatus[guid].message = "Preparing for failure...";
        Console.WriteLine(JobStatus[guid]);
        Task.Delay(5000).Wait();
        return BackgroundTaskResult.Failure;
    }
    public static string RequestNewBackgroundTask(JobData jobData, string userEmail){
        string newGuid = Guid.NewGuid().ToString();

        BackgroundTask f = jobData.name switch 
        {
            "short" => ShortTask,
            "medium" => MediumTask,
            "long" => LongTask,
            "fail" => FailTask,
            _ => (JsonNode data, string guid) => BackgroundTaskResult.Failure
        };

        Task task = Task.Run(() => {
            BackgroundTaskResult r = f(jobData.data, newGuid);
            switch(r){
                case BackgroundTaskResult.Success:
                    JobStatus[newGuid].message = "Task has completed successfully";
                    JobStatus[newGuid].running = false;
                    JobStatus[newGuid].success = true;
                    break;
                case BackgroundTaskResult.Failure:
                    JobStatus[newGuid].message = "Task has failed.";
                    JobStatus[newGuid].running = false;
                    JobStatus[newGuid].success = false;
                    break;
                default:
                    break;
            }
            Console.WriteLine(JobStatus[newGuid]);
            string htmlContent = $"""
            <pre>
            {newGuid}: {JobStatus[newGuid].message}
            </pre>
            """;
            mailService.SendMail(userEmail, "Job Completion Notification", htmlContent);
        });

        JobStatus.Add(newGuid, new StatusObject(){
            message = "Job in progress...",
            running = true,
            success = false
        });
        return newGuid;
    }

    public static string GetJobStatus(string guid)
    {
        string response;
        try{
            response = JsonSerializer.Serialize(JobStatus[guid]);
        }
        catch(KeyNotFoundException){
            response = "No status associated with this JobId";
        }
        catch{
            response = "Unknown error.";
        }
        return response;
    }
}