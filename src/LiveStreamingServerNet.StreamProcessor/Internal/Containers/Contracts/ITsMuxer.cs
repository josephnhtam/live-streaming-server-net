﻿namespace LiveStreamingServerNet.StreamProcessor.Internal.Containers.Contracts
{
    internal interface ITsMuxer : IDisposable
    {
        int BufferSize { get; }
        int PayloadSize { get; }
        uint SequenceNumber { get; }
        uint? SegmentTimestamp { get; }

        ValueTask<SeqSegmentPartial?> FlushPartialAsync();
        ValueTask<SeqSegment?> FlushAsync(uint timestamp);
        void SetAACSequenceHeader(AACSequenceHeader aacSequenceHeader);
        void SetAVCSequenceHeader(AVCSequenceHeader avcSequenceHeader);
        void SetHEVCSequenceHeader(HEVCSequenceHeader hevcSequenceHeader);
        bool WriteAudioPacket(ArraySegment<byte> buffer, uint timestamp);
        bool WriteVideoPacket(ArraySegment<byte> dataBuffer, uint timestamp, uint compositionTime, bool isKeyFrame);
    }
}
