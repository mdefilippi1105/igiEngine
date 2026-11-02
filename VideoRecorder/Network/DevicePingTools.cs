using System.Buffers;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace VideoRecorder.Network;
using System;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using System.Text;
using System.Xml;

/*******************************************************************************************
 * This class holds various network tools
 ********************************************************************************************/
public class DevicePingTools
{
    private readonly string _hostName = Dns.GetHostName();
    private IPAddress[] _addresses = Dns.GetHostAddresses(Dns.GetHostName());
    private string? ip;


    /*******************************************************************************************
     * CheckPort() - use a TCP client to check if a port is open
     ********************************************************************************************/
    private static async Task<bool> CheckPort(string ip, int port)
    {
        using var tcp = new TcpClient();
        using var cancelTokenSource = new CancellationTokenSource(500);
        try
        {
            await tcp.ConnectAsync(ip, port, cancelTokenSource.Token);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /*******************************************************************************************
     * RunPing() - run a generic ping
     ********************************************************************************************/
    //ping the retrieved ip address
    public bool RunPing(string address)
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
            Console.WriteLine($"Time to live: {reply.Options!.Ttl}");
            Console.WriteLine($"Status Code: {reply.Status}");
            Console.WriteLine("\n");
        }

        return true;
    }

    /*******************************************************************************************
     * PingOneDevice() - single ping to single address wrapped in a try/catch
     ********************************************************************************************/
    private static async Task<string?> PingOneDevice(string ip)
    {
        try
        {
            using var ping = new Ping();
            var reply = await ping.SendPingAsync(ip, 2000);
            var hostName = await GetHostnameAsync(reply.Address);

            if (reply.Status == IPStatus.Success)
                // builds 1 line per device or null for no answer
                return $"{ip} : {hostName} - {reply.RoundtripTime} ms";
        }
        catch (PingException e)
        {
            Console.WriteLine(e.Message);
        }

        if (await CheckPort(ip, 80) || await CheckPort(ip, 554))
            return $"{ip} no ping reply, but port 80 or 554 is open.";

        return null;
    }

    /*******************************************************************************************
     * ScanSubnet() - run PingOne as a list of tasks
     * The old way ran a ping sequentially in a for loop
     * We can blast a bunch at once, wait for completion, then display
     ********************************************************************************************/
    public async Task<List<string>> ScanSubnet(string hostName)
    {
        string[] parts = hostName.Split('.');
        var subnet = string.Join(".", parts[0], parts[1], parts[2]); // eg 255.255.255.0
        var goodPing = new List<string>();
        var pingTasks = new List<Task<string?>>();

        for (int i = 1; i <= 254; i++)
            pingTasks.Add(PingOneDevice($"{subnet}.{i}"));

        string?[] results = await Task.WhenAll(pingTasks);

        foreach (var result in results)
            if (result != null)
                goodPing.Add(result);

        return goodPing;
    }








    /*
     *
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

        NetworkInterface result = null;
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

    /*******************************************************************************************
     * GetHostnameAsync() - get the dns name of the device
     ********************************************************************************************/
    static async Task<string?> GetHostnameAsync(IPAddress address)
    {
        try
        {
            return (await Dns.GetHostEntryAsync(address)).HostName;
        }
        catch (Exception e)
        {
            return null;
        }
    }
}