namespace ASI.Basecode.Services.ServiceModels
{
    public class ProfileViewModel
    {
        public ChangePasswordViewModel UpdatePassword { get; set; }
        public bool IsPasswordChangeRequired { get; set; } = false;
        public ChangeEmailOrUsernameViewModel UpdateEmailOrUserName { get; set; }
        public string UserCode { get; set; }
        public string Email { get; set; }
        public string ProfilePicture { get; set; }
    }
}
