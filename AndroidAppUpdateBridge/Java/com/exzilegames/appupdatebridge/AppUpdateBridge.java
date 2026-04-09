package com.exzilegames.appupdatebridge;

import android.app.Activity;

import com.google.android.play.core.appupdate.AppUpdateInfo;
import com.google.android.play.core.appupdate.AppUpdateManager;
import com.google.android.play.core.appupdate.AppUpdateManagerFactory;
import com.google.android.play.core.appupdate.AppUpdateOptions;
import com.google.android.play.core.install.InstallStateUpdatedListener;
import com.google.android.play.core.install.model.AppUpdateType;
import com.google.android.play.core.install.model.InstallStatus;
import com.google.android.play.core.install.model.UpdateAvailability;

/**
 * Java bridge for the Google Play In-App Updates API.
 *
 * Holds strong references to listener objects to prevent the Android/Mono GC
 * from collecting them mid-update — the root cause of instability in the
 * Xamarin NuGet bindings when used directly from C#.
 */
public final class AppUpdateBridge {

    // ── Listener interfaces ──

    public interface UpdateInfoListener {
        void onUpdateInfo(int updateAvailability, int availableVersionCode, int staleDays,
                         boolean immediateAllowed, boolean flexibleAllowed);
        void onError(String message);
    }

    public interface UpdateResultListener {
        void onSuccess();
        void onCancelled();
        void onFailure(String message);
    }

    public interface FlexibleUpdateListener {
        void onDownloadProgress(long bytesDownloaded, long totalBytes);
        void onDownloaded();   // ready to call completeFlexibleUpdate()
        void onInstalled();
        void onFailed(String message);
    }

    // ── Fields ──

    private AppUpdateManager manager;

    /** Held as a field so the GC cannot collect it while a flexible update is in progress. */
    private InstallStateUpdatedListener flexibleListener;

    // ── Initialization ──

    public void initialize(Activity activity) {
        manager = AppUpdateManagerFactory.create(activity);
    }

    // ── Check for update ──

    public void checkForUpdate(UpdateInfoListener listener) {
        if (manager == null) {
            if (listener != null) listener.onError("AppUpdateManager not initialized");
            return;
        }

        manager.getAppUpdateInfo()
                .addOnSuccessListener(info -> {
                    if (listener == null) return;

                    int availability = info.updateAvailability();
                    int versionCode  = info.availableVersionCode();
                    Integer staleDays = info.clientVersionStalenessDays();
                    int stale = staleDays != null ? staleDays : 0;
                    boolean immediateAllowed = info.isUpdateTypeAllowed(AppUpdateType.IMMEDIATE);
                    boolean flexibleAllowed  = info.isUpdateTypeAllowed(AppUpdateType.FLEXIBLE);

                    listener.onUpdateInfo(availability, versionCode, stale,
                            immediateAllowed, flexibleAllowed);
                })
                .addOnFailureListener(e -> {
                    if (listener != null)
                        listener.onError(e.getMessage() != null ? e.getMessage() : "checkForUpdate failed");
                });
    }

    // ── Immediate update ──

    public void startImmediateUpdate(Activity activity, int requestCode, UpdateResultListener listener) {
        if (manager == null) {
            if (listener != null) listener.onFailure("AppUpdateManager not initialized");
            return;
        }

        manager.getAppUpdateInfo()
                .addOnSuccessListener(info -> {
                    if (info.updateAvailability() != UpdateAvailability.UPDATE_AVAILABLE
                            && info.updateAvailability() != UpdateAvailability.DEVELOPER_TRIGGERED_UPDATE_IN_PROGRESS) {
                        if (listener != null) listener.onCancelled();
                        return;
                    }

                    AppUpdateOptions options = AppUpdateOptions.newBuilder(AppUpdateType.IMMEDIATE).build();

                    try {
                        manager.startUpdateFlowForResult(info, activity, options, requestCode)
                                .addOnSuccessListener(result -> {
                                    if (listener != null) listener.onSuccess();
                                })
                                .addOnFailureListener(e -> {
                                    if (listener != null)
                                        listener.onFailure(e.getMessage() != null ? e.getMessage() : "startUpdateFlowForResult failed");
                                });
                    } catch (Exception e) {
                        if (listener != null)
                            listener.onFailure(e.getMessage() != null ? e.getMessage() : "startImmediateUpdate exception");
                    }
                })
                .addOnFailureListener(e -> {
                    if (listener != null)
                        listener.onFailure(e.getMessage() != null ? e.getMessage() : "getAppUpdateInfo failed");
                });
    }

    // ── Flexible update ──

    public void startFlexibleUpdate(Activity activity, int requestCode, FlexibleUpdateListener listener) {
        if (manager == null) {
            if (listener != null) listener.onFailed("AppUpdateManager not initialized");
            return;
        }

        // Unregister any previous listener before registering a new one.
        if (flexibleListener != null) {
            manager.unregisterListener(flexibleListener);
            flexibleListener = null;
        }

        // Assign to field — this strong reference prevents the GC from collecting
        // the listener while the download is running in the background.
        flexibleListener = state -> {
            if (listener == null) return;

            int status = state.installStatus();

            if (status == InstallStatus.DOWNLOADING) {
                listener.onDownloadProgress(state.bytesDownloaded(), state.totalBytesToDownload());
            } else if (status == InstallStatus.DOWNLOADED) {
                listener.onDownloaded();
            } else if (status == InstallStatus.INSTALLED) {
                listener.onInstalled();
            } else if (status == InstallStatus.FAILED) {
                listener.onFailed("Flexible update install failed (InstallStatus.FAILED)");
            }
        };

        manager.registerListener(flexibleListener);

        manager.getAppUpdateInfo()
                .addOnSuccessListener(info -> {
                    if (info.updateAvailability() != UpdateAvailability.UPDATE_AVAILABLE
                            && info.updateAvailability() != UpdateAvailability.DEVELOPER_TRIGGERED_UPDATE_IN_PROGRESS) {
                        if (listener != null) listener.onFailed("No update available for flexible flow");
                        return;
                    }

                    AppUpdateOptions options = AppUpdateOptions.newBuilder(AppUpdateType.FLEXIBLE).build();

                    try {
                        manager.startUpdateFlowForResult(info, activity, options, requestCode)
                                .addOnFailureListener(e -> {
                                    if (listener != null)
                                        listener.onFailed(e.getMessage() != null ? e.getMessage() : "startUpdateFlowForResult failed");
                                });
                    } catch (Exception e) {
                        if (listener != null)
                            listener.onFailed(e.getMessage() != null ? e.getMessage() : "startFlexibleUpdate exception");
                    }
                })
                .addOnFailureListener(e -> {
                    if (listener != null)
                        listener.onFailed(e.getMessage() != null ? e.getMessage() : "getAppUpdateInfo failed");
                });
    }

    // ── Complete flexible update ──

    /** Triggers the flexible update install. Call after {@link FlexibleUpdateListener#onDownloaded()}. */
    public void completeFlexibleUpdate() {
        if (manager != null) manager.completeUpdate();
    }

    // ── Cleanup ──

    /** Unregisters the flexible install-state listener and clears the field reference. */
    public void unregisterFlexibleListener() {
        if (manager != null && flexibleListener != null) {
            manager.unregisterListener(flexibleListener);
            flexibleListener = null;
        }
    }
}
