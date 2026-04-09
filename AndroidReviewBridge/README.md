# ExzileGames.AndroidReviewBridge

Java bridge for the Google Play In-App Review API on .NET Android.

## Usage

```csharp
// In Activity startup:
ReviewBridgeManager.SetImplementation(new AndroidReviewBridgeImpl(this));

// Trigger the in-app review flow (e.g. after completing a level):
await ReviewBridgeManager.RequestAndLaunchReviewAsync();
```

Note: Google does not expose whether the user actually submitted a review.
The flow may be suppressed by Play if the user has recently been prompted.
