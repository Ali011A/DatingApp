using AutoMapper;
using AutoMapper.QueryableExtensions;
using DatingApp.Dtos;
using DatingApp.Entities;
using DatingApp.Helpers;
using DatingApp.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DatingApp.Data.Repository
{
    public class UserRepository : IUserRepository
    {
        //Injection
        private readonly DatingDbContext _context;
        private readonly IMapper _mapper;
        public UserRepository(DatingDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<MemberDto?> GetMemberAsync(string username, bool isCurrentUser)
        {
            var query=_context.Users
                .Where(x => x.UserName == username)
                .ProjectTo<MemberDto>(_mapper.ConfigurationProvider)
                .AsQueryable();
            if (isCurrentUser) query = query.IgnoreQueryFilters();
            return await query.FirstOrDefaultAsync();
        }

     

        public async Task<PagedList<MemberDto>> GetMembersAsync(UserParams userParams)
        {

            var query = _context.Users.AsQueryable();

            query = query.Where(u => u.UserName != userParams.CurrentUserName);
            if (!string.IsNullOrEmpty(userParams.Gender))
            {
                query = query.Where(u => u.Gender == userParams.Gender);
            }
            var minDob = DateTime.Today.AddYears(-userParams.MaxAge - 1);
            var maxDob = DateTime.Today.AddYears(-userParams.MinAge);

            query = query.Where(u => u.DateOfBirth >= minDob && u.DateOfBirth <= maxDob);

            query = userParams.OrderBy switch
            {
                "created" => query.OrderByDescending(u => u.Created),
                _ => query.OrderByDescending(u => u.LastActive)
            };
        return await PagedList<MemberDto>.CreateAsync(query.ProjectTo<MemberDto>(_mapper.ConfigurationProvider)
            , userParams.PageNumber, userParams.PageSize);
        }

        public async Task<AppUser?> GetUserByIdAsync(int id)
        {
            
            //vaild
            return await _context.Users.FindAsync(id);

        }

        public async Task<AppUser?> GetUserByPhotoId(int photoId)
        {
            return await _context.Users
 .Include(p => p.Photos)
 .IgnoreQueryFilters()
 .Where(p => p.Photos.Any(p => p.Id == photoId))
 .FirstOrDefaultAsync();
        }

        public async Task<AppUser?> GetUserByUsernameAsync(string username)
        {
           
            return await _context.Users
                .Include(u => u.Photos).FirstOrDefaultAsync(x => x.UserName == username);
        }

        public async Task<IEnumerable<AppUser>> GetUsersAsync()
        {
          
            return await _context.Users
                .Include(u => u.Photos).ToListAsync();
        }

      

        public void Update(AppUser user)
        {

            //   This method is used to update an existing user
            if (user == null) throw new ArgumentNullException(nameof(user));

            // Set the state of the entity to Modified
            _context.Entry(user).State = EntityState.Modified;
            
        }
    }
}
