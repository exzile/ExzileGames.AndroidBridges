package com.exzilegames.adsbridge;

import android.app.Activity;

import com.google.android.gms.ads.AdError;
import com.google.android.gms.ads.AdRequest;
import com.google.android.gms.ads.FullScreenContentCallback;
import com.google.android.gms.ads.LoadAdError;
import com.google.android.gms.ads.interstitial.InterstitialAd;
import com.google.android.gms.ads.interstitial.InterstitialAdLoadCallback;

public final class InterstitialBridge {
    private InterstitialAd interstitial;
    private boolean loading;
    private InterstitialListener listener;

    public interface InterstitialListener {
        void onDismissed();
        void onFailedToShow(String message);
    }

    public void setListener(InterstitialListener listener) {
        this.listener = listener;
    }

    public boolean isReady() {
        return interstitial != null;
    }

    public void load(Activity activity, String adUnitId) {
        if (loading || interstitial != null) return;
        loading = true;

        AdRequest request = new AdRequest.Builder().build();
        InterstitialAd.load(activity, adUnitId, request, new InterstitialAdLoadCallback() {
            @Override
            public void onAdLoaded(InterstitialAd ad) {
                loading = false;
                interstitial = ad;
            }

            @Override
            public void onAdFailedToLoad(LoadAdError loadAdError) {
                loading = false;
                interstitial = null;
            }
        });
    }

    public boolean show(Activity activity) {
        final InterstitialAd ad = interstitial;
        interstitial = null;

        if (ad == null) return false;

        ad.setFullScreenContentCallback(new FullScreenContentCallback() {
            @Override
            public void onAdDismissedFullScreenContent() {
                if (listener != null) listener.onDismissed();
            }

            @Override
            public void onAdFailedToShowFullScreenContent(AdError adError) {
                if (listener != null) listener.onFailedToShow(adError != null ? adError.getMessage() : null);
            }
        });

        ad.show(activity);
        return true;
    }
}
