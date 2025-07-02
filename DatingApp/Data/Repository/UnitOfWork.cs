using DatingApp.Interfaces;

namespace DatingApp.Data.Repository
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly DatingDbContext _context;
        private readonly IUserRepository _userRepository;
        private readonly ILikesRepository _likesRepository;
        private readonly IMessageRepository _messageRepository;
        private readonly IPhotoRepository _photoRepository;

        public UnitOfWork( DatingDbContext context, IUserRepository userRepository,
            ILikesRepository likesRepository, IMessageRepository messageRepository, IPhotoRepository photoRepository)
        {
            _context = context;
            _userRepository = userRepository;
            _likesRepository = likesRepository;
            _messageRepository = messageRepository;
            _photoRepository = photoRepository;
        }

        public IUserRepository UserRepository => _userRepository;

        public ILikesRepository LikesRepository => _likesRepository;

        public IMessageRepository MessageRepository => _messageRepository;

        public IPhotoRepository PhotoRepository=> _photoRepository;

        public async Task<bool> Complete()
        {
            
            return await _context.SaveChangesAsync()>0;
        }

        public bool HasChanges()
        {
            return _context.ChangeTracker.HasChanges();
        }
    }
}
