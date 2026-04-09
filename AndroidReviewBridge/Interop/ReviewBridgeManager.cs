namespace AndroidReviewBridge.Interop
{
    /// <summary>
    /// Static access point for the Review Bridge. Set the platform implementation
    /// at startup via <see cref="SetImplementation"/>, then call from shared code.
    /// </summary>
    public static class ReviewBridgeManager
    {
        private static IReviewBridge? _impl;

        /// <summary>Gets whether a review implementation has been registered.</summary>
        public static bool IsAvailable => _impl != null;

        /// <summary>Registers the platform-specific review implementation.</summary>
        /// <param name="implementation">The platform review bridge to use.</param>
        public static void SetImplementation(IReviewBridge implementation) => _impl = implementation;

        /// <summary>Requests and launches the in-app review flow.</summary>
        /// <returns><c>true</c> if the flow completed; <c>false</c> if no implementation is set.</returns>
        public static Task<bool> RequestAndLaunchReviewAsync()
            => _impl?.RequestAndLaunchReviewAsync() ?? Task.FromResult(false);
    }
}
