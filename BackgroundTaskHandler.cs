namespace Company.Function;
using Company.Models;
using Microsoft.Extensions.Logging;
using System.Text.Json.Nodes;

using BackgroundTask = Func<System.Text.Json.Nodes.JsonNode, string, BackgroundTaskResult>;

public enum BackgroundTaskResult {
    Success,
    Failure
};

public static class BackgroundTaskHandler{
    static Dictionary<string, string> JobStatus = new();

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
        JobStatus[guid] = "Collecting monoids...";
        Console.WriteLine(JobStatus[guid]);
        Task.Delay(10000).Wait();
        JobStatus[guid] = "Sheafifying endofunctors...";
        Console.WriteLine(JobStatus[guid]);
        Task.Delay(10000).Wait();
        JobStatus[guid] = "Calculating rectilinear tensors...";
        Console.WriteLine(JobStatus[guid]);
        Task.Delay(10000).Wait();
        return BackgroundTaskResult.Success;
    }
    public static string RequestNewBackgroundTask(JobData jobData){
        string newGuid = Guid.NewGuid().ToString();

        BackgroundTask f = jobData.name switch 
        {
            "short" => ShortTask,
            "medium" => MediumTask,
            "long" => LongTask,
            _ => (JsonNode data, string guid) => BackgroundTaskResult.Failure
        };

        Task task = Task.Run(() => {
            BackgroundTaskResult r = f(jobData.data, newGuid);
            switch(r){
                case BackgroundTaskResult.Success:
                    JobStatus[newGuid] = "Task has completed successfully";
                    break;
                case BackgroundTaskResult.Failure:
                    JobStatus[newGuid] = "Task has failed.";
                    break;
                default:
                    break;
            }
            Console.WriteLine(JobStatus[newGuid]);
        });

        JobStatus.Add(newGuid, "Job in progress...");
        return newGuid;
    }

    public static string GetJobStatus(string guid)
    {
        string response;
        try{
            response = JobStatus[guid];
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