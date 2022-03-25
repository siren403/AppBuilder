FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["AppBuilderExecutor/AppBuilderExecutor.csproj", "AppBuilderExecutor/"]
RUN dotnet restore "AppBuilderExecutor/AppBuilderExecutor.csproj"
COPY . .
WORKDIR "/src/AppBuilderExecutor"
RUN dotnet build "AppBuilderExecutor.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "AppBuilderExecutor.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AppBuilderExecutor.dll"]
