using System.ComponentModel.DataAnnotations;

namespace VideoRecorder.Camera;

public class Camera
{
    public Guid Id { get; set; } = Guid.NewGuid();
    
   
    [StringLength(100)]
    [Display(Name = "Camera Name")]
    public string? Name { get; set; }
    
    
    [StringLength(100)]
    public string? Description { get; set; }
    
    [StringLength(100)]
    [Display(Name = "RTSP URL")]
    public string? RtspUrl { get; set; }
    
    
    [StringLength(100)]
    //these are for discovered devices 
    public string? Scheme { get; set; }
    
    
    [StringLength(100)]
    [Display(Name = "IP Address")]
    //regex to make sure its a valid IP
    [RegularExpression(@"^(\d{1,3}\.){3}\d{1,3}$", ErrorMessage = "Must be a valid IP")]
    public string? Host { get; set; }
    
    
    [StringLength(100)]
    public string? XAddress { get; set; }

   
    [Range(1, 65535)]
    [Display(Name = "Port")] 
    public int? Port { get; set; } = 554;
    
    [StringLength(100)]
    public string? Path { get; set; }
    
    [StringLength(100)]
    [Display(Name = "Username")]
    public string? Username  { get; set; }
    
    [StringLength(100)]
    [Display(Name = "Password")]
    public string? Password { get; set; }
    
    
    [Display(Name = "Enabled")]
    public bool IsEnabled { get; set; }

    
    [StringLength(100)]
    public string? Manufacturer { get; set; } = ""; //cheap fix to prevent crash
    
    public bool IsOnvif { get; set; }
    
   
    [StringLength(100)]
    public string? Model { get; set; }
    
    
    public DateTime CreatedAt { get; set; }
    
    
    
}
