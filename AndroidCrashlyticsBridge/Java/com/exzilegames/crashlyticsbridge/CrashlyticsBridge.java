package com.exzilegames.crashlyticsbridge;

import com.google.firebase.crashlytics.FirebaseCrashlytics;
import java.util.ArrayList;
import java.util.List;

public final class CrashlyticsBridge {

    private static FirebaseCrashlytics getInstance() {
        return FirebaseCrashlytics.getInstance();
    }

    /** Records a C# exception with proper type grouping in Crashlytics. */
    public void recordException(String exceptionType, String message, String stackTrace) {
        try {
            CSharpException ex = new CSharpException(exceptionType, message);
            ex.setStackTrace(parseStackTrace(stackTrace));
            getInstance().recordException(ex);
        } catch (Exception ignored) {
            getInstance().log("CrashlyticsBridge: failed to record exception - " + exceptionType + ": " + message);
        }
    }

    public void log(String message) { getInstance().log(message); }
    public void setUserId(String userId) { getInstance().setUserId(userId); }
    public void setCustomKeyString(String key, String value) { getInstance().setCustomKey(key, value); }
    public void setCustomKeyBool(String key, boolean value) { getInstance().setCustomKey(key, value); }
    public void setCustomKeyInt(String key, int value) { getInstance().setCustomKey(key, value); }
    public void setCustomKeyFloat(String key, float value) { getInstance().setCustomKey(key, value); }
    public void setCrashlyticsCollectionEnabled(boolean enabled) {
        getInstance().setCrashlyticsCollectionEnabled(enabled);
    }
    public boolean didCrashOnPreviousExecution() {
        return getInstance().didCrashOnPreviousExecution();
    }

    private static StackTraceElement[] parseStackTrace(String stackTrace) {
        if (stackTrace == null || stackTrace.isEmpty()) return new StackTraceElement[0];
        String[] lines = stackTrace.split("\n");
        List<StackTraceElement> elements = new ArrayList<>();
        for (String line : lines) {
            line = line.trim();
            if (line.isEmpty()) continue;
            // C# frames: "   at Namespace.Class.Method() in File.cs:line 42"
            // Map to StackTraceElement(class, method, file, lineNumber)
            String className = "";
            String methodName = line;
            String fileName = "";
            int lineNumber = 0;
            int atIdx = line.indexOf("at ");
            if (atIdx >= 0) methodName = line.substring(atIdx + 3);
            int inIdx = methodName.indexOf(" in ");
            if (inIdx >= 0) {
                String fileInfo = methodName.substring(inIdx + 4);
                methodName = methodName.substring(0, inIdx);
                int colonIdx = fileInfo.lastIndexOf(":line ");
                if (colonIdx >= 0) {
                    fileName = fileInfo.substring(0, colonIdx);
                    try { lineNumber = Integer.parseInt(fileInfo.substring(colonIdx + 6).trim()); } catch (Exception ignored) {}
                } else { fileName = fileInfo; }
            }
            int dotIdx = methodName.lastIndexOf('.');
            if (dotIdx >= 0) { className = methodName.substring(0, dotIdx); methodName = methodName.substring(dotIdx + 1); }
            elements.add(new StackTraceElement(className, methodName, fileName, lineNumber));
        }
        return elements.toArray(new StackTraceElement[0]);
    }

    /** Wrapper exception that carries the original C# type name as its class name. */
    private static class CSharpException extends RuntimeException {
        private final String typeName;
        public CSharpException(String typeName, String message) {
            super(typeName + ": " + message);
            this.typeName = typeName;
        }
        @Override public String toString() { return typeName + ": " + getMessage(); }
    }
}
