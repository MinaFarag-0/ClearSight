using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace ClearSight.Core.CustomValidations
{
    public class PasswordRequirementsAttribute : ValidationAttribute
    {
        public int MinimumLength { get; set; } = 6;
        public bool RequireUppercase { get; set; } = true;
        public bool RequireLowercase { get; set; } = true;
        public bool RequireDigit { get; set; } = true;
        public bool RequireSpecialCharacter { get; set; } = true;

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var password = value as string;

            if (string.IsNullOrEmpty(password))
            {
                return new ValidationResult("Password is required.");
            }

            var errors = new List<string>();

            if (password.Length < MinimumLength)
                errors.Add($"at least {MinimumLength} characters");

            if (RequireUppercase && !Regex.IsMatch(password, "[A-Z]"))
                errors.Add("one uppercase letter");

            if (RequireLowercase && !Regex.IsMatch(password, "[a-z]"))
                errors.Add("one lowercase letter");

            if (RequireDigit && !Regex.IsMatch(password, @"\d"))
                errors.Add("one number");

            if (RequireSpecialCharacter && !Regex.IsMatch(password, @"[^a-zA-Z\d]"))
                errors.Add("one special character");

            if (errors.Count == 0)
                return ValidationResult.Success;

            var errorMessage = "Password must contain " + string.Join(", ", errors) + ".";
            return new ValidationResult(errorMessage);
        }
    }
}