package com.exzilegames.adsbridge;

import android.app.Activity;

import com.google.android.gms.ads.AdError;
import com.google.android.gms.ads.AdRequest;
import com.google.android.gms.ads.FullScreenContentCallback;
import com.google.android.gms.ads.LoadAdError;
import com.google.android.gms.ads.appopen.AppOpenAd;

public final class AppOpenBridge {
    private AppOpenAd appOpenAd;
    private boolean loading;
    private AppOpenListener listener;

    public interface AppOpenListener {
        void onDismissed();
        void onFailedToShow(String message);
    }

    public void setListener(AppOpenListener listener) {
        this.listener = listener;
    }

    public boolean isReady() {
        return appOpenAd != null;
    }

    public void load(Activity activity, String adUnitId) {
        if (loading || appOpenAd != null) return;
        loading = true;

        AdRequest request = new AdRequest.Builder().build();
        AppOpenAd.load(activity, adUnitId, request, new AppOpenAd.AppOpenAdLoadCallback() {
            @Override
            public void onAdLoaded(AppOpenAd ad) {
                loading = false;
                appOpenAd = ad;
            }

            @Override
            public void onAdFailedToLoad(LoadAdError loadAdError) {
                loading = false;
                appOpenAd = null;
            }
        });
    }

    public boolean show(Activity activity) {
        final AppOpenAd ad = appOpenAd;
        appOpenAd = null;

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
