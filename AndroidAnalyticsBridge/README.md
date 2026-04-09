# AndroidAnalyticsBridge

A pure C# wrapper for Firebase Analytics on .NET Android. Provides a clean `Dictionary<string, object>`-based event logging API over the Xamarin Firebase Analytics binding — no manual `Bundle` construction or JNI parameter packing required in your app code.

## Problem

Using `Xamarin.Firebase.Analytics` directly requires constructing an Android `Bundle` object and calling typed `Put*` methods (`PutString`, `PutLong`, etc.) for every event parameter. This is verbose, platform-specific, and cannot be called from shared cross-platform code.

## Solution

This bridge wraps the Xamarin binding with a `Dictionary<string, object>` API. It inspects each value's runtime type and calls the correct `Put*` method automatically, so you write platform-agnostic C# and the bridge handles the Android-specific plumbing.

## Setup

### 1. Add NuGet package

```bash
dotnet add package ExzileGames.AndroidAnalyticsBridge
```

### 2. Add google-services.json

Place your `google-services.json` (from the Firebase Console) in the Android app project root and set its build action:

```xml
<GoogleServicesJson Include="google-services.json" />
```

Firebase Analytics will not initialize without it.

### 3. Initialize in your Activity

```csharp
using AndroidAnalyticsBridge.Interop;

protected override void OnCreate(Bundle? savedInstanceState)
{
    base.OnCreate(savedInstanceState);
    AnalyticsBridgeManager.SetImplementation(new AndroidAnalyticsBridgeImpl(this));
}
```

### 4. Use from shared code

```csharp
using AndroidAnalyticsBridge.Interop;

// Log a simple event
AnalyticsBridgeManager.LogEvent("level_complete");

// Log an event with parameters
AnalyticsBridgeManager.LogEvent("purchase", new Dictionary<string, object>
{
    ["item_id"]  = "redchest500",
    ["value"]    = 4.99,
    ["currency"] = "USD"
});

// Set user ID (pass null to clear)
AnalyticsBridgeManager.SetUserId("user-12345");

// Set a custom user property (pass null to clear)
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
