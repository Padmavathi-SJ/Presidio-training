using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using NotifySystem.Models;
using NotifySystem.Config;

namespace NotifySystem.Notifications{
    // this class is implementing the INotification interface,
    // this provides a specific implementation for sending email notification

    // inheritance: EmailNotification inherits from INotification
    // Polymorphism: we can treat EmailNotification as an INotification, like type of INotification.

    public class EmailNotification: INotification{
        // properties required by INotification interface
        public string Message { get;  set; }
        public DateTime SentDate { get; set; }
        // Email specific property
        public string Subject {get; set;}
        private readonly SmtpConfig _smtpConfig;

        //  constructor overloading: it allows multiple ways to create email notification
        
        // constructor 1: 
        public EmailNotification(string message, SmtpConfig smtpConfig){
            Message = message;
            SentDate = DateTime.Now;
            Subject = "Notification from your bank";
            _smtpConfig = smtpConfig;
        }

        //constructor 2: 
        public EmailNotification(string message, string subject, SmtpConfig smtpConfig){
            Message = message;
            SentDate = DateTime.Now;
            Subject = subject;
            _smtpConfig = smtpConfig;
        }

        // method implementation: defines how to send email notification
        public async Task Send(User recipient){
            await SendEmailAsync(recipient);
        }

        // mail sending logic -> encapsulating the complexity of email sending within this method 
        private async Task SendEmailAsync(User recipient){
            try{
                Console.WriteLine($"\nAttempting to send email to: {recipient.Email}");
                
                // creating email message
                using (var message = new MailMessage()){
                    message.From = new MailAddress(_smtpConfig.SenderEmail);
                    message.To.Add(recipient.Email);
                    message.Subject = Subject;
                    message.Body = $@"
Dear {recipient.Name},

{Message}

Sent at: {SentDate:yyyy-MM-dd HH:mm:ss}

Thank you,
Notification System";

                    message.IsBodyHtml = false;

                    Console.WriteLine($"SMTP Host: {_smtpConfig.Host}:{_smtpConfig.Port}");
                    Console.WriteLine($"Sender: {_smtpConfig.SenderEmail}");
                   

                    // configure SMTP client
                    using (var client = new SmtpClient(_smtpConfig.Host, _smtpConfig.Port)){
                        client.EnableSsl = _smtpConfig.EnableSsl;
                        client.Credentials = new NetworkCredential(_smtpConfig.SenderEmail, _smtpConfig.SenderPassword);

                        //send email asynchronously to avoid blocking the main thread
                        await client.SendMailAsync(message);
                        Console.WriteLine($"Email sent successfully to {recipient.Email}");
                        Console.WriteLine($"Subject: {Subject}");
                        Console.WriteLine($"Time: {SentDate:HH:mm:ss}");
                    }
                }
            }
            catch (SmtpException ex){
                Console.WriteLine($"SMTP error: {ex.StatusCode}");
                Console.WriteLine($"Message: {ex.Message}");
                
                throw;
            }
            catch (Exception ex){
                Console.WriteLine($"Email failed: {ex.Message}");
                
                throw;
            }
        }

        public string GetDeliveryMethod() => "Email";
        public override string ToString() => $"[Email] {Subject}";

    }
}