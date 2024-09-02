using System;
using System.Reflection;

namespace mexLib.Utilties
{
    internal static class EndianSwapUtilities
    {
        public static object SwapEndianness(object obj)
        {
            Type type = obj.GetType();
            foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
            {
                object? value = field.GetValue(obj);
                if (value == null)
                    continue;
                Type fieldType = field.FieldType;

                if (fieldType == typeof(short))
                {
                    short swapped = SwapEndianness((short)value);
                    field.SetValue(obj, swapped);
                }
                else if (fieldType == typeof(ushort))
                {
                    ushort swapped = SwapEndianness((ushort)value);
                    field.SetValue(obj, swapped);
                }
                else if (fieldType == typeof(int))
                {
                    int swapped = SwapEndianness((int)value);
                    field.SetValue(obj, swapped);
                }
                else if (fieldType == typeof(uint))
                {
                    uint swapped = SwapEndianness((uint)value);
                    field.SetValue(obj, swapped);
                }
                else if (fieldType == typeof(long))
                {
                    long swapped = SwapEndianness((long)value);
                    field.SetValue(obj, swapped);
                }
                else if (fieldType == typeof(ulong))
                {
                    ulong swapped = SwapEndianness((ulong)value);
                    field.SetValue(obj, swapped);
                }
                else if (fieldType == typeof(float))
                {
                    float swapped = SwapEndianness((float)value);
                    field.SetValue(obj, swapped);
                }
                else if (fieldType == typeof(double))
                {
                    double swapped = SwapEndianness((double)value);
                    field.SetValue(obj, swapped);
                }
                // Add more types as necessary
            }

            return obj;
        }

        private static short SwapEndianness(short value)
        {
            return (short)((value << 8) | ((value >> 8) & 0xFF));
        }

        private static ushort SwapEndianness(ushort value)
        {
            return (ushort)((value << 8) | (value >> 8));
        }

        private static int SwapEndianness(int value)
        {
            return (int)((SwapEndianness((ushort)(value & 0xFFFF)) << 16) |
                         (SwapEndianness((ushort)((value >> 16) & 0xFFFF))));
        }

        private static uint SwapEndianness(uint value)
        {
            return (uint)((SwapEndianness((ushort)(value & 0xFFFF)) << 16) |
                          (SwapEndianness((ushort)((value >> 16) & 0xFFFF))));
        }

        private static long SwapEndianness(long value)
        {
            return ((long)SwapEndianness((int)(value & 0xFFFFFFFF)) << 32) |
                    ((long)SwapEndianness((int)((value >> 32) & 0xFFFFFFFF)));
        }

        private static ulong SwapEndianness(ulong value)
        {
            return ((ulong)SwapEndianness((uint)(value & 0xFFFFFFFF)) << 32) |
                   ((ulong)SwapEndianness((uint)((value >> 32) & 0xFFFFFFFF)));
        }

        private static float SwapEndianness(float value)
        {
            uint intValue = BitConverter.SingleToUInt32Bits(value);
            uint swapped = SwapEndianness(intValue);
            return BitConverter.UInt32BitsToSingle(swapped);
        }

        private static double SwapEndianness(double value)
        {
            long longValue = BitConverter.DoubleToInt64Bits(value);
            long swapped = SwapEndianness(longValue);
            return BitConverter.Int64BitsToDouble(swapped);
        }
    }
}
