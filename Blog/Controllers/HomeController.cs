using Blog.Attributes;
using Microsoft.AspNetCore.Mvc;

namespace Blog.Controllers
{
    [ApiController]
    public class HomeController : ControllerBase
    {
        [HttpGet("status")]
        [ApiKey]
        public IActionResult GetStatus()
        {
            return Ok();
        }
    }
}
