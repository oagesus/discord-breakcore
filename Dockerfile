FROM mcr.microsoft.com/dotnet/sdk:10.0-preview AS build
WORKDIR /src
COPY DiscordBreakcore/DiscordBreakcore.csproj DiscordBreakcore/
RUN dotnet restore DiscordBreakcore/DiscordBreakcore.csproj
COPY . .
RUN dotnet publish DiscordBreakcore/DiscordBreakcore.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/runtime:10.0-preview
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "DiscordBreakcore.dll"]