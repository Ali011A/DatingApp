using DatingApp.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DatingApp.Data
{
    public class DatingDbContext:IdentityDbContext<AppUser,AppRole,int,
        IdentityUserClaim<int>,AppUserRole,IdentityUserLogin<int>,IdentityRoleClaim<int>,IdentityUserToken<int>>
    {
        public DatingDbContext(DbContextOptions<DatingDbContext> options) : base(options)
        {
        }
       // public DbSet<AppUser> Users { get; set; }
        public DbSet<UserLike> Likes  { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<Photo> Photos { get; set; }
        public DbSet<Group> Groups { get; set; }
        public DbSet<Connection> Connections { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AppUser>()
                .HasKey(u => u.Id);
            modelBuilder.Entity<AppUser>()
                .Property(u => u.UserName)
                .IsRequired()
                .HasMaxLength(50);
           
            modelBuilder.Entity<AppUserRole>(entity =>
            {
                entity.HasKey(ur => new { ur.UserId, ur.RoleId });

                entity.HasOne(ur => ur.User)
                      .WithMany(u => u.UserRoles)
                      .HasForeignKey(ur => ur.UserId)
                      .IsRequired();

                entity.HasOne(ur => ur.Role)
                      .WithMany(r => r.UserRoles)
                      .HasForeignKey(ur => ur.RoleId)
                      .IsRequired();
            });


            modelBuilder.Entity<AppUser>()
                .HasKey(u => u.Id);

            modelBuilder.Entity<IdentityUserLogin<int>>()
       .HasKey(l => l.UserId);

            modelBuilder.Entity<IdentityUserToken<int>>()
                .HasKey(t => new { t.UserId, t.LoginProvider, t.Name });

            modelBuilder.Entity<IdentityRoleClaim<int>>()
                .HasKey(rc => rc.Id);

            modelBuilder.Entity<IdentityUserClaim<int>>()
                .HasKey(uc => uc.Id);

            modelBuilder.Entity<UserLike>()
                .HasKey(k => new { k.SourceUserId, k.TargetUserId });
            modelBuilder.Entity<UserLike>()
                .HasOne(s => s.SourceUser)
                .WithMany(l => l.LikedUsers)
                .HasForeignKey(s => s.SourceUserId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<UserLike>()
                .HasOne(s => s.TargetUser)
                .WithMany(l => l.LikedByUsers)
                .HasForeignKey(s => s.TargetUserId)
                .OnDelete(DeleteBehavior.NoAction);
            modelBuilder.Entity<Message>()
                .HasOne(x=>x.Recipient)
                .WithMany(x=>x.MessagesReceived)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Message>()
                .HasOne(x => x.Sender)
                .WithMany(x => x.MessagesSent)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Photo>().HasQueryFilter(p => p.IsApproved);
            
        }

    }
}
