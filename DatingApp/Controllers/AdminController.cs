using DatingApp.Data.Repository;
using DatingApp.Entities;
using DatingApp.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DatingApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController(UserManager<AppUser> _userManger,IUnitOfWork _unitOfWork,IPhotoService _photoService) : ControllerBase
    {
        //Dpendency injection
        
       
        [Authorize(Policy = "RequireAdminRole")]
        [HttpGet("users-with-roles")]
        public async Task<ActionResult> GetUsersWithRoles()
        {
            var users = await _userManger.Users
                .Include(r => r.UserRoles)
                .ThenInclude(r => r.Role)
                .OrderBy(u => u.UserName)
                .Select(u => new
                {
                    u.Id,
                    Username = u.UserName,
                    Roles = u.UserRoles.Select(r => r.Role.Name).ToList()
                })
                .ToListAsync();
            return Ok(users);

        }

        [Authorize(Policy = "RequireAdminRole")]
        [HttpPost("edit-roles/{username}")]
        public async Task<ActionResult> EditRoles(string username, [FromQuery] string roles)
        {
            var selectedRoles = roles.Split(',').ToArray();
            var user = await _userManger.FindByNameAsync(username);
            if (user == null) return NotFound("Could not find user");
            var userRoles = await _userManger.GetRolesAsync(user);
            var result = await _userManger.AddToRolesAsync(user, selectedRoles.Except(userRoles));
            if (!result.Succeeded) return BadRequest("Failed to add to roles");
            result = await _userManger.RemoveFromRolesAsync(user, userRoles.Except(selectedRoles));
            if (!result.Succeeded) return BadRequest("Failed to remove from roles");
            return Ok(await _userManger.GetRolesAsync(user));
        }


        [Authorize(Policy = "ModeratePhotoRole")]
        [HttpGet("photos-to-moderate")]
        public async Task<ActionResult> GetPhotosForModeration()
        {
            var  photos=await _unitOfWork.PhotoRepository.GetUnapprovedPhotos();
            return Ok(photos);
        }
        [Authorize(Policy = "ModeratePhotoRole")]
        [HttpPost("approve-photo/{photoId:int}")]
        public async Task<ActionResult> ApprovePhoto(int photoId)
        {
           var photo = await _unitOfWork.PhotoRepository.GetPhoto(photoId);
            if (photo == null) return BadRequest("Could not get photo from db");
            photo.IsApproved = true;
            var user = await _unitOfWork.UserRepository.GetUserByPhotoId(photoId);
            if (user == null) return BadRequest("Could not get user from db");
            if (!user.Photos.Any(x => x.IsMain)) photo.IsMain = true;
            await _unitOfWork.Complete();
            return Ok();
        }

        [Authorize(Policy = "ModeratePhotoRole")]
        [HttpPost("reject-photo/{photoId}")]
        public async Task<ActionResult> RejectPhoto(int photoId)
        {
            var photo=await _unitOfWork.PhotoRepository.GetPhoto(photoId);
            if (photo == null) return BadRequest("Could not find photo");
            if(photo.PublicId != null)
            {
                var result = await _photoService.DeletePhotoAsync(photo.PublicId);
               if(result.Result == "ok")
                {
                    _unitOfWork.PhotoRepository.RemovePhoto(photo);
                }


            }
            else
            {
                _unitOfWork.PhotoRepository.RemovePhoto(photo);
            }
            await _unitOfWork.Complete();
            return Ok();

        }

    }
}
