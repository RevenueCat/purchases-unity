// Scanner fixture only (not compiled): multiple public top-level types in one file is fine here.
// Covers @Keep appearing in positions other than a leading, standalone annotation line.
package com.revenuecat.fixtures;

import androidx.annotation.Keep;

@SuppressWarnings("unchecked") @Keep
public class KeepAfterOtherAnnotation {
}

@Keep @SuppressWarnings("unchecked")
public class KeepBeforeOtherAnnotation {
}

@SuppressWarnings(
    "unchecked"
) @Keep
public class KeepOnClosingLine {
}

@Keep public class InlineKeep {
}
