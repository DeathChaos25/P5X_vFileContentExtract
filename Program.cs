using P5X_vFileContentExtract.FileContent;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection.PortableExecutable;
using System.Text;

namespace P5X_vFileContentExtract
{
    internal class Program
    {
        static List<string> errorList = new List<string>();
        public static async Task Main(string[] args)
        {
            string filePath = string.Empty;
            Stopwatch timer = new Stopwatch();
            timer.Start();

            if (args.Length == 0 || args[0].ToLower() == "-h" || args[0].ToLower() == "-help")
            {
                System.Console.WriteLine("P5X_vFileContentExtract:\nUsage:\nDrag and Drop a _vfileIndexV2.fb file into the program's exe\nPress any key to exit");
                Console.ReadKey();
                return;
            }
            else
            {
                 filePath = args[0];
            }

            FileInfo arg0 = new FileInfo(filePath);

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            Console.WriteLine($"Attempting to parse {arg0.Name}");

            List<FileContentHeader> allEntries = new List<FileContentHeader>();

            // Open the binary file for reading
            using (BinaryReader reader = new BinaryReader(File.OpenRead(filePath)))
            {
                FileHeader header = new FileHeader(reader);

                // Process each section pointer
                foreach (int sectionPTR in header.SectionPTRs)
                {
                    reader.BaseStream.Seek(sectionPTR, SeekOrigin.Begin);
                    FileContentHeader contentHeader = new FileContentHeader(reader);

                    foreach (int contentSectionPTR in contentHeader.ContentSectionPTRs)
                    {
                        reader.BaseStream.Seek(contentSectionPTR, SeekOrigin.Begin);
                        FileContentEntry contentEntry = new FileContentEntry(reader);
                        contentHeader.Entries.Add(contentEntry);
                    }

                    allEntries.Add(contentHeader);

                }

                Console.WriteLine("Verifying obtained data...\n");
                // Use allEntries list as needed
                /*foreach(FileContentHeader fileContentEntry in allEntries)
                {
                    string savePath = arg0.FullName.Replace(arg0.Name, fileContentEntry.FileContentFileName) + ".txt";
                    List<string> ContentFileListOutput = new List<string>();

                    foreach(FileContentEntry fileEntry in fileContentEntry.Entries)
                    {
                        ContentFileListOutput.Add($"Filename: {fileEntry.Name} -> Size 0x{fileEntry.ContentFileSize:X8} at offset 0x{fileEntry.ContentFileOffset:X8}\n");
                    }

                    File.WriteAllLines(savePath, ContentFileListOutput, Encoding.UTF8);
                }*/
            }

            List<Task> tasks = new List<Task>();
            foreach (FileContentHeader targetParse in allEntries)
            {
                tasks.Add(Task.Run(() => ProcessFileAsync(targetParse, arg0.DirectoryName)));
            }

            await Task.WhenAll(tasks);

            timer.Stop();

            if (errorList.Count > 0)
            {
                Console.WriteLine("Encountered the following errors:");
                foreach (var error in errorList)
                {
                    Console.WriteLine(error);
                }
            }

            Console.WriteLine($"\nDone! Time elapsed: {timer.Elapsed}\nPress any key to exit...");
            Console.ReadKey();
        }

        public static async Task ProcessFileAsync(FileContentHeader targetParse, string SourcePath)
        {
            string SourceFile = Path.Join(SourcePath, targetParse.FileContentFileName);

            if (!File.Exists(SourceFile))
            {
                Console.WriteLine($"\nError: File {targetParse.FileContentFileName} does not exist, missing from {SourceFile}");
                return;
            }
            else Console.WriteLine($"Dumping Files from {targetParse.FileContentFileName}");

            try
            {
                using (BinaryReader reader = new BinaryReader(File.Open(SourceFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)))
                {
                    foreach(FileContentEntry fileEntry in targetParse.Entries)
                    {
                        reader.BaseStream.Seek(fileEntry.ContentFileOffset, SeekOrigin.Begin);
                        string SavePath = Path.Join(SourcePath, "fileContentOut", fileEntry.Name);

                        FileInfo SavePath_info = new FileInfo(SavePath);
                        Directory.CreateDirectory(SavePath_info.DirectoryName);

                        byte[] fileData = reader.ReadBytes(fileEntry.ContentFileSize);

                        if (fileEntry.Name.Contains("Bms", StringComparison.CurrentCultureIgnoreCase)) // BMS beatmap file
                        {
                            // BMS filename is in hex, lets convert it to a string instead
                            string hexPart = Path.GetFileNameWithoutExtension(fileEntry.Name);

                            byte[] bytes = new byte[hexPart.Length / 2];
                            for (int i = 0; i < hexPart.Length; i += 2)
                            {
                                bytes[i / 2] = Convert.ToByte(hexPart.Substring(i, 2), 16);
                            }

                            string decodedString = Encoding.UTF8.GetString(bytes);

                            SavePath = Path.Join(SourcePath, "fileContentOut", fileEntry.Name.Replace(hexPart, decodedString));
                            SavePath = SavePath.Replace(Path.GetExtension(SavePath), ".bms");

                            Console.WriteLine($"Saving decrypted Bms file {fileEntry.Name} as Bms/{Path.GetFileName(SavePath)}");

                            await DecryptBMS.DecryptBMSFile(SavePath, fileData);
                        }
                        else if (fileEntry.Name.Contains(".bundle", StringComparison.CurrentCultureIgnoreCase))
                        {
                            Console.WriteLine($"Saving Deobfuscated bundle {fileEntry.Name}");

                            await P5XBundleDeobfuscate.DeobfuscateBundle(SavePath, fileData);
                        }
                        else
                        {
                            using (BinaryWriter writer = new BinaryWriter(File.Open(SavePath, FileMode.Create, FileAccess.Write, FileShare.Read)))
                            {
                                Console.WriteLine($"Saving file {fileEntry.Name}");

                                await writer.BaseStream.WriteAsync(fileData, 0, fileData.Length);
                            }
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                errorList.Add("Error on file " + SourceFile + ". " + ex.Message);
            }
        }
    }
}
