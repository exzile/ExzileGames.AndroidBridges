namespace AndroidBillingBridge.Interop
{
    /// <summary>
    /// Static access point for the Billing Bridge. Set the platform implementation
    /// at startup via <see cref="SetImplementation"/>, then call methods from shared code.
    /// </summary>
    public static class BillingBridgeManager
    {
        private static IBillingBridge? _impl;

        public static bool IsAvailable => _impl?.IsAvailable ?? false;
        public static bool IsReady => _impl?.IsReady ?? false;

        public static void SetImplementation(IBillingBridge implementation)
            => _impl = implementation;

        // ── Connection ──
        public static Task<ConnectionResult> ConnectAsync()
            => _impl?.ConnectAsync()
               ?? Task.FromResult(new ConnectionResult(false, -1, "No platform implementation"));

        public static void Disconnect() => _impl?.Disconnect();
        public static int GetConnectionState() => _impl?.GetConnectionState() ?? -1;

        // ── Product Details ──
        public static Task<ProductDetailsResult> QueryInAppProductsAsync(string[] productIds)
            => _impl?.QueryInAppProductsAsync(productIds)
               ?? Task.FromResult(new ProductDetailsResult(false, null, "No platform implementation"));

        public static Task<ProductDetailsResult> QuerySubscriptionsAsync(string[] productIds)
            => _impl?.QuerySubscriptionsAsync(productIds)
               ?? Task.FromResult(new ProductDetailsResult(false, null, "No platform implementation"));

        // ── Purchase Flow ──
        public static void SetPurchaseListener(Action<PurchaseResult> listener)
            => _impl?.SetPurchaseListener(listener);

        public static void LaunchPurchaseFlow(string productId)
            => _impl?.LaunchPurchaseFlow(productId);

        public static void LaunchPurchaseFlowWithOffer(string productDetailsJson, string? offerToken)
            => _impl?.LaunchPurchaseFlowWithOffer(productDetailsJson, offerToken);

        // ── Consume & Acknowledge ──
        public static Task<ConsumeResult> ConsumeAsync(string purchaseToken)
            => _impl?.ConsumeAsync(purchaseToken)
               ?? Task.FromResult(new ConsumeResult(false, purchaseToken, "No platform implementation"));

        public static Task<OperationResult> AcknowledgeAsync(string purchaseToken)
            => _impl?.AcknowledgeAsync(purchaseToken)
               ?? Task.FromResult(new OperationResult(false, "No platform implementation"));

        // ── Query Purchases ──
        public static Task<QueryPurchasesResult> QueryInAppPurchasesAsync()
            => _impl?.QueryInAppPurchasesAsync()
               ?? Task.FromResult(new QueryPurchasesResult(false, null, "No platform implementation"));

        public static Task<QueryPurchasesResult> QuerySubscriptionPurchasesAsync()
            => _impl?.QuerySubscriptionPurchasesAsync()
               ?? Task.FromResult(new QueryPurchasesResult(false, null, "No platform implementation"));

        // ── Purchase History ──
        public static Task<PurchaseHistoryResult> QueryInAppPurchaseHistoryAsync()
            => _impl?.QueryInAppPurchaseHistoryAsync()
               ?? Task.FromResult(new PurchaseHistoryResult(false, null, "No platform implementation"));

        public static Task<PurchaseHistoryResult> QuerySubscriptionPurchaseHistoryAsync()
            => _impl?.QuerySubscriptionPurchaseHistoryAsync()
               ?? Task.FromResult(new PurchaseHistoryResult(false, null, "No platform implementation"));

        // ── In-App Messages ──
        public static void ShowInAppMessages(Action<InAppMessageResult>? listener)
            => _impl?.ShowInAppMessages(listener);
    }
}
