using Microsoft.AspNetCore.Mvc;

namespace TeachTether.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        [HttpGet]
        public IActionResult GetString()
        {
            return Ok("Hello from ASP.NET Web API!");
        }
    }
}
