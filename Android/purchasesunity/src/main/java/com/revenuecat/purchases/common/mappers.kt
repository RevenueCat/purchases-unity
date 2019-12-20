package com.revenuecat.purchases.common

import com.android.billingclient.api.SkuDetails
import com.revenuecat.purchases.EntitlementInfo
import com.revenuecat.purchases.EntitlementInfos
import com.revenuecat.purchases.Offering
import com.revenuecat.purchases.Offerings
import com.revenuecat.purchases.Package
import com.revenuecat.purchases.PurchaserInfo
import org.json.JSONArray
import org.json.JSONObject

fun EntitlementInfo.map(): Map<String, Any?> =
    mapOf(
        "identifier" to this.identifier,
        "isActive" to this.isActive,
        "willRenew" to this.willRenew,
        "periodType" to this.periodType.name,
        "latestPurchaseDate" to latestPurchaseDate.time / 1000.0,
        "originalPurchaseDate" to originalPurchaseDate.time / 1000.0,
        "expirationDate" to this.expirationDate?.let { it.time / 1000.0 },
        "store" to this.store.name,
        "productIdentifier" to this.productIdentifier,
        "isSandbox" to this.isSandbox,
        "unsubscribeDetectedAt" to this.unsubscribeDetectedAt?.let { it.time / 1000.0 },
        "billingIssueDetectedAt" to this.billingIssueDetectedAt?.let { it.time / 1000.0 }
    )

fun EntitlementInfos.map(): Map<String, Any> =
    mapOf(
        "allKeys" to this.all.keys.toList(),
        "allValues" to this.all.map { it.value.map() }.toList(),
        "activeKeys" to this.active.keys.toList(),
        "activeValues" to this.active.map { it.value.map() }.toList()
    )


fun SkuDetails.map(): Map<String, Any?> =
    mapOf(
        "identifier" to sku,
        "description" to description,
        "title" to title,
        "price" to priceAmountMicros / 1000000.0,
        "priceString" to price,
        "currencyCode" to priceCurrencyCode
    ) + mapIntroPrice()

fun PurchaserInfo.map(): Map<String, Any?> =
    mapOf(
        "entitlements" to entitlements.map(),
        "activeSubscriptions" to activeSubscriptions.toList(),
        "allPurchasedProductIdentifiers" to allPurchasedSkus.toList(),
        "latestExpirationDate" to latestExpirationDate?.let { it.time / 1000.0 },
        "firstSeen" to firstSeen.time / 1000.0,
        "originalAppUserId" to originalAppUserId,
        "requestDate" to requestDate.time / 1000.0,
        "allExpirationDateKeys" to allExpirationDatesByProduct.keys.toList(),
        "allExpirationDateValues" to allExpirationDatesByProduct.values.map { it?.time?.div(1000.0) },
        "allPurchaseDateKeys" to allPurchaseDatesByProduct.keys.toList(),
        "allPurchaseDateValues" to allPurchaseDatesByProduct.values.map { it?.time?.div(1000.0) },
        "originalApplicationVersion" to null
    )

fun Offerings.map(): Map<String, Any?> =
    mapOf(
        "allKeys" to this.all.keys.toList(),
        "allValues" to this.all.map { it.value.map() }.toList(),
        "current" to this.current?.map()
    )

fun List<SkuDetails>.map(): List<Map<String, Any?>> = this.map { it.map() }

private fun Offering.map(): Map<String, Any?> =
    mapOf(
        "identifier" to identifier,
        "serverDescription" to serverDescription,
        "availablePackages" to availablePackages.map { it.map(identifier) },
        "lifetime" to lifetime?.map(identifier),
        "annual" to annual?.map(identifier),
        "sixMonth" to sixMonth?.map(identifier),
        "threeMonth" to threeMonth?.map(identifier),
        "twoMonth" to twoMonth?.map(identifier),
        "monthly" to monthly?.map(identifier),
        "weekly" to weekly?.map(identifier)
    )

private fun Package.map(offeringIdentifier: String): Map<String, Any?> =
    mapOf(
        "identifier" to identifier,
        "packageType" to packageType.name,
        "product" to product.map(),
        "offeringIdentifier" to offeringIdentifier
    )

private fun SkuDetails.mapIntroPrice(): Map<String, Any?> {
    return if (!freeTrialPeriod.isNullOrBlank()) {
        // Check freeTrialPeriod first to give priority to trials
        // Format using device locale. iOS will format using App Store locale, but there's no way
        // to figure out how the price in the SKUDetails is being formatted.
        val format = java.text.NumberFormat.getCurrencyInstance().apply {
            currency = java.util.Currency.getInstance(priceCurrencyCode)
        }
        mapOf(
            "introPrice" to 0,
            "introPriceString" to format.format(0),
            "introPricePeriod" to freeTrialPeriod,
            "introPriceCycles" to 1
        ) + freeTrialPeriod.mapPeriod()
    } else if (!introductoryPrice.isNullOrBlank()) {
        mapOf(
            "introPrice" to introductoryPriceAmountMicros / 1000000.0,
            "introPriceString" to introductoryPrice,
            "introPricePeriod" to introductoryPricePeriod,
            "introPriceCycles" to (introductoryPriceCycles?.takeUnless { it.isBlank() }?.toInt() ?: 0)
        ) + introductoryPricePeriod.mapPeriod()
    } else {
        mapOf(
            "introPrice" to null,
            "introPriceString" to null,
            "introPricePeriod" to null,
            "introPriceCycles" to null,
            "introPricePeriodUnit" to null,
            "introPricePeriodNumberOfUnits" to null
        )
    }
}

private fun String?.mapPeriod(): Map<String, Any?> {
    return if (this == null || this.isBlank()) {
        mapOf(
            "introPricePeriodUnit" to null,
            "introPricePeriodNumberOfUnits" to null
        )
    } else {
        PurchasesPeriod.parse(this).let { period ->
            when {
                period.years > 0 -> mapOf(
                    "introPricePeriodUnit" to "YEAR",
                    "introPricePeriodNumberOfUnits" to period.years
                )
                period.months > 0 -> mapOf(
                    "introPricePeriodUnit" to "MONTH",
                    "introPricePeriodNumberOfUnits" to period.months
                )
                period.days > 0 -> mapOf(
                    "introPricePeriodUnit" to "DAY",
                    "introPricePeriodNumberOfUnits" to period.days
                )
                else -> mapOf(
                    "introPricePeriodUnit" to "DAY",
                    "introPricePeriodNumberOfUnits" to 0
                )
            }
        }
    }
}

fun Map<String, *>.convertToJson(): JSONObject {
    val jsonObject = JSONObject()
    for ((key, value) in this) {
        when (value) {
            null -> jsonObject.put(key, JSONObject.NULL)
            is Map<*, *> -> jsonObject.put(key, (value as Map<String, *>).convertToJson())
            is List<*> -> jsonObject.put(key, value.convertToJsonArray())
            is Array<*> -> jsonObject.put(key, value.toList().convertToJsonArray())
            else -> jsonObject.put(key, value)
        }
    }
    return jsonObject
}

fun List<*>.convertToJsonArray(): JSONArray {
    val writableArray = JSONArray()
    for (item in this) {
        when (item) {
            null -> writableArray.put(JSONObject.NULL)
            is Map<*, *> -> writableArray.put((item as Map<String, *>).convertToJson())
            is Array<*> -> writableArray.put(item.asList().convertToJsonArray())
            is List<*> -> writableArray.put(item.convertToJsonArray())
            else -> writableArray.put(item)
        }
    }
    return writableArray
}
