﻿using CloudinaryDotNet.Actions;
using DatingApp.Dtos;
using DatingApp.Entities;

namespace DatingApp.Interfaces
{
    public interface IPhotoService
    {
        Task<ImageUploadResult> AddPhotoAsync(IFormFile file);
        Task<DeletionResult> DeletePhotoAsync(string publicId);
       
    }
}
