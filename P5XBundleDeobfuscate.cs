using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P5X_vFileContentExtract
{
    internal class P5XBundleDeobfuscate
    {
        public static uint GetHashCode(string content)
        {
            uint num = 131U;
            uint num2 = 0U;
            for (int i = 0; i < content.Length; i++)
            {
                num2 = num2 * num + content[i];
            }
            return num2 & 2147483647U;
        }

        public static int GetBundleObuscateOffset(string bundleName)
        {
            return (int)(GetHashCode(bundleName) % 32U + 8U);
        }


        public static async Task DeobfuscateBundle(string filePath, byte[] FileStream)
        {
            try
            {
                using (MemoryStream memoryStream = new MemoryStream(FileStream))
                {
                    using (BinaryReader reader = new BinaryReader(memoryStream))
                    {
                        int obfuscateOffset = GetBundleObuscateOffset(Path.GetFileName(filePath));

                        reader.BaseStream.Position = obfuscateOffset;

                        int Magic = reader.ReadInt32();

                        reader.BaseStream.Position -= 4;

                        if (Magic == 1953066581) // UnityFS Magic
                        {
                            byte[] remainingData = reader.ReadBytes((int)(reader.BaseStream.Length - reader.BaseStream.Position));

                            using (BinaryWriter writer = new BinaryWriter(File.Open(filePath, FileMode.Create, FileAccess.Write, FileShare.Read)))
                            {
                                await writer.BaseStream.WriteAsync(remainingData, 0, remainingData.Length);
                            }
                        }
                        else
                        {
                            using (BinaryWriter writer = new BinaryWriter(File.Open(filePath, FileMode.Create, FileAccess.Write, FileShare.Read)))
                            {
                                await writer.BaseStream.WriteAsync(FileStream, 0, FileStream.Length);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error reading or saving file: " + filePath + ". " + ex.Message);
            }
        }
    }
}
