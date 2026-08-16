using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Automation;

namespace TeamsClassJoiner.Services;

public class TeamsService
{
    public void OpenMeeting(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return;

        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true
            });
            // Try to automatically join the meeting by interacting with the
            // Teams window. Run in background so UI thread isn't blocked.
            var settings = SettingsService.Load();
            Logger.Log($"OpenMeeting: launched URL {url}");
            Task.Run(() => AutoJoinMeetingAsync(TimeSpan.FromSeconds(settings.AutoJoinTimeoutSeconds)));
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show(
                $"Unable to open Teams meeting.\n\n{ex.Message}",
                "Teams Class Joiner",
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Error);
        }
    }

    private async Task AutoJoinMeetingAsync(TimeSpan timeout)
    {
        try
        {
            var settings = SettingsService.Load();
            if (!settings.AutoJoinEnabled)
            {
                Logger.Log("AutoJoin disabled in settings; skipping automation.");
                return;
            }

            DateTime end = DateTime.Now + timeout;

            // Wait for Teams process/window to appear
            AutomationElement teamsWindow = null;

            while (DateTime.Now < end)
            {
                // Look for any top-level window that looks like Teams
                var desktop = AutomationElement.RootElement;

                var cond = new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Window);

                var windows = desktop.FindAll(TreeScope.Children, cond);

                for (int i = 0; i < windows.Count; i++)
                {
                    var w = windows[i];
                    try
                    {
                        string name = w.Current.Name ?? string.Empty;
                        // Check window name for Teams-like text
                        if (name.IndexOf("teams", StringComparison.OrdinalIgnoreCase) >= 0 ||
                            name.IndexOf("microsoft teams", StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            teamsWindow = w;
                            break;
                        }

                        // Also check process owning this window for "Teams"
                        try
                        {
                            var pid = w.Current.ProcessId;
                            var p = Process.GetProcessById(pid);
                            if (p != null && p.ProcessName.IndexOf("teams", StringComparison.OrdinalIgnoreCase) >= 0)
                            {
                                teamsWindow = w;
                                break;
                            }
                        }
                        catch { }
                    }
                    catch
                    {
                        // ignore window enumeration errors
                    }
                }

                if (teamsWindow != null)
                    break;

                await Task.Delay(1000);
            }

            if (teamsWindow == null)
            {
                Logger.Log("AutoJoin: Teams window not found within timeout.");
                return;
            }

            Logger.Log($"AutoJoin: Found Teams window '{teamsWindow.Current.Name}'");

            // Search for a button with name "Join now" or "Join" and invoke it
            var joinBtn = FindButtonByNames(teamsWindow, new[] { "Join now", "Join", "Join meeting" });

            if (joinBtn != null)
            {
                Logger.Log($"AutoJoin: Found join button '{joinBtn.Current.Name}', attempting invoke.");
                if (joinBtn.TryGetCurrentPattern(InvokePattern.Pattern, out object patternObj) && patternObj is InvokePattern invoke)
                {
                    try
                    {
                        invoke.Invoke();
                        Logger.Log("AutoJoin: Invoke succeeded.");
                    }
                    catch (Exception ex)
                    {
                        Logger.Log($"AutoJoin: Invoke failed - {ex.Message}");
                    }
                }
            }
            else
            {
                Logger.Log("AutoJoin: Join button not found in Teams window.");
            }
        }
        catch
        {
            // Swallow automation exceptions to avoid crashing background task
        }
    }

    private AutomationElement? FindButtonByNames(AutomationElement root, string[] names)
    {
        var condButton = new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Button);

        var buttons = root.FindAll(TreeScope.Descendants, condButton);

        for (int i = 0; i < buttons.Count; i++)
        {
            try
            {
                string name = buttons[i].Current.Name ?? string.Empty;
                foreach (var n in names)
                {
                    if (string.Equals(name, n, StringComparison.OrdinalIgnoreCase) || name.IndexOf(n, StringComparison.OrdinalIgnoreCase) >= 0)
                        return buttons[i];
                }
            }
            catch
            {
                // ignore
            }
        }

        return null;
    }
}