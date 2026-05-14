using KSOEModBus.Infrastructure;

namespace KSOEModBus.Models;

public sealed class MappingItem : ObservableObject
{
    private float _currentValue;
    private DateTime _updatedAt = DateTime.MinValue;

    public MappingItem(MappingDefinition definition)
    {
        Definition = definition;
    }

    public MappingDefinition Definition { get; }

    public string Category => Definition.Category;
    public string Equip => Definition.Equip;
    public string Direction => Definition.Direction == DataDirection.StrToKsoe ? "STR_TO_KSOE" : "KSOE_TO_STR";
    public string SignalKey => Definition.SignalKey;
    public int Address => Definition.Address;
    public string Description => Definition.Description;
    public string DataType => Definition.DataType;
    public string Unit => Definition.Unit;
    public string Note => Definition.Note;

    public float CurrentValue
    {
        get => _currentValue;
        set
        {
            if (SetProperty(ref _currentValue, value))
            {
                UpdatedAt = DateTime.Now;
                OnPropertyChanged(nameof(DisplayValue));
            }
        }
    }

    public DateTime UpdatedAt
    {
        get => _updatedAt;
        private set => SetProperty(ref _updatedAt, value);
    }

    public string DisplayValue => CurrentValue.ToString("G9");
}
