using System.Runtime.CompilerServices;
using FluentFTP;

namespace AppBuilderExtension.Ftp;

public class Client : IDisposable
{
    public class Profile
    {
        public string Host { get; set; }
        public int Port { get; set; }
        public string User { get; set; }
        public string Passwd { get; set; }

        public string LocalPath { get; set; }
        public string RemotePath { get; set; }

        public bool UploadValidate() => string.IsNullOrEmpty(LocalPath) || string.IsNullOrEmpty(RemotePath);
    }

    private readonly Profile _profile;
    private readonly FtpClient _client;

    public Client(Profile profile)
    {
        _profile = profile;
        _client = new FtpClient(_profile.Host, _profile.User, _profile.Passwd);
    }

    public async Task<bool> Connect()
    {
        await _client.ConnectAsync();
        return _client.IsConnected;
    }

    public async Task Upload()
    {
        if (!_client.IsConnected)
        {
            throw new Exception("not connected client");
        }

        if (_profile.UploadValidate())
        {
            throw new Exception($"not validated path: {_profile.LocalPath} / {_profile.RemotePath}");
        }

        var local = _profile.LocalPath;
        var remote = _profile.RemotePath;
        
        if (Directory.Exists(local))
        {
            var results = await _client.UploadDirectoryAsync(local, remote,
                existsMode: FtpRemoteExists.Overwrite);
            foreach (var result in results)
            {
                Console.WriteLine($"[{result.IsSuccess}] {result.LocalPath} -> {result.RemotePath}");
            }
        }
        else if (File.Exists(local))
        {
            var result = await _client.UploadFileAsync(local, remote, FtpRemoteExists.Overwrite,
                true);
            
            Console.WriteLine($"[{result.ToString()}] {local} -> {remote}");
        }
        else
        {
            throw new ArgumentException($"{nameof(local)} - {local}");
        }
    }

    public Task Disconnect() => _client.DisconnectAsync();

    public void Dispose()
    {
        _client.Dispose();
    }
}