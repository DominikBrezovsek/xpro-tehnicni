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

    [HttpPost("Create")]
        public async Task<ActionResult<Absence>> Create([FromBody] Absence absence)
        {
            _dataBaseContext.Absences.Add(absence);
            await _dataBaseContext.SaveChangesAsync();
            return Ok(absence);
        }

        [HttpPost("Update")]
        public async Task<ActionResult<Absence>> Update([FromBody] Absence absence)
        {
            _dataBaseContext.Absences.Update(absence);
            await _dataBaseContext.SaveChangesAsync();
            return Ok(absence);
        }

        [HttpPost("Delete")]
        public async Task<ActionResult<Absence>> Delete([FromBody] long id)
        {
            if ( await _dataBaseContext.Absences.FindAsync(id) == null)
            {
                return NotFound();
            }
            await _dataBaseContext.Absences.Where(w => w.AbsenceId == id).ExecuteDeleteAsync();
            return Ok();
        }
        
    }
}
