
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;


namespace Candal
{
    internal class Program
    {
        public static int Main(string[] args)
        {
            string name = Assembly.GetEntryAssembly()?.GetName().Name ?? "";
            string version = Assembly.GetEntryAssembly()?.GetName()?.Version?.ToString() ?? "";

            Console.WriteLine($"{name} ver:{version} - Transform standard resault file of 'linux ls -la' command into 'csv' file.");

            try
            {
                //Validatons
                if ((args.Length < 2) && (args.Length < 3))
                    throw new Exception($"Usage:{name} LsResultInputFile CSVOutputFile initialPath(optional)");

                string inputFile = args[0];
                string outputFile = args[1];
                string prefixPath = string.Empty;
                if (args.Length == 3)
                    prefixPath = args[2];

                //outputFile = "DirOutput.csv";
                //inputFile = "DirInput.txt";
                //initialPath = "/xpto";

                //Process
                ChangeToCsvLinearFormat(inputFile, outputFile, prefixPath);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine("Process aborted.");
                return 1;
            }

            return 0;
        }

        //Set msdos dir format to linux ls  format
        //dir /S Path
        /// linux relation command
        /// ls -l -h -a -p -R Path
        //
        // -l  -- list with long format - show permissions
        // -h  -- list long format with readable file size
        // -a  -- list all files including hidden file starting with '.'
        // -p  -- indicator-style=slash - append '/' indicator to directories
        // -R  -- list recursively directory tree

        private static void ChangeToCsvLinearFormat(string inputFileName, string outputFileName, string prefixPath)
        {
            //-rw-rw-r-- 1 root root 18 May 28 19:48 text_file_1.txt

            Log.Information("ChangeOutputToLinearFormat Started");

            Log.Information($"InputFile={inputFileName}");
            Log.Information($"OutputFile={outputFileName}");
            Log.Information($"PrefixPath={prefixPath}");

            //Stopwatch stopwatch = Utils.GetNewStopwatch();
            //Utils.Startwatch(stopwatch, "LinuxShellHelper", "ChangeOutputToLinearFormat");
            
            if (!File.Exists(inputFileName))
            {
                Log.Error($"Folder Root not exists=[{inputFileName}");
                return;
            }

            StreamReader? streamReader = null;
            StreamWriter? streamWriter = null;

            string? line = null;
            
            try
            {
                if (!File.Exists(inputFileName))
                throw new Exception($"InputFile:'{inputFileName}' not found.");

            if (!CanCreateFile(outputFileName))
                throw new Exception($"OutputFile:'{outputFileName}' cannot be created.");

                streamReader = new StreamReader(inputFileName, System.Text.Encoding.UTF8);
                streamWriter = new StreamWriter(outputFileName, false, System.Text.Encoding.UTF8);

                bool isFolder = false;
                bool hasEndFolderChar = false;
                bool useInitialPath = (prefixPath.Length > 0);
                string basePath = "";
                string rootPath = "";
                string member = "";
                int memberStartAt = 0;

                //get rootPath without last '/'
                if (useInitialPath)
                {
                    if (prefixPath.EndsWith("'/"))
                        rootPath = prefixPath.Substring(0, prefixPath.Length - 1);
                    else
                        rootPath = prefixPath;
                }

                while ((line = streamReader.ReadLine()) != null)
                {
                    if (line.Length == 0)
                        continue;

                    if (line.StartsWith("total "))
                        continue;

                    if (line.StartsWith("."))
                    {
                        if ((line.Length - 2) == 0)
                                basePath = "/";
                        else
                            basePath = line.Substring(1, line.Length - 2) + "/";
                        continue;
                    }

                    if (memberStartAt == 0) //only to improve performance
                    {
                        LinuxLineInfo lineInfo = GetLineInfo(line);
                        memberStartAt = lineInfo.MemberStartAt; //add to improve
                    }

                    member = line.Substring(memberStartAt, line.Length - memberStartAt);
                    char memberType = Char.Parse(line.Substring(0, 1));

                    if (!ValidateType(memberType))
                        throw new Exception($"Error in member type:{line}");

                    isFolder = memberType == 'd';

                    if (isFolder)
                    {
                        if (member == "." || member == ".."
                                || member == "./" || member == "../") //-p
                            continue;
                        hasEndFolderChar = member.EndsWith("/");
                    }

                    //tem custos if (ValidatePermissions(line) > 0)

                    //write
                    string newLine;

                    if (useInitialPath)
                        newLine = $"{rootPath}{basePath}{member}";
                    else
                        newLine = $"{basePath}{member}";

                    streamWriter.WriteLine(newLine);
                    streamWriter.Flush();
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Message Error:{ex.Message}");

                Log.Error($"InputFileName:{inputFileName}");
                Log.Error($"OutoutFileName:{outputFileName}");

                if (line != null)
                    Log.Error($"Line:{line}");
            }
            finally
            {
                if (streamReader != null)
                {
                    streamReader.Close();
                    streamReader.Dispose();
                }
                if (streamWriter != null)
                {
                    //_streamWriter.Flush();
                    streamWriter.Close();
                    streamWriter.Dispose();
                }
            }
        }

        private static bool CanCreateFile(string fileName)
        {
            bool canCreate = false;

            try
            {
                using (File.Create(fileName)) { };
                File.Delete(fileName);
                canCreate = true;
            }
            catch
            {
            }

            return canCreate;
        }

        private static int ValidatePermissions(string line)
        {
            //drwxrwxrwx

            //type 0
            //read 1, 4, 7
            //writ 2, 5, 8
            //exec 3, 6, 9

            if (line.Length < 10)
                return -1;

            char[] attrArray = line.ToCharArray(0, 10);

            //type 0
            if (!ValidateType(attrArray[0]))
                return 1;

            //user read 1
            if (!ValidateRead(attrArray[1]))
                return 2;

            //user write 2
            if (!ValidateWrite(attrArray[2]))
                return 3;

            //user execute 3
            if (!ValidateExecute(attrArray[3]))
                return 4;

            //group read 4
            if (!ValidateRead(attrArray[4]))
                return 5;

            //group write 5
            if (ValidateWrite(attrArray[5]))
                return 6;

            //group execute 6
            if (!ValidateExecute(attrArray[6]))
                return 7;

            //all read
            if (ValidateRead(attrArray[7]))
                return 8;

            //all write
            if (ValidateWrite(attrArray[8]))
                return 9;

            //all execute
            if (!ValidateWrite(attrArray[9]))
                return 10;

            return 0;
        }

        //-: Um arquivo regular
        //d: Um diretório
        //l: Um link simbólico
        //c: Um arquivo de dispositivo de caractere
        //b: Um arquivo de dispositivo de bloco
        //s: Uma tomada
        //p: Um pipe nomeado(FIFO)

        private const string attrType = "-lcdsp";
        private const string attrRead = "-r";
        private const string attrWrite = "-w";
        private const string attrExecute = "-x";

        private static bool ValidateType(char chr)
        {
            return attrType.Contains(chr);
        }

        private static bool ValidateRead(char chr)
        {
            return attrRead.Contains(chr);
        }

        private static bool ValidateWrite(char chr)
        {
            return attrWrite.Contains(chr);
        }

        private static bool ValidateExecute(char chr)
        {
            return attrExecute.Contains(chr);
        }

        private static LinuxLineInfo GetLineInfo(string line)
        {
            //-rwxrwxrwx 1 root root         69 Apr  3 23:34 file name.txt

            int count = 0;
            int posI = 0;
            int posE = 0;

            var lineInfo = new LinuxLineInfo();

            while (count < 9)
            {
                posI = NextNonSpace(line, posI);
                posE = NextSpace(line, posI + 1);

                switch (count)
                {
                    case 0:
                        lineInfo.Attributes = line.Substring(posI, posE - posI);
                        break;
                    case 1:
                        lineInfo.LinksNumber = line.Substring(posI, posE - posI);
                        break;
                    case 2:
                        lineInfo.User = line.Substring(posI, posE - posI);
                        break;
                    case 3:
                        lineInfo.Group = line.Substring(posI, posE - posI);
                        break;
                    case 4:
                        lineInfo.Size = line.Substring(posI, posE - posI);
                        break;
                    case 5: // for datetime remove case 6 and 7 passing to 6
                        lineInfo.Month = line.Substring(posI, posE - posI);
                        break;
                    case 6:
                        lineInfo.Day = line.Substring(posI, posE - posI);
                        break;
                    case 7:
                        lineInfo.Hour = line.Substring(posI, posE - posI);
                        break;
                    case 8:
                        //posI++;
                        lineInfo.Member = line.Substring(posI, line.Length - posI);
                        lineInfo.MemberStartAt = posI;
                        break;
                    default:
                        throw new Exception("ErrorEventArgs in Process line");
                }

                count++;
                posI = posE;
            };

            //lineInfo.MemberStartAt = posI;

            return lineInfo;
        }

        private static int NextNonSpace(string line, int startAt)
        {
            int pos = startAt;
            while ((pos < line.Length) && (line[pos] == ' '))
            {
                pos++;
            }

            return pos;
        }

        private static int NextSpace(string line, int startAt)
        {
            int pos = startAt;
            while ((pos < line.Length) && (line[pos] != ' '))
            {
                pos++;
            }

            return pos;
        }

        public class Constants
        {
            public static readonly System.Text.Encoding StreamsEncoding = System.Text.Encoding.UTF8;
        }
    }
}