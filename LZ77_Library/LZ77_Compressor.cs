using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LZ77_Library
{
    public class LZ77_Compressor
    {
        public int bufferLength;
        public LZ77_Compressor(int bufferLength)
        {
            if(bufferLength > ushort.MaxValue)
            {
                throw new Exception("buffer too big");
            }
            this.bufferLength = bufferLength;
        }

        //offcourse if the match is lower then four bytes this encoding actually makes it bigger fun fact
        private Memory<byte> GetDistanceLength(ushort distance, ushort length) {
            var result = new Memory<byte>(new byte[4]);
            BitConverter.TryWriteBytes(result.Span.Slice(0, 2), distance);
            BitConverter.TryWriteBytes(result.Span.Slice(2, 2), length);
            return result;
        }

        private int IndexOfSubarray(Memory<byte> x, Memory<byte> y)
        {
            for (int i = x.Length - y.Length; i >= 0 ; i--)
            {
                if (x.Span.Slice(i, y.Length).SequenceEqual(y.Span))
                {
                    return i;
                }
            }
            return -1;
        }

        public byte[] Encode(byte[] data) {
            Memory<byte> buffer = Memory<byte>.Empty;

            //output
            List<byte> output = new List<byte>();

            int i = 0;
            while (i < data.Length) {
                var matchingIndex = buffer.Span.LastIndexOf(data[i]);
                //handle case were no match is foud
                if (matchingIndex == -1)
                {
                    output.AddRange(new byte[] { 0x0, 0x0, 0x0, 0x0, data[i] });
                }
                else {
                    int j = i + 1;
                    while (j < data.Length){
                        Memory<byte> searchingOccurrence = data.AsMemory(i,(j - i +1));
                        int localIndex = IndexOfSubarray(buffer, searchingOccurrence);

                        if (localIndex == -1)
                        {
                            break;
                        }

                        matchingIndex = localIndex;
                        j++;
                    }

                    output.AddRange(GetDistanceLength((ushort)(buffer.Length - matchingIndex), (ushort)(j-i)).Span.ToArray());
                    i = j -1;
                }

                i++;
                buffer = data.AsMemory(i < bufferLength ? 0 : i - bufferLength, i < bufferLength ? i : bufferLength);
            }

            return output.ToArray();
        }

        public byte[] Decode(byte[] data)
        {
            List<byte> output = new List<byte>();
            for (int i = 0; i < data.Length; i++){
                ushort distance = BitConverter.ToUInt16(data, i);
                ushort length = BitConverter.ToUInt16(data, i+2);
                if (distance == 0 || length == 0)
                {
                    output.Add(data[i + 4]);
                    i += 4;
                }
                else
                {
                    var byteToCopyIndex = output.Count - distance;
                    output.AddRange(output.Skip(byteToCopyIndex).Take(length));
                    i += 3;
                }
            }

            return output.ToArray();
        }
    }
}
