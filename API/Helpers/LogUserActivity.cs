using API.Extensions;
using API.Interfaces;
using Microsoft.AspNetCore.Mvc.Filters;

namespace API.Helpers;

public class LogUserActivity : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var resultContext = await next(); // Api has executed and this is equal to the result, use 'ActionExecutingContext' to do something before the API executes

        if (!resultContext.HttpContext.User.Identity.IsAuthenticated) return;

        var userId = resultContext.HttpContext.User.GetUserId();

        var uow = resultContext.HttpContext.RequestServices.GetRequiredService<IUnitOfWork>();
        var user = await uow.UserRepository.GetUserByIdAsync(userId);
        user.LastActive = DateTime.UtcNow; // Logs when the user was last active
        await uow.Complete(); // Updates the db
    }
}
