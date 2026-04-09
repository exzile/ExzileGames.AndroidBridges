# AndroidConsentBridge

A Java bridge + C# interop library for Google's User Messaging Platform (UMP) v3 on .NET Android. It collects GDPR and CCPA consent from the user before any ads are shown, and must be called at app startup — before initializing AndroidAdsBridge or requesting any ad. The bridge compiles a Java class directly into your Android project so the UMP SDK is called natively without reflection hacks, then exposes a clean async C# API via auto-generated JNI bindings.

## Setup

### 1. Add project reference

```xml
<ProjectReference Include="..\AndroidConsentBridge\AndroidConsentBridge.csproj" />
```

### 2. Initialize in your Activity

```csharp
using AndroidConsentBridge.Interop;

protected override void OnCreate(Bundle? savedInstanceState)
{
    base.OnCreate(savedInstanceState);

    var consent = new AndroidConsentBridgeImpl(this);
    ConsentBridgeManager.SetImplementation(consent);
}
```

### 3. Collect consent before showing ads

```csharp
using AndroidConsentBridge.Interop;

// Request a consent info update from the UMP server
var updateResult = await ConsentBridgeManager.RequestConsentInfoUpdateAsync();

if (updateResult.Success)
{
    // Load and show the form if required (no-op when not in a regulated region)
    var formResult = await ConsentBridgeManager.LoadAndShowConsentFormIfRequiredAsync();

    if (formResult.CanRequestAds)
    {
        // Safe to initialize ads now
        AdsBridgeManager.Initialize(...);
    }
}
```

## License

MIT
