namespace Company.Function;
using Company.Models;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;
using System.Text.Json;
using System.Text.Json.Nodes;
using MongoDB.Driver;

using BackgroundTask = Func<System.Text.Json.Nodes.JsonNode, string, BackgroundTaskResult>;
using Grpc.Core;
using MongoDB.Bson;

public enum BackgroundTaskResult {
    Success,
    Failure
};

public class StatusObject {
    public string message {get; set;} = "";
    public bool running {get; set;} = false;
    public bool success {get; set;} = false;
}

public class MongoService {
    public const string connectionString = "mongodb://localhost:27017";
    public MongoClient client = new(connectionString);
    public IMongoCollection<BsonDocument> collection;
    public MongoService(){
        IMongoDatabase db = client.GetDatabase("GC_BACKGROUND_JOBS");
        IMongoCollection<BsonDocument> collection = db.GetCollection<BsonDocument>("JOBS");
        this.collection = collection;
    }
    public StatusObject Get(string guid){
        var filter = Builders<BsonDocument>.Filter.Eq("guid", guid);
        var result = collection.Find(filter).FirstOrDefault();
        Console.WriteLine($"Retrieved job status for guid: {guid} with value: {result}");
        return new StatusObject(){
            message = result["message"].ToString(),
            running = bool.Parse(result["running"].ToString()),
            success = bool.Parse(result["success"].ToString())
        };
    }

    public void Update(string guid, string field, string value){
        var filter = Builders<BsonDocument>.Filter.Eq("guid", guid);
        var update = Builders<BsonDocument>.Update.Set(field, value);
        collection.UpdateOne(filter, update);
    }

    public void Add(string guid, StatusObject status){
        Console.WriteLine($"Adding new job status for guid: {guid} with value: {JsonSerializer.Serialize(status)}");
        BsonDocument newJob = new BsonDocument{
            ["guid"] = guid,
            ["message"] = status.message,
            ["running"] = status.running,
            ["success"] = status.success
        };
        collection.InsertOne(newJob);
    }
}



public static class BackgroundTaskHandler{
    
    //private static readonly MailService mailService = new();

    private static readonly MongoService mongo = new();
    
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
        //JobStatus[guid].message = "Collecting monoids...";
        mongo.Update(guid, "message", "Collecting monoids...");
        Task.Delay(10000).Wait();
        //JobStatus[guid].message = "Sheafifying endofunctors...";
        mongo.Update(guid, "message", "Sheafifying endofunctors...");
        Task.Delay(10000).Wait();
        //JobStatus[guid].message = "Calculating rectilinear tensors...";
        mongo.Update(guid, "message", "Calculating rectilinear tensors...");
        Task.Delay(10000).Wait();
        return BackgroundTaskResult.Success;
    }

    static BackgroundTaskResult FailTask(JsonNode data, string guid)
    {
        Console.WriteLine($"FailTask recieved: {data}");
        //JobStatus[guid].message = "Preparing for failure...";
        mongo.Update(guid, "message", "Preparing for failure...");
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
                    // JobStatus[newGuid].message = "Task has completed successfully";
                    // JobStatus[newGuid].running = false;
                    // JobStatus[newGuid].success = true;
                    mongo.Update(newGuid, "message", "Task has completed successfully");
                    mongo.Update(newGuid, "running", "false");
                    mongo.Update(newGuid, "success", "true");
                    break;
                case BackgroundTaskResult.Failure:
                    // JobStatus[newGuid].message = "Task has failed.";
                    // JobStatus[newGuid].running = false;
                    // JobStatus[newGuid].success = false;
                    mongo.Update(newGuid, "message", "Task has failed.");
                    mongo.Update(newGuid, "running", "false");
                    mongo.Update(newGuid, "success", "false");
                    break;
                default:
                    break;
            }
            //Console.WriteLine($"Job Completed: {JsonSerializer.Serialize(JobStatus[newGuid])}");
            Console.WriteLine($"Job Completed: {JsonSerializer.Serialize(mongo.Get(newGuid))}");
            string htmlContent = $"""
            <pre>
            {newGuid}: {mongo.Get(newGuid).message}
            </pre>
            """;
            //mailService.SendMail(userEmail, "Job Completion Notification", htmlContent);
        });

        // JobStatus.Add(newGuid, new StatusObject(){
        //     message = "Job in progress...",
        //     running = true,
        //     success = false
        // });

        mongo.Add(newGuid, new StatusObject(){
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
            //response = JsonSerializer.Serialize(JobStatus[guid]);
            response = JsonSerializer.Serialize(mongo.Get(guid));
        }
        catch(KeyNotFoundException){
            response = "No status associated with this JobId";
        }
        catch{
            response = "Unknown error.";
        }
        return response;
    }

    public static void PurgeDatabase(){
        mongo.collection.DeleteMany(new BsonDocument());
    }
}