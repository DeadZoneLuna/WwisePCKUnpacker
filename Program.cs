using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using uSource.Formats.Extensions;
using uSource.Formats;

namespace WwisePCKUnpacker
{
    class Program
    {
        static bool WithSFX = false;
        static int SelectedLang = 0;
        const string EXTRACT_SFX_TOO = "Extract sfx too? ";
        const string PLEASE_SELECT_LANGUAGE = "Please select language: ";

        static PackageHeader Header;
        static StringMap[] LanguageMaps;
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Input file...");
                Console.ReadKey();
                return;
            }

            Console.WriteLine($"Input: {args[0]}\n");
            FileInfo fileInfo = new FileInfo(args[0].NormalizeSlashes());
            if (!fileInfo.Exists)
                throw new FileLoadException("File not found...");

            using (uReader reader = new uReader(fileInfo.OpenRead()))
            {
                #region Header
                Header = new PackageHeader();
                reader.ReadType(ref Header);
                Console.WriteLine(Header);

                if (Header.HeaderID != PackageHeader.AK_PACKAGE_ID)
                    throw new FileLoadException("Invalid HeaderID");

                #region Languages
                uint numLangs = reader.ReadUInt32();
                Console.WriteLine($"Language count: {numLangs}");
                LanguageMaps = new StringMap[numLangs];
                for (int i = 0; i < numLangs; i++)
                {
                    LanguageMaps[i].offset = reader.ReadUInt32();
                    LanguageMaps[i].id = reader.ReadUInt32();
                }
                for (int i = 0; i < numLangs; i++)
                    LanguageMaps[i].key = reader.ReadNullTerminatedString(Encoding.Unicode);

                // resort result (lowest -> highest)
                LanguageMaps = LanguageMaps.OrderBy(x => x.id).ToArray();

                // alignment current offset to 4
                reader.AlignSeek(4);
                #endregion

                #region Banks
                uint numBanks = reader.ReadUInt32();
                Console.WriteLine($"Banks count: {numBanks}");
                FileLUT[] banks = GetFileEntries(reader, numBanks);
                #endregion

                #region Streamed
                uint numStreams = reader.ReadUInt32();
                Console.WriteLine($"Streams count: {numStreams}");
                FileLUT[] streams = GetFileEntries(reader, numStreams);
                #endregion

                #region External
                uint numExternals = reader.ReadUInt32();
                Console.WriteLine($"Externals count: {numExternals}\n");
                FileLUT[] externals = GetFileEntries(reader, numExternals, true);
                #endregion
                #endregion

                #region Common (Selecting language & etc..)
                // print available languages
                for (int i = 0; i < LanguageMaps.Length; i++)
                {
                    StringMap lang = LanguageMaps[i];
                    Console.WriteLine($"{lang.id} - {lang.GetKeyName()}");
                }

                Console.Write(PLEASE_SELECT_LANGUAGE);
                while (!int.TryParse(Console.ReadLine(), out SelectedLang))
                    Console.Write(PLEASE_SELECT_LANGUAGE);

                Console.WriteLine($"Selected: {LanguageMaps[SelectedLang].GetKeyName()}\n");

                if (SelectedLang != FileLUT.AK_INVALID_LANGUAGE_ID)
                {
                    char tempChar = '0';
                    Console.WriteLine($"{EXTRACT_SFX_TOO} y - yes | n - no");
                    while (!((WithSFX = tempChar == 'y') || tempChar == 'n'))
                        tempChar = Console.ReadKey().KeyChar;
                }
                #endregion

                reader.BaseStream.Position = Header.m_uHeaderSize;

                // TODO: not sure if it works correctly with all files
                ExtractEntries(banks, reader, "Bank");
                ExtractEntries(streams, reader, "Streamed");
                ExtractEntries(externals, reader, "External");
            }

            Console.WriteLine("\nDone. Press any key to close.");
            Console.ReadKey();
        }

        static void ExtractEntries(FileLUT[] entries, uReader reader, string suffix)
        {
            if (entries.Length == 0)
                return;

            //reader.BaseStream.Position += entries[0].uStartingBlock;
            for (int i = 0; i < entries.Length; i++)
            {
                FileLUT entry = entries[i];
                StringMap language = LanguageMaps[entry.uLanguageID];

                // alignment current offset to block size (2048 by default)
                reader.AlignSeek(entry.uBlockSize);

                if (language.id == SelectedLang || (WithSFX && language.id == FileLUT.AK_INVALID_LANGUAGE_ID))
                {
                    // remember position before writing
                    long currPos = reader.BaseStream.Position;

                    // trying parse name
                    reader.BaseStream.Position += 122;
                    string fileName = entry.uFileID.ToString();
                    if (reader.ReadInt32() == 1414744396) //LIST
                    {
                        reader.BaseStream.Position += 20;
                        fileName = reader.ReadNullTerminatedString();
                    }

                    // build output path & make sure if path exist, otherwise create it (folder & subfolders)
                    string savePath = PathExtension.ProjectPath + language.key;
                    DirectoryInfo dirInfo = new DirectoryInfo(savePath);
                    if (!dirInfo.Exists)
                        dirInfo.Create();

                    // offset back to start writing file
                    reader.BaseStream.Position = currPos;

                    // writing to file...
                    fileName = $"{suffix}_{i}_{fileName}.wav";
                    using (BinaryWriter writer = new BinaryWriter(File.Create($"{savePath}{PathExtension.PathSeparator}{fileName}")))
                        writer.Write(reader.ReadBytes((int)entry.uFileSize));

                    Console.WriteLine(fileName);
                }
                else reader.BaseStream.Position += (long)entry.uFileSize;
            }
        }

        static FileLUT[] GetFileEntries(uReader reader, uint numFiles, bool isUINT64 = false)
        {
            if (numFiles == 0)
                return new FileLUT[0];

            FileLUT[] files = new FileLUT[numFiles];
            for (int i = 0; i < numFiles; i++)
            {
                files[i].uFileID = isUINT64 ? reader.ReadUInt64() : reader.ReadUInt32();
                files[i].uBlockSize = reader.ReadUInt32();
                files[i].uFileSize = reader.ReadUInt32();
                files[i].uStartingBlock = reader.ReadUInt32();
                files[i].uLanguageID = reader.ReadUInt32();
            }

            // not sure about sorting, but it looks ok
            return files = files.OrderBy(x => x.uStartingBlock).ToArray();
        }
    }
}