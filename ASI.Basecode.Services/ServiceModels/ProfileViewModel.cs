namespace ASI.Basecode.Services.ServiceModels
{
    public class ProfileViewModel
    {
        public ChangePasswordViewModel UpdatePassword { get; set; }
        public ChangeEmailViewModel UpdateEmail { get; set; }
        public ChangeUsernameViewModel UpdateUsername { get; set; }
        public bool IsPasswordChangeRequired { get; set; } = false;
        public string UserCode { get; set; }
        public string Email { get; set; }
        public string ProfilePicture { get; set; }
    }
}
