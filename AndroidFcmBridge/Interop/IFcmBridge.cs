namespace AndroidFcmBridge.Interop
{
    /// <summary>
    /// Platform-agnostic interface for Firebase Cloud Messaging.
    /// Provides async token retrieval, topic subscription, and push message callbacks.
    /// </summary>
    public interface IFcmBridge
    {
        /// <summary>Gets whether an FCM implementation is available on this platform.</summary>
        bool IsAvailable { get; }

        /// <summary>Retrieves the current FCM registration token asynchronously.</summary>
        /// <returns>A <see cref="FcmTokenResult"/> containing the token or an error message.</returns>
        Task<FcmTokenResult> GetTokenAsync();

        /// <summary>Subscribes the device to the given FCM topic asynchronously.</summary>
        /// <param name="topic">The topic name to subscribe to.</param>
        /// <returns>An <see cref="FcmOperationResult"/> indicating success or failure.</returns>
        Task<FcmOperationResult> SubscribeToTopicAsync(string topic);

        /// <summary>Unsubscribes the device from the given FCM topic asynchronously.</summary>
        /// <param name="topic">The topic name to unsubscribe from.</param>
        /// <returns>An <see cref="FcmOperationResult"/> indicating success or failure.</returns>
        Task<FcmOperationResult> UnsubscribeFromTopicAsync(string topic);

        /// <summary>
        /// Registers a listener that is invoked whenever the FCM registration token is refreshed.
        /// </summary>
        /// <param name="listener">The callback to invoke with the new token string.</param>
        void SetTokenRefreshListener(Action<string> listener);

        /// <summary>
        /// Registers a listener that is invoked whenever a push message is received.
        /// </summary>
        /// <param name="listener">The callback to invoke with the received <see cref="FcmMessage"/>.</param>
        void SetMessageListener(Action<FcmMessage> listener);
    }

    /// <summary>Result of an FCM token retrieval operation.</summary>
    /// <param name="Success">Whether the operation succeeded.</param>
    /// <param name="Token">The FCM registration token, or <see langword="null"/> on failure.</param>
    /// <param name="Message">An error message, or <see langword="null"/> on success.</param>
    public readonly record struct FcmTokenResult(bool Success, string? Token, string? Message);

    /// <summary>Result of an FCM topic subscription or unsubscription operation.</summary>
    /// <param name="Success">Whether the operation succeeded.</param>
    /// <param name="Message">An error message, or <see langword="null"/> on success.</param>
    public readonly record struct FcmOperationResult(bool Success, string? Message);

    /// <summary>Represents a push message received from Firebase Cloud Messaging.</summary>
    /// <param name="Title">The notification title, or an empty string if not present.</param>
    /// <param name="Body">The notification body, or an empty string if not present.</param>
    /// <param name="Data">The key-value data payload attached to the message.</param>
    public readonly record struct FcmMessage(string Title, string Body, IReadOnlyDictionary<string, string> Data);
}
