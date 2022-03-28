// See https://aka.ms/new-console-template for more information

var builder = ConsoleApp.CreateBuilder(args, options => { });
var app = builder.Build();

app.AddSubCommands<AdbCommands>();

app.Run();