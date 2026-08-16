using System;

namespace TeamsClassJoiner.Models;

public class ClassSchedule
{
	public Guid Id { get; set; } = Guid.NewGuid();

	public string ClassName { get; set; } = "";

	public DayOfWeek Day { get; set; }

	public TimeSpan StartTime { get; set; }

	public TimeSpan EndTime { get; set; }

	public string TeamsUrl { get; set; } = "";

	public bool Enabled { get; set; } = true;

	public int NotificationMinutesBefore { get; set; } = 0;
}