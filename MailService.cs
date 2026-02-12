using System;
using System.Collections.Generic;
using Azure;
using Azure.Communication.Email;
using Microsoft.Extensions.Logging;

public class MailService{
    public string connectionString;
    public EmailClient? emailClient;
    public MailService(){
        connectionString = Environment.GetEnvironmentVariable("COMMUNICATION_SERVICES_CONNECTION_STRING");
        if(connectionString == null){
            Console.WriteLine("Failed to get COMMUNICATION_SERVICES_CONNECTION_STRING.");
            return;
        }
        emailClient = new EmailClient(connectionString);
    }
    public async Task SendMail(string recip, string subject, string content){

        if(recip == ""){
            Console.WriteLine("No recipient defined for email.");
            return;
        }
        if(subject == ""){
            Console.WriteLine("No email subject. Default used.");
            subject = "no subject";
        }
        if(content == ""){
            Console.WriteLine("No email content");
            return;
        }
        if(emailClient == null){
            Console.WriteLine("Email client not initialized.");
            return;
        }

        EmailSendOperation emailSendOperation = await emailClient.SendAsync(
            Azure.WaitUntil.Started,
            "DoNotReply@6ee8648e-9c03-4afe-af7c-f21d996ec752.azurecomm.net",
            recip,
            subject,
            content
        );
        try
        {
            while (true)
            {
                await emailSendOperation.UpdateStatusAsync();
                if (emailSendOperation.HasCompleted)
                {
                    break;
                }
                await Task.Delay(100);
            }

            if (emailSendOperation.HasValue)
            {
                Console.WriteLine($"Email queued for delivery. Status = {emailSendOperation.Value.Status}");
            }
        }
        catch (RequestFailedException ex)
        {
            Console.WriteLine($"Email send failed with Code = {ex.ErrorCode} and Message = {ex.Message}");
        }

        string operationId = emailSendOperation.Id;
        Console.WriteLine($"Email operation id = {operationId}");
    }
}

