using System.ComponentModel.DataAnnotations;

namespace VideoRecorder.Camera;

public class Camera
{
    public Guid Id { get; set; } = Guid.NewGuid();
    
    
    [Display(Name = "Camera Name")]
    public string? Name { get; set; }
    
    public string? Description { get; set; }
    
    
    [Display(Name = "RTSP URL")]
    public string? RtspUrl { get; set; }
    
    
    //these are for discovered devices 
    public string? Scheme { get; set; }
    
    
    [Display(Name = "IP Address")]
    public string? Host { get; set; }
    
    
    [Display(Name = "Port")]
    public int? Port { get; set; }
    
    
    public string? Path { get; set; }
    
    
    [Display(Name = "Username")]
    public string? Username  { get; set; }
    
    
    [Display(Name = "Password")]
    public string? Password { get; set; }
    
    
    [Display(Name = "Enabled")]
    public bool IsEnabled { get; set; }
    
    public string? Manufacturer { get; set; }
    
    public bool IsOnvif { get; set; }
    
    
    public DateTime CreatedAt { get; set; }
    
    
    
}
