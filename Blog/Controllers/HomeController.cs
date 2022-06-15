using Blog.Attributes;
using Microsoft.AspNetCore.Mvc;

namespace Blog.Controllers
{
    [ApiController]
    public class HomeController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public HomeController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet("status")]
        [ApiKey]
        public IActionResult GetStatus()
        {
            return Ok();
        }

        [HttpGet("environment")]
        [ApiKey]
        public IActionResult GetEnvironment()
        {
            var env = _configuration.GetValue<string>("env");
            return Ok(new
            {
                environment = env
            });
        }
    }
}
