using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.Text;
using System.Net.Sockets;
using Client.Shared.Interfaces;
using Client.Models;

namespace Client.Net;

public class NetworkService : INetworkService
{
    private TcpClient _client;
    private NetworkStream _stream;

    private CancellationTokenSource _cts;
    private bool _isConnected;

    public long CurrentUserId { get; private set; } = -1;

    public event Action<MessageModel> MessageReceived;
    public event Action<UserModel[]> UsersReceived = (_)=>{};
    public event Action<(long userId, bool isOnline)> UserStatusChanged;

    private TaskCompletionSource<long> _loginTcs;
    private TaskCompletionSource<(long userId, string token)> _tokenTcs;
    private TaskCompletionSource<long> _sendedMsgTcs;
    private TaskCompletionSource<UserModel[]> _getUsersTcs;
    private TaskCompletionSource<MessageModel[]> _historyTcs;
    private TaskCompletionSource<UserModel[]> _searchTcs;

    class PingInfo
    {
        public DateTime LastSentAt { get; set; }
        public DateTime LastGetAt { get; set; }
        public double LastPingMs 
        {
            get 
            {
                return (LastGetAt.Ticks - LastSentAt.Ticks) / (double)10000;
            }
        }
    }

    private PingInfo _pingInfo;
    public double PingToServerMs => _pingInfo?.LastPingMs ?? -1;

    public async Task<bool> ConnectAsync(string host, int port)
    {
        if (_client != null && _client.Connected)
        {
            Log.Warn("Already connected.");
            return true;
        }
        try
        {
            _client = new TcpClient();
            await _client.ConnectAsync(host, port);
            _stream = _client.GetStream();
            _isConnected = true;
            _cts = new CancellationTokenSource();

            // Запускаем цикл чтения
            _ = Task.Run(() => ReceiveLoop(_cts.Token));
            
            _pingInfo = new PingInfo();
            await PingAsync();
            
            //цикл пингов
            _ = Task.Run(() => { do { Task.WaitAll(PingAsync(), Task.Delay(15 * 1000)); } while (!_cts.Token.IsCancellationRequested); });
            
            Log.Info($"Connected to {host}:{port}, ping: {PingToServerMs}ms");
            return true;
        }
        catch (Exception ex)
        {
            Log.Error($"Connection failed: {ex.Message}");
            return false;
        }
    }

    public async Task<(long userId, string token)> RegisterAsync(string username, string password)
    {
        _tokenTcs = new TaskCompletionSource<(long userId, string token)>();

        var packet = PacketBuilder.Create(PacketType.RegisterRequest, bw =>
        {
            bw.WriteString(username);
            bw.WriteString(password);
        });

        await SendDataAsync(packet);

        var task = _tokenTcs.Task;
        if (await Task.WhenAny(task, Task.Delay(5000)) == task)
        {
            var result = await task;
            CurrentUserId = result.userId;
            return result;
        }

        _tokenTcs.TrySetCanceled();
        throw new TimeoutException("Registration timed out");
    }

    public async Task<(long userId, string token)> LoginAsync(string username, string password)
    {
        _tokenTcs = new TaskCompletionSource<(long userId, string token)>();

        var packet = PacketBuilder.Create(PacketType.LoginRequest, bw =>
        {
            bw.WriteString(username);
            bw.WriteString(password);
        });

        await SendDataAsync(packet);

        // Ждем ответа или таймаута
        var task = _tokenTcs.Task;
        if (await Task.WhenAny(task, Task.Delay(5000)) == task)
        {
            var result = await task;
            CurrentUserId = result.userId;
            return result;
        }

        _tokenTcs.TrySetCanceled();
        throw new TimeoutException("Login timed out");
    }

    public async Task<long> AuthorizeAsync(string token)
    {
        _loginTcs = new TaskCompletionSource<long>();

        var packet = PacketBuilder.Create(PacketType.AuthTokenRequest, bw =>
        {
            bw.WriteString(token);
        });

        await SendDataAsync(packet);

        var task = _loginTcs.Task;
        if (await Task.WhenAny(task, Task.Delay(5000)) == task)
        {
            return CurrentUserId = await task;
        }
        return -1;
    }

    public async Task<long> SendMessageAsync(long recipientId, string text)
    {
        if (!_isConnected) throw new Exception("Not connected to server");
        _sendedMsgTcs = new();

        var packet = PacketBuilder.Create(PacketType.SendMessage, bw =>
        {
            bw.Write(recipientId);
            bw.WriteString(text);
        });

        await SendDataAsync(packet);

        var task = _sendedMsgTcs.Task;
        if (await Task.WhenAny(task, Task.Delay(5000)) == task)
        {
            return await task;
        }

        return default;
    }

    public async Task<MessageModel[]> GetHistoryAsync(long userId, int limit = 50)
    {
        _historyTcs = new();

        // user должен быть ID
        var packet = PacketBuilder.Create(PacketType.HistoryRequest, bw =>
        {
            bw.Write(userId); // User ID
            bw.Write((uint)limit); // лимит сообщений
        });

        await SendDataAsync(packet);

        var task = _historyTcs.Task;
        if (await Task.WhenAny(task, Task.Delay(5000)) == task)
        {
            return await task;
        }

        return [];
    }

    public async Task<UserModel[]> GetUserListAsync()
    {
        _getUsersTcs = new();

        var packet = PacketBuilder.Create(PacketType.UserListRequest);
        
        await SendDataAsync(packet);

        var task = _getUsersTcs.Task;
        if (await Task.WhenAny(task, Task.Delay(5000)) == task)
        {
            return await task;
        }

        return [];
    }

    public async Task PingAsync()
    {
        var packet = PacketBuilder.Create(PacketType.Ping);
        _pingInfo.LastSentAt = DateTime.Now;
        await SendDataAsync(packet);
    }

    public async Task<UserModel[]> SearchUsersAsync(string query)
    {
        _searchTcs = new();

        var packet = PacketBuilder.Create(PacketType.SearchUsersRequest, bw =>
        {
            bw.WriteString(query);
        });

        await SendDataAsync(packet);

        var task = _searchTcs.Task;
        if (await Task.WhenAny(task, Task.Delay(5000)) == task)
        {
            return await task;
        }

        return [];
    }

    private async Task SendDataAsync(byte[] data)
    {
        if (!_isConnected) return;
        try
        {
            await _stream.WriteAsync(data.AsMemory());
            await _stream.FlushAsync();
        }
        catch (Exception ex)
        {
            Log.Error($"Send error: {ex.Message}");
            _isConnected = false;
        }
    }
    public async Task Disconnect(bool sendPacket = false)
    {
        if (sendPacket)
            await SendDataAsync(PacketBuilder.Create(PacketType.LogoutRequest));
        _cts.Cancel();
    }
    private async Task ReceiveLoop(CancellationToken token)
    {
        Log.Info("Loop started.");
        try
        {
            while (!token.IsCancellationRequested && _isConnected)
            {
                // читаем длину пакета (4 байта)
                byte[] lengthBuffer = new byte[4];
                int bytesRead = await ReadExactAsync(lengthBuffer, 4, token);
                if (bytesRead == 0) break; // Disconnected

                int packetLength = BitConverter.ToInt32(lengthBuffer, 0);

                // остальное тело пакета + его ID
                int bodyLength = packetLength - 4;
                if (bodyLength < 2) throw new Exception("Invalid packet size"); //2 байта для ID

                byte[] bodyBuffer = new byte[bodyLength];
                if (await ReadExactAsync(bodyBuffer, bodyLength, token) != bodyLength) break;

                // парсим пакет
                using MemoryStream ms = new(bodyBuffer);
                using BinaryReader br = new(ms);
                ProcessPacket(br);
            }
        }
        catch (Exception ex)
        {
            Log.Error($"Receive loop error: {ex.Message}");
        }
        finally
        {
            _isConnected = false;
            _stream?.Close();
            _client?.Close();
            Log.Info("Disconnected");
        }
    }

    private async Task<int> ReadExactAsync(byte[] buffer, int count, CancellationToken token)
    {
        int totalRead = 0;
        while (totalRead < count)
        {
            int read = await _stream.ReadAsync(buffer, totalRead, count - totalRead, token);
            if (read == 0) return 0;
            totalRead += read;
        }
        return totalRead;
    }

    private void ProcessPacket(BinaryReader br)
    {
        // читаем ID (2 байта)
        ushort packetIdRaw = br.ReadUInt16();
        PacketType type = (PacketType)packetIdRaw;

        switch (type)
        {
            case PacketType.PacketError:
                {
                    ushort errorCode = br.ReadUInt16();
                    string errorMsg = br.ReadEncodedString();
                    Log.Error($"Server Error [{errorCode}]: {errorMsg}");

                    // если мы ждали чето, отменяем ожидание
                    _tokenTcs?.TrySetException(new Exception(errorMsg));
                    _loginTcs?.TrySetException(new Exception(errorMsg));
                    _sendedMsgTcs?.TrySetException(new Exception(errorMsg));
                    _getUsersTcs?.TrySetException(new Exception(errorMsg));
                    _historyTcs?.TrySetException(new Exception(errorMsg));
                    _searchTcs?.TrySetException(new Exception(errorMsg));
                    break;
                }
            case PacketType.RegisterResponse:
                {
                    bool success = br.ReadByte() != 0;
                    long UserId = br.ReadInt64();
                    string Token = br.ReadEncodedString();

                    if (success) _tokenTcs?.TrySetResult((UserId, Token));
                    else _tokenTcs?.TrySetException(new Exception("Registration failed"));
                    break;
                }
            case PacketType.LoginResponse:
                {
                    bool success = br.ReadByte() != 0;
                    long UserId = br.ReadInt64();
                    string Token = br.ReadEncodedString();

                    if (success) _tokenTcs?.TrySetResult((UserId, Token));
                    else _tokenTcs?.TrySetException(new Exception("Login failed"));
                    break;
                }
            case PacketType.AuthResponse:
                {
                    bool success = br.ReadByte() != 0;
                    long UserId = br.ReadInt64();
                    if (success) _loginTcs?.TrySetResult(UserId);
                    else _loginTcs?.TrySetException(new Exception("Auth failed"));
                    break;
                }
            case PacketType.SendMessageResponse:
                {
                    long msgId = br.ReadInt64();
                    if (msgId != -1) _sendedMsgTcs?.TrySetResult(msgId);
                    else _sendedMsgTcs?.TrySetException(new Exception("Sended message failed, Id = -1"));
                    break;
                }
            case PacketType.ReceiveMessage:
                {
                    long msgId = br.ReadInt64();
                    long timestamp = br.ReadInt64();
                    long senderId = br.ReadInt64();
                    long recipientId = br.ReadInt64();
                    string body = br.ReadEncodedString();

                    var msgModel = new MessageModel(
                        msgId,
                        body,
                        DateTimeOffset.FromUnixTimeSeconds(timestamp).LocalDateTime,
                        new UserModel(senderId, senderId.ToString()) // Пока имя неизвестно
                    );

                    MessageReceived?.Invoke(msgModel);
                    break;
                }
            case PacketType.HistoryResponse:
                {
                    uint msgCount = br.ReadUInt32();
                    var messages = new List<MessageModel>();
                    for (int i = 0; i < msgCount; i++)
                    {
                        long msgId = br.ReadInt64();
                        long timestamp = br.ReadInt64();
                        long senderId = br.ReadInt64();
                        /*long recipientId*/ _ = br.ReadInt64();
                        string text = br.ReadEncodedString();
                        var hMsgModel = new MessageModel(
                            msgId,
                            text,
                            DateTimeOffset.FromUnixTimeSeconds(timestamp).LocalDateTime,
                            new UserModel(senderId, senderId.ToString()) // Пока имя неизвестно
                        );
                        messages.Add(hMsgModel);
                    }

                    _historyTcs?.SetResult([.. messages]);

                    break;
                }
            case PacketType.UserListResponse:
                {
                    uint count = br.ReadUInt32();
                    var users = new List<UserModel>();
                    for (int i = 0; i < count; i++)
                    {
                        long userId = br.ReadInt64();
                        string username = br.ReadEncodedString();
                        bool status = br.ReadByte() != 0;

                        var userModel = new UserModel(userId, username, username) { IsOnline = status };
                        users.Add(userModel);
                    }

                    _getUsersTcs?.SetResult([.. users]);
                    //UsersReceived?.Invoke([.. users]);
                    break;
                }
            case PacketType.Pong:
                {
                    _pingInfo.LastGetAt = DateTime.Now;
                    Log.Info("Get PONG (0x000F): ping - {0}ms".SFormat(PingToServerMs));
                    break;
                }

            case PacketType.SearchUsersResponse:
                {
                    ushort count = br.ReadUInt16();
                    var results = new List<UserModel>();
                    for (int i = 0; i < count; i++)
                    {
                        long userId = br.ReadInt64();
                        string username = br.ReadEncodedString();
                        bool isOnline = br.ReadByte() != 0;
                        results.Add(new UserModel(userId, username, username) { IsOnline = isOnline });
                    }

                    _searchTcs?.SetResult([.. results]);

                    break;
                }
            case PacketType.UserStatusUpdate:
                {
                    long userId = br.ReadInt64();
                    bool isOnline = br.ReadByte() != 0;
                    UserStatusChanged?.Invoke((userId, isOnline));
                    break;
                }
            case PacketType.Ping:
                _ = SendDataAsync(PacketBuilder.Create(PacketType.Pong));
                break;
        }
    }
}