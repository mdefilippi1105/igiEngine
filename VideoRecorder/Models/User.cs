using System.ComponentModel.DataAnnotations;

namespace VideoRecorder.Models;

public class User
{
    public int Id { get; set; }

    [StringLength(50)] 
    public string Username { get; set; } = string.Empty;
    
    [StringLength(256)] 
    public string PasswordHash { get; set; } = string.Empty;
    
    public string Role { get; set; } = "Viewer";

}