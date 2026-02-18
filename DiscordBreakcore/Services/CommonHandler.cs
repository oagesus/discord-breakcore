using System.Reflection;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace DiscordBreakcore.Services;

public class CommandHandler
{
    private readonly DiscordSocketClient _client;
    private readonly InteractionService _interactions;
    private readonly IServiceProvider _services;
    private readonly IConfiguration _configuration;
    private readonly ILogger<CommandHandler> _logger;

    public CommandHandler(
        DiscordSocketClient client,
        IServiceProvider services,
        IConfiguration configuration,
        ILogger<CommandHandler> logger)
    {
        _client = client;
        _interactions = new InteractionService(client);
        _services = services;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task InitializeAsync()
    {
        _client.Ready += RegisterCommandsAsync;
        _client.InteractionCreated += HandleInteractionAsync;

        await _interactions.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
    }

    private async Task RegisterCommandsAsync()
    {
        var guildIdStr = _configuration["GUILD_ID"]
            ?? Environment.GetEnvironmentVariable("GUILD_ID");

        if (ulong.TryParse(guildIdStr, out var guildId))
        {
            await _interactions.RegisterCommandsToGuildAsync(guildId);
            _logger.LogInformation("Slash commands registered to guild {GuildId}", guildId);
        }
        else
        {
            await _interactions.RegisterCommandsGloballyAsync();
            _logger.LogInformation("Slash commands registered globally");
        }
    }

    private async Task HandleInteractionAsync(SocketInteraction interaction)
    {
        var context = new SocketInteractionContext(_client, interaction);
        await _interactions.ExecuteCommandAsync(context, _services);
    }
}