namespace AndroidReviewBridge.Interop
{
    /// <summary>
    /// Static access point for the Review Bridge. Set the platform implementation
    /// at startup via <see cref="SetImplementation"/>, then call from shared code.
    /// </summary>
    public static class ReviewBridgeManager
    {
        private static IReviewBridge? _impl;

        public static bool IsAvailable => _impl != null;

        public static void SetImplementation(IReviewBridge implementation) => _impl = implementation;

        public static Task<bool> RequestAndLaunchReviewAsync()
            => _impl?.RequestAndLaunchReviewAsync() ?? Task.FromResult(false);
    }
}
