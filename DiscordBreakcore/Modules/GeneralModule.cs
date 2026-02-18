using Discord.Interactions;

namespace DiscordBreakcore.Modules;

public class GeneralModule : InteractionModuleBase<SocketInteractionContext>
{
    [SlashCommand("ping", "Check if the bot is alive")]
    public async Task PingAsync()
    {
        await RespondAsync($"Pong! Latency: {Context.Client.Latency}ms");
    }

    [SlashCommand("info", "Get bot information")]
    public async Task InfoAsync()
    {
        var embed = new Discord.EmbedBuilder()
            .WithTitle("Bot Info")
            .WithDescription("A Discord bot built with Discord.Net and .NET 10")
            .WithColor(Discord.Color.Blue)
            .AddField("Servers", Context.Client.Guilds.Count.ToString(), true)
            .AddField("Latency", $"{Context.Client.Latency}ms", true)
            .Build();

        await RespondAsync(embed: embed);
    }
}