namespace AndroidAdsBridge.Interop
{
    /// <summary>
    /// Static access point for the Ads Bridge. Set the platform implementation
    /// at startup via <see cref="SetImplementation"/>, then call methods from shared code.
    /// </summary>
    public static class AdsBridgeManager
    {
        private static IAdsBridge? _impl;

        /// <summary>Gets whether an ads implementation has been registered.</summary>
        public static bool IsAvailable => _impl != null;

        /// <summary>Registers the platform-specific ads implementation.</summary>
        /// <param name="implementation">The platform ads bridge to use.</param>
        public static void SetImplementation(IAdsBridge implementation) => _impl = implementation;

        // ── Rewarded ──

        /// <summary>Gets whether a rewarded ad is loaded and ready to show.</summary>
        public static bool IsRewardedAdAvailable => _impl?.IsRewardedAdAvailable ?? false;

        /// <summary>Loads a rewarded ad for the given ad unit.</summary>
        /// <param name="adUnitId">The ad unit identifier.</param>
        public static void LoadRewardedAd(string adUnitId) => _impl?.LoadRewardedAd(adUnitId);

        /// <summary>Shows the loaded rewarded ad and returns the result.</summary>
        /// <param name="cancellationToken">Optional token to cancel the operation.</param>
        public static Task<AdsBridgeRewardedResult> ShowRewardedAdAsync(CancellationToken cancellationToken = default)
            => _impl?.ShowRewardedAdAsync(cancellationToken)
               ?? Task.FromResult(new AdsBridgeRewardedResult(false, false, "No platform implementation"));

        // ── Interstitial ──

        /// <summary>Gets whether an interstitial ad is loaded and ready to show.</summary>
        public static bool IsInterstitialAdAvailable => _impl?.IsInterstitialAdAvailable ?? false;

        /// <summary>Loads an interstitial ad for the given ad unit.</summary>
        /// <param name="adUnitId">The ad unit identifier.</param>
        public static void LoadInterstitialAd(string adUnitId) => _impl?.LoadInterstitialAd(adUnitId);

        /// <summary>Shows the loaded interstitial ad and returns whether it was shown.</summary>
        public static Task<bool> ShowInterstitialAdAsync()
            => _impl?.ShowInterstitialAdAsync() ?? Task.FromResult(false);

        // ── Rewarded Interstitial ──

        /// <summary>Gets whether a rewarded interstitial ad is loaded and ready to show.</summary>
        public static bool IsRewardedInterstitialAdAvailable => _impl?.IsRewardedInterstitialAdAvailable ?? false;

        /// <summary>Loads a rewarded interstitial ad for the given ad unit.</summary>
        /// <param name="adUnitId">The ad unit identifier.</param>
        public static void LoadRewardedInterstitialAd(string adUnitId) => _impl?.LoadRewardedInterstitialAd(adUnitId);

        /// <summary>Shows the loaded rewarded interstitial ad and returns the result.</summary>
        /// <param name="cancellationToken">Optional token to cancel the operation.</param>
        public static Task<AdsBridgeRewardedResult> ShowRewardedInterstitialAdAsync(CancellationToken cancellationToken = default)
            => _impl?.ShowRewardedInterstitialAdAsync(cancellationToken)
               ?? Task.FromResult(new AdsBridgeRewardedResult(false, false, "No platform implementation"));

        // ── App Open ──

        /// <summary>Gets whether an app open ad is loaded and ready to show.</summary>
        public static bool IsAppOpenAdAvailable => _impl?.IsAppOpenAdAvailable ?? false;

        /// <summary>Loads an app open ad for the given ad unit.</summary>
        /// <param name="adUnitId">The ad unit identifier.</param>
        public static void LoadAppOpenAd(string adUnitId) => _impl?.LoadAppOpenAd(adUnitId);

        /// <summary>Shows the loaded app open ad and returns whether it was shown.</summary>
        public static Task<bool> ShowAppOpenAdAsync()
            => _impl?.ShowAppOpenAdAsync() ?? Task.FromResult(false);

        // ── Banner ──

        /// <summary>Shows a banner ad at the specified screen position.</summary>
        /// <param name="adUnitId">The ad unit identifier.</param>
        /// <param name="position">The screen position for the banner.</param>
        public static void ShowBannerAd(string adUnitId, BannerPosition position = BannerPosition.Bottom)
            => _impl?.ShowBannerAd(adUnitId, position);

        /// <summary>Hides the currently visible banner ad.</summary>
        public static void HideBannerAd() => _impl?.HideBannerAd();

        /// <summary>Reveals a previously hidden banner ad.</summary>
        public static void RevealBannerAd() => _impl?.RevealBannerAd();

        /// <summary>Destroys the banner ad and releases its resources.</summary>
        public static void DestroyBannerAd() => _impl?.DestroyBannerAd();
    }
}
