using LibrarySystem.DataAccess.Repositories;
using LibrarySystem.Business.Exceptions;

namespace LibrarySystem.Business.Services
{
    public class AdminService : IAdminService
    {
        private readonly IAdminRepository _adminRepository;
        
        public AdminService(IAdminRepository adminRepository)
        {
            _adminRepository = adminRepository;
        }

        public async Task<bool> LoginAsync(string phoneNum, string password)
        {
            // validation
            if (string.IsNullOrWhiteSpace(phoneNum))
            {
                throw new ValidationException("Phone number is required.");
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                throw new ValidationException("Password is required.");
            }

            // get admin
            var admin = await _adminRepository.GetByPhoneNumAsync(phoneNum);
            if (admin == null)
            {
                throw new NotFoundException("Admin", phoneNum);
            }

            // verify password
            if (admin.Password != password)
            {
                throw new UnauthorizedException("Invalid password.");
            }

            // check active
            if (!admin.IsActive)
            {
                throw new BusinessRuleException("INACTIVE_ADMIN", "Admin account is inactive.");
            }

            return true;
        }
    }
}