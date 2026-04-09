#if ANDROID
using Android.App;

namespace AndroidCrashlyticsBridge.Interop
{
    /// <summary>
    /// Android implementation of <see cref="ICrashlyticsBridge"/> that delegates
    /// to the compiled Java class <c>CrashlyticsBridge</c> via auto-generated JNI bindings.
    /// </summary>
    public sealed class AndroidCrashlyticsBridgeImpl : ICrashlyticsBridge
    {
        private readonly Activity _activity;
        private readonly global::Com.Exzilegames.Crashlyticsbridge.CrashlyticsBridge _bridge;

        /// <inheritdoc/>
        public bool IsAvailable => true;

        /// <inheritdoc/>
        public bool DidCrashOnPreviousExecution => _bridge.DidCrashOnPreviousExecution();

        /// <summary>Creates a new Crashlytics bridge for the given Android activity.</summary>
        /// <param name="activity">The current Android activity. Stored for context but Crashlytics internally uses <c>getInstance()</c>.</param>
        public AndroidCrashlyticsBridgeImpl(Activity activity)
        {
            _activity = activity;
            _bridge = new global::Com.Exzilegames.Crashlyticsbridge.CrashlyticsBridge();
        }

        /// <inheritdoc/>
        public void RecordException(Exception exception)
            => _bridge.RecordException(
                exception.GetType().FullName ?? "Exception",
                exception.Message ?? "",
                exception.StackTrace ?? "");

        /// <inheritdoc/>
        public void RecordException(string exceptionType, string message, string stackTrace)
            => _bridge.RecordException(exceptionType, message, stackTrace);

        /// <inheritdoc/>
        public void Log(string message)
            => _bridge.Log(message);

        /// <inheritdoc/>
        public void SetUserId(string userId)
            => _bridge.SetUserId(userId);

        /// <inheritdoc/>
        public void SetCustomKey(string key, string value)
            => _bridge.SetCustomKeyString(key, value);

        /// <inheritdoc/>
        public void SetCustomKey(string key, bool value)
            => _bridge.SetCustomKeyBool(key, value);

        /// <inheritdoc/>
        public void SetCustomKey(string key, int value)
            => _bridge.SetCustomKeyInt(key, value);

        /// <inheritdoc/>
        public void SetCustomKey(string key, float value)
            => _bridge.SetCustomKeyFloat(key, value);

        /// <inheritdoc/>
        public void SetCollectionEnabled(bool enabled)
            => _bridge.SetCrashlyticsCollectionEnabled(enabled);
    }
}
#endif
