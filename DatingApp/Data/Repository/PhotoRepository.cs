using DatingApp.Dtos;
using DatingApp.Entities;
using DatingApp.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DatingApp.Data.Repository
{
    public class PhotoRepository : IPhotoRepository
    {
        private readonly DatingDbContext _context;

        public PhotoRepository(DatingDbContext context)
        {
            _context = context;
        }
        public async Task<Photo?> GetPhoto(int id)
        {
           return await _context.Photos
                .IgnoreQueryFilters()
                .SingleOrDefaultAsync(x => x.Id == id);

        }

        public async Task<IEnumerable<PhotoForApprovalDto>> GetUnapprovedPhotos()
        {
            return await _context.Photos
                .IgnoreQueryFilters()
                .Where(p => p.IsApproved == false)
                .Select(p => new PhotoForApprovalDto
                {
                    Id = p.Id,
                    Url = p.Url,
                    Username = p.AppUser.UserName
                }).ToListAsync();
           
        }

        public void RemovePhoto(Photo photo)
        {
            _context.Photos.Remove(photo);
        }
    }
}
