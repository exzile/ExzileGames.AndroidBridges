namespace AndroidRemoteConfigBridge.Interop
{
    /// <summary>
    /// Static access point for the Remote Config Bridge. Register the platform implementation
    /// at startup via <see cref="SetImplementation"/>, then call methods from shared code.
    /// </summary>
    public static class RemoteConfigBridgeManager
    {
        private static IRemoteConfigBridge? _impl;

        /// <summary>Gets whether a Remote Config implementation is available on this platform.</summary>
        public static bool IsAvailable => _impl?.IsAvailable ?? false;

        /// <summary>Registers the platform-specific Remote Config implementation.</summary>
        /// <param name="implementation">The platform Remote Config bridge to use.</param>
        public static void SetImplementation(IRemoteConfigBridge implementation)
            => _impl = implementation;

        /// <summary>
        /// Fetches and activates the latest Remote Config values asynchronously.
        /// </summary>
        /// <param name="minimumFetchInterval">
        /// Optional minimum interval between fetches. When <see langword="null"/>, uses the
        /// Firebase default (12 hours in production). Pass <see cref="TimeSpan.Zero"/> to
        /// force an immediate fetch (useful during development).
        /// </param>
        /// <returns>
        /// A <see cref="RemoteConfigFetchResult"/> indicating whether values were activated and
        /// whether the operation succeeded. Returns a failure result with the message
        /// "No platform implementation" when no implementation has been registered.
        /// </returns>
        public static Task<RemoteConfigFetchResult> FetchAndActivateAsync(TimeSpan? minimumFetchInterval = null)
            => _impl?.FetchAndActivateAsync(minimumFetchInterval)
               ?? Task.FromResult(new RemoteConfigFetchResult(false, false, "No platform implementation"));

        /// <summary>
        /// Sets in-app default values for Remote Config parameters.
        /// Defaults are used before a fetch completes or when a key has no remote value.
        /// Does nothing when no implementation is registered.
        /// </summary>
        /// <param name="defaults">A dictionary mapping parameter keys to their default values.</param>
        public static void SetDefaults(IDictionary<string, object> defaults)
            => _impl?.SetDefaults(defaults);

        /// <summary>Returns the Remote Config value for <paramref name="key"/> as a string.</summary>
        /// <param name="key">The Remote Config parameter key.</param>
        /// <param name="defaultValue">Value returned when no implementation is set or the key is absent.</param>
        /// <returns>The string value, or <paramref name="defaultValue"/> if no implementation is registered.</returns>
        public static string GetString(string key, string defaultValue = "")
            => _impl?.GetString(key, defaultValue) ?? defaultValue;

        /// <summary>Returns the Remote Config value for <paramref name="key"/> as a boolean.</summary>
        /// <param name="key">The Remote Config parameter key.</param>
        /// <param name="defaultValue">Value returned when no implementation is set or the key is absent.</param>
        /// <returns>The boolean value, or <paramref name="defaultValue"/> if no implementation is registered.</returns>
        public static bool GetBool(string key, bool defaultValue = false)
            => _impl?.GetBool(key, defaultValue) ?? defaultValue;

        /// <summary>Returns the Remote Config value for <paramref name="key"/> as a 64-bit integer.</summary>
        /// <param name="key">The Remote Config parameter key.</param>
        /// <param name="defaultValue">Value returned when no implementation is set or the key is absent.</param>
        /// <returns>The long value, or <paramref name="defaultValue"/> if no implementation is registered.</returns>
        public static long GetLong(string key, long defaultValue = 0)
            => _impl?.GetLong(key, defaultValue) ?? defaultValue;

        /// <summary>Returns the Remote Config value for <paramref name="key"/> as a double.</summary>
        /// <param name="key">The Remote Config parameter key.</param>
        /// <param name="defaultValue">Value returned when no implementation is set or the key is absent.</param>
        /// <returns>The double value, or <paramref name="defaultValue"/> if no implementation is registered.</returns>
        public static double GetDouble(string key, double defaultValue = 0.0)
            => _impl?.GetDouble(key, defaultValue) ?? defaultValue;
    }
}
