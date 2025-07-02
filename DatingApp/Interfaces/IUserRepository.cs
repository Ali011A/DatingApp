using DatingApp.Dtos;
using DatingApp.Entities;
using DatingApp.Helpers;

namespace DatingApp.Interfaces
{
    public interface IUserRepository
    {
        void Update(AppUser user);

       // Task<bool> SaveAllAsync();
        Task<IEnumerable<AppUser>> GetUsersAsync();
        Task<AppUser?> GetUserByIdAsync(int id);
        Task<AppUser?> GetUserByUsernameAsync(string username);
        // This interface defines methods for user-related operations
        Task<PagedList<MemberDto>> GetMembersAsync(UserParams userParams);
        Task<MemberDto?> GetMemberAsync(string username, bool isCurrentUser);
        Task<AppUser?> GetUserByPhotoId(int photoId);
        //Task<MemberDto?> GetMemberByIdAsync(string username,bool isCurrentUser);

    }
}
