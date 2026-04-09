# AndroidRemoteConfigBridge

A C# wrapper for Firebase Remote Config on .NET Android. Fixes the Xamarin binding's `FetchAndActivateAsync()` returning `Task<Java.Lang.Object>` (a boxed bool) and provides a typed API for feature flags and live-ops configuration.

## Problem

The `Xamarin.Firebase.Config` binding's `FetchAndActivateAsync` returns `Task<Java.Lang.Object>` instead of `Task<bool>`, requiring an unsafe cast to read the activation result. Additionally, `SetDefaultsAsync` only exposes the `int` (XML resource ID) overload — the `Map`-based overload is absent from the binding entirely and must be called via JNI.

## Solution

This bridge wraps both issues:
- `FetchAndActivateAsync` is unwrapped into a typed `RemoteConfigFetchResult` record with `activated` and `success` fields.
- `SetDefaults` calls `setDefaultsAsync(Map)` via `Android.Runtime.JNIEnv` since the binding doesn't expose it.

## Setup

### 1. Add NuGet package

```bash
dotnet add package ExzileGames.AndroidRemoteConfigBridge
```

### 2. Add google-services.json

Place your `google-services.json` (from the Firebase Console) in the Android app project root and set its build action:

```xml
<GoogleServicesJson Include="google-services.json" />
```

### 3. Register the implementation in your Activity

```csharp
using AndroidRemoteConfigBridge.Interop;

protected override void OnCreate(Bundle? savedInstanceState)
{
    base.OnCreate(savedInstanceState);
    RemoteConfigBridgeManager.SetImplementation(new AndroidRemoteConfigBridgeImpl());
}
```

### 4. Use from shared code

```csharp
using AndroidRemoteConfigBridge.Interop;

// Set in-app defaults used before the first fetch completes
RemoteConfigBridgeManager.SetDefaults(new Dictionary<string, object>
{
    ["enable_new_feature"] = false,
    ["max_lives"]          = 5L,
    ["difficulty_scale"]   = 1.0,
    ["welcome_message"]    = "Hello!"
});

// Fetch and activate latest values from Firebase
var result = await RemoteConfigBridgeManager.FetchAndActivateAsync();

if (result.Success)
{
    bool featureEnabled = RemoteConfigBridgeManager.GetBool("enable_new_feature");
    long maxLives       = RemoteConfigBridgeManager.GetLong("max_lives");
    double scale        = RemoteConfigBridgeManager.GetDouble("difficulty_scale");
    string message      = RemoteConfigBridgeManager.GetString("welcome_message");
}

// Bypass the 12-hour cache during development
var devResult = await RemoteConfigBridgeManager.FetchAndActivateAsync(TimeSpan.Zero);
```

## API

| Method | Description |
|--------|-------------|
| `FetchAndActivateAsync(TimeSpan?)` | Fetches latest config and activates it. Returns whether new values were activated. Pass `TimeSpan.Zero` to bypass the 12-hour cache. |
| `SetDefaults(IDictionary<string, object>)` | Sets in-app defaults used before the first successful fetch. |
| `GetString(key, defaultValue)` | Returns a config value as `string`. |
| `GetBool(key, defaultValue)` | Returns a config value as `bool`. |
| `GetLong(key, defaultValue)` | Returns a config value as `long`. |
| `GetDouble(key, defaultValue)` | Returns a config value as `double`. |

## License

MIT
