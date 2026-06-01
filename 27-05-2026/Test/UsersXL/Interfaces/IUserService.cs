using UsersXL.DTOs;

namespace UsersXL.Interfaces
{
    public interface IUserService
    {
        Task<UserDto> CreateUserAsync(CreateUserDto createUserDto);
        Task<byte[]> GetAllUsersAsExcelAsync();
    }
}