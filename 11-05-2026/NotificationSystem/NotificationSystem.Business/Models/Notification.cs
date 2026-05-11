using System;
using System.Text.RegularExpressions;

namespace NotificationSystem.Business.Models
{
    public enum NotificationType
    {
        Email = 1,
        Sms = 2
    }

    public class Notification
    {
        // private feilds for validation
        private string _subject = string.Empty;
        private string _message = string.Empty;
        private string _recipient = string.Empty;

        // basic properties(no validation required)
        public int Id {get; set;}
        public int UserId { get; set; }
        public NotificationType Type {get; set;}

        // status properties
        public bool IsSent {get; set;}
        public DateTime SentAt { get; set; }
        public string? ErrorMessage { get; set;}

        // subject property
        public string Subject
        {
            get => _subject;
            set
            {
                if(Type == NotificationType.Email)
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
                else
                {
                    _subject = string.Empty;
                }
            }
        }

        public string Message
        {
            get => _message;
            set
            {
                //check for null
                if (string.IsNullOrWhiteSpace(value))
                {
                    throw new ArgumentException("message content cannot be empty, please enter a valid message");
                }
                
                if(Type == NotificationType.Email)
                {
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
                } else if(Type == NotificationType.Sms)
                {
                    // check for minimum length
                    if(value.Length < 5)
                    {
                        throw new ArgumentException("sms message content should contain atleast 5 characters.");
                    }
                    if(value.Length > 160)
                    {
                        throw new ArgumentException("sms message should not exceed 20 characters.");
                    }
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
                // check for notification type
                if(Type == NotificationType.Email)
                {
                    string trimmedEmail = value.Trim().ToLower();
                    if (!trimmedEmail.Contains("@"))
                    {
                        throw new ArgumentException("invalid email address, email must contain '@' symbol.");
                    }
                    _recipient = trimmedEmail;
                }
                else if(Type == NotificationType.Sms)
                {
                    string cleaned_phone_num = new string(value.Where(Char.IsDigit).ToArray());
                    // check for length
                    if(cleaned_phone_num.Length != 10)
                    {
                        throw new ArgumentException("Invalid phone number, please enter a valid phone num.");
                    }
                    _recipient = cleaned_phone_num;
                }
            }
        }

        public static Notification CreateEmailNotification(int user_id, string subject, string message, string email)
        {
            return new Notification{
            UserId = user_id,
            Type = NotificationType.Email,
            Subject = subject,
            Message = message,
            Recipient = email,
            IsSent = false
            };
        }

        public static Notification CreateSmsNotification(int user_id, string message, string phone_num)
        {
            return new Notification{
            UserId = user_id,
            Type = NotificationType.Sms,
            Message = message,
            Recipient = phone_num,
            IsSent = false
        };
        }

        // Method to send notification (simulate)
        public async Task<bool> SendAsync()
        {
            try
            {
                await Task.Delay(500); // Simulate network delay
                
                Random random = new Random();
                bool success = Type == NotificationType.Email ? random.Next(1, 11) <= 9 : random.Next(1, 11) <= 8;
                
                if (success)
                {
                    IsSent = true;
                    SentAt = DateTime.Now;
                    ErrorMessage = null;
                    Console.WriteLine($"\n{(Type == NotificationType.Email ? "Email" : "SMS")} sent to {Recipient}");
                }
                else
                {
                    IsSent = false;
                    ErrorMessage = $"{(Type == NotificationType.Email ? "Email" : "SMS")} delivery failed";
                }
                return success;
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
                return false;
            }

        }

        public override string ToString()
        {
          if(Type == NotificationType.Email)
            {
                return $"[{Id}] Email sent to {Recipient}, Subject: {Subject} ";
            }
            else
            {
                return $"[{Id}] SMS sent to {Recipient}, Message: {Message}";
            }
        }
    }
}