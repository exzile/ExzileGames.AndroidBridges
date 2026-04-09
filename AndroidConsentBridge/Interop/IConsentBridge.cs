namespace AndroidConsentBridge.Interop
{
    /// <summary>
    /// Platform-agnostic interface for Google User Messaging Platform (UMP) consent collection.
    /// Provides GDPR/CCPA consent flows required before showing ads via AndroidAdsBridge.
    /// </summary>
    public interface IConsentBridge
    {
        /// <summary>Gets whether a consent bridge implementation is available on this platform.</summary>
        bool IsAvailable { get; }

        /// <summary>
        /// Requests an update of the user's consent information from the UMP server.
        /// Call this once per app session before calling <see cref="LoadAndShowConsentFormIfRequiredAsync"/>.
        /// </summary>
        /// <param name="tagForUnderAgeOfConsent">
        /// <see langword="true"/> to tag the request as directed at users under the age of consent
        /// (COPPA / EEA). Defaults to <see langword="false"/>.
        /// </param>
        /// <param name="debugReset">
        /// If <see langword="true"/>, resets any previously stored consent state before requesting
        /// an update. Use only during development and testing.
        /// </param>
        /// <returns>A <see cref="ConsentUpdateResult"/> indicating success or failure.</returns>
        Task<ConsentUpdateResult> RequestConsentInfoUpdateAsync(
            bool tagForUnderAgeOfConsent = false,
            bool debugReset = false);

        /// <summary>
        /// Loads and shows the UMP consent form if it is required for this user.
        /// If the form is not required (e.g. outside a regulated region), completes immediately
        /// with the current consent state and no error.
        /// </summary>
        /// <returns>
        /// A <see cref="ConsentFormResult"/> containing the final consent state and any error.
        /// </returns>
        Task<ConsentFormResult> LoadAndShowConsentFormIfRequiredAsync();

        /// <summary>
        /// Gets whether ads can be requested given the current consent state.
        /// Check this before initializing AndroidAdsBridge or requesting any ad.
        /// </summary>
        bool CanRequestAds { get; }

        /// <summary>Returns the current UMP consent status.</summary>
        ConsentStatus GetConsentStatus();

        /// <summary>
        /// Resets the consent state, clearing any previously collected consent.
        /// <para><strong>For debug/testing only.</strong> Do not call in production builds.</para>
        /// </summary>
        void Reset();
    }

    /// <summary>
    /// UMP consent status values, matching the integer constants returned by the UMP SDK.
    /// </summary>
    public enum ConsentStatus
    {
        /// <summary>Consent status is unknown (not yet determined).</summary>
        Unknown = 0,

        /// <summary>User consent is required but has not been obtained.</summary>
        Required = 1,

        /// <summary>User consent is not required (e.g. outside a regulated region).</summary>
        NotRequired = 2,

        /// <summary>User consent has been obtained.</summary>
        Obtained = 3
    }

    /// <summary>Result of a <see cref="IConsentBridge.RequestConsentInfoUpdateAsync"/> call.</summary>
    /// <param name="Success">Whether the update succeeded.</param>
    /// <param name="ErrorCode">The UMP error code, or 0 on success.</param>
    /// <param name="Message">A human-readable description of the error, or <see langword="null"/> on success.</param>
    public readonly record struct ConsentUpdateResult(bool Success, int ErrorCode, string? Message);

    /// <summary>Result of a <see cref="IConsentBridge.LoadAndShowConsentFormIfRequiredAsync"/> call.</summary>
    /// <param name="CanRequestAds">Whether ads may be requested given the final consent state.</param>
    /// <param name="Status">The final <see cref="ConsentStatus"/> after the form was shown or skipped.</param>
    /// <param name="HadError">Whether the form flow encountered an error.</param>
    /// <param name="ErrorCode">The UMP error code, or 0 if no error occurred.</param>
    /// <param name="ErrorMessage">A human-readable description of the error, or <see langword="null"/> if no error.</param>
    public readonly record struct ConsentFormResult(
        bool CanRequestAds,
        ConsentStatus Status,
        bool HadError,
        int ErrorCode,
        string? ErrorMessage);
}
