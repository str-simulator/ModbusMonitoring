using System.Collections.ObjectModel;
using System.IO;
using System.Threading;
using System.Windows;
using KSOEModBus.Infrastructure;
using KSOEModBus.Models;
using KSOEModBus.Services;

namespace KSOEModBus.ViewModels;

public sealed class MainViewModel : ObservableObject
{
    private readonly IniSettingsService _settingsService;
    private readonly ExcelMappingLoader _excelLoader;
    private readonly ExcelTemplateWriter _excelTemplateWriter;
    private readonly ModbusDataStore _dataStore;
    private readonly ModbusTcpServer _modbusServer;
    private readonly UdpJsonBridge _udpBridge;
    private readonly AppSettings _settings;
    private int _ksoeSyncPending;
    private int _refreshPending;
    private bool _initialized;
    private bool _isRunning;
    private int _modbusPort;
    private string _statusText = "Stopped";
    private string _excelStatusText = "Not loaded";
    private string _clientStatusText = "No client";

    public MainViewModel()
    {
        var iniPath = Path.Combine(AppContext.BaseDirectory, "ModbusSetting.ini");
        _settingsService = new IniSettingsService(iniPath);
        _excelLoader = new ExcelMappingLoader();
        _excelTemplateWriter = new ExcelTemplateWriter();
        _dataStore = new ModbusDataStore();
        _modbusServer = new ModbusTcpServer(_dataStore, AddLog);
        _udpBridge = new UdpJsonBridge(_dataStore, AddLog, RequestRefresh);
        _settings = _settingsService.Load();
        _modbusPort = _settings.ModbusPort;

        StartCommand = new RelayCommand(() => _ = StartAsync(), () => !IsRunning);
        StopCommand = new RelayCommand(() => _ = StopAsync(), () => IsRunning);
        ReloadExcelCommand = new RelayCommand(LoadExcel);
        ClearLogCommand = new RelayCommand(ClearLogs);
        KsoeReadTestCommand = new RelayCommand(ApplyKsoeReadTestData);

        _dataStore.KsoeDataWritten += _changedItems => ScheduleKsoeDataSync();
    }

    public ObservableCollection<MappingItem> KsoeToStrItems { get; } = [];
    public ObservableCollection<MappingItem> StrToKsoeItems { get; } = [];
    public ObservableCollection<string> Logs { get; } = [];

    public RelayCommand StartCommand { get; }
    public RelayCommand StopCommand { get; }
    public RelayCommand ReloadExcelCommand { get; }
    public RelayCommand ClearLogCommand { get; }
    public RelayCommand KsoeReadTestCommand { get; }

    public int ModbusPort
    {
        get => _modbusPort;
        set => SetProperty(ref _modbusPort, value);
    }

    public string StatusText
    {
        get => _statusText;
        private set => SetProperty(ref _statusText, value);
    }

    public string ExcelStatusText
    {
        get => _excelStatusText;
        private set => SetProperty(ref _excelStatusText, value);
    }

    public string ClientStatusText
    {
        get => _clientStatusText;
        private set => SetProperty(ref _clientStatusText, value);
    }

    public bool IsRunning
    {
        get => _isRunning;
        private set
        {
            if (SetProperty(ref _isRunning, value))
            {
                StartCommand.RaiseCanExecuteChanged();
                StopCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public async Task InitializeAsync()
    {
        if (_initialized)
        {
            return;
        }

        _initialized = true;
        AddLog($"INI loaded: ModbusPort={_settings.ModbusPort}, AutoStart={_settings.AutoStart}, ExcelSheetName={_settings.ExcelSheetName}, AutoLoadExcel={_settings.AutoLoadExcel}, UdpReceivePort={_settings.UdpReceivePort}, UdpSendIp={_settings.UdpSendIp}, UdpSendPort={_settings.UdpSendPort}");
        _excelTemplateWriter.EnsureTemplate(_settingsService.GetExcelPath());
        if (_settings.AutoLoadExcel)
        {
            LoadExcel();
        }

        if (_settings.AutoStart)
        {
            await StartAsync();
        }
    }

    public async Task ShutdownAsync()
    {
        await StopAsync();
        _settings.ModbusPort = ModbusPort;
        _settingsService.Save(_settings);
    }

    public void LoadExcel()
    {
        try
        {
            var excelPath = _settingsService.GetExcelPath();
            var mappings = _excelLoader.Load(excelPath, _settings.ExcelSheetName);
            _dataStore.LoadMappings(mappings);
            RefreshCollections();
            ExcelStatusText = $"Loaded {mappings.Count} items from {Path.GetFileName(excelPath)}";
            AddLog(ExcelStatusText);
        }
        catch (Exception ex)
        {
            ExcelStatusText = $"Excel load failed: {ex.Message}";
            AddLog(ExcelStatusText);
        }
    }

    public async Task StartAsync()
    {
        if (IsRunning)
        {
            return;
        }

        _settings.ModbusPort = ModbusPort;
        await _udpBridge.StartAsync(_settings);
        await _modbusServer.StartAsync(ModbusPort);
        StatusText = $"Listening on 0.0.0.0:{ModbusPort}";
        ClientStatusText = "Multi client mode enabled";
        IsRunning = true;
    }

    public async Task StopAsync()
    {
        if (!IsRunning)
        {
            return;
        }

        await _modbusServer.StopAsync();
        await _udpBridge.StopAsync();
        StatusText = "Stopped";
        ClientStatusText = "No client";
        IsRunning = false;
    }

    private void ApplyKsoeReadTestData()
    {
        var seeded = _dataStore.SeedDirectionValues(DataDirection.StrToKsoe, CreateReadTestValue);
        RefreshCollections();

        if (seeded == 0)
        {
            AddLog("KSOE read test skipped: no STR_TO_KSOE mappings loaded");
            return;
        }

        AddLog($"KSOE read test data applied to {seeded} STR_TO_KSOE items");
    }

    private static float CreateReadTestValue(MappingItem item)
    {
        // Keep values deterministic so KSOE can compare Modbus reads with the UI.
        return (item.Address / 2f) + 0.5f;
    }

    private void RefreshCollections()
    {
        RebuildCollection(KsoeToStrItems, _dataStore.GetItems(DataDirection.KsoeToStr));
        RebuildCollection(StrToKsoeItems, _dataStore.GetItems(DataDirection.StrToKsoe));
    }

    private void ScheduleKsoeDataSync()
    {
        if (Interlocked.Exchange(ref _ksoeSyncPending, 1) == 1)
        {
            return;
        }

        _ = HandleKsoeDataWrittenAsync();
    }

    private async Task HandleKsoeDataWrittenAsync()
    {
        try
        {
            // Batch rapid Modbus writes into a single UDP/UI update.
            await Task.Delay(100);
            await _udpBridge.SendSnapshotAsync(DataDirection.KsoeToStr);
            RequestRefresh();
        }
        catch (Exception ex)
        {
            AddLog($"KSOE data sync failed: {ex.Message}");
        }
        finally
        {
            Interlocked.Exchange(ref _ksoeSyncPending, 0);
        }
    }

    private void RequestRefresh()
    {
        if (Interlocked.Exchange(ref _refreshPending, 1) == 1)
        {
            return;
        }

        _ = Application.Current.Dispatcher.BeginInvoke(() =>
        {
            try
            {
                RefreshCollections();
            }
            finally
            {
                Interlocked.Exchange(ref _refreshPending, 0);
            }
        });
    }

    private static void RebuildCollection(ObservableCollection<MappingItem> target, IReadOnlyList<MappingItem> items)
    {
        target.Clear();
        foreach (var item in items)
        {
            target.Add(item);
        }
    }

    private void AddLog(string message)
    {
        if (Application.Current.Dispatcher.CheckAccess())
        {
            Logs.Add($"[{DateTime.Now:HH:mm:ss}] {message}");
            while (Logs.Count > 300)
            {
                Logs.RemoveAt(0);
            }
            return;
        }

        _ = Application.Current.Dispatcher.BeginInvoke(() =>
        {
            Logs.Add($"[{DateTime.Now:HH:mm:ss}] {message}");
            while (Logs.Count > 300)
            {
                Logs.RemoveAt(0);
            }
        });
    }

    private void ClearLogs()
    {
        Application.Current.Dispatcher.Invoke(() => Logs.Clear());
    }
}
