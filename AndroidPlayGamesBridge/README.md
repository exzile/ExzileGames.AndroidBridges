# AndroidPlayGamesBridge

A Java bridge + C# interop library that exposes Google Play Games Services v2 APIs missing from the `Xamarin.GooglePlayServices.Games.V2` NuGet bindings for .NET Android.

## Problem

The `Xamarin.GooglePlayServices.Games.V2` NuGet package (all versions) is missing several key PGS v2 APIs:
- **SnapshotsClient** (Saved Games / Cloud Save) — [dotnet/android-libraries#972](https://github.com/dotnet/android-libraries/issues/972)
- **PlayersClient** (Player info, display name, avatar) — [dotnet/android-libraries#975](https://github.com/dotnet/android-libraries/issues/975)
- **LeaderboardsClient**, **AchievementsClient**, **EventsClient**, **PlayerStatsClient**

These APIs exist in the Java AAR but the .NET binding generator excluded them. Creating linker descriptors does not help — the managed types were never generated.

## Solution

This library provides a **Java bridge class** compiled directly into your Android project. The Java class calls the native PGS v2 APIs, and C# code communicates with it via auto-generated JNI bindings. No custom binding library or AAR manipulation needed.

## APIs Exposed

| API | Methods | Status in NuGet |
|-----|---------|-----------------|
| **Sign-In** | `SignInAsync`, `IsAuthenticated` | Partially available |
| **Players** | `GetCurrentPlayerAsync` (ID, display name, avatar URIs) | Missing |
| **Snapshots** | `LoadSnapshotAsync`, `SaveSnapshotAsync`, `DeleteSnapshotAsync` | Missing |
| **Leaderboards** | `SubmitScoreAsync`, `ShowLeaderboard`, `ShowAllLeaderboards` | Missing |
| **Achievements** | `UnlockAsync`, `IncrementAsync`, `RevealAsync`, `ShowAchievements` | Missing |
| **Events** | `IncrementEvent`, `LoadEventsAsync` | Missing |
| **Player Stats** | `GetPlayerStatsAsync` (session length, churn, spend) | Missing |

## Setup

### 1. Add project reference

```xml
<ProjectReference Include="..\AndroidPlayGamesBridge\AndroidPlayGamesBridge.csproj" />
```

### 2. Ensure Play Games NuGet is referenced

The bridge project already includes `Xamarin.GooglePlayServices.Games.V2`. Your Android project needs the same package for Play Games initialization.

### 3. Initialize in your Activity

```csharp
using AndroidPlayGamesBridge.Interop;

protected override void OnCreate(Bundle? savedInstanceState)
{
    base.OnCreate(savedInstanceState);

    var bridge = new AndroidPlayGamesBridgeImpl(this);
    PlayGamesBridgeManager.SetImplementation(bridge);
}
```

### 4. Use from shared code

```csharp
using AndroidPlayGamesBridge.Interop;

// Sign in
var signIn = await PlayGamesBridgeManager.SignInAsync(silent: true);

// Get player info (no more auth code exchange workaround!)
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
Your Android Project (.csproj)
  └── references AndroidPlayGamesBridge project
        ├── Java/PlayGamesBridge.java     ← compiled by Android build, calls PGS v2 Java APIs
        ├── Interop/IPlayGamesBridge.cs   ← shared interface (works on all platforms)
        ├── Interop/PlayGamesBridgeManager.cs  ← static access point
        └── Interop/AndroidPlayGamesBridgeImpl.cs  ← C# wrapper calling Java via JNI
```

The Java source is compiled directly by the Android build system. The .NET Android binding generator automatically creates C# types for the Java class and its listener interfaces. No manual AAR binding or metadata transforms needed.

## Requirements

- .NET 9+ / .NET 10+
- `Xamarin.GooglePlayServices.Games.V2` NuGet package
- Android API 23+
- Google Play Games Services enabled in Google Play Console

## License

MIT
