//using Microsoft.VisualBasic;
//using System;
//using System.ComponentModel.Design;
//using System.Diagnostics;
//using System.IO;

//namespace Candal
//{
//    internal class Program3
//    {
//        internal static readonly System.Text.Encoding StreamsEncoding = System.Text.Encoding.UTF8;


//        private static StreamWriter? _streamWriter;

//        public static int Main3(string[] args)
//        {
//            Console.WriteLine("@Program Started...");

//            string rootPath = @"\\NAS-QNAP\music\_COLLECTION";
//            string dirOutput = "DirOutput.txt";
//            string dirTemp = "DirTemp.txt";

//            //rootPath = @"\\NAS-QNAP\movies";
//            /////////////////////////////////////
//            ///
//            switch(args.Length)
//            {
//                case 1:
//                    rootPath = args[0];
//                    break;
//                case 2:
//                    rootPath = args[1];
//                    break;
//                case 3:
//                    rootPath = args[1];
//                    dirOutput = args[1];
//                    break;
//            }

//            if (args.Length < 1 || args.Length > 3)
//            {
//                Console.WriteLine($"Usage folderName");
//                Console.WriteLine($"Usage folderName outputFile");
//                Console.WriteLine($"Usage /S folderName (/S recursive search");
//                Console.WriteLine($"Usage /S folderName outputFile (/S recursive search");
//                return 1;
//            }

//            bool isRecursive = (args[0].ToUpper() == "/S");

//            if (isRecursive)
//            {

//            }

//                //TODO: sinalizar

//                rootPath = args[0];
//                dirOutput = args[1];
               
            
//            dirTemp = $"{dirOutput}.tmp";

//            ////////////////////////////////////                        

//            string dirTempFile = Path.Combine(rootPath, dirTemp);
//            string dirOutputFile = Path.Combine(rootPath, dirOutput);

//            Console.WriteLine($"DIR Folder:{rootPath}");
//            Console.WriteLine($"Temp File:{dirTempFile}");
//            Console.WriteLine($"Output File:{dirOutputFile}");

//            if (!Directory.Exists(rootPath))
//            {
//                Console.WriteLine($"Error: DIR Folder Not Found.");
//                return 1;
//            }

//            //MSDOS command
//            //note: is 'nul'  not 'null' 
//            string msDosCommand = $"chcp 65001>nul & dir /S {rootPath}"; 
//            //string msDosCommand = $"chcp 65001>nul & dir /S /A:D {rootPath}";
//            //string msDosCommand = $"chcp 65001>nul & dir /S /A:-D {rootPath}";

//            //MsDosProcessSimple(msDosCommand, dirTempFile);
//            MsDosProcess(msDosCommand, dirTempFile);

//            ChangeOutputToLinearFormat(dirTempFile, dirOutputFile);

//            if (File.Exists(dirTempFile))
//                File.Delete(dirTempFile);

//            Console.WriteLine("@Programs Finished...");

//            return 0;
//        }


//        /// Process 
//        private static void MsDosProcess(string msDosCommand, string dirTempFile)
//        {
//            Console.WriteLine("@MsDosProcess Started...");
//            Console.WriteLine($"Output File:{dirTempFile}");

//            try
//            {
//                //output
//                _streamWriter = new StreamWriter(dirTempFile, false, Program.StreamsEncoding);

//                //Process Info
//                var startInfo = new ProcessStartInfo();
//                startInfo.FileName = "cmd.exe";
//                startInfo.Arguments = $"/C {msDosCommand}";

//                //dos without window
//                //startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
//                //startInfo.UseShellExecute = false;
//                //startInfo.CreateNoWindow = false;

//                //redirect output to files
//                startInfo.RedirectStandardOutput = true;
//                startInfo.RedirectStandardError = true;

//                //Process
//                Process process = new();
//                process.StartInfo = startInfo;

//                //V1
//                process.OutputDataReceived += OutputDataReceived;
//                process.ErrorDataReceived += ErrorDataReceived;

//                //V2 - with lambda (i dont like)
//                //process.OutputDataReceived += (sender, args) =>
//                //{
//                //    _streamWriter.WriteLine(args.Data);
//                //    _streamWriter.Flush();
//                //};

//                //process.OutputDataReceived += (sender, args) =>
//                //{
//                //    _streamWriter.WriteLine("ERROR:" + args.Data);
//                //    _streamWriter.Flush();
//                //};


//                //Start
//                process.Start();
//                process.BeginOutputReadLine(); //important to file output 
//                process.WaitForExit();
//            }
//            catch (Exception ex)
//            {
//                Console.WriteLine($"'MsDosProcess' ERROR:{ex.Message}");
//                throw;
//            }
//            finally
//            {
//                if (_streamWriter != null)
//                {
//                    _streamWriter.Flush();
//                    _streamWriter.Close();
//                }
//            }

//            Console.WriteLine("@MsDosProcess Finished...");
//        }

//        private static void ErrorDataReceived(object sender, DataReceivedEventArgs e)
//        {
//            if (_streamWriter == null)
//                return;

//            _streamWriter.WriteLine("ERROR:" + e.Data);
//            _streamWriter.Flush();
//        }

//        private static void OutputDataReceived(object sender, DataReceivedEventArgs e)
//        {
//            if (_streamWriter == null)
//                return;

//            _streamWriter.WriteLine(e.Data);
//            _streamWriter.Flush();
//        }

//        /// Process Simple
//        private static void MsDosProcessSimple(string msDosCommand, string dirTempFile)
//        {
//            try
//            {
//                //Process Info
//                var startInfo = new ProcessStartInfo();
//                startInfo.FileName = "cmd.exe";
//                startInfo.Arguments = $"/C {msDosCommand}";

//                //dos without window
//                //startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
//                //startInfo.UseShellExecute = false;
//                //startInfo.CreateNoWindow = false;

//                //Process
//                Process process = new();
//                process.StartInfo = startInfo;

//                //Start
//                process.Start();
//                process.WaitForExit();
//            }
//            catch (Exception ex)
//            {
//                Console.WriteLine($"'MsDosProcess' ERROR:{ex.Message}");
//            }
//        }

//        //Set msdos dir format to linux ls  format
//        //dir /S Path
//        //linux relation command
//        // ls -l -h -a -p -R Path
//        //
//        // -l  -- list with long format - show permissions
//        // -h  -- list long format with readable file size
//        // -a  -- list all files including hidden file starting with '.'
//        // -p  -- indicator-style=slash - append '/' indicator to directories
//        // -R  -- list recursively directory tree
//        private static void ChangeOutputToLinearFormat(string dirInputFile, string dirOutputFile)
//        { 
//            Console.WriteLine("@ChangeOutputToLinearFormat Started...");
//            Console.WriteLine($"Input File :{dirInputFile}");
//            Console.WriteLine($"Output File:{dirOutputFile}");

//            int countFolders = 0;
//            int countFiles = 0;
//            StreamReader? streamReader = null;
//            StreamWriter? streamWriter = null;

//            try
//            {
//                if (!File.Exists(dirInputFile))
//                {
//                    Console.WriteLine("Output File Not Found.");
//                    return;
//                }

//                string? line = null;

//                //using (StreamReader reader = new StreamReader(fileName)) //C# 8
//                //{
//                //}
//                //using var _streamReader = new StreamReader(_fullFileNameTemp, Constants.StreamsEncoding);

//                streamReader = new StreamReader(dirInputFile, Program.StreamsEncoding);
//                streamWriter = new StreamWriter(dirOutputFile, false, Program.StreamsEncoding);

//                bool isFolder = false;
//                string baseDir = "";
//                string item;
//                char dirMark = Path.DirectorySeparatorChar;

//                while ((line = streamReader.ReadLine()) != null)
//                {
//                    if (line.Length < 14) //less than phrase " Directory of "
//                        continue;

//                    if (line.Substring(0, 14) == " Directory of ") //new directory
//                    {
//                        baseDir = line.Substring(14);
//                        continue;
//                    }

//                    if (line.Length < 37) //less than phrase "2022/09/21  22:53    <DIR>          "
//                        continue;

//                    if (!DateTime.TryParse(line.Substring(0, 10), out DateTime dt))
//                        continue;

//                    isFolder = (line.Substring(21, 5) == "<DIR>");

//                    item = line.Substring(36);

//                    if (isFolder && (item == ".") || (item == ".."))
//                        continue;

//                    string newLine = $"{baseDir}{Path.DirectorySeparatorChar}{item}";

//                    if (isFolder)
//                        newLine += dirMark;

//                    streamWriter.WriteLine(newLine);
//                    streamWriter.Flush();

//                    if (isFolder)
//                        countFolders++;
//                    else
//                        countFiles++;
//                }
//            }
//            catch (Exception ex)
//            {
//                Console.WriteLine($"'ChangeOutputToLinearFormat' ERROR:{ex.Message}");
//                throw;
//            }
//            finally
//            {
//                if (streamReader != null)
//                {
//                    streamReader.Close();
//                    streamReader.Dispose();
//                }
//                if (streamWriter != null)
//                {
//                    streamWriter.Close();
//                    streamWriter.Dispose();
//                }
//            }

//            Console.WriteLine($"Counted Folders:{countFolders}");
//            Console.WriteLine($"Counted Files  :{countFiles}");

//            Console.WriteLine("@ChangeOutputToLinearFormat Finished...");
//        }

//    }
//}
