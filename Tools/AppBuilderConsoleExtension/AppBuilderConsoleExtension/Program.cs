// See https://aka.ms/new-console-template for more information

using AppBuilderConsoleExtension.Commands;
using Microsoft.Extensions.DependencyInjection;

var builder = ConsoleApp.CreateBuilder(args, options => { });
builder.ConfigureServices((ctx, services) =>
{
    services.Configure<FtpConfig>(ctx.Configuration.GetSection("Ftp"));
});
var app = builder.Build();

app.AddSubCommands<AdbCommands>();
app.AddSubCommands<FtpCommands>();

app.Run();