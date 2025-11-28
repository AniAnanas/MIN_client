namespace Client.Net
{
    public enum PacketType : ushort
    {
        PacketError = 0x0001,
        RegisterRequest = 0x0002,
        RegisterResponse = 0x0003,
        LoginRequest = 0x0004,
        LoginResponse = 0x0005,
        AuthTokenRequest = 0x0006,
        AuthResponse = 0x0007,
        SendMessage = 0x0008,
        ReceiveMessage = 0x0009,
        HistoryRequest = 0x000A,
        HistoryResponse = 0x000B,
        UserListRequest = 0x000C,
        UserListResponse = 0x000D,
        Ping = 0x000E,
        Pong = 0x000F,
        SearchUsersRequest = 0x0010,
        SearchUsersResponse = 0x0011,
        LogoutRequest = 0x0012,
        UserStatusUpdate = 0x0013,
        SendMessageResponse = 0x0014
    }

    public enum ErrorCode : ushort
    {
        AuthFailed = 1000,
        UserExists = 1001,
        InvalidToken = 1002,
        Unauthorized = 1003,
        UserNotFound = 1004,
        DatabaseError = 2000,
        InvalidPacket = 3000
    }
}
