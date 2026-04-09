namespace AndroidCrashlyticsBridge.Interop
{
    /// <summary>
    /// Static access point for the Crashlytics Bridge. Set the platform implementation
    /// at startup via <see cref="SetImplementation"/>, then call methods from shared code.
    /// All methods are safe no-ops when no implementation has been registered.
    /// </summary>
    public static class CrashlyticsBridgeManager
    {
        private static ICrashlyticsBridge? _impl;

        /// <summary>Gets whether a Crashlytics implementation is available on this platform.</summary>
        public static bool IsAvailable => _impl?.IsAvailable ?? false;

        /// <summary>
        /// Gets whether the app crashed during the previous execution.
        /// Returns <c>false</c> when no implementation has been registered.
        /// </summary>
        public static bool DidCrashOnPreviousExecution => _impl?.DidCrashOnPreviousExecution ?? false;

        /// <summary>Registers the platform-specific Crashlytics implementation.</summary>
        /// <param name="implementation">The platform Crashlytics bridge to use.</param>
        public static void SetImplementation(ICrashlyticsBridge implementation)
            => _impl = implementation;

        /// <summary>
        /// Records a C# exception in Crashlytics with proper type grouping.
        /// Extracts <see cref="Exception.GetType"/>, <see cref="Exception.Message"/>, and
        /// <see cref="Exception.StackTrace"/> and delegates to the platform implementation.
        /// No-ops when no implementation has been registered.
        /// </summary>
        /// <param name="exception">The exception to record.</param>
        public static void RecordException(Exception exception)
            => _impl?.RecordException(exception);

        /// <summary>
        /// Records an exception in Crashlytics using raw string components.
        /// No-ops when no implementation has been registered.
        /// </summary>
        /// <param name="exceptionType">The fully-qualified exception type name.</param>
        /// <param name="message">The exception message.</param>
        /// <param name="stackTrace">The stack trace string.</param>
        public static void RecordException(string exceptionType, string message, string stackTrace)
            => _impl?.RecordException(exceptionType, message, stackTrace);

        /// <summary>
        /// Writes a message to the Crashlytics log for the current session.
        /// No-ops when no implementation has been registered.
        /// </summary>
        /// <param name="message">The message to log.</param>
        public static void Log(string message)
            => _impl?.Log(message);

        /// <summary>
        /// Associates a user identifier with crash reports.
        /// No-ops when no implementation has been registered.
        /// </summary>
        /// <param name="userId">An application-specific user identifier.</param>
        public static void SetUserId(string userId)
            => _impl?.SetUserId(userId);

        /// <summary>
        /// Sets a custom string key-value pair that is attached to crash reports.
        /// No-ops when no implementation has been registered.
        /// </summary>
        /// <param name="key">The key name.</param>
        /// <param name="value">The string value.</param>
        public static void SetCustomKey(string key, string value)
            => _impl?.SetCustomKey(key, value);

        /// <summary>
        /// Sets a custom boolean key-value pair that is attached to crash reports.
        /// No-ops when no implementation has been registered.
        /// </summary>
        /// <param name="key">The key name.</param>
        /// <param name="value">The boolean value.</param>
        public static void SetCustomKey(string key, bool value)
            => _impl?.SetCustomKey(key, value);

        /// <summary>
        /// Sets a custom integer key-value pair that is attached to crash reports.
        /// No-ops when no implementation has been registered.
        /// </summary>
        /// <param name="key">The key name.</param>
        /// <param name="value">The integer value.</param>
        public static void SetCustomKey(string key, int value)
            => _impl?.SetCustomKey(key, value);

        /// <summary>
        /// Sets a custom float key-value pair that is attached to crash reports.
        /// No-ops when no implementation has been registered.
        /// </summary>
        /// <param name="key">The key name.</param>
        /// <param name="value">The float value.</param>
        public static void SetCustomKey(string key, float value)
            => _impl?.SetCustomKey(key, value);

        /// <summary>
        /// Enables or disables Crashlytics data collection.
        /// No-ops when no implementation has been registered.
        /// </summary>
        /// <param name="enabled"><c>true</c> to enable collection; <c>false</c> to disable it.</param>
        public static void SetCollectionEnabled(bool enabled)
            => _impl?.SetCollectionEnabled(enabled);
    }
}
