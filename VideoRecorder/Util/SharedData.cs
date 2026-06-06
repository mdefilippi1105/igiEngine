using System.Collections;
using System.Collections.Concurrent;
using VideoRecorder.Services;

namespace VideoRecorder.Util;

public class SharedData
{
    // just stores the camera's guid as a plain string 
    public static readonly ConcurrentDictionary<string, string> ActiveStreams =  new();
    
    // store streaming object itself
    public static ConcurrentDictionary<string, StreamVideo> StreamObjects = new();
    
    public static int StreamCount = 0;

    public static string ListStreams()
    {
        string data = "";
        
        foreach (KeyValuePair <string, string> kvp in ActiveStreams)
        {
          data  += "Stream List" + kvp.Key + ": " + kvp.Value +"\n";
            
        }
        return data;
    }
    
    public static string ListStreamObjects()
    {
        string data = "";
        
        foreach (KeyValuePair <string, StreamVideo> kvp in StreamObjects)
        {
            data  += "Stream List" + kvp.Key + ": " + kvp.Value +"\n";
            
        }
        return data;
    } 
}