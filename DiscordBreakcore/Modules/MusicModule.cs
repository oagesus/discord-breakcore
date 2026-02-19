using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Lavalink4NET;
using Lavalink4NET.Players;
using Lavalink4NET.Players.Queued;
using Lavalink4NET.Rest.Entities.Tracks;

namespace DiscordBreakcore.Modules;

[Group("break", "Breakcore Bot commands")]
public class MusicModule : InteractionModuleBase<SocketInteractionContext>
{
    private readonly IAudioService _audio;

    public MusicModule(IAudioService audio)
    {
        _audio = audio;
    }

    [SlashCommand("play", "Play a song from a URL or search query")]
    public async Task PlayAsync([Summary("query", "YouTube URL or search query")] string query)
    {
        var user = Context.User as SocketGuildUser;
        var voiceChannel = user?.VoiceChannel;

        if (voiceChannel == null)
        {
            await RespondAsync("You need to be in a voice channel!", ephemeral: true);
            return;
        }

        await DeferAsync();

        DiscordBreakcore.Services.TrackAnnouncerService.SetTextChannel(Context.Guild.Id, Context.Channel.Id);

        var playerOptions = new QueuedLavalinkPlayerOptions();

        var retrieveOptions = new PlayerRetrieveOptions(
            ChannelBehavior: PlayerChannelBehavior.Join);

        var result = await _audio.Players.RetrieveAsync(
            Context.Guild.Id,
            voiceChannel.Id,
            PlayerFactory.Queued,
            new OptionsWrapper<QueuedLavalinkPlayerOptions>(playerOptions),
            retrieveOptions);

        if (!result.IsSuccess)
        {
            await FollowupAsync("Failed to connect to voice channel.");
            return;
        }

        var player = result.Player;

        var guild = Context.Guild as SocketGuild;
        if (guild != null)
        {
            await guild.CurrentUser.ModifyAsync(x => x.Deaf = true);
        }

        var loadResult = await _audio.Tracks.LoadTracksAsync(query, TrackSearchMode.YouTube);

        if (loadResult.IsPlaylist)
        {
            var tracks = loadResult.Tracks.ToArray();

            if (tracks.Length == 0)
            {
                await FollowupAsync("Playlist is empty.");
                return;
            }

            var firstTrack = tracks[0];
            var position = await player.PlayAsync(firstTrack);

            var queued = 0;
            for (int i = 1; i < tracks.Length; i++)
            {
                await player.Queue.AddAsync(new TrackQueueItem(tracks[i]));
                queued++;
            }

            var playlistName = loadResult.Playlist?.Name ?? "Playlist";

            if (position == 0)
            {
                await FollowupAsync($"Queued **{queued}** more tracks from **{playlistName}**");
            }
            else
            {
                await FollowupAsync($"Queued **{tracks.Length}** tracks from **{playlistName}**");
            }
        }
        else
        {
            var track = loadResult.Track;

            if (track == null)
            {
                await FollowupAsync("Could not find any results.");
                return;
            }

            var position = await player.PlayAsync(track);

            if (position == 0)
            {
                await Context.Interaction.DeleteOriginalResponseAsync();
            }
            else
            {
                await FollowupAsync($"**{track.Title}** [{FormatDuration(track.Duration)}] added to queue (Position: {position})");
            }
        }
    }

    [SlashCommand("skip", "Skip one or more songs")]
    public async Task SkipAsync([Summary("count", "Number of songs to skip (default: 1)")] int count = 1)
    {
        var player = await GetPlayerAsync();
        if (player == null) return;

        if (player.CurrentTrack == null)
        {
            await RespondAsync("Nothing is playing right now.");
            return;
        }

        if (count < 1)
        {
            await RespondAsync("Count must be at least 1.", ephemeral: true);
            return;
        }

        var maxSkippable = player.Queue.Count + 1;
        if (count > maxSkippable)
            count = maxSkippable;

        var queueItems = player.Queue.ToList();

        await player.Queue.ClearAsync();

        for (int i = count - 1; i < queueItems.Count; i++)
        {
            await player.Queue.AddAsync(queueItems[i]);
        }

        await player.SkipAsync();
        var remaining = player.Queue.Count;

        await RespondAsync($"Skipped **{count}** {(count == 1 ? "track" : "tracks")}! **{remaining}** more {(remaining == 1 ? "track" : "tracks")} remaining");
    }

    [SlashCommand("queue", "Show the current queue")]
    public async Task QueueAsync()
    {
        var player = await GetPlayerAsync();
        if (player == null) return;

        var queue = player.Queue;

        if (queue.Count == 0 && player.CurrentTrack == null)
        {
            await RespondAsync("The queue is empty.");
            return;
        }

        var embed = new EmbedBuilder()
            .WithTitle($"Queue ({queue.Count} upcoming)")
            .WithColor(Color.Purple);

        if (player.CurrentTrack != null)
        {
            var pos = player.Position?.Position ?? TimeSpan.Zero;
            var dur = player.CurrentTrack.Duration;
            embed.AddField($"0. {player.CurrentTrack.Title}", $"Duration: {FormatDuration(pos)}/{FormatDuration(dur)}");
        }

        var items = queue.Take(10).ToList();
        for (int i = 0; i < items.Count; i++)
        {
            embed.AddField($"{i + 1}. {items[i].Track!.Title}", $"Duration: {FormatDuration(items[i].Track!.Duration)}");
        }

        if (queue.Count > 10)
            embed.WithFooter($"And {queue.Count - 10} more...");

        await RespondAsync(embed: embed.Build());
    }

    [SlashCommand("stop", "Stop playback and leave the channel")]
    public async Task StopAsync()
    {
        var player = await GetPlayerAsync();
        if (player == null) return;

        await player.DisconnectAsync();
        await RespondAsync("Stopped playback and left the channel.");
    }

    [SlashCommand("info", "Get bot information")]
    public async Task InfoAsync()
    {
        var embed = new EmbedBuilder()
            .WithTitle("Bot Info")
            .WithDescription("A Discord bot built with Discord.Net and .NET 10")
            .WithColor(Color.Blue)
            .AddField("Servers", Context.Guild.MemberCount.ToString(), true)
            .AddField("Latency", $"{Context.Client.Latency}ms", true)
            .Build();

        await RespondAsync(embed: embed);
    }

    private async Task<QueuedLavalinkPlayer?> GetPlayerAsync()
    {
        var result = await _audio.Players.RetrieveAsync(
            Context.Guild.Id,
            memberVoiceChannel: null,
            PlayerFactory.Queued,
            new OptionsWrapper<QueuedLavalinkPlayerOptions>(new QueuedLavalinkPlayerOptions()),
            new PlayerRetrieveOptions(ChannelBehavior: PlayerChannelBehavior.None));

        if (!result.IsSuccess)
        {
            await RespondAsync("Not connected to a voice channel.", ephemeral: true);
            return null;
        }

        return result.Player;
    }

    private static string FormatDuration(TimeSpan duration)
    {
        return duration.TotalHours >= 1
            ? duration.ToString(@"h\:mm\:ss")
            : duration.ToString(@"mm\:ss");
    }
}

internal sealed record class OptionsWrapper<T>(T Value) : Microsoft.Extensions.Options.IOptions<T> where T : class;