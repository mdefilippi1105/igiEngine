namespace VideoRecorder.Services;

// I took this from my Android app. Instead of throwing console.writeln()
// everywhere..try to gather them up in a centralized location.
public static class AppLogger
{
    private static List<string> _lines = new(); // every log line in order
     private const int MaxEvents = 500;
 
     public static void WriteLog(string msg)
     {
         lock (_lines)
         {
             _lines.Add($"{DateTime.Now:HH:mm:ss} {msg}"); // keep a copy with timestamp
             if (_lines.Count > MaxEvents)
                 _lines.RemoveAt(0); // drop the oldest line
         } 
     }
 
     public static string RetrieveLogs()
     {
         string all = string.Join("\n", AppLogger._lines);
         return all;
     }
    

    
 }