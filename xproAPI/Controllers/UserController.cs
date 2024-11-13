using System.Runtime.InteropServices.JavaScript;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using xproAPI.Models;
using BCrypt.Net;
using Newtonsoft.Json;
using NuGet.Protocol;

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
    public async Task<ActionResult<User>> PostUser([FromForm] string name, [FromForm] string surname,
        [FromForm] string email, [FromForm] string phone, [FromForm] string position, [FromForm] string empType,
        [FromForm] string username, [FromForm] string password, [FromForm] string empNote = "",
        [FromForm] string profileImage = "")
    {
        
        
        User user = new User()
        {
            Name = name,
            Surname = surname,
            Email = email,
            Phone = phone,
            Position = position,
            EmploymentType = empType,
            OtherEmploymentType = empNote,
            Username = username,
            Password = BCrypt.Net.BCrypt.EnhancedHashPassword(password),
            Active = true,
            
        };
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
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

    [HttpPost("login/")]
    public async Task<ActionResult<User>> Login([FromForm] string username, [FromForm] string password)
    {
        var userExists = _dataBaseContext.Users.Any(w => w.Username == username);
        if (!userExists)
        {
            var error = new
            {
                error = "username"
            };
            string json = JsonConvert.SerializeObject(error);
            return Ok(json);
        }
        var user  = await _dataBaseContext.Users.Where(w => w.Username == username).FirstAsync();
        var isPassValid = BCrypt.Net.BCrypt.EnhancedVerify(password, user.Password);
        if (!isPassValid)
        {
            var error = new
            {
                error = "password"
            };
            string json = JsonConvert.SerializeObject(error);
            return Ok(json);
        }
        return Ok(user);
    }
}