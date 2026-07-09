package com.revenuecat.fixtures

// Non-public Kotlin types are not reachable from C# by name, so @Keep is not required.

private class PrivateClass

internal class InternalClass

internal data class InternalData(val x: Int)

private sealed interface PrivateSealed
