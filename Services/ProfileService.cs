using VinfastWeb.ViewModels;

namespace VinfastWeb.Services
{
    public class ProfileService
    {
        private static UserProfileViewModel _profile =
            new UserProfileViewModel
            {
                UserId = 1,
                FullName = "Test User",
                Email = "test@email.com",
                Phone = "0123456789"
            };

        public UserProfileViewModel GetProfile()
        {
            return _profile;
        }

        public void SaveProfile(UserProfileViewModel profile)
        {
            _profile = profile;
        }

        public void Logout()
        {
        }
    }
}