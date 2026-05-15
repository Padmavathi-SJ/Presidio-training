using System;
using System.Net;
using System.Transactions;
using NotificationSystem.Business.Interfaces;
using NotificationSystem.Business.Config;
using System.Net.Mail;

namespace NotificationSystem.Business.Notifications
{
    public class EmailNotification : INotification
    {
        private string _subject = string.Empty;
        private string _message = string.Empty;
        private string _recipient = string.Empty;
        private readonly SmtpConfig _smtpConfig;
        public int Id {get; set;}
        public int UserId {get; set;}
        public string UserName {get; set;} = string.Empty;
        public bool IsSent{get; set;}
        public DateTime SentAt {get; set;}
        public string? ErrorMessage{get; set;}

        public string Subject
        {
            get => _subject;
            set
            {
                    // check subject is null or not
                    if (string.IsNullOrWhiteSpace(value))
                    {
                        throw new ArgumentException("email subject cannot be empty, please enter a valid subject");
                    }
                    // check for minimum length
                    if(value.Length < 3)
                    {
                        throw new ArgumentException("Email subject should contains atleast 3 characters.");
                    }
                    // check for maximum length
                    if(value.Length > 200)
                    {
                        throw new ArgumentException("Email subject should not exceed 200 characters.");
                    }
                    _subject = value.Trim();
                
            }
        }

        public string Message
        {
            get => _message;
            set
            {
                //check for null
                if (string.IsNullOrEmpty(value))
                {
                    throw new ArgumentException("message content cannot be empty, please enter a valid message");
                }
                
                    //check for minimum length of messgae content
                    if(value.Length < 10)
                    {
                        throw new ArgumentException("message content should contain atlease 10 characters.");
                    }
                    // check for maximum length
                    if(value.Length > 2000)
                    {
                        throw new ArgumentException("message content should not exceed 1000 characters.");
                    }
                
                _message = value.Trim();
            }
        }

        public string Recipient
        {
            get => _recipient;
            set
            {
                // check for null 
                if (string.IsNullOrWhiteSpace(value))
                {
                    throw new ArgumentException("recipient should not be empty, please enter a valid recipient address");
                }
                    string trimmedEmail = value.Trim().ToLower();
                    if (!trimmedEmail.Contains("@"))
                    {
                        throw new ArgumentException("invalid email address, email must contain '@' symbol.");
                    }
                    _recipient = trimmedEmail;
                }
               
                }

                public EmailNotification(SmtpConfig smtpConfig)
        {
            _smtpConfig = smtpConfig ?? throw new ArgumentNullException(nameof(smtpConfig));
        }

        public EmailNotification(SmtpConfig smtpConfig, int userId, string userName, string subject, string message, string recipient) : this(smtpConfig)
        {
            UserId = userId;
            UserName = userName;
            Subject = subject;
            Message = message;
            Recipient = recipient;
            IsSent = false;
        }
        public string GetNotificationType() =>"Email";

     public async Task<bool> SendAsync()
        {
            try
            {
                // validate configuration
                if (string.IsNullOrEmpty(_smtpConfig.SenderEmail))
                {
                    throw new InvalidOperationException("SMTP SenderEmail not configured");

                }
                if (string.IsNullOrEmpty(_smtpConfig.SenderPassword))
                {
                    throw new InvalidOperationException("SMTP senderPassword not configured");


                }

                // create email message
                using var mailMessage = new MailMessage();
                mailMessage.From = new MailAddress(_smtpConfig.SenderEmail);
                mailMessage.To.Add(Recipient);
                mailMessage.Subject = Subject;
                mailMessage.Body = $@"

                Dear {UserName},
                {Message}

                ----
                Sent at: {DateTime.UtcNow:yyyy--MM-dd HH:mm:ss}
                Notification System
                ";
                mailMessage.IsBodyHtml = false;

                // Configure SMTP client
                using var smtpClient = new SmtpClient(_smtpConfig.Host, _smtpConfig.Port);
                smtpClient.EnableSsl = _smtpConfig.EnableSsl;
                smtpClient.Credentials = new NetworkCredential(_smtpConfig.SenderEmail, _smtpConfig.SenderPassword);
                smtpClient.Timeout = 10000; // 10 seconds timeout

                // Send email
                await smtpClient.SendMailAsync(mailMessage);

                IsSent = true;
                SentAt = DateTime.UtcNow;
                ErrorMessage = null;

                Console.WriteLine($"Email sent successfully: ");
                Console.WriteLine($"To: {Recipient}");
                Console.WriteLine($"Subject: {Subject}");
                Console.WriteLine($"Time: {SentAt:HH:mm:ss}");
                return true;
            } catch(SmtpException ex)
            {
                IsSent = false;
                ErrorMessage = $"SMTP Error: {ex.StatusCode} - {ex.Message}";
                Console.WriteLine($"\n Email SMTP Error: {ErrorMessage}");
                return false;
            } catch(Exception ex)
            {
                IsSent = false;
                ErrorMessage = ex.Message;
                Console.WriteLine($"\n Email Error: {ex.Message}");
                return false;
            }

        }

        public static EmailNotification Create(SmtpConfig smtpConfig, int userId, string userName, string subject, string message, string email)
        {
            return new EmailNotification(smtpConfig, userId, userName, subject, message, email);
        }
        public override string ToString()
        {
         
            return $"[{Id}] Email sent to {Recipient}, Subject: {Subject}, - {(IsSent ? "Sent": "Failed")} ";
            
        }

    }
    }

