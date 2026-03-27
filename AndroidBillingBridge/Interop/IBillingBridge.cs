using System.Text.Json.Serialization;

namespace AndroidBillingBridge.Interop
{
    /// <summary>
    /// Platform-agnostic interface for Google Play Billing Library.
    /// Exposes the full billing API surface without reflection hacks.
    /// </summary>
    public interface IBillingBridge
    {
        bool IsAvailable { get; }
        bool IsReady { get; }

        // ── Connection ──
        Task<ConnectionResult> ConnectAsync();
        void Disconnect();
        int GetConnectionState();

        // ── Product Details ──
        Task<ProductDetailsResult> QueryInAppProductsAsync(string[] productIds);
        Task<ProductDetailsResult> QuerySubscriptionsAsync(string[] productIds);

        // ── Purchase Flow ──
        /// <summary>Set listener for purchase results (global callback from billing flow).</summary>
        void SetPurchaseListener(Action<PurchaseResult> listener);
        void LaunchPurchaseFlow(string productId);
        void LaunchPurchaseFlowWithOffer(string productDetailsJson, string? offerToken);

        // ── Consume & Acknowledge ──
        Task<ConsumeResult> ConsumeAsync(string purchaseToken);
        Task<OperationResult> AcknowledgeAsync(string purchaseToken);

        // ── Query Purchases ──
        Task<QueryPurchasesResult> QueryInAppPurchasesAsync();
        Task<QueryPurchasesResult> QuerySubscriptionPurchasesAsync();

        // ── Purchase History ──
        Task<PurchaseHistoryResult> QueryInAppPurchaseHistoryAsync();
        Task<PurchaseHistoryResult> QuerySubscriptionPurchaseHistoryAsync();

        // ── In-App Messages ──
        void ShowInAppMessages(Action<InAppMessageResult>? listener);
    }

    // ── Result types ──

    public readonly record struct ConnectionResult(bool Success, int ResponseCode, string? Message);
    public readonly record struct OperationResult(bool Success, string? Message);
    public readonly record struct ConsumeResult(bool Success, string? PurchaseToken, string? Message);

    public readonly record struct ProductDetailsResult(bool Success, ProductDetailsEntry[]? Products, string? Message);
    public readonly record struct PurchaseResult(int ResponseCode, PurchaseEntry[]? Purchases, string? Message);
    public readonly record struct QueryPurchasesResult(bool Success, PurchaseEntry[]? Purchases, string? Message);
    public readonly record struct PurchaseHistoryResult(bool Success, PurchaseHistoryEntry[]? Records, string? Message);
    public readonly record struct InAppMessageResult(int ResponseCode, string? PurchaseToken);

    // ── Data types ──

    public class ProductDetailsEntry
    {
        [JsonPropertyName("productId")]
        public string ProductId { get; set; } = "";
        [JsonPropertyName("productType")]
        public string ProductType { get; set; } = "";
        [JsonPropertyName("title")]
        public string Title { get; set; } = "";
        [JsonPropertyName("name")]
        public string Name { get; set; } = "";
        [JsonPropertyName("description")]
        public string Description { get; set; } = "";
        [JsonPropertyName("oneTimePurchaseOfferDetails")]
        public OneTimePriceDetails? OneTimePurchaseOfferDetails { get; set; }
        [JsonPropertyName("subscriptionOfferDetails")]
        public SubscriptionOfferEntry[]? SubscriptionOfferDetails { get; set; }

        /// <summary>The original JSON string for passing back to launchPurchaseFlow.</summary>
        [JsonIgnore]
        public string? RawJson { get; set; }
    }

    public class OneTimePriceDetails
    {
        [JsonPropertyName("formattedPrice")]
        public string FormattedPrice { get; set; } = "";
        [JsonPropertyName("priceAmountMicros")]
        public long PriceAmountMicros { get; set; }
        [JsonPropertyName("priceCurrencyCode")]
        public string PriceCurrencyCode { get; set; } = "";
    }

    public class SubscriptionOfferEntry
    {
        [JsonPropertyName("offerToken")]
        public string OfferToken { get; set; } = "";
        [JsonPropertyName("basePlanId")]
        public string BasePlanId { get; set; } = "";
        [JsonPropertyName("pricingPhases")]
        public PricingPhaseEntry[]? PricingPhases { get; set; }
    }

    public class PricingPhaseEntry
    {
        [JsonPropertyName("formattedPrice")]
        public string FormattedPrice { get; set; } = "";
        [JsonPropertyName("priceAmountMicros")]
        public long PriceAmountMicros { get; set; }
        [JsonPropertyName("priceCurrencyCode")]
        public string PriceCurrencyCode { get; set; } = "";
        [JsonPropertyName("billingPeriod")]
        public string BillingPeriod { get; set; } = "";
        [JsonPropertyName("recurrenceMode")]
        public int RecurrenceMode { get; set; }
        [JsonPropertyName("billingCycleCount")]
        public int BillingCycleCount { get; set; }
    }

    public class PurchaseEntry
    {
        [JsonPropertyName("orderId")]
        public string OrderId { get; set; } = "";
        [JsonPropertyName("purchaseToken")]
        public string PurchaseToken { get; set; } = "";
        [JsonPropertyName("purchaseState")]
        public int PurchaseState { get; set; }
        [JsonPropertyName("purchaseTime")]
        public long PurchaseTime { get; set; }
        [JsonPropertyName("isAcknowledged")]
        public bool IsAcknowledged { get; set; }
        [JsonPropertyName("isAutoRenewing")]
        public bool IsAutoRenewing { get; set; }
        [JsonPropertyName("quantity")]
        public int Quantity { get; set; }
        [JsonPropertyName("products")]
        public string[]? Products { get; set; }
        [JsonPropertyName("originalJson")]
        public string? OriginalJson { get; set; }
    }

    public class PurchaseHistoryEntry
    {
        [JsonPropertyName("purchaseToken")]
        public string PurchaseToken { get; set; } = "";
        [JsonPropertyName("purchaseTime")]
        public long PurchaseTime { get; set; }
        [JsonPropertyName("quantity")]
        public int Quantity { get; set; }
        [JsonPropertyName("products")]
        public string[]? Products { get; set; }
        [JsonPropertyName("originalJson")]
        public string? OriginalJson { get; set; }
    }
}
