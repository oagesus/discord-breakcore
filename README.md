# discord-breakcore

A Discord music bot built with C# and .NET 10, powered by [Lavalink](https://github.com/lavalink-devs/Lavalink). You host and run it yourself on your own machine or server. Supports playback from YouTube, SoundCloud, and Bandcamp.

## Features

- Play music from YouTube, SoundCloud, and Bandcamp URLs, or YouTube search query
- Queue management with support for playlists
- Skip one or multiple tracks
- Now-playing announcements with clickable links and track duration
- Slash command interface (`/break`)
- Runs via Docker — no need to install .NET, Java, or any other dependencies

## Tech Stack

| Component | Technology |
|-----------|-----------|
| Language | C# / .NET 10 |
| Discord library | [Discord.Net](https://github.com/discord-net/Discord.Net) 3.18.0 |
| Audio server | [Lavalink](https://github.com/lavalink-devs/Lavalink) 4.2.0 |
| Audio client | [Lavalink4NET](https://github.com/angelobreuer/Lavalink4NET) 4.2.0 |
| YouTube plugin | [lavalink-youtube-plugin](https://github.com/lavalink-devs/youtube-source) 1.18.0 |
| Hosting | Docker |
| CI/CD | GitHub Actions + Docker Hub |

## Setup

### 1. Install Docker

Download and install [Docker](https://docs.docker.com/get-docker/).

### 2. Download the required files

Download [compose.yaml](compose.yaml) and [lavalink/application.yml](lavalink/application.yml), create an empty `.env` file, and place them in the same folder structure:

```
your-folder/
├── compose.yaml
├── .env
└── lavalink/
    └── application.yml
```

### 3. Create a Discord bot

1. Go to the [Discord Developer Portal](https://discord.com/developers/applications) and create a new application. The name you choose will be the bot's name in Discord.
2. Open the **Bot** tab, enable the **Message Content Intent** under Privileged Gateway Intents, and copy the token.
3. Go to **OAuth2 → URL Generator**, select the `bot` and `applications.commands` scopes, and grant the following permissions:
   - View Channels
   - Send Messages
   - Connect (voice)
   - Speak (voice)
   - Use Application Commands
4. Use the generated URL to invite the bot to your server.

### 4. Fill in the .env file

Add your Discord token to the `.env` file. Leave the OAuth token empty for now:

```env
DISCORD_TOKEN=your_discord_bot_token_here
LAVALINK_OAUTH_REFRESH_TOKEN=
# GUILD_ID=your_guild_id_here   # Optional: uncomment for guild-scoped commands (faster registration during development)
```

> **Note:** Never share or commit your `.env` file.

### 5. Get a YouTube OAuth refresh token

Required by the YouTube plugin for reliable playback.

1. Open a terminal (Command Prompt, PowerShell, or any terminal) and navigate to your folder which you created earlier with `cd path/to/your-folder`.
2. Run `docker compose up lavalink` to start Lavalink.
3. In a new terminal window (same folder), run `docker compose logs lavalink` and look for a Google authorization URL in the output.
4. Open the URL in your browser and sign in with a Google account.
5. Lavalink will print the refresh token to the logs — copy it into `LAVALINK_OAUTH_REFRESH_TOKEN` in your `.env`.
6. Stop Lavalink by pressing `Ctrl+C` in the terminal, or run `docker compose down` in a new terminal window.

### 6. Start the bot

```bash
docker compose up -d
```

This starts two containers:
- **lavalink** — the audio server (health-checked before the bot starts)
- **discord-bot** — the bot itself

To view logs:

```bash
docker compose logs -f
```

To stop:

```bash
docker compose down
```

## Commands

All commands are grouped under `/break`:

| Command | Description |
|---------|-------------|
| `/break play <query>` | Play a song by URL (YouTube, SoundCloud, Bandcamp) or YouTube search query. Adds to queue if something is already playing. |
| `/break skip [count]` | Skip the current track, or skip multiple tracks at once. |
| `/break queue` | Show the current queue (up to 10 upcoming tracks). |
| `/break stop` | Stop playback and disconnect from the voice channel. |
| `/break info` | Display bot information. |

## Development

### Run without Docker

You need a running Lavalink instance. Set the environment variables in your shell (or a local `.env`), then:

```bash
cd DiscordBreakcore
dotnet run
```

### Build Docker image locally

```bash
docker build -t discord-breakcore .
```

### Lavalink configuration

The Lavalink server configuration lives in [lavalink/application.yml](lavalink/application.yml). Enabled audio sources:

- YouTube (via plugin, with OAuth support)
- SoundCloud
- Bandcamp

## License

[MIT](LICENSE)
