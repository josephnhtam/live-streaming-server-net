using LiveStreamingServer.Newtorking.Contracts;
using LiveStreamingServer.Rtmp.Core.Utilities;
using LiveStreamingServer.Utilities.Extensions;
using System.Security.Cryptography;

namespace LiveStreamingServer.Rtmp.Core.RtmpMessageHandler.Handshakes
{
    public enum ComplexHandshakeType
    {
        Schema0,
        Schema1
    }

    // todo: refactor and optimize
    public class ComplexHandshake
    {
        private const byte ClientType = 3;
        private static readonly byte[] Version = [4, 5, 0, 1];

        private readonly INetBuffer _incomingBuffer;
        private readonly ComplexHandshakeType _type;

        public ComplexHandshake(INetBuffer incomingBuffer, ComplexHandshakeType type)
        {
            _incomingBuffer = incomingBuffer;
            _type = type;
        }

        public bool ValidateC1()
        {
            var digestDataIndex = DigestBlock.GetDigestDataIndex(_incomingBuffer, _type);
            var providedDigestData = DigestBlock.GetDigestData(_incomingBuffer, digestDataIndex);
            var computedDigestData = DigestBlock.ComputeDigestData(_incomingBuffer, digestDataIndex);

            return providedDigestData.SequenceEqual(computedDigestData);
        }

        public void WriteS0S1S2(INetBuffer outgoingBuffer)
        {
            WriteS0(outgoingBuffer);
            WriteS1(outgoingBuffer);
            WriteS2(outgoingBuffer);
        }

        public void WriteS0(INetBuffer outgoingBuffer)
        {
            outgoingBuffer.Write(ClientType);
        }

        public void WriteS1(INetBuffer outgoingBuffer)
        {
            int initialPosition = outgoingBuffer.Position;

            outgoingBuffer.Write(HandshakeUtilities.GetTime());
            outgoingBuffer.Write(Version, 0, 4);
            outgoingBuffer.WriteRandomBytes(1536 - 8);

            var c1KeyIndex = KeyBlock.GetKeyIndex(_incomingBuffer, _type);
            var c1Key = KeyBlock.GetProvidedKeyData(_incomingBuffer, c1KeyIndex);
            var s1Key = ComputeSharedKey(c1Key);
            outgoingBuffer.MoveTo(initialPosition + c1KeyIndex).Write(s1Key, 0, 128);

            var c1DigestDataIndex = DigestBlock.GetDigestDataIndex(_incomingBuffer, _type);
            var c1JoinedBytes = DigestBlock.GetJoinedBytes(_incomingBuffer, c1DigestDataIndex);
            var s1DigestData = c1JoinedBytes.CalculateHmacSha256(HandshakeConstants.FMSKey);
            outgoingBuffer.MoveTo(initialPosition + c1DigestDataIndex).Write(s1DigestData, 0, 32);

            outgoingBuffer.MoveTo(outgoingBuffer.Size);
        }

        public void WriteS2(INetBuffer outgoingBuffer)
        {
            outgoingBuffer.WriteRandomBytes(1536 - 32);

            var c1DigestDataIndex = DigestBlock.GetDigestDataIndex(_incomingBuffer, _type);
            var c1DigestData = DigestBlock.GetDigestData(_incomingBuffer, c1DigestDataIndex);

            var s2RandomData = new byte[32];
            RandomNumberGenerator.Fill(s2RandomData);

            var tempKey = c1DigestData.CalculateHmacSha256(HandshakeConstants.FMSKey);
            var s2DigestData = s2RandomData.CalculateHmacSha256(tempKey);

            outgoingBuffer.Write(s2DigestData, 0, 32);
        }

        private static byte[] ComputeSharedKey(byte[] peerPublicKey)
        {
            return peerPublicKey;
        }

        internal static class DigestBlock
        {
            public static byte[] GetDigestData(INetBuffer netBuffer, int digestDataIndex)
            {
                return netBuffer.MoveTo(digestDataIndex).ReadBytes(32);
            }

            public static byte[] ComputeDigestData(INetBuffer netBuffer, int digestDataIndex)
            {
                var joinedBytes = GetJoinedBytes(netBuffer, digestDataIndex);
                return joinedBytes.CalculateHmacSha256(HandshakeConstants.FPKey);
            }

            public static byte[] GetJoinedBytes(INetBuffer netBuffer, int digestDataIndex)
            {
                byte[] joinedBytes = new byte[1536 - 32];
                netBuffer.MoveTo(0).ReadBytes(joinedBytes, 0, digestDataIndex);
                netBuffer.MoveTo(digestDataIndex + 32).ReadBytes(joinedBytes, digestDataIndex, 1536 - 32 - digestDataIndex);
                return joinedBytes;
            }

            public static int GetDigestDataIndex(INetBuffer netBuffer, ComplexHandshakeType type)
            {
                return type switch
                {
                    ComplexHandshakeType.Schema0 => GetDigestOffset(netBuffer, type) + 8 + 764 + 4,
                    ComplexHandshakeType.Schema1 => GetDigestOffset(netBuffer, type) + 8 + 4,
                    _ => throw new ArgumentOutOfRangeException(nameof(type))
                };
            }

            public static int GetDigestOffset(INetBuffer netBuffer, ComplexHandshakeType type)
            {
                const int MaxKeyOffset = 764 - 32 - 4;

                netBuffer.Position = type switch
                {
                    ComplexHandshakeType.Schema0 => 8 + 764,
                    ComplexHandshakeType.Schema1 => 8,
                    _ => throw new ArgumentOutOfRangeException(nameof(type))
                };

                return (netBuffer.ReadByte() + netBuffer.ReadByte() + netBuffer.ReadByte() + netBuffer.ReadByte()) % MaxKeyOffset;
            }
        }

        internal static class KeyBlock
        {
            public static byte[] GetProvidedKeyData(INetBuffer netBuffer, int keyDataIndex)
            {
                return netBuffer.MoveTo(keyDataIndex).ReadBytes(128);
            }

            public static int GetKeyIndex(INetBuffer netBuffer, ComplexHandshakeType type)
            {
                return type switch
                {
                    ComplexHandshakeType.Schema0 => GetKeyOffset(netBuffer, type) + 8,
                    ComplexHandshakeType.Schema1 => GetKeyOffset(netBuffer, type) + 8 + 764,
                    _ => throw new ArgumentOutOfRangeException(nameof(type))
                };
            }

            public static int GetKeyOffset(INetBuffer netBuffer, ComplexHandshakeType type)
            {
                const int MaxKeyOffset = 764 - 128 - 4;

                netBuffer.Position = type switch
                {
                    ComplexHandshakeType.Schema0 => 8 + 764 - 4,
                    ComplexHandshakeType.Schema1 => 1536 - 4,
                    _ => throw new ArgumentOutOfRangeException(nameof(type))
                };

                return (netBuffer.ReadByte() + netBuffer.ReadByte() + netBuffer.ReadByte() + netBuffer.ReadByte()) % MaxKeyOffset;
            }
        }
    }
}
