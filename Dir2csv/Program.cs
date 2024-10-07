
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Reflection;


namespace Candal
{
    internal class Program
    {
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

        public static int Main(string[] args)
        {
            int count = 0;


            ChangeOutputToLinearFormatLinux(@"E:\Torrents10\lsp.txt", @"E:\Torrents10\lsp_end.txt", "/root/pmfg");



            string dirOutputFile = "DirOutput.txt";
            string dirInputFile = "DirInput.txt";

            //Validatons
            if (args.Length != 2)
            {
                string Name = Assembly.GetEntryAssembly().GetName().Name;
                Console.WriteLine($"Transform the output of 'msdos dir' into 'linux ls'.");
                Console.WriteLine($"Usage:{Name} InputFile, OutputFile");
                return 1;
            }

            dirInputFile = args[0];
            dirOutputFile = args[1];

            if (!File.Exists(dirInputFile))
            {
                Console.WriteLine($"InputFile:{dirInputFile} not found.");
                return 1;
            }

            if (!CanCreateFile(dirOutputFile))
            {
                Console.WriteLine($"OutputFile:{dirOutputFile} cannot be created.");
                return 1;
            }

            //Process
            try
            {
                ChangeOutputToLinearFormat(dirInputFile, dirOutputFile);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR:{ex.Message}");
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
        private static void ChangeOutputToLinearFormat(string dirInputFile, string dirOutputFile)
        {
            // Console.WriteLine("@ChangeOutputToLinearFormat Started...");
            Console.WriteLine($"InputFile={dirInputFile}");
            Console.WriteLine($"OutputFile={dirOutputFile}");

            int countFolders = 0;
            int countFiles = 0;
            StreamReader streamReader = null;
            StreamWriter streamWriter = null;

            try
            {
                string line = null;

                streamReader = new StreamReader(dirInputFile, System.Text.Encoding.UTF8);
                streamWriter = new StreamWriter(dirOutputFile, false, System.Text.Encoding.UTF8);

                bool isFolder = false;
                string baseDir = "";
                string item;
                char dirMark = Path.DirectorySeparatorChar;

                while ((line = streamReader.ReadLine()) != null)
                {
                    if (line.Length < 14) //less than phrase " Directory of "
                        continue;

                    if (line.Substring(0, 14) == " Directory of ") //new directory
                    {
                        baseDir = line.Substring(14);
                        continue;
                    }

                    if (line.Length < 37) //less than phrase "2022/09/21  22:53    <DIR>          "
                        continue;

                    if (!DateTime.TryParse(line.Substring(0, 10), out DateTime dt))
                        continue;

                    isFolder = (line.Substring(21, 5) == "<DIR>");

                    item = line.Substring(36);

                    if (isFolder && (item == ".") || (item == ".."))
                        continue;

                    string newLine = $"{baseDir}{Path.DirectorySeparatorChar}{item}";

                    if (isFolder)
                        newLine += dirMark;

                    streamWriter.WriteLine(newLine);
                    streamWriter.Flush();

                    if (isFolder)
                        countFolders++;
                    else
                        countFiles++;
                }
            }
            catch //(Exception ex)
            {
                throw;
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
                    streamWriter.Close();
                    streamWriter.Dispose();
                }
            }

            Console.WriteLine($"Counted Folders:{countFolders}");
            Console.WriteLine($"Counted Files  :{countFiles}");
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






        private static void ChangeOutputToLinearFormatLinux(string _fullFileNameTemp, string _fullFileNameOut, string initialPath = "")
        {
            //-rw-rw-r-- 1 root root 18 May 28 19:48 text_file_1.txt

            //Log.Information("LinuxShellHelper.ChangeOutputToLinearFormat Started");

            //Stopwatch stopwatch = Utils.GetNewStopwatch();
            //Utils.Startwatch(stopwatch, "LinuxShellHelper", "ChangeOutputToLinearFormat");

            ////_fullFileNameTemp = @"E:\_MEGA_DRIVE\__GitHub\__Synchronized\C_Sharp\MusicCollectionList\MusicCollectionLinuxShell\putty_lossless.txt";
            ////_fullFileNameOut = @"E:\_MEGA_DRIVE\__GitHub\__Synchronized\C_Sharp\MusicCollectionList\MusicCollectionLinuxShell\putty_lossless_new.txt";
            ////_applyExtensionsFilter = false;

            //if (!File.Exists(_fullFileNameTemp))
            //{
            //    Log.Error($"Folder Root not exists=[{_fullFileNameTemp}");
            //    return;
            //}

            StreamReader _streamReader = null;
            StreamWriter _streamWriter = null;

            string line = null;

            try
            {
                _streamReader = new StreamReader(_fullFileNameTemp, Constants.StreamsEncoding);
                _streamWriter = new StreamWriter(_fullFileNameOut, false, Constants.StreamsEncoding);

                bool isFolder = false;
                bool hasEndFolderChar = false;
                bool useInitialPath = (initialPath.Length > 0);
                string basePath = "";
                string rootPath = "";
                string member = "";
                int memberStartAt = 0;

                //get rootPath without last '/'
                if (useInitialPath)
                {
                    if (initialPath.EndsWith("'/"))
                        rootPath = initialPath.Substring(0, initialPath.Length - 1);
                    else
                        rootPath = initialPath;
                }

                while ((line = _streamReader.ReadLine()) != null)
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
                    //if (isValid)
                    //{
                    //_rootPath
                    string newLine;

                    //newLine = System.IO.Path.Join(_rootPath, basePath, member);
                    //newLine = System.IO.Path.Combine(initialPath, basePath, member);
                    if (useInitialPath)
                        newLine = $"{rootPath}{basePath}{member}"; //{Path.DirectorySeparatorChar}
                    else
                        newLine = $"{basePath}{member}"; //{Path.DirectorySeparatorChar}

                    Console.WriteLine(newLine);
                    _streamWriter.WriteLine(newLine);
                    _streamWriter.Flush();
                    //}
                }
            }
            catch (Exception ex)
            {
                if (line != null)
                    Log.Error($"Line:{line}");
                Log.Error($"Outout:{_fullFileNameOut}");
                Log.Error($"Message Error:{ex.Message}");
            }
            finally
            {
                if (_streamReader != null)
                {
                    _streamReader.Close();
                    _streamReader.Dispose();
                }
                if (_streamWriter != null)
                {
                    //_streamWriter.Flush();
                    _streamWriter.Close();
                    _streamWriter.Dispose();
                }
            }
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

            //string attrs = line.Substring(0, 10);
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

        //    private static string GetMember(string text)
        //    {
        //        int count = 0;
        //        int pos = 0;
        //        pos = text.IndexOf(" ", pos);

        //        //permissiona

        //        if (text == null)
        //        {
        //            //        return "";

        //            //    string result = "";
        //            //    foreach (char c in text)
        //            //    {
        //            //        if (c != ' ')
        //            //        bar += c;
        //        }
        //        return "";
        //    }
        //}

        public class Constants
        {
            public static readonly System.Text.Encoding StreamsEncoding = System.Text.Encoding.UTF8;
        }
        public class Log
        {
            public static void Error(string a) { }
        }

        internal class LinuxLineInfo
        {
            public string Attributes { get; set; }
            public string LinksNumber { get; set; }
            public string User { get; set; }
            public string Group { get; set; }
            public string Size { get; set; }
            public string Month { get; set; }
            public string Day { get; set; }
            public string Hour { get; set; }
            public string Date { get; set; }
            public string Member { get; set; }
            public int MemberStartAt { get; set; }
        }
    }
}