using AutoMapper;
using AutoMapper.QueryableExtensions;
using DatingApp.Dtos;
using DatingApp.Entities;
using DatingApp.Helpers;
using DatingApp.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DatingApp.Data.Repository
{
    public class LikesRepository : ILikesRepository
    {
        //DEPENDENCY INJECTION
        private readonly DatingDbContext _context;
        private readonly IMapper  _mapper;
        public LikesRepository(DatingDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public void AddUserLike(UserLike userLike)
        {
            
            _context.Likes.Add(userLike);
        }
        public void RemoveUserLike(UserLike userLike)
        {
            _context.Likes.Remove(userLike);
        }
        public async Task<IEnumerable<int>> GetCurrentUserLikeIds(int currentUserId)
        {
          
            var likes = _context.Likes
                .Where(like => like.SourceUserId == currentUserId)
                .Select(like => like.TargetUserId).ToListAsync();
            return await likes;
        }

        public async Task<UserLike?> GetUserLike(int sourceUserId, int likedUserId)
        {
            return await _context.Likes.FindAsync(sourceUserId, likedUserId);
           
        }

        public async Task<PagedList<MemberDto>> GetUserLikes(LikesParams likesParams)
         {
            var likes = _context.Likes.AsQueryable();
            IQueryable<MemberDto> query; 
            switch (likesParams.Predicate)
            {
                case "liked":
                    query= likes
                        .Where(like => like.SourceUserId == likesParams.UserId)
                        .Select(like => like.TargetUser)
                        .ProjectTo<MemberDto>(_mapper.ConfigurationProvider);
                    break;
                    case "likedBy":
                    query= likes
                        .Where(like => like.TargetUserId == likesParams.UserId)
                        .Select(like => like.SourceUser)
                        .ProjectTo<MemberDto>(_mapper.ConfigurationProvider);
                    break;
                    default:
                    var likeIds = await GetCurrentUserLikeIds(likesParams.UserId);
                  query = likes
                        .Where(x => x.TargetUserId == likesParams.UserId && likeIds.Contains(x.SourceUserId))
                        .Select(x => x.SourceUser)
                        .ProjectTo<MemberDto>(_mapper.ConfigurationProvider);
                    break;
            }

            return await PagedList<MemberDto>.CreateAsync(query, likesParams.PageNumber, likesParams.PageSize);
            
           
        }

      

       
    }
}
