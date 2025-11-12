using System.Collections.Generic;
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
    private StreamReader _reader;
    private StreamWriter _writer;

    public event Action<MessageModel> MessageRecieved;
    public event Action<string[]> UsersRecieved;
    public event Action<MessageModel> MessageReceived;
    public event Action<string[]> UsersReceived;

    public bool recievingHistory = false;

    public async Task<bool> ConnectAsync(string host, int port)
    {
        _client = new();
        await _client.ConnectAsync(host, port);
        _stream = _client.GetStream();
        _reader = new StreamReader(_stream, Encoding.UTF8);
        _writer = new StreamWriter(_stream, Encoding.UTF8) { AutoFlush = true };
        _ = Task.Run(RecieveLoop);
        return true;
    }

    public async Task<string> LoginAsync(string username, string password)
    {
        await _writer.WriteLineAsync($"[LOGIN]{username};{password}");
        string resp = await ReadResponse("[TOKEN]");

        return resp;
    }

    public async Task<string> RegisterAsync(string username, string password)
    {
        await _writer.WriteLineAsync($"[REG]{username};{password}");
        return await ReadResponse("[TOKEN]");
    }

    public async Task<bool> AuthorizeAsync(string token)
    {
        await _writer.WriteLineAsync($"[AUTH]{token}");
        try
        {
            string resp = await ReadResponse("[OK]");
            if (resp != "AUTH")
            {
                return false;
            }
        }
        catch (Exception e)
        {
            if (e.Message.Contains("invalid_token"))
            {
                return false;
            }
        }
        return true;
    }

    public async Task<bool> SendMessageAsync(string sender, string recipient, string body)
    {
        await _writer.WriteLineAsync($"[MSG]{sender}>{recipient}:{body}");
        return true;
    }

    public async Task<string[]> GetUsersAsync()
    {
        throw new NotImplementedException("Server doesn't supports manual getting users, try LoginAsync, RegisterAsync, AuthorizeAsync");
    }

    private async Task RecieveLoop()
    {
        string line;
        while ((line = await _reader.ReadLineAsync()) != null)
        {
            if (line.StartsWith("[MSG]"))
            {
                var msg = ParseMsg(line[5..]);
                if (msg != null) MessageRecieved?.Invoke(msg);
            }
            else if (line.StartsWith("[USERS]"))
            {
                var users = line[7..].Split(';', StringSplitOptions.RemoveEmptyEntries);
                UsersRecieved?.Invoke(users);
            }
            else if (line.StartsWith("[HIST]BEGIN"))
            {
                recievingHistory = true;
            }
            else if (line.StartsWith("[HIST]END"))
            {
                recievingHistory = false;
            }
            else
            {

            }
        }
    }

    private static MessageModel? ParseMsg(string line) 
    {
        // sender>recipient:body

        int firstSplit = line.IndexOf('>');
        int secondSplit = line.IndexOf(':');

        if (firstSplit < 0 || secondSplit < 0) return null;

        string sender = line[..firstSplit];
        string recipient = line.Substring(firstSplit + 1, secondSplit - firstSplit - 1);
        string body = line[(secondSplit + 1)..];

        return new MessageModel(0, body, DateTime.Now, new UserModel(0, sender));
    }
    private async Task<string> ReadResponse(string marker)
    {
        string? line;
        while ((line = await _reader.ReadLineAsync()) != null)
        {
            if (line.StartsWith(marker))
            {
                return line[marker.Length..];
            }
            else if (line.StartsWith("[ERR]"))
            {
                throw new Exception(line[5..]);
            }
        }
        return "";
    }

    public Task<string[]> GetPeersAsync()
    {
        throw new NotImplementedException();
    }

    public Task<MessageModel[]> GetHistoryAsync(string user, int id, int count, bool upper = false)
    {
        throw new NotImplementedException();
    }
}
