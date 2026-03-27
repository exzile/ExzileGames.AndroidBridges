namespace AndroidPlayGamesBridge.Interop
{
    /// <summary>
    /// Static access point for the Play Games Bridge. Set the platform implementation
    /// at startup via <see cref="SetImplementation"/>, then call methods from shared code.
    /// </summary>
    public static class PlayGamesBridgeManager
    {
        private static IPlayGamesBridge? _impl;

        public static bool IsAvailable => _impl?.IsAvailable ?? false;

        public static void SetImplementation(IPlayGamesBridge implementation)
        {
            _impl = implementation;
        }

        // ── Sign-In ──

        public static Task<SignInResult> SignInAsync(bool silent = true)
            => _impl?.SignInAsync(silent)
               ?? Task.FromResult(new SignInResult(false, "No platform implementation"));

        // ── Players ──

        public static Task<PlayerInfo> GetCurrentPlayerAsync()
            => _impl?.GetCurrentPlayerAsync()
               ?? Task.FromResult(new PlayerInfo(false, null, null, null, null, "No platform implementation"));

        // ── Snapshots ──

        public static Task<SnapshotLoadResult> LoadSnapshotAsync(string snapshotName)
            => _impl?.LoadSnapshotAsync(snapshotName)
               ?? Task.FromResult(new SnapshotLoadResult(false, null, "No platform implementation"));

        public static Task<SnapshotSaveResult> SaveSnapshotAsync(string snapshotName, string data, string description)
            => _impl?.SaveSnapshotAsync(snapshotName, data, description)
               ?? Task.FromResult(new SnapshotSaveResult(false, "No platform implementation"));

        public static Task<SnapshotDeleteResult> DeleteSnapshotAsync(string snapshotName)
            => _impl?.DeleteSnapshotAsync(snapshotName)
               ?? Task.FromResult(new SnapshotDeleteResult(false, "No platform implementation"));

        // ── Leaderboards ──

        public static Task<OperationResult> SubmitScoreAsync(string leaderboardId, long score)
            => _impl?.SubmitScoreAsync(leaderboardId, score)
               ?? Task.FromResult(new OperationResult(false, "No platform implementation"));

        public static void ShowLeaderboard(string leaderboardId)
            => _impl?.ShowLeaderboard(leaderboardId);

        public static void ShowAllLeaderboards()
            => _impl?.ShowAllLeaderboards();

        // ── Achievements ──

        public static Task<OperationResult> UnlockAchievementAsync(string achievementId)
            => _impl?.UnlockAchievementAsync(achievementId)
               ?? Task.FromResult(new OperationResult(false, "No platform implementation"));

        public static Task<OperationResult> IncrementAchievementAsync(string achievementId, int steps)
            => _impl?.IncrementAchievementAsync(achievementId, steps)
               ?? Task.FromResult(new OperationResult(false, "No platform implementation"));

        public static Task<OperationResult> RevealAchievementAsync(string achievementId)
            => _impl?.RevealAchievementAsync(achievementId)
               ?? Task.FromResult(new OperationResult(false, "No platform implementation"));

        public static void ShowAchievements()
            => _impl?.ShowAchievements();

        // ── Events ──

        public static void IncrementEvent(string eventId, int steps)
            => _impl?.IncrementEvent(eventId, steps);

        public static Task<EventsResult> LoadEventsAsync()
            => _impl?.LoadEventsAsync()
               ?? Task.FromResult(new EventsResult(false, null, "No platform implementation"));

        // ── Recall ──

        public static Task<RecallAccessResult> RequestRecallAccessAsync(string sessionId)
            => _impl?.RequestRecallAccessAsync(sessionId)
               ?? Task.FromResult(new RecallAccessResult(false, null, "No platform implementation"));

        // ── Player Stats ──

        public static Task<PlayerStatsResult> GetPlayerStatsAsync()
            => _impl?.GetPlayerStatsAsync()
               ?? Task.FromResult(new PlayerStatsResult(false, 0, 0, 0, 0, 0, 0, 0, 0, 0, "No platform implementation"));
    }
}
