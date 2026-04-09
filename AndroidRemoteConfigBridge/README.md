# AndroidRemoteConfigBridge

A C# wrapper for Firebase Remote Config on .NET Android. Fixes the Xamarin binding's `FetchAndActivateAsync()` returning `Task<Java.Lang.Object>` (a boxed bool) and provides a clean typed API for feature flags and live-ops configuration.

## Requirements

- A `google-services.json` file placed in your Android project root (set **Build Action** to `GoogleServicesJson`).
- Firebase initialised in your app before calling any bridge methods.

## Setup

### 1. Add project reference

```xml
<ProjectReference Include="..\AndroidRemoteConfigBridge\AndroidRemoteConfigBridge.csproj" />
```

### 2. Register the implementation in your Activity

```csharp
using AndroidRemoteConfigBridge.Interop;

protected override void OnCreate(Bundle? savedInstanceState)
{
    base.OnCreate(savedInstanceState);

    RemoteConfigBridgeManager.SetImplementation(new AndroidRemoteConfigBridgeImpl());
}
```

### 3. Use from shared code

```csharp
using AndroidRemoteConfigBridge.Interop;

// Optional: set in-app defaults used before a fetch completes
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

// Force an immediate fetch during development (bypasses the 12-hour cache)
var devResult = await RemoteConfigBridgeManager.FetchAndActivateAsync(TimeSpan.Zero);
```

## API

| Method | Description |
|--------|-------------|
| `FetchAndActivateAsync(TimeSpan?)` | Fetches latest config and activates it. Pass `TimeSpan.Zero` to bypass the cache. |
| `SetDefaults(IDictionary<string, object>)` | Sets in-app defaults used before the first fetch. |
| `GetString(key, defaultValue)` | Returns a config value as `string`. |
| `GetBool(key, defaultValue)` | Returns a config value as `bool`. |
| `GetLong(key, defaultValue)` | Returns a config value as `long`. |
| `GetDouble(key, defaultValue)` | Returns a config value as `double`. |

## License

MIT
