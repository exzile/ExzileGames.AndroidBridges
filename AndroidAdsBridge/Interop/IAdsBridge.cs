namespace AndroidAdsBridge.Interop
{
    /// <summary>
    /// Platform-agnostic interface for Google Mobile Ads (AdMob).
    /// Covers rewarded, interstitial, rewarded interstitial, app open, and banner ad types.
    /// </summary>
    public interface IAdsBridge
    {
        // ── Rewarded ──

        /// <summary>Gets whether a rewarded ad is loaded and ready to show.</summary>
        bool IsRewardedAdAvailable { get; }

        /// <summary>Loads a rewarded ad for the given ad unit.</summary>
        void LoadRewardedAd(string adUnitId);

        /// <summary>Shows the loaded rewarded ad and returns the result.</summary>
        Task<AdsBridgeRewardedResult> ShowRewardedAdAsync(CancellationToken cancellationToken = default);

        // ── Interstitial ──

        /// <summary>Gets whether an interstitial ad is loaded and ready to show.</summary>
        bool IsInterstitialAdAvailable { get; }

        /// <summary>Loads an interstitial ad for the given ad unit.</summary>
        void LoadInterstitialAd(string adUnitId);

        /// <summary>Shows the loaded interstitial ad and returns whether it was shown.</summary>
        Task<bool> ShowInterstitialAdAsync();

        // ── Rewarded Interstitial ──

        /// <summary>Gets whether a rewarded interstitial ad is loaded and ready to show.</summary>
        bool IsRewardedInterstitialAdAvailable { get; }

        /// <summary>Loads a rewarded interstitial ad for the given ad unit.</summary>
        void LoadRewardedInterstitialAd(string adUnitId);

        /// <summary>Shows the loaded rewarded interstitial ad and returns the result.</summary>
        Task<AdsBridgeRewardedResult> ShowRewardedInterstitialAdAsync(CancellationToken cancellationToken = default);

        // ── App Open ──

        /// <summary>Gets whether an app open ad is loaded and ready to show.</summary>
        bool IsAppOpenAdAvailable { get; }

        /// <summary>Loads an app open ad for the given ad unit.</summary>
        void LoadAppOpenAd(string adUnitId);

        /// <summary>Shows the loaded app open ad and returns whether it was shown.</summary>
        Task<bool> ShowAppOpenAdAsync();

        // ── Banner ──

        /// <summary>Shows a banner ad at the specified screen position.</summary>
        void ShowBannerAd(string adUnitId, BannerPosition position = BannerPosition.Bottom);

        /// <summary>Hides the currently visible banner ad.</summary>
        void HideBannerAd();

        /// <summary>Reveals a previously hidden banner ad.</summary>
        void RevealBannerAd();

        /// <summary>Destroys the banner ad and releases its resources.</summary>
        void DestroyBannerAd();
    }

    /// <summary>Result of showing a rewarded ad, indicating whether it was shown and if the reward was earned.</summary>
    public readonly record struct AdsBridgeRewardedResult(bool Shown, bool RewardEarned, string? Message);

    /// <summary>Screen position for banner ads.</summary>
    public enum BannerPosition
    {
        /// <summary>Banner displayed at the top of the screen.</summary>
        Top,

        /// <summary>Banner displayed at the bottom of the screen.</summary>
        Bottom
    }
}
