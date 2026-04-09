# AndroidAppUpdateBridge

A Java bridge + C# interop library for the Google Play In-App Updates API on .NET Android. Supports both immediate and flexible update flows with clean async C# wrappers.

## Problem

The Xamarin binding for `com.google.android.play:app-update` has a GC lifetime bug: `InstallStateUpdatedListener` instances created in C# can be collected by the Mono GC mid-update because nothing on the managed heap holds a strong reference once the call site returns. This causes silent failures during flexible update downloads.

## Solution

A compiled Java class (`AppUpdateBridge`) holds a strong reference to the `InstallStateUpdatedListener` as a Java field, keeping it alive for the entire download lifecycle. C# communicates with it via auto-generated JNI bindings — no reflection needed.

## Setup

### 1. Add NuGet package

```bash
dotnet add package ExzileGames.AndroidAppUpdateBridge
```

### 2. Initialize in your Activity

```csharp
using AndroidAppUpdateBridge.Interop;

protected override void OnCreate(Bundle? savedInstanceState)
{
    base.OnCreate(savedInstanceState);
    AppUpdateBridgeManager.SetImplementation(new AndroidAppUpdateBridgeImpl(this));
}
```

### 3. Use from shared code

```csharp
using AndroidAppUpdateBridge.Interop;

var info = await AppUpdateBridgeManager.CheckForUpdateAsync();

if (info.Availability == UpdateAvailability.UpdateAvailable)
{
    if (info.ImmediateAllowed)
    {
        // Blocks the user until the update is installed
        var result = await AppUpdateBridgeManager.StartImmediateUpdateAsync();
    }
    else if (info.FlexibleAllowed)
    {
        // Downloads in the background; prompt user to restart when ready
        var progress = new Progress<FlexibleUpdateProgress>(p =>
        {
            if (p.IsDownloaded)
                AppUpdateBridgeManager.CompleteFlexibleUpdate();
        });

        var result = await AppUpdateBridgeManager.StartFlexibleUpdateAsync(progress);
    }
}
```

## Update Types

| Type | Behaviour |
|------|-----------|
| **Immediate** | Full-screen UI blocks the app until the update is installed. Best for critical updates. |
| **Flexible** | Downloads in the background while the user continues using the app. Call `CompleteFlexibleUpdate()` once `IsDownloaded` is true to trigger installation. |

## License

MIT
