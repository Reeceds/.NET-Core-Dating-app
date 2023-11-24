using API.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ServiceFilter(typeof(LogUserActivity))] // Updates the users 'lastActive' field
[ApiController]
[Route("api/[controller]")]

public class BaseApiController : ControllerBase
{

}
