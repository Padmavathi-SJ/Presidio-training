using System;

namespace LibrarySystem.DataAccess.Entities
{
    public class Admin{
    public int Id {get; set;}
    public string Name {get; set;} = string.Empty;
    public string PhoneNum {get; set;} = string.Empty;
    public string Password {get; set;} = string.Empty;
    public bool IsActive {get; set;}
    public DateTime CreatedAt {get; set;} = DateTime.UtcNow;
    public DateTime UpdatedAt {get; set;} = DateTime.UtcNow;
}
}