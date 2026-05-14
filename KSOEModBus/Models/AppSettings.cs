namespace KSOEModBus.Models;

public sealed class AppSettings
{
    public int ModbusPort { get; set; } = 502;
    public bool AutoStart { get; set; } = true;
    public string ExcelSheetName { get; set; } = "Sheet1";
    public bool AutoLoadExcel { get; set; } = true;
    public int UdpReceivePort { get; set; } = 12121;
    public string UdpSendIp { get; set; } = "192.168.1.10";
    public int UdpSendPort { get; set; } = 13131;
}