# AndroidFcmBridge

A Java bridge + C# interop library for Firebase Cloud Messaging (FCM) on .NET Android. Provides async token retrieval, topic subscription, and push message callbacks for re-engagement notifications.

## Problem

`FirebaseMessagingService` must be subclassed in Java and registered in the Android manifest — it cannot be implemented in C# alone. This library handles the Java side for you, forwarding token refreshes and incoming messages to your C# code via auto-generated JNI bindings.

## APIs Exposed

| API | Methods |
|-----|---------|
| **Token** | `GetTokenAsync`, `SetTokenRefreshListener` |
| **Topics** | `SubscribeToTopicAsync`, `UnsubscribeFromTopicAsync` |
| **Messages** | `SetMessageListener` |

## Setup

### 1. Add `google-services.json`

Place your project's `google-services.json` file in the root of the consuming Android project. This file is obtained from the Firebase console and is required for FCM to initialise.

### 2. Register the service in AndroidManifest.xml

The `ExzileFcmService` must be declared in the consuming project's `AndroidManifest.xml` so that the Android system can route FCM events to the bridge:

```xml
<service android:name="com.exzilegames.fcmbridge.ExzileFcmService"
         android:exported="false">
    <intent-filter>
        <action android:name="com.google.firebase.MESSAGING_EVENT" />
    </intent-filter>
</service>
```

### 3. Add project reference

```xml
<ProjectReference Include="..\AndroidFcmBridge\AndroidFcmBridge.csproj" />
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

// Listen for token refreshes (call before GetTokenAsync to avoid missing the first refresh)
FcmBridgeManager.SetTokenRefreshListener(token =>
{
    Console.WriteLine($"New FCM token: {token}");
    // Send token to your server
});

// Listen for incoming push messages
FcmBridgeManager.SetMessageListener(message =>
{
    Console.WriteLine($"Push received: {message.Title} — {message.Body}");
    foreach (var (key, value) in message.Data)
        Console.WriteLine($"  {key} = {value}");
});

// Retrieve the current token
var result = await FcmBridgeManager.GetTokenAsync();
if (result.Success)
    Console.WriteLine($"FCM token: {result.Token}");

// Subscribe to a topic
var sub = await FcmBridgeManager.SubscribeToTopicAsync("news");
if (!sub.Success)
    Console.WriteLine($"Subscribe failed: {sub.Message}");

// Unsubscribe from a topic
await FcmBridgeManager.UnsubscribeFromTopicAsync("news");
```

## License

MIT
