using System;
using Microsoft.EntityFrameworkCore;
using LibrarySystem.DataAccess.Context;
using LibrarySystem.DataAccess.Entities;
using LibrarySystem.DataAccess.Enums;
using LibrarySystem.DataAccess.Repositories;
using LibrarySystem.Business.Exceptions;


namespace LibrarySystem.Business.Services
{
    public class BorrowingService : IBorrowingService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMemberRepository _memberRepository;
        private readonly IBorrowingRepository _borrowingRepository;
        private readonly IBookCopyRepository _bookCopyRepository;

        public BorrowingService(
            ApplicationDbContext context,
            IMemberRepository memberRepository,
            IBorrowingRepository borrowingRepository,
            IBookCopyRepository bookCopyRepository)
        {
            _context = context;
            _memberRepository = memberRepository;
            _borrowingRepository = borrowingRepository;
            _bookCopyRepository = bookCopyRepository;
        }




public async Task<List<Borrowing>> GetAllBorrowingsAsync()
{
    return await _context.Borrowings
        .Include(b => b.Member)
        .Include(b => b.Book)
        .Include(b => b.BookCopy)
        .OrderByDescending(b => b.BorrowedDate)
        .ToListAsync();
}

public async Task<List<Borrowing>> GetAllActiveBorrowingsAsync()
{
    return await _context.Borrowings
        .Include(b => b.Member)
        .Include(b => b.Book)
        .Include(b => b.BookCopy)
        .Where(b => b.Status == BookBorrowStatus.Borrowed && b.IsActive)
        .OrderBy(b => b.DueDate)
        .ToListAsync();
}

public async Task<List<Borrowing>> GetOverdueBorrowingsAsync()
{
    return await _context.Borrowings
        .Include(b => b.Member)
        .Include(b => b.Book)
        .Include(b => b.BookCopy)
        .Where(b => b.Status == BookBorrowStatus.Borrowed && 
                   b.DueDate < DateTime.UtcNow &&
                   b.IsActive)
        .OrderBy(b => b.DueDate)
        .ToListAsync();
}

        public async Task<Borrowing> BorrowBookAsync(int memberId, int bookCopyId)
        {
            // Get member
            var member = await _memberRepository.GetById(memberId);
            if (member == null)
                throw new Exception("Member not found.");

            if (!member.IsActive)
                throw new Exception("Member account is inactive.");

            if (member.MembershipStatus != MembershipStatus.Active)
                throw new Exception("Membership is not active.");

            // Check unpaid fines
            var unpaidFines = await _borrowingRepository.GetUnpaidFineAmountByMemberAsync(memberId);
            if (unpaidFines > 500)
                throw new Exception($"You have unpaid fines of Rs.{unpaidFines}. Please clear fines first.");

            // Check borrowing limit
            var activeCount = await _borrowingRepository.GetActiveCountByMemberAsync(memberId);
            int maxAllowed = GetMaxBooksAllowed(member.MembershipType);
            
            if (activeCount >= maxAllowed)
                throw new Exception($"You can only borrow {maxAllowed} books at a time.");

            // Find book copy by its unique ID (string)
            var bookCopy = await _context.BookCopies
                .Include(bc => bc.Book)
                .FirstOrDefaultAsync(bc => bc.BookCopyId == bookCopyId); // ✅ string == string

            if (bookCopy == null)
                throw new Exception("Book copy not found.");

            if (!bookCopy.IsAvailable || bookCopy.IsBorrowed)
                throw new Exception("Book copy is not available.");

            if (bookCopy.IsDamaged)
                throw new Exception("Book copy is damaged and cannot be borrowed.");

            // ✅ Use bookCopy.BookId (int) - this is correct
            var alreadyBorrowed = await _borrowingRepository.HasMemberBorrowedBookAsync(memberId, bookCopy.BookId);
            if (alreadyBorrowed)
                throw new Exception("You have already borrowed this book and not returned it yet.");

            // Calculate due date
            int dueDays = GetBorrowDaysAllowed(member.MembershipType);
            DateTime dueDate = DateTime.UtcNow.AddDays(dueDays);

            // Create borrowing record
            var borrowing = new Borrowing
            {
                MemberId = memberId,
                BookCopyId = bookCopy.Id,
                BookId = bookCopy.BookId,
                BorrowedDate = DateTime.UtcNow,
                DueDate = dueDate,
                Status = BookBorrowStatus.Borrowed,
                IsActive = true,
                FineAmount = 0,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            // Update book copy status
            bookCopy.IsAvailable = false;
            bookCopy.IsBorrowed = true;
            bookCopy.UpdatedAt = DateTime.UtcNow;

            // Update member's borrowed count
            member.CurrentBorrowedCount++;
            member.UpdatedAt = DateTime.UtcNow;

            // Save all changes
            _context.Borrowings.Add(borrowing);
            await _context.SaveChangesAsync();

            return borrowing;
        }

        public async Task<Borrowing> ReturnBookAsync(int borrowingId)
        {
            var borrowing = await _context.Borrowings
                .Include(b => b.Member)
                .Include(b => b.BookCopy)
                .Include(b => b.Book)
                .FirstOrDefaultAsync(b => b.Id == borrowingId);

            if (borrowing == null)
                throw new Exception("Borrowing record not found.");

            if (borrowing.Status == BookBorrowStatus.Returned)
                throw new Exception("Book has already been returned.");

            DateTime returnDate = DateTime.UtcNow;
            decimal fineAmount = 0;
            int daysOverdue = 0;

            if (returnDate > borrowing.DueDate)
            {
                daysOverdue = (returnDate - borrowing.DueDate).Days;
                fineAmount = daysOverdue * 10;
                borrowing.Status = BookBorrowStatus.Overdue;
            }
            else
            {
                borrowing.Status = BookBorrowStatus.Returned;
            }

            borrowing.MemberReturnedDate = returnDate;
            borrowing.FineAmount = fineAmount;
            borrowing.UpdatedAt = DateTime.UtcNow;

            if (borrowing.BookCopy != null)
            {
                borrowing.BookCopy.IsAvailable = true;
                borrowing.BookCopy.IsBorrowed = false;
                borrowing.BookCopy.UpdatedAt = DateTime.UtcNow;
            }

            if (borrowing.Member != null && borrowing.Member.CurrentBorrowedCount > 0)
            {
                borrowing.Member.CurrentBorrowedCount--;
                borrowing.Member.UpdatedAt = DateTime.UtcNow;
            }

            if (fineAmount > 0)
            {
                var fine = new Fine
                {
                    BorrowingId = borrowing.Id,
                    MemberId = borrowing.MemberId,
                    FineAmount = fineAmount,
                    FineReason = $"Book returned {daysOverdue} days late. Fine: ₹10 per day",
                    PaymentStatus = FinePaymentStatus.Pending,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                _context.Fines.Add(fine);
            }

            await _context.SaveChangesAsync();
            return borrowing;
        }

        public async Task<List<Borrowing>> GetMemberBorrowingsAsync(int memberId)
        {
            return await _borrowingRepository.GetByMemberIdAsync(memberId);
        }

        public async Task<List<Borrowing>> GetActiveBorrowingsAsync(int memberId)
        {
            return await _borrowingRepository.GetActiveByMemberIdAsync(memberId);
        }

        public async Task<(bool CanBorrow, string Message, int CurrentCount, int MaxAllowed, decimal UnpaidFines)> 
            CheckBorrowingEligibilityAsync(int memberId)
        {
            var member = await _memberRepository.GetById(memberId);
            if (member == null)
                return (false, "Member not found.", 0, 0, 0);

            var currentCount = await _borrowingRepository.GetActiveCountByMemberAsync(memberId);
            var maxAllowed = GetMaxBooksAllowed(member.MembershipType);
            var unpaidFines = await _borrowingRepository.GetUnpaidFineAmountByMemberAsync(memberId);

            if (unpaidFines > 500)
                return (false, $"You have unpaid fines of Rs.{unpaidFines}. Maximum allowed is Rs.500.", currentCount, maxAllowed, unpaidFines);
            
            if (currentCount >= maxAllowed)
                return (false, $"You have reached the maximum borrowing limit of {maxAllowed} books.", currentCount, maxAllowed, unpaidFines);

            return (true, "You can borrow books.", currentCount, maxAllowed, unpaidFines);
        }

        public async Task<(bool IsValid, string ErrorMessage, int CurrentBorrowings, int AllowedBorrowings, decimal UnpaidFines)> 
            ValidateBorrowingAsync(int memberId, int bookCopyId)
        {
            var member = await _memberRepository.GetById(memberId);
            if (member == null)
                return (false, "Member not found.", 0, 0, 0);

            if (!member.IsActive)
                return (false, "Member account is inactive.", 0, 0, 0);

            if (member.MembershipStatus != MembershipStatus.Active)
                return (false, "Membership is not active.", 0, 0, 0);

            var unpaidFines = await _borrowingRepository.GetUnpaidFineAmountByMemberAsync(memberId);
            if (unpaidFines > 500)
                return (false, $"You have unpaid fines of Rs.{unpaidFines}. Maximum allowed is Rs.500.", 0, 0, unpaidFines);

            var currentCount = await _borrowingRepository.GetActiveCountByMemberAsync(memberId);
            var maxAllowed = GetMaxBooksAllowed(member.MembershipType);

            if (currentCount >= maxAllowed)
                return (false, $"You can only borrow {maxAllowed} books at a time.", currentCount, maxAllowed, unpaidFines);

            // ✅ Find book copy - string comparison is correct here
            var bookCopy = await _context.BookCopies
                .FirstOrDefaultAsync(bc => bc.BookCopyId == bookCopyId);

            if (bookCopy == null)
                return (false, "Book copy not found.", currentCount, maxAllowed, unpaidFines);

            if (!bookCopy.IsAvailable || bookCopy.IsBorrowed)
                return (false, "Book copy is not available.", currentCount, maxAllowed, unpaidFines);

            if (bookCopy.IsDamaged)
                return (false, "Book copy is damaged.", currentCount, maxAllowed, unpaidFines);

            // ✅ Use bookCopy.BookId (int) - this is correct
            var alreadyBorrowed = await _borrowingRepository.HasMemberBorrowedBookAsync(memberId, bookCopy.BookId);
            if (alreadyBorrowed)
                return (false, "You have already borrowed this book.", currentCount, maxAllowed, unpaidFines);

            return (true, "Valid to borrow.", currentCount, maxAllowed, unpaidFines);
        }

        public async Task<decimal> GetUnpaidFineAmountAsync(int memberId)
        {
            return await _borrowingRepository.GetUnpaidFineAmountByMemberAsync(memberId);
        }

        private int GetMaxBooksAllowed(MembershipType membershipType)
        {
            return membershipType switch
            {
                MembershipType.Basic => 3,
                MembershipType.Premium => 10,
                MembershipType.Student => 5,
                _ => 3
            };
        }

        private int GetBorrowDaysAllowed(MembershipType membershipType)
        {
            return membershipType switch
            {
                MembershipType.Basic => 14,
                MembershipType.Premium => 30,
                MembershipType.Student => 21,
                _ => 14
            };
        }
    }
}