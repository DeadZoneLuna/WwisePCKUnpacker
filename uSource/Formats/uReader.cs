using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using uSource.Formats.Extensions;

namespace uSource.Formats
{
    public class uReader : BinaryReader
    {
        public uReader(Stream inputStream) : base(inputStream)
        {
            if (!inputStream.CanRead)
                throw new ArgumentException("Stream unreadable!");
        }

        public uReader(Stream inputStream, Encoding enconding) : base(inputStream, enconding)
        {
            if (!inputStream.CanRead)
                throw new ArgumentException("Stream unreadable!");
        }

        public long AlignSeek(long alignTo, SeekOrigin origin = SeekOrigin.Current)
        {
            long seekBytes = BaseStream.Position % alignTo;
            if (seekBytes > 0)
            {
                long nextPos = alignTo - seekBytes;
                switch (origin)
                {
                    case SeekOrigin.Begin: BaseStream.Position = nextPos; break;
                    case SeekOrigin.Current: BaseStream.Position += nextPos; break;
                    case SeekOrigin.End: BaseStream.Position = BaseStream.Length - nextPos; break;
                }
            }

            return BaseStream.Position;
        }

        #region Universal Types
        public unsafe T ReadTypeFixed<T>(ref T value, int sizeOf, long? offset = null)
        {
            if (offset.HasValue) BaseStream.Seek(offset.Value, SeekOrigin.Begin);

            fixed (byte* ptr = &ReadBytes(sizeOf)[0])
            {
                return value = (T)Marshal.PtrToStructure((IntPtr)ptr, typeof(T));
            }
        }

        public T[] ReadArrayFixed<T>(int length, int sizeOf, long? offset = null)
        {
            T[] tempArr = new T[length / sizeOf];
            ReadArrayFixed(ref tempArr, sizeOf, offset);

            return tempArr;
        }

        public T[] ReadArrayFixed<T>(ref T[] tempArr, int sizeOf, long? offset = null)
        {
            if (offset.HasValue) BaseStream.Seek(offset.Value, SeekOrigin.Begin);

            for (int i = 0; i < tempArr.Length; i++)
                ReadTypeFixed(ref tempArr[i], sizeOf);

            return tempArr;
        }

        public T ReadType<T>(ref T value, long? offset = null)
        {
            return ReadTypeFixed(ref value, Marshal.SizeOf(typeof(T)), offset);
        }

        public T[] ReadArray<T>(int length, long? offset = null)
        {
            return ReadArrayFixed<T>(length, Marshal.SizeOf(typeof(T)), offset);
        }

        public T[] ReadArray<T>(ref T[] tempArr, long? offset = null)
        {
            return ReadArrayFixed<T>(ref tempArr, Marshal.SizeOf(typeof(T)), offset);
        }
        #endregion

        #region Strings
        public string ReadFixedLengthString(Encoding encoding, int length, long? Offset = null)
        {
            if (Offset.HasValue) BaseStream.Seek(Offset.Value, SeekOrigin.Begin);

            byte[] bstr = ReadBytes(length).TakeWhile(b => b != 0).ToArray();
            return encoding.GetString(bstr);

        }

        public string ReadNullTerminatedString(long? Offset = null)
        {
            if (Offset.HasValue) BaseStream.Seek(Offset.Value, SeekOrigin.Begin);

            char c;
            string str = string.Empty;
            while ((c = Convert.ToChar(ReadByte())) != 0)
                str += c;

            return str;
        }

        public string ReadNullTerminatedString(Encoding encoding, long? Offset = null)
        {
            if (Offset.HasValue) BaseStream.Seek(Offset.Value, SeekOrigin.Begin);

            List<byte> strBytes = new List<byte>();
            byte[] terminator = encoding.GetBytes("\0");    // Problem: The encoding may not have a NULL character
            int charSize = terminator.Length;               // Problem: The character size may be variable

            byte[] chr;
            while (!(chr = ReadBytes(charSize)).SequenceEqual(terminator))
            {
                if (chr.Length != charSize)
                    throw new EndOfStreamException();

                strBytes.AddRange(chr);
            }

            return encoding.GetString(strBytes.ToArray());
        }
        #endregion

        #region Primitive types
        public float[] ReadSingleArray(int num)
        {
            float[] arr = new float[num];
            for (int i = 0; i < num; i++)
                arr[i] = ReadSingle();

            return arr;
        }

        public int[] ReadIntArray(int num)
        {
            int[] arr = new int[num];
            for (int i = 0; i < num; i++)
                arr[i] = ReadInt32();

            return arr;
        }

        public ushort[] ReadUshortArray(int num)
        {
            ushort[] arr = new ushort[num];
            for (int i = 0; i < num; i++)
                arr[i] = ReadUInt16();

            return arr;
        }

        public short[] ReadShortArray(int num)
        {
            short[] arr = new short[num];
            for (int i = 0; i < num; i++)
                arr[i] = ReadInt16();

            return arr;
        }

        #region BigEndian
        public static byte[] Reverse(byte[] b)
        {
            Array.Reverse(b);
            return b;
        }

        public UInt16 ReadUInt16BE()
        {
            return BitConverter.ToUInt16(Reverse(ReadBytesRequired(sizeof(UInt16))), 0);
        }

        public Int16 ReadInt16BE()
        {
            return BitConverter.ToInt16(Reverse(ReadBytesRequired(sizeof(Int16))), 0);
        }

        public UInt32 ReadUInt32BE()
        {
            return BitConverter.ToUInt32(Reverse(ReadBytesRequired(sizeof(UInt32))), 0);
        }

        public Int32 ReadInt32BE()
        {
            return BitConverter.ToInt32(Reverse(ReadBytesRequired(sizeof(Int32))), 0);
        }

        public int[] ReadInt32ArrayBE(int num)
        {
            int[] arr = new int[num];
            for (int i = 0; i < num; i++)
                arr[i] = ReadInt32BE();

            return arr;
        }

        public byte[] ReadBytesRequired(int byteCount)
        {
            var result = ReadBytes(byteCount);

            if (result.Length != byteCount)
                throw new EndOfStreamException(string.Format("{0} bytes required, but only {1} returned.", byteCount, result.Length));

            return result;
        }
        #endregion
        #endregion
    }
}