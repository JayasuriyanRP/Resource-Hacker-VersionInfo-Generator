using NDesk.Options;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace ResourceFileGenerator
{
    public class Program
    {
        private const string ResourceHackerEXE = "ResourceHacker.exe";

        public static void Main(string[] args)
        {
            string version = null;
            string companyName = null;
            string fileDescription = null;
            string internalName = null;
            string legalCopyright = null;
            string setupPath = null;
            string logoPath = null;
            bool showHelp = false;
            OptionSet optionSet = new OptionSet()
            {
                { "v|version=", "the {version} of installer.", v =>  version =v},
                { "cn|companyName=", "Installer Company Name", v => companyName = v },
                { "d|fileDescription=", "File Description", v => fileDescription = v },
                { "i|internalName=", "Internal Name of the file", v => internalName = v },
                { "c|legalCopyright=", "Copyright Information", v => legalCopyright = v },
                { "s|setupPath=", "SetupInstallerPath", v => setupPath = v },
                { "l|logoPath=", "Logo for updating", v => logoPath = v },
                { "h|help", "Help Information", v => showHelp = v != null},
            };
            try
            {
                List<string> parser = optionSet.Parse(args);

                if (args.Length > 0)
                {
                    if (logoPath == null)
                    {
                        logoPath = Path.Combine(Directory.GetCurrentDirectory(), @"ResourceHacker\Logo_ABB.ico");
                    }

                    StringBuilder stringBuilder = new StringBuilder();
                    stringBuilder.AppendLine("1 VERSIONINFO");
                    stringBuilder.AppendLine($"FILEVERSION {version.Replace('.', ',')}");
                    stringBuilder.AppendLine($"PRODUCTVERSION {version.Replace('.', ',')}");
                    stringBuilder.AppendLine("FILEOS 0x40004");
                    stringBuilder.AppendLine("FILETYPE 0x1 \n{");
                    stringBuilder.AppendLine("BLOCK \"StringFileInfo\" \n{ \n\tBLOCK \"000004B0\" \n\t{");
                    stringBuilder.AppendLine($"\t\tVALUE \"CompanyName\", \"{companyName}\"");
                    stringBuilder.AppendLine($"\t\tVALUE \"FileDescription\", \"{fileDescription}\"");
                    stringBuilder.AppendLine($"\t\tVALUE \"FileVersion\", \"{version}\"");
                    stringBuilder.AppendLine($"\t\tVALUE \"InternalName\", \"{internalName}\"");
                    stringBuilder.AppendLine($"\t\tVALUE \"LegalCopyright\", \"{legalCopyright}\"");
                    stringBuilder.AppendLine($"\t\tVALUE \"OriginalFilename\", \"{Path.GetFileName(setupPath)}\"");
                    stringBuilder.AppendLine($"\t\tVALUE \"PrivateBuild\", \"{DateTime.Now:yyyy.MMdd}\"");
                    stringBuilder.AppendLine($"\t\tVALUE \"ProductName\", \"{internalName}\"");
                    stringBuilder.AppendLine($"\t\tVALUE \"ProductVersion\", \"{version}\"");
                    stringBuilder.AppendLine($"\t}}\n}}\n");

                    stringBuilder.AppendLine("BLOCK \"VarFileInfo\" \n{");
                    stringBuilder.AppendLine($"\tVALUE \"Translation\", 0x0000 0x04B0 ");
                    stringBuilder.AppendLine($"}}\n}}");

                    string directory = Path.GetDirectoryName(setupPath);
                    string newFileName = $"{Path.GetFileNameWithoutExtension(setupPath)}";

                    if (File.Exists(setupPath))
                    {
                        string rcFile = $"{newFileName}.rc";
                        string rcFilePath = Path.Combine(directory, rcFile);
                        string resFile = $"{newFileName}.res";
                        string resFilePath = Path.Combine(directory, resFile);

                        CreateRCFile(stringBuilder, rcFilePath);
                        CompileRcToResFile(rcFilePath, resFilePath);
                        ReplaceResIntoSetup(resFilePath, setupPath);
                        ReplaceLogoIntoSetup(logoPath, setupPath);

                        if (File.Exists(rcFilePath))
                        {
                            File.Delete(rcFilePath);
                        }
                        if (File.Exists(resFilePath))
                        {
                            File.Delete(resFilePath);
                        }
                    }
                    else
                    {
                        Console.WriteLine("File Doesnt Exits");
                    }
                }
                else
                {
                    Console.WriteLine($"The Arguments passed are incorrect, Please pass the following arguments" +
                        $"\n-FileVersion," +
                        $"\n-CompanyName," +
                        $"\n-FileDescription," +
                        $"\n-InternalName" +
                        $"\n-LegalCopyright" +
                        $"\n-OriginalileName" +
                        $"\n-PrivateBuild");
                }
            }
            catch (OptionException e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine("Try `--help' for more information.");
                return;
            }
        }

        private static void CompileRcToResFile(string rcFilePath, string resFilePath)
        {
            Console.WriteLine("\nCompiling RC file to RES ");
            string compileResourceArgs = $"-open \"{rcFilePath}\" -save \"{resFilePath}\" -action compile -log COMPILE";
            RunResourceHackerProcess(compileResourceArgs);
        }

        private static void CreateRCFile(StringBuilder stringBuilder, string rcFilePath)
        {
            if (File.Exists(rcFilePath))
            {
                File.Delete(rcFilePath);
            }

            using (FileStream fileToWrite = File.Create(rcFilePath, 1024, FileOptions.Asynchronous))
            {
                byte[] textByte = new UTF8Encoding(true).GetBytes(stringBuilder.ToString());
                fileToWrite.Write(textByte, 0, textByte.Length);
            }
        }

        private static void ReplaceLogoIntoSetup(string logoPath, string setupPath)
        {
            Console.WriteLine("\nReplacing Logo");
            string replaceLogoArgs = $"-open \"{setupPath}\" -save \"{setupPath}\" -action add -res \"{logoPath}\" -mask ICONGROUP ,101,1049 -log CONSOLE";

            RunResourceHackerProcess(replaceLogoArgs);
        }

        private static void ReplaceResIntoSetup(string resFilePath, string setupPath)
        {
            Console.WriteLine("\nRemoving Version Info resourcefile 1,1033");

            string removeVersionInfoArgs = $"-open \"{setupPath}\" -save \"{setupPath}\"  -action delete -mask VERSIONINFO,1,1033 -log CONSOLE";
            RunResourceHackerProcess(removeVersionInfoArgs);

            Console.WriteLine("\nAdding Version Info resourcefile 1,1033");
            string replaceVersionInfoArgs = $"-open \"{setupPath}\" -save \"{setupPath}\"  -action addoverwrite -res \"{resFilePath}\" -mask VERSIONINFO,1,0 -log CONSOLE";

            RunResourceHackerProcess(replaceVersionInfoArgs);
        }

        private static void RunResourceHackerProcess(string arguments)
        {
            ProcessStartInfo processStartInfo = new ProcessStartInfo()
            {
                FileName = Path.Combine(Directory.GetCurrentDirectory(), $"ResourceHacker\\{ResourceHackerEXE}"),
                Arguments = arguments,
                UseShellExecute = false
            };
            Process process = new Process()
            {
                StartInfo = processStartInfo
            };
            process.Start();
            process.WaitForExit();
        }
    }
}
