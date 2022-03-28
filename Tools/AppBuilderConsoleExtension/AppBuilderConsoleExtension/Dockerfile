FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["AppBuilderConsoleExtension/AppBuilderConsoleExtension.csproj", "AppBuilderConsoleExtension/"]
RUN dotnet restore "AppBuilderConsoleExtension/AppBuilderConsoleExtension.csproj"
COPY . .
WORKDIR "/src/AppBuilderConsoleExtension"
RUN dotnet build "AppBuilderConsoleExtension.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "AppBuilderConsoleExtension.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AppBuilderConsoleExtension.dll"]
