using System;
using System.IO;
using FluentFTP;
using FluentFTP.Helpers;
using Xunit;
using Xunit.Abstractions;

namespace AppBuilderConsoleExtension.Tests;

public class FtpClientTest
{
    private readonly ITestOutputHelper _testOutputHelper;

    public FtpClientTest(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    [Fact]
    public void ConnectTest()
    {
        using var client = new FtpClient("localhost", "appbuilder", "0000");
        client.Connect();
        Assert.True(client.IsConnected);
        client.Disconnect();
    }
    
    private const string Path = "D:/workspace/unity/AppBuilder/Tools/AppBuilderConsoleExtension";

    [Theory]
    [InlineData(Path + "/FtpServer")]
    [InlineData(Path + "/FtpServer/android/AppBuilder.apk")]
    public void 로컬패스_구분_테스트(string localPath)
    {
        _testOutputHelper.WriteLine(localPath);
        _testOutputHelper.WriteLine(File.Exists(localPath).ToString());
        _testOutputHelper.WriteLine(Directory.Exists(localPath).ToString());
    }

    [Fact]
    public void DisposableExceptionTest()
    {
        _testOutputHelper.WriteLine("in");
        using (new Program(_testOutputHelper))
        {
            throw new Exception();
        }
        _testOutputHelper.WriteLine("out");
    }

    public class Program : IDisposable
    {
        private readonly ITestOutputHelper _testOutputHelper;

        public Program(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        public void Dispose()
        {
            _testOutputHelper.WriteLine("end");
        }
    }
    
}