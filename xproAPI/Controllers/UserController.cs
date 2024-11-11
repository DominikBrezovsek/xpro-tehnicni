using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using xproAPI.Models;

namespace xproAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UserController : ControllerBase
{
    private readonly DataBaseContext _dataBaseContext;

    public UserController(DataBaseContext dataBaseContext)
    {
        _dataBaseContext = dataBaseContext;
    }

    // Get all users 
    [HttpGet]
    public async Task<ActionResult<IEnumerable<User>>> GetUsers()
    {
        if (_dataBaseContext.Users == null) return NotFound();

        return await _dataBaseContext.Users.ToListAsync();
    }

    // Get one user
    [HttpGet("{Id:long}")]
    public async Task<ActionResult<User>> GetUser(long Id)
    {
        if (_dataBaseContext.Users == null) return NotFound();
        var user = await _dataBaseContext.Users.FindAsync(Id);
        if (user == null) return NotFound();
        return user;
    }

    [HttpPost]
    public async Task<ActionResult<User>> PostUser(User user)
    {
        _dataBaseContext.Users.Add(user);
        await _dataBaseContext.SaveChangesAsync();
        return CreatedAtAction(nameof(GetUser), new { user.Id }, user);
    }

    [HttpPut("{Id:long}")]
    public async Task<ActionResult<User>> PutUser(long Id, User user)
    {
        if (Id != user.Id) return BadRequest();
        _dataBaseContext.Entry(user).State = EntityState.Modified;
        try
        {
            await _dataBaseContext.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!UserExists(Id)) return NotFound();

            throw;
        }

        return NoContent();
    }

    private bool UserExists(long Id)
    {
        return (_dataBaseContext.Users?.Any(e => e.Id == Id)).GetValueOrDefault();
    }

    [HttpDelete("{Id:long}")]
    public async Task<ActionResult<User>> DeleteUser(long Id)
    {
        var user = await _dataBaseContext.Users.FindAsync(Id);
        if (user == null) return NotFound();
        _dataBaseContext.Users.Remove(user);
        await _dataBaseContext.SaveChangesAsync();
        return Ok();
    }
}