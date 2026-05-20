using System;
using LibrarySystem.DataAccess.Enums;

namespace LibrarySystem.DataAccess.Entities
{
    public class Member{

    public int Id {get; set;}
    public string Name {get; set;} = string.Empty;
    public string Email {get; set;} = string.Empty;
    public string PhoneNum {get; set;} = string.Empty;
    public string Password {get; set;} = string.Empty;
    public MembershipType MembershipType {get; set;} = MembershipType.Basic;
    public MembershipStatus MembershipStatus {get; set;} = MembershipStatus.Active;
    public bool IsActive {get; set;} = true;
    public int AllowedBorrowingCount {get; set;}
    public int CurrentBorrowedCount {get; set;}
    public DateTime CreatedAt {get; set;} = DateTime.UtcNow;
    public DateTime UpdatedAt {get; set;} = DateTime.UtcNow;

    //navigation(foreign keys)
    public virtual ICollection<Borrowing> Borrowings {get; set;} = new List<Borrowing>();
    public virtual ICollection<Fine> Fines {get; set;} = new List<Fine>();
}

}