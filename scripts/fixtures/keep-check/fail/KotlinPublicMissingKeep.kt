package com.revenuecat.fixtures

// Default-public Kotlin types without @Keep.
// Expected flagged: PublicData, PublicObj, PublicEnum
data class PublicData(val x: Int)

object PublicObj

enum class PublicEnum { A, B }
