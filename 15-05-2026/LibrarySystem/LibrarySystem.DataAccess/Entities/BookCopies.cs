using System;

namespace LibrarySystem.DataAccess.Entities
{
    public class BookCopy{

    public int Id { get; set;}
    public int BookId {get; set;}
    public int BookCopyId {get; set;}
    public bool IsAvailable {get; set;} = true;
    public bool IsBorrowed {get; set;} = false;
    public bool IsDamaged {get; set;} = false;
    public string ConditionNotes {get; set;} = string.Empty;
    public DateTime CreatedAt {get; set;} = DateTime.UtcNow;
    public DateTime UpdatedAt {get; set;} = DateTime.UtcNow;

    public virtual Book Book {get; set;} = null!;
    public virtual ICollection<Borrowing> Borrowings {get; set;} = new List<Borrowing>();
}
}