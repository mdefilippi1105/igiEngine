
using Rtsp;
using Rtsp.Messages;

namespace VideoRecorder.Services;

using Rtsp;
using Rtsp.Messages;


public class StreamListener : IDisposable
{
    // these are nullable by default
    private RtspTcpTransport? _tcpSocket;
    private RtspListener? _rtspListener;
    private string? _url;
    private string? _username;
    private string? _password;
    private bool _attemptedAuth = false;
    private bool _notNull;
    private bool _setupSent = false;
    private string? _sessionId;
    private string? _authHeader;
    
    /************************************************************
     * This ties all of these together.
     * At some point, if all testing is passed, I will try
     * to implement this on igi engine.
     * 
     ************************************************************/
    public void RunStreaming(string target, string username, string password)
    {
        ClientConnect(target, username, password);
        StartListening();
        SendOptions();
        SendDescription();
        Console.ReadLine();

    }
    
    /*******************************************************************************
     * create a new socket that we connect to
     * this would be considered the rtsp server
     * in this case, it would be an ip cam
     * IMPORTANT: Some Rtsp url strings don't contain a port
     * for new Uri() to parse cleanly, you need a full URI string in this format:
     * rtsp://username:password@192.168.1.202:554/axis-media/media.amp
     ******************************************************************************/
    void ClientConnect(string url, string username, string password)
    {
        Dispose();
        
        _username = username;
        _password = password;
        
        // create a URI object with the supplied URL
        var uri = new Uri(url);
        int port;
        if (uri.Port == -1) // check if the URL has no explicit port specified
            port = 554;
        else
            port = uri.Port;
        
        //test
        Console.WriteLine(uri.AbsolutePath);
        Console.WriteLine(uri.AbsoluteUri);
        Console.WriteLine(uri.Host);
        Console.WriteLine(uri.Scheme);
        Console.WriteLine(uri.UserInfo);
        Console.WriteLine(uri.Port.ToString());
        
        
        var builder = new UriBuilder(uri);
        builder.Port = port;
        
        var uriWithPort = builder.Uri;
        _url = uriWithPort.ToString();
        
        // Creates a standard socket
        // This just opens the pipe. 
        _tcpSocket = new RtspTcpTransport(uriWithPort);

       if (!_tcpSocket.Connected)
       {
           Console.WriteLine("Client not connected");
           return;
       }
       Console.WriteLine("Client connected");
    }
    
    
    /*********************************************************
     * create the rtsp listener and attach it to the tcp socket
     * from ClientConnect().
     *
     **********************************************************/
    private void StartListening()
    {
        if (_tcpSocket == null) 
            return;
        _rtspListener = new RtspListener(_tcpSocket); // this is the object that will handle all read/write rtsp msg
        _rtspListener.MessageReceived += OnMessageReceived; //event handler
        _rtspListener.Start(); // start background thread

        Console.WriteLine("Listener started");
    }
    
    /*********************************************************
     * this method is called when cam sends back an RTSP response
     * here, we read the cameras reply and decide what to do next
     * will add more switch parameters and response codes
     **********************************************************/
    void OnMessageReceived(object? sender, RtspChunkEventArgs e)
    {
        if (e.Message is not RtspResponse response) // make sure what we get back is an actual response
            return;

        Console.WriteLine($"Response: {response.ReturnCode} to {response.OriginalRequest}"); //may want to comment out the second half

        switch (response.ReturnCode)
        {
            case 401:
                Console.WriteLine("You are not authorized to use this command");
                HandleUnauthorize(response);
                Console.WriteLine(response.Headers["WWW-Authenticate"]);
                return;
            case 200:
                HandleOk(response);
                return;
        }
    }

    /**********************************************************
     * this is for when the camera sends back a 200 - OK
     * switch -> if OK -> Send DESCRIBE
     * if OK to DESCRIBE -> Print SDP (stream info)
     * SDP is basically a big string that lists different
     * configurations to be used.
     **********************************************************/
    void HandleOk(RtspResponse response)
    {
        _attemptedAuth = false;
        switch (response.OriginalRequest)
        {
            case RtspRequestOptions:
                SendDescription();
                break;
            case RtspRequestDescribe:
                if (_setupSent) //quick check to prevent sending double describes
                    break;
                _setupSent = true;
                Console.WriteLine("I got sdp:");
                
                //convert the raw cam bytes into a readable string
                string sdp = System.Text.Encoding.UTF8.GetString(response.Data.ToArray());
                Console.WriteLine(sdp);

                string? controlUrl = null;
                bool inVideoSection = false;
                
                // Split the chunk of text by line. The SDP has two "a=control:" lines
                foreach (var line in sdp.Split('\n'))
                {
                    if (line.StartsWith("m=video"))
                    {
                        inVideoSection = true;
                        continue;
                    }
                    // if (line.StartsWith("a=control:") && line.Contains("stream="))
                    if (inVideoSection && line.StartsWith("a=control:")) 
                    {
                        //yank out the "a=control:" part and remove whitespace via Trim(). we need a clean URL to send below.
                        controlUrl = line.Replace("a=control:", "").Trim();
                        break; 
                    }
                    
                }
                
                // leave alone if it already has URL & remove the trailing slash 
                if (controlUrl != null && !controlUrl.StartsWith("rtsp://")) 
                    controlUrl = _url.TrimEnd('/') + "/" + controlUrl;
                Console.WriteLine($"Control: {controlUrl}");
                
                // if when parsing the SDP we find a control URL...
                if (response.Data.Length > 0)
                    _notNull = true;
                
                if (controlUrl != null)
                {
                    SendSetup(controlUrl); // send in here
                }
                break;
            
            case RtspRequestSetup:
                
                Console.WriteLine("I got setup");
                _sessionId = response.Headers["Session"];
                Console.WriteLine($"Session ID: {_sessionId}");
                SendPlay();
                break;
        }
    }
    
    
    /**********************************************************
     * Handling unauthorized events
     * 
     **********************************************************/
    private void HandleUnauthorize(RtspResponse response)
    {
        if (_attemptedAuth) // this is false by default
        {
            Console.WriteLine("Auth failed - please check username and password");
            _attemptedAuth = false;
            return;
        }
        _attemptedAuth = true;
        
        if (response.OriginalRequest == null)
            return;
        
        // make a copy of original request. we do not want to modify the original,
        // and need a fresh object to add the auth header to.
        var retry = response.OriginalRequest.Clone() as RtspRequest;
        if (retry == null)
            return;
        
        //read the WWW-Authenticate header from the 401 response
        string? wwwAuth = response.Headers["WWW-Authenticate"];
        
        // this is for DIGEST AUTH
        if (wwwAuth != null && wwwAuth.StartsWith("Digest"))
        {
            //pull out the realm and nonce values
            string realm = ParseValue(wwwAuth, "realm");
            string nonce = ParseValue(wwwAuth, "nonce");
            
            //get the rtsp method of the original request.
            string method = response.OriginalRequest.RequestTyped.ToString().ToUpper();

            //call the DigestAuth class and slap the result in the header
            retry.Headers["Authorization"] = DigestAuth.BuildHeader(
                _username!, _password!, realm, nonce, method, retry.RtspUri!.ToString());
        }
        else
        {
            // building a basic auth header. we need to convert user/pass
            // into array of bytes. we then convert the string of numbers
            // into Base64 string. this is safe for a HTTP header
            var creds = Convert.ToBase64String(
                System.Text.Encoding.UTF8.GetBytes($"{_username}:{_password}"));
        
            retry.Headers["Authorization"] = _authHeader =  $"Basic {creds}"; // add the headers
        }
        _rtspListener!.SendMessage(retry); // takes care of the rest aka convert to bytes and send down the line
    }

    private string ParseValue(string header, string key)
    {
        string search = $"{key}=\"";
        int start = header.IndexOf(search) + search.Length;
        int end = header.IndexOf("\"", start);
        return header.Substring(start, end - start);
    }
    
    
    /**********************************************************
     * Send OPTIONS
     * creates rtspOptions request object. asks camera what
     * RTSP options are supported.
     **********************************************************/
    private void SendOptions()
    { 
        if (_rtspListener == null)
            return;

        var options = new RtspRequestOptions();
        options.RtspUri = new Uri(_url!);
        _rtspListener.SendMessage(options);

        Console.WriteLine($"Sent message: {_url}");
    }
    
    /**********************************************************
     * Send DESCRIPTION
     **********************************************************/
    private void SendDescription()
    {
        if (_rtspListener == null)
            return;
        var describe = new RtspRequestDescribe();
        describe.RtspUri  = new Uri(_url!);
        _rtspListener.SendMessage(describe);
        
        Console.WriteLine($"Sent description: {_url}");
    }
    
    /**********************************************************
     * Send SETUP
     **********************************************************/
    private void SendSetup(string controlUrl)
    {
        {
            if (_rtspListener == null)
                return;
            var setup = new RtspRequestSetup();
            setup.RtspUri = new Uri(controlUrl);
            Console.WriteLine($"setup: {setup.RtspUri}");
        
            // very important. tells the camera how you want to receive the video
            // rtp/avp/tcp - send video over same tcp connection; unicast - no broadcast
            // interleaved01 - video channel 0, control signals channel 1
            setup.Headers["Transport"] = "RTP/AVP/TCP;unicast;interleaved=0-1";
            // setup.AddTransport(new );
            
            if (_authHeader != null)
                setup.Headers["Authorization"] = _authHeader;
        
            _rtspListener.SendMessage(setup);
        
            Console.WriteLine($"Sent setup: {setup.RtspUri}");
        }
    }
    
    /**********************************************************
     * Send PLAY
     **********************************************************/
    private void SendPlay()
    {
        if (_rtspListener == null)
            return;
        var play = new RtspRequestPlay();
        play.RtspUri = new Uri(_url!);
        
        play.Headers["Session"] = _sessionId!;

        // play.Headers["Authorization"] = _authHeader;
        _rtspListener.SendMessage(play);
        Console.WriteLine($"Sent play: {_url}");
    }
    
    /**********************************************************
     * Close down the session
     **********************************************************/
    public void Dispose()
    {
        if (_tcpSocket == null)
            return;
        try
        {
            _tcpSocket.Close();
            GC.SuppressFinalize(this);
            
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
        finally
        {
            _tcpSocket = null;
        }
    }
    
} 