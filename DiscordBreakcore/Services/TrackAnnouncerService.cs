using Discord.WebSocket;
using Lavalink4NET;
using Lavalink4NET.Events.Players;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DiscordBreakcore.Services;

public class TrackAnnouncerService : IHostedService
{
    private readonly IAudioService _audio;
    private readonly DiscordSocketClient _client;
    private readonly ILogger<TrackAnnouncerService> _logger;
    private static readonly Dictionary<ulong, ulong> _textChannels = new();

    public static void SetTextChannel(ulong guildId, ulong textChannelId)
    {
        _textChannels[guildId] = textChannelId;
    }

    public TrackAnnouncerService(IAudioService audio, DiscordSocketClient client, ILogger<TrackAnnouncerService> logger)
    {
        _audio = audio;
        _client = client;
        _logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _audio.TrackStarted += OnTrackStarted;
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _audio.TrackStarted -= OnTrackStarted;
        return Task.CompletedTask;
    }

    private async Task OnTrackStarted(object sender, TrackStartedEventArgs args)
    {
        try
        {
            var guildId = args.Player.GuildId;

            if (!_textChannels.TryGetValue(guildId, out var channelId))
                return;

            var track = args.Track;
            var channel = _client.GetChannel(channelId) as ISocketMessageChannel;

            if (channel == null)
                return;

            var duration = track.Duration.TotalHours >= 1
                ? track.Duration.ToString(@"h\:mm\:ss")
                : track.Duration.ToString(@"mm\:ss");

            await channel.SendMessageAsync($"Now playing: **[{track.Title}]({track.Uri})** [{duration}]");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error announcing track");
        }
    }
}