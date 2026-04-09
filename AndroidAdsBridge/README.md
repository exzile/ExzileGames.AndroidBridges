# ExzileGames.AndroidAdsBridge

Java bridge for Google Mobile Ads (AdMob) rewarded ads on .NET Android.

## Usage

```csharp
// In Activity startup:
AdsBridgeManager.SetImplementation(new AndroidAdsBridgeImpl(this));

// Load an ad:
AdsBridgeManager.LoadRewardedAd(adUnitId);

// Show an ad:
var result = await AdsBridgeManager.ShowRewardedAdAsync();
if (result.RewardEarned) { /* grant reward */ }
```
