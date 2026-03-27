namespace AndroidPlayGamesBridge.Interop
{
    /// <summary>
    /// Platform-agnostic interface for Google Play Games Services v2.
    /// Exposes APIs missing from the Xamarin.GooglePlayServices.Games.V2 NuGet bindings.
    /// </summary>
    public interface IPlayGamesBridge
    {
        bool IsAvailable { get; }

        // ── Sign-In ──
        Task<SignInResult> SignInAsync(bool silent = true);

        // ── Players ──
        Task<PlayerInfo> GetCurrentPlayerAsync();

        // ── Snapshots (Saved Games) ──
        Task<SnapshotLoadResult> LoadSnapshotAsync(string snapshotName);
        Task<SnapshotSaveResult> SaveSnapshotAsync(string snapshotName, string data, string description);
        Task<SnapshotDeleteResult> DeleteSnapshotAsync(string snapshotName);

        // ── Leaderboards ──
        Task<OperationResult> SubmitScoreAsync(string leaderboardId, long score);
        void ShowLeaderboard(string leaderboardId);
        void ShowAllLeaderboards();

        // ── Achievements ──
        Task<OperationResult> UnlockAchievementAsync(string achievementId);
        Task<OperationResult> IncrementAchievementAsync(string achievementId, int steps);
        Task<OperationResult> RevealAchievementAsync(string achievementId);
        void ShowAchievements();

        // ── Events ──
        void IncrementEvent(string eventId, int steps);
        Task<EventsResult> LoadEventsAsync();

        // ── Recall (cross-device identity linking) ──
        Task<RecallAccessResult> RequestRecallAccessAsync(string sessionId);

        // ── Player Stats ──
        Task<PlayerStatsResult> GetPlayerStatsAsync();
    }

    // ── Result types ──

    public readonly record struct SignInResult(bool Success, string? Message);

    public readonly record struct PlayerInfo(
        bool Success,
        string? PlayerId,
        string? DisplayName,
        string? HiResImageUri,
        string? IconImageUri,
        string? Message);

    public readonly record struct SnapshotLoadResult(bool Success, string? Data, string? Message);
    public readonly record struct SnapshotSaveResult(bool Success, string? Message);
    public readonly record struct SnapshotDeleteResult(bool Success, string? Message);

    public readonly record struct OperationResult(bool Success, string? Message);

    public readonly record struct RecallAccessResult(bool Success, string? SessionId, string? Message);

    public readonly record struct EventEntry(string Id, string Name, long Value);
    public readonly record struct EventsResult(bool Success, EventEntry[]? Events, string? Message);

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
