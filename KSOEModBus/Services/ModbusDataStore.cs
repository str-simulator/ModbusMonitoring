using KSOEModBus.Models;

namespace KSOEModBus.Services;

public sealed class ModbusDataStore
{
    private readonly object _syncRoot = new();
    private readonly Dictionary<string, MappingItem> _signalIndex = new(StringComparer.OrdinalIgnoreCase);
    private readonly List<MappingItem> _items = [];
    private ushort[] _registers = Array.Empty<ushort>();

    public event Action<IReadOnlyList<MappingItem>>? KsoeDataWritten;

    public void LoadMappings(IEnumerable<MappingDefinition> mappings)
    {
        lock (_syncRoot)
        {
            _items.Clear();
            _signalIndex.Clear();

            foreach (var definition in mappings.OrderBy(item => item.Address))
            {
                var item = new MappingItem(definition);
                _items.Add(item);
                _signalIndex[item.SignalKey] = item;
            }

            var registerCount = _items.Count == 0 ? 256 : _items.Max(item => item.Address) + 2;
            _registers = new ushort[Math.Max(registerCount, 256)];
        }
    }

    public IReadOnlyList<MappingItem> GetItems(DataDirection direction)
    {
        lock (_syncRoot)
        {
            return _items.Where(item => item.Definition.Direction == direction).ToList();
        }
    }

    public Dictionary<string, float> CreateSnapshot(DataDirection direction)
    {
        lock (_syncRoot)
        {
            return _items
                .Where(item => item.Definition.Direction == direction)
                .ToDictionary(item => item.SignalKey, item => item.CurrentValue, StringComparer.OrdinalIgnoreCase);
        }
    }

    public int SeedDirectionValues(DataDirection direction, Func<MappingItem, float> valueFactory)
    {
        lock (_syncRoot)
        {
            var changedCount = 0;
            foreach (var item in _items.Where(item => item.Definition.Direction == direction))
            {
                var value = valueFactory(item);
                item.CurrentValue = value;
                WriteFloat(item.Address, value);
                changedCount++;
            }

            return changedCount;
        }
    }

    public bool UpdateFromSignal(string signalKey, float value)
    {
        lock (_syncRoot)
        {
            if (!_signalIndex.TryGetValue(signalKey, out var item))
            {
                return false;
            }

            item.CurrentValue = value;
            WriteFloat(item.Address, value);
            return true;
        }
    }

    public bool TryReadRegisters(ushort startAddress, ushort count, out ushort[] values)
    {
        lock (_syncRoot)
        {
            values = Array.Empty<ushort>();
            if (startAddress + count > _registers.Length)
            {
                return false;
            }

            values = new ushort[count];
            Array.Copy(_registers, startAddress, values, 0, count);
            return true;
        }
    }

    public bool TryWriteRegisters(ushort startAddress, IReadOnlyList<ushort> values, out byte exceptionCode)
    {
        List<MappingItem>? changed = null;

        lock (_syncRoot)
        {
            exceptionCode = 0;
            if (startAddress + values.Count > _registers.Length)
            {
                exceptionCode = 0x02;
                return false;
            }

            if (!ValidateWritableRange(startAddress, values.Count))
            {
                exceptionCode = 0x03;
                return false;
            }

            for (var index = 0; index < values.Count; index++)
            {
                _registers[startAddress + index] = values[index];
            }

            changed = new List<MappingItem>();
            foreach (var item in _items.Where(item => item.Definition.Direction == DataDirection.KsoeToStr))
            {
                if (!RangesOverlap(item.Address, 2, startAddress, values.Count))
                {
                    continue;
                }

                item.CurrentValue = ReadFloat(item.Address);
                changed.Add(item);
            }

            if (changed.Count == 0)
            {
                changed = null;
            }
        }

        if (changed is not null)
        {
            KsoeDataWritten?.Invoke(changed);
        }

        return true;
    }

    private bool ValidateWritableRange(int startAddress, int count)
    {
        for (var address = startAddress; address < startAddress + count; address++)
        {
            if (!_items.Any(item => item.Definition.Direction == DataDirection.KsoeToStr &&
                                   address >= item.Address &&
                                   address < item.Address + 2))
            {
                return false;
            }
        }

        return true;
    }

    private void WriteFloat(int address, float value)
    {
        var bytes = BitConverter.GetBytes(value);
        if (BitConverter.IsLittleEndian)
        {
            Array.Reverse(bytes);
        }

        _registers[address] = (ushort)((bytes[0] << 8) | bytes[1]);
        _registers[address + 1] = (ushort)((bytes[2] << 8) | bytes[3]);
    }

    private float ReadFloat(int address)
    {
        var bytes = new byte[4];
        bytes[0] = (byte)(_registers[address] >> 8);
        bytes[1] = (byte)(_registers[address] & 0xFF);
        bytes[2] = (byte)(_registers[address + 1] >> 8);
        bytes[3] = (byte)(_registers[address + 1] & 0xFF);
        if (BitConverter.IsLittleEndian)
        {
            Array.Reverse(bytes);
        }

        return BitConverter.ToSingle(bytes, 0);
    }

    private static bool RangesOverlap(int startA, int lengthA, int startB, int lengthB)
    {
        return startA < startB + lengthB && startB < startA + lengthA;
    }
}
