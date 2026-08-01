using System.Net.NetworkInformation;
using Microsoft.EntityFrameworkCore;
using VideoRecorder.Database;
using VideoRecorder.Models;

namespace VideoRecorder.Services;

/***************************************************
 * This is the background service to check camera
 * status every few seconds. Without this service,
 * we would only get the camera health status
 * every time we hit refresh or the page loads.
 * When a request comes in, ASP.NET creates a "scope" -
 * a little box for the request. Background service
 * has no request so no box.
 * We also use recorderCount =
 * 
 ****************************************************/
public class CameraHealthService : BackgroundService
{
    
    // service is a singleton, 
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly RecordingService _recording;
    private readonly ILogger<CameraHealthService> _logger;
    private int _recorderCount = 0;
    
    
    public CameraHealthService(IServiceScopeFactory scopeFactory,
        RecordingService recording,
        ILogger<CameraHealthService> logger)
    {
        _scopeFactory = scopeFactory;
        _recording = recording;
        _logger = logger;
    }
    
    
    
    // this is our logic to delete the recordings
    // camera retention time is set by
    // the adding camera / edit camera pages
    private void DeleteOldRecordings(Camera camera)
    {
        var dir = $"/Users/michaeldefilippi/rec-test/{camera.Id}";
        if (!Directory.Exists(dir))
            return;
        
        // this is the oldest video file we keep.
        // anything older than this time stamp gets deleted.
        var cutoff = DateTime.Now.AddDays(-camera.RetentionDays);
        try
        {
            foreach (var file in Directory.GetFiles(dir, "*.mp4"))
            {
                if (File.GetCreationTime(file) < cutoff)
                {
                    File.Delete(file);
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }

    
    public static async Task<bool> SendAsyncHealthCheck(Camera camera)
    {
        try
        {
            if (camera == null)
                return false;

            // use camera.Host if it's not null, else parse RtspUrl and pull host out of it
            var host = camera.Host ?? new Uri(camera.RtspUrl!).Host;
            if (string.IsNullOrEmpty(host))
                return false;
            
            // set to using to implement IDisposable
            using var ping = new Ping();
            var reply = ping.Send(host, 1000);

            return reply.Status == IPStatus.Success;

        }
        catch
        {
            return  false;
        }
        
    }

    
    // background service requires this method
    // host calls it once at startup and runs for the rest of apps life
    protected override async Task ExecuteAsync(CancellationToken stopToken)
    {
        while (!stopToken.IsCancellationRequested)
        {
            // dispose it when finished.
            using var scope = _scopeFactory.CreateScope();
            
            // make a new instance, don't remember everything the main Dbcontext loads
            // then make a fresh connection then return it on dispose()
            var context = scope.ServiceProvider.GetRequiredService<VideoRecorderContext>();
            
            var cameras = await context.Camera.ToListAsync(stopToken);
            
            foreach (var cam in await context.Camera.ToListAsync(stopToken))
            {
                cam.IsOnline = await SendAsyncHealthCheck(cam);
                _recording.RecordingAuthorize(cam);
            }
            
            // 360 counts x 10 seconds = once per hour
            // then we run DeleteRecordings()
            if (_recorderCount++ % 360 == 0)
            {
                foreach (var cam in cameras)
                {
                    DeleteOldRecordings(cam);
                }
            }
            
            await context.SaveChangesAsync(stopToken);
            await Task.Delay(10000, stopToken);
            
        }
        
    }
    
}