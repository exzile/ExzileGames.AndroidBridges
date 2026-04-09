# ExzileGames.AndroidBridges

Monorepo for .NET Android Java bridge libraries that fix missing or broken APIs in Xamarin/MAUI NuGet bindings.

Each package compiles Java source directly into your Android app and exposes native APIs to C# via auto-generated JNI bindings. No custom binding libraries, no AAR manipulation, no reflection hacks.

## Packages

| Package | NuGet | Description |
|---------|-------|-------------|
| [AndroidPlayGamesBridge](AndroidPlayGamesBridge/) | `ExzileGames.AndroidPlayGamesBridge` | Exposes missing PGS v2 APIs: SnapshotsClient, PlayersClient, LeaderboardsClient, AchievementsClient |
| [AndroidBillingBridge](AndroidBillingBridge/) | `ExzileGames.AndroidBillingBridge` | Clean async Billing Library v8 API: products, purchases, consume, subscriptions, history |
| [AndroidAdsBridge](AndroidAdsBridge/) | `ExzileGames.AndroidAdsBridge` | AdMob bridge: rewarded, interstitial, rewarded interstitial, app open, and banner ads |
| [AndroidReviewBridge](AndroidReviewBridge/) | `ExzileGames.AndroidReviewBridge` | Google Play In-App Review API: trigger the review prompt async |
| [AndroidConsentBridge](AndroidConsentBridge/) | `ExzileGames.AndroidConsentBridge` | UMP v3 consent collection for GDPR/CCPA — call before showing any ads |
| [AndroidFcmBridge](AndroidFcmBridge/) | `ExzileGames.AndroidFcmBridge` | Firebase Cloud Messaging: token retrieval, topic subscriptions, push message callbacks |
| [AndroidCrashlyticsBridge](AndroidCrashlyticsBridge/) | `ExzileGames.AndroidCrashlyticsBridge` | Firebase Crashlytics with proper C# exception type grouping |
| [AndroidAppUpdateBridge](AndroidAppUpdateBridge/) | `ExzileGames.AndroidAppUpdateBridge` | Google Play In-App Updates: immediate and flexible update flows |
| [AndroidAnalyticsBridge](AndroidAnalyticsBridge/) | `ExzileGames.AndroidAnalyticsBridge` | Firebase Analytics with `Dictionary<string, object>` event API |
| [AndroidRemoteConfigBridge](AndroidRemoteConfigBridge/) | `ExzileGames.AndroidRemoteConfigBridge` | Firebase Remote Config with typed value access and clean async fetch |

## Why

Official Xamarin/MAUI NuGet bindings frequently have missing types, broken overloads, or JNI lifetime bugs that force reflection workarounds:

- `Xamarin.GooglePlayServices.Games.V2` is missing `SnapshotsClient`, `PlayersClient`, and several other clients ([#972](https://github.com/dotnet/android-libraries/issues/972), [#975](https://github.com/dotnet/android-libraries/issues/975))
- `Xamarin.Android.Google.BillingClient` has JNI lifetime bugs and missing overloads
- `FirebaseMessagingService` must be subclassed in Java — it cannot be implemented in C# alone
- `Xamarin.Firebase.Config` only exposes `SetDefaultsAsync(int)`, missing the `Map`-based overload

These bridges call the Java APIs directly from compiled source, eliminating all workarounds.

## Quick Start

```bash
dotnet add package ExzileGames.AndroidPlayGamesBridge
dotnet add package ExzileGames.AndroidBillingBridge
dotnet add package ExzileGames.AndroidAdsBridge
dotnet add package ExzileGames.AndroidReviewBridge
dotnet add package ExzileGames.AndroidConsentBridge
dotnet add package ExzileGames.AndroidFcmBridge
dotnet add package ExzileGames.AndroidCrashlyticsBridge
dotnet add package ExzileGames.AndroidAppUpdateBridge
dotnet add package ExzileGames.AndroidAnalyticsBridge
dotnet add package ExzileGames.AndroidRemoteConfigBridge
```

See each package's README for setup and usage.

## Architecture

All bridges follow the same pattern:

```
Your Android Project
  └── references ExzileGames.XxxBridge (NuGet)
        ├── Java/XxxBridge.java          ← compiled by Android build; calls native Java APIs
        ├── Interop/IXxxBridge.cs        ← platform-agnostic interface (works on all targets)
        ├── Interop/XxxBridgeManager.cs  ← static singleton access point
        └── Interop/AndroidXxxBridgeImpl.cs  ← C# implementation calling Java via JNI
```

- **Interfaces** return typed result records on all platforms (error results on non-Android).
- **Managers** use `SetImplementation(IXxxBridge)` — register the Android impl in your `Activity.OnCreate`.
- **Java sources** are included in the NuGet via `buildTransitive/` so consuming projects get them automatically.

## Building

```bash
dotnet build ExzileGames.Bridges.slnx
```

## Packing

```bash
dotnet pack ExzileGames.Bridges.slnx --configuration Release --output ./artifacts
```

Packages are published to NuGet.org automatically by GitHub Actions on every push to `main`.

## Adding a New Bridge

1. Create a folder: `AndroidMyNewBridge/`
2. Add Java source in `AndroidMyNewBridge/Java/com/exzilegames/mynewbridge/`
3. Add C# interop in `AndroidMyNewBridge/Interop/`
4. Add `AndroidMyNewBridge.csproj` following the existing pattern
5. Add to `ExzileGames.Bridges.slnx`
6. Add a `README.md` and `LICENSE`

## License

MIT
