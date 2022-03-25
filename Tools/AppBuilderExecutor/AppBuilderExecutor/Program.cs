// See https://aka.ms/new-console-template for more information
// https://docs.unity3d.com/kr/2021.2/Manual/EditorCommandLineArguments.html

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Zx;
using static Zx.Env;


var builder = ConsoleApp.CreateBuilder(args);
builder.ConfigureServices((ctx, services) =>
{
    services.Configure<Config>(ctx.Configuration);
});

var app = builder.Build();
// app.AddAllCommandType();
app.AddRootCommand(async (IOptions<Config> config) =>
{
    shell = "powershell";

    // string env(string path) => $"$env:{path}";
    var builder = new UnityCommandBuilder(config.Value.Editor, config.Value.Project)
        .BatchMode()
        .NoGraphics()
        .Quit()
        .ExecuteMethod(config.Value.Method)
        .BuildTarget(config.Value.BuildTarget)
        .AddArgument("host", "127.0.0.1")
        .AddArgument("outputPath", "D:/workspace/unity/AppBuilder/Build/Android-Batch/Android");
    
    var hasLogFile = !string.IsNullOrEmpty(config.Value.LogFile);
    if (hasLogFile)
    {
        builder.LogFile(config.Value.LogFile);
    }
    // .AddArgument("variant", "Development")
    // .Build();
    
    Console.WriteLine(builder.ToString());
    await builder.Build();

    if (hasLogFile)
    {
        await $"ii {config.Value.LogFile}";
    }
});
app.AddCommand("print", async (IOptions<Config> config) =>
{
    shell = "powershell";

    // string env(string path) => $"$env:{path}";
    var builder = new UnityCommandBuilder(config.Value.Editor, config.Value.Project)
        .BatchMode()
        .NoGraphics()
        .Quit()
        .ExecuteMethod(config.Value.Method)
        .BuildTarget(config.Value.BuildTarget)
        .AddArgument("host", "127.0.0.1");

    await $"echo {builder}";

});

app.Run();


// await $"./Unity.exe -batchmode -nographics -quit -projectPath '{project}' -executeMethod '{method}' -buildTarget '{buildTarget}'";
// await $"./Unity.exe -batchmode -nographics -quit -projectPath '{project}' -executeMethod '{method}'";
// await $"code {await env("LOCALAPPDATA")}/Unity/Editor/Editor.log";

public class Config
{
    public string Editor { get; set; }
    public string Project { get; set; }
    public string BuildTarget { get; set; }
    public string Method { get; set; }
    public string? LogFile { get; set; }
}