package com.exzilegames.fcmbridge;

import com.google.firebase.messaging.FirebaseMessaging;

public final class FcmBridge {
    // Static listeners relay service callbacks to C#
    private static volatile TokenListener tokenListener;
    private static volatile MessageListener messageListener;

    public interface TokenListener { void onNewToken(String token); }
    public interface MessageListener { void onMessageReceived(String title, String body, String dataJson); }
    public interface TokenCallback { void onToken(String token); void onError(String message); }
    public interface ResultCallback { void onSuccess(); void onFailure(String message); }

    public static void setTokenListener(TokenListener listener) { tokenListener = listener; }
    public static void setMessageListener(MessageListener listener) { messageListener = listener; }

    // Called by ExzileFcmService
    public static void dispatchToken(String token) {
        if (tokenListener != null) tokenListener.onNewToken(token);
    }
    public static void dispatchMessage(String title, String body, String dataJson) {
        if (messageListener != null) messageListener.onMessageReceived(title, body, dataJson);
    }

    public void getToken(TokenCallback callback) {
        FirebaseMessaging.getInstance().getToken()
            .addOnSuccessListener(token -> callback.onToken(token))
            .addOnFailureListener(e -> callback.onError(e.getMessage()));
    }

    public void subscribeToTopic(String topic, ResultCallback callback) {
        FirebaseMessaging.getInstance().subscribeToTopic(topic)
            .addOnSuccessListener(v -> callback.onSuccess())
            .addOnFailureListener(e -> callback.onFailure(e.getMessage()));
    }

    public void unsubscribeFromTopic(String topic, ResultCallback callback) {
        FirebaseMessaging.getInstance().unsubscribeFromTopic(topic)
            .addOnSuccessListener(v -> callback.onSuccess())
            .addOnFailureListener(e -> callback.onFailure(e.getMessage()));
    }
}
