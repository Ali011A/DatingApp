using AutoMapper;
using DatingApp.Data;
using DatingApp.Data.Repository;
using DatingApp.Dtos;
using DatingApp.Entities;
using DatingApp.Extensions;
using DatingApp.Helpers;
using DatingApp.Interfaces;
using DatingApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace DatingApp.Controllers
{
    [ServiceFilter(typeof(LogUserActivity))]
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        private readonly IMapper _mapper;
        //Injection
        private readonly IPhotoService _photoService;

        public UsersController(IUnitOfWork unitOfWork, IMapper mapper, IPhotoService photoService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _photoService = photoService;

        }
       // [Authorize(Roles = "Admin")]
        [HttpGet] //api/users/getusers
        public async Task<ActionResult<IEnumerable<MemberDto>>> GetUsers([FromQuery]UserParams userParams)
        {
            // This is a placeholder for actual user retrieval logic
            var users = await _unitOfWork.UserRepository.GetMembersAsync( userParams);
            Response.AddPaginationHeader(users);
            //var usersToReturn = _mapper.Map<IEnumerable<MemberDto>>(users);
            return Ok(users);
        }
       // [Authorize(Roles = "Member")]
        [HttpGet("{id:int}")]
        public async Task<ActionResult<MemberDto>> GetUser(int id)
        {
            // This is a placeholder for actual user retrieval logic
            var user = await _unitOfWork.UserRepository.GetUserByIdAsync(id);
            if (user == null)
            {
                return NotFound($"User with ID {id} not found.");
            }
            return _mapper.Map<MemberDto>(user);
        }
      //  [Authorize(Roles = "Member")]
        [HttpGet("{username:alpha}")]
        public async Task<ActionResult<MemberDto>> GetUserByUsername(string username)
        {
            // This is a placeholder for actual user retrieval logic
            var currentUsername = User.GetUsername();
            var user = await _unitOfWork.UserRepository.GetMemberAsync(username,
                isCurrentUser: currentUsername == username);
            if (user == null)
            {
                return NotFound($"User with username {username} not found.");
            }
            return user;

        }

        [HttpPut]
        public async Task<ActionResult> UpdateUser(MemberUpdateDto memberUpdateDto)
        {

            var username = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (username == null)
            {
                return Unauthorized("User not authenticated.");
            }
            var user = await _unitOfWork.UserRepository.GetUserByUsernameAsync(username);
            if (user == null)
            {
                return NotFound($"User with username {username} not found.");
            }
            _mapper.Map(memberUpdateDto, user);
            if (await _unitOfWork.Complete()) return NoContent();
            return BadRequest("Failed to update user.");
        }
        [HttpPost("add-photo")]
        public async Task<ActionResult<PhotoDto>> AddPhoto(IFormFile file)
        {
            var username = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (username == null)
            {
                return Unauthorized("User not authenticated.");
            }
            var user = await _unitOfWork.UserRepository.GetUserByUsernameAsync(User.GetUsername());
            if (user == null)
            {
                return NotFound($"User with username {username} not found.");
            }
            var result = await _photoService.AddPhotoAsync(file);
            if (result.Error != null)
            {
                return BadRequest(result.Error.Message);
            }
            var photo = new Photo
            {
                Url = result.SecureUrl.AbsoluteUri,
                PublicId = result.PublicId
            };
            //if (user.Photos.Count == 0)
            //{
            //    photo.IsMain = true;
            //}
            user.Photos.Add(photo);
            if (await _unitOfWork.Complete())
            {
                return CreatedAtAction(nameof(GetUserByUsername), new { username = user.UserName }, _mapper.Map<PhotoDto>(photo));
            }
            return BadRequest("Problem adding photo");
        }


    [HttpPut("set-main-photo/{photoId:int}")]
        public async Task<ActionResult> SetMainPhoto(int photoId)
        {
            var user = await _unitOfWork.UserRepository.GetUserByUsernameAsync(User.GetUsername());
            if (user == null)
            {
                return NotFound($"User with username {User.GetUsername()} not found.");
            }
            var photo = user.Photos.FirstOrDefault(x => x.Id == photoId);
            if (photo == null)
            {
                return NotFound($"Photo with ID {photoId} not found.");
            }
            if (photo.IsMain)
            {
                return BadRequest("This is already your main photo.");
            }
            var currentMain = user.Photos.FirstOrDefault(x => x.IsMain);
            if (currentMain != null)
            {
                currentMain.IsMain = false;
            }
            photo.IsMain = true;
            if (await _unitOfWork.Complete())
            {
                return NoContent();
            }
            return BadRequest("Failed to set main photo.");
        }
        [HttpDelete("delete-photo/{photoId:int}")]
        public async Task<ActionResult> DeletePhoto(int photoId)
        {
            var user = await
                     _unitOfWork.UserRepository.GetUserByUsernameAsync(User.GetUsername());
            if (user == null) return BadRequest("User not found");
            var photo = await _unitOfWork.PhotoRepository.GetPhoto(photoId);
            if (photo == null || photo.IsMain) return BadRequest("This photo cannot be deleted");
            if (photo.PublicId != null)
            {
                var result = await _photoService.DeletePhotoAsync(photo.PublicId);
                if (result.Error != null) return BadRequest(result.Error.Message);
            }
            user.Photos.Remove(photo);
            if (await _unitOfWork.Complete()) return Ok();
            return BadRequest("Problem deleting photo");
        }
        
    }
}
