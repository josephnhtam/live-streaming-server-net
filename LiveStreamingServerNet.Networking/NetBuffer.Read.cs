namespace LiveStreamingServerNet.Newtorking
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

        public ushort ReadUInt16BigEndian()
        {
            return (ushort)(_reader.ReadByte() << 8 | _reader.ReadByte());
        }

        public uint ReadUInt24BigEndian()
        {
            return (uint)(_reader.ReadByte() << 16 | _reader.ReadByte() << 8 | _reader.ReadByte());
        }

        public uint ReadUInt32BigEndian()
        {
            return (uint)(_reader.ReadByte() << 24 | _reader.ReadByte() << 16 | _reader.ReadByte() << 8 | _reader.ReadByte());
        }

        public short ReadInt16BiEndian()
        {
            var value = _reader.ReadByte() << 8 | _reader.ReadByte();

            if ((value & 0x8000) != 0)
                value |= unchecked((short)0xffff0000);

            return (short)value;
        }

        public int ReadInt24BigEndian()
        {
            var value = _reader.ReadByte() << 16 | _reader.ReadByte() << 8 | _reader.ReadByte();

            if ((value & 0x800000) != 0)
                value |= unchecked((int)0xff000000);

            return value;
        }

        public int ReadInt32BigEndian()
        {
            var value = _reader.ReadByte() << 24 | _reader.ReadByte() << 16 | _reader.ReadByte() << 8 | _reader.ReadByte();

            if ((value & 0x80000000) != 0)
                value |= unchecked((int)0xff000000);

            return value;
        }
    }
}
