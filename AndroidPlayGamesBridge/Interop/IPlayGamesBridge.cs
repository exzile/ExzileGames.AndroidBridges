namespace AndroidPlayGamesBridge.Interop
{
    /// <summary>
    /// Platform-agnostic interface for Google Play Games Services v2.
    /// Exposes APIs missing from the Xamarin.GooglePlayServices.Games.V2 NuGet bindings.
    /// </summary>
    public interface IPlayGamesBridge
    {
        /// <summary>Gets whether a Play Games implementation is available on this platform.</summary>
        bool IsAvailable { get; }

        // ── Sign-In ──

        /// <summary>Signs in to Play Games Services.</summary>
        /// <param name="silent">If true, attempts silent sign-in without UI prompts.</param>
        Task<SignInResult> SignInAsync(bool silent = true);

        // ── Players ──

        /// <summary>Gets the currently signed-in player's information.</summary>
        Task<PlayerInfo> GetCurrentPlayerAsync();

        // ── Snapshots (Saved Games) ──

        /// <summary>Loads a saved game snapshot by name.</summary>
        /// <param name="snapshotName">The name of the snapshot to load.</param>
        Task<SnapshotLoadResult> LoadSnapshotAsync(string snapshotName);

        /// <summary>Saves data to a named snapshot.</summary>
        /// <param name="snapshotName">The name of the snapshot.</param>
        /// <param name="data">The data to save.</param>
        /// <param name="description">A human-readable description of the save.</param>
        Task<SnapshotSaveResult> SaveSnapshotAsync(string snapshotName, string data, string description);

        /// <summary>Deletes a saved game snapshot.</summary>
        /// <param name="snapshotName">The name of the snapshot to delete.</param>
        Task<SnapshotDeleteResult> DeleteSnapshotAsync(string snapshotName);

        // ── Leaderboards ──

        /// <summary>Submits a score to a leaderboard.</summary>
        /// <param name="leaderboardId">The leaderboard identifier.</param>
        /// <param name="score">The score value to submit.</param>
        Task<OperationResult> SubmitScoreAsync(string leaderboardId, long score);

        /// <summary>Shows the UI for a specific leaderboard.</summary>
        /// <param name="leaderboardId">The leaderboard identifier.</param>
        void ShowLeaderboard(string leaderboardId);

        /// <summary>Shows the UI for all leaderboards.</summary>
        void ShowAllLeaderboards();

        // ── Achievements ──

        /// <summary>Unlocks an achievement.</summary>
        /// <param name="achievementId">The achievement identifier.</param>
        Task<OperationResult> UnlockAchievementAsync(string achievementId);

        /// <summary>Increments a step-based achievement.</summary>
        /// <param name="achievementId">The achievement identifier.</param>
        /// <param name="steps">The number of steps to increment.</param>
        Task<OperationResult> IncrementAchievementAsync(string achievementId, int steps);

        /// <summary>Reveals a hidden achievement.</summary>
        /// <param name="achievementId">The achievement identifier.</param>
        Task<OperationResult> RevealAchievementAsync(string achievementId);

        /// <summary>Shows the achievements UI.</summary>
        void ShowAchievements();

        // ── Events ──

        /// <summary>Increments an event counter.</summary>
        /// <param name="eventId">The event identifier.</param>
        /// <param name="steps">The number of steps to increment.</param>
        void IncrementEvent(string eventId, int steps);

        /// <summary>Loads all events and their current values.</summary>
        Task<EventsResult> LoadEventsAsync();

        // ── Recall (cross-device identity linking) ──

        /// <summary>Requests a recall access token for cross-device identity linking.</summary>
        /// <param name="sessionId">The session identifier.</param>
        Task<RecallAccessResult> RequestRecallAccessAsync(string sessionId);

        // ── Player Stats ──

        /// <summary>Gets gameplay statistics for the current player.</summary>
        Task<PlayerStatsResult> GetPlayerStatsAsync();
    }

    // ── Result types ──

    /// <summary>Result of a Play Games sign-in attempt.</summary>
    public readonly record struct SignInResult(bool Success, string? Message);

    /// <summary>Information about the signed-in player.</summary>
    public readonly record struct PlayerInfo(
        bool Success,
        string? PlayerId,
        string? DisplayName,
        string? HiResImageUri,
        string? IconImageUri,
        string? Message);

    /// <summary>Result of loading a saved game snapshot.</summary>
    public readonly record struct SnapshotLoadResult(bool Success, string? Data, string? Message);

    /// <summary>Result of saving a game snapshot.</summary>
    public readonly record struct SnapshotSaveResult(bool Success, string? Message);

    /// <summary>Result of deleting a game snapshot.</summary>
    public readonly record struct SnapshotDeleteResult(bool Success, string? Message);

    /// <summary>Result of a generic Play Games operation.</summary>
    public readonly record struct OperationResult(bool Success, string? Message);

    /// <summary>Result of requesting a recall access token.</summary>
    public readonly record struct RecallAccessResult(bool Success, string? SessionId, string? Message);

    /// <summary>A single Play Games event entry.</summary>
    public readonly record struct EventEntry(string Id, string Name, long Value);

    /// <summary>Result of loading Play Games events.</summary>
    public readonly record struct EventsResult(bool Success, EventEntry[]? Events, string? Message);

    /// <summary>Gameplay statistics for the current player.</summary>
    public readonly record struct PlayerStatsResult(
        bool Success,
        float AverageSessionLength,
        float ChurnProbability,
        int DaysSinceLastPlayed,
        int NumberOfPurchases,
        int NumberOfSessions,
        float SessionPercentile,
        float SpendPercentile,
        float SpendProbability,
        float TotalSpendNext28Days,
        string? Message);
}
