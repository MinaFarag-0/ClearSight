using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace ClearSight.Core.CustomValidations
{
    public class PasswordRequirementsAttribute : ValidationAttribute
    {
        private const string Pattern = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^a-zA-Z\d]).{6,}$";

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value is string password && Regex.IsMatch(password, Pattern))
            {
                return ValidationResult.Success;
            }

            return new ValidationResult("Password Must Be 1 UpperCase, 1 LowerCase, 1 Number, 1 Special Character And Length More Than Or Equals 6");
        }
    }
}