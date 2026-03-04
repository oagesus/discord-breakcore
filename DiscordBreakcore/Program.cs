using Discord;
using Discord.WebSocket;
using DiscordBreakcore.Services;
using Lavalink4NET.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((_, services) =>
    {
        services.AddSingleton(new DiscordSocketConfig
        {
            GatewayIntents = GatewayIntents.Guilds
                           | GatewayIntents.GuildMessages
                           | GatewayIntents.GuildVoiceStates
                           | GatewayIntents.MessageContent,
            LogLevel = LogSeverity.Info
        });

        services.AddSingleton<DiscordSocketClient>();
        services.AddSingleton<CommandHandler>();
        services.AddHostedService<BotService>();
        services.AddHostedService<TrackAnnouncerService>();

        services.AddLavalink();
        services.ConfigureLavalink(config =>
        {
            config.BaseAddress = new Uri("http://lavalink:2333");
            config.Passphrase = "breakcore-secret";
        });
    })
    .Build();

await host.RunAsync();