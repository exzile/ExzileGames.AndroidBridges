#if ANDROID
using Android.App;

namespace AndroidAdsBridge.Interop
{
    /// <summary>
    /// Android implementation of <see cref="IAdsBridge"/> that delegates to compiled Java
    /// bridge classes via auto-generated JNI bindings.
    /// </summary>
    public sealed class AndroidAdsBridgeImpl : Java.Lang.Object, IAdsBridge
    {
        private readonly WeakReference<Activity> _activityRef;

        private readonly global::Com.Exzilegames.Adsbridge.RewardedBridge _rewarded;
        private readonly global::Com.Exzilegames.Adsbridge.InterstitialBridge _interstitial;
        private readonly global::Com.Exzilegames.Adsbridge.RewardedInterstitialBridge _rewardedInterstitial;
        private readonly global::Com.Exzilegames.Adsbridge.AppOpenBridge _appOpen;
        private readonly global::Com.Exzilegames.Adsbridge.BannerBridge _banner;

        private DateTime _lastRewardedLoadUtc = DateTime.MinValue;
        private DateTime _lastInterstitialLoadUtc = DateTime.MinValue;
        private DateTime _lastRewardedInterstitialLoadUtc = DateTime.MinValue;
        private DateTime _lastAppOpenLoadUtc = DateTime.MinValue;

        public AndroidAdsBridgeImpl(Activity activity)
        {
            _activityRef = new WeakReference<Activity>(activity);
            _rewarded = new global::Com.Exzilegames.Adsbridge.RewardedBridge();
            _interstitial = new global::Com.Exzilegames.Adsbridge.InterstitialBridge();
            _rewardedInterstitial = new global::Com.Exzilegames.Adsbridge.RewardedInterstitialBridge();
            _appOpen = new global::Com.Exzilegames.Adsbridge.AppOpenBridge();
            _banner = new global::Com.Exzilegames.Adsbridge.BannerBridge();
        }

        // ── Rewarded ──

        public bool IsRewardedAdAvailable
        {
            get { try { return _rewarded.IsReady; } catch { return false; } }
        }

        public void LoadRewardedAd(string adUnitId)
        {
            if (!Throttle(ref _lastRewardedLoadUtc)) return;
            if (!_activityRef.TryGetTarget(out var activity)) return;
            activity.RunOnUiThread(() => { try { _rewarded.Load(activity, adUnitId); } catch { } });
        }

        public Task<AdsBridgeRewardedResult> ShowRewardedAdAsync(CancellationToken cancellationToken = default)
        {
            if (!IsRewardedAdAvailable)
                return Task.FromResult(new AdsBridgeRewardedResult(false, false, "rewarded ad not loaded"));

            var tcs = new TaskCompletionSource<AdsBridgeRewardedResult>(TaskCreationOptions.RunContinuationsAsynchronously);
            var rewardEarned = false;

            if (cancellationToken.CanBeCanceled)
                cancellationToken.Register(() => tcs.TrySetResult(new AdsBridgeRewardedResult(false, false, "cancelled")));

            if (!_activityRef.TryGetTarget(out var activity))
            {
                tcs.TrySetResult(new AdsBridgeRewardedResult(false, false, "activity unavailable"));
                return tcs.Task;
            }

            activity.RunOnUiThread(() =>
            {
                try
                {
                    _rewarded.SetListener(new RewardedListenerImpl(
                        onEarned: () => rewardEarned = true,
                        onDismissed: () => tcs.TrySetResult(new AdsBridgeRewardedResult(true, rewardEarned, null)),
                        onFailedToShow: msg => tcs.TrySetResult(new AdsBridgeRewardedResult(false, false, msg))));

                    if (!_rewarded.Show(activity))
                        tcs.TrySetResult(new AdsBridgeRewardedResult(false, false, "rewarded ad not loaded"));
                }
                catch (Exception ex) { tcs.TrySetResult(new AdsBridgeRewardedResult(false, false, ex.Message)); }
            });

            return tcs.Task;
        }

        // ── Interstitial ──

        public bool IsInterstitialAdAvailable
        {
            get { try { return _interstitial.IsReady; } catch { return false; } }
        }

        public void LoadInterstitialAd(string adUnitId)
        {
            if (!Throttle(ref _lastInterstitialLoadUtc)) return;
            if (!_activityRef.TryGetTarget(out var activity)) return;
            activity.RunOnUiThread(() => { try { _interstitial.Load(activity, adUnitId); } catch { } });
        }

        public Task<bool> ShowInterstitialAdAsync()
        {
            if (!IsInterstitialAdAvailable) return Task.FromResult(false);

            var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

            if (!_activityRef.TryGetTarget(out var activity))
            {
                tcs.TrySetResult(false);
                return tcs.Task;
            }

            activity.RunOnUiThread(() =>
            {
                try
                {
                    _interstitial.SetListener(new InterstitialListenerImpl(
                        onDismissed: () => tcs.TrySetResult(true),
                        onFailedToShow: _ => tcs.TrySetResult(false)));

                    if (!_interstitial.Show(activity))
                        tcs.TrySetResult(false);
                }
                catch { tcs.TrySetResult(false); }
            });

            return tcs.Task;
        }

        // ── Rewarded Interstitial ──

        public bool IsRewardedInterstitialAdAvailable
        {
            get { try { return _rewardedInterstitial.IsReady; } catch { return false; } }
        }

        public void LoadRewardedInterstitialAd(string adUnitId)
        {
            if (!Throttle(ref _lastRewardedInterstitialLoadUtc)) return;
            if (!_activityRef.TryGetTarget(out var activity)) return;
            activity.RunOnUiThread(() => { try { _rewardedInterstitial.Load(activity, adUnitId); } catch { } });
        }

        public Task<AdsBridgeRewardedResult> ShowRewardedInterstitialAdAsync(CancellationToken cancellationToken = default)
        {
            if (!IsRewardedInterstitialAdAvailable)
                return Task.FromResult(new AdsBridgeRewardedResult(false, false, "rewarded interstitial not loaded"));

            var tcs = new TaskCompletionSource<AdsBridgeRewardedResult>(TaskCreationOptions.RunContinuationsAsynchronously);
            var rewardEarned = false;

            if (cancellationToken.CanBeCanceled)
                cancellationToken.Register(() => tcs.TrySetResult(new AdsBridgeRewardedResult(false, false, "cancelled")));

            if (!_activityRef.TryGetTarget(out var activity))
            {
                tcs.TrySetResult(new AdsBridgeRewardedResult(false, false, "activity unavailable"));
                return tcs.Task;
            }

            activity.RunOnUiThread(() =>
            {
                try
                {
                    _rewardedInterstitial.SetListener(new RewardedInterstitialListenerImpl(
                        onEarned: () => rewardEarned = true,
                        onDismissed: () => tcs.TrySetResult(new AdsBridgeRewardedResult(true, rewardEarned, null)),
                        onFailedToShow: msg => tcs.TrySetResult(new AdsBridgeRewardedResult(false, false, msg))));

                    if (!_rewardedInterstitial.Show(activity))
                        tcs.TrySetResult(new AdsBridgeRewardedResult(false, false, "rewarded interstitial not loaded"));
                }
                catch (Exception ex) { tcs.TrySetResult(new AdsBridgeRewardedResult(false, false, ex.Message)); }
            });

            return tcs.Task;
        }

        // ── App Open ──

        public bool IsAppOpenAdAvailable
        {
            get { try { return _appOpen.IsReady; } catch { return false; } }
        }

        public void LoadAppOpenAd(string adUnitId)
        {
            if (!Throttle(ref _lastAppOpenLoadUtc)) return;
            if (!_activityRef.TryGetTarget(out var activity)) return;
            activity.RunOnUiThread(() => { try { _appOpen.Load(activity, adUnitId); } catch { } });
        }

        public Task<bool> ShowAppOpenAdAsync()
        {
            if (!IsAppOpenAdAvailable) return Task.FromResult(false);

            var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

            if (!_activityRef.TryGetTarget(out var activity))
            {
                tcs.TrySetResult(false);
                return tcs.Task;
            }

            activity.RunOnUiThread(() =>
            {
                try
                {
                    _appOpen.SetListener(new AppOpenListenerImpl(
                        onDismissed: () => tcs.TrySetResult(true),
                        onFailedToShow: _ => tcs.TrySetResult(false)));

                    if (!_appOpen.Show(activity))
                        tcs.TrySetResult(false);
                }
                catch { tcs.TrySetResult(false); }
            });

            return tcs.Task;
        }

        // ── Banner ──

        public void ShowBannerAd(string adUnitId, BannerPosition position = BannerPosition.Bottom)
        {
            if (!_activityRef.TryGetTarget(out var activity)) return;
            _banner.Show(activity, adUnitId, position == BannerPosition.Top
                ? global::Com.Exzilegames.Adsbridge.BannerBridge.PositionTop
                : global::Com.Exzilegames.Adsbridge.BannerBridge.PositionBottom);
        }

        public void HideBannerAd()
        {
            if (_activityRef.TryGetTarget(out var activity)) _banner.Hide(activity);
        }

        public void RevealBannerAd()
        {
            if (_activityRef.TryGetTarget(out var activity)) _banner.Reveal(activity);
        }

        public void DestroyBannerAd()
        {
            if (_activityRef.TryGetTarget(out var activity)) _banner.Destroy(activity);
        }

        // ── Helpers ──

        private static bool Throttle(ref DateTime last, double seconds = 2.0)
        {
            if ((DateTime.UtcNow - last).TotalSeconds < seconds) return false;
            last = DateTime.UtcNow;
            return true;
        }

        // ── Listener implementations ──

        private sealed class RewardedListenerImpl : Java.Lang.Object,
            global::Com.Exzilegames.Adsbridge.RewardedBridge.IRewardedListener
        {
            private readonly Action _onEarned;
            private readonly Action _onDismissed;
            private readonly Action<string?> _onFailed;
            public RewardedListenerImpl(Action onEarned, Action onDismissed, Action<string?> onFailedToShow)
            { _onEarned = onEarned; _onDismissed = onDismissed; _onFailed = onFailedToShow; }
            public void OnRewardedEarned() => _onEarned();
            public void OnRewardedDismissed() => _onDismissed();
            public void OnRewardedFailedToShow(string? message) => _onFailed(message);
        }

        private sealed class InterstitialListenerImpl : Java.Lang.Object,
            global::Com.Exzilegames.Adsbridge.InterstitialBridge.IInterstitialListener
        {
            private readonly Action _onDismissed;
            private readonly Action<string?> _onFailed;
            public InterstitialListenerImpl(Action onDismissed, Action<string?> onFailedToShow)
            { _onDismissed = onDismissed; _onFailed = onFailedToShow; }
            public void OnDismissed() => _onDismissed();
            public void OnFailedToShow(string? message) => _onFailed(message);
        }

        private sealed class RewardedInterstitialListenerImpl : Java.Lang.Object,
            global::Com.Exzilegames.Adsbridge.RewardedInterstitialBridge.IRewardedInterstitialListener
        {
            private readonly Action _onEarned;
            private readonly Action _onDismissed;
            private readonly Action<string?> _onFailed;
            public RewardedInterstitialListenerImpl(Action onEarned, Action onDismissed, Action<string?> onFailedToShow)
            { _onEarned = onEarned; _onDismissed = onDismissed; _onFailed = onFailedToShow; }
            public void OnRewardedEarned() => _onEarned();
            public void OnDismissed() => _onDismissed();
            public void OnFailedToShow(string? message) => _onFailed(message);
        }

        private sealed class AppOpenListenerImpl : Java.Lang.Object,
            global::Com.Exzilegames.Adsbridge.AppOpenBridge.IAppOpenListener
        {
            private readonly Action _onDismissed;
            private readonly Action<string?> _onFailed;
            public AppOpenListenerImpl(Action onDismissed, Action<string?> onFailedToShow)
            { _onDismissed = onDismissed; _onFailed = onFailedToShow; }
            public void OnDismissed() => _onDismissed();
            public void OnFailedToShow(string? message) => _onFailed(message);
        }
    }
}
#endif
