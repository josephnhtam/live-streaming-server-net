namespace LiveStreamingServer.Newtorking.Contracts
{
    public interface INetBuffer : IDisposable
    {
        int Position { get; set; }
        int Size { get; set; }

        MemoryStream UnderlyingStream { get; }

        INetBuffer MoveTo(int position);
        void Reset();
        void Flush(Stream output);
        void Flush(INetBuffer output);
        void CopyTo(INetBuffer targetBuffer, int bytesCount);
        Task CopyStreamData(Stream stream, int bytesCount, CancellationToken cancellationToken = default);

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
        string ReadString();
        ushort ReadUInt16();
        uint ReadUInt32();
        ulong ReadUInt64();

        void Write(bool value);
        void Write(byte value);
        void Write(byte[] buffer, int offset, int count);
        void Write(char value);
        void Write(double value);
        void Write(float value);
        void Write(int value);
        void Write(long value);
        void Write(Memory<byte> memory);
        void Write(ReadOnlySpan<byte> buffer);
        void Write(short value);
        void Write(string value);
        void Write(uint value);
        void Write(ulong value);
        void Write(ushort value);
    }
}