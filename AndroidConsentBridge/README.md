# AndroidConsentBridge

A Java bridge + C# interop library for Google's User Messaging Platform (UMP) v3 on .NET Android. Collects GDPR and CCPA consent before any ads are shown. Must be called at app startup — before initializing `AndroidAdsBridge` or requesting any ad.

## Problem

The `Xamarin.Google.UserMessagingPlatform` NuGet binding exposes the UMP SDK but requires wiring up Java `ConsentInformation` and form callbacks manually from C#. The binding's async surface returns `Task<Java.Lang.Object>` in several places, and errors from the UMP SDK (network errors, form load failures) are surfaced as raw Java exceptions that are hard to handle gracefully.

## Solution

A compiled Java bridge class manages the `ConsentInformation.requestConsentInfoUpdate` → form load → form show lifecycle. It returns structured results to C# with error codes and messages, all without requiring any Java callback wiring in your application code.

## APIs Exposed

| Method | Description |
|--------|-------------|
| `RequestConsentInfoUpdateAsync(bool underAgeOfConsent, bool debugReset)` | Fetches current consent status from UMP servers. Pass `debugReset: true` during testing to force the form to appear. |
| `LoadAndShowConsentFormIfRequiredAsync()` | Shows the consent form if consent is required. No-op when not in a regulated region. |
| `CanRequestAds` | `true` when it is safe to initialize AdMob and request ads. |
| `GetConsentStatus()` | Returns `Unknown`, `Required`, `NotRequired`, or `Obtained`. |
| `Reset()` | Resets consent state. Debug/testing only — do not call in production. |

## Setup

### 1. Add NuGet package

```bash
dotnet add package ExzileGames.AndroidConsentBridge
```

### 2. Initialize in your Activity

```csharp
using AndroidConsentBridge.Interop;

protected override void OnCreate(Bundle? savedInstanceState)
{
    base.OnCreate(savedInstanceState);
    ConsentBridgeManager.SetImplementation(new AndroidConsentBridgeImpl(this));
}
```

### 3. Collect consent before showing ads

```csharp
using AndroidConsentBridge.Interop;

// Request consent status from UMP servers
var update = await ConsentBridgeManager.RequestConsentInfoUpdateAsync(
    tagForUnderAgeOfConsent: false,
    debugReset: false);

if (update.Success)
{
    // Show the form if required (no-op outside regulated regions)
    var form = await ConsentBridgeManager.LoadAndShowConsentFormIfRequiredAsync();

    if (form.CanRequestAds)
    {
        // Safe to initialize and load ads now
        AdsBridgeManager.SetImplementation(new AndroidAdsBridgeImpl(this));
        AdsBridgeManager.LoadRewardedAd(adUnitId);
    }
}
```

## Notes

- Always call `RequestConsentInfoUpdateAsync` and `LoadAndShowConsentFormIfRequiredAsync` before initializing `AndroidAdsBridge`. Showing ads without collecting consent in regulated regions violates Google's policies.
- Use `debugReset: true` during development to force the consent form to appear regardless of region.
- `CanRequestAds` returns `true` both when consent has been obtained and when the user is not in a regulated region.

## License

MIT
