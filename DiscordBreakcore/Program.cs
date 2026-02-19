using Discord;
using Discord.WebSocket;
using DiscordBreakcore.Services;
using Lavalink4NET.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
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
            var lavalinkHost = context.Configuration["LAVALINK_HOST"] ?? "lavalink";
            var lavalinkPort = context.Configuration["LAVALINK_PORT"] ?? "2333";
            var lavalinkPassword = context.Configuration["LAVALINK_PASSWORD"] ?? "breakcore-secret";

            config.BaseAddress = new Uri($"http://{lavalinkHost}:{lavalinkPort}");
            config.Passphrase = lavalinkPassword;
        });
    })
    .Build();

await host.RunAsync();