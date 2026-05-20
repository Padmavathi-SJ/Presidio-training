using System;
using LibrarySystem.DataAccess.Enums;

namespace LibrarySystem.DataAccess.Entities
{
    public class Borrowing{
    public int Id { get; set;}
    public int MemberId { get; set;}
    public int BookCopyId { get; set;}
    public int BookId {get; set;}
    public DateTime BorrowedDate {get; set;} = DateTime.UtcNow;
    public DateTime DueDate {get; set;} = DateTime.UtcNow;
    public BookBorrowStatus Status {get; set;} = BookBorrowStatus.Borrowed;
    public DateTime? MemberReturnedDate {get; set;}
    public decimal FineAmount {get; set;} = 0;
    public bool IsActive {get; set;} = true;
     public DateTime CreatedAt {get; set;} = DateTime.UtcNow;
    public DateTime UpdatedAt {get; set;} = DateTime.UtcNow;

    public virtual Member Member {get; set;} = null!;
    public virtual BookCopy BookCopy {get; set;} = null!;
    public virtual Book Book {get; set;} = null!;
    public virtual ICollection<Fine> Fines {get; set;} = new List<Fine>();
    
    }
}