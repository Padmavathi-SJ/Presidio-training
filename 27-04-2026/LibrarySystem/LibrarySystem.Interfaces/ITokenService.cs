using LibrarySystem.Models.DTOs;

namespace LibrarySystem.Interfaces
{
    public interface ITokenService
    {
        public class CreateNewToken(TokenRequest request);
    }
}