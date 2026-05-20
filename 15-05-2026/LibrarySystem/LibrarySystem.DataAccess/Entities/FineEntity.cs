using System;
using LibrarySystem.DataAccess.Enums;

namespace LibrarySystem.DataAccess.Entities
{
    public class Fine{

    public int Id {get; set;}
    public int BorrowingId {get; set;}
    public int MemberId {get; set;}
    public decimal FineAmount {get; set;}
    public string FineReason {get; set;} = string.Empty;
    public FinePaymentStatus PaymentStatus {get; set;} = FinePaymentStatus.Pending;
    public DateTime? PaymentDate {get; set;}
    public bool IsActive { get; set; } = true; 
    public DateTime CreatedAt {get; set;} = DateTime.UtcNow;
    public DateTime UpdatedAt {get; set;} = DateTime.UtcNow;

    public virtual Member Member {get; set;} = null!;
    public virtual Borrowing Borrowing {get; set;} = null!;

}
}