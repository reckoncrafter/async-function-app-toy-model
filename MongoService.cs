namespace Company.Function;
using MongoDB.Driver;
using MongoDB.Bson;
using System.Text.Json;

public class MongoService {
    public string connectionString;
    public MongoClient client;
    public IMongoCollection<BsonDocument> collection;
    public MongoService(){
        connectionString = Environment.GetEnvironmentVariable("DOCUMENTDB_CONNECTION_STRING") ?? "mongodb://localhost:27017";
        client = new(connectionString);
        IMongoDatabase db = client.GetDatabase("GC_BACKGROUND_JOBS");
        IMongoCollection<BsonDocument> collection = db.GetCollection<BsonDocument>("JOBS");
        this.collection = collection;
    }
    // public StatusObject Get(string guid){
    //     var filter = Builders<BsonDocument>.Filter.Eq("guid", guid);
    //     var result = collection.Find(filter).FirstOrDefault();
    //     Console.WriteLine($"Retrieved job status for guid: {guid} with value: {result}");
    //     return new StatusObject(){
    //         message = result["message"].ToString(),
    //         running = bool.Parse(result["running"].ToString()),
    //         success = bool.Parse(result["success"].ToString())
    //     };
    // }

    // public void Update(string guid, string field, string value){
    //     var filter = Builders<BsonDocument>.Filter.Eq("guid", guid);
    //     var update = Builders<BsonDocument>.Update.Set(field, value);
    //     collection.UpdateOne(filter, update);
    // }

    // public void Add(string guid, StatusObject status){
    //     Console.WriteLine($"Adding new job status for guid: {guid} with value: {JsonSerializer.Serialize(status)}");
    //     BsonDocument newJob = new BsonDocument{
    //         ["guid"] = guid,
    //         ["message"] = status.message,
    //         ["running"] = status.running,
    //         ["success"] = status.success
    //     };
    //     collection.InsertOne(newJob);
    // }
}