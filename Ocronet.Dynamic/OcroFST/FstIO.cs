using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Ocronet.Dynamic.Interfaces;

namespace Ocronet.Dynamic.OcroFST
{
    public class FstIO
    {
        // They say it also encodes endianness. But I haven't seen any BE variant.
        protected static readonly Int32 OPENFST_MAGIC = 2125659606; // 0x7EB2FDD6;
        protected static readonly Int32 OPENFST_SYMBOL_TABLE_MAGIC = 2125658996; // 0x7EB2FB74
        protected static readonly Int32 FLAG_HAS_ISYMBOLS = 1;
        protected static readonly Int32 FLAG_HAS_OSYMBOLS = 2;
        protected static readonly Int32 MIN_VERSION = 2;
        protected static readonly Int64 PROPERTIES = 3; // expanded, mutable

        #region IO Helpers

        protected static Int32 read_int32_LE(BinaryReader reader)
        {
            return reader.ReadInt32();
        }

        protected static void write_int32_LE(BinaryWriter writer, Int32 n)
        {
            writer.Write(n);
        }

        protected static Int64 read_int64_LE(BinaryReader reader)
        {
            return reader.ReadInt64();
        }

        protected static void write_int64_LE(BinaryWriter writer, Int64 n)
        {
            write_int32_LE(writer, (int)n);
            write_int32_LE(writer, (int)(n >> 32));
        }

        protected static float read_float(BinaryReader reader)
        {
            return reader.ReadSingle();
        }

        protected static void write_float(BinaryWriter writer, float f)
        {
            writer.Write(f);
        }

        protected static bool read_magic_string(BinaryReader reader, string s)
        {
            int n = read_int32_LE(reader);
            if (s.Length != n)
                return false;
            byte[] buf = reader.ReadBytes(n);
            string str = Encoding.ASCII.GetString(buf);
            if (str != s)
                return false;
            return true;
        }

        protected static void skip_string(BinaryReader reader)
        {
            int n = read_int32_LE(reader);
            reader.ReadBytes(n);
        }

        protected static void write_string(BinaryWriter writer, string s)
        {
            int n = s.Length;
            write_int32_LE(writer, n);
            byte[] buf = Encoding.ASCII.GetBytes(s);
            writer.Write(buf);
        }

        protected static bool skip_symbol_table(BinaryReader reader)
        {
            if (read_int32_LE(reader) != OPENFST_SYMBOL_TABLE_MAGIC)
                return false;
            skip_string(reader); // name
            read_int64_LE(reader); // available key
            Int64 n = read_int64_LE(reader);
            for (int i = 0; i < n; i++)
            {
                skip_string(reader);    // key
                read_int64_LE(reader);  // value
            }
            return true;
        }

        #endregion // IO Helpers

        public static void fst_write(BinaryWriter writer, IGenericFst fst)
        {
            write_header_and_symbols(writer, fst);
            for (int i = 0; i < fst.nStates(); i++)
                write_node(writer, fst, i);
        }

        protected static void write_header_and_symbols(BinaryWriter writer, IGenericFst fst)
        {
            write_int32_LE(writer, OPENFST_MAGIC);
            write_string(writer, "vector");
            write_string(writer, "standard");
            write_int32_LE(writer, MIN_VERSION);
            write_int32_LE(writer, /* flags: */ 0);
            write_int64_LE(writer, PROPERTIES);
            write_int64_LE(writer, fst.GetStart());
            write_int64_LE(writer, fst.nStates());
            write_int64_LE(writer, /* narcs (seems to be unused): */ 0L);
        }

        protected static void write_node(BinaryWriter writer, IGenericFst fst, int index)
        {
            Intarray inputs = new Intarray();
            Intarray targets = new Intarray();
            Intarray outputs = new Intarray();
            Floatarray costs = new Floatarray();
            fst.Arcs(inputs, targets, outputs, costs, index);
            int narcs = targets.Length();

            write_float(writer, fst.GetAcceptCost(index));
            write_int64_LE(writer, narcs);
            for (int i = 0; i < narcs; i++)
            {
                write_int32_LE(writer, inputs[i]);
                write_int32_LE(writer, outputs[i]);
                write_float(writer, costs[i]);
                write_int32_LE(writer, targets[i]);
            }
        }

        public static void fst_read(IGenericFst fst, BinaryReader reader)
        {
            read_header_and_symbols(fst, reader);
            for (int i = 0; i < fst.nStates(); i++)
                read_node(reader, fst, i);
        }

        protected static void read_header_and_symbols(IGenericFst fst, BinaryReader reader)
        {
            if (read_int32_LE(reader) != OPENFST_MAGIC)
                throw new Exception("invalid magic number");
            read_magic_string(reader, "vector");
            read_magic_string(reader, "standard");
            int version = read_int32_LE(reader);
            if (version < MIN_VERSION)
                throw new Exception("file has too old version");
            int flags = read_int32_LE(reader);
            read_int64_LE(reader); // properties
            Int64 start = read_int64_LE(reader);
            Int64 nstates = read_int64_LE(reader);
            if (nstates < 0)
                return;   // to prevent creating 2^31 nodes in case of sudden EOF
            fst.Clear();
            for (int i = 0; i < nstates; i++)
                fst.NewState();
            fst.SetStart((int)start);

            read_int64_LE(reader); // narcs

            if ((flags & FLAG_HAS_ISYMBOLS) > 0)
                skip_symbol_table(reader);
            if ((flags & FLAG_HAS_OSYMBOLS) > 0)
                skip_symbol_table(reader);
        }

        protected static void read_node(BinaryReader reader, IGenericFst fst, int index)
        {
            fst.SetAccept(index, read_float(reader));
            Int64 narcs = read_int64_LE(reader);
            for (int i = 0; i < narcs; i++)
            {
                int input = read_int32_LE(reader);
                int output = read_int32_LE(reader);
                float cost = read_float(reader);
                int target = read_int32_LE(reader);
                fst.AddTransition(index, target, output, cost, input);
            }
        }

    }
}
