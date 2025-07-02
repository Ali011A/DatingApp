using DatingApp.Dtos;
using DatingApp.Entities;
using DatingApp.Helpers;

namespace DatingApp.Interfaces
{
    public interface ILikesRepository
    {
        Task<UserLike?> GetUserLike(int sourceUserId, int likedUserId);
        Task<PagedList<MemberDto>> GetUserLikes(LikesParams likesParams);
        Task<IEnumerable<int>> GetCurrentUserLikeIds(int currentUserId);
        void AddUserLike(UserLike userLike);
        void RemoveUserLike(UserLike userLike);
        //Task<bool> SaveChanges();
        //Task<AppUser> GetUserWithLikes(int userId);
    }
}
