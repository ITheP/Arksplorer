using System;
using System.Diagnostics;
using System.IO;
using System.Windows;

namespace Arksplorer
{
    /// <summary>
    /// Error handling functionality, including logging to crash file
    /// </summary>
    public static class Errors
    {
        public static string CrashFile { get; } = "./crash.txt";

        public static void AddToCrashFile(Exception ex, string message = null)
        {
            var trace = new StackTrace(ex);
            var frame = trace.GetFrame(0);
            var method = frame.GetMethod();
            string location = $"{method.DeclaringType.FullName}.{method.Name} ({frame.GetFileLineNumber()})";

            AddToCrashFile($"{(message == null ? "" : $"{message}{Environment.NewLine}")}" +
                $"{location}{Environment.NewLine}" +
                $"Exception:{Environment.NewLine}{ex.Message}" +
                $"{(ex.InnerException != null ? $"{Environment.NewLine}Inner exception:{Environment.NewLine}{ex.InnerException.Message}" : "")}");
        }

        public static void AddToCrashFile(string content)
        {
            try
            {
                content = $"===[Arksplorer {Globals.Version} @ {DateTime.Now}]================================{Environment.NewLine}" +
                    $"{content}{Environment.NewLine}";

                File.AppendAllText(CrashFile, content);

                Debug.Print(content);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Unable to write log to {CrashFile}. Not going so well is it!{Environment.NewLine}{ex.Message}", "Error logging an error!");
            }
        }

        public static void ReportProblem(string description, string postDescription = null, string caption = null)
        {
            MessageBox.Show($"{description}" +
                $"{(postDescription != null ? $"{Environment.NewLine}{Environment.NewLine}{postDescription}" : "")}",
                caption ?? "Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);

            AddToCrashFile(description);
        }

        public static void ReportProblem(Exception ex, string description, string postDescription = null, string caption = null)
        {
            MessageBox.Show($"{description}" +
                $"{(ex != null ? $"{Environment.NewLine}{Environment.NewLine}Error details:{Environment.NewLine}{ex.Message}" : "")}" +
                $"{(postDescription != null ? $"{Environment.NewLine}{Environment.NewLine}{postDescription}" : "")}",
                caption ?? "Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);

            AddToCrashFile(ex, description);
        }
    }
}
