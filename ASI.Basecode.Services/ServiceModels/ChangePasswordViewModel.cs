using System.ComponentModel.DataAnnotations;

namespace ASI.Basecode.Services.ServiceModels
{
    public class ChangePasswordViewModel
    {
        [Required(ErrorMessage = "Old Password is required")]
        [StringLength(50, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters long.")]
        [DataType(DataType.Password)]
        public string OldPassword { get; set; }

        [Required(ErrorMessage = "New Password is required")]
        [DataType(DataType.Password)]
        [NotEqualTo("OldPassword", ErrorMessage = "The new password should be different from the old password.")]
        [StringLength(50, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters long.")]
        public string NewPassword { get; set; }

        [Required(ErrorMessage = "Confirm Password is required")]
        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

    }

    public class NotEqualToAttribute : ValidationAttribute
    {
        private readonly string _comparisonProperty;

        public NotEqualToAttribute(string comparisonProperty)
        {
            _comparisonProperty = comparisonProperty;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            // Get the property to compare against (e.g., OldPassword)
            var comparisonProperty = validationContext.ObjectType.GetProperty(_comparisonProperty);

            if (comparisonProperty == null)
            {
                return new ValidationResult($"Property '{_comparisonProperty}' not found.");
            }

            // Get the value of the property (e.g., OldPassword value)
            var comparisonValue = comparisonProperty.GetValue(validationContext.ObjectInstance);

            // Check if the values are equal
            if (value != null && value.Equals(comparisonValue))
            {
                return new ValidationResult(ErrorMessage ?? $"{validationContext.DisplayName} must be different from {_comparisonProperty}.");
            }

            return ValidationResult.Success;
        }
    }

}
