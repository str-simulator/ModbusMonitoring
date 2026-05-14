namespace KSOEModBus.Models;

public sealed class UdpSnapshotMessage
{
    public string MessageType { get; set; } = string.Empty;
    public long Timestamp { get; set; }
    public Dictionary<string, float> Values { get; set; } = [];
}
