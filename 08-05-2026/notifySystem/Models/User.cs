using System;

namespace NotifySystem.Models{
    // this encapsulate all user-related data in one place
    // WE CAN control how user data is accessed and modified through properties and methods in this class.
    // defining user class 
    
    public class User{
        // properties to store user information, and control access to private fields
        // allows validation and controlled modification
        public string Name { get; set; }
        public string Email { get; set; }
        public string PhoneNumber {get; set; }

        public User(string name, string email, string phoneNumber){
            Name = name;
            Email = email;
            PhoneNumber = phoneNumber;

        }

        // method overriding: custom string representation of user object for easy display
        // if we need meaningful text representation of user objects instead of default types
        public override string ToString(){
            return $"Name: {Name}\nEmail: {Email}\nPhone: {PhoneNumber}";
        }
    }
}