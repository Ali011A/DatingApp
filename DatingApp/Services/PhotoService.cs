using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using DatingApp.Helpers;
using DatingApp.Interfaces;
using Microsoft.Extensions.Options;

namespace DatingApp.Services
{
    public class PhotoService : IPhotoService
    {
        private readonly Cloudinary _cloudinary;
        public PhotoService(IOptions<CloudinarySettings> config)
        {
            var cloudinaryConfig = new Account(
                config.Value.CloudName, config.Value.ApiKey, config.Value.ApiSecret);
               _cloudinary = new Cloudinary(cloudinaryConfig);
            // Initialize the Cloudinary client with the provided configuration
        }
        public async Task<ImageUploadResult> AddPhotoAsync(IFormFile file)
        {
          
            var uploadResult = new ImageUploadResult();
            if (file.Length > 0)
            {
                using var stream = file.OpenReadStream();
                var uploadParams = new ImageUploadParams
                {
                    File = new FileDescription(file.FileName, stream),
                    Transformation = new Transformation().Width(500).Height(500).Crop("fill").Gravity("face"),
                    Folder = "datingapp-photos"

                };
            uploadResult = await _cloudinary.UploadAsync(uploadParams);
            }
            return uploadResult;

        }

        public async Task<DeletionResult> DeletePhotoAsync(string publicId)
        {
            var deleteParams = new DeletionParams(publicId)
            {
                ResourceType = ResourceType.Image
            };
            return await _cloudinary.DestroyAsync(deleteParams);


        }
    }
}
