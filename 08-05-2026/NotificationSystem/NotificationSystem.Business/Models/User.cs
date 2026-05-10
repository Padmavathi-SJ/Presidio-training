using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace NotificationSystem.Business.Models
{
    public class User
    {
        // private feilds for validation
        private string _name = string.Empty;
        private string _email = string.Empty;
        private string _phoneNumber = string.Empty;

        public bool ReceiveEmailNotifications { get; set; } = true; 
public bool ReceiveSmsNotifications { get; set; } = true;     

        public int Id { get; set; }

        public string Name
        {
            get => _name;
            set
            {
                // check null or empty
                if (string.IsNullOrWhiteSpace(value))
                {
                    throw new ArgumentException("Name cannot be empty, please enter a valid name.");
                }
                // check if name contains only letters or not
                if(!Regex.IsMatch(value, @"^[a-zA-Z\s\.\-']+$"))
                {
                    throw new ArgumentException("Name should not contain any numbers of special characters.");
                }
                _name = value.Trim();
            }
        }

        public string Email
        {
            get => _email;
            set
            {
                // check for empty value
                if (string.IsNullOrWhiteSpace(value))
                {
                    throw new ArgumentException("Email cannot be empty, please enter a valid email address.");
                }
                string trimmedEmail = value.Trim().ToLower();
                if (!trimmedEmail.Contains("@"))
                {
                    throw new ArgumentException("Invalid email format, email must contain '@' symbol.");
                }

                _email = trimmedEmail;
            }
        }

        public string PhoneNumber
        {
            get => _phoneNumber;
            set
            {
                // check if null or empty
                if (string.IsNullOrWhiteSpace(value))
                {
                    throw new ArgumentException("phonenum cannot be empty, please enter a valid phone number.");
                }
                string cleaned = new string(value.Where(char.IsDigit).ToArray());
                // validation to check if phone number length is correct or not
                if(cleaned.Length != 10 && cleaned.Length != 12)
                {
                    throw new ArgumentException("phonenum you entered is not has the correct length, please the valid phonenum with length 10 or 12");
                }
                _phoneNumber = cleaned;
            }
        }

        //Notification preferences
        public bool IsActive{get; set; } = true;
        public bool ReceiveEmailNotification {get; set; } = true;
        public bool ReceiveSmsNotification {get; set; } = true;
        public DateTime CreatedAt {get; set;}

        public override string ToString()
        {
            return $"{Id} - {Name} - {Email} - {PhoneNumber}";
        }

    }
}