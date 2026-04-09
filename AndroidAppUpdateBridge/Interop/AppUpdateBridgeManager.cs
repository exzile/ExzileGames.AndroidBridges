namespace AndroidAppUpdateBridge.Interop
{
    /// <summary>
    /// Static access point for the App Update Bridge. Register the platform implementation
    /// at startup via <see cref="SetImplementation"/>, then call from shared code.
    /// </summary>
    public static class AppUpdateBridgeManager
    {
        private static IAppUpdateBridge? _impl;

        /// <summary>
        /// Gets whether a platform implementation has been registered via
        /// <see cref="SetImplementation"/>.
        /// </summary>
        public static bool IsAvailable => _impl != null;

        /// <summary>
        /// Registers the platform-specific app update implementation.
        /// Call this once during Android activity initialisation before using any other members.
        /// </summary>
        /// <param name="implementation">The platform app update bridge to use.</param>
        public static void SetImplementation(IAppUpdateBridge implementation) => _impl = implementation;

        /// <summary>
        /// Queries Google Play for update availability without starting any update flow.
        /// </summary>
        /// <returns>
        /// An <see cref="AppUpdateInfo"/> with the result, or a default value indicating
        /// no platform implementation is available when none has been registered.
        /// </returns>
        public static Task<AppUpdateInfo> CheckForUpdateAsync()
            => _impl?.CheckForUpdateAsync()
               ?? Task.FromResult(new AppUpdateInfo(UpdateAvailability.Unknown, 0, 0, false, false,
                   "No platform implementation"));

        /// <summary>
        /// Starts an immediate (blocking) update flow. The user cannot interact with the app
        /// until the update is installed.
        /// </summary>
        /// <returns>
        /// An <see cref="AppUpdateResult"/> indicating success, cancellation, or failure.
        /// Returns a failure result when no implementation has been registered.
        /// </returns>
        public static Task<AppUpdateResult> StartImmediateUpdateAsync()
            => _impl?.StartImmediateUpdateAsync()
               ?? Task.FromResult(new AppUpdateResult(false, false, "No platform implementation"));

        /// <summary>
        /// Starts a flexible update flow that downloads in the background while the user
        /// continues using the app.
        /// </summary>
        /// <param name="progress">
        /// Optional progress receiver that is notified as the download proceeds and when
        /// the update is ready to install.
        /// </param>
        /// <returns>
        /// An <see cref="AppUpdateResult"/> that completes when the update is installed
        /// or has failed/been cancelled. Returns a failure result when no implementation
        /// has been registered.
        /// </returns>
        public static Task<AppUpdateResult> StartFlexibleUpdateAsync(IProgress<FlexibleUpdateProgress>? progress = null)
            => _impl?.StartFlexibleUpdateAsync(progress)
               ?? Task.FromResult(new AppUpdateResult(false, false, "No platform implementation"));

        /// <summary>
        /// Triggers installation of a flexible update that has already finished downloading.
        /// Call this after receiving a <see cref="FlexibleUpdateProgress"/> with
        /// <see cref="FlexibleUpdateProgress.IsDownloaded"/> set to <c>true</c>.
        /// Has no effect when no implementation has been registered.
        /// </summary>
        public static void CompleteFlexibleUpdate() => _impl?.CompleteFlexibleUpdate();
    }
}
