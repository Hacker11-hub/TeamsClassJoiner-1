using System.Collections.Generic;
using System;
using System.Windows.Threading;
using TeamsClassJoiner.Models;

namespace TeamsClassJoiner.Services;

public class SchedulerService
{
    private readonly DispatcherTimer _timer;

    private readonly TeamsService _teamsService;

    private List<ClassSchedule> _schedules = new();

    private readonly HashSet<string> _executedClasses = new();

    public bool Enabled { get; set; } = true;

    public event Action<ClassSchedule>? ClassStarted;

    public SchedulerService(
        TeamsService teamsService)
    {
        _teamsService = teamsService;

        _timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(10)
        };

        _timer.Tick += Timer_Tick;
    }

    public void Start(List<ClassSchedule> schedules)
    {
        _schedules = schedules;

        _timer.Start();
    }

    public void UpdateSchedules(
        List<ClassSchedule> schedules)
    {
        _schedules = schedules;
    }

    public void Stop()
    {
        _timer.Stop();
    }

    private void Timer_Tick(
        object? sender,
        EventArgs e)
    {
        if (!Enabled)
            return;

        DateTime now = DateTime.Now;

        foreach (ClassSchedule schedule in _schedules)
        {
            if (!schedule.Enabled)
                continue;

            if (schedule.Day != now.DayOfWeek)
                continue;

            TimeSpan difference =
                now.TimeOfDay - schedule.StartTime;

            if (difference.TotalSeconds < 0)
                continue;

            if (difference.TotalMinutes > 1)
                continue;

            string executionKey =
                $"{schedule.Id}_{now:yyyyMMddHHmm}";

            if (_executedClasses.Contains(executionKey))
                continue;

            _executedClasses.Add(executionKey);

            _teamsService.OpenMeeting(
                schedule.TeamsUrl);

            ClassStarted?.Invoke(schedule);
        }

        CleanupOldExecutionKeys();
    }

    private void CleanupOldExecutionKeys()
    {
        if (_executedClasses.Count < 500)
            return;

        _executedClasses.Clear();
    }
}