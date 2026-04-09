# AndroidFcmBridge

A Java bridge + C# interop library for Firebase Cloud Messaging (FCM) on .NET Android. Provides async token retrieval, topic subscriptions, and push message callbacks for re-engagement notifications.

## Problem

`FirebaseMessagingService` must be subclassed in Java and registered in the Android manifest — it cannot be implemented in C# alone. Without a compiled Java subclass, FCM token refresh events and incoming push messages are never delivered to your app.

## Solution

This library compiles a `FirebaseMessagingService` subclass (`ExzileFcmService`) directly into your Android project. It forwards token refreshes and incoming messages to your C# code via auto-generated JNI bindings, with no manual Java callback wiring required.

## APIs Exposed

| API | Methods |
|-----|---------|
| **Token** | `GetTokenAsync`, `SetTokenRefreshListener` |
| **Topics** | `SubscribeToTopicAsync`, `UnsubscribeFromTopicAsync` |
| **Messages** | `SetMessageListener` |

## Setup

### 1. Add NuGet package

```bash
dotnet add package ExzileGames.AndroidFcmBridge
```

### 2. Add google-services.json

Place your `google-services.json` (from the Firebase Console) in the Android app project root:

```xml
<GoogleServicesJson Include="google-services.json" />
```

### 3. Register the service in AndroidManifest.xml

```xml
<service android:name="com.exzilegames.fcmbridge.ExzileFcmService"
         android:exported="false">
    <intent-filter>
        <action android:name="com.google.firebase.MESSAGING_EVENT" />
    </intent-filter>
</service>
```

### 4. Initialize in your Activity

```csharp
using AndroidFcmBridge.Interop;

protected override void OnCreate(Bundle? savedInstanceState)
{
    base.OnCreate(savedInstanceState);

    var fcm = new AndroidFcmBridgeImpl(this);
    FcmBridgeManager.SetImplementation(fcm);
}
```

### 5. Use from shared code

```csharp
using AndroidFcmBridge.Interop;

// Register listeners before calling GetTokenAsync to avoid missing the first refresh
FcmBridgeManager.SetTokenRefreshListener(token =>
{
    // Send updated token to your server
    Console.WriteLine($"New FCM token: {token}");
});

FcmBridgeManager.SetMessageListener(message =>
{
    Console.WriteLine($"Push received: {message.Title} — {message.Body}");
    foreach (var (key, value) in message.Data)
        Console.WriteLine($"  {key} = {value}");
});

// Retrieve current token
var result = await FcmBridgeManager.GetTokenAsync();
if (result.Success)
    Console.WriteLine($"FCM token: {result.Token}");

// Subscribe/unsubscribe topics
await FcmBridgeManager.SubscribeToTopicAsync("news");
await FcmBridgeManager.UnsubscribeFromTopicAsync("news");
```

## License

MIT
