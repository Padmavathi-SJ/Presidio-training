using System;
using LibrarySystem.DataAccess.Enums;

namespace LibrarySystem.DataAccess.Entities
{
    public class BorrowingRules{
    public int Id {get; set;}
    public MembershipType MembershipType{get; set;} = MembershipType.Basic;
    public int MaxActiveBorrowings {get; set;}
    public int MaxBorrowDays {get; set;}
    public DateTime CreatedAt {get; set;} = DateTime.UtcNow;
     public DateTime UpdatedAt {get; set;} = DateTime.UtcNow;
    }
}