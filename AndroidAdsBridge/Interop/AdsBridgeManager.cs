namespace AndroidAdsBridge.Interop
{
    /// <summary>
    /// Static access point for the Ads Bridge. Set the platform implementation
    /// at startup via <see cref="SetImplementation"/>, then call methods from shared code.
    /// </summary>
    public static class AdsBridgeManager
    {
        private static IAdsBridge? _impl;

        public static bool IsAvailable => _impl != null;

        public static void SetImplementation(IAdsBridge implementation) => _impl = implementation;

        // ── Rewarded ──
        public static bool IsRewardedAdAvailable => _impl?.IsRewardedAdAvailable ?? false;
        public static void LoadRewardedAd(string adUnitId) => _impl?.LoadRewardedAd(adUnitId);
        public static Task<AdsBridgeRewardedResult> ShowRewardedAdAsync(CancellationToken cancellationToken = default)
            => _impl?.ShowRewardedAdAsync(cancellationToken)
               ?? Task.FromResult(new AdsBridgeRewardedResult(false, false, "No platform implementation"));

        // ── Interstitial ──
        public static bool IsInterstitialAdAvailable => _impl?.IsInterstitialAdAvailable ?? false;
        public static void LoadInterstitialAd(string adUnitId) => _impl?.LoadInterstitialAd(adUnitId);
        public static Task<bool> ShowInterstitialAdAsync()
            => _impl?.ShowInterstitialAdAsync() ?? Task.FromResult(false);

        // ── Rewarded Interstitial ──
        public static bool IsRewardedInterstitialAdAvailable => _impl?.IsRewardedInterstitialAdAvailable ?? false;
        public static void LoadRewardedInterstitialAd(string adUnitId) => _impl?.LoadRewardedInterstitialAd(adUnitId);
        public static Task<AdsBridgeRewardedResult> ShowRewardedInterstitialAdAsync(CancellationToken cancellationToken = default)
            => _impl?.ShowRewardedInterstitialAdAsync(cancellationToken)
               ?? Task.FromResult(new AdsBridgeRewardedResult(false, false, "No platform implementation"));

        // ── App Open ──
        public static bool IsAppOpenAdAvailable => _impl?.IsAppOpenAdAvailable ?? false;
        public static void LoadAppOpenAd(string adUnitId) => _impl?.LoadAppOpenAd(adUnitId);
        public static Task<bool> ShowAppOpenAdAsync()
            => _impl?.ShowAppOpenAdAsync() ?? Task.FromResult(false);

        // ── Banner ──
        public static void ShowBannerAd(string adUnitId, BannerPosition position = BannerPosition.Bottom)
            => _impl?.ShowBannerAd(adUnitId, position);
        public static void HideBannerAd() => _impl?.HideBannerAd();
        public static void RevealBannerAd() => _impl?.RevealBannerAd();
        public static void DestroyBannerAd() => _impl?.DestroyBannerAd();
    }
}
