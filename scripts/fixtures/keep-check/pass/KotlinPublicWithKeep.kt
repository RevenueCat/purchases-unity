package com.revenuecat.fixtures

import androidx.annotation.Keep

@Keep
class PublicClass

@Keep
data class PublicData(val x: Int)

@Keep
enum class PublicEnum { A, B }

@Keep
object PublicObject

@Keep
sealed interface PublicSealed

// Public class whose constructor is private: still a public type, @Keep present -> exempt.
@Keep
class CtorVisibilityWithKeep private constructor()
