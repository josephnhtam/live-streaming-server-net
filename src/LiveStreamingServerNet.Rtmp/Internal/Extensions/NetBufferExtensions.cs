using LiveStreamingServerNet.Networking.Contracts;
using LiveStreamingServerNet.Rtmp.Internal.Utilities;
using mtanksl.ActionMessageFormat;

namespace LiveStreamingServerNet.Rtmp.Internal.Extensions
{
    internal static class NetBufferExtensions
    {
        public static void WriteAmf(this INetBuffer buffer, IReadOnlyList<object?> values, AmfEncodingType type)
        {
            var writer = new AmfWriter();

            foreach (var value in values)
            {
                if (value is AmfArray array)
                {
                    if (type == AmfEncodingType.Amf0)
                        writer.WriteAmf0(array.Map);
                    else
                        writer.WriteAmf3(array.Map);

                    continue;
                }

                if (value is IEnumerable<KeyValuePair<string, object?>> map)
                {
                    if (type == AmfEncodingType.Amf0)
                        writer.WriteAmf0ObjectWithType(map);
                    else
                        writer.WriteAmf3ObjectWithType(map);

                    continue;
                }

                if (type == AmfEncodingType.Amf0)
                    writer.WriteAmf0(value);
                else
                    writer.WriteAmf3(value);
            }

            buffer.Write(writer.Data);
        }

        public static object[] ReadAmf(this INetBuffer buffer, int bytesCount, int valuesCount, AmfEncodingType type)
        {
            var data = buffer.ReadBytes(bytesCount);
            var reader = new AmfReader(data);
            var results = new object[valuesCount];

            try
            {
                for (int i = 0; i < valuesCount; i++)
                {
                    var parameter = type == AmfEncodingType.Amf3 ? reader.ReadAmf3() : reader.ReadAmf0();

                    if (parameter is IAmfObject amfObject)
                        parameter = amfObject.ToObject();

                    results[i] = parameter;
                }
            }
            catch (IndexOutOfRangeException) { }

            return results;
        }

        public static IList<object> ReadAmf(this INetBuffer buffer, int bytesCount, AmfEncodingType type)
        {
            var data = buffer.ReadBytes(bytesCount);
            var reader = new AmfReader(data);
            var results = new List<object>();

            try
            {
                while (true)
                {
                    var parameter = type == AmfEncodingType.Amf3 ? reader.ReadAmf3() : reader.ReadAmf0();

                    if (parameter is IAmfObject amfObject)
                        parameter = amfObject.ToObject();

                    results.Add(parameter);
                }
            }
            catch (IndexOutOfRangeException) { }

            return results;
        }
    }

    internal enum AmfEncodingType
    {
        Amf0,
        Amf3
    }
}
