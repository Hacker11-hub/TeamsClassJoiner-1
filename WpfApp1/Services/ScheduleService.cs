using System.Collections.Generic;
using System.IO;
using System;
using System.Text.Json;
using TeamsClassJoiner.Models;

namespace TeamsClassJoiner.Services;

public class ScheduleService
{
    private readonly string _dataDirectory;
    private readonly string _filePath;

    private readonly JsonSerializerOptions _options =
        new()
        {
            WriteIndented = true
        };

    public ScheduleService()
    {
        _dataDirectory = Path.Combine(
            Environment.GetFolderPath(
                Environment.SpecialFolder.ApplicationData),
            "TeamsClassJoiner");

        _filePath = Path.Combine(
            _dataDirectory,
            "schedules.json");

        Directory.CreateDirectory(_dataDirectory);
    }

    public List<ClassSchedule> Load()
    {
        try
        {
            if (!File.Exists(_filePath))
                return new List<ClassSchedule>();

            string json = File.ReadAllText(_filePath);

            return JsonSerializer.Deserialize<List<ClassSchedule>>(
                       json,
                       _options)
                   ?? new List<ClassSchedule>();
        }
        catch
        {
            return new List<ClassSchedule>();
        }
    }

    public void Save(List<ClassSchedule> schedules)
    {
        Directory.CreateDirectory(_dataDirectory);

        string json = JsonSerializer.Serialize(
            schedules,
            _options);

        File.WriteAllText(_filePath, json);
    }
}