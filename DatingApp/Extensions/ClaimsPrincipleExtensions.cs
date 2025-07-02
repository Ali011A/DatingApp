using System.Security.Claims;

namespace DatingApp.Extensions
{
    public static class ClaimsPrincipleExtensions
    {
        public static string GetUsername(this ClaimsPrincipal user)
        {
            var username=user.FindFirstValue(ClaimTypes.Name);
            if (username == null)
            {
                throw new ArgumentNullException(nameof(username), "Username claim not found.");
            }
            return username;
           
        }
        public static int GetUserId(this ClaimsPrincipal user)
        {
            var userIdValue = user.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userIdValue))
            {
                throw new InvalidOperationException("NameIdentifier claim is missing");
            }

            if (!int.TryParse(userIdValue, out var userId))
            {
                throw new InvalidOperationException($"Invalid user ID format: {userIdValue}");
            }

            return userId;
        }


    }
}
