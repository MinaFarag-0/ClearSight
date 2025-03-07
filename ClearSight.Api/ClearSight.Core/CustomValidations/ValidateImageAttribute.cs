using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace ClearSight.Core.CustomValidations
{
    public class ValidateImageAttribute : ValidationAttribute
    {
        private readonly string[] _allowedExtensions = { ".jpg", ".jpeg", ".png" };
        private readonly long _maxFileSize; 

        public ValidateImageAttribute(long maxFileSizeMB)
        {
            _maxFileSize = maxFileSizeMB * 1024 * 1024; // Convert MB to bytes
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var file = value as IFormFile;
            if (file == null || file.Length == 0)
            {
                return ValidationResult.Success;
            }

            if (file.Length > _maxFileSize)
            {
                return new ValidationResult($"File size must be less than {_maxFileSize / (1024 * 1024)} MB.");
            }

            var splitFileName = file.FileName.Split('.');
            var fileExtension = Path.GetExtension(file.FileName).ToLower();
            if (splitFileName.Length > 2 || !_allowedExtensions.Contains(fileExtension))
            {
                return new ValidationResult("Invalid file type. Only JPG , JPEG and PNG are allowed.");
            }

            return ValidationResult.Success;
        }
    }
}