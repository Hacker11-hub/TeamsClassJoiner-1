using System;
using System.Windows;
using TeamsClassJoiner.Models;

namespace TeamsClassJoiner.Views;

public partial class AddClassWindow : Window
{
    public ClassSchedule ClassSchedule { get; private set; }

    public AddClassWindow()
    {
        InitializeComponent();

        ClassSchedule = new ClassSchedule();

        LoadDays();

        DayComboBox.SelectedItem =
            DateTime.Now.DayOfWeek;

        StartTimeTextBox.Text =
            "09:00";

        EndTimeTextBox.Text =
            "10:00";
    }

    public AddClassWindow(
        ClassSchedule existing)
    {
        InitializeComponent();

        ClassSchedule = new ClassSchedule
        {
            Id = existing.Id,
            ClassName = existing.ClassName,
            Day = existing.Day,
            StartTime = existing.StartTime,
            EndTime = existing.EndTime,
            TeamsUrl = existing.TeamsUrl,
            Enabled = existing.Enabled,
            NotificationMinutesBefore =
                existing.NotificationMinutesBefore
        };

        LoadDays();

        ClassNameTextBox.Text =
            ClassSchedule.ClassName;

        DayComboBox.SelectedItem =
            ClassSchedule.Day;

        StartTimeTextBox.Text =
            ClassSchedule.StartTime.ToString(@"hh\:mm");

        EndTimeTextBox.Text =
            ClassSchedule.EndTime.ToString(@"hh\:mm");

        TeamsUrlTextBox.Text =
            ClassSchedule.TeamsUrl;

        EnabledCheckBox.IsChecked =
            ClassSchedule.Enabled;
    }

    private void LoadDays()
    {
        DayComboBox.ItemsSource =
            Enum.GetValues<DayOfWeek>();
    }

    private void Save_Click(
        object sender,
        RoutedEventArgs e)
    {
        string className =
            ClassNameTextBox.Text.Trim();

        string teamsUrl =
            TeamsUrlTextBox.Text.Trim();

        if (string.IsNullOrWhiteSpace(className))
        {
            System.Windows.MessageBox.Show(
                "Please enter a class name.",
                "Validation Error");

            return;
        }

        if (DayComboBox.SelectedItem is not DayOfWeek day)
        {
            System.Windows.MessageBox.Show(
                "Please select a day.",
                "Validation Error");

            return;
        }

        if (!TimeSpan.TryParse(
                StartTimeTextBox.Text.Trim(),
                out TimeSpan startTime))
        {
            System.Windows.MessageBox.Show(
                "Start time must be in HH:mm format.\nExample: 09:30",
                "Validation Error");

            return;
        }

        if (!TimeSpan.TryParse(
                EndTimeTextBox.Text.Trim(),
                out TimeSpan endTime))
        {
            System.Windows.MessageBox.Show(
                "End time must be in HH:mm format.\nExample: 10:30",
                "Validation Error");

            return;
        }

        if (string.IsNullOrWhiteSpace(teamsUrl))
        {
            System.Windows.MessageBox.Show(
                "Please enter the Teams meeting URL.",
                "Validation Error");

            return;
        }

        ClassSchedule.ClassName =
            className;

        ClassSchedule.Day =
            day;

        ClassSchedule.StartTime =
            startTime;

        ClassSchedule.EndTime =
            endTime;

        ClassSchedule.TeamsUrl =
            teamsUrl;

        ClassSchedule.Enabled =
            EnabledCheckBox.IsChecked == true;

        DialogResult = true;

        Close();
    }

    private void Cancel_Click(
        object sender,
        RoutedEventArgs e)
    {
        DialogResult = false;

        Close();
    }
}