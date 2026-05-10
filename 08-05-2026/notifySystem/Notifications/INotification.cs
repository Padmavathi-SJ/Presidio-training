using System;
using NotifySystem.Models;

namespace NotifySystem.Notifications{
    // interface: contract that defines the structure for notification classes, ensuring they implement the SendNotification method
    // this enforces consistent behavior across different notification types
    // when multiple classes need to provide same service but with different implementations, we can use interfaces to define the common behavior without distating how it is implemented.
    // and interfaces allows treating email and sms the same way(polymorphism)
    // when unrelated classes need to share common behavior(sending msg).

    public interface INotification{
        string Message { get; set; }

        DateTime SentDate  {get; set; }

        //method: every notification must define how to send itself
        // no imeplementation here, each class decides How t send
        Task Send(User recipient);

        // method: get delivery method name
        string GetDeliveryMethod();
    }
}

// KEY concept - Abstraction
// Interface hides the "HOW" (implementation details)
// only shows "what" (send a message) and "when" (sent date) without exposing the underlying code for sending notifications.
// so program works with INotification interface, not concrete classes(EmailNotification.cs, SMSNotification.cs)