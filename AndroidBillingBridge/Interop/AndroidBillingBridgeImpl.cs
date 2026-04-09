#if ANDROID
using Android.App;
using System.Text.Json;

namespace AndroidBillingBridge.Interop
{
    /// <summary>
    /// Android implementation of <see cref="IBillingBridge"/> that delegates
    /// to the compiled Java class <c>BillingBridge</c> via auto-generated JNI bindings.
    /// </summary>
    public sealed class AndroidBillingBridgeImpl : IBillingBridge
    {
        private readonly Activity _activity;
        private readonly global::Com.Exzilegames.Billingbridge.BillingBridge _bridge;
        private Action<PurchaseResult>? _purchaseListener;

        /// <inheritdoc/>
        public bool IsAvailable => true;

        /// <inheritdoc/>
        public bool IsReady => _bridge.IsReady;

        /// <summary>Creates a new billing bridge backed by the given Android activity.</summary>
        /// <param name="activity">The activity used to run billing operations on the UI thread.</param>
        public AndroidBillingBridgeImpl(Activity activity)
        {
            _activity = activity;
            _bridge = new global::Com.Exzilegames.Billingbridge.BillingBridge();
            _bridge.Initialize(activity, true);
            _bridge.SetPurchaseListener(new PurchaseListenerImpl(this));
        }

        // ── Connection ──

        /// <inheritdoc/>
        public Task<ConnectionResult> ConnectAsync()
        {
            var tcs = new TaskCompletionSource<ConnectionResult>();
            _activity.RunOnUiThread(() =>
            {
                _bridge.Connect(new ConnectionListenerImpl(tcs));
            });
            return tcs.Task;
        }

        /// <inheritdoc/>
        public void Disconnect() => _bridge.EndConnection();

        /// <inheritdoc/>
        public int GetConnectionState() => _bridge.ConnectionState;

        // ── Product Details ──

        /// <inheritdoc/>
        public Task<ProductDetailsResult> QueryInAppProductsAsync(string[] productIds)
        {
            var tcs = new TaskCompletionSource<ProductDetailsResult>();
            _activity.RunOnUiThread(() =>
            {
                _bridge.QueryInAppProducts(productIds, new ProductDetailsListenerImpl(tcs));
            });
            return tcs.Task;
        }

        /// <inheritdoc/>
        public Task<ProductDetailsResult> QuerySubscriptionsAsync(string[] productIds)
        {
            var tcs = new TaskCompletionSource<ProductDetailsResult>();
            _activity.RunOnUiThread(() =>
            {
                _bridge.QuerySubscriptions(productIds, new ProductDetailsListenerImpl(tcs));
            });
            return tcs.Task;
        }

        // ── Purchase Flow ──

        /// <inheritdoc/>
        public void SetPurchaseListener(Action<PurchaseResult> listener)
            => _purchaseListener = listener;

        /// <inheritdoc/>
        public void LaunchPurchaseFlow(string productId)
        {
            _activity.RunOnUiThread(() => _bridge.LaunchPurchaseFlowByProductId(productId));
        }

        /// <inheritdoc/>
        public void LaunchPurchaseFlowWithOffer(string productDetailsJson, string? offerToken)
        {
            _activity.RunOnUiThread(() =>
                _bridge.LaunchPurchaseFlow(productDetailsJson, offerToken ?? ""));
        }

        // ── Consume & Acknowledge ──

        /// <inheritdoc/>
        public Task<ConsumeResult> ConsumeAsync(string purchaseToken)
        {
            var tcs = new TaskCompletionSource<ConsumeResult>();
            _activity.RunOnUiThread(() =>
            {
                _bridge.Consume(purchaseToken, new ConsumeListenerImpl(tcs));
            });
            return tcs.Task;
        }

        /// <inheritdoc/>
        public Task<OperationResult> AcknowledgeAsync(string purchaseToken)
        {
            var tcs = new TaskCompletionSource<OperationResult>();
            _activity.RunOnUiThread(() =>
            {
                _bridge.Acknowledge(purchaseToken, new AcknowledgeListenerImpl(tcs));
            });
            return tcs.Task;
        }

        // ── Query Purchases ──

        /// <inheritdoc/>
        public Task<QueryPurchasesResult> QueryInAppPurchasesAsync()
        {
            var tcs = new TaskCompletionSource<QueryPurchasesResult>();
            _activity.RunOnUiThread(() =>
            {
                _bridge.QueryInAppPurchases(new QueryPurchasesListenerImpl(tcs));
            });
            return tcs.Task;
        }

        /// <inheritdoc/>
        public Task<QueryPurchasesResult> QuerySubscriptionPurchasesAsync()
        {
            var tcs = new TaskCompletionSource<QueryPurchasesResult>();
            _activity.RunOnUiThread(() =>
            {
                _bridge.QuerySubscriptionPurchases(new QueryPurchasesListenerImpl(tcs));
            });
            return tcs.Task;
        }

        // ── Purchase History ──

        /// <inheritdoc/>
        public Task<PurchaseHistoryResult> QueryInAppPurchaseHistoryAsync()
            => QueryPurchaseHistoryInternal("inapp");

        /// <inheritdoc/>
        public Task<PurchaseHistoryResult> QuerySubscriptionPurchaseHistoryAsync()
            => QueryPurchaseHistoryInternal("subs");

        private Task<PurchaseHistoryResult> QueryPurchaseHistoryInternal(string productType)
        {
            var tcs = new TaskCompletionSource<PurchaseHistoryResult>();
            _activity.RunOnUiThread(() =>
            {
                _bridge.QueryPurchaseHistory(productType, new PurchaseHistoryListenerImpl(tcs));
            });
            return tcs.Task;
        }

        // ── In-App Messages (callback not available in Xamarin binding) ──

        /// <inheritdoc/>
        public void ShowInAppMessages(Action<InAppMessageResult>? listener)
        {
            // InAppMessageResponseCallback is not exposed by the Xamarin binding JAR,
            // so we cannot call showInAppMessages from Java. Notify caller as unsupported.
            listener?.Invoke(new InAppMessageResult(-1, ""));
        }

        // ── JSON deserialization helpers ──

        private static ProductDetailsEntry[]? DeserializeProducts(string? json)
        {
            if (string.IsNullOrEmpty(json) || json == "[]") return null;
            try
            {
                var entries = JsonSerializer.Deserialize<ProductDetailsEntry[]>(json);
                if (entries != null)
                {
                    // Store raw JSON per entry for passing back to purchase flow
                    foreach (var e in entries)
                        e.RawJson = JsonSerializer.Serialize(e);
                }
                return entries;
            }
            catch { return null; }
        }

        private static PurchaseEntry[]? DeserializePurchases(string? json)
        {
            if (string.IsNullOrEmpty(json) || json == "[]") return null;
            try { return JsonSerializer.Deserialize<PurchaseEntry[]>(json); }
            catch { return null; }
        }

        private static PurchaseHistoryEntry[]? DeserializeHistory(string? json)
        {
            if (string.IsNullOrEmpty(json) || json == "[]") return null;
            try { return JsonSerializer.Deserialize<PurchaseHistoryEntry[]>(json); }
            catch { return null; }
        }

        // ── Listener implementations ──

        private sealed class ConnectionListenerImpl : Java.Lang.Object,
            global::Com.Exzilegames.Billingbridge.BillingBridge.IConnectionListener
        {
            private readonly TaskCompletionSource<ConnectionResult> _tcs;
            public ConnectionListenerImpl(TaskCompletionSource<ConnectionResult> tcs) => _tcs = tcs;
            public void OnConnectionResult(bool success, int responseCode, string? message)
                => _tcs.TrySetResult(new ConnectionResult(success, responseCode, message));
        }

        private sealed class ProductDetailsListenerImpl : Java.Lang.Object,
            global::Com.Exzilegames.Billingbridge.BillingBridge.IProductDetailsListener
        {
            private readonly TaskCompletionSource<ProductDetailsResult> _tcs;
            public ProductDetailsListenerImpl(TaskCompletionSource<ProductDetailsResult> tcs) => _tcs = tcs;
            public void OnProductDetailsResult(bool success, string? json, string? message)
                => _tcs.TrySetResult(new ProductDetailsResult(success, DeserializeProducts(json), message));
        }

        private sealed class PurchaseListenerImpl : Java.Lang.Object,
            global::Com.Exzilegames.Billingbridge.BillingBridge.IPurchaseListener
        {
            private readonly AndroidBillingBridgeImpl _parent;
            public PurchaseListenerImpl(AndroidBillingBridgeImpl parent) => _parent = parent;
            public void OnPurchaseResult(int responseCode, string? json, string? message)
            {
                var result = new PurchaseResult(responseCode, DeserializePurchases(json), message);
                _parent._purchaseListener?.Invoke(result);
            }
        }

        private sealed class ConsumeListenerImpl : Java.Lang.Object,
            global::Com.Exzilegames.Billingbridge.BillingBridge.IConsumeListener
        {
            private readonly TaskCompletionSource<ConsumeResult> _tcs;
            public ConsumeListenerImpl(TaskCompletionSource<ConsumeResult> tcs) => _tcs = tcs;
            public void OnConsumeResult(bool success, string? token, string? message)
                => _tcs.TrySetResult(new ConsumeResult(success, token, message));
        }

        private sealed class AcknowledgeListenerImpl : Java.Lang.Object,
            global::Com.Exzilegames.Billingbridge.BillingBridge.IAcknowledgeListener
        {
            private readonly TaskCompletionSource<OperationResult> _tcs;
            public AcknowledgeListenerImpl(TaskCompletionSource<OperationResult> tcs) => _tcs = tcs;
            public void OnAcknowledgeResult(bool success, string? message)
                => _tcs.TrySetResult(new OperationResult(success, message));
        }

        private sealed class QueryPurchasesListenerImpl : Java.Lang.Object,
            global::Com.Exzilegames.Billingbridge.BillingBridge.IQueryPurchasesListener
        {
            private readonly TaskCompletionSource<QueryPurchasesResult> _tcs;
            public QueryPurchasesListenerImpl(TaskCompletionSource<QueryPurchasesResult> tcs) => _tcs = tcs;
            public void OnQueryPurchasesResult(bool success, string? json, string? message)
                => _tcs.TrySetResult(new QueryPurchasesResult(success, DeserializePurchases(json), message));
        }

        private sealed class PurchaseHistoryListenerImpl : Java.Lang.Object,
            global::Com.Exzilegames.Billingbridge.BillingBridge.IPurchaseHistoryListener
        {
            private readonly TaskCompletionSource<PurchaseHistoryResult> _tcs;
            public PurchaseHistoryListenerImpl(TaskCompletionSource<PurchaseHistoryResult> tcs) => _tcs = tcs;
            public void OnPurchaseHistoryResult(bool success, string? json, string? message)
                => _tcs.TrySetResult(new PurchaseHistoryResult(success, DeserializeHistory(json), message));
        }

    }
}
#endif
