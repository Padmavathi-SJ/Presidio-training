using System;

namespace LibrarySystem.Business.Exceptions
{
    public class BusinessRuleException : Exception
    {
        public string RuleCode { get; set; }

        public BusinessRuleException() : base("Business rule violation occurred.")
        {
            RuleCode = string.Empty;
        }

        public BusinessRuleException(string message) : base(message)
        {
            RuleCode = string.Empty;
        }

        public BusinessRuleException(string ruleCode, string message) : base(message)
        {
            RuleCode = ruleCode;
        }

        public BusinessRuleException(string message, Exception innerException) 
            : base(message, innerException)
        {
            RuleCode = string.Empty;
        }
    }
}