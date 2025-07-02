using DatingApp.Dtos;
using DatingApp.Entities;
using DatingApp.Extensions;
using DatingApp.Helpers;
using DatingApp.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DatingApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LikesController : ControllerBase
    {
        //Dependency injection
        private readonly IUnitOfWork _unitOfWork;

        public LikesController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        [HttpPost("{targetUserId:int}")] //api/likes/3
        public async Task<IActionResult> ToggleLike(int targetUserId)
        {
            var sourceUserId = User.GetUserId();
            if(sourceUserId == targetUserId) return BadRequest("You cannot like yourself.");
            var existingLike = await _unitOfWork.LikesRepository.GetUserLike(sourceUserId, targetUserId);
            if(existingLike == null)
            {
                var like = new UserLike
                {
                    SourceUserId = sourceUserId,
                    TargetUserId = targetUserId
                };
                _unitOfWork.LikesRepository.AddUserLike(like);
               
            }
            else
            {
                _unitOfWork.LikesRepository.RemoveUserLike(existingLike);
            }
            if(await _unitOfWork.Complete()) return Ok();
            return BadRequest("Failed to like user.");
        }
        [HttpGet("list")]
        public async Task<ActionResult<IEnumerable<int>>> GetCurrentUserLikesIds()
        {
            var userId = User.GetUserId();
            var likes = await _unitOfWork.LikesRepository.GetCurrentUserLikeIds(userId);
            return Ok(likes);
        }
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MemberDto>>> GetUserLikes([FromQuery]LikesParams likesParams)
        {
             likesParams.UserId = User.GetUserId();
            var users = await _unitOfWork.LikesRepository.GetUserLikes(likesParams);
            Response.AddPaginationHeader(users);
            return Ok(users);
        }

    }
}
