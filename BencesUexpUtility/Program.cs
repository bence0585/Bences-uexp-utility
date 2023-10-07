using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace BencesUexpUtility
{
    internal class Program
    {
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

            if (!(args.Length > 0))
            {
                Console.WriteLine("Drag and drop an MP3 file onto this executable to read its contents and open the related text file.");
                Console.Read();
                return;
            }
            string audioFilePath = args[0];
            FileInfo audioFileInfo = new FileInfo(audioFilePath);

            if (!(File.Exists(audioFilePath)))
            {
                Console.WriteLine($"Music file not found: {audioFilePath}");
                Console.Read();
                return;
            }
            string[] acceptedExtensions = { ".mp3", ".wav", ".flac", ".ogg", ".webm" };
            if (!(acceptedExtensions.Contains(audioFileInfo.Extension)))
            {
                Console.Read();
                return;
            }

            string ffmpegFileName = "ffmpeg.exe";
            bool isFfmpegInstalled = File.Exists(ffmpegFileName);

            if (!isFfmpegInstalled)
            {
                Console.WriteLine("FFmpeg is not installed. You need to put ffmpeg.exe besides the utility exe for it to work.");
            }
            Console.WriteLine("FFmpeg is installed.");

            string ffmpegPath = @"ffmpeg.exe";

            string inputFilePath = audioFilePath;
            string outputFilePath = Path.ChangeExtension(audioFilePath, "re.wav");

            string ffmpegArgs = $"-hide_banner -loglevel error -y -i \"{inputFilePath}\" -map_metadata -1 -fflags +bitexact -flags:a +bitexact -flags:v +bitexact -vn -c:a pcm_s16le \"{outputFilePath}\" ";


            try
            {
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = ffmpegPath,
                    Arguments = ffmpegArgs,
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

            FileInfo ubulkFileInfo = new FileInfo(ubulkFilePath);
            byte[] original64bit = BitConverter.GetBytes(ubulkFileInfo.Length);

            string wwiseFileName = "";


            if (File.Exists("wwise.exe"))
            {
                wwiseFileName = "wwise.exe";
            }
            else if (File.Exists("wwise_pd3.exe"))
            {
                wwiseFileName = "wwise_pd3.exe";
            }
            else
            {
                Console.WriteLine("Wwise is not installed. You need to put wwise.exe besides the utility exe for it to work.");
                Console.Read();
                return;
            }
            Console.WriteLine("wwise is installed.");

            inputFilePath = outputFilePath;
            string replacementFilePath = Path.ChangeExtension(audioFilePath, ".ubulk");
            string wwiseArgs = $"-encode  \"{inputFilePath}\"  \"{replacementFilePath}\"";

            try
            {
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = wwiseFileName,
                    Arguments = wwiseArgs,
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

            FileInfo replacementFileInfo = new FileInfo(replacementFilePath);

            Console.WriteLine($"UBULK File Details for {ubulkFilePath}:");
            Console.WriteLine($"Size (Bytes): {ubulkFileInfo.Length}");

            Console.WriteLine($"Audio File Details for {replacementFilePath}:");
            Console.WriteLine($"Size (Bytes): {replacementFileInfo.Length}");

            byte[] fileBytes = File.ReadAllBytes(uexpFilePath);

            byte[] replacement64bit = BitConverter.GetBytes(replacementFileInfo.Length);

            byte[] original32bit = new byte[4];
            Array.Copy(original64bit, original32bit, 4);
            byte[] replacement32bit = new byte[4];
            Array.Copy(replacement64bit, replacement32bit, 4);

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

            Console.WriteLine("Replacement completed. Uexp file updated.");

            string directoryPath = @".\out\";

            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            Console.WriteLine(directoryPath + new FileInfo(uexpFilePath).Name);

            
            moveFiles(uexpFilePath,directoryPath);
            moveFiles(ubulkFilePath,directoryPath);
            moveFiles(uassetFilePath,directoryPath);
            File.Delete(outputFilePath);
            Console.Read();
        }

    }
}