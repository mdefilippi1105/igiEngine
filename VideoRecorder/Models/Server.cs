namespace VideoRecorder.Models;

public class Server
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    
    public string IpAddress { get; set; } = string.Empty;
    public string Hostname { get; set; }  = string.Empty;
    public int Port { get; set; } = 8554;
    public bool IsEnabled { get; set; } = true;
    public DateTime? LastSeenUtc { get; set; }
    
    public string DrivePath { get; set; } = string.Empty;

    public enum ServerOS
    {
        Windows = 0,
        Linux = 1,
        MacOS = 2
    }
    
    public ServerOS Os { get; set; } = ServerOS.Windows;

    public ICollection<Camera> Cameras { get; set; } = new List<Camera>();

}