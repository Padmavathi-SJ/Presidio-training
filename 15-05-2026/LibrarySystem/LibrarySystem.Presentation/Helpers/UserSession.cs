namespace LibrarySystem.Presentation.Helpers
{
    public static class UserSession
    {
        public static int CurrentMemberId { get; private set; }

        public static string CurrentEmail { get; private set; } = "";

        public static bool IsLoggedIn => CurrentMemberId > 0;

        public static void Login(int memberId, string email)
        {
            CurrentMemberId = memberId;
            CurrentEmail = email;
        }

        public static void Logout()
        {
            CurrentMemberId = 0;
            CurrentEmail = "";
        }
    }
}