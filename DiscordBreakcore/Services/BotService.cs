using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DiscordBreakcore.Services;

public class BotService : IHostedService
{
    private readonly DiscordSocketClient _client;
    private readonly CommandHandler _commandHandler;
    private readonly IConfiguration _configuration;
    private readonly ILogger<BotService> _logger;

    public BotService(
        DiscordSocketClient client,
        CommandHandler commandHandler,
        IConfiguration configuration,
        ILogger<BotService> logger)
    {
        _client = client;
        _commandHandler = commandHandler;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _client.Log += LogAsync;
        _client.Ready += ReadyAsync;

        var token = _configuration["DISCORD_TOKEN"]
            ?? Environment.GetEnvironmentVariable("DISCORD_TOKEN")
            ?? throw new InvalidOperationException("DISCORD_TOKEN is not configured.");

        await _client.LoginAsync(TokenType.Bot, token);
        await _client.StartAsync();
        await _commandHandler.InitializeAsync();

        _logger.LogInformation("Bot started successfully");
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Bot is shutting down...");
        await _client.StopAsync();
        await _client.DisposeAsync();
    }

    private Task LogAsync(LogMessage log)
    {
        _logger.LogInformation("[Discord] {Message}", log.ToString());
        return Task.CompletedTask;
    }

    private Task ReadyAsync()
    {
        _logger.LogInformation("Bot is connected as {User}", _client.CurrentUser?.Username);
        return Task.CompletedTask;
    }
}