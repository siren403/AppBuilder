using System.Text.Json;
using Cysharp.Diagnostics;

namespace AppBuilderConsoleExtension.Commands;

[Command("adb")]
public class AdbCommands : ConsoleAppBase
{
    [Command("devices")]
    public async Task Devices()
    {
        var result = new
        {
            devices = new List<string>()
        };

        await foreach (string device in ProcessX.StartAsync("adb devices"))
        {
            if (device.StartsWith("List") || string.IsNullOrEmpty(device)) continue;
            result.devices.Add(device.Split("\t").First());
        }

        Console.WriteLine(JsonSerializer.Serialize(result));
    }

    private const string UnityActivity = "com.unity3d.player.UnityPlayerActivity";

    //todo: string packageName -> to start
    [Command("install")]
    public async Task Install(string device, string apk, string? package = null)
    {
        await ProcessX.StartAsync($"adb -s {device} install -r {apk}").WaitAsync();
        Console.WriteLine("installed");

        if (string.IsNullOrEmpty(package)) return;

        await ProcessX.StartAsync($"adb -s {device}  shell am start -n {package}/{UnityActivity}").WaitAsync();
        Console.WriteLine("started");
    }
}