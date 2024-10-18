using System.ComponentModel.DataAnnotations;

namespace ASI.Basecode.Services.ServiceModels
{
    public class SigninViewModel
    {
        [Required(ErrorMessage = "Username is required.")]
        public string UserCode { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        public string Password { get; set; }
    }
}