using Microsoft.AspNetCore.Identity;
using System.Collections;

namespace DatingApp.Entities
{
    public class AppRole:IdentityRole<int>
    {
        public ICollection<AppUserRole> UserRoles { get; set; } = []; 

    }
}
