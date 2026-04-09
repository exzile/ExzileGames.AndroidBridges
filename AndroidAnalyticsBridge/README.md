# ExzileGames.AndroidAnalyticsBridge

A pure C# wrapper for Firebase Analytics on .NET Android. Provides a clean `Dictionary<string, object>`-based event logging API over the Xamarin Firebase Analytics binding — no JNI bundle construction required in your app code.

## Requirements

Your Android project must include a valid `google-services.json` file (downloaded from the Firebase Console) in its root directory. Firebase Analytics will not initialize without it.

## Setup

### 1. Add project reference

```xml
<ProjectReference Include="..\AndroidAnalyticsBridge\AndroidAnalyticsBridge.csproj" />
```

### 2. Initialize in your Activity

```csharp
using AndroidAnalyticsBridge.Interop;

protected override void OnCreate(Bundle? savedInstanceState)
{
    base.OnCreate(savedInstanceState);

    AnalyticsBridgeManager.SetImplementation(new AndroidAnalyticsBridgeImpl(this));
}
```

### 3. Use from shared code

```csharp
using AndroidAnalyticsBridge.Interop;

// Log a simple event
AnalyticsBridgeManager.LogEvent("level_complete");

// Log an event with parameters
AnalyticsBridgeManager.LogEvent("purchase", new Dictionary<string, object>
{
    ["item_id"] = "redchest500",
    ["value"] = 4.99,
    ["currency"] = "USD"
});

// Set user ID (call with null to clear)
AnalyticsBridgeManager.SetUserId("user-12345");

// Set a custom user property
AnalyticsBridgeManager.SetUserProperty("player_tier", "gold");

// Disable collection (e.g. for GDPR opt-out)
AnalyticsBridgeManager.SetAnalyticsCollectionEnabled(false);

// Get the Firebase app instance ID
string? instanceId = await AnalyticsBridgeManager.GetAppInstanceIdAsync();
```

## Supported Parameter Types

| C# Type | Firebase Bundle Method |
|---------|----------------------|
| `string` | `PutString` |
| `int` | `PutInt` |
| `long` | `PutLong` |
| `float` | `PutFloat` |
| `double` | `PutDouble` |
| `bool` | `PutBoolean` |
| anything else | `PutString` via `ToString()` |

## License

MIT
