# AndroidCrashlyticsBridge

A Java bridge + C# interop library for Firebase Crashlytics on .NET Android. Records C# exceptions with proper type grouping so that Crashlytics groups crashes by exception type rather than lumping them all under a single generic bucket.

## Problem

When you call `FirebaseCrashlytics.getInstance().recordException(throwable)` from C# via the Xamarin binding, all recorded exceptions appear under the same generic issue in the Crashlytics dashboard. There is no way to pass a meaningful type name through the raw binding, so `NullReferenceException`, `InvalidOperationException`, and every other C# exception look identical.

## Solution

This library compiles a Java bridge class directly into your Android project. The Java class constructs a real `Throwable` subclass whose `toString()` returns the original C# type name. Crashlytics uses `toString()` for grouping, so each distinct C# exception type gets its own issue in the dashboard. C# stack traces are also parsed and mapped to `StackTraceElement` objects so frames are readable in the Crashlytics UI.

## APIs Exposed

| API | Methods |
|-----|---------|
| **Exception recording** | `RecordException(Exception)`, `RecordException(string type, string message, string stackTrace)` |
| **Logging** | `Log(string)` |
| **User identification** | `SetUserId(string)` |
| **Custom keys** | `SetCustomKey(string, string)`, `SetCustomKey(string, bool)`, `SetCustomKey(string, int)`, `SetCustomKey(string, float)` |
| **Collection control** | `SetCollectionEnabled(bool)` |
| **Crash detection** | `DidCrashOnPreviousExecution` |

## Setup

### 1. Add NuGet package

```bash
dotnet add package ExzileGames.AndroidCrashlyticsBridge
```

### 2. Add google-services.json

Place your `google-services.json` in the Android app project root and set its build action:

```xml
<GoogleServicesJson Include="google-services.json" />
```

Firebase Crashlytics initializes automatically via `FirebaseInitProvider` — no explicit `FirebaseApp.InitializeApp` call is required.

### 3. Initialize in your Activity

```csharp
using AndroidCrashlyticsBridge.Interop;

protected override void OnCreate(Bundle? savedInstanceState)
{
    base.OnCreate(savedInstanceState);

    var crashlytics = new AndroidCrashlyticsBridgeImpl(this);
    CrashlyticsBridgeManager.SetImplementation(crashlytics);
}
```

### 4. Use from shared code

```csharp
using AndroidCrashlyticsBridge.Interop;

// Record a caught exception (grouped by C# type in the dashboard)
try
{
    DoSomethingRisky();
}
catch (Exception ex)
{
    CrashlyticsBridgeManager.RecordException(ex);
}

// Attach contextual information
CrashlyticsBridgeManager.SetUserId("player-12345");
CrashlyticsBridgeManager.SetCustomKey("level", 7);
CrashlyticsBridgeManager.SetCustomKey("scene", "MainMenu");

// Write a breadcrumb log
CrashlyticsBridgeManager.Log("Player entered the shop");

// Check if the previous session crashed
if (CrashlyticsBridgeManager.DidCrashOnPreviousExecution)
    ShowCrashRecoveryDialog();
```

## License

MIT
