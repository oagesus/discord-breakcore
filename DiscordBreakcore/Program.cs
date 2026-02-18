using Discord;
using Discord.WebSocket;
using DiscordBreakcore.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        services.AddSingleton(new DiscordSocketConfig
        {
            GatewayIntents = GatewayIntents.Guilds
                           | GatewayIntents.GuildMessages
                           | GatewayIntents.MessageContent,
            LogLevel = LogSeverity.Info
        });

        services.AddSingleton<DiscordSocketClient>();
        services.AddSingleton<CommandHandler>();
        services.AddHostedService<BotService>();
    })
    .Build();

await host.RunAsync();