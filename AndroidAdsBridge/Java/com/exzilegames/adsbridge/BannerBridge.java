package com.exzilegames.adsbridge;

import android.app.Activity;
import android.view.Gravity;
import android.view.View;
import android.widget.FrameLayout;

import com.google.android.gms.ads.AdRequest;
import com.google.android.gms.ads.AdSize;
import com.google.android.gms.ads.AdView;

public final class BannerBridge {
    public static final int POSITION_TOP = 0;
    public static final int POSITION_BOTTOM = 1;

    private AdView adView;

    public void show(Activity activity, String adUnitId, int position) {
        activity.runOnUiThread(() -> {
            if (adView != null) {
                adView.destroy();
                removeFromParent();
            }

            adView = new AdView(activity);
            adView.setAdUnitId(adUnitId);
            adView.setAdSize(AdSize.BANNER);

            FrameLayout.LayoutParams params = new FrameLayout.LayoutParams(
                FrameLayout.LayoutParams.MATCH_PARENT,
                FrameLayout.LayoutParams.WRAP_CONTENT
            );
            params.gravity = (position == POSITION_TOP) ? Gravity.TOP : Gravity.BOTTOM;

            FrameLayout rootView = activity.findViewById(android.R.id.content);
            rootView.addView(adView, params);

            adView.loadAd(new AdRequest.Builder().build());
        });
    }

    public void hide(Activity activity) {
        activity.runOnUiThread(() -> {
            if (adView != null) adView.setVisibility(View.GONE);
        });
    }

    public void reveal(Activity activity) {
        activity.runOnUiThread(() -> {
            if (adView != null) adView.setVisibility(View.VISIBLE);
        });
    }

    public void destroy(Activity activity) {
        activity.runOnUiThread(() -> {
            if (adView != null) {
                adView.destroy();
                removeFromParent();
                adView = null;
            }
        });
    }

    private void removeFromParent() {
        if (adView != null && adView.getParent() instanceof FrameLayout) {
            ((FrameLayout) adView.getParent()).removeView(adView);
        }
    }
}
