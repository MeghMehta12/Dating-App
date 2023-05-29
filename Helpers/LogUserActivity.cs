using API.Extensions;
using API.Interface;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace API.Helpers
{
    public class LogUserActivity : IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var resultContext = await next(); 
            if(!resultContext.HttpContext.User.Identity.IsAuthenticated) { return; }

            var userRepo = resultContext.HttpContext.RequestServices.GetRequiredService<IUserRepository>();
            var user =await userRepo.GetUserById(resultContext.HttpContext.User.GetUserId());
            user.LastActive = DateTime.Now;
            await userRepo.SaveAllAsync();
        }
    }
}
