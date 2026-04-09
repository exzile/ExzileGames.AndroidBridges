#if ANDROID
using Android.App;
using Android.OS;
using Firebase.Analytics;

namespace AndroidAnalyticsBridge.Interop
{
    /// <summary>
    /// Android implementation of <see cref="IAnalyticsBridge"/> backed by Firebase Analytics.
    /// </summary>
    public sealed class AndroidAnalyticsBridgeImpl : IAnalyticsBridge
    {
        private readonly FirebaseAnalytics _analytics;

        /// <inheritdoc/>
        public bool IsAvailable => true;

        /// <summary>
        /// Initializes a new instance of <see cref="AndroidAnalyticsBridgeImpl"/> using the
        /// provided activity as the Android context.
        /// </summary>
        /// <param name="activity">The current Android <see cref="Activity"/>.</param>
        public AndroidAnalyticsBridgeImpl(Activity activity)
        {
            _analytics = FirebaseAnalytics.GetInstance(activity);
        }

        /// <inheritdoc/>
        public void LogEvent(string name, IDictionary<string, object>? parameters = null)
        {
            Bundle? bundle = null;
            if (parameters != null && parameters.Count > 0)
            {
                bundle = new Bundle();
                foreach (var (key, value) in parameters)
                {
                    switch (value)
                    {
                        case string s: bundle.PutString(key, s); break;
                        case int i: bundle.PutInt(key, i); break;
                        case long l: bundle.PutLong(key, l); break;
                        case float f: bundle.PutFloat(key, f); break;
                        case double d: bundle.PutDouble(key, d); break;
                        case bool b: bundle.PutBoolean(key, b); break;
                        default: bundle.PutString(key, value?.ToString()); break;
                    }
                }
            }
            _analytics.LogEvent(name, bundle);
        }

        /// <inheritdoc/>
        public void SetUserId(string? userId) => _analytics.SetUserId(userId);

        /// <inheritdoc/>
        public void SetUserProperty(string name, string? value) => _analytics.SetUserProperty(name, value);

        /// <inheritdoc/>
        public void SetAnalyticsCollectionEnabled(bool enabled) => _analytics.SetAnalyticsCollectionEnabled(enabled);

        /// <inheritdoc/>
        public Task<string?> GetAppInstanceIdAsync()
        {
            var tcs = new TaskCompletionSource<string?>();
            _analytics.GetAppInstanceId()
                .AddOnSuccessListener(new SuccessListener<Java.Lang.String>(id => tcs.TrySetResult(id?.ToString())))
                .AddOnFailureListener(new FailureListener(e => tcs.TrySetResult(null)));
            return tcs.Task;
        }

        private sealed class SuccessListener<T> : Java.Lang.Object, Android.Gms.Tasks.IOnSuccessListener where T : Java.Lang.Object
        {
            private readonly Action<T?> _action;
            public SuccessListener(Action<T?> action) => _action = action;
            public void OnSuccess(Java.Lang.Object? result) => _action(result as T);
        }

        private sealed class FailureListener : Java.Lang.Object, Android.Gms.Tasks.IOnFailureListener
        {
            private readonly Action<Java.Lang.Exception> _action;
            public FailureListener(Action<Java.Lang.Exception> action) => _action = action;
            public void OnFailure(Java.Lang.Exception e) => _action(e);
        }
    }
}
#endif
