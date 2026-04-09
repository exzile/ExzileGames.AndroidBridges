---
name: Xamarin Binding Missing Overloads — JNI Fallback
description: When a Xamarin/MAUI binding is missing a Java method overload, call it via JNI using Android.Runtime.JNIEnv
type: feedback
---

When a Xamarin binding only exposes a subset of a Java class's overloads, call the missing one directly via JNI rather than guessing at type coercions.

**Why:** The `net10.0-android36.0` binding for `Xamarin.Firebase.Config` 123.0.1.2 only exposes `SetDefaultsAsync(int resourceId)`. The map-based `setDefaultsAsync(Map<String,Object>)` overload is absent from the binding. Attempting `HashMap` or `IDictionary<string,object>` both fail with CS1503.

**How to apply:**
```csharp
using Android.Runtime; // JNIEnv, JValue

using var map = new Java.Util.HashMap();
foreach (var (key, value) in defaults)
    map.Put(new Java.Lang.String(key), new Java.Lang.String(value?.ToString() ?? ""));

var classRef = JNIEnv.FindClass("com/google/firebase/remoteconfig/FirebaseRemoteConfig");
var methodId  = JNIEnv.GetMethodID(classRef, "setDefaultsAsync",
    "(Ljava/util/Map;)Lcom/google/android/gms/tasks/Task;");
JNIEnv.CallObjectMethod(_config.Handle, methodId, new JValue(map));
JNIEnv.DeleteLocalRef(classRef);
```

`JNIEnv` and `JValue` both live in `Android.Runtime` — add that using directive.
