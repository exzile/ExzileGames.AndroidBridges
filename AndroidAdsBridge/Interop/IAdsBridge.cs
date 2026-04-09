namespace AndroidAdsBridge.Interop
{
    /// <summary>
    /// Platform-agnostic interface for Google Mobile Ads (AdMob).
    /// Covers rewarded, interstitial, rewarded interstitial, app open, and banner ad types.
    /// </summary>
    public interface IAdsBridge
    {
        // ── Rewarded ──
        bool IsRewardedAdAvailable { get; }
        void LoadRewardedAd(string adUnitId);
        Task<AdsBridgeRewardedResult> ShowRewardedAdAsync(CancellationToken cancellationToken = default);

        // ── Interstitial ──
        bool IsInterstitialAdAvailable { get; }
        void LoadInterstitialAd(string adUnitId);
        Task<bool> ShowInterstitialAdAsync();

        // ── Rewarded Interstitial ──
        bool IsRewardedInterstitialAdAvailable { get; }
        void LoadRewardedInterstitialAd(string adUnitId);
        Task<AdsBridgeRewardedResult> ShowRewardedInterstitialAdAsync(CancellationToken cancellationToken = default);

        // ── App Open ──
        bool IsAppOpenAdAvailable { get; }
        void LoadAppOpenAd(string adUnitId);
        Task<bool> ShowAppOpenAdAsync();

        // ── Banner ──
        void ShowBannerAd(string adUnitId, BannerPosition position = BannerPosition.Bottom);
        void HideBannerAd();
        void RevealBannerAd();
        void DestroyBannerAd();
    }

    public readonly record struct AdsBridgeRewardedResult(bool Shown, bool RewardEarned, string? Message);

    public enum BannerPosition { Top, Bottom }
}
