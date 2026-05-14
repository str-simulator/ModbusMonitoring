using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using KSOEModBus.Models;

namespace KSOEModBus.Services;

public sealed class UdpJsonBridge : IAsyncDisposable
{
    private readonly ModbusDataStore _dataStore;
    private readonly Action<string> _log;
    private readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web);
    private UdpClient? _receiver;
    private UdpClient? _sender;
    private CancellationTokenSource? _cts;
    private Task? _receiveLoopTask;
    private AppSettings? _settings;

    public UdpJsonBridge(ModbusDataStore dataStore, Action<string> log)
    {
        _dataStore = dataStore;
        _log = log;
    }

    public Task StartAsync(AppSettings settings, CancellationToken cancellationToken = default)
    {
        _settings = settings;
        _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        _receiver = new UdpClient(settings.UdpReceivePort);
        _sender = new UdpClient();
        _receiveLoopTask = Task.Run(() => ReceiveLoopAsync(_cts.Token), _cts.Token);
        _log($"UDP receive started on {settings.UdpReceivePort}, send target {settings.UdpSendIp}:{settings.UdpSendPort}");
        return Task.CompletedTask;
    }

    public async Task StopAsync()
    {
        if (_cts is not null)
        {
            _cts.Cancel();
        }

        if (_receiveLoopTask is not null)
        {
            try
            {
                await _receiveLoopTask;
            }
            catch (OperationCanceledException)
            {
            }
        }

        _receiver?.Dispose();
        _sender?.Dispose();
        _receiver = null;
        _sender = null;
        _cts?.Dispose();
        _cts = null;
    }

    public async Task SendSnapshotAsync(DataDirection direction, CancellationToken cancellationToken = default)
    {
        if (_sender is null || _settings is null)
        {
            return;
        }

        var message = new UdpSnapshotMessage
        {
            MessageType = direction == DataDirection.StrToKsoe ? "str_to_ksoe" : "ksoe_to_str",
            Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            Values = _dataStore.CreateSnapshot(direction),
        };

        cancellationToken.ThrowIfCancellationRequested();
        var bytes = JsonSerializer.SerializeToUtf8Bytes(message, _jsonOptions);
        await _sender.SendAsync(bytes, bytes.Length, _settings.UdpSendIp, _settings.UdpSendPort);
        _log($"UDP TX {message.MessageType} ({message.Values.Count} items)");
    }

    private async Task ReceiveLoopAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested && _receiver is not null)
        {
            try
            {
                var result = await _receiver.ReceiveAsync(cancellationToken);
                var json = Encoding.UTF8.GetString(result.Buffer);
                var message = JsonSerializer.Deserialize<UdpSnapshotMessage>(json, _jsonOptions);
                if (message?.Values is null)
                {
                    continue;
                }

                var changedCount = 0;
                foreach (var pair in message.Values)
                {
                    if (_dataStore.UpdateFromSignal(pair.Key, pair.Value))
                    {
                        changedCount++;
                    }
                }

                _log($"UDP RX {message.MessageType} from {result.RemoteEndPoint.Address}:{result.RemoteEndPoint.Port} ({changedCount} items)");
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _log($"UDP RX error: {ex.Message}");
            }
        }
    }

    public async ValueTask DisposeAsync()
    {
        await StopAsync();
    }
}
