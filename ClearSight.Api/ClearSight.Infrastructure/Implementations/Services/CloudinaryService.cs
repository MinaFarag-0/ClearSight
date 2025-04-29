using ClearSight.Core.Enums;
using ClearSight.Core.Helpers;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace ClearSight.Infrastructure.Implementations.Services
{
    public class CloudinaryService
    {
        private readonly Cloudinary _cloudinary;
        private readonly CloudinarySettings _cloudinarySettings;

        public CloudinaryService(IOptions<CloudinarySettings> cloudinarySettings)
        {
            _cloudinarySettings = cloudinarySettings.Value;
            Account account = new()
            {
                Cloud = _cloudinarySettings.CloudName,
                ApiKey = _cloudinarySettings.ApiKey,
                ApiSecret = _cloudinarySettings.ApiSecret,
            };
            _cloudinary = new Cloudinary(account);
        }

        public async Task<string> UploadImageAsync(IFormFile file, CloudFolder folder)
        {
            using var stream = file.OpenReadStream();
            var uploadParams = new ImageUploadParams()
            {
                AllowedFormats = ["jpg", "png", "jpeg"],
                Folder = folder.ToString(),
                File = new FileDescription(Guid.NewGuid().ToString(), stream),
            };
            var uploadResult = await _cloudinary.UploadAsync(uploadParams);
            return uploadResult.Url.ToString();
        }
    }
}
