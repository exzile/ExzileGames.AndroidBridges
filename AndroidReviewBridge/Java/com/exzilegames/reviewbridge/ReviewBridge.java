package com.exzilegames.reviewbridge;

import android.app.Activity;

import com.google.android.play.core.review.ReviewInfo;
import com.google.android.play.core.review.ReviewManager;
import com.google.android.play.core.review.ReviewManagerFactory;
import com.google.android.play.core.tasks.Task;

public final class ReviewBridge {

    public interface ReviewListener {
        void onComplete();
        void onFailed(String message);
    }

    public void requestAndLaunch(Activity activity, ReviewListener listener) {
        ReviewManager manager = ReviewManagerFactory.create(activity);
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
