# ExzileGames.AndroidBridges

Monorepo for .NET Android Java bridge libraries that fix missing or broken APIs in Xamarin/MAUI NuGet bindings.

Each project compiles Java source directly into your Android app and exposes the native APIs to C# via auto-generated JNI bindings. No custom binding libraries, no AAR manipulation, no reflection hacks.

## Packages

| Package | NuGet | Description |
|---------|-------|-------------|
| [AndroidPlayGamesBridge](AndroidPlayGamesBridge/) | `ExzileGames.AndroidPlayGamesBridge` | Exposes missing PGS v2 APIs: SnapshotsClient, PlayersClient, RecallClient |
| [AndroidBillingBridge](AndroidBillingBridge/) | `ExzileGames.AndroidBillingBridge` | Clean async billing API: products, purchases, consume, subscriptions |
| [AndroidAdsBridge](AndroidAdsBridge/) | `ExzileGames.AndroidAdsBridge` | AdMob rewarded ads bridge: load, show, and await reward results |
| [AndroidReviewBridge](AndroidReviewBridge/) | `ExzileGames.AndroidReviewBridge` | Google Play In-App Review API bridge: trigger the review flow async |

## Why

The `Xamarin.GooglePlayServices.Games.V2` NuGet is missing `SnapshotsClient`, `PlayersClient`, and `RecallClient` ([#972](https://github.com/dotnet/android-libraries/issues/972), [#975](https://github.com/dotnet/android-libraries/issues/975)). The `Xamarin.Android.Google.BillingClient` has JNI lifetime bugs and awkward overloads that force reflection workarounds.

These bridges solve both by calling the Java APIs directly from compiled source.

## Quick Start

```bash
dotnet add package ExzileGames.AndroidPlayGamesBridge
dotnet add package ExzileGames.AndroidBillingBridge
dotnet add package ExzileGames.AndroidAdsBridge
dotnet add package ExzileGames.AndroidReviewBridge
```

See each package's README for setup and usage.

## Building

```bash
dotnet build ExzileGames.Bridges.slnx
```

## Packing NuGets

```bash
dotnet pack AndroidPlayGamesBridge/AndroidPlayGamesBridge.csproj --configuration Release --output ./nupkg
dotnet pack AndroidBillingBridge/AndroidBillingBridge.csproj --configuration Release --output ./nupkg
dotnet pack AndroidAdsBridge/AndroidAdsBridge.csproj --configuration Release --output ./nupkg
dotnet pack AndroidReviewBridge/AndroidReviewBridge.csproj --configuration Release --output ./nupkg
```

## Publishing

```bash
dotnet nuget push ./nupkg/ExzileGames.AndroidPlayGamesBridge.1.0.0.nupkg --api-key YOUR_KEY --source https://api.nuget.org/v3/index.json
dotnet nuget push ./nupkg/ExzileGames.AndroidBillingBridge.1.0.0.nupkg --api-key YOUR_KEY --source https://api.nuget.org/v3/index.json
dotnet nuget push ./nupkg/ExzileGames.AndroidAdsBridge.1.0.0.nupkg --api-key YOUR_KEY --source https://api.nuget.org/v3/index.json
dotnet nuget push ./nupkg/ExzileGames.AndroidReviewBridge.1.0.0.nupkg --api-key YOUR_KEY --source https://api.nuget.org/v3/index.json
```

## Adding a New Bridge

1. Create a folder: `AndroidMyNewBridge/`
2. Add Java source in `AndroidMyNewBridge/Java/com/exzilegames/mynewbridge/`
3. Add C# interop in `AndroidMyNewBridge/Interop/`
4. Add `AndroidMyNewBridge.csproj` following the existing pattern
5. Add to `ExzileGames.Bridges.slnx`
6. Add README.md and LICENSE

## License

MIT
