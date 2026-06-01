using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Drawing;
using UsersXL.DTOs;
using UsersXL.Models;
using UsersXL.Repositories;
using UsersXL.Interfaces;

namespace UsersXL.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;

        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
            // No license configuration needed for EPPlus 7.5.2 - it works without it
        }

        public async Task<UserDto> CreateUserAsync(CreateUserDto createUserDto)
        {
            // Check if email already exists
            var emailExists = await _userRepository.EmailExistsAsync(createUserDto.Email);
            if (emailExists)
            {
                throw new InvalidOperationException($"User with email {createUserDto.Email} already exists");
            }

            var user = new User
            {
                Name = createUserDto.Name,
                Email = createUserDto.Email,
                PhoneNum = createUserDto.PhoneNum,
                Age = createUserDto.Age
            };

            var createdUser = await _userRepository.CreateAsync(user);
            return MapToDto(createdUser);
        }

        public async Task<byte[]> GetAllUsersAsExcelAsync()
        {
            var users = await _userRepository.GetAllAsync();
            
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Users");
                
                // Add headers
                worksheet.Cells[1, 1].Value = "ID";
                worksheet.Cells[1, 2].Value = "Name";
                worksheet.Cells[1, 3].Value = "Email";
                worksheet.Cells[1, 4].Value = "Phone Number";
                worksheet.Cells[1, 5].Value = "Age";
                
                // Style headers
                using (var range = worksheet.Cells[1, 1, 1, 5])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(Color.LightGray);
                    range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                }
                
                // Add data rows
                int row = 2;
                foreach (var user in users)
                {
                    worksheet.Cells[row, 1].Value = user.Id;
                    worksheet.Cells[row, 2].Value = user.Name;
                    worksheet.Cells[row, 3].Value = user.Email;
                    worksheet.Cells[row, 4].Value = user.PhoneNum;
                    worksheet.Cells[row, 5].Value = user.Age;
                    row++;
                }
                
                // Auto-fit columns
                worksheet.Cells[1, 1, row - 1, 5].AutoFitColumns();
                
                // Return as byte array
                return await package.GetAsByteArrayAsync();
            }
        }

        private UserDto MapToDto(User user)
        {
            return new UserDto
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                PhoneNum = user.PhoneNum,
                Age = user.Age
            };
        }
    }
}