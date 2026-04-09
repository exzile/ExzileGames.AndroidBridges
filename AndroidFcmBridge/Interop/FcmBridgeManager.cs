namespace AndroidFcmBridge.Interop
{
    /// <summary>
    /// Static access point for the FCM Bridge. Set the platform implementation
    /// at startup via <see cref="SetImplementation"/>, then call methods from shared code.
    /// </summary>
    /// <remarks>
    /// <para>
    /// On Android the consuming project must register <c>ExzileFcmService</c> in its
    /// <c>AndroidManifest.xml</c> so that Firebase can deliver token refreshes and
    /// incoming messages to the bridge:
    /// </para>
    /// <code>
    /// &lt;service android:name="com.exzilegames.fcmbridge.ExzileFcmService"
    ///          android:exported="false"&gt;
    ///     &lt;intent-filter&gt;
    ///         &lt;action android:name="com.google.firebase.MESSAGING_EVENT" /&gt;
    ///     &lt;/intent-filter&gt;
    /// &lt;/service&gt;
    /// </code>
    /// </remarks>
    public static class FcmBridgeManager
    {
        private static IFcmBridge? _impl;

        /// <summary>Gets whether an FCM implementation is available on this platform.</summary>
        public static bool IsAvailable => _impl?.IsAvailable ?? false;

        /// <summary>Registers the platform-specific FCM implementation.</summary>
        /// <param name="implementation">The platform FCM bridge to use.</param>
        public static void SetImplementation(IFcmBridge implementation)
            => _impl = implementation;

        /// <summary>Retrieves the current FCM registration token asynchronously.</summary>
        /// <returns>
        /// A <see cref="FcmTokenResult"/> containing the token, or a failure result if no
        /// implementation has been registered.
        /// </returns>
        public static Task<FcmTokenResult> GetTokenAsync()
            => _impl?.GetTokenAsync()
               ?? Task.FromResult(new FcmTokenResult(false, null, "No platform implementation"));

        /// <summary>Subscribes the device to the given FCM topic asynchronously.</summary>
        /// <param name="topic">The topic name to subscribe to.</param>
        /// <returns>
        /// An <see cref="FcmOperationResult"/> indicating success or failure.
        /// </returns>
        public static Task<FcmOperationResult> SubscribeToTopicAsync(string topic)
            => _impl?.SubscribeToTopicAsync(topic)
               ?? Task.FromResult(new FcmOperationResult(false, "No platform implementation"));

        /// <summary>Unsubscribes the device from the given FCM topic asynchronously.</summary>
        /// <param name="topic">The topic name to unsubscribe from.</param>
        /// <returns>
        /// An <see cref="FcmOperationResult"/> indicating success or failure.
        /// </returns>
        public static Task<FcmOperationResult> UnsubscribeFromTopicAsync(string topic)
            => _impl?.UnsubscribeFromTopicAsync(topic)
               ?? Task.FromResult(new FcmOperationResult(false, "No platform implementation"));

        /// <summary>
        /// Registers a listener that is invoked whenever the FCM registration token is refreshed.
        /// </summary>
        /// <param name="listener">The callback to invoke with the new token string.</param>
        public static void SetTokenRefreshListener(Action<string> listener)
            => _impl?.SetTokenRefreshListener(listener);

        /// <summary>
        /// Registers a listener that is invoked whenever a push message is received.
        /// </summary>
        /// <param name="listener">The callback to invoke with the received <see cref="FcmMessage"/>.</param>
        public static void SetMessageListener(Action<FcmMessage> listener)
            => _impl?.SetMessageListener(listener);
    }
}
