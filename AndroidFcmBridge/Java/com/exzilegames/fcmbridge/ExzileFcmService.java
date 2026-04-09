package com.exzilegames.fcmbridge;

import com.google.firebase.messaging.FirebaseMessagingService;
import com.google.firebase.messaging.RemoteMessage;
import org.json.JSONObject;
import java.util.Map;

public final class ExzileFcmService extends FirebaseMessagingService {
    @Override
    public void onNewToken(String token) {
        FcmBridge.dispatchToken(token);
    }

    @Override
    public void onMessageReceived(RemoteMessage message) {
        String title = "";
        String body = "";
        if (message.getNotification() != null) {
            title = message.getNotification().getTitle() != null ? message.getNotification().getTitle() : "";
            body = message.getNotification().getBody() != null ? message.getNotification().getBody() : "";
        }
        // Serialize data map to JSON
        String dataJson = "{}";
        try {
            JSONObject json = new JSONObject();
            for (Map.Entry<String, String> entry : message.getData().entrySet()) {
                json.put(entry.getKey(), entry.getValue());
            }
            dataJson = json.toString();
        } catch (Exception ignored) {}
        FcmBridge.dispatchMessage(title, body, dataJson);
    }
}
