#if ANDROID
using Android.App;

namespace AndroidPlayGamesBridge.Interop
{
    /// <summary>
    /// Android implementation of <see cref="IPlayGamesBridge"/> that delegates
    /// to the compiled Java class <c>PlayGamesBridge</c> via auto-generated JNI bindings.
    /// </summary>
    public sealed class AndroidPlayGamesBridgeImpl : IPlayGamesBridge
    {
        private readonly Activity _activity;
        private readonly global::Com.Exzilegames.Playgamesbridge.PlayGamesBridge _bridge;

        /// <inheritdoc/>
        public bool IsAvailable => true;

        /// <summary>Creates a new Play Games bridge backed by the given Android activity.</summary>
        /// <param name="activity">The activity used to run Play Games operations on the UI thread.</param>
        public AndroidPlayGamesBridgeImpl(Activity activity)
        {
            _activity = activity;
            _bridge = new global::Com.Exzilegames.Playgamesbridge.PlayGamesBridge();
        }

        // ── Sign-In ──

        /// <inheritdoc/>
        public Task<SignInResult> SignInAsync(bool silent = true)
        {
            var tcs = new TaskCompletionSource<SignInResult>();
            _activity.RunOnUiThread(() =>
            {
                _bridge.SignIn(_activity, silent, new SignInListenerImpl(tcs));
            });
            return tcs.Task;
        }

        // ── Players ──

        /// <inheritdoc/>
        public Task<PlayerInfo> GetCurrentPlayerAsync()
        {
            var tcs = new TaskCompletionSource<PlayerInfo>();
            _activity.RunOnUiThread(() =>
            {
                _bridge.GetCurrentPlayer(_activity, new PlayerInfoListenerImpl(tcs));
            });
            return tcs.Task;
        }

        // ── Snapshots ──

        /// <inheritdoc/>
        public Task<SnapshotLoadResult> LoadSnapshotAsync(string snapshotName)
        {
            var tcs = new TaskCompletionSource<SnapshotLoadResult>();
            _activity.RunOnUiThread(() =>
            {
                _bridge.LoadSnapshot(_activity, snapshotName, new SnapshotLoadListenerImpl(tcs));
            });
            return tcs.Task;
        }

        /// <inheritdoc/>
        public Task<SnapshotSaveResult> SaveSnapshotAsync(string snapshotName, string data, string description)
        {
            var tcs = new TaskCompletionSource<SnapshotSaveResult>();
            _activity.RunOnUiThread(() =>
            {
                _bridge.SaveSnapshot(_activity, snapshotName, data, description, new SnapshotSaveListenerImpl(tcs));
            });
            return tcs.Task;
        }

        /// <inheritdoc/>
        public Task<SnapshotDeleteResult> DeleteSnapshotAsync(string snapshotName)
        {
            var tcs = new TaskCompletionSource<SnapshotDeleteResult>();
            _activity.RunOnUiThread(() =>
            {
                _bridge.DeleteSnapshot(_activity, snapshotName, new SnapshotDeleteListenerImpl(tcs));
            });
            return tcs.Task;
        }

        // ── Leaderboards ──

        /// <inheritdoc/>
        public Task<OperationResult> SubmitScoreAsync(string leaderboardId, long score)
        {
            var tcs = new TaskCompletionSource<OperationResult>();
            _activity.RunOnUiThread(() =>
            {
                _bridge.SubmitScore(_activity, leaderboardId, score, new LeaderboardSubmitListenerImpl(tcs));
            });
            return tcs.Task;
        }

        /// <inheritdoc/>
        public void ShowLeaderboard(string leaderboardId)
        {
            _activity.RunOnUiThread(() => _bridge.ShowLeaderboard(_activity, leaderboardId));
        }

        /// <inheritdoc/>
        public void ShowAllLeaderboards()
        {
            _activity.RunOnUiThread(() => _bridge.ShowAllLeaderboards(_activity));
        }

        // ── Achievements ──

        /// <inheritdoc/>
        public Task<OperationResult> UnlockAchievementAsync(string achievementId)
        {
            var tcs = new TaskCompletionSource<OperationResult>();
            _activity.RunOnUiThread(() =>
            {
                _bridge.UnlockAchievement(_activity, achievementId, new AchievementListenerImpl(tcs));
            });
            return tcs.Task;
        }

        /// <inheritdoc/>
        public Task<OperationResult> IncrementAchievementAsync(string achievementId, int steps)
        {
            var tcs = new TaskCompletionSource<OperationResult>();
            _activity.RunOnUiThread(() =>
            {
                _bridge.IncrementAchievement(_activity, achievementId, steps, new AchievementListenerImpl(tcs));
            });
            return tcs.Task;
        }

        /// <inheritdoc/>
        public Task<OperationResult> RevealAchievementAsync(string achievementId)
        {
            var tcs = new TaskCompletionSource<OperationResult>();
            _activity.RunOnUiThread(() =>
            {
                _bridge.RevealAchievement(_activity, achievementId, new AchievementListenerImpl(tcs));
            });
            return tcs.Task;
        }

        /// <inheritdoc/>
        public void ShowAchievements()
        {
            _activity.RunOnUiThread(() => _bridge.ShowAchievements(_activity));
        }

        // ── Events ──

        /// <inheritdoc/>
        public void IncrementEvent(string eventId, int steps)
        {
            _activity.RunOnUiThread(() => _bridge.IncrementEvent(_activity, eventId, steps));
        }

        /// <inheritdoc/>
        public Task<EventsResult> LoadEventsAsync()
        {
            var tcs = new TaskCompletionSource<EventsResult>();
            _activity.RunOnUiThread(() =>
            {
                _bridge.LoadEvents(_activity, new EventsListenerImpl(tcs));
            });
            return tcs.Task;
        }

        // ── Recall (not available in Xamarin binding) ──

        /// <inheritdoc/>
        public Task<RecallAccessResult> RequestRecallAccessAsync(string sessionId)
            => Task.FromResult(new RecallAccessResult(false, null, "RecallClient API is not available in the Xamarin binding"));

        // ── Player Stats ──

        /// <inheritdoc/>
        public Task<PlayerStatsResult> GetPlayerStatsAsync()
        {
            var tcs = new TaskCompletionSource<PlayerStatsResult>();
            _activity.RunOnUiThread(() =>
            {
                _bridge.GetPlayerStats(_activity, new PlayerStatsListenerImpl(tcs));
            });
            return tcs.Task;
        }

        // ── Listener implementations ──

        private sealed class SignInListenerImpl : Java.Lang.Object,
            global::Com.Exzilegames.Playgamesbridge.PlayGamesBridge.ISignInListener
        {
            private readonly TaskCompletionSource<SignInResult> _tcs;
            public SignInListenerImpl(TaskCompletionSource<SignInResult> tcs) => _tcs = tcs;
            public void OnSignInResult(bool success, string? message)
                => _tcs.TrySetResult(new SignInResult(success, message));
        }

        private sealed class PlayerInfoListenerImpl : Java.Lang.Object,
            global::Com.Exzilegames.Playgamesbridge.PlayGamesBridge.IPlayerInfoListener
        {
            private readonly TaskCompletionSource<PlayerInfo> _tcs;
            public PlayerInfoListenerImpl(TaskCompletionSource<PlayerInfo> tcs) => _tcs = tcs;
            public void OnPlayerInfo(bool success, string? playerId, string? displayName,
                string? hiResImageUri, string? iconImageUri, string? message)
                => _tcs.TrySetResult(new PlayerInfo(success, playerId, displayName, hiResImageUri, iconImageUri, message));
        }

        private sealed class SnapshotLoadListenerImpl : Java.Lang.Object,
            global::Com.Exzilegames.Playgamesbridge.PlayGamesBridge.ISnapshotLoadListener
        {
            private readonly TaskCompletionSource<SnapshotLoadResult> _tcs;
            public SnapshotLoadListenerImpl(TaskCompletionSource<SnapshotLoadResult> tcs) => _tcs = tcs;
            public void OnSnapshotLoaded(bool success, string? data, string? message)
                => _tcs.TrySetResult(new SnapshotLoadResult(success, data, message));
        }

        private sealed class SnapshotSaveListenerImpl : Java.Lang.Object,
            global::Com.Exzilegames.Playgamesbridge.PlayGamesBridge.ISnapshotSaveListener
        {
            private readonly TaskCompletionSource<SnapshotSaveResult> _tcs;
            public SnapshotSaveListenerImpl(TaskCompletionSource<SnapshotSaveResult> tcs) => _tcs = tcs;
            public void OnSnapshotSaved(bool success, string? message)
                => _tcs.TrySetResult(new SnapshotSaveResult(success, message));
        }

        private sealed class SnapshotDeleteListenerImpl : Java.Lang.Object,
            global::Com.Exzilegames.Playgamesbridge.PlayGamesBridge.ISnapshotDeleteListener
        {
            private readonly TaskCompletionSource<SnapshotDeleteResult> _tcs;
            public SnapshotDeleteListenerImpl(TaskCompletionSource<SnapshotDeleteResult> tcs) => _tcs = tcs;
            public void OnSnapshotDeleted(bool success, string? message)
                => _tcs.TrySetResult(new SnapshotDeleteResult(success, message));
        }

        private sealed class LeaderboardSubmitListenerImpl : Java.Lang.Object,
            global::Com.Exzilegames.Playgamesbridge.PlayGamesBridge.ILeaderboardSubmitListener
        {
            private readonly TaskCompletionSource<OperationResult> _tcs;
            public LeaderboardSubmitListenerImpl(TaskCompletionSource<OperationResult> tcs) => _tcs = tcs;
            public void OnScoreSubmitted(bool success, string? message)
                => _tcs.TrySetResult(new OperationResult(success, message));
        }

        private sealed class AchievementListenerImpl : Java.Lang.Object,
            global::Com.Exzilegames.Playgamesbridge.PlayGamesBridge.IAchievementListener
        {
            private readonly TaskCompletionSource<OperationResult> _tcs;
            public AchievementListenerImpl(TaskCompletionSource<OperationResult> tcs) => _tcs = tcs;
            public void OnAchievementResult(bool success, string? message)
                => _tcs.TrySetResult(new OperationResult(success, message));
        }

        private sealed class EventsListenerImpl : Java.Lang.Object,
            global::Com.Exzilegames.Playgamesbridge.PlayGamesBridge.IEventsListener
        {
            private readonly TaskCompletionSource<EventsResult> _tcs;
            public EventsListenerImpl(TaskCompletionSource<EventsResult> tcs) => _tcs = tcs;
            public void OnEventsLoaded(bool success, string? eventsJson, string? message)
            {
                EventEntry[]? entries = null;
                if (success && !string.IsNullOrEmpty(eventsJson))
                {
                    try
                    {
                        entries = System.Text.Json.JsonSerializer.Deserialize<EventEntry[]>(eventsJson);
                    }
                    catch { entries = []; }
                }
                _tcs.TrySetResult(new EventsResult(success, entries, message));
            }
        }

        private sealed class PlayerStatsListenerImpl : Java.Lang.Object,
            global::Com.Exzilegames.Playgamesbridge.PlayGamesBridge.IPlayerStatsListener
        {
            private readonly TaskCompletionSource<PlayerStatsResult> _tcs;
            public PlayerStatsListenerImpl(TaskCompletionSource<PlayerStatsResult> tcs) => _tcs = tcs;
            public void OnPlayerStats(bool success, float averageSessionLength,
                float churnProbability, int daysSinceLastPlayed, int numberOfPurchases,
                int numberOfSessions, float sessionPercentile, float spendPercentile,
                float spendProbability, float totalSpendNext28Days, string? message)
                => _tcs.TrySetResult(new PlayerStatsResult(success, averageSessionLength,
                    churnProbability, daysSinceLastPlayed, numberOfPurchases, numberOfSessions,
                    sessionPercentile, spendPercentile, spendProbability, totalSpendNext28Days, message));
        }
    }
}
#endif
