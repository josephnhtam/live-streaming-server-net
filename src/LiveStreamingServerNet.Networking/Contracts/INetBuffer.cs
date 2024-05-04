namespace LiveStreamingServerNet.Networking.Contracts
{
    public interface INetBuffer : IDisposable
    {
        int Position { get; set; }
        int Size { get; set; }

        byte[] UnderlyingBuffer { get; }

        INetBuffer MoveTo(int position);
        void Reset();
        Task FlushAsync(Stream output);
        void Flush(Stream output);
        void Flush(INetBuffer output);
        void CopyAllTo(INetBuffer targetBuffer);
        void ReadAndWriteTo(INetBuffer targetBuffer, int bytesCount);
        ValueTask FromStreamData(Stream stream, int bytesCount, CancellationToken cancellationToken = default);
        ValueTask AppendStreamData(Stream stream, int bytesCount, CancellationToken cancellationToken = default);
        ValueTask FromStreamData(INetworkStreamReader streamReader, int bytesCount, CancellationToken cancellationToken = default);
        ValueTask AppendStreamData(INetworkStreamReader streamReader, int bytesCount, CancellationToken cancellationToken = default);

        void WriteRandomBytes(int count);
        bool ReadBoolean();
        byte ReadByte();
        void ReadBytes(byte[] buffer, int index, int count);
        byte[] ReadBytes(int count);
        char ReadChar();
        double ReadDouble();
        short ReadInt16();
        int ReadInt32();
        long ReadInt64();
        float ReadSingle();
        ushort ReadUInt16();
        uint ReadUInt32();
        ulong ReadUInt64();
        ushort ReadUInt16BigEndian();
        uint ReadUInt24BigEndian();
        uint ReadUInt32BigEndian();
        short ReadInt16BiEndian();
        int ReadInt24BigEndian();
        int ReadInt32BigEndian();

        void Write(bool value);
        void Write(byte value);
        void Write(byte[] buffer);
        void Write(byte[] buffer, int offset, int count);
        void Write(char value);
        void Write(double value);
        void Write(float value);
        void Write(int value);
        void Write(long value);
        void Write(Memory<byte> memory);
        void Write(ReadOnlySpan<byte> buffer);
        void Write(short value);
        void Write(uint value);
        void Write(ulong value);
        void Write(ushort value);

        void WriteUint16BigEndian(ushort value);
        void WriteUInt24BigEndian(uint value);
        void WriteUInt32BigEndian(uint value);
        void WriteInt16BigEndian(short value);
        void WriteInt24BigEndian(int value);
        void WriteInt32BigEndian(int value);
    }
}