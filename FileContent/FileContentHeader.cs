using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P5X_vFileContentExtract.FileContent
{
    internal class FileContentHeader
    {
        public int Hash { get; private set; }
        public uint SectionTotalSize { get; private set; }
        public int PtrToPtrList { get; private set; }
        public int NumOfContentEntries { get; private set; }
        public List<uint> ContentSectionPTRs { get; private set; }
        public List<FileContentEntry> Entries { get; private set; }
        public string FileContentFileName { get; private set; }

        public FileContentHeader(BinaryReader reader)
        {
            Hash = reader.ReadInt32();
            SectionTotalSize = (uint)reader.BaseStream.Position + reader.ReadUInt32();
            PtrToPtrList = reader.ReadInt32();
            NumOfContentEntries = reader.ReadInt32();
            ContentSectionPTRs = new List<uint>();
            for (uint i = 0; i < NumOfContentEntries; i++)
            {
                ContentSectionPTRs.Add((uint)reader.BaseStream.Position + reader.ReadUInt32());
            }
            Entries = new List<FileContentEntry>();

            // Store the current position
            long currentPosition = reader.BaseStream.Position;

            // Skip to the end of the section
            reader.BaseStream.Seek(SectionTotalSize, SeekOrigin.Begin);

            // Read the name
            int nameSize = reader.ReadInt32();
            FileContentFileName = System.Text.Encoding.ASCII.GetString(reader.ReadBytes(nameSize));

            // Reset the position
            reader.BaseStream.Seek(currentPosition, SeekOrigin.Begin);
        }
    }
}
