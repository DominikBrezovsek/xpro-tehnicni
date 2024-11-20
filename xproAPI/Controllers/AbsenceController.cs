using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using xproAPI.Models;

namespace xproAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AbsenceController : ControllerBase
    {
        private readonly DataBaseContext _dataBaseContext;

        public AbsenceController(DataBaseContext dataBaseContext)
        {
            _dataBaseContext = dataBaseContext;
        }

        [HttpGet("getAbsences/")]
        public async Task<ActionResult<IEnumerable<Absence>>> GetAbsences()
        {
            return await _dataBaseContext.Absences.ToListAsync();
        }

    [HttpPost("createAbsence/")]
        public async Task<ActionResult<Absence>> Create([FromForm] string absenceName)
        {
            Absence absence = new Absence()
            {
                AbsenceName = absenceName
            };
            _dataBaseContext.Absences.Add(absence);
            await _dataBaseContext.SaveChangesAsync();
            return Ok(absence);
        }

        [HttpPost("updateAbsence/")]
        public async Task<ActionResult<Absence>> Update([FromForm] string absenceName, [FromForm] long absenceId)
        {
            await _dataBaseContext.Absences.Where(w => w.AbsenceId == absenceId).ExecuteUpdateAsync(update =>
                update.SetProperty(u => u.AbsenceName, absenceName));
            await _dataBaseContext.SaveChangesAsync();
            return Ok();
        }

        [HttpPost("deleteAbsence/")]
        public async Task<ActionResult<Absence>> Delete([FromForm] long absenceId)
        {
            if ( await _dataBaseContext.Absences.FindAsync(absenceId) == null)
            {
                return NotFound();
            }
            await _dataBaseContext.Absences.Where(w => w.AbsenceId == absenceId).ExecuteDeleteAsync();
            return Ok();
        }
        
    }
}
