#if ANDROID
using Android.App;

namespace AndroidReviewBridge.Interop
{
    /// <summary>
    /// Android implementation of <see cref="IReviewBridge"/> that delegates to the
    /// compiled Java class <c>ReviewBridge</c> via auto-generated JNI bindings.
    /// </summary>
    public sealed class AndroidReviewBridgeImpl : Java.Lang.Object, IReviewBridge
    {
        private readonly WeakReference<Activity> _activityRef;
        private readonly global::Com.Exzilegames.Reviewbridge.ReviewBridge _bridge;

        /// <summary>Creates a new review bridge backed by the given Android activity.</summary>
        /// <param name="activity">The activity used to launch the review flow on the UI thread.</param>
        public AndroidReviewBridgeImpl(Activity activity)
        {
            _activityRef = new WeakReference<Activity>(activity);
            _bridge = new global::Com.Exzilegames.Reviewbridge.ReviewBridge();
        }

        /// <inheritdoc/>
        public Task<bool> RequestAndLaunchReviewAsync()
        {
            if (!_activityRef.TryGetTarget(out var activity))
                return Task.FromResult(false);

            var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

            activity.RunOnUiThread(() =>
            {
                try
                {
                    _bridge.RequestAndLaunch(activity, new ReviewListenerImpl(
                        onComplete: () => tcs.TrySetResult(true),
                        onFailed: _ => tcs.TrySetResult(false)));
                }
                catch { tcs.TrySetResult(false); }
            });

            return tcs.Task;
        }

        private sealed class ReviewListenerImpl : Java.Lang.Object,
            global::Com.Exzilegames.Reviewbridge.ReviewBridge.IReviewListener
        {
            private readonly Action _onComplete;
            private readonly Action<string?> _onFailed;

            public ReviewListenerImpl(Action onComplete, Action<string?> onFailed)
            {
                _onComplete = onComplete;
                _onFailed = onFailed;
            }

            public void OnComplete() => _onComplete();
            public void OnFailed(string? message) => _onFailed(message);
        }
    }
}
#endif
