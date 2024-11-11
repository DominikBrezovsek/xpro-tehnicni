using Humanizer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using xproAPI.Models;

namespace xproAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WorkTimeController : ControllerBase
    {
        private readonly DataBaseContext _workTimeContext;

        public WorkTimeController(DataBaseContext workTimeContext)
        {
            _workTimeContext = workTimeContext;
        }
        [HttpGet("{userId:long}")]
        public async Task<ActionResult<IEnumerable<WorkTime>>> GetWorkTimes(long userId)
        {
            if (_workTimeContext == null)
            {
                return NotFound();
            }

            if (!UserHasWorkTime(userId))
            {
                return NotFound();
            }
            return await _workTimeContext.WorkTimes.Where(x => x.UserId == userId).ToArrayAsync();
        }

        [HttpPost("startWork/{userId:long}", Name = "StartWork")]
        public async Task<ActionResult<WorkTime>> StartWork(long userId, [FromBody] DateTime startTime)
        {
            if (_workTimeContext == null)
            {
                return NotFound();
            }
            var allowedBreakDuration = await _workTimeContext.BreakDurations.Where(w => w.Valid == true).FirstAsync();
            Console.Out.WriteLine("Start date is" + DateOnly.FromDateTime(DateTime.UtcNow));
            var startTimeUtc = startTime.ToUniversalTime();
            Console.Out.WriteLine(startTimeUtc);
            var workTime = new WorkTime
            {
                UserId = userId,
                Date = DateOnly.FromDateTime(DateTime.UtcNow),
                ClockIn = TimeOnly.FromDateTime(startTimeUtc),
                TimeZone = TimeZoneInfo.Local.GetUtcOffset(startTime.ToUniversalTime()).Hours.ToString("00"),
                BreakDurationId = allowedBreakDuration.Id,
            };
            _workTimeContext.WorkTimes.Add(workTime);
            await _workTimeContext.SaveChangesAsync();
            return Ok(workTime);
        }

        private bool UserHasWorkTime(long userId)
        {
            return _workTimeContext.WorkTimes.Any(u => u.UserId == userId);
        }

        [HttpPost("endWork/{userId:long}", Name = "EndWork")]
        public async Task<ActionResult<WorkTime>> EndWork(long userId, [FromBody] DateTime clockOutTime)
        {
            if (_workTimeContext == null)
            {
                return NotFound();
            }
            if (!UserHasWorkTime(userId))
            {
                return NotFound();
            }
            await _workTimeContext.WorkTimes
                .Where(w => w.UserId == userId)
                .Where(w => w.ClockOut == null)
                .ExecuteUpdateAsync(update => update.SetProperty(u => u.ClockOut, TimeOnly.FromDateTime(clockOutTime.ToUniversalTime())));
            
             TimeOnly clockIn = (_workTimeContext.WorkTimes.Where(w => w.UserId == userId).Select(u => u.ClockIn).First());
             TimeOnly clockOut = TimeOnly.FromDateTime(clockOutTime.ToUniversalTime());
             TimeSpan totalTime = (clockOut - clockIn);
             string totalTimeString = totalTime.ToString("hh\\:mm\\:ss");
             Console.WriteLine(totalTimeString);
             await _workTimeContext.WorkTimes.Where(w => w.UserId == userId)
                 .Where(w => w.Date == DateOnly.FromDateTime(DateTime.Now))
                 .Where(w => w.ClockOut == clockOut)
                 .ExecuteUpdateAsync(update => update.SetProperty(u => u.TotalWorkTime, totalTimeString ));
            return Ok(totalTime);
        }

        [HttpPost("/startBreak/{userId:long}")]
        public async Task<ActionResult<WorkTime>> StartBreak(long userId, [FromBody] DateTime startTime)
        {
            if (!UserHasWorkTime(userId))
            {
                return NotFound();
            }
            await _workTimeContext.WorkTimes.Where(w => w.UserId == userId)
                .Where(w => w.BreakStart == null)
                .Where(w => w.Date == DateOnly.FromDateTime(DateTime.Now))
                .ExecuteUpdateAsync(update => update.SetProperty(u => u.BreakStart, TimeOnly.FromDateTime(startTime.ToUniversalTime())));
            return Ok($"Break started at {TimeOnly.FromDateTime(startTime).ToString()}");
        }
        
        [HttpPost("/endBreak/{userId:long}")]
        public async Task<ActionResult<WorkTime>> EndBreak(long userId, [FromBody] DateTime endTime)
        {
            if (!UserHasWorkTime(userId))
            {
                return NotFound();
            } 
            var currentBreak = await _workTimeContext.WorkTimes.Where(w => w.UserId == userId)
                .Where(w => w.BreakEnd == null)
                .Where(w => w.Date == DateOnly.FromDateTime(DateTime.Now))
                .FirstAsync();
            var allowedBreakDuration = _workTimeContext.BreakDurations.Where(w => w.Valid == true).Select(s => s.Duration).First();
            await _workTimeContext.WorkTimes.Where(w => w.UserId == userId)
                .Where(w => w.Id == currentBreak.Id)
                .ExecuteUpdateAsync(update => update.SetProperty(u => u.BreakEnd, TimeOnly.FromDateTime(endTime.ToUniversalTime())));
            var breakStartedAt = _workTimeContext.WorkTimes.Where(w => w.UserId == userId)
                .Where(w => w.Id == currentBreak.Id)
                .Select(w => w.BreakStart).First();
            var breakStart = TimeOnly.Parse(breakStartedAt.ToString());
            var breakDuration = (TimeOnly.FromDateTime(endTime) - breakStart);
            await _workTimeContext.WorkTimes.Where(w => w.UserId == userId)
                .Where(w => w.Id == currentBreak.Id)
                .ExecuteUpdateAsync(update => update.SetProperty(u => u.BreakDuration, TimeOnly.FromTimeSpan(breakDuration)));
            var allowedBreak = TimeSpan.FromMinutes(allowedBreakDuration);
            Console.WriteLine(allowedBreak);
            if (allowedBreak < breakDuration)
            {
                var exceededBreakDuration = breakDuration - allowedBreak;
                await _workTimeContext.WorkTimes.Where(w => w.UserId == userId)
                    .Where(w => w.Id == currentBreak.Id)
                    .ExecuteUpdateAsync(update => update.SetProperty(u => u.BreakOverAllowedTime, TimeOnly.FromTimeSpan(exceededBreakDuration)));
            }
            return Ok($"Break ended at {TimeOnly.FromDateTime(endTime).ToString()}");
        }
    }
}