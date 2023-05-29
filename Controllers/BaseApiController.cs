using API.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ServiceFilter(typeof(LogUserActivity))]
    [ApiController]
    [Route("Api/[controller]")]
    public class BaseApiController : Controller
    {
       
    }
}
