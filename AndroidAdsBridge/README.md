# AndroidAdsBridge

A Java bridge + C# interop library for Google Mobile Ads (AdMob) on .NET Android. Supports rewarded, interstitial, rewarded interstitial, app open, and banner ad formats with a clean async C# API.

## Problem

The `Xamarin.GooglePlayServices.Ads` NuGet binding wraps the AdMob SDK but exposes a callback-heavy Java API that is cumbersome to use from C#. Loading an ad, waiting for it to be ready, showing it, and receiving the result requires wiring up multiple Java listener interfaces by hand. Banner ad lifecycle (show/hide/destroy) is especially fragile when done through the raw binding.

## Solution

This library compiles Java bridge classes directly into your Android project — one per ad format. Each bridge manages the listener lifecycle internally and exposes a clean async C# API via auto-generated JNI bindings. No manual listener wiring, no reflection.

## APIs Exposed

| Format | Methods |
|--------|---------|
| **Rewarded** | `LoadRewardedAd`, `IsRewardedAdAvailable`, `ShowRewardedAdAsync` |
| **Interstitial** | `LoadInterstitialAd`, `IsInterstitialAdAvailable`, `ShowInterstitialAdAsync` |
| **Rewarded Interstitial** | `LoadRewardedInterstitialAd`, `IsRewardedInterstitialAdAvailable`, `ShowRewardedInterstitialAdAsync` |
| **App Open** | `LoadAppOpenAd`, `IsAppOpenAdAvailable`, `ShowAppOpenAdAsync` |
| **Banner** | `ShowBannerAd`, `HideBannerAd`, `RevealBannerAd`, `DestroyBannerAd` |

## Setup

### 1. Add NuGet package

```bash
dotnet add package ExzileGames.AndroidAdsBridge
```

### 2. Initialize in your Activity

```csharp
using AndroidAdsBridge.Interop;

protected override void OnCreate(Bundle? savedInstanceState)
{
    base.OnCreate(savedInstanceState);
    AdsBridgeManager.SetImplementation(new AndroidAdsBridgeImpl(this));
}
```

### 3. Use from shared code

```csharp
using AndroidAdsBridge.Interop;

// --- Rewarded ads ---
// Load (call early, e.g. at level start)
AdsBridgeManager.LoadRewardedAd("ca-app-pub-xxx/yyy");

// Show when ready
if (AdsBridgeManager.IsRewardedAdAvailable)
{
    var result = await AdsBridgeManager.ShowRewardedAdAsync();
    if (result.RewardEarned)
        GrantReward();
}

// --- Interstitial ads ---
AdsBridgeManager.LoadInterstitialAd("ca-app-pub-xxx/yyy");

if (AdsBridgeManager.IsInterstitialAdAvailable)
    await AdsBridgeManager.ShowInterstitialAdAsync();

// --- Banner ads ---
AdsBridgeManager.ShowBannerAd("ca-app-pub-xxx/yyy", BannerPosition.Bottom);
AdsBridgeManager.HideBannerAd();   // hide temporarily
AdsBridgeManager.RevealBannerAd(); // show again
AdsBridgeManager.DestroyBannerAd(); // remove permanently
```

## Notes

- Call `LoadXxxAd` before `ShowXxxAdAsync` — show fails immediately if no ad is loaded.
- `ShowRewardedAdAsync` and `ShowRewardedInterstitialAdAsync` accept an optional `CancellationToken`.
- Consent must be collected via `AndroidConsentBridge` before initializing or loading any ads.

## License

MIT
