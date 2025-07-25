﻿using DatingApp.Extensions;
using DatingApp.Interfaces;
using Microsoft.AspNetCore.Mvc.Filters;

namespace DatingApp.Helpers
{
    public class LogUserActivity : IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
          var resultContext = await next();
            if (resultContext.HttpContext.User.Identity?.IsAuthenticated != true)
                return;

            var userId = resultContext.HttpContext.User.GetUserId();
            var repo = resultContext.HttpContext.RequestServices.GetRequiredService<IUnitOfWork>();
            var user = await repo.UserRepository.GetUserByIdAsync(userId);
            if(user != null) user.LastActive = DateTime.UtcNow;
            await repo.Complete();

        }
    }
}
