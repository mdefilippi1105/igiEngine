using System.Buffers;
using System.Diagnostics;
using System.Net;
using System.Text;

namespace VideoRecorder.Network;
using System;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using System.Text;
using System.Xml;


public class DevicePingTools
{
    private readonly string _hostName = Dns.GetHostName();
    private IPAddress[] _addresses = Dns.GetHostAddresses(Dns.GetHostName());
    private string? ip;
    
    //ping the retrieved ip address
    public void RunPing(string address)
    {
        
        var ping = new Ping();
        var options = new PingOptions();
        // use default 128 ttl
        options.DontFragment = true;

        var timeout = 500;
        byte[] buffer = Encoding.ASCII.GetBytes(address);
        PingReply reply = ping.Send(address, timeout, buffer, options);
        if (reply.Status == IPStatus.Success)
        {
            Console.WriteLine($"{reply.RoundtripTime} ms");
            Console.WriteLine($"Requested Address: {reply.Address} ");
            Console.WriteLine($"Time to live {0}", reply.Options.Ttl);
            Console.WriteLine($"Status Code: {reply.Status}");
        }
    }
    
    public async Task <List<string>> ScanSubnet(string hostName)
    {
        string[] parts = hostName.Split('.');
        var subnet = string.Join(".", parts[0], parts[1], parts[2]); // eg 255.255.255.0
        
        var goodPing = new List<string>();

        for (int i = 0; i <= 254; i++)
        {
            var ip = $"{subnet}.{i}";
            try
            {
                var reply = await new Ping().SendPingAsync(ip, 600);
                if (reply.Status == IPStatus.Success)
                {
                    Console.WriteLine($"Alive: {ip}");
                    goodPing.Add($"{ip} is currently online");
                }
                else
                {
                    Console.WriteLine($"Failed: {ip}");
                }
            }
            catch (PingException e)
            {
                Console.WriteLine(e.ToString());
            }

        }
        Console.WriteLine($"Current amount of pings is {goodPing.Count}.");
        
        return goodPing;
    }
    
    
    
    
    /*
     * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
     * how this works :
     * start with empty string, loop through 
     * every network interface on the machine
     * AA - BB - CC - DD - EE - FF
     * ↑    ↑    ↑    ↑    ↑    ↑
     * byte byte byte byte byte byte
     * 1    2    3    4    5    6
     * if it's not the last byte, add a " : "
     * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
     */
    
    public static String GetInterfaces()
    {
        NetworkInterface[] interfaces = NetworkInterface.GetAllNetworkInterfaces();
        // create a byte array called mac address

        NetworkInterface result  = null;
        foreach (NetworkInterface iface in interfaces)
        {
            if (iface.OperationalStatus == OperationalStatus.Up &&
                iface.NetworkInterfaceType != NetworkInterfaceType.Loopback)
            {
                result = iface;
                break;
            }
        }
        
        byte[]? mac = result.GetPhysicalAddress().GetAddressBytes();
        if (mac != null) 
        {
            StringBuilder macAddress = new StringBuilder();
            for (int i = 0; i < mac!.Length; i++) 
            {
                macAddress.Append(String.Format("{0:x2}", mac[i]));
                
                if (i < mac.Length - 1) 
                {
                    macAddress.Append(":");
                }
            }
            Console.WriteLine(macAddress);
            return macAddress.ToString();
        } 
        return "";
    }
    
    public static List<string> AddressResolution()
    {
        var proc = new Process();
        proc.StartInfo.FileName = "arp";
        proc.StartInfo.Arguments = "-a";
        proc.StartInfo.UseShellExecute = false;
        proc.StartInfo.RedirectStandardOutput = true;
        proc.StartInfo.RedirectStandardError = true;
        proc.Start();
        var output = proc.StandardOutput.ReadToEnd();
        
        var items = new List<string>();
        foreach (var item in output.Split('\n'))
        {
            if (item.Trim().Length > 0)
            {
                items.Add(item.Trim());
            }
        }
        return items;
    }
}