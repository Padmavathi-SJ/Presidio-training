using System;

namespace LibrarySystem.Business.Exceptions
{
    public class NotFoundException : Exception
    {
        public string? EntityName { get; set; }
        public string? EntityId { get; set; }

        public NotFoundException() : base("The requested resource was not found.")
        {
        }

        public NotFoundException(string message) : base(message)
        {
        }

        public NotFoundException(string entityName, object entityId) 
            : base($"{entityName} with ID {entityId} was not found.")
        {
            EntityName = entityName;
            EntityId = entityId?.ToString();
        }

        public NotFoundException(string message, Exception innerException) 
            : base(message, innerException)
        {
        }
    }
}