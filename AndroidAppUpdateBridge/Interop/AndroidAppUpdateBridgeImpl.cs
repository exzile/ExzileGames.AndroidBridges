#if ANDROID
using Android.App;

namespace AndroidAppUpdateBridge.Interop
{
    /// <summary>
    /// Android implementation of <see cref="IAppUpdateBridge"/> that delegates to the
    /// compiled Java class <c>AppUpdateBridge</c> via auto-generated JNI bindings.
    /// </summary>
    public sealed class AndroidAppUpdateBridgeImpl : Java.Lang.Object, IAppUpdateBridge
    {
        private const int ImmediateRequestCode = 1001;
        private const int FlexibleRequestCode  = 1002;

        private readonly Activity _activity;
        private readonly global::Com.Exzilegames.Appupdatebridge.AppUpdateBridge _bridge;

        /// <summary>
        /// Creates a new app update bridge backed by the given Android activity.
        /// </summary>
        /// <param name="activity">The activity used to launch update flows on the UI thread.</param>
        public AndroidAppUpdateBridgeImpl(Activity activity)
        {
            _activity = activity;
            _bridge = new global::Com.Exzilegames.Appupdatebridge.AppUpdateBridge();
            _bridge.Initialize(activity);
        }

        /// <inheritdoc/>
        public bool IsAvailable => true;

        /// <inheritdoc/>
        public Task<AppUpdateInfo> CheckForUpdateAsync()
        {
            var tcs = new TaskCompletionSource<AppUpdateInfo>(TaskCreationOptions.RunContinuationsAsynchronously);

            _activity.RunOnUiThread(() =>
            {
                try
                {
                    _bridge.CheckForUpdate(new UpdateInfoListenerImpl(
                        onUpdateInfo: (availability, versionCode, staleDays, immediateAllowed, flexibleAllowed) =>
                        {
                            var avail = availability switch
                            {
                                1 => UpdateAvailability.UpdateAvailable,           // UpdateAvailability.UPDATE_AVAILABLE
                                2 => UpdateAvailability.InProgress,                // DEVELOPER_TRIGGERED_UPDATE_IN_PROGRESS
                                _ => UpdateAvailability.UpdateNotAvailable,
                            };
                            tcs.TrySetResult(new AppUpdateInfo(avail, versionCode, staleDays,
                                immediateAllowed, flexibleAllowed, null));
                        },
                        onError: msg => tcs.TrySetResult(
                            new AppUpdateInfo(UpdateAvailability.Unknown, 0, 0, false, false, msg))));
                }
                catch (Exception ex)
                {
                    tcs.TrySetResult(new AppUpdateInfo(UpdateAvailability.Unknown, 0, 0, false, false, ex.Message));
                }
            });

            return tcs.Task;
        }

        /// <inheritdoc/>
        public Task<AppUpdateResult> StartImmediateUpdateAsync()
        {
            var tcs = new TaskCompletionSource<AppUpdateResult>(TaskCreationOptions.RunContinuationsAsynchronously);

            _activity.RunOnUiThread(() =>
            {
                try
                {
                    _bridge.StartImmediateUpdate(_activity, ImmediateRequestCode, new UpdateResultListenerImpl(
                        onSuccess:   ()    => tcs.TrySetResult(new AppUpdateResult(true,  false, null)),
                        onCancelled: ()    => tcs.TrySetResult(new AppUpdateResult(false, true,  null)),
                        onFailure:   msg   => tcs.TrySetResult(new AppUpdateResult(false, false, msg))));
                }
                catch (Exception ex)
                {
                    tcs.TrySetResult(new AppUpdateResult(false, false, ex.Message));
                }
            });

            return tcs.Task;
        }

        /// <inheritdoc/>
        public Task<AppUpdateResult> StartFlexibleUpdateAsync(IProgress<FlexibleUpdateProgress>? progress = null)
        {
            var tcs = new TaskCompletionSource<AppUpdateResult>(TaskCreationOptions.RunContinuationsAsynchronously);

            _activity.RunOnUiThread(() =>
            {
                try
                {
                    _bridge.StartFlexibleUpdate(_activity, FlexibleRequestCode, new FlexibleUpdateListenerImpl(
                        onDownloadProgress: (bytesDownloaded, totalBytes) =>
                            progress?.Report(new FlexibleUpdateProgress(bytesDownloaded, totalBytes, false)),
                        onDownloaded: () =>
                            progress?.Report(new FlexibleUpdateProgress(0, 0, true)),
                        onInstalled: () =>
                            tcs.TrySetResult(new AppUpdateResult(true,  false, null)),
                        onFailed: msg =>
                            tcs.TrySetResult(new AppUpdateResult(false, false, msg))));
                }
                catch (Exception ex)
                {
                    tcs.TrySetResult(new AppUpdateResult(false, false, ex.Message));
                }
            });

            return tcs.Task;
        }

        /// <inheritdoc/>
        public void CompleteFlexibleUpdate() => _bridge.CompleteFlexibleUpdate();

        // ── Listener implementations ──

        private sealed class UpdateInfoListenerImpl : Java.Lang.Object,
            global::Com.Exzilegames.Appupdatebridge.AppUpdateBridge.IUpdateInfoListener
        {
            private readonly Action<int, int, int, bool, bool> _onUpdateInfo;
            private readonly Action<string?> _onError;

            public UpdateInfoListenerImpl(
                Action<int, int, int, bool, bool> onUpdateInfo,
                Action<string?> onError)
            {
                _onUpdateInfo = onUpdateInfo;
                _onError = onError;
            }

            public void OnUpdateInfo(int updateAvailability, int availableVersionCode, int staleDays,
                                     bool immediateAllowed, bool flexibleAllowed)
                => _onUpdateInfo(updateAvailability, availableVersionCode, staleDays,
                                 immediateAllowed, flexibleAllowed);

            public void OnError(string? message) => _onError(message);
        }

        private sealed class UpdateResultListenerImpl : Java.Lang.Object,
            global::Com.Exzilegames.Appupdatebridge.AppUpdateBridge.IUpdateResultListener
        {
            private readonly Action _onSuccess;
            private readonly Action _onCancelled;
            private readonly Action<string?> _onFailure;

            public UpdateResultListenerImpl(Action onSuccess, Action onCancelled, Action<string?> onFailure)
            {
                _onSuccess   = onSuccess;
                _onCancelled = onCancelled;
                _onFailure   = onFailure;
            }

            public void OnSuccess()                    => _onSuccess();
            public void OnCancelled()                  => _onCancelled();
            public void OnFailure(string? message)     => _onFailure(message);
        }

        private sealed class FlexibleUpdateListenerImpl : Java.Lang.Object,
            global::Com.Exzilegames.Appupdatebridge.AppUpdateBridge.IFlexibleUpdateListener
        {
            private readonly Action<long, long> _onDownloadProgress;
            private readonly Action _onDownloaded;
            private readonly Action _onInstalled;
            private readonly Action<string?> _onFailed;

            public FlexibleUpdateListenerImpl(
                Action<long, long> onDownloadProgress,
                Action onDownloaded,
                Action onInstalled,
                Action<string?> onFailed)
            {
                _onDownloadProgress = onDownloadProgress;
                _onDownloaded       = onDownloaded;
                _onInstalled        = onInstalled;
                _onFailed           = onFailed;
            }

            public void OnDownloadProgress(long bytesDownloaded, long totalBytes)
                => _onDownloadProgress(bytesDownloaded, totalBytes);

            public void OnDownloaded()              => _onDownloaded();
            public void OnInstalled()               => _onInstalled();
            public void OnFailed(string? message)   => _onFailed(message);
        }
    }
}
#endif
