namespace VideoRecorder.Util;

public static class ManufacturerTable
{
    public static readonly Dictionary<string, string> DefaultRtspPaths = new()
    {
        { "axis", "/axis-media/media.amp" },
        { "vivotek", "/live.sdp" },
        { "networkcamera", "/live.sdp" },
        { "hikvision", "/Streaming/Channels/101" },
        { "dahua", "/cam/realmonitor?channel=1&subtype=0" },
    };
}