using System.Collections.Concurrent;
using System.Diagnostics;

namespace VideoRecorder.Services;

public class RecordingService
{
    //  the id is the key, the running Process is the value
    // concurrent with multiple requests hitting it 
    private readonly ConcurrentDictionary<Guid, Process> _recordings = new();

    public void Start(Guid cameraId, string cameraUrl)
    {
        // guard - if cam in dictionary, return
        if (_recordings.ContainsKey(cameraId))
            return;
        
        // $@ keeps the slashes verbatim and string interpolation
        var recordingDirectory = $@"/Users/michaeldefilippi/rec-test/{cameraId}";
        Directory.CreateDirectory(recordingDirectory);
        
        // here we start ffmpeg and spin up a new Process
        var recordProcess = new Process();
        recordProcess.StartInfo.FileName = "/Users/michaeldefilippi/RiderProjects/VideoRecorder/VideoRecorder/ThirdParty/ffmpeg";
        recordProcess.StartInfo.Arguments = $"-rtsp_transport tcp -i \"{cameraUrl}\" " +
                                            $"-c copy " +
                                            $"-f segment -segment_time 300 " +
                                            $"-reset_timestamps 1 -strftime 1 " +
                                            $"-segment_format_options movflags=+frag_keyframe+empty_moov " +
                                            $"\"{recordingDirectory}/%Y%m%d_%H%M%S.mp4\"";
        recordProcess.StartInfo.RedirectStandardInput = true;
        recordProcess.StartInfo.UseShellExecute = false;
        recordProcess.StartInfo.CreateNoWindow = true;
        
        
        
        // lastly we add the id and the process to the dictionary
        _recordings.TryAdd(cameraId, recordProcess);
        
        // start the recordings
        recordProcess.Start();
    }
    
    
    // we pull the reference out of the dictionary, to kill
    // the process and then dispose of it cleanly
    // if cameraId comes back false, out var process is null
    public void Stop(Guid cameraId)
    {
        if (_recordings.TryRemove(cameraId, out var process))
        {
            //simulate hitting q - shuts down the process clean
            process.StandardInput.Write("q");
            
            process.WaitForExit(5000);
            process.Dispose();
        }
    }
}