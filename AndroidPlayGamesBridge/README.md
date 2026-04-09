# AndroidPlayGamesBridge

A Java bridge + C# interop library that exposes Google Play Games Services v2 APIs missing from the `Xamarin.GooglePlayServices.Games.V2` NuGet binding for .NET Android.

## Problem

The `Xamarin.GooglePlayServices.Games.V2` NuGet package is missing several key PGS v2 APIs:
- **SnapshotsClient** (Saved Games / Cloud Save) — [dotnet/android-libraries#972](https://github.com/dotnet/android-libraries/issues/972)
- **PlayersClient** (Player info, display name, avatar) — [dotnet/android-libraries#975](https://github.com/dotnet/android-libraries/issues/975)
- **LeaderboardsClient**, **AchievementsClient**, **EventsClient**, **PlayerStatsClient**

These APIs exist in the Java AAR but the .NET binding generator excluded them. Creating linker descriptors does not help — the managed types were never generated.

## Solution

This library provides a Java bridge class compiled directly into your Android project. The Java class calls the native PGS v2 APIs, and C# code communicates with it via auto-generated JNI bindings. No custom binding library or AAR manipulation needed.

## APIs Exposed

| API | Methods | Status in NuGet |
|-----|---------|-----------------|
| **Sign-In** | `SignInAsync(bool silent)` | Partially available |
| **Players** | `GetCurrentPlayerAsync` (ID, display name, avatar URIs) | Missing |
| **Snapshots** | `LoadSnapshotAsync`, `SaveSnapshotAsync`, `DeleteSnapshotAsync` | Missing |
| **Leaderboards** | `SubmitScoreAsync`, `ShowLeaderboard`, `ShowAllLeaderboards` | Missing |
| **Achievements** | `UnlockAchievementAsync`, `IncrementAchievementAsync`, `RevealAchievementAsync`, `ShowAchievements` | Missing |
| **Events** | `IncrementEvent`, `LoadEventsAsync` | Missing |
| **Player Stats** | `GetPlayerStatsAsync` (session length, churn, spend percentiles) | Missing |

## Setup

### 1. Add NuGet package

```bash
dotnet add package ExzileGames.AndroidPlayGamesBridge
```

### 2. Initialize in your Activity

```csharp
using AndroidPlayGamesBridge.Interop;

protected override void OnCreate(Bundle? savedInstanceState)
{
    base.OnCreate(savedInstanceState);

    var bridge = new AndroidPlayGamesBridgeImpl(this);
    PlayGamesBridgeManager.SetImplementation(bridge);
}
```

### 3. Use from shared code

```csharp
using AndroidPlayGamesBridge.Interop;

// Sign in (silent first, fall back to interactive)
var signIn = await PlayGamesBridgeManager.SignInAsync(silent: true);
if (!signIn.Success)
    signIn = await PlayGamesBridgeManager.SignInAsync(silent: false);

// Get player info
var player = await PlayGamesBridgeManager.GetCurrentPlayerAsync();
if (player.Success)
    Console.WriteLine($"Hello {player.DisplayName} ({player.PlayerId})");

// Cloud save
await PlayGamesBridgeManager.SaveSnapshotAsync("save_slot_1", jsonData, "Auto-save");
var load = await PlayGamesBridgeManager.LoadSnapshotAsync("save_slot_1");
if (load.Success)
    Console.WriteLine(load.Data);

// Leaderboards
await PlayGamesBridgeManager.SubmitScoreAsync("leaderboard_id", 9001);
PlayGamesBridgeManager.ShowAllLeaderboards();

// Achievements
await PlayGamesBridgeManager.UnlockAchievementAsync("achievement_id");
PlayGamesBridgeManager.ShowAchievements();
```

## Architecture

```
Your Android Project
  └── references ExzileGames.AndroidPlayGamesBridge (NuGet)
        ├── Java/PlayGamesBridge.java          ← compiled by Android build; calls PGS v2 Java APIs
        ├── Interop/IPlayGamesBridge.cs        ← platform-agnostic interface
        ├── Interop/PlayGamesBridgeManager.cs  ← static access point
        └── Interop/AndroidPlayGamesBridgeImpl.cs  ← C# wrapper calling Java via JNI
```

## Requirements

- .NET 9+ / .NET 10+
- `Xamarin.GooglePlayServices.Games.V2` NuGet package
- Android API 23+
- Google Play Games Services enabled in Google Play Console

## License

MIT
