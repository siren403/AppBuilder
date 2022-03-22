AppBuilder
==

## Table of Contents

- [Quick Start](#quick-start)
- [AppSettings](#appsettings)

Quick Start
---
``` csharp
namespace Builds
{
    public static class UseCase
    {
        // Tool에 빌드 함수 전달
        [Build]
        public static void QuickStart()
        {
            BuildPlayer.Build((ctx, builder) =>
            {
                //현재 에디터 세팅으로 설정
                builder.ConfigureCurrentSettings();
            });
        }
    }
}
```
``` zsh
./Unity.exe -executeMethod "Builds.UseCase.QuickStart" ...
```

AppSettings
---
