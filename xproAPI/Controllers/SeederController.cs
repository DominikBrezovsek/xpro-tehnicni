using Microsoft.AspNetCore.Mvc;
using xproAPI.Models;

namespace xproAPI.Controllers
{
    [ApiController]
    [Route("api/Seed")]
    public class SeederController : ControllerBase
    {
        private readonly DataBaseContext _context;

        public SeederController(DataBaseContext context)
        {
            _context = context;
        }

        [HttpGet("seed/")]
        public IActionResult Seed()
        {
            DataBaseContext.Seed(_context);
            return Ok("Database seeded successfully.");
        }
        
    }
}
