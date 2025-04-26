using QRay.Utility.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace QRay.Utility
{
    public static class Logger
    {
        private static readonly string logPath = "Logs";

        public static void Log(
            string message, 
            LogLevel level,
            [CallerMemberName] string memberName = "Unknown member",
            [CallerFilePath] string fileName = "Unknown file",
            [CallerLineNumber] int lineNumber = 0)
        {
            // Getting the log folder's path
            string root = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, @"..\..\.."));
            string folderPath = Path.Combine(root, logPath);

            // Make sure that the directory exists
            Directory.CreateDirectory(folderPath);

            // Logging the message to the file
            string path = Path.Combine(folderPath, $"{Path.GetFileNameWithoutExtension(fileName)}.log");

            string formattedMessage = $" - [{level}] {DateTime.Now:yyyy-MM-dd HH:mm:ss} # {memberName}:{lineNumber}\n{PadMessage(message, 3)}\n";

            File.AppendAllText(path, NormalizeEnds(formattedMessage));
        }

        private static string PadMessage(string message, int padding)
        {
            string indent = new string(' ', padding);
            return string.Join("\n",
                message.Split('\n').Select(line => indent + line)
            );
        }

        private static string NormalizeEnds(string message)
        {
            return message.Replace("\r\n", "\n").Replace("\n", Environment.NewLine);
        }
    }
}
