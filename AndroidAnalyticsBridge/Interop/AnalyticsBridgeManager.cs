namespace AndroidAnalyticsBridge.Interop
{
    /// <summary>
    /// Static access point for the Analytics Bridge. Register the platform implementation
    /// at startup via <see cref="SetImplementation"/>, then call from shared code.
    /// All methods are safe no-ops when no implementation has been registered.
    /// </summary>
    public static class AnalyticsBridgeManager
    {
        private static IAnalyticsBridge? _impl;

        /// <summary>
        /// Gets whether an analytics implementation has been registered and is available.
        /// </summary>
        public static bool IsAvailable => _impl?.IsAvailable ?? false;

        /// <summary>
        /// Registers the platform-specific analytics implementation.
        /// Call this once at app startup (e.g. in <c>Activity.OnCreate</c>) before
        /// logging any events.
        /// </summary>
        /// <param name="implementation">The platform analytics bridge to use.</param>
        public static void SetImplementation(IAnalyticsBridge implementation) => _impl = implementation;

        /// <summary>
        /// Logs a Firebase Analytics event with an optional set of parameters.
        /// No-op if no implementation is registered.
        /// </summary>
        /// <param name="name">The name of the event. Must follow Firebase naming rules (1–40 chars, letters/digits/underscores, must start with a letter).</param>
        /// <param name="parameters">Optional dictionary of event parameters. Supported value types: <see cref="string"/>, <see cref="int"/>, <see cref="long"/>, <see cref="float"/>, <see cref="double"/>, <see cref="bool"/>. All other types are converted via <c>ToString()</c>.</param>
        public static void LogEvent(string name, IDictionary<string, object>? parameters = null)
            => _impl?.LogEvent(name, parameters);

        /// <summary>
        /// Sets the user ID for this Firebase Analytics session.
        /// No-op if no implementation is registered.
        /// Pass <c>null</c> to clear the current user ID.
        /// </summary>
        /// <param name="userId">The user identifier, or <c>null</c> to clear.</param>
        public static void SetUserId(string? userId) => _impl?.SetUserId(userId);

        /// <summary>
        /// Sets a custom user property to the given value.
        /// No-op if no implementation is registered.
        /// Pass <c>null</c> as the value to clear the property.
        /// </summary>
        /// <param name="name">The name of the user property (1–24 chars).</param>
        /// <param name="value">The value of the user property (1–36 chars), or <c>null</c> to clear.</param>
        public static void SetUserProperty(string name, string? value) => _impl?.SetUserProperty(name, value);

        /// <summary>
        /// Enables or disables Analytics data collection for this app.
        /// No-op if no implementation is registered.
        /// </summary>
        /// <param name="enabled"><c>true</c> to enable collection; <c>false</c> to disable.</param>
        public static void SetAnalyticsCollectionEnabled(bool enabled)
            => _impl?.SetAnalyticsCollectionEnabled(enabled);

        /// <summary>
        /// Asynchronously retrieves the app instance ID assigned by Firebase Analytics.
        /// Returns <c>null</c> if no implementation is registered or the ID cannot be retrieved.
        /// </summary>
        /// <returns>A task resolving to the app instance ID string, or <c>null</c>.</returns>
        public static Task<string?> GetAppInstanceIdAsync()
            => _impl?.GetAppInstanceIdAsync() ?? Task.FromResult<string?>(null);
    }
}
