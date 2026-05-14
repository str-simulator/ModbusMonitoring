namespace KSOEModBus.Models;

public sealed class MappingDefinition
{
    public required string Category { get; init; }
    public required string Equip { get; init; }
    public required DataDirection Direction { get; init; }
    public required string SignalKey { get; init; }
    public required int Address { get; init; }
    public required string Description { get; init; }
    public string Protocol { get; init; } = "Modbus";
    public string DataType { get; init; } = "float";
    public string Unit { get; init; } = string.Empty;
    public string Note { get; init; } = string.Empty;
    public bool Writable => Direction == DataDirection.KsoeToStr;
}
