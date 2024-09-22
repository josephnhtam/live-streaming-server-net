using LiveStreamingServerNet.Rtmp.Internal;
using LiveStreamingServerNet.Rtmp.Internal.Extensions;
using LiveStreamingServerNet.Rtmp.Client.Internal.Services.Contracts;

namespace LiveStreamingServerNet.Rtmp.Client.Internal.Services.Extensions
{
    internal static class RtmpCommandMessageSenderServiceExtensions
    {
        //public static void SendConnectCommandMessage(
        //    this IRtmpCommandMessageSenderService sender,
        //    string appName,
        //    AmfEncodingType amfEncodingType = AmfEncodingType.Amf0,
        //    Action<bool>? callback = null)
        //{
        //    var properties = new Dictionary<string, object?>
        //    {
        //        { RtmpArgumentNames.Level, level },
        //        { RtmpArgumentNames.Code, code },
        //        { RtmpArgumentNames.Description, description }
        //    };

        //    sender.SendCommandMessage(3, "connect", 0, null, new List<object?> { properties }, amfEncodingType, callback);
        //}
    }
}
