using System;
using System.IO;

namespace TeamsClassJoiner.Services;

public static class Logger
{
    private static readonly object _lock = new object();
    private static readonly string _logFile = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "TeamsClassJoiner",
        "automation.log");

    public static void Log(string message)
    {
        try
        {
            string dir = Path.GetDirectoryName(_logFile) ?? string.Empty;
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            string line = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}" + Environment.NewLine;

            lock (_lock)
            {
                File.AppendAllText(_logFile, line);
            }
        }
        catch
        {
            // ignore logging failures
        }
    }
}
