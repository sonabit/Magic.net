using JetBrains.Annotations;


// ReSharper disable once CheckNamespace
namespace System
{
    public static class ValueTypWriteExtensions
    {
        public static void ToBuffer(this Int32 value, byte[] buffer, int offset = 0)
        {
            buffer[offset + 3] = (byte)(value >> 24);
            buffer[offset + 2] = (byte)(value >> 16);
            buffer[offset + 1] = (byte)(value >> 8);
            buffer[offset + 0] = (byte)value;
        }
        
        public static void ToBuffer(this byte value,[NotNull] byte[] buffer, int offset = 0)
        {
            buffer[offset] = value;
        }
    }
}
