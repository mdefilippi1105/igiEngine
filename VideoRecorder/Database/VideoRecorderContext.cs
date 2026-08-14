using Microsoft.EntityFrameworkCore;
using VideoRecorder.Models;


namespace VideoRecorder.Database;
/************************************************************************
// collection that maps directly to database table
// core base class that manages db interaction
// this is the bridge between the code and SQL Server
// think of it like dbo.Camera, dbo.User, etc
************************************************************************/
public class VideoRecorderContext : DbContext 
{

    public VideoRecorderContext(DbContextOptions<VideoRecorderContext> options)
        : base(options)
    {
    }
    public DbSet<Camera> Camera { get; set; }
    
    public DbSet<User> User { get; set; }
    
    public DbSet<CameraGroup> CameraGroup { get; set; }

    public DbSet<Server> Server { get; set; } = null!;


}

