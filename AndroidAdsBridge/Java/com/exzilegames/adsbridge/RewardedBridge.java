package com.exzilegames.adsbridge;

import android.app.Activity;

import com.google.android.gms.ads.AdRequest;
import com.google.android.gms.ads.OnUserEarnedRewardListener;
import com.google.android.gms.ads.FullScreenContentCallback;
import com.google.android.gms.ads.LoadAdError;
import com.google.android.gms.ads.rewarded.RewardItem;
import com.google.android.gms.ads.rewarded.RewardedAd;
import com.google.android.gms.ads.rewarded.RewardedAdLoadCallback;

public final class RewardedBridge {
    private RewardedAd rewarded;
    private boolean loading;
    private RewardedListener listener;

    public interface RewardedListener {
        void onRewardedEarned();
        void onRewardedDismissed();
        void onRewardedFailedToShow(String message);
    }

    public void setListener(RewardedListener listener) {
        this.listener = listener;
    }

    public boolean isReady() {
        return rewarded != null;
    }

    public void load(Activity activity, String adUnitId) {
        if (loading || rewarded != null) return;
        loading = true;

        AdRequest request = new AdRequest.Builder().build();
        RewardedAd.load(activity, adUnitId, request, new RewardedAdLoadCallback() {
            @Override
            public void onAdLoaded(RewardedAd ad) {
                loading = false;
                rewarded = ad;
            }

            @Override
            public void onAdFailedToLoad(LoadAdError loadAdError) {
                loading = false;
                rewarded = null;
            }
        });
    }

    public boolean show(Activity activity) {
        final RewardedAd ad = rewarded;
        rewarded = null;

        if (ad == null) return false;

        ad.setFullScreenContentCallback(new FullScreenContentCallback() {
            @Override
            public void onAdDismissedFullScreenContent() {
                if (listener != null) {
                    listener.onRewardedDismissed();
                }
            }

            @Override
            public void onAdFailedToShowFullScreenContent(com.google.android.gms.ads.AdError adError) {
                if (listener != null) {
                    listener.onRewardedFailedToShow(adError != null ? adError.getMessage() : null);
                }
            }
        });

        ad.show(activity, new OnUserEarnedRewardListener() {
            @Override
            public void onUserEarnedReward(RewardItem rewardItem) {
                if (listener != null) {
                    listener.onRewardedEarned();
                }
            }
        });

        return true;
    }
}
