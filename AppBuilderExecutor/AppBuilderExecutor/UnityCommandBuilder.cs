using System.Text;
using Zx;

public readonly struct UnityCommandBuilder
{
    private readonly string _editorPath;
    private readonly StringBuilder _commands = new StringBuilder();

    public UnityCommandBuilder(string editorPath, string projectPath)
    {
        if (string.IsNullOrEmpty(editorPath) || string.IsNullOrEmpty(projectPath))
        {
            throw new ArgumentException();
        }

        _editorPath = editorPath;
        AddArgument("projectPath", projectPath);
    }

    public UnityCommandBuilder AddArgument(string arg)
    {
        _commands.Append($" {arg}");
        return this;
    }

    public UnityCommandBuilder AddArgument(string name, string arg)
    {
        _commands.Append($" -{name} '{arg}'");
        return this;
    }


    public UnityCommandBuilder BatchMode()
    {
        AddArgument("-batchmode");
        return this;
    }

    public UnityCommandBuilder NoGraphics()
    {
        AddArgument("-nographics");
        return this;
    }

    public UnityCommandBuilder Quit()
    {
        AddArgument("-quit");
        return this;
    }

    public UnityCommandBuilder ExecuteMethod(string method)
    {
        if (string.IsNullOrEmpty(method))
        {
            throw new ArgumentException($"{nameof(method)} is empty");
        }

        AddArgument("executeMethod", method);
        return this;
    }

    public UnityCommandBuilder BuildTarget(string target)
    {
        AddArgument("buildTarget", target);
        return this;
    }

    public UnityCommandBuilder BuildTarget(Platform platform)
    {
        var str = platform switch
        {
            Platform.Android => "Android",
            Platform.IOS => "iOS",
            _ => string.Empty
        };

        if (!string.IsNullOrEmpty(str))
        {
            BuildTarget(str);
        }

        return this;
    }

    public UnityCommandBuilder LogFile(string path)
    {
        if (string.IsNullOrEmpty(path))
        {
            throw new ArgumentException($"{nameof(path)} is empty");
        }

        AddArgument("logFile", path);
        return this;
    }

    public async Task Build()
    {
        await $"cd {_editorPath}";
        _commands.Insert(0, "./Unity.exe");
        await _commands.ToString();
    }

    public override string ToString()
    {
        return _commands.ToString();
    }

    public enum Platform
    {
        Android,
        IOS,
    }
}