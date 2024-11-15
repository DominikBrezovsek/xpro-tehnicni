using Humanizer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
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
        [HttpPost("getWorkTime/")]
        [Authorize]
        public async Task<ActionResult<WorkTime>> GetWorkTimes([FromForm] long userId)
        {
            if (_workTimeContext == null)
            {
                return NotFound();
            }

            if (!UserHasWorkTime(userId))
            {
                return NotFound();
            }
            return await _workTimeContext.WorkTimes.Where(x => x.UserId == userId)
                .Where(w => w.Date == DateOnly.FromDateTime(DateTime.Now))
                .FirstAsync();
        }

        [HttpPost("startWork/")]
        [Authorize]
        public async Task<ActionResult<WorkTime>> StartWork([FromForm] long userId, [FromForm] string startTime)
        {
            if (_workTimeContext == null)
            {
                return NotFound();
            }

            if (UserHasWorkTime(userId))
            {
                var message = new
                {
                    hasWorkTime = "true"
                };
                var json = JsonConvert.SerializeObject(message);
                return Ok(json);
            };
            var allowedBreakDuration = await _workTimeContext.BreakDurations.Where(w => w.Valid == true).FirstAsync();
            Console.Out.WriteLine("Start date is" + DateOnly.FromDateTime(DateTime.UtcNow));
            var time = DateTime.Parse(startTime);
            Console.Out.WriteLine(time);
            var workTime = new WorkTime
            {
                UserId = userId,
                Date = DateOnly.FromDateTime(DateTime.UtcNow),
                ClockIn = TimeOnly.FromDateTime(time),
                TimeZone = TimeZoneInfo.Local.GetUtcOffset(time.ToUniversalTime()).Hours.ToString("00"),
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

        [HttpPost("endWork/"), Authorize]
        public async Task<ActionResult<WorkTime>> EndWork([FromForm] long userId, [FromForm] string endTime)
        {
            if (_workTimeContext == null)
            {
                return NotFound();
            }
            if (!UserHasWorkTime(userId))
            {
                return NotFound();
            }
            var clockOutTimeDt = DateTime.Parse(endTime);
            await _workTimeContext.WorkTimes
                .Where(w => w.UserId == userId)
                .Where(w => w.ClockOut == null)
                .ExecuteUpdateAsync(update => update.SetProperty(u => u.ClockOut, TimeOnly.FromDateTime(clockOutTimeDt)));
            
             TimeOnly clockIn = (_workTimeContext.WorkTimes.Where(w => w.UserId == userId).Select(u => u.ClockIn).First());
             TimeOnly clockOut = TimeOnly.FromDateTime(clockOutTimeDt);
             TimeSpan totalTime = (clockOut - clockIn);
             string totalTimeString = totalTime.ToString("hh\\:mm\\:ss");
             Console.WriteLine(totalTimeString);
             await _workTimeContext.WorkTimes.Where(w => w.UserId == userId)
                 .Where(w => w.Date == DateOnly.FromDateTime(DateTime.Now))
                 .Where(w => w.ClockOut == clockOut)
                 .ExecuteUpdateAsync(update => update.SetProperty(u => u.TotalWorkTime, totalTimeString ));
            return Ok(totalTime);
        }
        
        [HttpPost("addBreak/")]
        public async Task<ActionResult<WorkTime>> EndBreak([FromForm]long userId, [FromForm] string startTime, [FromForm] string endTime)
        {
            if (!UserHasWorkTime(userId))
            {
                return NotFound();
            } 
            var startTimeDt = DateTime.Parse(startTime);
            var endTimeDt = DateTime.Parse(endTime);
            await _workTimeContext.WorkTimes.Where(w => w.UserId == userId)
                .Where(w => w.BreakStart == null)
                .Where(w => w.Date == DateOnly.FromDateTime(DateTime.Now))
                .ExecuteUpdateAsync(update => update.SetProperty(u => u.BreakStart, TimeOnly.FromDateTime(startTimeDt)));
            
            var currentBreak = await _workTimeContext.WorkTimes.Where(w => w.UserId == userId)
                .Where(w => w.BreakEnd == null)
                .Where(w => w.Date == DateOnly.FromDateTime(DateTime.Now))
                .FirstAsync();
            var allowedBreakDuration = _workTimeContext.BreakDurations.Where(w => w.Valid == true).Select(s => s.Duration).First();
            await _workTimeContext.WorkTimes.Where(w => w.UserId == userId)
                .Where(w => w.Id == currentBreak.Id)
                .ExecuteUpdateAsync(update => update.SetProperty(u => u.BreakEnd, TimeOnly.FromDateTime(endTimeDt)));
            var breakStartedAt = _workTimeContext.WorkTimes.Where(w => w.UserId == userId)
                .Where(w => w.Id == currentBreak.Id)
                .Select(w => w.BreakStart).First();
            var breakStart = TimeOnly.Parse(breakStartedAt.ToString());
            var breakDuration = (TimeOnly.FromDateTime(endTimeDt) - breakStart);
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
            return Ok();
        }
    }
}