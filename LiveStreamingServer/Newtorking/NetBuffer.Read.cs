namespace LiveStreamingServer.Newtorking
{
    public partial class NetBuffer
    {
        public void ReadBytes(byte[] buffer, int index, int count)
        {
            _reader.Read(buffer, index, count);
        }

        public byte[] ReadBytes(int count)
        {
            return _reader.ReadBytes(count);
        }

        public byte ReadByte()
        {
            return _reader.ReadByte();
        }

        public short ReadInt16()
        {
            return _reader.ReadInt16();
        }

        public int ReadInt32()
        {
            return _reader.ReadInt32();
        }

        public long ReadInt64()
        {
            return _reader.ReadInt64();
        }

        public ushort ReadUInt16()
        {
            return _reader.ReadUInt16();
        }

        public uint ReadUInt32()
        {
            return _reader.ReadUInt32();
        }

        public ulong ReadUInt64()
        {
            return _reader.ReadUInt64();
        }

        public float ReadSingle()
        {
            return _reader.ReadSingle();
        }

        public double ReadDouble()
        {
            return _reader.ReadDouble();
        }

        public string ReadString()
        {
            return _reader.ReadString();
        }

        public bool ReadBoolean()
        {
            return _reader.ReadBoolean();
        }

        public char ReadChar()
        {
            return _reader.ReadChar();
        }
    }
}
