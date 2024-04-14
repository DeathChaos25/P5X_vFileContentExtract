using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;

namespace P5X_vFileContentExtract.FileContent
{
    internal class FileHeader
    {
        public int Field00 { get; private set; }
        public int Field04 { get; private set; }
        public int Field08 { get; private set; }
        public int Field0C { get; private set; }
        public int NumOfPTRs { get; private set; }
        public List<uint> SectionPTRs { get; private set; }

        public FileHeader(BinaryReader reader)
        {
            Field00 = reader.ReadInt32();
            Field04 = reader.ReadInt32();
            Field08 = reader.ReadInt32();
            Field0C = reader.ReadInt32();
            NumOfPTRs = reader.ReadInt32();
            SectionPTRs = new List<uint>();
            for (uint i = 0; i < NumOfPTRs; i++)
            {
                SectionPTRs.Add((uint)reader.BaseStream.Position + reader.ReadUInt32());
            }
        }
    }
}
