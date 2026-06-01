namespace VideoRecorder.Services;
using System.Diagnostics;



/************************************************************************
 * This is the main ffmpeg and mediamtx class. Pretty self-explanatory.
 ************************************************************************/

// get the ffmpeg data of whatever link you add
public class StreamVideo
{
    private int _processCounter;
    private bool _isFfmpegRunning;
    
    public void StreamDataTest(string filename, Guid cameraId)
    {
            Process fProcess = new Process();
            // verbose logs for seeing everything, rtsp transport over tcp, point to the rtsp address
            fProcess.StartInfo.FileName = "/Users/michaeldefilippi/RiderProjects/VideoRecorder/VideoRecorder/Services/ffmpeg";
            fProcess.StartInfo.Arguments = $"-hide_banner -loglevel verbose " +
                                           $"-rtsp_transport tcp -i \"{filename}\" " +
                                           $"-c:v copy -f rtsp " +
                                           $"rtsp://localhost:8554/live/{cameraId}";            
            fProcess.StartInfo.RedirectStandardError = true;
            fProcess.StartInfo.UseShellExecute = false;
            
            if (fProcess.Start())
            {
                _processCounter++;
                Console.WriteLine($"Process counter: {_processCounter}");
            }
            
            fProcess.ErrorDataReceived += (sender, e) => Console.WriteLine(e.Data);
            fProcess.BeginErrorReadLine();
    }
    
    
    // this is run in Program.cs
    public bool StartMediaMtx()
    {
        Process mediaProcess = new Process();
        mediaProcess.StartInfo.FileName = "/Users/michaeldefilippi/RiderProjects/VideoRecorder/VideoRecorder/Services/mediamtx";
        mediaProcess.StartInfo.WorkingDirectory = "/Users/michaeldefilippi/RiderProjects/VideoRecorder/VideoRecorder/Services";
        mediaProcess.StartInfo.RedirectStandardError = true;
        mediaProcess.StartInfo.UseShellExecute = false;
        mediaProcess.ErrorDataReceived += (sender, e) => Console.WriteLine(e.Data);

        _processCounter++;
        
        mediaProcess.Start();
        mediaProcess.BeginErrorReadLine();
        
        return true;
    }
        

    
    
}