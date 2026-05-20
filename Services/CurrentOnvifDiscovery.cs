using System.ServiceModel;
using System.Xml;
using Onvif.Core.Client;
using Onvif.Core.Client.Common;
using Rtsp.Sdp;
using Onvif.Core.Client.Media;
using OnvifDiscovery;
using DateTime = System.DateTime;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using VideoRecorder.Camera;

namespace OnvifDiscovery;

public class CurrentOnvifDiscovery
{
    public async IAsyncEnumerable<Camera> DiscoverAsync(
        string username,
        string password,
        int timeoutSeconds)
    
    {
        var discovery = new Discovery();
        var cts = new CancellationTokenSource();
        Console.WriteLine("Discovering Onvif...");

        await foreach (var device in discovery.DiscoverAsync(timeoutSeconds, cts.Token))
        {
            Camera? info = null;
            
                //create the Media client. this includes SOAP binding,
                //gets the media service URL by requesting cams capabilities
                //create the end point
                var media = await OnvifClientFactory.CreateMediaClientAsync(device.Address, username, password);
                
                //ask cam for list of stream profiles
                var profilesResponse = await media.GetProfilesAsync();
                Console.WriteLine($"DEVICE FOUND: {device.Address}");
                
                //grab the token - an id you pass to GetStreamUri - "give me the URL for this stream"
                string token = profilesResponse.Profiles[0].token;
                
                //set up the stream object with these params
                var streamSetup = new StreamSetup
                {
                    Stream = StreamType.RTPUnicast,
                    Transport = new Transport { Protocol = TransportProtocol.RTSP }
                };
                //
                var streamUri = await media.GetStreamUriAsync(streamSetup, token);

                info = new Camera
                {
                    Manufacturer = device.Mfr,
                    Model = device.Model,
                    Host = device.Address,
                    XAddress = device.XAddresses.FirstOrDefault()?.ToString(),
                    RtspUrl = streamUri.Uri,
                    CreatedAt = DateTime.Now,
                };
            
            if (info != null)
                yield return info;
            else
                Console.WriteLine("No Onvif camera found");
            
            
            
        }

        Console.WriteLine("Loop finished");
    }
}