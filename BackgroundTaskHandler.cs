namespace Company.Function;
using Company.Models;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Text.Json.Nodes;
using MongoDB.Driver;

using BackgroundTask = Func<System.Text.Json.Nodes.JsonNode, int, BackgroundTaskResult>;
using MongoDB.Bson;

public enum BackgroundTaskResult {
    Success,
    Failure
};


public static class BackgroundTaskHandler{
    private static readonly MailService mailService = new();
    //private static readonly MongoService mongo = new();

    private static readonly SqlService sql = new();
    static BackgroundTaskResult ShortTask(JsonNode data, int db_jobId)
    {
        try{
            sql.Update(db_jobId, jobDescription: "ShortTask: Simulating a short task...");
        }catch(Exception ex){
            Console.WriteLine($"Error updating job description for jobId: {db_jobId}. Exception: {ex}");
        }
        Task.Delay(1000).Wait();
        return BackgroundTaskResult.Success;
    }

    static BackgroundTaskResult MediumTask(JsonNode data, int db_jobId)
    {
        sql.Update(db_jobId, jobDescription: "MediumTask: Simulating a medium task...");
        Console.WriteLine($"MediumTask recieved: {data}");
        Task.Delay(5000).Wait();
        return BackgroundTaskResult.Success;
    }

    static BackgroundTaskResult LongTask(JsonNode data, int db_jobId)
    {
        sql.Update(db_jobId, jobDescription: "LongTask: Simulating a long task...");
        Console.WriteLine($"LongTask recieved: {data}");
        sql.Update(db_jobId, statusMessage: "Collecting monoids...");
        Task.Delay(10000).Wait();
        sql.Update(db_jobId, statusMessage: "Sheafifying endofunctors...");
        Task.Delay(10000).Wait();
        sql.Update(db_jobId, statusMessage: "Calculating rectilinear tensors...");
        Task.Delay(10000).Wait();
        return BackgroundTaskResult.Success;
    }

    static BackgroundTaskResult FailTask(JsonNode data, int db_jobId)
    {
        sql.Update(db_jobId, jobDescription: "FailTask: Simulating a task failure...");
        Console.WriteLine($"FailTask recieved: {data}");
        sql.Update(db_jobId, statusMessage: "Preparing for failure...");
        Task.Delay(5000).Wait();
        return BackgroundTaskResult.Failure;
    }
    public static string RequestNewBackgroundTask(JobData jobData, string userEmail){
        int newJobId = sql.NewJob();
        sql.Update(newJobId, jobDescription: $"Job submitted with name: {jobData.name}");
        sql.Update(newJobId, statusMessage: "Job in progress...");

        BackgroundTask f = jobData.name switch 
        {
            "short" => ShortTask,
            "medium" => MediumTask,
            "long" => LongTask,
            "fail" => FailTask,
            _ => (JsonNode data, int db_jobId) => BackgroundTaskResult.Failure
        };

        Task task = Task.Run(() => {
            Console.WriteLine($"Executing job with id: {newJobId} and name: {jobData.name}");
            BackgroundTaskResult r = f(jobData.data, newJobId);
            Console.WriteLine($"Job with id: {newJobId} completed with result: {r}");
            switch(r){
                case BackgroundTaskResult.Success:
                    sql.Update(newJobId, isCompleted: 1, isError: 0, dateCompleted: DateTime.Now, statusMessage: "Task has completed successfully");
                    break;
                case BackgroundTaskResult.Failure:
                    sql.Update(newJobId, isCompleted: 1, isError: 1, dateCompleted: DateTime.Now, statusMessage: "Task has failed.");
                    break;
                default:
                    break;
            }
            Console.WriteLine($"Job Completed: {JsonSerializer.Serialize(sql.Get(newJobId))}");
            string htmlContent = $"""
            <pre>
            JobId: {newJobId}
            Date Completed: {sql.Get(newJobId).dateCompleted}
            Status: {sql.Get(newJobId).statusMessage}
            Description: {sql.Get(newJobId).jobDescription}
            </pre>
            """;
            mailService.SendMail(userEmail, "Job Completion Notification", htmlContent);
        });

        return newJobId.ToString();
    }

    public static string GetJobStatus(string JobId)
    {
        string response;
        int jobIdInt = int.Parse(JobId);
        try{
            response = JsonSerializer.Serialize(sql.Get(jobIdInt));
        }
        catch(KeyNotFoundException){
            response = "{statusMessage: 'Job ID not found'}";
        }
        catch(Exception ex){
            Console.WriteLine($"Error retrieving job status for jobId: {jobIdInt}. Exception: {ex}");
            response = "{statusMessage: 'Error retrieving job status'}";
        }
        return response;
    }
}