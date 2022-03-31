using AppBuilderExtension.Ftp;
using Microsoft.Extensions.Options;

namespace AppBuilderConsoleExtension.Commands;

public class FtpConfig
{
    public Dictionary<string, Client.Profile> Profiles { get; set; }
}

[Command("ftp")]
public class FtpCommands : ConsoleAppBase
{
    [Command("upload-profile")]
    public async Task Upload(IOptions<FtpConfig> options, string name,
        [Option("local")] string? localPath = null,
        [Option("remote")] string? remotePath = null)
    {
        if (options.Value.Profiles == null)
        {
            Console.WriteLine("not found profiles");
            return;
        }

        foreach (var key in options.Value.Profiles.Keys)
        {
            Console.WriteLine(key);
        }

        if (!options.Value.Profiles.TryGetValue(name, out var profile))
        {
            Console.WriteLine($"not found profile: {name}");
            return;
        }

        if (localPath != null) profile.LocalPath = localPath;
        if (remotePath != null) profile.RemotePath = remotePath;

        using var client = new Client(profile);

        var isConnected = await client.Connect();

        if (isConnected)
        {
            Console.WriteLine("connect success");
            await client.Upload();
        }
        else
        {
            Console.WriteLine("connect failed");
        }

        await client.Disconnect();
    }

    [Command("upload")]
    public async Task Upload(string host, string user, string passwd, string local, string remote)
    {
        using var client = new Client(new Client.Profile()
        {
            Host = host,
            User = user,
            Passwd = passwd,
            LocalPath = local,
            RemotePath = remote
        });

        await client.Connect();

        await client.Upload();

        await client.Disconnect();
    }
}