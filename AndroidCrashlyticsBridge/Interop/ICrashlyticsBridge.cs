namespace AndroidCrashlyticsBridge.Interop
{
    /// <summary>
    /// Platform-agnostic interface for Firebase Crashlytics.
    /// Exposes crash reporting, logging, user identification, and custom keys
    /// without direct dependency on the Android Crashlytics SDK.
    /// </summary>
    public interface ICrashlyticsBridge
    {
        /// <summary>Gets whether a Crashlytics implementation is available on this platform.</summary>
        bool IsAvailable { get; }

        /// <summary>
        /// Records a C# exception in Crashlytics with proper type grouping.
        /// Extracts the type name, message, and stack trace from the exception
        /// and wraps them in a Java <c>Throwable</c> so Crashlytics groups crashes
        /// by exception type rather than lumping them all as "recorded exception".
        /// </summary>
        /// <param name="exception">The exception to record.</param>
        void RecordException(Exception exception);

        /// <summary>
        /// Records an exception in Crashlytics using raw string components.
        /// Use this overload when the original <see cref="Exception"/> object is unavailable,
        /// for example when re-hydrating a serialised crash report.
        /// </summary>
        /// <param name="exceptionType">The fully-qualified exception type name (e.g. <c>System.NullReferenceException</c>).</param>
        /// <param name="message">The exception message.</param>
        /// <param name="stackTrace">The stack trace string. C# format (<c>at … in …:line N</c>) is parsed automatically.</param>
        void RecordException(string exceptionType, string message, string stackTrace);

        /// <summary>Writes a message to the Crashlytics log for the current session.</summary>
        /// <param name="message">The message to log.</param>
        void Log(string message);

        /// <summary>
        /// Associates a user identifier with crash reports.
        /// Pass an empty string to clear the current user identifier.
        /// </summary>
        /// <param name="userId">An application-specific user identifier.</param>
        void SetUserId(string userId);

        /// <summary>Sets a custom string key-value pair that is attached to crash reports.</summary>
        /// <param name="key">The key name.</param>
        /// <param name="value">The string value.</param>
        void SetCustomKey(string key, string value);

        /// <summary>Sets a custom boolean key-value pair that is attached to crash reports.</summary>
        /// <param name="key">The key name.</param>
        /// <param name="value">The boolean value.</param>
        void SetCustomKey(string key, bool value);

        /// <summary>Sets a custom integer key-value pair that is attached to crash reports.</summary>
        /// <param name="key">The key name.</param>
        /// <param name="value">The integer value.</param>
        void SetCustomKey(string key, int value);

        /// <summary>Sets a custom float key-value pair that is attached to crash reports.</summary>
        /// <param name="key">The key name.</param>
        /// <param name="value">The float value.</param>
        void SetCustomKey(string key, float value);

        /// <summary>
        /// Enables or disables Crashlytics data collection.
        /// Call with <c>false</c> before initialisation to opt the user out of crash reporting.
        /// </summary>
        /// <param name="enabled"><c>true</c> to enable collection; <c>false</c> to disable it.</param>
        void SetCollectionEnabled(bool enabled);

        /// <summary>
        /// Gets whether the app crashed during the previous execution.
        /// Useful for prompting users to submit a crash report or for diagnostic flows.
        /// </summary>
        bool DidCrashOnPreviousExecution { get; }
    }
}
