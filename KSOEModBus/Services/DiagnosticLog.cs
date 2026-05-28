using System.IO;
using System.Text;

namespace KSOEModBus.Services;

public static class DiagnosticLog
{
    private static readonly object SyncRoot = new();
    private static readonly string LogPath = Path.Combine(AppContext.BaseDirectory, "diagnostic.log");

    public static void Write(string message)
    {
        var line = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] {message}{Environment.NewLine}";
        lock (SyncRoot)
        {
            File.AppendAllText(LogPath, line, Encoding.UTF8);
        }
    }

    public static void WriteException(string title, Exception exception)
    {
        Write($"{title}: {exception}");
    }
}
