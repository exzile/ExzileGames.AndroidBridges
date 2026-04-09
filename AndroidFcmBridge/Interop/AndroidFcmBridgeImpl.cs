#if ANDROID
using Android.App;
using System.Text.Json;

namespace AndroidFcmBridge.Interop
{
    /// <summary>
    /// Android implementation of <see cref="IFcmBridge"/> that delegates to the compiled
    /// Java class <c>FcmBridge</c> via auto-generated JNI bindings.
    /// </summary>
    public sealed class AndroidFcmBridgeImpl : IFcmBridge
    {
        private readonly global::Com.Exzilegames.Fcmbridge.FcmBridge _bridge;
        private Action<string>? _tokenRefreshListener;
        private Action<FcmMessage>? _messageListener;

        /// <inheritdoc/>
        public bool IsAvailable => true;

        /// <summary>
        /// Creates a new FCM bridge instance and wires up the static Java listeners
        /// so that token refreshes and incoming messages are forwarded to any registered
        /// C# callbacks.
        /// </summary>
        /// <param name="activity">
        /// The current Android activity. Not used directly by FCM but kept for consistency
        /// with other bridge constructors and future use.
        /// </param>
        public AndroidFcmBridgeImpl(Activity activity)
        {
            _bridge = new global::Com.Exzilegames.Fcmbridge.FcmBridge();

            global::Com.Exzilegames.Fcmbridge.FcmBridge.SetTokenListener(new TokenListenerImpl(this));
            global::Com.Exzilegames.Fcmbridge.FcmBridge.SetMessageListener(new MessageListenerImpl(this));
        }

        /// <inheritdoc/>
        public Task<FcmTokenResult> GetTokenAsync()
        {
            var tcs = new TaskCompletionSource<FcmTokenResult>();
            _bridge.GetToken(new TokenCallbackImpl(tcs));
            return tcs.Task;
        }

        /// <inheritdoc/>
        public Task<FcmOperationResult> SubscribeToTopicAsync(string topic)
        {
            var tcs = new TaskCompletionSource<FcmOperationResult>();
            _bridge.SubscribeToTopic(topic, new ResultCallbackImpl(tcs));
            return tcs.Task;
        }

        /// <inheritdoc/>
        public Task<FcmOperationResult> UnsubscribeFromTopicAsync(string topic)
        {
            var tcs = new TaskCompletionSource<FcmOperationResult>();
            _bridge.UnsubscribeFromTopic(topic, new ResultCallbackImpl(tcs));
            return tcs.Task;
        }

        /// <inheritdoc/>
        public void SetTokenRefreshListener(Action<string> listener)
            => _tokenRefreshListener = listener;

        /// <inheritdoc/>
        public void SetMessageListener(Action<FcmMessage> listener)
            => _messageListener = listener;

        // ── JSON deserialization helpers ──

        private static IReadOnlyDictionary<string, string> ParseDataJson(string? dataJson)
        {
            if (string.IsNullOrEmpty(dataJson) || dataJson == "{}")
                return new Dictionary<string, string>();

            try
            {
                var dict = JsonSerializer.Deserialize<Dictionary<string, string>>(dataJson);
                return dict ?? new Dictionary<string, string>();
            }
            catch
            {
                return new Dictionary<string, string>();
            }
        }

        // ── Listener implementations ──

        private sealed class TokenListenerImpl : Java.Lang.Object,
            global::Com.Exzilegames.Fcmbridge.FcmBridge.ITokenListener
        {
            private readonly AndroidFcmBridgeImpl _parent;
            public TokenListenerImpl(AndroidFcmBridgeImpl parent) => _parent = parent;
            public void OnNewToken(string? token)
                => _parent._tokenRefreshListener?.Invoke(token ?? string.Empty);
        }

        private sealed class MessageListenerImpl : Java.Lang.Object,
            global::Com.Exzilegames.Fcmbridge.FcmBridge.IMessageListener
        {
            private readonly AndroidFcmBridgeImpl _parent;
            public MessageListenerImpl(AndroidFcmBridgeImpl parent) => _parent = parent;
            public void OnMessageReceived(string? title, string? body, string? dataJson)
            {
                var data = ParseDataJson(dataJson);
                _parent._messageListener?.Invoke(new FcmMessage(title ?? string.Empty, body ?? string.Empty, data));
            }
        }

        private sealed class TokenCallbackImpl : Java.Lang.Object,
            global::Com.Exzilegames.Fcmbridge.FcmBridge.ITokenCallback
        {
            private readonly TaskCompletionSource<FcmTokenResult> _tcs;
            public TokenCallbackImpl(TaskCompletionSource<FcmTokenResult> tcs) => _tcs = tcs;
            public void OnToken(string? token)
                => _tcs.TrySetResult(new FcmTokenResult(true, token, null));
            public void OnError(string? message)
                => _tcs.TrySetResult(new FcmTokenResult(false, null, message));
        }

        private sealed class ResultCallbackImpl : Java.Lang.Object,
            global::Com.Exzilegames.Fcmbridge.FcmBridge.IResultCallback
        {
            private readonly TaskCompletionSource<FcmOperationResult> _tcs;
            public ResultCallbackImpl(TaskCompletionSource<FcmOperationResult> tcs) => _tcs = tcs;
            public void OnSuccess()
                => _tcs.TrySetResult(new FcmOperationResult(true, null));
            public void OnFailure(string? message)
                => _tcs.TrySetResult(new FcmOperationResult(false, message));
        }
    }
}
#endif
