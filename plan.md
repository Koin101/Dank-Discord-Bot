# DSharpPlus v4.4.6 → v5.x Migration Plan

## Context

The bot currently uses DSharpPlus v4.4.6 with the now-removed `DSharpPlus.SlashCommands` extension for slash commands and `DSharpPlus.CommandsNext` for legacy prefix commands. DSharpPlus v5 is a major breaking release that consolidates all command handling into a unified `DSharpPlus.Commands` extension and introduces a new client setup API. This migration removes deprecated extensions, adopts the new `IServiceCollection`-based setup (which fits the existing hosted service pattern), and rewrites command modules.

> **Status: Parked** — waiting for DSharpPlus v5 stable release and Lavalink4NET v5 support before proceeding.

---

## Critical Pre-Migration Blockers

### 1. DSharpPlus v5 is still pre-release (nightly only)
- Latest available: `5.0.0-nightly-02581` (2026-04-04)
- No stable v5.0.0 release exists yet
- API may still change before release

### 2. Lavalink4NET.DSharpPlus incompatibility — BLOCKER
- `Lavalink4NET.DSharpPlus` v4.2.1 (latest) requires `DSharpPlus >= 4.5.1` — it does NOT support v5
- No Lavalink4NET version currently supports DSharpPlus v5
- Check https://github.com/uwu/Lavalink4NET for a v5-compatible branch/pre-release before resuming

---

## Chosen Approach: IServiceCollection

The existing project already uses `IHostBuilder` with `builder.Services.*` for all DI registration. The `IServiceCollection` approach maps directly to this pattern — no restructuring needed.

```csharp
// Old
builder.Services.AddSingleton<DiscordClient>(...);
// New
builder.Services.AddDiscordClient(token, DiscordIntents.AllUnprivileged);
```

---

## Files to Modify

- `Discord Bot.csproj`
- `Program.cs`
- `Bot/Bot.cs`
- `Commands/CivRolls.cs`
- `Commands/Music_Lavalink4.0.cs`
- `Commands/Reddit.cs`
- `Commands/Admin.cs`
- `Commands/Misc.cs` (prefix commands — decide to migrate or drop)
- `Commands/LeagueModule.cs` (prefix commands — already broken/commented)

---

## Step-by-Step Migration

### Step 1 — Update Packages (`Discord Bot.csproj`)

**Remove:**
```xml
<PackageReference Include="DSharpPlus.SlashCommands" Version="4.4.6" />
<PackageReference Include="DSharpPlus.CommandsNext" Version="4.4.6" />
<PackageReference Include="DSharpPlus.Lavalink" Version="4.4.6" />
<PackageReference Include="DSharpPlus.Interactivity" Version="4.4.6" />
```

**Update:**
```xml
<PackageReference Include="DSharpPlus" Version="5.0.0-nightly-XXXXX" />
```
> Pin to a specific nightly build number for reproducibility. Add `<AllowedPackageReleaseTypes>prerelease</AllowedPackageReleaseTypes>` or use a `nuget.config` with pre-release allowed.

**Keep (pending Lavalink4NET v5 support):**
```xml
<PackageReference Include="Lavalink4NET.DSharpPlus" Version="4.2.1" />
<PackageReference Include="Lavalink4NET.InactivityTracking" Version="4.2.1" />
<PackageReference Include="Lavalink4NET.Lyrics" Version="4.2.1" />
```
> Also update Lavalink4NET from 4.0.13 to 4.2.1 (latest stable, still v4 DSharpPlus compatible).

---

### Step 2 — Update `Program.cs`

**Remove:**
- `using DSharpPlus.Lavalink;` and `using DSharpPlus.Net;`
- `builder.Services.AddSingleton<DiscordClient>(...)` and `DiscordConfiguration` factory

**Add:**
```csharp
builder.Services.AddDiscordClient(
    Environment.GetEnvironmentVariable("DiscordToken")!,
    DiscordIntents.AllUnprivileged
);
```

> Custom logging setup is required with IServiceCollection in v5 — DSharpPlus default logging won't auto-register. Ensure `builder.Services.AddLogging(...)` is explicit.

Lavalink registration (`AddLavalink()`, `AddLyrics()`, `AddInactivityTracking()`) stays unchanged.

Move command registration here (see Step 3):
```csharp
ulong guildId = ulong.Parse(Environment.GetEnvironmentVariable("GuildID")!);

builder.Services.AddCommandsExtension(
    extension =>
    {
        extension.AddCommands([
            typeof(CivRolls),
            typeof(MusicLavalink40),
            typeof(Reddit),
            typeof(Admin)
        ], guildId);
    },
    new CommandsConfiguration { RegisterDefaultCommandProcessors = false }
);
```

Move event handler registration here:
```csharp
builder.Services.ConfigureEventHandlers(b => b
    .HandleComponentInteractionCreated(ClientOnComponentInteractionCreated)
    .HandleMessageCreated(OnMessageCreated)
    .HandleMessageReactionRemovedEmoji(OnReactionRemovedEmoji)
    .HandleMessageReactionsCleared(OnReactionsClearedEmoji)
);
```

---

### Step 3 — Update `Bot/Bot.cs`

- Remove `RegisterSlashCommands()` entirely (`discord.UseSlashCommands()` no longer exists)
- Remove `discord.UseCommandsNext(...)` registration
- Remove direct event subscriptions (`discord.ComponentInteractionCreated += ...`, etc.) — replaced by `ConfigureEventHandlers` in Program.cs
- Keep `await discord.ConnectAsync()` and `await audioService.WaitForReadyAsync(stoppingToken)` — unchanged

---

### Step 4 — Migrate Command Modules

**Pattern changes across all slash command modules:**

| v4 | v5 |
|---|---|
| `extends ApplicationCommandModule` | No base class needed |
| `using DSharpPlus.SlashCommands;` | `using DSharpPlus.Commands;` |
| `[SlashCommand("name", "desc")]` | `[Command("name")]` + `[Description("desc")]` on method |
| `[Option("name", "desc")] string param` | `string param` + `[Description("desc")]` on param |
| `InteractionContext ctx` | `SlashCommandContext ctx` |
| `ctx.DeferAsync()` | `ctx.DeferResponseAsync()` |
| `ctx.DeferAsync(true)` | `ctx.DeferResponseAsync(ephemeral: true)` |
| `ctx.EditResponseAsync(new DiscordWebhookBuilder()...)` | `ctx.EditResponseAsync(new DiscordMessageBuilder()...)` |
| `ctx.CreateResponseAsync("text", ephemeral: true)` | `ctx.RespondAsync("text")` (with ephemeral via builder) |

**`Admin.cs` specific:**
- Remove `using DSharpPlus.SlashCommands.Attributes;`
- `[SlashRequireOwner]` → `[RequireApplicationOwner]` (verify exact v5 attribute name)

**`Music_Lavalink4.0.cs` specific:**
- Same pattern changes as above
- `ctx.Guild.Id` and `ctx.Member.VoiceState?.Channel.Id` — verify still available on `SlashCommandContext`
- Lavalink4NET API calls (`audioService.Players.*`, `audioService.Tracks.*`) are unchanged

**`Misc.cs` / prefix commands:**
- `CommandsNext` is removed — either convert `!!creategif` to a slash command or drop it

---

### Step 5 — Update `Bot/EmbedDisplayPlayer.cs`

- Verify `NotFoundException` is still in `DSharpPlus.Exceptions` namespace in v5
- `DiscordEmbedBuilder`, `DiscordMessageBuilder`, `DiscordButtonComponent`, `DiscordLinkButtonComponent`, `ButtonStyle` — expected to remain under `DSharpPlus.Entities`

---

### Step 6 — Update `Scripts/Pickwick.cs`

- `discord.GetChannelAsync(id)` and `channel.SendMessageAsync(string)` — verify unchanged in v5 (likely fine)

---

### Step 7 — Clean Up Obsolete Files

- `Commands/Trolling.cs` — empty, delete
- `Bot/HelpFormatter.cs` — CommandsNext help formatter, delete
- `Bot/Lavalink4NetClasses.cs` — example/reference code, delete
- `Deprecated/` — old Lavalink v3 code, delete if desired

---

## Verification

Manual testing via Discord (no automated tests):

1. `dotnet build` — no errors
2. `dotnet run` — bot connects and appears online
3. `/rollcivs` — CivRolls basic test
4. `/redditPost subreddit:aww` — Reddit command
5. `/play query:test` — Music (join a voice channel first)
6. `/timeout` — Admin command (requires owner)
7. Music player buttons (play/pause, skip, repeat, shuffle)
8. "Resend Link" button on Reddit posts
9. Funny auto-replies via chat message
