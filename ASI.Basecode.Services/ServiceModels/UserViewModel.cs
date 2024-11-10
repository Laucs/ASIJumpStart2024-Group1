using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace ASI.Basecode.Services.ServiceModels
{
    public class UserViewModel
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        [Required(ErrorMessage = "This is required")]
        public string UserCode { get; set; }

        [Required(ErrorMessage = "This is required")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "This is required")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "This is required")]
        public string Password { get; set; }

        [Required(ErrorMessage = "This is required")]
        public string Mail { get; set; }

        public string EmailVerificationToken { get; set; }
        public DateTime? VerificationTokenExpiration { get; set; }
        public bool IsEmailVerified { get; set; }
        public string PasswordResetToken { get; set; }
        public DateTime? ResetTokenExpiration { get; set; }
        public string ProfilePic { get; set; }
    }

    public class UserListViewModel
    {
        [DisplayName("ID")]
        public string IdFilter { get; set; }

        [Display(Name = "FirstName")]
        public string FirstNameFilter { get; set; }

        public IEnumerable<UserViewModel> dataList { get; set; }
    }
}
