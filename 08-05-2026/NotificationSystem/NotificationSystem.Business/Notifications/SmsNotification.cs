using System;
using System.Transactions;
using NotificationSystem.Business.Interfaces;

namespace NotificationSystem.Business.Notifications
{
    public class SmsNotification : INotification
    {
        private string _message = string.Empty;
        private string _recipient = string.Empty;

        public int Id {get; set;}
        public int UserId {get; set;}
        public string UserName {get; set;} = string.Empty;
        public bool IsSent{get; set;}
        public DateTime SentAt {get; set;}
        public string? ErrorMessage{get; set;}

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
                
                    // check for minimum length
                    if(value.Length < 5)
                    {
                        throw new ArgumentException("sms message content should contain atleast 5 characters.");
                    }
                    if(value.Length > 160)
                    {
                        throw new ArgumentException("sms message should not exceed 20 characters.");
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
                    string cleaned_phone_num = new string(value.Where(Char.IsDigit).ToArray());
                    // check for length
                    if(cleaned_phone_num.Length != 10)
                    {
                        throw new ArgumentException("Invalid phone number, please enter a valid phone num.");
                    }
                    _recipient = cleaned_phone_num;
                }
        }

         public string GetNotificationType() =>"Sms";

         public async Task<bool> SendAsync()
        {
            try
            {
                
                await Task.Delay(300);
                
                // Simulate 85% success rate for demo
                Random random = new Random();
                bool success = random.Next(1, 11) <= 8;
                
                if (success)
                {
                    IsSent = true;
                    SentAt = DateTime.Now;
                    ErrorMessage = null;

                    Console.WriteLine($"\nSMS SENT SUCCESSFULLY:");
                    Console.WriteLine($"To: {Recipient}");
                    Console.WriteLine($"Message: {Message}");
                    Console.WriteLine($"Time: {SentAt:HH:mm:ss}");
                }
                else
                {
                    IsSent = false;
                    ErrorMessage = "SMS delivery failed - gateway error";
                    Console.WriteLine($"\nSMS FAILED: Could not reach {Recipient}");
                }
                
                return success;
            }  catch (Exception ex)
            {
                IsSent = false;
                ErrorMessage = ex.Message;
                Console.WriteLine($"\nSMS ERROR: {ex.Message}");
                return false;
            }
        }

        public static SmsNotification Create(int userId, string userName, string message, string phoneNumber)
        {
            return new SmsNotification
            {
                UserId = userId,
                UserName = userName,
                Message = message,
                Recipient = phoneNumber,
                IsSent = false
            };
        }


           
        public override string ToString()
        {
            return $"[{Id}] SMS sent to {Recipient}, Message: {Message} - {(IsSent ? "yes": "no")}";
            }
        }
    
    }