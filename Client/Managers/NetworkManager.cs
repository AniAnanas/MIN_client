using Client.Services.Interfaces;
using System.Net.Sockets;
using System.Net.Http;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Client.Managers
{
    public class NetworkManager
    {
        private readonly ILoggingService _logger;
        private readonly HttpClient _httpClient;
        private TcpClient? _tcpClient;
        private NetworkStream? _networkStream;

        // Events for different message types
        public event EventHandler<ChatMessage>? ChatMessageReceived;
        public event EventHandler<UserStatusMessage>? UserStatusReceived;
        public event EventHandler<FileMessage>? FileMessageReceived;
        public event EventHandler<AuthResponse>? AuthResponseReceived;

        public NetworkManager(ILoggingService logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri("https://api.yourmessenger.com/"); // TODO: Load from config
        }

        #region HTTP API Methods (for auth/registration)

        public async Task<AuthResponse> RegisterAsync(string username, string email, string password)
        {
            try
            {
                var request = new RegisterRequest
                {
                    Username = username,
                    Email = email,
                    Password = password
                };

                var response = await _httpClient.PostAsync("auth/register",
                    new ByteArrayContent(request.ToByteArray()));

                if (response.IsSuccessStatusCode)
                {
                    var data = await response.Content.ReadAsByteArrayAsync();
                    //var authResponse = AuthResponse.Parser.ParseFrom(data);
                    //AuthResponseReceived?.Invoke(this, authResponse);
                    //return authResponse;
                    return new AuthResponse { Success = true, Message = "Registration failed" };
                }
                else
                {
                    _logger.Error($"Registration failed: {response.StatusCode}");
                    return new AuthResponse { Success = false, Message = "Registration failed" };
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Registration error: {ex.Message}");
                return new AuthResponse { Success = false, Message = ex.Message };
            }
        }

        public async Task<AuthResponse> LoginAsync(string username, string password)
        {
            try
            {
                var request = new LoginRequest
                {
                    Username = username,
                    Password = password
                };

                var response = await _httpClient.PostAsync("auth/login",
                    new ByteArrayContent(request.ToByteArray()));

                if (response.IsSuccessStatusCode)
                {
                    var data = await response.Content.ReadAsByteArrayAsync();
                    //var authResponse = AuthResponse.Parser.ParseFrom(data);
                    //AuthResponseReceived?.Invoke(this, authResponse);

                    //if (authResponse.Success)
                    //{
                    //    // Connect to TCP socket after successful auth
                    //    await ConnectTcpAsync(authResponse.ServerAddress, authResponse.ServerPort);
                    //}

                    //return authResponse;
                    return new AuthResponse { Success = true, Message = "Login failed" };
                }
                else
                {
                    _logger.Error($"Login failed: {response.StatusCode}");
                    return new AuthResponse { Success = false, Message = "Login failed" };
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Login error: {ex.Message}");
                return new AuthResponse { Success = false, Message = ex.Message };
            }
        }

        public async Task LogoutAsync()
        {
            try
            {
                await _httpClient.PostAsync("auth/logout", null);
                await DisconnectTcpAsync();
                _logger.Info("Logged out successfully");
            }
            catch (Exception ex)
            {
                _logger.Error($"Logout error: {ex.Message}");
            }
        }

        #endregion

        #region TCP Socket Methods (for real-time communication)

        public async Task ConnectTcpAsync(string serverAddress, int port)
        {
            try
            {
                _tcpClient = new TcpClient();
                await _tcpClient.ConnectAsync(serverAddress, port);
                _networkStream = _tcpClient.GetStream();

                _logger.Info($"Connected to TCP server: {serverAddress}:{port}");

                // Start listening for messages
                _ = ListenForMessagesAsync();
            }
            catch (Exception ex)
            {
                _logger.Error($"TCP connection error: {ex.Message}");
                throw;
            }
        }

        public async Task DisconnectTcpAsync()
        {
            try
            {
                if (_networkStream != null)
                {
                    await _networkStream.DisposeAsync();
                    _networkStream = null;
                }

                if (_tcpClient != null)
                {
                    _tcpClient.Dispose();
                    _tcpClient = null;
                }

                _logger.Info("Disconnected from TCP server");
            }
            catch (Exception ex)
            {
                _logger.Error($"TCP disconnection error: {ex.Message}");
            }
        }

        public async Task SendMessageAsync(IMessage message)
        {
            if (_networkStream == null)
            {
                _logger.Error("No active TCP connection");
                return;
            }

            try
            {
                var data = message.ToByteArray();
                var length = BitConverter.GetBytes(data.Length);

                await _networkStream.WriteAsync(length);
                await _networkStream.WriteAsync(data);

                _logger.Info($"Sent message of type: {message.GetType().Name}");
            }
            catch (Exception ex)
            {
                _logger.Error($"Send message error: {ex.Message}");
            }
        }

        private async Task ListenForMessagesAsync()
        {
            if (_networkStream == null) return;

            try
            {
                var buffer = new byte[1024];

                while (_tcpClient?.Connected == true)
                {
                    // Read message length
                    var lengthBuffer = new byte[4];
                    var bytesRead = await _networkStream.ReadAsync(lengthBuffer, 0, 4);
                    if (bytesRead != 4) break;

                    var messageLength = BitConverter.ToInt32(lengthBuffer);

                    // Read message data
                    var messageBuffer = new byte[messageLength];
                    bytesRead = 0;
                    while (bytesRead < messageLength)
                    {
                        bytesRead += await _networkStream.ReadAsync(messageBuffer, bytesRead, messageLength - bytesRead);
                    }

                    // Parse and handle message
                    await HandleMessageAsync(messageBuffer);
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Listen messages error: {ex.Message}");
            }
        }

        private async Task HandleMessageAsync(byte[] data)
        {
            try
            {
                // Try to parse as different message types
                // This is a simplified version - in real implementation you'd have proper message type detection

                // Example: Handle chat message
                try
                {
                    //var chatMessage = ChatMessage.Parser.ParseFrom(data);
                    //ChatMessageReceived?.Invoke(this, chatMessage);
                    //_logger.Info($"Received chat message: {chatMessage.Id}");
                }
                catch
                {
                    // Try other message types...
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Handle message error: {ex.Message}");
            }
        }

        #endregion

        public bool IsConnected => _tcpClient?.Connected == true;

        public void Dispose()
        {
            _httpClient.Dispose();
            _ = DisconnectTcpAsync();
        }
    }

    // Protobuf message definitions (simplified)
    public class RegisterRequest : IMessage
    {
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;

        public MessageDescriptor Descriptor => throw new NotImplementedException();

        public ByteString ToByteString() => ByteString.CopyFrom(ToByteArray());
        public void WriteTo(CodedOutputStream output) { /* Implementation */ }
        public int CalculateSize() => 0;
        public void MergeFrom(CodedInputStream input) { /* Implementation */ }
        public byte[] ToByteArray() => Array.Empty<byte>();
    }

    public class LoginRequest : IMessage
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;

        public MessageDescriptor Descriptor => throw new NotImplementedException();
        public ByteString ToByteString() => ByteString.CopyFrom(ToByteArray());
        public void WriteTo(CodedOutputStream output) { /* Implementation */ }
        public int CalculateSize() => 0;
        public void MergeFrom(CodedInputStream input) { /* Implementation */ }
        public byte[] ToByteArray() => Array.Empty<byte>();
    }

    public class AuthResponse : IMessage
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string ServerAddress { get; set; } = string.Empty;
        public int ServerPort { get; set; }

        public MessageDescriptor Descriptor => throw new NotImplementedException();
        public ByteString ToByteString() => ByteString.CopyFrom(ToByteArray());
        public void WriteTo(CodedOutputStream output) { /* Implementation */ }
        public int CalculateSize() => 0;
        public void MergeFrom(CodedInputStream input) { /* Implementation */ }
        public byte[] ToByteArray() => Array.Empty<byte>();
    }

    public class ChatMessage : IMessage
    {
        public string Id { get; set; } = string.Empty;
        public string ChatId { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
        public string SenderId { get; set; } = string.Empty;
        public long Timestamp { get; set; }

        public MessageDescriptor Descriptor => throw new NotImplementedException();
        public ByteString ToByteString() => ByteString.CopyFrom(ToByteArray());
        public void WriteTo(CodedOutputStream output) { /* Implementation */ }
        public int CalculateSize() => 0;
        public void MergeFrom(CodedInputStream input) { /* Implementation */ }
        public byte[] ToByteArray() => Array.Empty<byte>();
    }

    public class UserStatusMessage : IMessage
    {
        public string UserId { get; set; } = string.Empty;
        public bool IsOnline { get; set; }
        public long LastSeen { get; set; }

        public MessageDescriptor Descriptor => throw new NotImplementedException();
        public ByteString ToByteString() => ByteString.CopyFrom(ToByteArray());
        public void WriteTo(CodedOutputStream output) { /* Implementation */ }
        public int CalculateSize() => 0;
        public void MergeFrom(CodedInputStream input) { /* Implementation */ }
        public byte[] ToByteArray() => Array.Empty<byte>();
    }

    public class FileMessage : IMessage
    {
        public string Id { get; set; } = string.Empty;
        public string ChatId { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public byte[] FileData { get; set; } = Array.Empty<byte>();

        public MessageDescriptor Descriptor => throw new NotImplementedException();
        public ByteString ToByteString() => ByteString.CopyFrom(ToByteArray());
        public void WriteTo(CodedOutputStream output) { /* Implementation */ }
        public int CalculateSize() => 0;
        public void MergeFrom(CodedInputStream input) { /* Implementation */ }
        public byte[] ToByteArray() => Array.Empty<byte>();
    }
}
