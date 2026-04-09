namespace AndroidRemoteConfigBridge.Interop
{
    /// <summary>
    /// Platform-agnostic interface for Firebase Remote Config.
    /// Provides typed value access and a clean async fetch/activate API.
    /// </summary>
    public interface IRemoteConfigBridge
    {
        /// <summary>Gets whether a Remote Config implementation is available on this platform.</summary>
        bool IsAvailable { get; }

        /// <summary>
        /// Fetches and activates the latest Remote Config values asynchronously.
        /// </summary>
        /// <param name="minimumFetchInterval">
        /// Optional minimum interval between fetches. When <see langword="null"/>, uses the
        /// Firebase default (12 hours in production). Pass <see cref="TimeSpan.Zero"/> to
        /// force an immediate fetch (useful during development).
        /// </param>
        /// <returns>
        /// A <see cref="RemoteConfigFetchResult"/> indicating whether values were activated,
        /// whether the operation succeeded, and an optional error message.
        /// </returns>
        Task<RemoteConfigFetchResult> FetchAndActivateAsync(TimeSpan? minimumFetchInterval = null);

        /// <summary>
        /// Sets in-app default values for Remote Config parameters.
        /// Defaults are used before a fetch completes or when a key has no remote value.
        /// </summary>
        /// <param name="defaults">A dictionary mapping parameter keys to their default values.</param>
        void SetDefaults(IDictionary<string, object> defaults);

        /// <summary>Returns the Remote Config value for <paramref name="key"/> as a string.</summary>
        /// <param name="key">The Remote Config parameter key.</param>
        /// <param name="defaultValue">Value returned when no implementation is set or the key is absent.</param>
        /// <returns>The string value, or <paramref name="defaultValue"/> if unavailable.</returns>
        string GetString(string key, string defaultValue = "");

        /// <summary>Returns the Remote Config value for <paramref name="key"/> as a boolean.</summary>
        /// <param name="key">The Remote Config parameter key.</param>
        /// <param name="defaultValue">Value returned when no implementation is set or the key is absent.</param>
        /// <returns>The boolean value, or <paramref name="defaultValue"/> if unavailable.</returns>
        bool GetBool(string key, bool defaultValue = false);

        /// <summary>Returns the Remote Config value for <paramref name="key"/> as a 64-bit integer.</summary>
        /// <param name="key">The Remote Config parameter key.</param>
        /// <param name="defaultValue">Value returned when no implementation is set or the key is absent.</param>
        /// <returns>The long value, or <paramref name="defaultValue"/> if unavailable.</returns>
        long GetLong(string key, long defaultValue = 0);

        /// <summary>Returns the Remote Config value for <paramref name="key"/> as a double.</summary>
        /// <param name="key">The Remote Config parameter key.</param>
        /// <param name="defaultValue">Value returned when no implementation is set or the key is absent.</param>
        /// <returns>The double value, or <paramref name="defaultValue"/> if unavailable.</returns>
        double GetDouble(string key, double defaultValue = 0.0);
    }

    /// <summary>Result of a Firebase Remote Config fetch-and-activate operation.</summary>
    /// <param name="Activated">
    /// <see langword="true"/> if new values were fetched and activated;
    /// <see langword="false"/> if the cached values were already current or activation failed.
    /// </param>
    /// <param name="Success"><see langword="true"/> if the operation completed without error.</param>
    /// <param name="Message">An optional error or diagnostic message, or <see langword="null"/> on success.</param>
    public readonly record struct RemoteConfigFetchResult(bool Activated, bool Success, string? Message);
}
