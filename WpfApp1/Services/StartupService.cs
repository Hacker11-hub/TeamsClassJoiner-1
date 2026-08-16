using Microsoft.Win32;
using System;

namespace TeamsClassJoiner.Services;

public class StartupService
{
    private const string AppName = "TeamsClassJoiner";

    private const string RegistryPath =
        @"Software\Microsoft\Windows\CurrentVersion\Run";

    public bool IsEnabled()
    {
        using RegistryKey? key =
            Registry.CurrentUser.OpenSubKey(RegistryPath);

        return key?.GetValue(AppName) != null;
    }

    public void SetEnabled(bool enabled)
    {
        using RegistryKey key =
            Registry.CurrentUser.OpenSubKey(
                RegistryPath,
                true)
            ?? Registry.CurrentUser.CreateSubKey(
                RegistryPath);

        if (enabled)
        {
            string exePath =
                Environment.ProcessPath
                ?? throw new InvalidOperationException(
                    "Unable to determine application path.");

            key.SetValue(AppName, $"\"{exePath}\"");
        }
        else
        {
            key.DeleteValue(
                AppName,
                false);
        }
    }
}