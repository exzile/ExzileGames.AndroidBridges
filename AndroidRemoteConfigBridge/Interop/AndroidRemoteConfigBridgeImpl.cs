#if ANDROID
using Android.Gms.Tasks;
using Firebase.RemoteConfig;

namespace AndroidRemoteConfigBridge.Interop
{
    /// <summary>
    /// Android implementation of <see cref="IRemoteConfigBridge"/> backed by
    /// <see cref="FirebaseRemoteConfig"/>. Wraps the Xamarin binding's
    /// <c>Task&lt;Java.Lang.Object&gt;</c> return types into properly typed results.
    /// </summary>
    public sealed class AndroidRemoteConfigBridgeImpl : IRemoteConfigBridge
    {
        private readonly FirebaseRemoteConfig _config;

        /// <inheritdoc/>
        public bool IsAvailable => true;

        /// <summary>
        /// Initialises the implementation using the app-default
        /// <see cref="FirebaseRemoteConfig"/> singleton.
        /// Firebase must be initialised (via <c>google-services.json</c>) before
        /// constructing this instance.
        /// </summary>
        public AndroidRemoteConfigBridgeImpl()
        {
            _config = FirebaseRemoteConfig.Instance;
        }

        /// <inheritdoc/>
        public Task<RemoteConfigFetchResult> FetchAndActivateAsync(TimeSpan? minimumFetchInterval = null)
        {
            var tcs = new TaskCompletionSource<RemoteConfigFetchResult>();

            if (minimumFetchInterval.HasValue)
            {
                // Fetch with a custom minimum interval, then activate separately.
                var fetchTask = _config.Fetch((long)minimumFetchInterval.Value.TotalSeconds);
                fetchTask
                    .AddOnSuccessListener(new SuccessListener<Java.Lang.Object>(_ =>
                    {
                        _config.Activate()
                            .AddOnSuccessListener(new SuccessListener<Java.Lang.Object>(activateResult =>
                            {
                                bool activated = activateResult is Java.Lang.Boolean b && b.BooleanValue();
                                tcs.TrySetResult(new RemoteConfigFetchResult(activated, true, null));
                            }))
                            .AddOnFailureListener(new FailureListener(e =>
                                tcs.TrySetResult(new RemoteConfigFetchResult(false, false, e?.Message))));
                    }))
                    .AddOnFailureListener(new FailureListener(e =>
                        tcs.TrySetResult(new RemoteConfigFetchResult(false, false, e?.Message))));
            }
            else
            {
                // FetchAndActivate: result is a boxed Java Boolean indicating whether
                // new values were activated (false = already up-to-date).
                _config.FetchAndActivate()
                    .AddOnSuccessListener(new SuccessListener<Java.Lang.Object>(result =>
                    {
                        bool activated = result is Java.Lang.Boolean b && b.BooleanValue();
                        tcs.TrySetResult(new RemoteConfigFetchResult(activated, true, null));
                    }))
                    .AddOnFailureListener(new FailureListener(e =>
                        tcs.TrySetResult(new RemoteConfigFetchResult(false, false, e?.Message))));
            }

            return tcs.Task;
        }

        /// <inheritdoc/>
        public void SetDefaults(IDictionary<string, object> defaults)
        {
            var map = new Dictionary<string, object>(defaults);
            _config.SetDefaultsAsync(map);
        }

        /// <inheritdoc/>
        public string GetString(string key, string defaultValue = "")
            => _config.GetString(key) ?? defaultValue;

        /// <inheritdoc/>
        public bool GetBool(string key, bool defaultValue = false)
            => _config.GetBoolean(key);

        /// <inheritdoc/>
        public long GetLong(string key, long defaultValue = 0)
            => _config.GetLong(key);

        /// <inheritdoc/>
        public double GetDouble(string key, double defaultValue = 0.0)
            => _config.GetDouble(key);

        // ── Listener helpers ────────────────────────────────────────────────────

        private sealed class SuccessListener<T> : Java.Lang.Object, IOnSuccessListener
            where T : Java.Lang.Object
        {
            private readonly Action<T?> _action;
            public SuccessListener(Action<T?> action) => _action = action;
            public void OnSuccess(Java.Lang.Object? result) => _action(result as T);
        }

        private sealed class FailureListener : Java.Lang.Object, IOnFailureListener
        {
            private readonly Action<Java.Lang.Exception?> _action;
            public FailureListener(Action<Java.Lang.Exception?> action) => _action = action;
            public void OnFailure(Java.Lang.Exception e) => _action(e);
        }
    }
}
#endif
