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
 * 
 ****************************************************/
public class CameraHealthService : BackgroundService
{
    public async Task<bool> SendAsyncHealthCheck(Camera camera)
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

    // service is a singleton, 
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly RecordingService _recording;
    private readonly ILogger<CameraHealthService> _logger;

    public CameraHealthService(IServiceScopeFactory scopeFactory,
        RecordingService recording,
        ILogger<CameraHealthService> logger)
    {
        _scopeFactory = scopeFactory;
        _recording = recording;
        _logger = logger;
    }
    
    // background service requires this method
    // host calls it once at startup and runs for the rest of apps life
    protected override async Task ExecuteAsync(CancellationToken stopToken)
    {
        while (!stopToken.IsCancellationRequested)
        {
            // dispose it when finished.
            using var scope = _scopeFactory.CreateScope();
            
            // make a new instance, dont remember everything the main Dbcontext loads
            // then make a fresh connection then return it on dispose()
            var context = scope.ServiceProvider.GetRequiredService<VideoRecorderContext>();

            foreach (var cam in await context.Camera.ToListAsync(stopToken))
            {
                cam.IsOnline = await SendAsyncHealthCheck(cam);
                _recording.RecordingAuthorize(cam);
            }
            
            await context.SaveChangesAsync(stopToken);
            await Task.Delay(5000, stopToken);
        }
        
    }
    
}