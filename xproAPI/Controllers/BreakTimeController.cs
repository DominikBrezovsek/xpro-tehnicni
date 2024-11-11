using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using xproAPI.Models;

namespace xproAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BreakTimeController : ControllerBase
    {
        private readonly DataBaseContext _dataBaseContext;

        public BreakTimeController(DataBaseContext dataBaseContext)
        {
            _dataBaseContext = dataBaseContext;
        }

        [HttpPost("AddBreakTime")]
        public async Task<ActionResult<BreakDuration>> AddBreakTime([FromBody] int breakDuration)
        {
            await _dataBaseContext.BreakDurations.Where(w => w.Valid == true)
                .ExecuteUpdateAsync(update => update.SetProperty(x => x.Valid, false));
            var breakTime = new BreakDuration()
            {
                Duration = breakDuration,
                Valid = true
            };
            _dataBaseContext.BreakDurations.Add(breakTime);
            await _dataBaseContext.SaveChangesAsync();
            return Ok();
        }
    }
}
