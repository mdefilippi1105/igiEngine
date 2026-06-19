using System.ComponentModel.DataAnnotations;

namespace VideoRecorder.Models;

public class User
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [StringLength(50)] 
    public string Username { get; set; } = string.Empty;
    
    [StringLength(256)] 
    public string PasswordHash { get; set; } = string.Empty;

    public UserRoles Role { get; set; } = UserRoles.Viewer;
    
    public bool IsEnabled { get; set; } = true;
    
    /********************************************************
     * these following fields are kind of tertiary...as in
     * they really don't need to be set, but they can, in case
     * somebody wants more info other than a user/pass
     ******************************************************* */
    [StringLength(50)] 
    public string FirstName { get; set; } = string.Empty;
    
    [StringLength(50)] 
    public string LastName { get; set; } = string.Empty;
    
    [StringLength(50)] 
    public string Email { get; set; } = string.Empty;
    
    [StringLength(20)] 
    public string PhoneNumber { get; set; } = string.Empty;
    
    [StringLength(50)] 
    public string Department { get; set; } = string.Empty;
    
}