using System.Windows;
using Forms = System.Windows.Forms;
using TeamsClassJoiner.Views;

namespace TeamsClassJoiner;

public partial class App : System.Windows.Application
{
    private Forms.NotifyIcon? _notifyIcon;

    private MainWindow? _mainWindow;

    protected override void OnStartup(
        StartupEventArgs e)
    {
        base.OnStartup(e);

        _mainWindow =
            new MainWindow();

        _mainWindow.Show();

        CreateTrayIcon();
    }

    private void CreateTrayIcon()
    {
        _notifyIcon = new Forms.NotifyIcon
        {
            Icon = System.Drawing.SystemIcons.Application,
            Visible = true,
            Text = "Teams Class Joiner"
        };

        Forms.ContextMenuStrip menu =
            new Forms.ContextMenuStrip();

        Forms.ToolStripMenuItem openItem =
            new Forms.ToolStripMenuItem("Open");

        openItem.Click += (_, _) =>
        {
            ShowMainWindow();
        };

        Forms.ToolStripMenuItem exitItem =
            new Forms.ToolStripMenuItem("Exit");

        exitItem.Click += (_, _) =>
        {
            ExitApplication();
        };

        menu.Items.Add(openItem);
        menu.Items.Add(new Forms.ToolStripSeparator());
        menu.Items.Add(exitItem);

        _notifyIcon.ContextMenuStrip = menu;

        _notifyIcon.DoubleClick += (_, _) =>
        {
            ShowMainWindow();
        };
    }

    private void ShowMainWindow()
    {
        if (_mainWindow == null)
            return;

        _mainWindow.Show();

        _mainWindow.WindowState =
            WindowState.Normal;

        _mainWindow.Activate();
    }

    private void ExitApplication()
    {
        _notifyIcon?.Dispose();

        Shutdown();
    }

    protected override void OnExit(
        ExitEventArgs e)
    {
        _notifyIcon?.Dispose();

        base.OnExit(e);
    }
}