# AndroidReviewBridge

A Java bridge + C# interop library for the Google Play In-App Review API on .NET Android. Triggers the native Play review prompt from your C# code with a single async call.

## Problem

The `Xamarin.Google.Android.Play.Review` NuGet binding wraps the Play Review API but requires manually managing `ReviewInfo` request tasks and launch tasks — both of which return `Task<Java.Lang.Object>` requiring unsafe casts. Errors surface as generic Java exceptions with no meaningful message.

## Solution

A compiled Java bridge class handles the two-step flow (`requestReviewFlow` → `launchReviewFlow`) internally and exposes a single async method to C#. Result and error handling are done on the Java side and surfaced as a typed `bool` result.

## Setup

### 1. Add NuGet package

```bash
dotnet add package ExzileGames.AndroidReviewBridge
```

### 2. Initialize in your Activity

```csharp
using AndroidReviewBridge.Interop;

protected override void OnCreate(Bundle? savedInstanceState)
{
    base.OnCreate(savedInstanceState);
    ReviewBridgeManager.SetImplementation(new AndroidReviewBridgeImpl(this));
}
```

### 3. Use from shared code

```csharp
using AndroidReviewBridge.Interop;

// Trigger the review flow (e.g. after completing a level or milestone)
bool completed = await ReviewBridgeManager.RequestAndLaunchReviewAsync();
```

## Notes

- Google does not expose whether the user actually submitted a review — the result only indicates whether the flow completed without error.
- Play may suppress the prompt silently if the user has been prompted recently or has already reviewed the app. This is by design and cannot be overridden.
- Call the flow at a natural pause point (post-level, post-session) — not in response to a button press or a direct ask.

## License

MIT
