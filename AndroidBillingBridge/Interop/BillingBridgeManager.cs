namespace AndroidBillingBridge.Interop
{
    /// <summary>
    /// Static access point for the Billing Bridge. Set the platform implementation
    /// at startup via <see cref="SetImplementation"/>, then call methods from shared code.
    /// </summary>
    public static class BillingBridgeManager
    {
        private static IBillingBridge? _impl;

        /// <summary>Gets whether a billing implementation is available on this platform.</summary>
        public static bool IsAvailable => _impl?.IsAvailable ?? false;

        /// <summary>Gets whether the billing service is connected and ready for requests.</summary>
        public static bool IsReady => _impl?.IsReady ?? false;

        /// <summary>Registers the platform-specific billing implementation.</summary>
        /// <param name="implementation">The platform billing bridge to use.</param>
        public static void SetImplementation(IBillingBridge implementation)
            => _impl = implementation;

        // ── Connection ──

        /// <summary>Connects to the billing service asynchronously.</summary>
        /// <returns>A <see cref="ConnectionResult"/> indicating success or failure.</returns>
        public static Task<ConnectionResult> ConnectAsync()
            => _impl?.ConnectAsync()
               ?? Task.FromResult(new ConnectionResult(false, -1, "No platform implementation"));

        /// <summary>Disconnects from the billing service.</summary>
        public static void Disconnect() => _impl?.Disconnect();

        /// <summary>Returns the current connection state of the billing service.</summary>
        /// <returns>An integer representing the connection state, or -1 if no implementation is set.</returns>
        public static int GetConnectionState() => _impl?.GetConnectionState() ?? -1;

        // ── Product Details ──

        /// <summary>Queries details for one-time in-app products.</summary>
        /// <param name="productIds">The product identifiers to query.</param>
        /// <returns>A <see cref="ProductDetailsResult"/> containing the queried product details.</returns>
        public static Task<ProductDetailsResult> QueryInAppProductsAsync(string[] productIds)
            => _impl?.QueryInAppProductsAsync(productIds)
               ?? Task.FromResult(new ProductDetailsResult(false, null, "No platform implementation"));

        /// <summary>Queries details for subscription products.</summary>
        /// <param name="productIds">The subscription product identifiers to query.</param>
        /// <returns>A <see cref="ProductDetailsResult"/> containing the queried subscription details.</returns>
        public static Task<ProductDetailsResult> QuerySubscriptionsAsync(string[] productIds)
            => _impl?.QuerySubscriptionsAsync(productIds)
               ?? Task.FromResult(new ProductDetailsResult(false, null, "No platform implementation"));

        // ── Purchase Flow ──

        /// <summary>Registers a listener that is invoked when a purchase completes.</summary>
        /// <param name="listener">The callback to invoke with the purchase result.</param>
        public static void SetPurchaseListener(Action<PurchaseResult> listener)
            => _impl?.SetPurchaseListener(listener);

        /// <summary>Launches the purchase flow for a product.</summary>
        /// <param name="productId">The product identifier to purchase.</param>
        public static void LaunchPurchaseFlow(string productId)
            => _impl?.LaunchPurchaseFlow(productId);

        /// <summary>Launches the purchase flow for a product with an optional offer token.</summary>
        /// <param name="productDetailsJson">The product details JSON string.</param>
        /// <param name="offerToken">An optional offer token for the purchase.</param>
        public static void LaunchPurchaseFlowWithOffer(string productDetailsJson, string? offerToken)
            => _impl?.LaunchPurchaseFlowWithOffer(productDetailsJson, offerToken);

        // ── Consume & Acknowledge ──

        /// <summary>Consumes a one-time purchase so it can be bought again.</summary>
        /// <param name="purchaseToken">The token of the purchase to consume.</param>
        /// <returns>A <see cref="ConsumeResult"/> indicating success or failure.</returns>
        public static Task<ConsumeResult> ConsumeAsync(string purchaseToken)
            => _impl?.ConsumeAsync(purchaseToken)
               ?? Task.FromResult(new ConsumeResult(false, purchaseToken, "No platform implementation"));

        /// <summary>Acknowledges a purchase to prevent it from being refunded.</summary>
        /// <param name="purchaseToken">The token of the purchase to acknowledge.</param>
        /// <returns>An <see cref="OperationResult"/> indicating success or failure.</returns>
        public static Task<OperationResult> AcknowledgeAsync(string purchaseToken)
            => _impl?.AcknowledgeAsync(purchaseToken)
               ?? Task.FromResult(new OperationResult(false, "No platform implementation"));

        // ── Query Purchases ──

        /// <summary>Queries active one-time in-app purchases.</summary>
        /// <returns>A <see cref="QueryPurchasesResult"/> containing the active purchases.</returns>
        public static Task<QueryPurchasesResult> QueryInAppPurchasesAsync()
            => _impl?.QueryInAppPurchasesAsync()
               ?? Task.FromResult(new QueryPurchasesResult(false, null, "No platform implementation"));

        /// <summary>Queries active subscription purchases.</summary>
        /// <returns>A <see cref="QueryPurchasesResult"/> containing the active subscriptions.</returns>
        public static Task<QueryPurchasesResult> QuerySubscriptionPurchasesAsync()
            => _impl?.QuerySubscriptionPurchasesAsync()
               ?? Task.FromResult(new QueryPurchasesResult(false, null, "No platform implementation"));

        // ── Purchase History ──

        /// <summary>Queries the purchase history for one-time in-app products.</summary>
        /// <returns>A <see cref="PurchaseHistoryResult"/> containing the purchase history.</returns>
        public static Task<PurchaseHistoryResult> QueryInAppPurchaseHistoryAsync()
            => _impl?.QueryInAppPurchaseHistoryAsync()
               ?? Task.FromResult(new PurchaseHistoryResult(false, null, "No platform implementation"));

        /// <summary>Queries the purchase history for subscriptions.</summary>
        /// <returns>A <see cref="PurchaseHistoryResult"/> containing the subscription history.</returns>
        public static Task<PurchaseHistoryResult> QuerySubscriptionPurchaseHistoryAsync()
            => _impl?.QuerySubscriptionPurchaseHistoryAsync()
               ?? Task.FromResult(new PurchaseHistoryResult(false, null, "No platform implementation"));

        // ── In-App Messages ──

        /// <summary>Shows in-app messages from the billing service (e.g., price change confirmations).</summary>
        /// <param name="listener">An optional callback invoked with the message result.</param>
        public static void ShowInAppMessages(Action<InAppMessageResult>? listener)
            => _impl?.ShowInAppMessages(listener);
    }
}
