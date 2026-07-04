namespace VideoRecorder.Models;

public class CameraGroup
{
    // when ef core sees a property like id it ttreats it as the primary key
    // SQL generates this when we Save()
    public int Id { get; set; }
    
    // group name like west school, east school
    public string Name { get; set; } = "";
    
    public List<Camera> Cameras { get; set; } = new();

    public List<User> Users { get; set; } = new();
    
    public bool IsEnabled { get; set; } = true;
    
    

}