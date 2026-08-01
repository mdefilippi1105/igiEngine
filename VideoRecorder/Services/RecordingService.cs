using System.Collections.Concurrent;
using System.Diagnostics;
using VideoRecorder.Models;

namespace VideoRecorder.Services;

public class RecordingService
{
    //  the id is the key, the running Process is the value
    // concurrent with multiple requests hitting it 
    private readonly ConcurrentDictionary<Guid, Process> _recordings = new();
    
    
    /*************************************************
     * ffmpeg args explained:
     * 1. TCP instead of UDP, copy codec its already using (H264)
     * 2. segment output - series of files vs 1 big file
     * new segment every 300 seconds
     * 3. set each timestamp to start at 0 instead of continuing from the stream
     * 4. pass mp4 options into each segment. write a fragmented mp4
     * so files stay playable even if the service dies mid-segment.
     * 5. {dir}/date and time format
     ***************************************************/
    public void Start(Guid cameraId, string cameraUrl)
    {
        // guard - if cam in dictionary, return
        if (_recordings.ContainsKey(cameraId))
            return;
        
        // $@ keeps the slashes verbatim and string interpolation
        var recordingDirectory = $@"/Users/michaeldefilippi/rec-test/{cameraId}";
        Directory.CreateDirectory(recordingDirectory);
        

        var recordProcess = new Process();
        recordProcess.StartInfo.FileName = "/Users/michaeldefilippi/RiderProjects/VideoRecorder/VideoRecorder/ThirdParty/ffmpeg";
        recordProcess.StartInfo.Arguments = $"-rtsp_transport tcp -i \"{cameraUrl}\" " +
                                            $"-c copy " +
                                            $"-f segment -segment_time 300 " +
                                            $"-reset_timestamps 1 -strftime 1 " +
                                            $"-segment_format_options movflags=+frag_keyframe+empty_moov " +
                                            $"\"{recordingDirectory}/%Y%m%d_%H%M%S.mp4\"";
        
        //c# commands are now the keyboard instead of terminal keyboard
        recordProcess.StartInfo.RedirectStandardInput = true;
        
        //direct method, no shell needed
        recordProcess.StartInfo.UseShellExecute = false;
        
        //suppresses a blank cmd window from popping up - for Windows OS only
        recordProcess.StartInfo.CreateNoWindow = true;
        
        // start the recordings
        recordProcess.Start();
        
        // lastly we add the id and the process to the dictionary
        _recordings.TryAdd(cameraId, recordProcess);
    }
    
    
    // we pull the reference out of the dictionary, to kill
    // the process and then dispose of it cleanly
    // if cameraId comes back false, out var process is null
    public void Stop(Guid cameraId)
    {
        if (!_recordings.TryRemove(cameraId, out var process))
            return;
        try
        {
            if (!process.HasExited)
            {
                //simulate hitting q - shuts down the process clean
                process.StandardInput.Write("q");
                //flush out the buffer
                process.StandardInput.Flush();

                if (!process.WaitForExit(1000))
                    process.Kill(entireProcessTree: true);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
        finally
        {
            process.Dispose();
        }
        
    }
    
    // allow recording if the user enabled recording button, the cam is online,
    // and the camera is enabled.
    public void RecordingAuthorize(Camera camera)
    {
        var recordingAllowed = camera.UserToggledRecording && camera.IsOnline && camera.IsEnabled;

        if (recordingAllowed && !camera.IsRecording)
        {
            var url = $"rtsp://{camera!.Username}:{camera.Password}@{camera.Host}{camera.Path}";
            Start(camera.Id, url);
            
            camera.IsRecording = true;
        }
        else if (!recordingAllowed && camera.IsRecording)
        {
            Stop(camera.Id);
            camera.IsRecording = false;
        }
    }
    
    
    
    
    
    
    
    
    
    
    
}