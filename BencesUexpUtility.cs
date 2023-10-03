using System;
using System.Diagnostics;
using System.IO;

namespace BencesUexpUtility
{
    internal class Program
    {
        static void Main(string[] args)
        {

            //Checking if a file was provided either by drag&drop or by cli

            if (!(args.Length > 0))
            {
                Console.WriteLine("Drag and drop an MP3 file onto this executable to read its contents and open the related text file.");
                Console.Read();
                return;
            }
            string audioFilePath = args[0];
            FileInfo audioFileInfo = new FileInfo(audioFilePath);

            // Check if the file exists
            if (!(File.Exists(audioFilePath)))
            {
                Console.WriteLine($"Music file not found: {audioFilePath}");
                Console.Read();
                return;
            }
            //Checking the extensions of the file
            if (!(audioFileInfo.Extension == ".mp3" || audioFileInfo.Extension == ".wav"))
            {
                Console.Read();
                return;
            }

            //Checking if ffmpeg is available
            string ffmpegFileName = "ffmpeg.exe"; // The name of the FFmpeg executable

            // Check if FFmpeg is found in the current directory or specified directory
            bool isFfmpegInstalled = File.Exists(ffmpegFileName);

            if (!isFfmpegInstalled)
            {
                Console.WriteLine("FFmpeg is not installed. You need to put ffmpeg.exe besides the utility exe for it to work.");
            }
            Console.WriteLine("FFmpeg is installed.");


            // Path to the FFmpeg executable
            string ffmpegPath = @"ffmpeg.exe"; // Replace with the actual path to ffmpeg.exe

            // Input and output file paths
            string inputFilePath = audioFilePath;     // Replace with your input file path
            string outputFilePath = Path.ChangeExtension(audioFilePath, ".wav");   // Replace with your desired output file path

            // FFmpeg command with arguments
            string ffmpegArgs = $"-hide_banner -loglevel error -y -i \"{inputFilePath}\" -c:a pcm_s16le \"{outputFilePath}\"";

            try
            {
                // Create a ProcessStartInfo instance to configure the FFmpeg process
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = ffmpegPath,
                    Arguments = ffmpegArgs,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = false
                };

                // Start the FFmpeg process
                Process ffmpegProcess = new Process { StartInfo = startInfo };
                ffmpegProcess.Start();

                // Optionally, wait for the process to finish
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
            FileInfo replacementFileInfo = new FileInfo(outputFilePath);

            // Display file details
            Console.WriteLine($"UBULK File Details for {uexpFilePath}:");
            Console.WriteLine($"Size (Bytes): {ubulkFileInfo.Length}");

            Console.WriteLine($"Audio File Details for {audioFilePath}:");
            Console.WriteLine($"Size (Bytes): {replacementFileInfo.Length}");

            byte[] fileBytes = File.ReadAllBytes(uexpFilePath);

            byte[] original64bit = BitConverter.GetBytes(ubulkFileInfo.Length);
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
                    // Replace the original number with the replacement number
                    for (int j = 0; j < replacement32bit.Length; j++)
                    {
                        fileBytes[i + j] = replacement32bit[j];
                    }

                }
            }

            File.WriteAllBytes(uexpFilePath, fileBytes);

            Console.WriteLine("Replacement completed. Uexp file updated.");



            //Checking if ffmpeg is available
            string wwiseFileName = "wwise.exe"; // The name of the FFmpeg executable

            // Check if FFmpeg is found in the current directory or specified directory
            bool isWwiseinstalled = File.Exists(wwiseFileName);

            if (!isWwiseinstalled)
            {
                Console.WriteLine("Wwise is not installed. You need to put wwise.exe besides the utility exe for it to work.");
            }
            Console.WriteLine("wwise is installed.");


            // Path to the FFmpeg executable
            string wwiseFilePath = @"wwise.exe"; // Replace with the actual path to ffmpeg.exe

            // Input and output file paths
            inputFilePath = outputFilePath;     // Replace with your input file path
            outputFilePath = Path.ChangeExtension(audioFilePath, ".ubulk");   // Replace with your desired output file path

            // FFmpeg command with arguments
            string wwiseArgs = $"-encode  \"{inputFilePath}\"  \"{outputFilePath}\"";

            try
            {
                // Create a ProcessStartInfo instance to configure the FFmpeg process
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = wwiseFilePath,
                    Arguments = wwiseArgs,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = false
                };

                // Start the wwise process
                Process wwiseProcess = new Process { StartInfo = startInfo };
                wwiseProcess.Start();

                // Optionally, wait for the process to finish
                wwiseProcess.WaitForExit();

                Console.WriteLine("Wwise encode process completed successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error running wwise.exe: {ex.Message}");
            }

            string directoryPath = @".\out\";

            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            Console.WriteLine(directoryPath + new FileInfo(uexpFilePath).Name);

            File.Move(uexpFilePath, directoryPath + new FileInfo(uexpFilePath).Name);
            File.Move(ubulkFilePath, directoryPath + new FileInfo(ubulkFilePath).Name);
            File.Move(uassetFilePath, directoryPath + new FileInfo(uassetFilePath).Name);




            Console.Read();
        }

    }
}
