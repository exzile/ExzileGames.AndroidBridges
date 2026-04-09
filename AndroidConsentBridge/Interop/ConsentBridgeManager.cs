namespace AndroidConsentBridge.Interop
{
    /// <summary>
    /// Static access point for the Consent Bridge. Register the platform implementation
    /// at startup via <see cref="SetImplementation"/>, then call methods from shared code.
    /// All members return safe defaults when no implementation has been registered.
    /// </summary>
    public static class ConsentBridgeManager
    {
        private static IConsentBridge? _impl;

        /// <summary>Gets whether a consent bridge implementation is available on this platform.</summary>
        public static bool IsAvailable => _impl?.IsAvailable ?? false;

        /// <summary>
        /// Gets whether ads can be requested given the current consent state.
        /// Returns <see langword="false"/> if no implementation has been registered.
        /// </summary>
        public static bool CanRequestAds => _impl?.CanRequestAds ?? false;

        /// <summary>
        /// Registers the platform-specific consent bridge implementation.
        /// Call this once at application startup before using any other member.
        /// </summary>
        /// <param name="implementation">The platform consent bridge to use.</param>
        public static void SetImplementation(IConsentBridge implementation)
            => _impl = implementation;

        /// <summary>
        /// Requests an update of the user's consent information from the UMP server.
        /// Call this once per app session before calling
        /// <see cref="LoadAndShowConsentFormIfRequiredAsync"/>.
        /// </summary>
        /// <param name="tagForUnderAgeOfConsent">
        /// <see langword="true"/> to tag the request as directed at users under the age of consent.
        /// Defaults to <see langword="false"/>.
        /// </param>
        /// <param name="debugReset">
        /// If <see langword="true"/>, clears any previously stored consent state before requesting
        /// an update. Use only during development and testing.
        /// </param>
        /// <returns>
        /// A <see cref="ConsentUpdateResult"/> indicating success or failure, or a failed result
        /// with an explanatory message when no implementation is registered.
        /// </returns>
        public static Task<ConsentUpdateResult> RequestConsentInfoUpdateAsync(
            bool tagForUnderAgeOfConsent = false,
            bool debugReset = false)
            => _impl?.RequestConsentInfoUpdateAsync(tagForUnderAgeOfConsent, debugReset)
               ?? Task.FromResult(new ConsentUpdateResult(false, -1, "No platform implementation"));

        /// <summary>
        /// Loads and shows the UMP consent form if it is required for this user.
        /// If the form is not required (e.g. outside a regulated region), completes immediately
        /// with the current consent state and no error.
        /// </summary>
        /// <returns>
        /// A <see cref="ConsentFormResult"/> containing the final consent state and any error,
        /// or a failed result when no implementation is registered.
        /// </returns>
        public static Task<ConsentFormResult> LoadAndShowConsentFormIfRequiredAsync()
            => _impl?.LoadAndShowConsentFormIfRequiredAsync()
               ?? Task.FromResult(new ConsentFormResult(false, ConsentStatus.Unknown, true, -1,
                   "No platform implementation"));

        /// <summary>
        /// Returns the current UMP consent status.
        /// Returns <see cref="ConsentStatus.Unknown"/> when no implementation is registered.
        /// </summary>
        /// <returns>The current <see cref="ConsentStatus"/>.</returns>
        public static ConsentStatus GetConsentStatus()
            => _impl?.GetConsentStatus() ?? ConsentStatus.Unknown;

        /// <summary>
        /// Resets the consent state, clearing any previously collected consent.
        /// <para><strong>For debug/testing only.</strong> Do not call in production builds.</para>
        /// </summary>
        public static void Reset() => _impl?.Reset();
    }
}
