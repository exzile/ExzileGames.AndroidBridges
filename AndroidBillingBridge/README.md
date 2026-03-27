# AndroidBillingBridge

A Java bridge + C# interop library for Google Play Billing Library on .NET Android. Provides direct access to the full Billing API without reflection hacks or incomplete NuGet bindings.

## Problem

The `Xamarin.Android.Google.BillingClient` NuGet bindings have issues with certain API surfaces — missing method overloads, parameter order inconsistencies, and types that require reflection to invoke. This forces developers to write fragile reflection-based code to call basic operations like `ConsumeAsync` and `QueryProductDetailsAsync`.

## Solution

This library compiles a Java bridge class directly into your Android project. The Java class calls the Billing Library APIs natively, and C# communicates via auto-generated JNI bindings. No reflection needed.

## APIs Exposed

| API | Methods |
|-----|---------|
| **Connection** | `ConnectAsync`, `Disconnect`, `GetConnectionState` |
| **Product Details** | `QueryInAppProductsAsync`, `QuerySubscriptionsAsync` |
| **Purchase Flow** | `LaunchPurchaseFlow`, `LaunchPurchaseFlowWithOffer`, `SetPurchaseListener` |
| **Consume** | `ConsumeAsync` (no reflection hacks) |
| **Acknowledge** | `AcknowledgeAsync` |
| **Query Purchases** | `QueryInAppPurchasesAsync`, `QuerySubscriptionPurchasesAsync` |
| **Purchase History** | `QueryInAppPurchaseHistoryAsync`, `QuerySubscriptionPurchaseHistoryAsync` |
| **In-App Messages** | `ShowInAppMessages` (price changes, subscription status) |
| **Subscriptions** | Full offer token + pricing phase support |

## Setup

### 1. Add project reference

```xml
<ProjectReference Include="..\AndroidBillingBridge\AndroidBillingBridge.csproj" />
```

### 2. Initialize in your Activity

```csharp
using AndroidBillingBridge.Interop;

protected override void OnCreate(Bundle? savedInstanceState)
{
    base.OnCreate(savedInstanceState);

    var billing = new AndroidBillingBridgeImpl(this);
    BillingBridgeManager.SetImplementation(billing);
}
```

### 3. Use from shared code

```csharp
using AndroidBillingBridge.Interop;

// Connect
var conn = await BillingBridgeManager.ConnectAsync();

// Query products (with real prices from Google Play)
var products = await BillingBridgeManager.QueryInAppProductsAsync(
    ["redchest500", "redchest2000"]);

if (products.Success)
{
    foreach (var p in products.Products!)
        Console.WriteLine($"{p.Name}: {p.OneTimePurchaseOfferDetails?.FormattedPrice}");
}

// Set purchase listener
BillingBridgeManager.SetPurchaseListener(result =>
{
    if (result.ResponseCode == 0 && result.Purchases != null)
    {
        foreach (var purchase in result.Purchases)
        {
            // Consume or acknowledge
            _ = BillingBridgeManager.ConsumeAsync(purchase.PurchaseToken);
        }
    }
});

// Launch purchase
BillingBridgeManager.LaunchPurchaseFlow("redchest500");

// Query existing purchases (restore)
var existing = await BillingBridgeManager.QueryInAppPurchasesAsync();
```

## What this replaces

The existing `AndroidInAppPurchaseService` uses reflection to work around binding issues:

```csharp
// BEFORE: reflection hacks
var method = billingClient.GetType().GetMethods()
    .Where(m => m.Name == "ConsumeAsync" || m.Name == "Consume")...
method.Invoke(billingClient, args);
```

```csharp
// AFTER: direct bridge call
var result = await BillingBridgeManager.ConsumeAsync(purchaseToken);
```

## License

MIT
