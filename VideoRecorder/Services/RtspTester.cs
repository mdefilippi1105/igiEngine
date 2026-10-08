using System.Configuration;
using System.Net.Sockets;
using System.Text;

namespace VideoRecorder.Services;

public class RtspTester
{
    public static async Task<int> DescribeAsync(string host, int port, string path)
    {
        string url = $"rtsp://{host}:{port}{path}";
        Console.WriteLine($"RTSP URL: {url}");

        // start a 5-second timer that cancels anything it's passed to
        using var cancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        using var tcp = new TcpClient();
        await tcp.ConnectAsync(host, port, cancellationToken.Token); // give up if the timer fires

        var stream = tcp.GetStream();
        var reader = new StreamReader(stream, Encoding.ASCII);
        
        string request =
            $"DESCRIBE {url} RTSP/1.0\r\n" + //what we want, for which URL and which protocol
            "CSeq: 1\r\n" + //sequence number RTSP requires
            "Accept: application/sdp\r\n" + //we want the stream description in SDP
            "\r\n"; // blank line...we're done
        
        // convert this to bytes and send it 
        await stream.WriteAsync(Encoding.ASCII.GetBytes(request), cancellationToken.Token);
        
        // read the first line reply
        string? statusLine = await reader.ReadLineAsync(cancellationToken.Token);
        if (statusLine == null) return 0; // handle cam hanging up without replying
        
        // splits on spaces and returns middle piece as a number
        return int.Parse(statusLine.Split(' ')[1]); 
    }

            
}