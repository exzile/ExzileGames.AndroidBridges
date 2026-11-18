package com.exzilegames.reviewbridge;

import android.app.Activity;

import com.google.android.play.core.review.ReviewInfo;
import com.google.android.play.core.review.ReviewManager;
import com.google.android.play.core.review.ReviewManagerFactory;
import com.google.android.play.core.review.testing.FakeReviewManager;
import com.google.android.gms.tasks.Task;

public final class ReviewBridge {

    public interface ReviewListener {
        void onComplete();
        void onFailed(String message);
    }

    public void requestAndLaunch(Activity activity, ReviewListener listener) {
        requestAndLaunch(activity, false, listener);
    }

    /**
     * Requests and launches the review flow. With useFakeManager, Play's FakeReviewManager runs
     * the whole flow without showing a card (for testing on sideloaded or debug builds, where
     * the real manager never shows one).
     */
    public void requestAndLaunch(Activity activity, boolean useFakeManager, ReviewListener listener) {
        ReviewManager manager = useFakeManager ? new FakeReviewManager(activity) : ReviewManagerFactory.create(activity);
        Task<ReviewInfo> request = manager.requestReviewFlow();

        request.addOnCompleteListener(task -> {
            if (!task.isSuccessful()) {
                String msg = task.getException() != null ? task.getException().getMessage() : "request failed";
                if (listener != null) listener.onFailed(msg);
                return;
            }

            ReviewInfo reviewInfo = task.getResult();
            Task<Void> flow = manager.launchReviewFlow(activity, reviewInfo);
            flow.addOnCompleteListener(flowTask -> {
                // Flow completion does not indicate whether the user submitted a review.
                // Google intentionally does not expose that information.
                if (listener != null) listener.onComplete();
            });
        });
    }
}
