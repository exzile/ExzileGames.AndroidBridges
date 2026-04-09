#if ANDROID
using Android.App;

namespace AndroidConsentBridge.Interop
{
    /// <summary>
    /// Android implementation of <see cref="IConsentBridge"/> that delegates to the compiled
    /// Java class <c>ConsentBridge</c> via auto-generated JNI bindings.
    /// </summary>
    public sealed class AndroidConsentBridgeImpl : IConsentBridge
    {
        private readonly Activity _activity;
        private readonly global::Com.Exzilegames.Consentbridge.ConsentBridge _bridge;

        /// <inheritdoc/>
        public bool IsAvailable => true;

        /// <inheritdoc/>
        public bool CanRequestAds => _bridge.CanRequestAds(_activity);

        /// <summary>
        /// Creates a new consent bridge backed by the given Android activity.
        /// </summary>
        /// <param name="activity">The foreground activity used to present the consent form
        /// and to retrieve the UMP <c>ConsentInformation</c> object.</param>
        public AndroidConsentBridgeImpl(Activity activity)
        {
            _activity = activity;
            _bridge = new global::Com.Exzilegames.Consentbridge.ConsentBridge();
        }

        /// <inheritdoc/>
        public Task<ConsentUpdateResult> RequestConsentInfoUpdateAsync(
            bool tagForUnderAgeOfConsent = false,
            bool debugReset = false)
        {
            var tcs = new TaskCompletionSource<ConsentUpdateResult>();
            _activity.RunOnUiThread(() =>
            {
                _bridge.RequestConsentInfoUpdate(
                    _activity,
                    tagForUnderAgeOfConsent,
                    debugReset,
                    new ConsentUpdateListenerImpl(tcs));
            });
            return tcs.Task;
        }

        /// <inheritdoc/>
        public Task<ConsentFormResult> LoadAndShowConsentFormIfRequiredAsync()
        {
            var tcs = new TaskCompletionSource<ConsentFormResult>();
            _activity.RunOnUiThread(() =>
            {
                _bridge.LoadAndShowConsentFormIfRequired(
                    _activity,
                    new ConsentFormListenerImpl(tcs));
            });
            return tcs.Task;
        }

        /// <inheritdoc/>
        public ConsentStatus GetConsentStatus()
            => (ConsentStatus)_bridge.GetConsentStatus(_activity);

        /// <inheritdoc/>
        public void Reset()
            => _activity.RunOnUiThread(() => _bridge.Reset(_activity));

        // ── Listener implementations ──

        private sealed class ConsentUpdateListenerImpl : Java.Lang.Object,
            global::Com.Exzilegames.Consentbridge.ConsentBridge.IConsentUpdateListener
        {
            private readonly TaskCompletionSource<ConsentUpdateResult> _tcs;

            public ConsentUpdateListenerImpl(TaskCompletionSource<ConsentUpdateResult> tcs)
                => _tcs = tcs;

            public void OnSuccess()
                => _tcs.TrySetResult(new ConsentUpdateResult(true, 0, null));

            public void OnFailure(int errorCode, string? message)
                => _tcs.TrySetResult(new ConsentUpdateResult(false, errorCode, message));
        }

        private sealed class ConsentFormListenerImpl : Java.Lang.Object,
            global::Com.Exzilegames.Consentbridge.ConsentBridge.IConsentFormListener
        {
            private readonly TaskCompletionSource<ConsentFormResult> _tcs;

            public ConsentFormListenerImpl(TaskCompletionSource<ConsentFormResult> tcs)
                => _tcs = tcs;

            public void OnComplete(bool canRequestAds, int consentStatus, int errorCode, string? errorMessage)
            {
                bool hadError = errorCode != 0;
                _tcs.TrySetResult(new ConsentFormResult(
                    canRequestAds,
                    (ConsentStatus)consentStatus,
                    hadError,
                    errorCode,
                    hadError ? errorMessage : null));
            }
        }
    }
}
#endif
