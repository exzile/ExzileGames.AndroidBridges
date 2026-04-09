namespace AndroidAppUpdateBridge.Interop
{
    /// <summary>
    /// Platform-agnostic interface for the Google Play In-App Updates API.
    /// </summary>
    public interface IAppUpdateBridge
    {
        /// <summary>
        /// Gets whether this bridge has been initialised with an Android activity.
        /// </summary>
        bool IsAvailable { get; }

        /// <summary>
        /// Queries Google Play for update availability without starting any update flow.
        /// </summary>
        /// <returns>
        /// An <see cref="AppUpdateInfo"/> describing whether an update is available,
        /// which update types are allowed, and how stale the installed version is.
        /// </returns>
        Task<AppUpdateInfo> CheckForUpdateAsync();

        /// <summary>
        /// Starts an immediate (blocking) update flow. The user cannot interact with the
        /// app until the update is downloaded and installed.
        /// </summary>
        /// <returns>
        /// An <see cref="AppUpdateResult"/> indicating success, cancellation, or failure.
        /// </returns>
        Task<AppUpdateResult> StartImmediateUpdateAsync();

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
        /// or has failed/been cancelled.
        /// </returns>
        Task<AppUpdateResult> StartFlexibleUpdateAsync(IProgress<FlexibleUpdateProgress>? progress = null);

        /// <summary>
        /// Triggers installation of a flexible update that has already finished downloading.
        /// Call this after receiving a <see cref="FlexibleUpdateProgress"/> with
        /// <see cref="FlexibleUpdateProgress.IsDownloaded"/> set to <c>true</c>.
        /// </summary>
        void CompleteFlexibleUpdate();
    }

    /// <summary>Describes whether a Google Play update is available for the running app.</summary>
    public enum UpdateAvailability
    {
        /// <summary>The availability state is unknown or could not be determined.</summary>
        Unknown,

        /// <summary>No update is currently available on Google Play.</summary>
        UpdateNotAvailable,

        /// <summary>An update is available and can be started.</summary>
        UpdateAvailable,

        /// <summary>An update is already in progress (e.g. previously started and resumed).</summary>
        InProgress,
    }

    /// <summary>
    /// Information returned by <see cref="IAppUpdateBridge.CheckForUpdateAsync"/>.
    /// </summary>
    /// <param name="Availability">Whether an update is available.</param>
    /// <param name="AvailableVersionCode">The version code of the available update, or 0 if none.</param>
    /// <param name="StaleDays">
    /// The number of days the current client version has been available on Google Play,
    /// or 0 if unavailable.
    /// </param>
    /// <param name="ImmediateAllowed">Whether an immediate update flow is permitted.</param>
    /// <param name="FlexibleAllowed">Whether a flexible update flow is permitted.</param>
    /// <param name="Message">An error message when the check itself failed, otherwise <c>null</c>.</param>
    public readonly record struct AppUpdateInfo(
        UpdateAvailability Availability,
        int AvailableVersionCode,
        int StaleDays,
        bool ImmediateAllowed,
        bool FlexibleAllowed,
        string? Message);

    /// <summary>
    /// Outcome of an update flow started by
    /// <see cref="IAppUpdateBridge.StartImmediateUpdateAsync"/> or
    /// <see cref="IAppUpdateBridge.StartFlexibleUpdateAsync"/>.
    /// </summary>
    /// <param name="Success">
    /// <c>true</c> when the update completed (immediate) or was installed (flexible).
    /// </param>
    /// <param name="Cancelled"><c>true</c> when the user dismissed the update dialog.</param>
    /// <param name="Message">A diagnostic message on failure, otherwise <c>null</c>.</param>
    public readonly record struct AppUpdateResult(bool Success, bool Cancelled, string? Message);

    /// <summary>
    /// Download and install progress for a flexible update reported via
    /// <see cref="IAppUpdateBridge.StartFlexibleUpdateAsync"/>.
    /// </summary>
    /// <param name="BytesDownloaded">Bytes downloaded so far.</param>
    /// <param name="TotalBytes">Total bytes to download, or 0 if unknown.</param>
    /// <param name="IsDownloaded">
    /// <c>true</c> when the download is complete and
    /// <see cref="IAppUpdateBridge.CompleteFlexibleUpdate"/> may be called.
    /// </param>
    public readonly record struct FlexibleUpdateProgress(long BytesDownloaded, long TotalBytes, bool IsDownloaded);
}
