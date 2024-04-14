using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P5X_vFileContentExtract.FileContent
{
    internal class FileContentEntry
    {
        public int Hash { get; private set; }
        public int EntrySize { get; private set; }
        public int ContentFileSize { get; private set; }
        public int ContentFileOffset { get; private set; }
        public int Field10 { get; private set; }
        public int Field14 { get; private set; }
        public int NameSize { get; private set; }
        public string Name { get; private set; }

        public FileContentEntry(BinaryReader reader)
        {
            Hash = reader.ReadInt32();
            EntrySize = reader.ReadInt32();
            ContentFileSize = reader.ReadInt32();
            ContentFileOffset = EntrySize >= 0x10 ? reader.ReadInt32() : 0;
            Field10 = EntrySize >= 0x14 ? reader.ReadInt32() : 0;
            Field14 = EntrySize >= 0x18 ? reader.ReadInt32() : 0;
            int test = reader.ReadInt32();
            if (test != 0) reader.BaseStream.Seek(reader.BaseStream.Position - 4, SeekOrigin.Begin);
            NameSize = reader.ReadInt32();
            Name = System.Text.Encoding.ASCII.GetString(reader.ReadBytes(NameSize));
        }
    }
}
