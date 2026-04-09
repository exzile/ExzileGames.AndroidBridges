package com.exzilegames.consentbridge;

import android.app.Activity;
import android.util.Log;

import com.google.android.ump.ConsentInformation;
import com.google.android.ump.ConsentRequestParameters;
import com.google.android.ump.FormError;
import com.google.android.ump.UserMessagingPlatform;

/**
 * Java bridge exposing Google User Messaging Platform (UMP) v3 APIs to .NET Android.
 *
 * Handles consent info updates and loading/showing the consent form required before
 * displaying ads under GDPR and CCPA. All callbacks are dispatched on the UI thread
 * as required by the UMP SDK.
 */
public final class ConsentBridge {

    private static final String TAG = "ConsentBridge";

    // ── Listener interfaces ──

    /**
     * Callback for {@link #requestConsentInfoUpdate}.
     */
    public interface ConsentUpdateListener {
        /** Called when the consent info update succeeded. */
        void onSuccess();
        /** Called when the consent info update failed. */
        void onFailure(int errorCode, String message);
    }

    /**
     * Callback for {@link #loadAndShowConsentFormIfRequired}.
     */
    public interface ConsentFormListener {
        /**
         * Called when the consent form flow is complete (or skipped when not required).
         *
         * @param canRequestAds  whether ads may be requested given the current consent state.
         * @param consentStatus  the raw UMP consent status integer (0=UNKNOWN, 1=REQUIRED,
         *                       2=NOT_REQUIRED, 3=OBTAINED).
         * @param errorCode      0 if no error occurred, otherwise the UMP {@link FormError} code.
         * @param errorMessage   human-readable error description, or empty string on success.
         */
        void onComplete(boolean canRequestAds, int consentStatus, int errorCode, String errorMessage);
    }

    // ── Public API ──

    /**
     * Requests an update of the user's consent information from the UMP server.
     *
     * <p>Call this once per app session before calling
     * {@link #loadAndShowConsentFormIfRequired}. The listener is always invoked on
     * the thread that called this method (UMP posts it back via the activity's main
     * looper).</p>
     *
     * @param activity                 the foreground activity.
     * @param tagForUnderAgeOfConsent  {@code true} to tag the request as directed at users
     *                                 under the age of consent (COPPA / EEA).
     * @param debugReset               if {@code true}, resets the consent state via
     *                                 {@link ConsentInformation#reset()} before requesting an
     *                                 update. Use only during development/testing.
     * @param listener                 callback invoked on success or failure.
     */
    public void requestConsentInfoUpdate(Activity activity,
                                         boolean tagForUnderAgeOfConsent,
                                         boolean debugReset,
                                         ConsentUpdateListener listener) {
        if (activity == null) {
            listener.onFailure(-1, "Activity is null");
            return;
        }

        ConsentInformation consentInfo = UserMessagingPlatform.getConsentInformation(activity);

        if (debugReset) {
            consentInfo.reset();
            Log.d(TAG, "Consent state reset for debug");
        }

        ConsentRequestParameters.Builder paramsBuilder = new ConsentRequestParameters.Builder()
                .setTagForUnderAgeOfConsent(tagForUnderAgeOfConsent);

        ConsentRequestParameters params = paramsBuilder.build();

        consentInfo.requestConsentInfoUpdate(
                activity,
                params,
                () -> {
                    Log.d(TAG, "Consent info update succeeded");
                    listener.onSuccess();
                },
                formError -> {
                    Log.w(TAG, "Consent info update failed: " + formError.getMessage());
                    listener.onFailure(formError.getErrorCode(), formError.getMessage());
                });
    }

    /**
     * Loads and shows the UMP consent form if it is required for this user.
     *
     * <p>Must be called after a successful {@link #requestConsentInfoUpdate} call.
     * If the form is not required (e.g. the user is outside a regulated region),
     * the listener is invoked immediately with the current consent state and no
     * error.</p>
     *
     * @param activity the foreground activity used to present the form.
     * @param listener callback invoked when the form is dismissed or when no form
     *                 was needed.
     */
    public void loadAndShowConsentFormIfRequired(Activity activity,
                                                  ConsentFormListener listener) {
        if (activity == null) {
            listener.onComplete(false, ConsentInformation.ConsentStatus.UNKNOWN, -1, "Activity is null");
            return;
        }

        UserMessagingPlatform.loadAndShowConsentFormIfRequired(
                activity,
                formError -> {
                    ConsentInformation consentInfo =
                            UserMessagingPlatform.getConsentInformation(activity);
                    boolean canAds = consentInfo.canRequestAds();
                    int status = consentInfo.getConsentStatus();

                    if (formError != null) {
                        Log.w(TAG, "Consent form error: " + formError.getMessage());
                        listener.onComplete(canAds, status,
                                formError.getErrorCode(), formError.getMessage());
                    } else {
                        Log.d(TAG, "Consent form complete. canRequestAds=" + canAds
                                + " status=" + status);
                        listener.onComplete(canAds, status, 0, "");
                    }
                });
    }

    /**
     * Returns whether ads can be requested given the current consent state.
     *
     * @param activity the context used to retrieve the consent information object.
     * @return {@code true} if ads may be shown.
     */
    public boolean canRequestAds(Activity activity) {
        if (activity == null) return false;
        return UserMessagingPlatform.getConsentInformation(activity).canRequestAds();
    }

    /**
     * Returns the current UMP consent status as a raw integer.
     *
     * <p>Values: 0=UNKNOWN, 1=REQUIRED, 2=NOT_REQUIRED, 3=OBTAINED.</p>
     *
     * @param activity the context used to retrieve the consent information object.
     * @return the consent status integer.
     */
    public int getConsentStatus(Activity activity) {
        if (activity == null) return ConsentInformation.ConsentStatus.UNKNOWN;
        return UserMessagingPlatform.getConsentInformation(activity).getConsentStatus();
    }

    /**
     * Resets the consent state, clearing any previously collected consent.
     *
     * <p><strong>For debug/testing only.</strong> Do not call this in production builds.</p>
     *
     * @param activity the context used to retrieve the consent information object.
     */
    public void reset(Activity activity) {
        if (activity == null) return;
        UserMessagingPlatform.getConsentInformation(activity).reset();
        Log.d(TAG, "Consent state reset");
    }
}
