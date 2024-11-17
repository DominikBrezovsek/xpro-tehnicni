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
                var message = new  {
                    hasWorkTime = "false",
                };
                var json = JsonConvert.SerializeObject(message);
                return Ok(json);
            }

            if (!UserHasWorkTime(userId))
            {
                var message = new  {
                    hasWorkTime = "false",
                };
                var json = JsonConvert.SerializeObject(message);
                return Ok(json);
            }
            var currentDate = DateTime.Now;
            var entry = await _workTimeContext.WorkTimes.Where(w => w.UserId == userId)
                .Where(w => w.Date == DateOnly.FromDateTime(currentDate))
                .FirstOrDefaultAsync();
            Console.WriteLine(currentDate);
            return entry;
        }

        [HttpPost("startWork/")]
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
            var currentDate = DateTime.Now;
            var allowedBreakDuration = await _workTimeContext.BreakDurations.Where(w => w.Valid == true).FirstAsync();
            Console.Out.WriteLine("Start date is" + DateOnly.FromDateTime(DateTime.Now));
            var time = DateTime.Parse(startTime);
            Console.Out.WriteLine(time);
            var workTime = new WorkTime
            {
                UserId = userId,
                Date = DateOnly.FromDateTime(currentDate),
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
            var currentDate = DateTime.Now;
            return _workTimeContext.WorkTimes.Where(u => u.UserId == userId)
                .Any(w => w.Date == DateOnly.FromDateTime(currentDate));
        }

        [HttpPost("endWork/")]
        [Authorize]
        public async Task<ActionResult<WorkTime>> EndWork([FromForm] long workId, [FromForm] string endTime)
        {
            if (_workTimeContext == null)
            {
                return NotFound();
            }

            var clockOutTimeDt = DateTime.Parse(endTime);
            var clockIn = (_workTimeContext.WorkTimes.Where(w => w.Id == workId).Select(u => u.ClockIn).First());
            if (TimeOnly.FromDateTime(clockOutTimeDt) < clockIn)
            {
                var error = new
                {
                    error = "endBeforeBeginning"
                };
                var json = JsonConvert.SerializeObject(error);
                return Ok(json);
            }

            await _workTimeContext.WorkTimes
                .Where(w => w.Id == workId)
                .ExecuteUpdateAsync(
                    update => update.SetProperty(u => u.ClockOut, TimeOnly.FromDateTime(clockOutTimeDt)));
            TimeSpan totalTime;
            TimeOnly clockOut = TimeOnly.FromDateTime(clockOutTimeDt);
            if (clockIn.HasValue && clockIn != null)
            {
                totalTime = (TimeSpan)(clockIn - clockIn);

                await _workTimeContext.WorkTimes.Where(w => w.Id == workId)
                    .ExecuteUpdateAsync(update =>
                        update.SetProperty(u => u.TotalWorkTime, totalTime.ToString("hh\\:mm\\:ss")));
                return Ok(totalTime);
            }
            return Ok("Napaka pri totalTime");
        }
        
        [HttpPost("addBreak/")]
        [Authorize]
        public async Task<ActionResult<WorkTime>> AddBreak([FromForm]long workId, [FromForm] string startTime, [FromForm] string endTime)
        {
            var worktime = await _workTimeContext.WorkTimes
                .Where(w => w.Id == workId)
                .FirstAsync();
            if (worktime == null)
            {
                var response = new
                {
                    hasWorkTime = "false"
                };
                var json = JsonConvert.SerializeObject(response);
                return Ok(json);
            }
            Console.Out.WriteLine(startTime);
            Console.Out.WriteLine(endTime);
            var startTimeDt = DateTime.Parse(startTime);
            var endTimeDt = DateTime.Parse(endTime);
            if (TimeOnly.FromDateTime(startTimeDt) < worktime.ClockIn)
            {
                var error = new
                {
                    error = "beforeWorkTime"
                };
                var json = JsonConvert.SerializeObject(error);
                return Ok(json);
            }
            if (TimeOnly.FromDateTime(endTimeDt) > worktime.ClockOut)
            {
                var error = new
                {
                    error = "afterWorkTime"
                };
                var json = JsonConvert.SerializeObject(error);
                return Ok(json);
            }
            // set break start time
            await _workTimeContext.WorkTimes.Where(w => w.Id == worktime.Id)
                .ExecuteUpdateAsync(update => 
                    update.SetProperty(u => u.BreakStart, TimeOnly.FromDateTime(startTimeDt)
                    ));
            //get allowed break time duration for later use
            var allowedBreakDuration = _workTimeContext.BreakDurations.Where(w => w.Valid == true).Select(s => s.Duration).First();
            // set break end time
            await _workTimeContext.WorkTimes.Where(w => w.Id == worktime.Id)
                .ExecuteUpdateAsync(update => update.SetProperty(u => u.BreakEnd, TimeOnly.FromDateTime(endTimeDt))); 
            var breakStart = TimeOnly.FromDateTime(startTimeDt);
            var breakEnd = TimeOnly.FromDateTime(endTimeDt);
            var breakDuration = (breakEnd- breakStart);
            Console.Out.WriteLine($"breakStart is {breakStart} and breakEnd is {breakEnd}");
            await _workTimeContext.WorkTimes.Where(w => w.Id == worktime.Id)
                .ExecuteUpdateAsync(update => update.SetProperty(u => u.BreakDuration, TimeOnly.FromTimeSpan(breakDuration)));
            var allowedBreak = TimeSpan.FromMinutes(allowedBreakDuration);
            Console.WriteLine(allowedBreak);
            if (allowedBreak < breakDuration)
            {
                var exceededBreakDuration = breakDuration - allowedBreak;
                await _workTimeContext.WorkTimes.Where(w => w.Id == worktime.Id)
                    .ExecuteUpdateAsync(update => update.SetProperty(u => u.BreakOverAllowedTime, TimeOnly.FromTimeSpan(exceededBreakDuration)));
            }
            return Ok();
        }

        [HttpPost("getUserWorkTimes/")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<WorkTime>>> GetUserWorkTimes([FromForm] long userId, [FromForm] string? month)
        {
            int searchMonth = 0;
            if (month == null)
            {
                searchMonth = DateTime.Now.Month;
            }
            else
            {
                searchMonth = DateTime.Parse(month).Month;
            }
            return await _workTimeContext.WorkTimes.Where(w => w.UserId == userId).Where(w => w.Date.Month == searchMonth ).OrderBy(o => o.Date).ToListAsync();
        }
        [HttpPost("updateUserWorkTimes/")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<WorkTime>>> UpdateUserWorkTimes([FromForm] long workId, [FromForm] string clockInTime, [FromForm] string clockOutTime,
            [FromForm] string breakStartTime, [FromForm] string breakEndTime)
        {
            var newClockIn = DateTime.Parse(clockInTime);
            var newClockOut = DateTime.Parse(clockOutTime);
            var newBreakStart = DateTime.Parse(breakStartTime);
            var newBreakEnd = DateTime.Parse(breakEndTime);
            var newBreakDuration = (newBreakEnd - newBreakStart);
            var newTotalWorkTime = (newClockOut - newClockIn);
            var allowedBreakDuration = await _workTimeContext.BreakDurations.Where(w => w.Valid == true).Select(s => s.Duration).FirstAsync();
            await _workTimeContext.WorkTimes.Where(w => w.Id == workId)
                .ExecuteUpdateAsync(update => update.SetProperty(u => u.ClockIn, TimeOnly.FromDateTime(newClockIn))
                .SetProperty(u => u.ClockOut, TimeOnly.FromDateTime(newClockOut))
                .SetProperty(u => u.BreakStart, TimeOnly.FromDateTime(newBreakStart))
                .SetProperty(u => u.BreakEnd, TimeOnly.FromDateTime(newBreakEnd)));
            if (newBreakDuration > TimeSpan.FromMinutes(allowedBreakDuration))
            {
                var exceededBreakDuration = newBreakDuration - TimeSpan.FromMinutes(allowedBreakDuration);
                await _workTimeContext.WorkTimes.Where(w => w.Id == workId)
                    .ExecuteUpdateAsync(update =>
                        update.SetProperty(u => u.BreakOverAllowedTime, TimeOnly.FromTimeSpan(exceededBreakDuration)));
            }
            else
            {
                await _workTimeContext.WorkTimes.Where(w => w.Id == workId)
                    .ExecuteUpdateAsync(update => update.SetProperty(u => u.BreakOverAllowedTime, TimeOnly.FromTimeSpan(TimeSpan.FromMinutes(0))));
            }
            await _workTimeContext.WorkTimes.Where(w => w.Id == workId)
                .ExecuteUpdateAsync(update => update.SetProperty(u => u.BreakDuration, TimeOnly.FromTimeSpan(newBreakDuration))
                    .SetProperty(u => u.TotalWorkTime, TimeOnly.FromTimeSpan(newTotalWorkTime).ToString("hh\\:mm\\:ss")));
            
            return Ok();
        }

        [HttpPost("addAbsence/")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<WorkTime>>> AddUserAbsence([FromForm] long userId,
            [FromForm] long absenceId)
        {
            var today = DateTime.Now;
            var date = DateOnly.FromDateTime(today);
            var userIsAlreadyWorking = await _workTimeContext.WorkTimes.Where(w => w.UserId == userId)
                .AnyAsync(a => a.Date == date);
            if (userIsAlreadyWorking)
            {
                var message = new
                {
                    error = "alreadyWorking",
                };
                var json = JsonConvert.SerializeObject(message);
                return Ok(json);
            }
            var allowedBreakId = await _workTimeContext.BreakDurations.Where(w => w.Valid == true).Select(s => s.Id).FirstAsync();
            var worktime = new WorkTime()
            {
                UserId = userId,
                Date = date,
                Absent = true,
                AbsentType = absenceId,
                BreakDurationId = allowedBreakId,
                ClockIn = null,
            };
            await _workTimeContext.WorkTimes.AddAsync(worktime);
            await _workTimeContext.SaveChangesAsync();
            return Ok();

        }
    }
}