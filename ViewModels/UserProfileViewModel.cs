namespace VinfastWeb.ViewModels
{
    public class UserProfileViewModel
    {
        public int UserId { get; set; }

        public string AvatarUri { get; set; } = "";
        public string FullName { get; set; } = "";
        public string Email { get; set; } = "";
        public string Phone { get; set; } = "";
        public string Birthday { get; set; } = "";
        public string Gender { get; set; } = "";
        public string Address { get; set; } = "";
    }
}