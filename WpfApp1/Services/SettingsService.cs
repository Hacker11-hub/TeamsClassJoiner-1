using System;
using System.IO;
using System.Text.Json;

namespace TeamsClassJoiner.Services;

public class AppSettings
{
    public bool AutoJoinEnabled { get; set; } = true;

    // Timeout in seconds to wait for Teams window to appear
    public int AutoJoinTimeoutSeconds { get; set; } = 60;

    public bool AutoJoinOnStartup { get; set; } = false;
}

public static class SettingsService
{
    private static readonly string _settingsFile = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "TeamsClassJoiner",
        "settings.json");

    private static AppSettings? _settings;

    public static AppSettings Load()
    {
        if (_settings != null)
            return _settings;

        try
        {
            if (!File.Exists(_settingsFile))
            {
                _settings = new AppSettings();
                Save(_settings);
                return _settings;
            }

            string json = File.ReadAllText(_settingsFile);
            _settings = JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
            return _settings;
        }
        catch
        {
            _settings = new AppSettings();
            return _settings;
        }
    }

    public static void Save(AppSettings settings)
    {
        try
        {
            string dir = Path.GetDirectoryName(_settingsFile) ?? string.Empty;
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            string json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_settingsFile, json);
            _settings = settings;
        }
        catch
        {
            // ignore
        }
    }
}
