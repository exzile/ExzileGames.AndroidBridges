namespace AndroidReviewBridge.Interop
{
    /// <summary>
    /// Platform-agnostic interface for the Google Play In-App Review API.
    /// </summary>
    public interface IReviewBridge
    {
        /// <summary>
        /// Requests a ReviewInfo token from Google Play and immediately launches the
        /// in-app review flow. Returns true when the flow completes (regardless of
        /// whether the user actually submitted a review — Google does not expose that).
        /// </summary>
        Task<bool> RequestAndLaunchReviewAsync();
    }
}
