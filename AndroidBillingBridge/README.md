# AndroidBillingBridge

A Java bridge + C# interop library for Google Play Billing Library v8 on .NET Android. Provides direct access to the full Billing API without reflection hacks or incomplete NuGet bindings.

## Problem

The `Xamarin.Android.Google.BillingClient` NuGet binding has issues with certain API surfaces — missing method overloads, parameter type inconsistencies, and types that require reflection to invoke. This forces developers to write fragile reflection-based code to call basic operations like `ConsumeAsync` and `QueryProductDetailsAsync`.

## Solution

This library compiles a Java bridge class directly into your Android project. The Java class calls the Billing Library APIs natively, and C# communicates via auto-generated JNI bindings. No reflection needed.

## APIs Exposed

| API | Methods |
|-----|---------|
| **Connection** | `ConnectAsync`, `Disconnect`, `GetConnectionState` |
| **Product Details** | `QueryInAppProductsAsync`, `QuerySubscriptionsAsync` |
| **Purchase Flow** | `LaunchPurchaseFlow`, `LaunchPurchaseFlowWithOffer`, `SetPurchaseListener` |
| **Consume** | `ConsumeAsync` |
| **Acknowledge** | `AcknowledgeAsync` |
| **Query Purchases** | `QueryInAppPurchasesAsync`, `QuerySubscriptionPurchasesAsync` |
| **Purchase History** | `QueryInAppPurchaseHistoryAsync`, `QuerySubscriptionPurchaseHistoryAsync` |
| **In-App Messages** | `ShowInAppMessages` (price changes, subscription status) |

## Setup

### 1. Add NuGet package

```bash
dotnet add package ExzileGames.AndroidBillingBridge
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

// Connect to the billing service
var conn = await BillingBridgeManager.ConnectAsync();

// Query products (returns real prices from Google Play)
var products = await BillingBridgeManager.QueryInAppProductsAsync(
    ["redchest500", "redchest2000"]);

if (products.Success)
{
    foreach (var p in products.Products!)
        Console.WriteLine($"{p.Name}: {p.OneTimePurchaseOfferDetails?.FormattedPrice}");
}

// Listen for purchase results
BillingBridgeManager.SetPurchaseListener(result =>
{
    if (result.ResponseCode == 0 && result.Purchases != null)
    {
        foreach (var purchase in result.Purchases)
            _ = BillingBridgeManager.ConsumeAsync(purchase.PurchaseToken);
    }
});

// Launch purchase flow
BillingBridgeManager.LaunchPurchaseFlow("redchest500");

// Restore purchases
var existing = await BillingBridgeManager.QueryInAppPurchasesAsync();
```

## What This Replaces

```csharp
// BEFORE: reflection hacks required by the raw binding
var method = billingClient.GetType().GetMethods()
    .Where(m => m.Name == "ConsumeAsync" || m.Name == "Consume")...
method.Invoke(billingClient, args);

// AFTER: direct bridge call
var result = await BillingBridgeManager.ConsumeAsync(purchaseToken);
```

## License

MIT
