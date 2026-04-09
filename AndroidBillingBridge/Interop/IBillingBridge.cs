using System.Text.Json.Serialization;

namespace AndroidBillingBridge.Interop
{
    /// <summary>
    /// Platform-agnostic interface for Google Play Billing Library.
    /// Exposes the full billing API surface without reflection hacks.
    /// </summary>
    public interface IBillingBridge
    {
        /// <summary>Gets whether a billing implementation is available on this platform.</summary>
        bool IsAvailable { get; }

        /// <summary>Gets whether the billing service is connected and ready for requests.</summary>
        bool IsReady { get; }

        // ── Connection ──

        /// <summary>Connects to the billing service asynchronously.</summary>
        Task<ConnectionResult> ConnectAsync();

        /// <summary>Disconnects from the billing service.</summary>
        void Disconnect();

        /// <summary>Returns the current connection state of the billing service.</summary>
        int GetConnectionState();

        // ── Product Details ──

        /// <summary>Queries details for one-time in-app products.</summary>
        Task<ProductDetailsResult> QueryInAppProductsAsync(string[] productIds);

        /// <summary>Queries details for subscription products.</summary>
        Task<ProductDetailsResult> QuerySubscriptionsAsync(string[] productIds);

        // ── Purchase Flow ──

        /// <summary>Sets listener for purchase results (global callback from billing flow).</summary>
        void SetPurchaseListener(Action<PurchaseResult> listener);

        /// <summary>Launches the purchase flow for a product by its identifier.</summary>
        void LaunchPurchaseFlow(string productId);

        /// <summary>Launches the purchase flow for a product with an optional offer token.</summary>
        void LaunchPurchaseFlowWithOffer(string productDetailsJson, string? offerToken);

        // ── Consume & Acknowledge ──

        /// <summary>Consumes a one-time purchase so it can be bought again.</summary>
        Task<ConsumeResult> ConsumeAsync(string purchaseToken);

        /// <summary>Acknowledges a purchase to prevent it from being refunded.</summary>
        Task<OperationResult> AcknowledgeAsync(string purchaseToken);

        // ── Query Purchases ──

        /// <summary>Queries active one-time in-app purchases.</summary>
        Task<QueryPurchasesResult> QueryInAppPurchasesAsync();

        /// <summary>Queries active subscription purchases.</summary>
        Task<QueryPurchasesResult> QuerySubscriptionPurchasesAsync();

        // ── Purchase History ──

        /// <summary>Queries the purchase history for one-time in-app products.</summary>
        Task<PurchaseHistoryResult> QueryInAppPurchaseHistoryAsync();

        /// <summary>Queries the purchase history for subscriptions.</summary>
        Task<PurchaseHistoryResult> QuerySubscriptionPurchaseHistoryAsync();

        // ── In-App Messages ──

        /// <summary>Shows in-app messages from the billing service.</summary>
        void ShowInAppMessages(Action<InAppMessageResult>? listener);
    }

    // ── Result types ──

    /// <summary>Result of a billing service connection attempt.</summary>
    public readonly record struct ConnectionResult(bool Success, int ResponseCode, string? Message);

    /// <summary>Result of a generic billing operation.</summary>
    public readonly record struct OperationResult(bool Success, string? Message);

    /// <summary>Result of consuming a purchase.</summary>
    public readonly record struct ConsumeResult(bool Success, string? PurchaseToken, string? Message);

    /// <summary>Result of a product details query.</summary>
    public readonly record struct ProductDetailsResult(bool Success, ProductDetailsEntry[]? Products, string? Message);

    /// <summary>Result of a purchase flow or purchase update.</summary>
    public readonly record struct PurchaseResult(int ResponseCode, PurchaseEntry[]? Purchases, string? Message);

    /// <summary>Result of querying active purchases.</summary>
    public readonly record struct QueryPurchasesResult(bool Success, PurchaseEntry[]? Purchases, string? Message);

    /// <summary>Result of querying purchase history.</summary>
    public readonly record struct PurchaseHistoryResult(bool Success, PurchaseHistoryEntry[]? Records, string? Message);

    /// <summary>Result of an in-app message interaction.</summary>
    public readonly record struct InAppMessageResult(int ResponseCode, string? PurchaseToken);

    // ── Data types ──

    /// <summary>Details for a product listed in Google Play.</summary>
    public class ProductDetailsEntry
    {
        /// <summary>The product identifier.</summary>
        [JsonPropertyName("productId")]
        public string ProductId { get; set; } = "";

        /// <summary>The product type (e.g. "inapp" or "subs").</summary>
        [JsonPropertyName("productType")]
        public string ProductType { get; set; } = "";

        /// <summary>The localized title of the product.</summary>
        [JsonPropertyName("title")]
        public string Title { get; set; } = "";

        /// <summary>The localized name of the product.</summary>
        [JsonPropertyName("name")]
        public string Name { get; set; } = "";

        /// <summary>The localized description of the product.</summary>
        [JsonPropertyName("description")]
        public string Description { get; set; } = "";

        /// <summary>Price details for one-time purchase products, or null for subscriptions.</summary>
        [JsonPropertyName("oneTimePurchaseOfferDetails")]
        public OneTimePriceDetails? OneTimePurchaseOfferDetails { get; set; }

        /// <summary>Offer details for subscription products, or null for one-time purchases.</summary>
        [JsonPropertyName("subscriptionOfferDetails")]
        public SubscriptionOfferEntry[]? SubscriptionOfferDetails { get; set; }

        /// <summary>The original JSON string for passing back to launchPurchaseFlow.</summary>
        [JsonIgnore]
        public string? RawJson { get; set; }
    }

    /// <summary>Price details for a one-time purchase product.</summary>
    public class OneTimePriceDetails
    {
        /// <summary>The formatted price string including currency symbol.</summary>
        [JsonPropertyName("formattedPrice")]
        public string FormattedPrice { get; set; } = "";

        /// <summary>The price in micro-units (1,000,000 micro-units = one unit of currency).</summary>
        [JsonPropertyName("priceAmountMicros")]
        public long PriceAmountMicros { get; set; }

        /// <summary>The ISO 4217 currency code.</summary>
        [JsonPropertyName("priceCurrencyCode")]
        public string PriceCurrencyCode { get; set; } = "";
    }

    /// <summary>A subscription offer with pricing phases.</summary>
    public class SubscriptionOfferEntry
    {
        /// <summary>The offer token used to initiate a purchase.</summary>
        [JsonPropertyName("offerToken")]
        public string OfferToken { get; set; } = "";

        /// <summary>The base plan identifier.</summary>
        [JsonPropertyName("basePlanId")]
        public string BasePlanId { get; set; } = "";

        /// <summary>The pricing phases for this offer.</summary>
        [JsonPropertyName("pricingPhases")]
        public PricingPhaseEntry[]? PricingPhases { get; set; }
    }

    /// <summary>A single pricing phase within a subscription offer.</summary>
    public class PricingPhaseEntry
    {
        /// <summary>The formatted price string including currency symbol.</summary>
        [JsonPropertyName("formattedPrice")]
        public string FormattedPrice { get; set; } = "";

        /// <summary>The price in micro-units (1,000,000 micro-units = one unit of currency).</summary>
        [JsonPropertyName("priceAmountMicros")]
        public long PriceAmountMicros { get; set; }

        /// <summary>The ISO 4217 currency code.</summary>
        [JsonPropertyName("priceCurrencyCode")]
        public string PriceCurrencyCode { get; set; } = "";

        /// <summary>The billing period in ISO 8601 format (e.g. "P1M" for one month).</summary>
        [JsonPropertyName("billingPeriod")]
        public string BillingPeriod { get; set; } = "";

        /// <summary>The recurrence mode (1 = infinite, 2 = finite, 3 = non-recurring).</summary>
        [JsonPropertyName("recurrenceMode")]
        public int RecurrenceMode { get; set; }

        /// <summary>The number of billing cycles this phase lasts.</summary>
        [JsonPropertyName("billingCycleCount")]
        public int BillingCycleCount { get; set; }
    }

    /// <summary>Represents a purchase made by the user.</summary>
    public class PurchaseEntry
    {
        /// <summary>The order identifier assigned by Google Play.</summary>
        [JsonPropertyName("orderId")]
        public string OrderId { get; set; } = "";

        /// <summary>The token that uniquely identifies this purchase.</summary>
        [JsonPropertyName("purchaseToken")]
        public string PurchaseToken { get; set; } = "";

        /// <summary>The purchase state (0 = purchased, 1 = pending).</summary>
        [JsonPropertyName("purchaseState")]
        public int PurchaseState { get; set; }

        /// <summary>The time the purchase was made, in milliseconds since epoch.</summary>
        [JsonPropertyName("purchaseTime")]
        public long PurchaseTime { get; set; }

        /// <summary>Whether the purchase has been acknowledged.</summary>
        [JsonPropertyName("isAcknowledged")]
        public bool IsAcknowledged { get; set; }

        /// <summary>Whether the subscription is set to auto-renew.</summary>
        [JsonPropertyName("isAutoRenewing")]
        public bool IsAutoRenewing { get; set; }

        /// <summary>The quantity of the purchased item.</summary>
        [JsonPropertyName("quantity")]
        public int Quantity { get; set; }

        /// <summary>The product identifiers included in this purchase.</summary>
        [JsonPropertyName("products")]
        public string[]? Products { get; set; }

        /// <summary>The original JSON response from the billing service.</summary>
        [JsonPropertyName("originalJson")]
        public string? OriginalJson { get; set; }
    }

    /// <summary>A historical purchase record.</summary>
    public class PurchaseHistoryEntry
    {
        /// <summary>The token that uniquely identifies this purchase.</summary>
        [JsonPropertyName("purchaseToken")]
        public string PurchaseToken { get; set; } = "";

        /// <summary>The time the purchase was made, in milliseconds since epoch.</summary>
        [JsonPropertyName("purchaseTime")]
        public long PurchaseTime { get; set; }

        /// <summary>The quantity of the purchased item.</summary>
        [JsonPropertyName("quantity")]
        public int Quantity { get; set; }

        /// <summary>The product identifiers included in this purchase.</summary>
        [JsonPropertyName("products")]
        public string[]? Products { get; set; }

        /// <summary>The original JSON response from the billing service.</summary>
        [JsonPropertyName("originalJson")]
        public string? OriginalJson { get; set; }
    }
}
