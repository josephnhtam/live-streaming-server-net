namespace LiveStreamingServerNet.Newtorking
{
    public partial class NetBufferBase
    {
        public void ReadBytes(byte[] buffer, int index, int count)
        {
            GetReader().Read(buffer, index, count);
        }

        public byte[] ReadBytes(int count)
        {
            return GetReader().ReadBytes(count);
        }

        public byte ReadByte()
        {
            return GetReader().ReadByte();
        }

        public short ReadInt16()
        {
            return GetReader().ReadInt16();
        }

        public int ReadInt32()
        {
            return GetReader().ReadInt32();
        }

        public long ReadInt64()
        {
            return GetReader().ReadInt64();
        }

        public ushort ReadUInt16()
        {
            return GetReader().ReadUInt16();
        }

        public uint ReadUInt32()
        {
            return GetReader().ReadUInt32();
        }

        public ulong ReadUInt64()
        {
            return GetReader().ReadUInt64();
        }

        public float ReadSingle()
        {
            return GetReader().ReadSingle();
        }

        public double ReadDouble()
        {
            return GetReader().ReadDouble();
        }

        public string ReadString()
        {
            return GetReader().ReadString();
        }

        public bool ReadBoolean()
        {
            return GetReader().ReadBoolean();
        }

        public char ReadChar()
        {
            return GetReader().ReadChar();
        }

        public ushort ReadUInt16BigEndian()
        {
            var reader = GetReader();
            return (ushort)(reader.ReadByte() << 8 | reader.ReadByte());
        }

        public uint ReadUInt24BigEndian()
        {
            var reader = GetReader();
            return (uint)(reader.ReadByte() << 16 | reader.ReadByte() << 8 | reader.ReadByte());
        }

        public uint ReadUInt32BigEndian()
        {
            var reader = GetReader();
            return (uint)(reader.ReadByte() << 24 | reader.ReadByte() << 16 | reader.ReadByte() << 8 | reader.ReadByte());
        }

        public short ReadInt16BiEndian()
        {
            var reader = GetReader();
            var value = reader.ReadByte() << 8 | reader.ReadByte();

            if ((value & 0x8000) != 0)
                value |= unchecked((short)0xffff0000);

            return (short)value;
        }

        public int ReadInt24BigEndian()
        {
            var reader = GetReader();
            var value = reader.ReadByte() << 16 | reader.ReadByte() << 8 | reader.ReadByte();

            if ((value & 0x800000) != 0)
                value |= unchecked((int)0xff000000);

            return value;
        }

        public int ReadInt32BigEndian()
        {
            var reader = GetReader();
            var value = reader.ReadByte() << 24 | reader.ReadByte() << 16 | reader.ReadByte() << 8 | reader.ReadByte();

            if ((value & 0x80000000) != 0)
                value |= unchecked((int)0xff000000);

            return value;
        }
    }
}
