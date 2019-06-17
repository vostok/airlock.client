using System.Runtime.CompilerServices;
using System.Text;
using JetBrains.Annotations;

namespace Vostok.Hercules.Client.Serialization.Readers
{
    internal class BinaryBufferReader : Commons.Binary.BinaryBufferReader
    {
        private const string EmptyString = "";

        public bool SkipMode;
        
        public BinaryBufferReader([NotNull] byte[] buffer, long position)
            : base(buffer, position)
        {
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override string ReadString(Encoding encoding)
        {
            if (SkipMode)
            {
                var size = ReadInt32();
                Position += size;
                return EmptyString;
            }

            return base.ReadString(encoding);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override string ReadShortString(Encoding encoding)
        {
            if (SkipMode)
            {
                var size = ReadByte();
                Position += size;
                return EmptyString;
            }

            return base.ReadShortString(encoding);
        }
    }
}