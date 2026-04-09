namespace AndroidPlayGamesBridge.Interop
{
    /// <summary>
    /// Static access point for the Play Games Bridge. Set the platform implementation
    /// at startup via <see cref="SetImplementation"/>, then call methods from shared code.
    /// </summary>
    public static class PlayGamesBridgeManager
    {
        private static IPlayGamesBridge? _impl;

        /// <summary>Gets whether a Play Games implementation has been registered.</summary>
        public static bool IsAvailable => _impl?.IsAvailable ?? false;

        /// <summary>Registers the platform-specific Play Games implementation.</summary>
        /// <param name="implementation">The platform Play Games bridge to use.</param>
        public static void SetImplementation(IPlayGamesBridge implementation)
        {
            _impl = implementation;
        }

        // ── Sign-In ──

        /// <summary>Signs in to Play Games Services.</summary>
        /// <param name="silent">If true, attempts silent sign-in without UI prompts.</param>
        public static Task<SignInResult> SignInAsync(bool silent = true)
            => _impl?.SignInAsync(silent)
               ?? Task.FromResult(new SignInResult(false, "No platform implementation"));

        // ── Players ──

        /// <summary>Gets the currently signed-in player's information.</summary>
        public static Task<PlayerInfo> GetCurrentPlayerAsync()
            => _impl?.GetCurrentPlayerAsync()
               ?? Task.FromResult(new PlayerInfo(false, null, null, null, null, "No platform implementation"));

        // ── Snapshots ──

        /// <summary>Loads a saved game snapshot by name.</summary>
        /// <param name="snapshotName">The name of the snapshot to load.</param>
        public static Task<SnapshotLoadResult> LoadSnapshotAsync(string snapshotName)
            => _impl?.LoadSnapshotAsync(snapshotName)
               ?? Task.FromResult(new SnapshotLoadResult(false, null, "No platform implementation"));

        /// <summary>Saves data to a named snapshot.</summary>
        /// <param name="snapshotName">The name of the snapshot.</param>
        /// <param name="data">The data to save.</param>
        /// <param name="description">A human-readable description of the save.</param>
        public static Task<SnapshotSaveResult> SaveSnapshotAsync(string snapshotName, string data, string description)
            => _impl?.SaveSnapshotAsync(snapshotName, data, description)
               ?? Task.FromResult(new SnapshotSaveResult(false, "No platform implementation"));

        /// <summary>Deletes a saved game snapshot.</summary>
        /// <param name="snapshotName">The name of the snapshot to delete.</param>
        public static Task<SnapshotDeleteResult> DeleteSnapshotAsync(string snapshotName)
            => _impl?.DeleteSnapshotAsync(snapshotName)
               ?? Task.FromResult(new SnapshotDeleteResult(false, "No platform implementation"));

        // ── Leaderboards ──

        /// <summary>Submits a score to a leaderboard.</summary>
        /// <param name="leaderboardId">The leaderboard identifier.</param>
        /// <param name="score">The score value to submit.</param>
        public static Task<OperationResult> SubmitScoreAsync(string leaderboardId, long score)
            => _impl?.SubmitScoreAsync(leaderboardId, score)
               ?? Task.FromResult(new OperationResult(false, "No platform implementation"));

        /// <summary>Shows the UI for a specific leaderboard.</summary>
        /// <param name="leaderboardId">The leaderboard identifier.</param>
        public static void ShowLeaderboard(string leaderboardId)
            => _impl?.ShowLeaderboard(leaderboardId);

        /// <summary>Shows the UI for all leaderboards.</summary>
        public static void ShowAllLeaderboards()
            => _impl?.ShowAllLeaderboards();

        // ── Achievements ──

        /// <summary>Unlocks an achievement.</summary>
        /// <param name="achievementId">The achievement identifier.</param>
        public static Task<OperationResult> UnlockAchievementAsync(string achievementId)
            => _impl?.UnlockAchievementAsync(achievementId)
               ?? Task.FromResult(new OperationResult(false, "No platform implementation"));

        /// <summary>Increments a step-based achievement.</summary>
        /// <param name="achievementId">The achievement identifier.</param>
        /// <param name="steps">The number of steps to increment.</param>
        public static Task<OperationResult> IncrementAchievementAsync(string achievementId, int steps)
            => _impl?.IncrementAchievementAsync(achievementId, steps)
               ?? Task.FromResult(new OperationResult(false, "No platform implementation"));

        /// <summary>Reveals a hidden achievement.</summary>
        /// <param name="achievementId">The achievement identifier.</param>
        public static Task<OperationResult> RevealAchievementAsync(string achievementId)
            => _impl?.RevealAchievementAsync(achievementId)
               ?? Task.FromResult(new OperationResult(false, "No platform implementation"));

        /// <summary>Shows the achievements UI.</summary>
        public static void ShowAchievements()
            => _impl?.ShowAchievements();

        // ── Events ──

        /// <summary>Increments an event counter.</summary>
        /// <param name="eventId">The event identifier.</param>
        /// <param name="steps">The number of steps to increment.</param>
        public static void IncrementEvent(string eventId, int steps)
            => _impl?.IncrementEvent(eventId, steps);

        /// <summary>Loads all events and their current values.</summary>
        public static Task<EventsResult> LoadEventsAsync()
            => _impl?.LoadEventsAsync()
               ?? Task.FromResult(new EventsResult(false, null, "No platform implementation"));

        // ── Recall ──

        /// <summary>Requests a recall access token for cross-device identity linking.</summary>
        /// <param name="sessionId">The session identifier.</param>
        public static Task<RecallAccessResult> RequestRecallAccessAsync(string sessionId)
            => _impl?.RequestRecallAccessAsync(sessionId)
               ?? Task.FromResult(new RecallAccessResult(false, null, "No platform implementation"));

        // ── Player Stats ──

        /// <summary>Gets gameplay statistics for the current player.</summary>
        public static Task<PlayerStatsResult> GetPlayerStatsAsync()
            => _impl?.GetPlayerStatsAsync()
               ?? Task.FromResult(new PlayerStatsResult(false, 0, 0, 0, 0, 0, 0, 0, 0, 0, "No platform implementation"));
    }
}
