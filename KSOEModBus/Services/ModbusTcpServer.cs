using System.Net;
using System.Net.Sockets;

namespace KSOEModBus.Services;

public sealed class ModbusTcpServer : IAsyncDisposable
{
    private readonly ModbusDataStore _dataStore;
    private readonly Action<string> _log;
    private TcpListener? _listener;
    private CancellationTokenSource? _cts;
    private Task? _acceptLoopTask;
    private readonly List<TcpClient> _clients = [];
    private readonly List<Task> _clientTasks = [];
    private readonly object _clientSync = new();
    private bool _isStopping;

    public ModbusTcpServer(ModbusDataStore dataStore, Action<string> log)
    {
        _dataStore = dataStore;
        _log = log;
    }

    public Task StartAsync(int port, CancellationToken cancellationToken = default)
    {
        if (_listener is not null)
        {
            return Task.CompletedTask;
        }

        _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        _isStopping = false;
        _listener = new TcpListener(IPAddress.Any, port);
        _listener.Start();
        _acceptLoopTask = Task.Run(() => AcceptLoopAsync(_cts.Token), _cts.Token);
        _log($"Modbus TCP server started on 0.0.0.0:{port}");
        return Task.CompletedTask;
    }

    public async Task StopAsync()
    {
        lock (_clientSync)
        {
            _isStopping = true;
        }

        _cts?.Cancel();
        _listener?.Stop();
        lock (_clientSync)
        {
            foreach (var client in _clients)
            {
                client.Close();
            }
        }

        if (_acceptLoopTask is not null)
        {
            try
            {
                await _acceptLoopTask;
            }
            catch (OperationCanceledException)
            {
            }
        }

        Task[] clientTasks;
        lock (_clientSync)
        {
            clientTasks = _clientTasks.ToArray();
            _clients.Clear();
            _clientTasks.Clear();
        }

        try
        {
            await Task.WhenAll(clientTasks);
        }
        catch (OperationCanceledException)
        {
        }

        _listener = null;
        _cts?.Dispose();
        _cts = null;
        _log("Modbus TCP server stopped");
    }

    private async Task AcceptLoopAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested && _listener is not null)
        {
            TcpClient? client = null;
            try
            {
                client = await _listener.AcceptTcpClientAsync(cancellationToken);
                lock (_clientSync)
                {
                    if (_isStopping)
                    {
                        client.Close();
                        client = null;
                        continue;
                    }

                    _clients.Add(client);
                }

                _log($"Client connected: {client.Client.RemoteEndPoint}");
                var clientTask = HandleClientAsync(client, cancellationToken);
                lock (_clientSync)
                {
                    _clientTasks.Add(clientTask);
                }

                _ = clientTask.ContinueWith(
                    _ =>
                    {
                        lock (_clientSync)
                        {
                            _clientTasks.Remove(clientTask);
                        }
                    },
                    CancellationToken.None,
                    TaskContinuationOptions.ExecuteSynchronously,
                    TaskScheduler.Default);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (ObjectDisposedException)
            {
                break;
            }
            catch (Exception ex)
            {
                client?.Dispose();
                _log($"Accept error: {ex.Message}");
            }
        }
    }

    private async Task HandleClientAsync(TcpClient client, CancellationToken cancellationToken)
    {
        EndPoint? remoteEndPoint = client.Client.RemoteEndPoint;
        try
        {
            using (client)
            {
                using var stream = client.GetStream();
                while (!cancellationToken.IsCancellationRequested)
                {
                    var header = await ReadExactAsync(stream, 7, cancellationToken);
                    if (header.Length == 0)
                    {
                        break;
                    }

                    var transactionId = (ushort)((header[0] << 8) | header[1]);
                    var protocolId = (ushort)((header[2] << 8) | header[3]);
                    var length = (ushort)((header[4] << 8) | header[5]);
                    var unitId = header[6];
                    if (protocolId != 0 || length < 2)
                    {
                        break;
                    }

                    var pdu = await ReadExactAsync(stream, length - 1, cancellationToken);
                    if (pdu.Length == 0)
                    {
                        break;
                    }

                    var responsePdu = ProcessRequest(pdu);
                    var responseLength = (ushort)(responsePdu.Length + 1);
                    var response = new byte[7 + responsePdu.Length];
                    response[0] = (byte)(transactionId >> 8);
                    response[1] = (byte)(transactionId & 0xFF);
                    response[2] = 0;
                    response[3] = 0;
                    response[4] = (byte)(responseLength >> 8);
                    response[5] = (byte)(responseLength & 0xFF);
                    response[6] = unitId;
                    Buffer.BlockCopy(responsePdu, 0, response, 7, responsePdu.Length);
                    await stream.WriteAsync(response, cancellationToken);
                }
            }
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception ex)
        {
            _log($"Client error: {ex.Message}");
        }
        finally
        {
            lock (_clientSync)
            {
                _clients.Remove(client);
            }

            _log($"Client disconnected: {remoteEndPoint}");
        }
    }

    private byte[] ProcessRequest(byte[] pdu)
    {
        var functionCode = pdu[0];
        return functionCode switch
        {
            0x03 => ProcessReadHoldingRegisters(pdu),
            0x06 => ProcessWriteSingleRegister(pdu),
            0x10 => ProcessWriteMultipleRegisters(pdu),
            _ => BuildException(functionCode, 0x01),
        };
    }

    private byte[] ProcessReadHoldingRegisters(byte[] pdu)
    {
        if (pdu.Length < 5)
        {
            return BuildException(0x03, 0x03);
        }

        var start = (ushort)((pdu[1] << 8) | pdu[2]);
        var count = (ushort)((pdu[3] << 8) | pdu[4]);
        if (count == 0 || count > 125)
        {
            return BuildException(0x03, 0x03);
        }

        if (!_dataStore.TryReadRegisters(start, count, out var registers))
        {
            return BuildException(0x03, 0x02);
        }

        var response = new byte[2 + (count * 2)];
        response[0] = 0x03;
        response[1] = (byte)(count * 2);
        for (var index = 0; index < registers.Length; index++)
        {
            response[2 + (index * 2)] = (byte)(registers[index] >> 8);
            response[3 + (index * 2)] = (byte)(registers[index] & 0xFF);
        }

        _log($"Modbus read HR start={start} count={count}");
        return response;
    }

    private byte[] ProcessWriteSingleRegister(byte[] pdu)
    {
        if (pdu.Length < 5)
        {
            return BuildException(0x06, 0x03);
        }

        var address = (ushort)((pdu[1] << 8) | pdu[2]);
        var value = (ushort)((pdu[3] << 8) | pdu[4]);
        if (!_dataStore.TryWriteRegisters(address, [value], out var exceptionCode))
        {
            return BuildException(0x06, exceptionCode);
        }

        _log($"Modbus write single HR address={address} value=0x{value:X4}");
        return [0x06, pdu[1], pdu[2], pdu[3], pdu[4]];
    }

    private byte[] ProcessWriteMultipleRegisters(byte[] pdu)
    {
        if (pdu.Length < 6)
        {
            return BuildException(0x10, 0x03);
        }

        var start = (ushort)((pdu[1] << 8) | pdu[2]);
        var count = (ushort)((pdu[3] << 8) | pdu[4]);
        var byteCount = pdu[5];
        if (count == 0 || count > 123 || byteCount != count * 2 || pdu.Length < 6 + byteCount)
        {
            return BuildException(0x10, 0x03);
        }

        var values = new ushort[count];
        for (var index = 0; index < count; index++)
        {
            values[index] = (ushort)((pdu[6 + (index * 2)] << 8) | pdu[7 + (index * 2)]);
        }

        if (!_dataStore.TryWriteRegisters(start, values, out var exceptionCode))
        {
            return BuildException(0x10, exceptionCode);
        }

        _log($"Modbus write multi HR start={start} count={count}");
        return [0x10, pdu[1], pdu[2], pdu[3], pdu[4]];
    }

    private static byte[] BuildException(byte functionCode, byte exceptionCode)
        => [(byte)(functionCode | 0x80), exceptionCode];

    private static async Task<byte[]> ReadExactAsync(NetworkStream stream, int byteCount, CancellationToken cancellationToken)
    {
        var buffer = new byte[byteCount];
        var offset = 0;
        while (offset < byteCount)
        {
            var read = await stream.ReadAsync(buffer.AsMemory(offset, byteCount - offset), cancellationToken);
            if (read == 0)
            {
                return [];
            }

            offset += read;
        }

        return buffer;
    }

    public async ValueTask DisposeAsync()
    {
        await StopAsync();
    }
}
