
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
        public static int Main(string[] args)
        {
            string name = Assembly.GetEntryAssembly().GetName().Name;
            string version = Assembly.GetEntryAssembly().GetName().Version.ToString();

            Console.WriteLine($"{name} {version} - Transform standard resault file of 'msdos dir' command into 'csv' file.");

            try
            {
                //Validatons
                if (args.Length != 2)
                    throw new Exception($"Usage:{name} DirResultInputFile, CSVOutputFile");

                string inputFile = args[0];
                string outputFile = args[1];

                //outputFile = "DirOutput.csv";
                //inputFile = "DirInput.txt";

                //Process
                ChangeToCsvLinearFormat(inputFile, outputFile);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine("Process aborted.");
                return 1;
            }

            return 0;
        }

        //Typical input:
        //  dir Path
        //  dir /S Path
        private static void ChangeToCsvLinearFormat(string inputFile, string outputFile)
        {
            Console.WriteLine($"InputFile={inputFile}");
            Console.WriteLine($"OutputFile={outputFile}");

            StreamReader streamReader = null;
            StreamWriter streamWriter = null;

            string line = null;
            int countFolders = 0;
            int countFiles = 0;

            try
            {
                if (!File.Exists(inputFile))
                    throw new Exception($"InputFile:'{inputFile}' not found.");

                if (!CanCreateFile(outputFile))
                    throw new Exception($"OutputFile:'{outputFile}' cannot be created.");

                bool isFolder = false;
                bool isValid = true;
                string baseDir = "";
                string item;

                streamReader = new StreamReader(inputFile, System.Text.Encoding.UTF8);
                streamWriter = new StreamWriter(outputFile, false, System.Text.Encoding.UTF8);

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

                    //Verify Context Filter
                    //Apply Extensions Filter

                    //write
                    if (isValid)
                    {
                        string newLine = $"{baseDir}{Path.DirectorySeparatorChar}{item}";

                        if (isFolder)
                            newLine += Path.DirectorySeparatorChar;

                        streamWriter.WriteLine(newLine);
                        streamWriter.Flush();
                    }

                    //counters
                    if (isFolder)
                        countFolders++;
                    else
                        countFiles++;
                }
            }
            catch
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

            Console.WriteLine($"Total Folders:{countFolders}");
            Console.WriteLine($"Total Files  :{countFiles}");
            Console.WriteLine($"Total        :{countFolders + countFiles}");
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
    }
}