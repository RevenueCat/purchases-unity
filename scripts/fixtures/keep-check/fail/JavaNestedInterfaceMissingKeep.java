package com.revenuecat.fixtures;

import androidx.annotation.Keep;

// Outer class is annotated, but the nested public interface is not.
// Expected flagged: NestedNoKeep (outer exempt via @Keep; private Helper exempt)
@Keep
public class JavaNestedInterfaceMissingKeep {
    public interface NestedNoKeep {
        void onResult(String value);
    }

    private static class Helper {
    }
}
