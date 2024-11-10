using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using xproAPI.Models;

namespace xproAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly UserContext _userContext;

        public UserController(UserContext userContext)
        {
            _userContext = userContext;
        }

        // Get all users 
        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> GetUsers()
        {
            if (_userContext.Users == null)
            {
                return NotFound();
            }

            return await _userContext.Users.ToListAsync();
        }

        // Get one user
        [HttpGet("{Id:long}")]
        public async Task<ActionResult<User>> GetUser(long Id)
        {
            if (_userContext.Users == null)
            {
                return NotFound();
            }
            var user =  await _userContext.Users.FindAsync(Id);
            if (user == null)
            {
                return NotFound();
            }
            return user;
        }

        [HttpPost]
        public async Task<ActionResult<User>> PostUser(User user)
        {
            _userContext.Users.Add(user);
            await _userContext.SaveChangesAsync();
            return CreatedAtAction(nameof(GetUser), new { Id = user.Id }, user);
        }

        [HttpPut("{Id:long}")]
        public async Task<ActionResult<User>> PutUser(long Id, User user)
        {
            if (Id != user.Id)
            {
                return BadRequest();
            }
            _userContext.Entry(user).State = EntityState.Modified;
            try
            {
                await _userContext.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!(UserExists(Id)))
                {
                    return NotFound();
                }

                throw;
            }
            return NoContent();
        }

        private bool UserExists(long Id)
        {
            return (_userContext.Users?.Any(e => e.Id == Id)).GetValueOrDefault();
        }

        [HttpDelete("{Id:long}")]
        public async Task<ActionResult<User>> DeleteUser(long Id)
        {
            var user = await _userContext.Users.FindAsync(Id);
            if (user == null)
            {
                return NotFound();
            }
            _userContext.Users.Remove(user);
            await _userContext.SaveChangesAsync();
            return Ok();
            
        }
}
}
