using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace uWdfArchiver
{
    class Program
    {
        struct FileEntry {
            public int startOffset;
            public int size;
            public uint key;
        }

        static void Main(string[] args)
        {
            string inputdir = args[0];
            string outputFile = args[1];

            var file = File.OpenWrite(outputFile);
            var writer = new BinaryWriter(file, Encoding.UTF8);
            writer.Write("PFDW".ToCharArray());

            string[] foundFiles = Directory.GetFiles(inputdir, "**", SearchOption.AllDirectories).Select(x => x.Substring(inputdir.Length + 1)).ToArray();

            List<FileEntry> files = new List<FileEntry>();

            writer.Write((UInt32)foundFiles.Length);
            writer.Write((UInt32)0); // Placeholder for the filetable offset which will be generated at the end of the file
            writer.Write((UInt32)0); // 4 byte padding between first file and header

            foreach (string inputFile in foundFiles)
            {
                var openFile = File.OpenRead(inputdir + "/" + inputFile);
                var contents = new BinaryReader(openFile).ReadBytes((int)openFile.Length);
                int size = contents.Length;
                int startoffset = (int)file.Position;
                uint id = inputFile.Hash();

                writer.Write(contents);
                files.Add(new FileEntry()
                {
                    key = id,
                    size = size,
                    startOffset = startoffset
                }
                );
            }

            int fileTableStartPosition = (int)file.Position;
            foreach (var entry in files)
            {
                writer.Write((UInt32)entry.key);
                writer.Write((UInt32)entry.startOffset);
                writer.Write((UInt32)entry.size);
                writer.Write((UInt32)0);
            }

            file.Seek(8, SeekOrigin.Begin);
            writer.Write((UInt32)fileTableStartPosition);

            writer.Close();
            file.Close();
        }
    }
}
