namespace Company.Function;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.SqlServer;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class BackgroundJobObject {
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int jobId {get; set;}
    public byte isCompleted {get; set;}
    public byte isError {get; set;}
    public DateTime dateCompleted {get; set;}
    [StringLength(256)]
    public string statusMessage {get; set;} = "";
    [StringLength(512)]
    public string jobDescription {get; set;} = "";
}

public enum BackgroundJobField {
    isCompleted,
    isError,
    dateCompleted,
    statusMessage,
    jobDescription
}
public class BackgroundJobContext : DbContext
{
    public BackgroundJobContext(DbContextOptions<BackgroundJobContext> options) : base(options) { }
    public DbSet<BackgroundJobObject> BackgroundJobs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BackgroundJobObject>(entity =>
        {
            entity.ToTable("BackgroundJobs");
            entity.HasKey(e => e.jobId);
            entity.Property(e => e.isCompleted).IsRequired();
            entity.Property(e => e.isError).IsRequired();
            entity.Property(e => e.dateCompleted).HasColumnType("datetime").IsRequired();
            entity.Property(e => e.statusMessage).HasMaxLength(256);
            entity.Property(e => e.jobDescription).HasMaxLength(512);
        });
    }
}
public class SqlService {
    private string connectionString = Environment.GetEnvironmentVariable("SQL_SERVER_CONNECTION_STRING");
    private BackgroundJobContext context;

    public Mutex sqlMutex = new Mutex();
    public SqlService(){
        if(connectionString == null){
            Console.WriteLine("Failed to get SQL_SERVER_CONNECTION_STRING.");
            throw new Exception("SQL_SERVER_CONNECTION_STRING environment variable not set.");
        }
        var optionsBuilder = new DbContextOptionsBuilder<BackgroundJobContext>();
        optionsBuilder.UseSqlServer(connectionString);
        context = new BackgroundJobContext(optionsBuilder.Options);
    }

    public BackgroundJobObject Get(int jobId){
        sqlMutex.WaitOne();
        var result = context.BackgroundJobs.Where(job => job.jobId == jobId).FirstOrDefault();
        Console.WriteLine($"Retrieved job status for jobId: {jobId} with value: {result}");
        sqlMutex.ReleaseMutex();
        return result;
    }

    public void Update(int jobId, byte? isCompleted = null, byte? isError = null, DateTime? dateCompleted = null, string? statusMessage = null, string? jobDescription = null){
        sqlMutex.WaitOne();
        try{
            var job = context.BackgroundJobs.Where(job => job.jobId == jobId).FirstOrDefault();
            if (job != null){
                if (isCompleted.HasValue)
                    job.isCompleted = isCompleted.Value;
                if (isError.HasValue)
                    job.isError = isError.Value;
                if (dateCompleted.HasValue)
                    job.dateCompleted = dateCompleted.Value;
                if (statusMessage != null)
                    job.statusMessage = statusMessage;
                if (jobDescription != null)
                    job.jobDescription = jobDescription;
                
                context.SaveChanges();
            }
            else{
                Console.WriteLine($"Attempted to update jobId: {jobId} but it was not found.");
            }
        }
        finally{
            sqlMutex.ReleaseMutex();
        }
    }

    public int NewJob(){
        sqlMutex.WaitOne();
        try{
            BackgroundJobObject newJob = new BackgroundJobObject(){
                isCompleted = 0,
                isError = 0,
                dateCompleted = DateTime.Now,
                statusMessage = "uninitialized",
                jobDescription = "No description provided"
            };
            var result = context.BackgroundJobs.Add(newJob);
            context.SaveChanges();
            sqlMutex.ReleaseMutex();
            return result.Entity.jobId;
        }
        catch(Exception ex){
            Console.WriteLine($"Error creating new job: {ex.Message}");
            Console.WriteLine(ex.InnerException?.Message);
            sqlMutex.ReleaseMutex();
            throw;
        }

    }
}