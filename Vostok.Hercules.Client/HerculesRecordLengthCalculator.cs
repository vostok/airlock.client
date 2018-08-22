using System;
using System.Net;
using Vostok.Hercules.Client.TimeBasedUuid;

namespace Vostok.Hercules.Client
{
    internal static class HerculesRecordLengthCalculator
    {
        public static int Calculate(byte[] buffer, int position)
        {
            var offset = position + sizeof(byte) + TimeGuid.Size;

            var tagsCount = ReadInt16(buffer, offset);
            offset += sizeof(short);

            for (var i = 0; i < tagsCount; i++)
            {
                var tagKeyLength = buffer[offset++];
                offset += tagKeyLength;

                var tagValueTypeDefinition = (TagValueTypeDefinition) buffer[offset++];
                offset += HerculesRecordTagValueLengthCalculator.Calculate(buffer, offset, tagValueTypeDefinition);
            }

            return offset - position;
        }

        private static short ReadInt16(byte[] buffer, int position) =>
            IPAddress.NetworkToHostOrder(BitConverter.ToInt16(buffer, position));
    }
}