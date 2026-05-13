using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityFramework.Models
{
    public class Customer
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        
        private DateTime _dateOfBirth;
        public DateTime DateOfBirth 
        { 
            get => _dateOfBirth;
            set => _dateOfBirth = DateTime.SpecifyKind(value, DateTimeKind.Utc);
        }
    }
}