using VideoRecorder.Util;

namespace VideoRecorder.Services;
using System.Diagnostics;



/************************************************************************
 * This is the main ffmpeg and mediamtx class. Pretty self-explanatory.
 ************************************************************************/

// get the ffmpeg data of whatever link you add
public class StreamVideo : IDisposable
{
    private int _processCounter;
    private bool _isFfmpegRunning;
    private Process _fProcess;


    
    public void StreamDataTest(string filename, Guid cameraId)
    {
            _fProcess = new Process();
            // verbose logs for seeing everything, rtsp transport over tcp, point to the rtsp address
            _fProcess.StartInfo.FileName = "/Users/michaeldefilippi/RiderProjects/VideoRecorder/VideoRecorder/ThirdParty/ffmpeg";
            _fProcess.StartInfo.Arguments = $"-hide_banner " +
                                           $"-loglevel verbose " +
                                           $"-rtsp_transport tcp -i \"{filename}\" " +
                                           $"-c:v copy -f rtsp " +
                                           $"rtsp://localhost:8554/live/{cameraId}";            
            _fProcess.StartInfo.RedirectStandardError = true;
            _fProcess.StartInfo.UseShellExecute = false;
            
            if (_fProcess.Start())
            {
                _processCounter++;
                Console.WriteLine($"Process counter: {_processCounter}");
            }
            
            _fProcess.ErrorDataReceived += (sender, e) => Console.WriteLine(e.Data);
            _fProcess.BeginErrorReadLine();
            
            // here we are adding the cam id to our dictionary
            SharedData.StreamObjects[cameraId.ToString()] = this;
        
    }
    // this destroys the ffmpeg process
    public void Dispose()
    {
        if (_fProcess is not null && !_fProcess.HasExited)
        {
            _fProcess.WaitForExit(3000);
            _fProcess.Kill(entireProcessTree: true);
        }
        _fProcess?.Dispose();
        
        GC.SuppressFinalize(this);
    }
    
    
    // this is run in Program.cs
    public bool StartMediaMtx()
    {
        Process mediaProcess = new Process();
        mediaProcess.StartInfo.FileName = "/Users/michaeldefilippi/RiderProjects/VideoRecorder/VideoRecorder/ThirdParty/mediamtx";
        mediaProcess.StartInfo.WorkingDirectory = "/Users/michaeldefilippi/RiderProjects/VideoRecorder/VideoRecorder/ThirdParty";
        mediaProcess.StartInfo.RedirectStandardError = true;
        mediaProcess.StartInfo.UseShellExecute = false;
        mediaProcess.ErrorDataReceived += (sender, e) => Console.WriteLine(e.Data);

        _processCounter++;
        
        mediaProcess.Start();
        mediaProcess.BeginErrorReadLine();
        
        return true;
    }
    




}