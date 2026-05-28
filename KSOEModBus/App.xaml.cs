using System.Windows;
using KSOEModBus.Services;

namespace KSOEModBus;

public partial class App : Application
{
    public App()
    {
        Startup += OnStartup;
        Exit += OnExit;
        SessionEnding += OnSessionEnding;
        DispatcherUnhandledException += OnDispatcherUnhandledException;
        AppDomain.CurrentDomain.UnhandledException += OnCurrentDomainUnhandledException;
        TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;
    }

    private void OnStartup(object sender, StartupEventArgs e)
    {
        DiagnosticLog.Write("Application startup");
    }

    private void OnExit(object sender, ExitEventArgs e)
    {
        DiagnosticLog.Write($"Application exit with code {e.ApplicationExitCode}");
    }

    private void OnSessionEnding(object sender, SessionEndingCancelEventArgs e)
    {
        DiagnosticLog.Write($"Session ending: reason={e.ReasonSessionEnding}, cancel={e.Cancel}");
    }

    private void OnDispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
    {
        DiagnosticLog.WriteException("Dispatcher unhandled exception", e.Exception);
    }

    private void OnCurrentDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        if (e.ExceptionObject is Exception exception)
        {
            DiagnosticLog.WriteException($"AppDomain unhandled exception (terminating={e.IsTerminating})", exception);
            return;
        }

        DiagnosticLog.Write($"AppDomain unhandled non-exception object (terminating={e.IsTerminating})");
    }

    private void OnUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
    {
        DiagnosticLog.WriteException("TaskScheduler unobserved task exception", e.Exception);
    }
}
