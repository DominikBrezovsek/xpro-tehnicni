using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using xproAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Newtonsoft.Json;
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
    [HttpGet("getAllUsers/"), Authorize]
    public async Task<ActionResult<IEnumerable<User>>> GetUsers()
    {
        if (_dataBaseContext.Users == null) return NotFound();

        return await _dataBaseContext.Users.ToListAsync();
    }

    // Get one user
    [HttpPost("GetUser/")]
    public async Task<ActionResult<User>> GetUser([FromForm] long id)
    {
        if (_dataBaseContext.Users == null) return NotFound();
        var user = await _dataBaseContext.Users.FindAsync(id);
        if (user == null) return NotFound();
        return user;
    }

    [HttpPost("createUser/")]
    [Authorize]
    public async Task<ActionResult<User>> PostUser([FromForm] string name, [FromForm] string surname,
        [FromForm] string email, [FromForm] string phone, [FromForm] string position, [FromForm] string empType,
        [FromForm] string username, [FromForm] string password, [FromForm] bool active,  [FromForm] string empNote = "",
        [FromForm] string role="user")
    {
        
        
        User user = new User()
        {
            Name = name,
            Surname = surname,
            Email = email,
            Phone = phone,
            Position = position,
            Role = role,
            EmploymentType = empType,
            OtherEmploymentType = empNote,
            Username = username,
            Password = BCrypt.Net.BCrypt.EnhancedHashPassword(password),
            Active = active,
            
        };
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        _dataBaseContext.Users.Add(user);
        await _dataBaseContext.SaveChangesAsync();
        return CreatedAtAction(nameof(GetUser), new { user.Id }, user);
    }

    [HttpPost("updateUser/")]
    [Authorize]
    public async Task<ActionResult<User>> PutUser([FromForm] long userId, [FromForm] string name, [FromForm] string surname,
        [FromForm] string email, [FromForm] string phone, [FromForm] string active)
    {
        bool userIsActive = active == "Aktiven";
        await _dataBaseContext.Users.Where(w => w.Id == userId).ExecuteUpdateAsync(update =>
            update.SetProperty(u => u.Name, name)
                .SetProperty(u => u.Surname, surname)
                .SetProperty(u => u.Email, email)
                .SetProperty(u => u.Phone, phone)
                .SetProperty(u => u.Active, userIsActive));
        return Ok(new {success = true});
    }

    private bool UserExists(long Id)
    {
        return (_dataBaseContext.Users?.Any(e => e.Id == Id)).GetValueOrDefault();
    }

    [HttpPost("deleteUser/")]
    [Authorize]
    public async Task<ActionResult<User>> DeleteUser([FromForm]long userId)
    {
        var user = await _dataBaseContext.Users.FindAsync(userId);
        if (user == null) return NotFound();
        _dataBaseContext.Users.Remove(user);
        await _dataBaseContext.SaveChangesAsync();
        return Ok(new {success = true});
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

        var token = TokenController.CreateToken(user.Role);
        var response = new
        {
            id = user.Id,
            token = token
        };
        return Ok(response);
    }

    [HttpPost("setProfilePicture/")]
    [Authorize]

    public async Task<ActionResult> SetProfilePicture([FromForm] long userId, [FromForm] IFormFile image)
    {
        using var memoryStream = new MemoryStream();
        await image.CopyToAsync(memoryStream);

        if (memoryStream.Length < 2097152)
        {
            await _dataBaseContext.Users.Where(w => w.Id == userId).ExecuteUpdateAsync(update =>
                update.SetProperty(u => u.ProfileImage, memoryStream.ToArray()));
            return Ok(new { success = true });
        } 
        return Ok(new { error = "tooLarge" });
    }

    [HttpPost("setNewPassword/")]
    [Authorize]
    public async Task<ActionResult> SetNewPassword([FromForm] long userId,[FromForm] string currentPass , [FromForm] string newPass)
    {
        var curentPassHash = await _dataBaseContext.Users.Where(w => w.Id == userId).Select(s => s.Password).FirstAsync();
        if (!BCrypt.Net.BCrypt.EnhancedVerify(currentPass, curentPassHash))
        {
            return Ok(new { error = "wrongCurrentPass" });
        }
        var hash = BCrypt.Net.BCrypt.EnhancedHashPassword(newPass);

        await _dataBaseContext.Users
            .Where(w=> w.Id == userId)
            .ExecuteUpdateAsync(update => update.SetProperty(u => u.Password, hash));
        return Ok();
    }
}