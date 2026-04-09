namespace AndroidAnalyticsBridge.Interop
{
    /// <summary>
    /// Platform-agnostic interface for Firebase Analytics event logging.
    /// </summary>
    public interface IAnalyticsBridge
    {
        /// <summary>
        /// Gets whether the analytics implementation is available and ready to use.
        /// </summary>
        bool IsAvailable { get; }

        /// <summary>
        /// Logs a Firebase Analytics event with an optional set of parameters.
        /// </summary>
        /// <param name="name">The name of the event. Must follow Firebase naming rules (1–40 chars, letters/digits/underscores, must start with a letter).</param>
        /// <param name="parameters">Optional dictionary of event parameters. Supported value types: <see cref="string"/>, <see cref="int"/>, <see cref="long"/>, <see cref="float"/>, <see cref="double"/>, <see cref="bool"/>. All other types are converted via <c>ToString()</c>.</param>
        void LogEvent(string name, IDictionary<string, object>? parameters = null);

        /// <summary>
        /// Sets the user ID for this Firebase Analytics session.
        /// Pass <c>null</c> to clear the current user ID.
        /// </summary>
        /// <param name="userId">The user identifier, or <c>null</c> to clear.</param>
        void SetUserId(string? userId);

        /// <summary>
        /// Sets a custom user property to the given value.
        /// Pass <c>null</c> to clear the property.
        /// </summary>
        /// <param name="name">The name of the user property (1–24 chars).</param>
        /// <param name="value">The value of the user property (1–36 chars), or <c>null</c> to clear.</param>
        void SetUserProperty(string name, string? value);

        /// <summary>
        /// Enables or disables Analytics data collection for this app.
        /// </summary>
        /// <param name="enabled"><c>true</c> to enable collection; <c>false</c> to disable.</param>
        void SetAnalyticsCollectionEnabled(bool enabled);

        /// <summary>
        /// Asynchronously retrieves the app instance ID assigned by Firebase Analytics.
        /// Returns <c>null</c> if the ID cannot be retrieved.
        /// </summary>
        /// <returns>A task resolving to the app instance ID string, or <c>null</c> on failure.</returns>
        Task<string?> GetAppInstanceIdAsync();
    }
}
