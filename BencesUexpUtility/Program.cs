using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Messaging;

namespace BencesUexpUtility
{
    internal class Program
    {
        static void runFFmpeg(string ffmpegFile, string input, string output)
        {
            try
            {
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = ffmpegFile,
                    Arguments = $"-hide_banner -loglevel error -y -i \"{input}\" -map_metadata -1 -fflags +bitexact -flags:a +bitexact -flags:v +bitexact -vn -c:a pcm_s16le \"{output}\" ",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = false
                };

                Process ffmpegProcess = new Process
                {
                    StartInfo = startInfo
                };
                ffmpegProcess.Start();

                ffmpegProcess.WaitForExit();

                Console.WriteLine("FFmpeg process completed successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error running FFmpeg: {ex.Message}");
                Console.Read();
                return;
            }
        }
        static void runWwise(string wwiseFile, string input, string output)
        {

            try
            {
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = wwiseFile,
                    Arguments = $"-encode  \"{input}\"  \"{output}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = false
                };

                Process wwiseProcess = new Process
                {
                    StartInfo = startInfo
                };
                wwiseProcess.Start();

                wwiseProcess.WaitForExit();

                Console.WriteLine("Wwise encode process completed successfully.");

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error running wwise.exe: {ex.Message}");
            }
        }
        static void createFolder(string name)
        {
            if (!Directory.Exists(name))
            {
                Directory.CreateDirectory(name);
            }
        }
        static void moveFiles(string file, string dest)
        {
            string fileDest = dest + new FileInfo(file).Name;
            if (File.Exists(fileDest))
            {
                File.Delete(fileDest);
            }
            File.Move(file, fileDest);
        }
        static void Main(string[] args)
        {
            //Check for an audio file
            string audioFilePath = args[0];
            if (!(args.Length > 0))
            {
                Console.WriteLine("Drag and drop an MP3 file onto this executable to read its contents and open the related text file.");
                Console.Read();
                return;
            }
            if (!(File.Exists(audioFilePath)))
            {
                Console.WriteLine($"Music file not found: {audioFilePath}");
                Console.Read();
                return;
            }
            FileInfo audioFileInfo = new FileInfo(audioFilePath);
            string[] acceptedExtensions = { ".mp3", ".wav", ".flac", ".ogg", ".webm" };

            if (!(acceptedExtensions.Contains(audioFileInfo.Extension)))
            {
                Console.Read();
                return;
            }
            //

            //Check for all neccesary files
            string uexpFilePath = Path.ChangeExtension(audioFilePath, ".uexp");
            string ubulkFilePath = Path.ChangeExtension(audioFilePath, ".ubulk");
            string uassetFilePath = Path.ChangeExtension(audioFilePath, ".uasset");

            if (!File.Exists(uexpFilePath))
            {
                Console.WriteLine($"uexp file not found: {uexpFilePath}");
                Console.Read();
                return;
            }
            if (!File.Exists(ubulkFilePath))
            {
                Console.WriteLine($"uexp file not found: {uexpFilePath}");
                Console.Read();
                return;
            }
            if (!File.Exists(uassetFilePath))
            {
                Console.WriteLine($"uasset file not found: {uassetFilePath}");
                Console.Read();
                return;
            }
            //

            //Check for ffmpeg and wwise
            string[] files = Directory.GetFiles("./");
            string wwiseFile = Array.Find(files, file => Path.GetFileName(file).StartsWith("wwise")) != null ? new FileInfo(Array.Find(files, file => Path.GetFileName(file).StartsWith("wwise"))).Name : "-1";
            string ffmpegFile = Array.Find(files, file => Path.GetFileName(file).StartsWith("ffmpeg")) != null ? new FileInfo(Array.Find(files, file => Path.GetFileName(file).StartsWith("ffmpeg"))).Name : "-1";

            if (ffmpegFile == "-1")
            {
                Console.WriteLine("FFmpeg is not installed. You need to put ffmpeg.exe besides the utility exe for it to work.");
                Console.Read();
                return;
            }
            Console.WriteLine("FFmpeg is installed.");
            if (wwiseFile == "-1")
            {
                Console.WriteLine("WWise is not installed. You need to put wwise.exe besides the utility exe for it to work.");
                Console.Read();
                return;
            }
            Console.WriteLine("Wwise is installed.");
            //

            //run ffmpeg and wwise
            string replacementFilePath = Path.ChangeExtension(audioFilePath, "re.wav");

            runFFmpeg(ffmpegFile, audioFilePath, replacementFilePath);

            runWwise(wwiseFile, replacementFilePath, Path.ChangeExtension(audioFilePath, ".ubulk"));
            //

            //Uexp editing
            FileInfo ubulkFileInfo = new FileInfo(ubulkFilePath);
            byte[] fileBytes = File.ReadAllBytes(uexpFilePath);

            byte[] replacement64bit = BitConverter.GetBytes(new FileInfo(replacementFilePath).Length);
            byte[] original64bit = BitConverter.GetBytes(ubulkFileInfo.Length);
            byte[] original32bit = new byte[4];
            Array.Copy(original64bit, original32bit, 4);
            byte[] replacement32bit = new byte[4];
            Array.Copy(replacement64bit, replacement32bit, 4);

            Console.WriteLine($"UBULK File Details for {ubulkFileInfo.Name}:");
            Console.WriteLine($"Size (Bytes): {ubulkFileInfo.Length}");

            Console.WriteLine($"Audio File Details for {new FileInfo(replacementFilePath).Name}:");
            Console.WriteLine($"Size (Bytes): {new FileInfo(replacementFilePath).Length}");

            for (int i = 0; i < fileBytes.Length - original32bit.Length + 1; i++)
            {
                bool found = true;
                for (int j = 0; j < original32bit.Length; j++)
                {
                    if (fileBytes[i + j] != original32bit[j])
                    {
                        found = false;
                        break;
                    }
                }

                if (found)
                {
                    for (int j = 0; j < replacement32bit.Length; j++)
                    {
                        fileBytes[i + j] = replacement32bit[j];
                    }

                }
            }

            File.WriteAllBytes(uexpFilePath, fileBytes);
            //

            Console.WriteLine("Replacement completed. Uexp file updated.");


            string mainOutput = @".\out\";
            string miscOutput = @".\miscOut\";


            createFolder(mainOutput);
            createFolder(miscOutput);

            moveFiles(uexpFilePath, mainOutput);
            moveFiles(ubulkFilePath, mainOutput);
            moveFiles(uassetFilePath, mainOutput);
            moveFiles(audioFilePath, miscOutput);

            File.Delete(replacementFilePath);

            Console.Read();
        }

    }
}