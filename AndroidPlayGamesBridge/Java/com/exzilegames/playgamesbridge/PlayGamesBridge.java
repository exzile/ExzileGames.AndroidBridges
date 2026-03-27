package com.exzilegames.playgamesbridge;

import android.app.Activity;
import android.content.Intent;
import android.graphics.Bitmap;
import android.net.Uri;
import android.util.Log;

import com.google.android.gms.games.PlayGames;
import com.google.android.gms.games.GamesSignInClient;
import com.google.android.gms.games.PlayersClient;
import com.google.android.gms.games.Player;
import com.google.android.gms.games.SnapshotsClient;
import com.google.android.gms.games.snapshot.Snapshot;
import com.google.android.gms.games.snapshot.SnapshotMetadata;
import com.google.android.gms.games.snapshot.SnapshotMetadataChange;
import com.google.android.gms.games.snapshot.SnapshotContents;
import com.google.android.gms.games.LeaderboardsClient;
import com.google.android.gms.games.AchievementsClient;
import com.google.android.gms.games.EventsClient;
import com.google.android.gms.games.event.EventBuffer;
import com.google.android.gms.games.PlayerStatsClient;
import com.google.android.gms.games.stats.PlayerStats;
import com.google.android.gms.tasks.OnCompleteListener;
import com.google.android.gms.tasks.OnSuccessListener;
import com.google.android.gms.tasks.Task;

import java.io.IOException;
import java.nio.charset.StandardCharsets;

/**
 * Java bridge exposing Google Play Games Services v2 APIs that are missing
 * from the Xamarin.GooglePlayServices.Games.V2 NuGet bindings.
 *
 * Usage from C#: instantiate via JNI auto-binding and call methods,
 * passing the Activity reference for API context.
 *
 * GitHub: https://github.com/exzile/ExzileGames.AndroidBridges
 */
@SuppressWarnings("unchecked")
public final class PlayGamesBridge {

    private static final String TAG = "PlayGamesBridge";

    // ── Listener interfaces for async callbacks ──

    public interface SignInListener {
        void onSignInResult(boolean success, String message);
    }

    public interface PlayerInfoListener {
        void onPlayerInfo(boolean success, String playerId, String displayName,
                          String hiResImageUri, String iconImageUri, String message);
    }

    public interface SnapshotLoadListener {
        void onSnapshotLoaded(boolean success, String data, String message);
    }

    public interface SnapshotSaveListener {
        void onSnapshotSaved(boolean success, String message);
    }

    public interface SnapshotDeleteListener {
        void onSnapshotDeleted(boolean success, String message);
    }

    public interface LeaderboardSubmitListener {
        void onScoreSubmitted(boolean success, String message);
    }

    public interface AchievementListener {
        void onAchievementResult(boolean success, String message);
    }

    public interface EventsListener {
        void onEventsLoaded(boolean success, String eventsJson, String message);
    }

    public interface PlayerStatsListener {
        void onPlayerStats(boolean success, float averageSessionLength,
                           float churnProbability, int daysSinceLastPlayed,
                           int numberOfPurchases, int numberOfSessions,
                           float sessionPercentile, float spendPercentile,
                           float spendProbability, float totalSpendNext28Days,
                           String message);
    }

    // ── Sign-In ──

    public void signIn(Activity activity, boolean silent, SignInListener listener) {
        try {
            GamesSignInClient client = PlayGames.getGamesSignInClient(activity);
            if (silent) {
                client.isAuthenticated().addOnCompleteListener(new OnCompleteListener() {
                    @Override
                    public void onComplete(Task task) {
                        boolean authenticated = task.isSuccessful()
                                && ((com.google.android.gms.games.AuthenticationResult) task.getResult()).isAuthenticated();
                        listener.onSignInResult(authenticated,
                                authenticated ? "Authenticated" : "Not authenticated");
                    }
                });
            } else {
                client.signIn().addOnCompleteListener(new OnCompleteListener() {
                    @Override
                    public void onComplete(Task task) {
                        boolean success = task.isSuccessful();
                        listener.onSignInResult(success,
                                success ? "Signed in" : "Sign-in failed");
                    }
                });
            }
        } catch (Exception e) {
            Log.e(TAG, "signIn error", e);
            listener.onSignInResult(false, e.getMessage());
        }
    }

    public boolean isAuthenticated(Activity activity) {
        try {
            // This is a synchronous check of cached state
            return PlayGames.getGamesSignInClient(activity)
                    .isAuthenticated()
                    .isSuccessful();
        } catch (Exception e) {
            return false;
        }
    }

    // ── Players ──

    public void getCurrentPlayer(Activity activity, PlayerInfoListener listener) {
        try {
            PlayersClient client = PlayGames.getPlayersClient(activity);
            client.getCurrentPlayer().addOnCompleteListener(new OnCompleteListener() {
                @Override
                public void onComplete(Task task) {
                    if (task.isSuccessful()) {
                        Player p = (Player) task.getResult();
                        String hiRes = p.getHiResImageUri() != null
                                ? p.getHiResImageUri().toString() : "";
                        String icon = p.getIconImageUri() != null
                                ? p.getIconImageUri().toString() : "";
                        listener.onPlayerInfo(true, p.getPlayerId(),
                                p.getDisplayName(), hiRes, icon, "OK");
                    } else {
                        String msg = task.getException() != null
                                ? task.getException().getMessage() : "Unknown error";
                        listener.onPlayerInfo(false, "", "", "", "", msg);
                    }
                }
            });
        } catch (Exception e) {
            Log.e(TAG, "getCurrentPlayer error", e);
            listener.onPlayerInfo(false, "", "", "", "", e.getMessage());
        }
    }

    // ── Snapshots (Saved Games) ──

    public void loadSnapshot(Activity activity, String snapshotName,
                             SnapshotLoadListener listener) {
        try {
            SnapshotsClient client = PlayGames.getSnapshotsClient(activity);
            client.open(snapshotName, true,
                    SnapshotsClient.RESOLUTION_POLICY_MOST_RECENTLY_MODIFIED)
                    .addOnCompleteListener(new OnCompleteListener() {
                        @Override
                        public void onComplete(Task task) {
                            if (task.isSuccessful()) {
                                SnapshotsClient.DataOrConflict<Snapshot> result =
                                        (SnapshotsClient.DataOrConflict<Snapshot>) task.getResult();
                                Snapshot snapshot = result.getData();
                                if (snapshot != null) {
                                    try {
                                        byte[] bytes = snapshot.getSnapshotContents()
                                                .readFully();
                                        String data = new String(bytes,
                                                StandardCharsets.UTF_8);
                                        listener.onSnapshotLoaded(true, data, "OK");
                                    } catch (IOException e) {
                                        listener.onSnapshotLoaded(false, "",
                                                "Read error: " + e.getMessage());
                                    }
                                } else {
                                    // Conflict — resolve with most recent
                                    listener.onSnapshotLoaded(false, "",
                                            "Snapshot conflict, resolved automatically");
                                }
                            } else {
                                String msg = task.getException() != null
                                        ? task.getException().getMessage()
                                        : "Failed to open snapshot";
                                listener.onSnapshotLoaded(false, "", msg);
                            }
                        }
                    });
        } catch (Exception e) {
            Log.e(TAG, "loadSnapshot error", e);
            listener.onSnapshotLoaded(false, "", e.getMessage());
        }
    }

    public void saveSnapshot(Activity activity, String snapshotName,
                             String data, String description,
                             SnapshotSaveListener listener) {
        try {
            final SnapshotsClient client = PlayGames.getSnapshotsClient(activity);
            client.open(snapshotName, true,
                    SnapshotsClient.RESOLUTION_POLICY_MOST_RECENTLY_MODIFIED)
                    .addOnCompleteListener(new OnCompleteListener() {
                        @Override
                        public void onComplete(Task openTask) {
                            if (openTask.isSuccessful()) {
                                SnapshotsClient.DataOrConflict<Snapshot> openResult =
                                        (SnapshotsClient.DataOrConflict<Snapshot>) openTask.getResult();
                                Snapshot snapshot = openResult.getData();
                                if (snapshot != null) {
                                    snapshot.getSnapshotContents().writeBytes(
                                            data.getBytes(StandardCharsets.UTF_8));
                                    SnapshotMetadataChange change =
                                            new SnapshotMetadataChange.Builder()
                                                    .setDescription(description)
                                                    .build();
                                    client.commitAndClose(snapshot, change)
                                            .addOnCompleteListener(new OnCompleteListener() {
                                                @Override
                                                public void onComplete(Task commitTask) {
                                                    if (commitTask.isSuccessful()) {
                                                        listener.onSnapshotSaved(true, "OK");
                                                    } else {
                                                        String msg = commitTask.getException() != null
                                                                ? commitTask.getException().getMessage()
                                                                : "Commit failed";
                                                        listener.onSnapshotSaved(false, msg);
                                                    }
                                                }
                                            });
                                } else {
                                    listener.onSnapshotSaved(false,
                                            "Snapshot conflict during save");
                                }
                            } else {
                                String msg = openTask.getException() != null
                                        ? openTask.getException().getMessage()
                                        : "Failed to open snapshot for save";
                                listener.onSnapshotSaved(false, msg);
                            }
                        }
                    });
        } catch (Exception e) {
            Log.e(TAG, "saveSnapshot error", e);
            listener.onSnapshotSaved(false, e.getMessage());
        }
    }

    public void deleteSnapshot(Activity activity, String snapshotName,
                               SnapshotDeleteListener listener) {
        try {
            final SnapshotsClient client = PlayGames.getSnapshotsClient(activity);
            client.open(snapshotName, false,
                    SnapshotsClient.RESOLUTION_POLICY_MOST_RECENTLY_MODIFIED)
                    .addOnCompleteListener(new OnCompleteListener() {
                        @Override
                        public void onComplete(Task task) {
                            if (task.isSuccessful()) {
                                SnapshotsClient.DataOrConflict<Snapshot> result =
                                        (SnapshotsClient.DataOrConflict<Snapshot>) task.getResult();
                                Snapshot snapshot = result.getData();
                                if (snapshot != null) {
                                    String id = snapshot.getMetadata().getSnapshotId();
                                    client.delete(snapshot.getMetadata())
                                            .addOnCompleteListener(new OnCompleteListener() {
                                                @Override
                                                public void onComplete(Task delTask) {
                                                    listener.onSnapshotDeleted(
                                                            delTask.isSuccessful(),
                                                            delTask.isSuccessful() ? "Deleted"
                                                                    : "Delete failed");
                                                }
                                            });
                                } else {
                                    listener.onSnapshotDeleted(false,
                                            "Snapshot not found");
                                }
                            } else {
                                listener.onSnapshotDeleted(false,
                                        "Failed to open snapshot for delete");
                            }
                        }
                    });
        } catch (Exception e) {
            Log.e(TAG, "deleteSnapshot error", e);
            listener.onSnapshotDeleted(false, e.getMessage());
        }
    }

    // ── Leaderboards ──

    public void submitScore(Activity activity, String leaderboardId,
                            long score, LeaderboardSubmitListener listener) {
        try {
            LeaderboardsClient client = PlayGames.getLeaderboardsClient(activity);
            client.submitScoreImmediate(leaderboardId, score)
                    .addOnCompleteListener(new OnCompleteListener() {
                        @Override
                        public void onComplete(Task task) {
                            listener.onScoreSubmitted(task.isSuccessful(),
                                    task.isSuccessful() ? "OK"
                                            : "Submit failed");
                        }
                    });
        } catch (Exception e) {
            Log.e(TAG, "submitScore error", e);
            listener.onScoreSubmitted(false, e.getMessage());
        }
    }

    public void showLeaderboard(Activity activity, String leaderboardId) {
        try {
            LeaderboardsClient client = PlayGames.getLeaderboardsClient(activity);
            client.getLeaderboardIntent(leaderboardId)
                    .addOnSuccessListener(new OnSuccessListener<Intent>() {
                        @Override
                        public void onSuccess(Intent intent) {
                            activity.startActivityForResult(intent, 9100);
                        }
                    });
        } catch (Exception e) {
            Log.e(TAG, "showLeaderboard error", e);
        }
    }

    public void showAllLeaderboards(Activity activity) {
        try {
            LeaderboardsClient client = PlayGames.getLeaderboardsClient(activity);
            client.getAllLeaderboardsIntent()
                    .addOnSuccessListener(new OnSuccessListener<Intent>() {
                        @Override
                        public void onSuccess(Intent intent) {
                            activity.startActivityForResult(intent, 9100);
                        }
                    });
        } catch (Exception e) {
            Log.e(TAG, "showAllLeaderboards error", e);
        }
    }

    // ── Achievements ──

    public void unlockAchievement(Activity activity, String achievementId,
                                  AchievementListener listener) {
        try {
            AchievementsClient client = PlayGames.getAchievementsClient(activity);
            client.unlockImmediate(achievementId)
                    .addOnCompleteListener(new OnCompleteListener() {
                        @Override
                        public void onComplete(Task task) {
                            listener.onAchievementResult(task.isSuccessful(),
                                    task.isSuccessful() ? "Unlocked" : "Unlock failed");
                        }
                    });
        } catch (Exception e) {
            Log.e(TAG, "unlockAchievement error", e);
            listener.onAchievementResult(false, e.getMessage());
        }
    }

    public void incrementAchievement(Activity activity, String achievementId,
                                     int steps, AchievementListener listener) {
        try {
            AchievementsClient client = PlayGames.getAchievementsClient(activity);
            client.incrementImmediate(achievementId, steps)
                    .addOnCompleteListener(new OnCompleteListener() {
                        @Override
                        public void onComplete(Task task) {
                            listener.onAchievementResult(task.isSuccessful(),
                                    task.isSuccessful() ? "Incremented"
                                            : "Increment failed");
                        }
                    });
        } catch (Exception e) {
            Log.e(TAG, "incrementAchievement error", e);
            listener.onAchievementResult(false, e.getMessage());
        }
    }

    public void revealAchievement(Activity activity, String achievementId,
                                  AchievementListener listener) {
        try {
            AchievementsClient client = PlayGames.getAchievementsClient(activity);
            client.revealImmediate(achievementId)
                    .addOnCompleteListener(new OnCompleteListener() {
                        @Override
                        public void onComplete(Task task) {
                            listener.onAchievementResult(task.isSuccessful(),
                                    task.isSuccessful() ? "Revealed" : "Reveal failed");
                        }
                    });
        } catch (Exception e) {
            Log.e(TAG, "revealAchievement error", e);
            listener.onAchievementResult(false, e.getMessage());
        }
    }

    public void showAchievements(Activity activity) {
        try {
            AchievementsClient client = PlayGames.getAchievementsClient(activity);
            client.getAchievementsIntent()
                    .addOnSuccessListener(new OnSuccessListener<Intent>() {
                        @Override
                        public void onSuccess(Intent intent) {
                            activity.startActivityForResult(intent, 9101);
                        }
                    });
        } catch (Exception e) {
            Log.e(TAG, "showAchievements error", e);
        }
    }

    // ── Events ──

    public void incrementEvent(Activity activity, String eventId, int steps) {
        try {
            EventsClient client = PlayGames.getEventsClient(activity);
            client.increment(eventId, steps);
        } catch (Exception e) {
            Log.e(TAG, "incrementEvent error", e);
        }
    }

    public void loadEvents(Activity activity, EventsListener listener) {
        try {
            EventsClient client = PlayGames.getEventsClient(activity);
            client.load(true).addOnCompleteListener(new OnCompleteListener() {
                @Override
                public void onComplete(Task task) {
                    if (task.isSuccessful()) {
                        EventBuffer buffer = ((com.google.android.gms.games.AnnotatedData<EventBuffer>) task.getResult()).get();
                        StringBuilder sb = new StringBuilder("[");
                        if (buffer != null) {
                            for (int i = 0; i < buffer.getCount(); i++) {
                                if (i > 0) sb.append(",");
                                com.google.android.gms.games.event.Event ev = buffer.get(i);
                                sb.append("{\"id\":\"").append(ev.getEventId())
                                  .append("\",\"name\":\"").append(escapeJson(ev.getName()))
                                  .append("\",\"value\":").append(ev.getValue())
                                  .append("}");
                            }
                            buffer.release();
                        }
                        sb.append("]");
                        listener.onEventsLoaded(true, sb.toString(), "OK");
                    } else {
                        listener.onEventsLoaded(false, "[]", "Failed to load events");
                    }
                }
            });
        } catch (Exception e) {
            Log.e(TAG, "loadEvents error", e);
            listener.onEventsLoaded(false, "[]", e.getMessage());
        }
    }

    // ── Player Stats ──

    public void getPlayerStats(Activity activity, PlayerStatsListener listener) {
        try {
            PlayerStatsClient client = PlayGames.getPlayerStatsClient(activity);
            client.loadPlayerStats(true).addOnCompleteListener(new OnCompleteListener() {
                @Override
                public void onComplete(Task task) {
                    if (task.isSuccessful()) {
                        PlayerStats stats = ((com.google.android.gms.games.AnnotatedData<PlayerStats>) task.getResult()).get();
                        if (stats != null) {
                            listener.onPlayerStats(true,
                                    stats.getAverageSessionLength(),
                                    stats.getChurnProbability(),
                                    stats.getDaysSinceLastPlayed(),
                                    stats.getNumberOfPurchases(),
                                    stats.getNumberOfSessions(),
                                    stats.getSessionPercentile(),
                                    stats.getSpendPercentile(),
                                    stats.getSpendProbability(),
                                    stats.getTotalSpendNext28Days(),
                                    "OK");
                        } else {
                            listener.onPlayerStats(false, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                                    "No stats available");
                        }
                    } else {
                        listener.onPlayerStats(false, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                                "Failed to load stats");
                    }
                }
            });
        } catch (Exception e) {
            Log.e(TAG, "getPlayerStats error", e);
            listener.onPlayerStats(false, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                    e.getMessage());
        }
    }

    // ── Utility ──

    private static String escapeJson(String s) {
        if (s == null) return "";
        return s.replace("\\", "\\\\").replace("\"", "\\\"")
                .replace("\n", "\\n").replace("\r", "\\r");
    }
}
