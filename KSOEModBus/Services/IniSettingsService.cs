using System.Text;
using System.IO;
using KSOEModBus.Models;

namespace KSOEModBus.Services;

public sealed class IniSettingsService
{
    private const string UdpSectionName = "Udp";
    private const string LegacyUdpSectionName = "InstructorConsoleUDP";
    private readonly string _iniPath;

    public IniSettingsService(string iniPath)
    {
        _iniPath = iniPath;
    }

    public AppSettings Load()
    {
        var settings = new AppSettings
        {
            ExcelSheetName = "Sheet1",
        };

        if (!File.Exists(_iniPath))
        {
            Save(settings);
            return settings;
        }

        string? section = null;
        foreach (var rawLine in File.ReadAllLines(_iniPath, Encoding.UTF8))
        {
            var line = rawLine.Trim();
            if (string.IsNullOrWhiteSpace(line) || line.StartsWith(';') || line.StartsWith('#'))
            {
                continue;
            }

            if (line.StartsWith('[') && line.EndsWith(']'))
            {
                section = line[1..^1];
                continue;
            }

            var separatorIndex = line.IndexOf('=');
            if (separatorIndex <= 0)
            {
                continue;
            }

            var key = line[..separatorIndex].Trim();
            var value = line[(separatorIndex + 1)..].Trim();

            switch (section)
            {
                case "Connection":
                    if (key.Equals("Port", StringComparison.OrdinalIgnoreCase) && int.TryParse(value, out var port))
                    {
                        settings.ModbusPort = port;
                    }
                    else if (key.Equals("AutoStart", StringComparison.OrdinalIgnoreCase) && bool.TryParse(value, out var autoStart))
                    {
                        settings.AutoStart = autoStart;
                    }
                    break;
                case "Excel":
                    if (key.Equals("SheetName", StringComparison.OrdinalIgnoreCase))
                    {
                        settings.ExcelSheetName = value;
                    }
                    else if (key.Equals("AutoLoad", StringComparison.OrdinalIgnoreCase) && bool.TryParse(value, out var autoLoad))
                    {
                        settings.AutoLoadExcel = autoLoad;
                    }
                    break;
                case var currentSection when IsUdpSection(currentSection):
                    if (key.Equals("ReceivePort", StringComparison.OrdinalIgnoreCase) && int.TryParse(value, out var recvPort))
                    {
                        settings.UdpReceivePort = recvPort;
                    }
                    else if (key.Equals("SendIp", StringComparison.OrdinalIgnoreCase))
                    {
                        settings.UdpSendIp = value;
                    }
                    else if (key.Equals("SendPort", StringComparison.OrdinalIgnoreCase) && int.TryParse(value, out var sendPort))
                    {
                        settings.UdpSendPort = sendPort;
                    }
                    break;
            }
        }

        return settings;
    }

    public void Save(AppSettings settings)
    {
        var content = $$"""
[Connection]
Port={{settings.ModbusPort}}
AutoStart={{settings.AutoStart}}

[Excel]
SheetName={{settings.ExcelSheetName}}
AutoLoad={{settings.AutoLoadExcel}}

[Udp]
ReceivePort={{settings.UdpReceivePort}}
SendIp={{settings.UdpSendIp}}
SendPort={{settings.UdpSendPort}}
""";

        File.WriteAllText(_iniPath, content, Encoding.UTF8);
    }

    private static string FindDefaultExcelPath()
    {
        return Path.Combine(AppContext.BaseDirectory, "GRC_HILS_Interface_IO.xlsx");
    }

    public string GetExcelPath()
    {
        return FindDefaultExcelPath();
    }

    private static bool IsUdpSection(string? section)
    {
        return section is not null &&
               (section.Equals(UdpSectionName, StringComparison.OrdinalIgnoreCase) ||
                section.Equals(LegacyUdpSectionName, StringComparison.OrdinalIgnoreCase));
    }
}
