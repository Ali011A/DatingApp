using DatingApp.Dtos;
using DatingApp.Entities;

namespace DatingApp.Interfaces
{
    public interface IPhotoRepository
    {
        Task<IEnumerable<PhotoForApprovalDto>> GetUnapprovedPhotos();
        Task<Photo?> GetPhoto(int id);
        void RemovePhoto(Photo photo);
    }
}
