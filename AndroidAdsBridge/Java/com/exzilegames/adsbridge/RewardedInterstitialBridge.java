package com.exzilegames.adsbridge;

import android.app.Activity;

import com.google.android.gms.ads.AdError;
import com.google.android.gms.ads.AdRequest;
import com.google.android.gms.ads.FullScreenContentCallback;
import com.google.android.gms.ads.LoadAdError;
import com.google.android.gms.ads.OnUserEarnedRewardListener;
import com.google.android.gms.ads.rewarded.RewardItem;
import com.google.android.gms.ads.rewardedinterstitial.RewardedInterstitialAd;
import com.google.android.gms.ads.rewardedinterstitial.RewardedInterstitialAdLoadCallback;

public final class RewardedInterstitialBridge {
    private RewardedInterstitialAd rewardedInterstitial;
    private boolean loading;
    private RewardedInterstitialListener listener;

    public interface RewardedInterstitialListener {
        void onRewardedEarned();
        void onDismissed();
        void onFailedToShow(String message);
    }

    public void setListener(RewardedInterstitialListener listener) {
        this.listener = listener;
    }

    public boolean isReady() {
        return rewardedInterstitial != null;
    }

    public void load(Activity activity, String adUnitId) {
        if (loading || rewardedInterstitial != null) return;
        loading = true;

        AdRequest request = new AdRequest.Builder().build();
        RewardedInterstitialAd.load(activity, adUnitId, request, new RewardedInterstitialAdLoadCallback() {
            @Override
            public void onAdLoaded(RewardedInterstitialAd ad) {
                loading = false;
                rewardedInterstitial = ad;
            }

            @Override
            public void onAdFailedToLoad(LoadAdError loadAdError) {
                loading = false;
                rewardedInterstitial = null;
            }
        });
    }

    public boolean show(Activity activity) {
        final RewardedInterstitialAd ad = rewardedInterstitial;
        rewardedInterstitial = null;

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

        ad.show(activity, new OnUserEarnedRewardListener() {
            @Override
            public void onUserEarnedReward(RewardItem rewardItem) {
                if (listener != null) listener.onRewardedEarned();
            }
        });

        return true;
    }
}
