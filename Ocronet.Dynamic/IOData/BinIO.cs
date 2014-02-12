using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Ocronet.Dynamic.IOData
{
    public static class BinIO
    {
        public static uint magic_number<T>()
        {
            uint bytecount = 0;
            Type type = typeof(T);
            switch (type.Name)
            {
                case "Byte":
                    bytecount = 1;
                    break;
                case "SByte":
                    bytecount = 1;
                    break;
                case "Boolean":
                    bytecount = 1;
                    break;
                case "Int16":
                    bytecount = 2;
                    break;
                case "Int32":
                    bytecount = 4;
                    break;
                case "Single":
                    bytecount = 4;
                    break;
                case "Double":
                    bytecount = 8;
                    break;
                default:
                    throw new Exception("Use only for scalar argument!");
            }
            return 0x0abe0000 + bytecount;
        }

        public static unsafe void scalar_read<T>(BinaryReader reader, out T value)
        {
            Object obj;
            switch (typeof(T).Name)
            {
                case "Byte":
                    obj = reader.ReadByte();
                    break;
                case "SByte":
                    obj = reader.ReadSByte();
                    break;
                case "Boolean":
                    obj = reader.ReadBoolean();
                    break;
                case "Int16":
                    obj = reader.ReadInt16();
                    break;
                case "Int32":
                    obj = reader.ReadInt32();
                    break;
                case "Single":
                    obj = reader.ReadSingle();
                    break;
                case "Double":
                    obj = reader.ReadDouble();
                    break;
                default:
                    throw new Exception("Use only for scalar argument!");
            }
            value = (T)obj;
        }

        public static void scalar_write<T>(BinaryWriter writer, T value)
        {
            Object obj = value;
            switch (typeof(T).Name)
            {
                case "Byte":
                    writer.Write((Byte)obj);
                    break;
                case "SByte":
                    writer.Write((SByte)obj);
                    break;
                case "Boolean":
                    writer.Write((Boolean)obj);
                    break;
                case "Int16":
                    writer.Write((Int16)obj);
                    break;
                case "Int32":
                    writer.Write((Int32)obj);
                    break;
                case "Single":
                    writer.Write((Single)obj);
                    break;
                case "Double":
                    writer.Write((Double)obj);
                    break;
                default:
                    throw new Exception("Use only for scalar argument!");
            }
        }

        /// <summary>
        /// Read integer magic number.
        /// </summary>
        public static void magic_read(BinaryReader reader, uint value)
        {
            uint temp = reader.ReadUInt32();
            if (temp != value)
                throw new Exception(String.Format("Mismatch magic value! ({0,x}={1,x})", temp, value));
        }

        /// <summary>
        /// Write integer magic number.
        /// </summary>
        public static void magic_write(BinaryWriter writer, uint value)
        {
            writer.Write(value);
        }

        /// <summary>
        /// Read and Compare string object type identifiers
        /// </summary>
        public static void magic_read(BinaryReader reader, string str)
        {
            int n = str.Length;
            byte[] buf = reader.ReadBytes(n);
            string magic = Encoding.ASCII.GetString(buf);
            if (magic != str)
                throw new Exception(String.Format("magic_read: wanted '{0}', got '{1}'", str, magic));
        }

        public static string magic_get(BinaryReader reader, int n)
        {
            byte[] buf = reader.ReadBytes(n);
            string magic = Encoding.ASCII.GetString(buf);
            return magic;
        }

        /// <summary>
        /// Write string object type identifiers
        /// </summary>
        public static void magic_write(BinaryWriter writer, string str)
        {
            // write string as ASCII 8 bit per char
            byte[] bytes = Encoding.ASCII.GetBytes(str);
            writer.Write(bytes);
        }

        public static void narray_read<T>(BinaryReader reader, Narray<T> data)
        {
            // read and compare magic number
            magic_read(reader, magic_number<T>());
            // read dimensions (4 number)
            int[] dims = new int[4];
            dims[0] = reader.ReadInt32();
            dims[1] = reader.ReadInt32();
            dims[2] = reader.ReadInt32();
            dims[3] = reader.ReadInt32();
            // create empty Narray
            data.Resize(dims[0], dims[1], dims[2], dims[3]);
            // read data of Narray
            for (int i = 0; i < data.Length1d(); i++)
            {
                T val;
                scalar_read<T>(reader, out val);
                data.UnsafePut1d(i, val);
            }
        }

        public static void narray_write<T>(BinaryWriter writer, Narray<T> data)
        {
            uint magic = magic_number<T>();
            // write magic number
            writer.Write(magic);
            // write dimensions (4 number)
            writer.Write(data.Dim(0)); writer.Write(data.Dim(1)); writer.Write(data.Dim(2)); writer.Write(data.Dim(3));
            if (data.Length1d() > 0)
            {
                // write data of Narray
                for (int i = 0; i < data.Length1d(); i++)
                    scalar_write<T>(writer, data.UnsafeAt1d(i));
            }
        }

        public static void string_read(BinaryReader reader, out string s)
        {
            // read string from ASCII 8 bit per char
            StringBuilder sb = new StringBuilder();
            int maxread = 100000;
            while (--maxread > 0)
            {
                char charval = (char)reader.ReadByte();
                if (charval == '\n' || charval == '\x00')
                    break;
                sb.Append(charval);
            }
            s = sb.ToString();
        }

        public static void string_write(BinaryWriter writer, string str)
        {
            // write string as ASCII 8 bit per char
            byte[] bytes = Encoding.ASCII.GetBytes(str);
            writer.Write(bytes);
            // write new line symbol
            writer.Write((byte)'\n');
        }

    }
}
