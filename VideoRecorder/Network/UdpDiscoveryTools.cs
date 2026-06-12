
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace VideoRecorder.Network;

public class UdpDiscoveryTools {
    private static UdpClient _socket;
    private static int _localPort;

     private static int Start()
     {
         
        _socket = new UdpClient(0);
        
        if (_socket.Client.LocalEndPoint is IPEndPoint endPoint)
        {
            _localPort = endPoint.Port;
        }
        
        return _localPort;
    }

    public static void Stop() 
    {
        
        // make sure actually points to a socket and make sure socket not already closed
        if (_socket != null)
        {
            Thread.Sleep(5000);
            _socket.Close();
        }
    }


    /************************************************************************
     *  Udp Discovery - start() creates the udp socket and returns
     * listening port number. the probe is a WS-Discovery message
     * XML that requests network video devices
     ************************************************************************/
    
    public static void SendDiscovery()
    {
        
        int port = Start();

        const string probe = """
                            <?xml version=""1.0"" encoding=""utf-8""?>
                            <Envelope xmlns=""http://www.w3.org/2003/05/soap-envelope"">
                                <Body>
                                    <Probe xmlns=""http://schemas.xmlsoap.org/ws/2005/04/discovery"">
                                        <Types>dn:NetworkVideoTransmitter</Types>
                                    </Probe>
                                </Body>
                            </Envelope>
                            """;
        
        byte[] data = Encoding.UTF8.GetBytes(probe);
        Console.WriteLine($"Sending UDP ON PORT: {port}");
        
        // we are sending 5 discovery packets. we can change this later if need be.
        for (int i = 0; i < 10; i++)
        {
            _socket.Send(data, data.Length, "239.255.255.250", 3702);
            Thread.Sleep(500);
        }
    }

    public static List<string> ReceiveResponse()
    {
        var results = new List<string>();
        _socket.Client.ReceiveTimeout = 10000; // this is 5 seconds, it may have to be adjusted
        try
        {
            while (true)
            {
                var endpoint = new IPEndPoint(IPAddress.Any, 0);
                
                //this does 2 things: block until udp packet arrives
                //then, write the senders ip into endpoint
                _socket.Receive(ref endpoint);
                
                var ipString = endpoint.Address.ToString();
                
                //if the list has the string already don't re-add as a duplicate
                if (!results.Contains(ipString))
                {
                    Console.WriteLine(ipString.Length + " : " + ipString);
                    
                    results.Add(ipString);
                }
            }
        }
        catch (SocketException e)
        {
            Console.WriteLine(e.Message);
        }
        return results;
    }
}
