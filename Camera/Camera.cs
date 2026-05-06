using System.ComponentModel.DataAnnotations;

namespace VideoRecorder.Camera;

public class Camera
{
    public Guid Id { get; set; } = Guid.NewGuid();
    
    [Required]
    public string? Name { get; set; }
    
    public string? Description { get; set; }
    
    [Required]
    [Display(Name = "RTSP URL")]
    public string RtspUrl { get; set; }
    
    
    //these are for discovered devices 
    
    public string? Scheme { get; set; }
    
    public string? Host { get; set; }
    
    
    public int Port { get; set; }
    
    public string? Path { get; set; }
    
    public string? Username { get; set; }
    
    public string? Password { get; set; }
    
    [Display(Name = "ENABLED")]
    public bool IsEnabled { get; set; }
    
    public bool IsOnvif { get; set; }
    
    public DateTime CreatedAt { get; set; }
    
    
    
}
