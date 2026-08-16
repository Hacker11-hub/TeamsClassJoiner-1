using System.Collections.Generic;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using TeamsClassJoiner.Models;
using TeamsClassJoiner.Services;

namespace TeamsClassJoiner.Views;

public partial class MainWindow : Window
{
    private readonly ScheduleService _scheduleService;
    private readonly TeamsService _teamsService;
    private readonly SchedulerService _schedulerService;
    private readonly StartupService _startupService;

    private ObservableCollection<ClassSchedule> _schedules;

    public MainWindow()
    {
        InitializeComponent();

        _scheduleService = new ScheduleService();

        _teamsService = new TeamsService();

        _schedulerService =
            new SchedulerService(_teamsService);

        _startupService =
            new StartupService();

        _schedules =
            new ObservableCollection<ClassSchedule>(
                _scheduleService.Load());

        ScheduleGrid.ItemsSource = _schedules;

        AutoJoinCheckBox.IsChecked = true;

        StartupCheckBox.IsChecked =
            _startupService.IsEnabled();

        UpdateStatus();

        _schedulerService.ClassStarted +=
            SchedulerService_ClassStarted;

        _schedulerService.Start(
            _schedules.ToList());

        // Load settings
        var settings = SettingsService.Load();
        AutoJoinCheckBox.IsChecked = settings.AutoJoinEnabled;
        AutoJoinTimeoutTextBox.Text = settings.AutoJoinTimeoutSeconds.ToString();
    }

    private void SchedulerService_ClassStarted(
        ClassSchedule schedule)
    {
        Dispatcher.Invoke(() =>
        {
            System.Windows.MessageBox.Show(
                $"Starting class:\n\n{schedule.ClassName}",
                "Teams Class Joiner",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        });
    }

    private void AddClass_Click(
        object sender,
        RoutedEventArgs e)
    {
        AddClassWindow window =
            new AddClassWindow();

        window.Owner = this;

        if (window.ShowDialog() == true)
        {
            _schedules.Add(
                window.ClassSchedule);

            SaveSchedules();
        }
    }

    private void EditClass_Click(
        object sender,
        RoutedEventArgs e)
    {
        if (ScheduleGrid.SelectedItem
            is not ClassSchedule selected)
        {
            System.Windows.MessageBox.Show(
                "Please select a class first.",
                "Teams Class Joiner");
            return;
        }

        AddClassWindow window =
            new AddClassWindow(selected);

        window.Owner = this;

        if (window.ShowDialog() == true)
        {
            int index =
                _schedules.IndexOf(selected);

            _schedules[index] =
                window.ClassSchedule;

            SaveSchedules();
        }
    }

    private void DeleteClass_Click(
        object sender,
        RoutedEventArgs e)
    {
        if (ScheduleGrid.SelectedItem
            is not ClassSchedule selected)
        {
            System.Windows.MessageBox.Show(
                "Please select a class first.",
                "Teams Class Joiner");
            return;
        }

        System.Windows.MessageBoxResult result =
            System.Windows.MessageBox.Show(
                $"Delete '{selected.ClassName}'?",
                "Confirm Delete",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

        if (result == System.Windows.MessageBoxResult.Yes)
        {
            _schedules.Remove(selected);

            SaveSchedules();
        }
    }

    private void SaveSchedules()
    {
        _scheduleService.Save(
            _schedules.ToList());

        _schedulerService.UpdateSchedules(
            _schedules.ToList());

        UpdateStatus();
    }

    private void Refresh_Click(
        object sender,
        RoutedEventArgs e)
    {
        List<ClassSchedule> schedules =
            _scheduleService.Load();

        _schedules.Clear();

        foreach (ClassSchedule schedule in schedules)
            _schedules.Add(schedule);

        _schedulerService.UpdateSchedules(
            _schedules.ToList());

        UpdateStatus();
    }

    private void AutoJoinCheckBox_Changed(
        object sender,
        RoutedEventArgs e)
    {
        if (_schedulerService != null)
        {
            _schedulerService.Enabled =
                AutoJoinCheckBox.IsChecked == true;

            // Save setting
            var settings = SettingsService.Load();
            settings.AutoJoinEnabled = AutoJoinCheckBox.IsChecked == true;
            if (int.TryParse(AutoJoinTimeoutTextBox.Text.Trim(), out int seconds) && seconds > 0)
                settings.AutoJoinTimeoutSeconds = seconds;
            SettingsService.Save(settings);

            UpdateStatus();
        }
    }

    private void StartupCheckBox_Changed(
        object sender,
        RoutedEventArgs e)
    {
        if (_startupService == null)
            return;

        try
        {
            _startupService.SetEnabled(
                StartupCheckBox.IsChecked == true);
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show(
                $"Unable to change Windows startup setting.\n\n{ex.Message}",
                "Teams Class Joiner",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private void UpdateStatus()
    {
        if (_schedulerService == null)
            return;

        StatusText.Text =
            _schedulerService.Enabled
                ? "? Automatic joining is ON"
                : "? Automatic joining is OFF";
    }
}