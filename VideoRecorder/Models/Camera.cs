using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace VideoRecorder.Models;

public class Camera
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public bool IsOnline { get; set; } = false;
    
    [StringLength(100)]
    [Display(Name = "Camera Name")]
    public string? Name { get; set; }
    
    
    [StringLength(100)]
    public string? Description { get; set; }
    
    [StringLength(500)]
    [Display(Name = "RTSP URL")]
    public string? RtspUrl { get; set; }
    
    
    [StringLength(100)]
    //these are for discovered devices 
    public string? Scheme { get; set; }
    
    
    [StringLength(100)]
    [Display(Name = "IP Address")]
    //regex to make sure it's a valid IP
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
    
    public int? CameraGroupId { get; set; }
    
    public CameraGroup? CameraGroup { get; set; }
    
    /*********************************************
     * Recording settings and retention
     **********************************************/
    public bool IsRecording { get; set; }
    
    public bool UserToggledRecording { get; set; }

    // we are keeping the default retention at 30, this can be selected via camera edit page
    public int RetentionDays { get; set; } = 30;


}
