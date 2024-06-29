using Mailjet.Client;
using Mailjet.Client.Resources;
using Newtonsoft.Json.Linq;

namespace Ecommerce.Templates
{
    public class EmailSender(string mailjetApiKey, string mailjetApiSecret)
    {
        public async Task SendEmailAsync(string recipientEmail, string recipientName, string confirmationLink)
        {
            var client = new MailjetClient(mailjetApiKey, mailjetApiSecret);

            var htmlContent = $$"""
                                
                                                <html>
                                                <head>
                                                    <style>
                                                        .email-container {
                                                            font-family: Arial, sans-serif;
                                                            padding: 20px;
                                                            color: #333333;
                                                        }
                                                        .email-header {
                                                            background-color: #f4f4f4;
                                                            padding: 10px;
                                                            border-bottom: 1px solid #dddddd;
                                                        }
                                                        .email-content {
                                                            margin-top: 20px;
                                                        }
                                                        .email-footer {
                                                            margin-top: 30px;
                                                            font-size: 12px;
                                                            color: #777777;
                                                        }
                                                        .button {
                                                            display: inline-block;
                                                            padding: 10px 20px;
                                                            font-size: 14px;
                                                            color: #ffffff;
                                                            background-color: #007BFF;
                                                            text-decoration: none;
                                                            border-radius: 5px;
                                                        }
                                                    </style>
                                                </head>
                                                <body>
                                                    <div class='email-container'>
                                                        <div class='email-header'>
                                                            <h2>Email Confirmation</h2>
                                                        </div>
                                                        <div class='email-content'>
                                                            <p>Hello {{recipientName}},</p>
                                                            <p>Please confirm your email by clicking the link below:</p>
                                                            <a href='{{confirmationLink}}' class='button'>Confirm Email</a>
                                                        </div>
                                                        <div class='email-footer'>
                                                            <p>If you did not request this email, please ignore it.</p>
                                                        </div>
                                                    </div>
                                                </body>
                                                </html>
                                """;

            var request = new MailjetRequest
                {
                    Resource = Send.Resource,
                }
                .Property(Send.FromEmail, "kalismith220022@gmail.com")
                .Property(Send.FromName, "Kali Smith")
                .Property(Send.Subject, "Email Confirmation")
                .Property(Send.HtmlPart, htmlContent)
                .Property(Send.Recipients, new JArray {
                    new JObject {
                        { "Email", recipientEmail },
                        { "Name", recipientName }
                    }
                });

            try
            {
                var response = await client.PostAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"Email sent successfully to {recipientEmail}");
                    Console.WriteLine($"Total: {response.GetTotal()}, Count: {response.GetCount()}\n");
                    Console.WriteLine(response.GetData());
                    Console.WriteLine(confirmationLink);
                }
                else
                {
                    Console.WriteLine($"Failed to send email to {recipientEmail}");
                    Console.WriteLine($"StatusCode: {response.StatusCode}\n");
                    Console.WriteLine($"ErrorInfo: {response.GetErrorInfo()}\n");
                    Console.WriteLine(response.GetData());
                    Console.WriteLine($"ErrorMessage: {response.GetErrorMessage()}\n");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Mailjet API request failed: {ex.Message}");
                // Handle exception as per your application's error handling strategy
            }
        }
    }
}
