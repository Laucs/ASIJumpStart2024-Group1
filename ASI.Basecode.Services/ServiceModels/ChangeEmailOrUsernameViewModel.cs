using System.ComponentModel.DataAnnotations;

namespace ASI.Basecode.Services.ServiceModels
{
    public class ChangeEmailOrUsernameViewModel
    {
        [Required(ErrorMessage = "Email is required.")]
        [DataType(DataType.EmailAddress)]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        public string NewEmail { get; set; }
        [Required(ErrorMessage = "Username is required.")]
        public string NewUsername { get; set; }

    }
}
