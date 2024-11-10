namespace xproAPI.Models;

public class User
{
    public long Id { get; set; }
    public string Name { get; set; }
    public string Surname { get; set; }
    public string Email { get; set; }
    public string Phone { get; set; }
    public string Position { get; set; }
    public string EmploymentType { get; set; }
    public string? OtherEmploymentType { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
    public string? ProfileImage { get; set; }
    public bool Active { get; set; }
    
}