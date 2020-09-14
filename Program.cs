using System;
using System.IO;
using System.Text;

namespace ResourceFileGenerator
{
    public class Program
    {
        public static void Main(string[] args)
        {
            if (args.Length > 0)
            {
                string version = args[0];
                string companyName = args[1];
                string fileDescription = args[2];
                string internalName = args[3];
                string legalCopyright = args[4];
                string originalFileName = args[5];
                string privateBuild = args[6];
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
                stringBuilder.AppendLine($"\t\tVALUE \"OriginalFilename\", \"{originalFileName}\"");
                stringBuilder.AppendLine($"\t\tVALUE \"PrivateBuild\", \"{privateBuild}\"");
                stringBuilder.AppendLine($"\t\tVALUE \"ProductName\", \"{internalName}\"");
                stringBuilder.AppendLine($"\t\tVALUE \"ProductVersion\", \"{version}\"");
                stringBuilder.AppendLine($"\t}}\n}}\n");

                stringBuilder.AppendLine("BLOCK \"VarFileInfo\" \n{");
                stringBuilder.AppendLine($"\tVALUE \"Translation\", 0x0000 0x04B0 ");
                stringBuilder.AppendLine($"}}\n}}");

                string newFilePath = Path.Combine(Directory.GetCurrentDirectory(), $"{internalName}.rc");
                if (File.Exists(newFilePath))
                {
                    File.Delete(newFilePath);
                }

                using (FileStream fileToWrite = File.Create(newFilePath, 1024, FileOptions.Asynchronous))
                {
                    byte[] textByte = new UTF8Encoding(true).GetBytes(stringBuilder.ToString());
                    fileToWrite.Write(textByte, 0, textByte.Length);
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
    }
}
