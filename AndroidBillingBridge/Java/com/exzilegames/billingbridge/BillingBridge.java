package com.exzilegames.billingbridge;

import android.app.Activity;
import android.util.Log;

import com.android.billingclient.api.AcknowledgePurchaseParams;
import com.android.billingclient.api.AcknowledgePurchaseResponseListener;
import com.android.billingclient.api.BillingClient;
import com.android.billingclient.api.BillingClientStateListener;
import com.android.billingclient.api.BillingFlowParams;
import com.android.billingclient.api.BillingResult;
import com.android.billingclient.api.ConsumeParams;
import com.android.billingclient.api.ConsumeResponseListener;
import com.android.billingclient.api.InAppMessageParams;
import com.android.billingclient.api.InAppMessageResult;
import com.android.billingclient.api.PendingPurchasesParams;
import com.android.billingclient.api.ProductDetails;
import com.android.billingclient.api.ProductDetailsResponseListener;
import com.android.billingclient.api.Purchase;
import com.android.billingclient.api.PurchaseHistoryRecord;
import com.android.billingclient.api.PurchasesResponseListener;
import com.android.billingclient.api.PurchasesUpdatedListener;
import com.android.billingclient.api.QueryProductDetailsParams;
import com.android.billingclient.api.QueryPurchasesParams;

import org.json.JSONArray;
import org.json.JSONObject;

import java.util.ArrayList;
import java.util.Collections;
import java.util.List;

/**
 * Java bridge exposing Google Play Billing Library APIs that are missing
 * or unreliable in the Xamarin.Android.Google.BillingClient NuGet bindings.
 *
 * Handles: connection, product queries, purchases, consume, acknowledge,
 * purchase history, subscriptions, and in-app messages.
 */
public final class BillingBridge implements PurchasesUpdatedListener {

    private static final String TAG = "BillingBridge";

    private BillingClient billingClient;
    private Activity activity;
    private PurchaseListener purchaseListener;

    // ── Listener interfaces ──

    public interface ConnectionListener {
        void onConnectionResult(boolean success, int responseCode, String message);
    }

    public interface ProductDetailsListener {
        void onProductDetailsResult(boolean success, String productDetailsJson, String message);
    }

    public interface PurchaseListener {
        void onPurchaseResult(int responseCode, String purchasesJson, String message);
    }

    public interface ConsumeListener {
        void onConsumeResult(boolean success, String purchaseToken, String message);
    }

    public interface AcknowledgeListener {
        void onAcknowledgeResult(boolean success, String message);
    }

    public interface QueryPurchasesListener {
        void onQueryPurchasesResult(boolean success, String purchasesJson, String message);
    }

    public interface PurchaseHistoryListener {
        void onPurchaseHistoryResult(boolean success, String historyJson, String message);
    }

    public interface InAppMessageListener {
        void onInAppMessageResult(int responseCode, String purchaseToken);
    }

    // ── Initialization ──

    public void initialize(Activity activity, boolean enablePendingPurchases) {
        this.activity = activity;
        PendingPurchasesParams pendingParams = PendingPurchasesParams.newBuilder()
                .enableOneTimeProducts()
                .enablePrepaidPlans()
                .build();

        billingClient = BillingClient.newBuilder(activity)
                .setListener(this)
                .enablePendingPurchases(pendingParams)
                .build();
    }

    public void connect(ConnectionListener listener) {
        if (billingClient == null) {
            listener.onConnectionResult(false, -1, "BillingClient not initialized");
            return;
        }
        billingClient.startConnection(new BillingClientStateListener() {
            @Override
            public void onBillingSetupFinished(BillingResult result) {
                boolean ok = result.getResponseCode() == BillingClient.BillingResponseCode.OK;
                listener.onConnectionResult(ok, result.getResponseCode(),
                        result.getDebugMessage());
            }

            @Override
            public void onBillingServiceDisconnected() {
                Log.w(TAG, "Billing service disconnected");
            }
        });
    }

    public boolean isReady() {
        return billingClient != null && billingClient.isReady();
    }

    public void endConnection() {
        if (billingClient != null) {
            billingClient.endConnection();
        }
    }

    public void setPurchaseListener(PurchaseListener listener) {
        this.purchaseListener = listener;
    }

    // ── PurchasesUpdatedListener (global callback) ──

    @Override
    public void onPurchasesUpdated(BillingResult result, List<Purchase> purchases) {
        if (purchaseListener == null) return;
        String json = purchasesToJson(purchases);
        purchaseListener.onPurchaseResult(result.getResponseCode(), json,
                result.getDebugMessage());
    }

    // ── Query Product Details ──

    public void queryProductDetails(String[] productIds, String productType,
                                    ProductDetailsListener listener) {
        if (billingClient == null || !billingClient.isReady()) {
            listener.onProductDetailsResult(false, "[]", "BillingClient not ready");
            return;
        }

        List<QueryProductDetailsParams.Product> products = new ArrayList<>();
        for (String id : productIds) {
            products.add(QueryProductDetailsParams.Product.newBuilder()
                    .setProductId(id)
                    .setProductType(productType)
                    .build());
        }

        QueryProductDetailsParams params = QueryProductDetailsParams.newBuilder()
                .setProductList(products)
                .build();

        billingClient.queryProductDetailsAsync(params, (result, queryResult) -> {
            boolean ok = result.getResponseCode() == BillingClient.BillingResponseCode.OK;
            List<ProductDetails> detailsList = queryResult != null ? queryResult.getProductDetailsList() : null;
            String json = productDetailsToJson(detailsList);
            listener.onProductDetailsResult(ok, json, result.getDebugMessage());
        });
    }

    public void queryInAppProducts(String[] productIds, ProductDetailsListener listener) {
        queryProductDetails(productIds, BillingClient.ProductType.INAPP, listener);
    }

    public void querySubscriptions(String[] productIds, ProductDetailsListener listener) {
        queryProductDetails(productIds, BillingClient.ProductType.SUBS, listener);
    }

    // ── Launch Purchase Flow ──

    public int launchPurchaseFlow(String productDetailsJson, String offerToken) {
        if (billingClient == null || activity == null) return -1;

        try {
            // Re-query the product to get a live ProductDetails object
            JSONObject json = new JSONObject(productDetailsJson);
            String productId = json.getString("productId");
            String productType = json.optString("productType",
                    BillingClient.ProductType.INAPP);

            List<QueryProductDetailsParams.Product> products = Collections.singletonList(
                    QueryProductDetailsParams.Product.newBuilder()
                            .setProductId(productId)
                            .setProductType(productType)
                            .build());

            QueryProductDetailsParams qParams = QueryProductDetailsParams.newBuilder()
                    .setProductList(products)
                    .build();

            billingClient.queryProductDetailsAsync(qParams, (result, queryResult) -> {
                List<ProductDetails> detailsList = queryResult != null ? queryResult.getProductDetailsList() : null;
                if (result.getResponseCode() != BillingClient.BillingResponseCode.OK
                        || detailsList == null || detailsList.isEmpty()) {
                    if (purchaseListener != null) {
                        purchaseListener.onPurchaseResult(
                                result.getResponseCode(), "[]",
                                "Failed to get product details for purchase flow");
                    }
                    return;
                }

                ProductDetails details = detailsList.get(0);
                BillingFlowParams.ProductDetailsParams.Builder pdBuilder =
                        BillingFlowParams.ProductDetailsParams.newBuilder()
                                .setProductDetails(details);

                if (offerToken != null && !offerToken.isEmpty()) {
                    pdBuilder.setOfferToken(offerToken);
                }

                BillingFlowParams flowParams = BillingFlowParams.newBuilder()
                        .setProductDetailsParamsList(
                                Collections.singletonList(pdBuilder.build()))
                        .build();

                activity.runOnUiThread(() -> {
                    billingClient.launchBillingFlow(activity, flowParams);
                });
            });
            return 0;
        } catch (Exception e) {
            Log.e(TAG, "launchPurchaseFlow error", e);
            return -1;
        }
    }

    public int launchPurchaseFlowByProductId(String productId) {
        return launchPurchaseFlow("{\"productId\":\"" + productId
                + "\",\"productType\":\"inapp\"}", null);
    }

    // ── Consume ──

    public void consume(String purchaseToken, ConsumeListener listener) {
        if (billingClient == null) {
            listener.onConsumeResult(false, purchaseToken, "BillingClient not initialized");
            return;
        }

        ConsumeParams params = ConsumeParams.newBuilder()
                .setPurchaseToken(purchaseToken)
                .build();

        billingClient.consumeAsync(params, (result, token) -> {
            boolean ok = result.getResponseCode() == BillingClient.BillingResponseCode.OK;
            listener.onConsumeResult(ok, token, result.getDebugMessage());
        });
    }

    // ── Acknowledge ──

    public void acknowledge(String purchaseToken, AcknowledgeListener listener) {
        if (billingClient == null) {
            listener.onAcknowledgeResult(false, "BillingClient not initialized");
            return;
        }

        AcknowledgePurchaseParams params = AcknowledgePurchaseParams.newBuilder()
                .setPurchaseToken(purchaseToken)
                .build();

        billingClient.acknowledgePurchase(params, result -> {
            boolean ok = result.getResponseCode() == BillingClient.BillingResponseCode.OK;
            listener.onAcknowledgeResult(ok, result.getDebugMessage());
        });
    }

    // ── Query Existing Purchases ──

    public void queryPurchases(String productType, QueryPurchasesListener listener) {
        if (billingClient == null || !billingClient.isReady()) {
            listener.onQueryPurchasesResult(false, "[]", "BillingClient not ready");
            return;
        }

        QueryPurchasesParams params = QueryPurchasesParams.newBuilder()
                .setProductType(productType)
                .build();

        billingClient.queryPurchasesAsync(params, (result, purchases) -> {
            boolean ok = result.getResponseCode() == BillingClient.BillingResponseCode.OK;
            listener.onQueryPurchasesResult(ok, purchasesToJson(purchases),
                    result.getDebugMessage());
        });
    }

    public void queryInAppPurchases(QueryPurchasesListener listener) {
        queryPurchases(BillingClient.ProductType.INAPP, listener);
    }

    public void querySubscriptionPurchases(QueryPurchasesListener listener) {
        queryPurchases(BillingClient.ProductType.SUBS, listener);
    }

    // ── Purchase History ──

    public void queryPurchaseHistory(String productType, PurchaseHistoryListener listener) {
        if (billingClient == null || !billingClient.isReady()) {
            listener.onPurchaseHistoryResult(false, "[]", "BillingClient not ready");
            return;
        }

        // queryPurchaseHistoryAsync was removed in Billing Library v8+.
        // Fall back to queryPurchasesAsync and convert the result.
        QueryPurchasesParams params = QueryPurchasesParams.newBuilder()
                .setProductType(productType)
                .build();

        billingClient.queryPurchasesAsync(params, (result, purchases) -> {
            boolean ok = result.getResponseCode() == BillingClient.BillingResponseCode.OK;
            listener.onPurchaseHistoryResult(ok, purchasesToHistoryJson(purchases),
                    result.getDebugMessage());
        });
    }

    // ── In-App Messages (price changes, subscription status) ──

    public void showInAppMessages(InAppMessageListener listener) {
        if (billingClient == null || activity == null) return;

        InAppMessageParams params = InAppMessageParams.newBuilder()
                .addInAppMessageCategoryToShow(
                        InAppMessageParams.InAppMessageCategoryId.TRANSACTIONAL)
                .build();

        billingClient.showInAppMessages(activity, params, result -> {
            String token = result.getPurchaseToken();
            listener.onInAppMessageResult(result.getResponseCode(),
                    token != null ? token : "");
        });
    }

    // ── Connection State ──

    public int getConnectionState() {
        if (billingClient == null) return -1;
        return billingClient.getConnectionState();
    }

    // ── JSON Serialization Helpers ──

    private static String productDetailsToJson(List<ProductDetails> list) {
        if (list == null) return "[]";
        JSONArray arr = new JSONArray();
        try {
            for (ProductDetails pd : list) {
                JSONObject obj = new JSONObject();
                obj.put("productId", pd.getProductId());
                obj.put("productType", pd.getProductType());
                obj.put("title", pd.getTitle());
                obj.put("name", pd.getName());
                obj.put("description", pd.getDescription());

                // One-time purchase pricing
                ProductDetails.OneTimePurchaseOfferDetails oneTime =
                        pd.getOneTimePurchaseOfferDetails();
                if (oneTime != null) {
                    JSONObject otObj = new JSONObject();
                    otObj.put("formattedPrice", oneTime.getFormattedPrice());
                    otObj.put("priceAmountMicros", oneTime.getPriceAmountMicros());
                    otObj.put("priceCurrencyCode", oneTime.getPriceCurrencyCode());
                    obj.put("oneTimePurchaseOfferDetails", otObj);
                }

                // Subscription pricing (multiple offers)
                List<ProductDetails.SubscriptionOfferDetails> subOffers =
                        pd.getSubscriptionOfferDetails();
                if (subOffers != null && !subOffers.isEmpty()) {
                    JSONArray offersArr = new JSONArray();
                    for (ProductDetails.SubscriptionOfferDetails offer : subOffers) {
                        JSONObject offerObj = new JSONObject();
                        offerObj.put("offerToken", offer.getOfferToken());
                        offerObj.put("basePlanId", offer.getBasePlanId());

                        JSONArray phasesArr = new JSONArray();
                        for (ProductDetails.PricingPhase phase :
                                offer.getPricingPhases().getPricingPhaseList()) {
                            JSONObject phaseObj = new JSONObject();
                            phaseObj.put("formattedPrice", phase.getFormattedPrice());
                            phaseObj.put("priceAmountMicros", phase.getPriceAmountMicros());
                            phaseObj.put("priceCurrencyCode", phase.getPriceCurrencyCode());
                            phaseObj.put("billingPeriod", phase.getBillingPeriod());
                            phaseObj.put("recurrenceMode", phase.getRecurrenceMode());
                            phaseObj.put("billingCycleCount", phase.getBillingCycleCount());
                            phasesArr.put(phaseObj);
                        }
                        offerObj.put("pricingPhases", phasesArr);
                        offersArr.put(offerObj);
                    }
                    obj.put("subscriptionOfferDetails", offersArr);
                }

                arr.put(obj);
            }
        } catch (Exception e) {
            Log.e(TAG, "productDetailsToJson error", e);
        }
        return arr.toString();
    }

    private static String purchasesToJson(List<Purchase> list) {
        if (list == null) return "[]";
        JSONArray arr = new JSONArray();
        try {
            for (Purchase p : list) {
                JSONObject obj = new JSONObject();
                obj.put("orderId", p.getOrderId());
                obj.put("purchaseToken", p.getPurchaseToken());
                obj.put("purchaseState", p.getPurchaseState());
                obj.put("purchaseTime", p.getPurchaseTime());
                obj.put("isAcknowledged", p.isAcknowledged());
                obj.put("isAutoRenewing", p.isAutoRenewing());
                obj.put("quantity", p.getQuantity());
                obj.put("originalJson", p.getOriginalJson());

                JSONArray products = new JSONArray();
                for (String pid : p.getProducts()) {
                    products.put(pid);
                }
                obj.put("products", products);
                arr.put(obj);
            }
        } catch (Exception e) {
            Log.e(TAG, "purchasesToJson error", e);
        }
        return arr.toString();
    }

    private static String purchasesToHistoryJson(List<Purchase> list) {
        if (list == null) return "[]";
        JSONArray arr = new JSONArray();
        try {
            for (Purchase p : list) {
                JSONObject obj = new JSONObject();
                obj.put("purchaseToken", p.getPurchaseToken());
                obj.put("purchaseTime", p.getPurchaseTime());
                obj.put("quantity", p.getQuantity());
                obj.put("originalJson", p.getOriginalJson());

                JSONArray products = new JSONArray();
                for (String pid : p.getProducts()) {
                    products.put(pid);
                }
                obj.put("products", products);
                arr.put(obj);
            }
        } catch (Exception e) {
            Log.e(TAG, "purchasesToHistoryJson error", e);
        }
        return arr.toString();
    }

    private static String purchaseHistoryToJson(List<PurchaseHistoryRecord> list) {
        if (list == null) return "[]";
        JSONArray arr = new JSONArray();
        try {
            for (PurchaseHistoryRecord r : list) {
                JSONObject obj = new JSONObject();
                obj.put("purchaseToken", r.getPurchaseToken());
                obj.put("purchaseTime", r.getPurchaseTime());
                obj.put("quantity", r.getQuantity());
                obj.put("originalJson", r.getOriginalJson());

                JSONArray products = new JSONArray();
                for (String pid : r.getProducts()) {
                    products.put(pid);
                }
                obj.put("products", products);
                arr.put(obj);
            }
        } catch (Exception e) {
            Log.e(TAG, "purchaseHistoryToJson error", e);
        }
        return arr.toString();
    }
}
