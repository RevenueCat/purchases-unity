package com.revenuecat.fixtures;

import androidx.annotation.Keep;

@Keep
public class JavaNestedWithKeep {
    // Nested public interface reached from C# via AndroidJavaProxy: needs its own @Keep.
    @Keep
    public interface Callbacks {
        void onResult(String value);
    }

    // Private nested type: internal detail, exempt.
    private static class Helper {
    }
}
