package com.revenuecat.purchases.common

import com.android.billingclient.api.SkuDetails
import com.revenuecat.purchases.EntitlementInfo
import com.revenuecat.purchases.EntitlementInfos
import com.revenuecat.purchases.Offering
import com.revenuecat.purchases.Offerings
import com.revenuecat.purchases.Package
import com.revenuecat.purchases.PurchaserInfo
import com.revenuecat.purchases.util.Iso8601Utils
import org.json.JSONArray
import org.json.JSONObject
import java.text.NumberFormat
import java.util.Currency
import java.util.Date

fun EntitlementInfo.map(): Map<String, Any?> =
    mapOf(
        "identifier" to this.identifier,
        "isActive" to this.isActive,
        "willRenew" to this.willRenew,
        "periodType" to this.periodType.name,
        "latestPurchaseDateMillis" to this.latestPurchaseDate.toMillis(),
        "latestPurchaseDate" to this.latestPurchaseDate.toIso8601(),
        "originalPurchaseDateMillis" to this.originalPurchaseDate.toMillis(),
        "originalPurchaseDate" to this.originalPurchaseDate.toIso8601(),
        "expirationDateMillis" to this.expirationDate?.toMillis(),
        "expirationDate" to this.expirationDate?.toIso8601(),
        "store" to this.store.name,
        "productIdentifier" to this.productIdentifier,
        "isSandbox" to this.isSandbox,
        "unsubscribeDetectedAt" to this.unsubscribeDetectedAt?.toIso8601(),
        "unsubscribeDetectedAtMillis" to this.unsubscribeDetectedAt?.toMillis(),
        "billingIssueDetectedAt" to this.billingIssueDetectedAt?.toIso8601(),
        "billingIssueDetectedAtMillis" to this.billingIssueDetectedAt?.toMillis()
    )

fun EntitlementInfos.map(): Map<String, Any> =
    mapOf(
        "all" to this.all.asIterable().associate { it.key to it.value.map() },
        "active" to this.active.asIterable().associate { it.key to it.value.map() }
    )

fun SkuDetails.map(): Map<String, Any?> =
    mapOf(
        "identifier" to sku,
        "description" to description,
        "title" to title,
        "price" to priceAmountMicros / 1000000.0,
        "price_string" to price,
        "currency_code" to priceCurrencyCode,
        "introPrice" to mapIntroPrice(),
        "discounts" to null
    ) + mapIntroPriceDeprecated()

fun PurchaserInfo.map(): Map<String, Any?> =
    mapOf(
        "entitlements" to entitlements.map(),
        "activeSubscriptions" to activeSubscriptions.toList(),
        "allPurchasedProductIdentifiers" to allPurchasedSkus.toList(),
        "latestExpirationDate" to latestExpirationDate?.toIso8601(),
        "latestExpirationDateMillis" to latestExpirationDate?.toMillis(),
        "firstSeen" to firstSeen.toIso8601(),
        "firstSeenMillis" to firstSeen.toMillis(),
        "originalAppUserId" to originalAppUserId,
        "requestDate" to requestDate.toIso8601(),
        "requestDateMillis" to requestDate.toMillis(),
        "allExpirationDates" to allExpirationDatesByProduct.mapValues { it.value?.toIso8601() },
        "allExpirationDatesMillis" to allExpirationDatesByProduct.mapValues { it.value?.toMillis() },
        "allPurchaseDates" to allPurchaseDatesByProduct.mapValues { it.value?.toIso8601() },
        "allPurchaseDatesMillis" to allPurchaseDatesByProduct.mapValues { it.value?.toMillis() },
        "originalApplicationVersion" to null
    )

fun Offerings.map(): Map<String, Any?> =
    mapOf(
        "all" to this.all.mapValues { it.value.map() },
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

private fun SkuDetails.mapIntroPriceDeprecated(): Map<String, Any?> {
    val isFreeTrialAvailable = freeTrialPeriod != null && freeTrialPeriod.isNotBlank()
    return if (isFreeTrialAvailable) {
        val format = formatUsingDeviceLocale()
        mapOf(
            "intro_price" to 0,
            "intro_price_string" to format.format(0),
            "intro_price_period" to freeTrialPeriod,
            "intro_price_cycles" to 1
        ) + freeTrialPeriod.mapPeriodDeprecated()
    } else if (introductoryPrice != null || introductoryPrice.isNotBlank()) {
        mapOf(
            "intro_price" to introductoryPriceAmountMicros / 1000000.0,
            "intro_price_string" to introductoryPrice,
            "intro_price_period" to introductoryPricePeriod,
            "intro_price_cycles" to introductoryPriceCycles?.ifBlank { "0" }?.toInt()
        ) + introductoryPricePeriod.mapPeriodDeprecated()
    } else {
        mapOf(
            "intro_price" to null,
            "intro_price_string" to null,
            "intro_price_period" to null,
            "intro_price_cycles" to null
        ) + introductoryPricePeriod.mapPeriodDeprecated()
    }
}

private fun SkuDetails.formatUsingDeviceLocale(): NumberFormat {
    return NumberFormat.getCurrencyInstance().apply {
        currency = Currency.getInstance(priceCurrencyCode)
    }
}

private fun SkuDetails.mapIntroPrice(): Map<String, Any?> {
    return if (freeTrialPeriod != null && freeTrialPeriod.isNotBlank()) {
        // Check freeTrialPeriod first to give priority to trials
        // Format using device locale. iOS will format using App Store locale, but there's no way
        // to figure out how the price in the SKUDetails is being formatted.
        val format = java.text.NumberFormat.getCurrencyInstance().apply {
            currency = java.util.Currency.getInstance(priceCurrencyCode)
        }
        mapOf(
            "price" to 0,
            "priceString" to format.format(0),
            "period" to freeTrialPeriod,
            "cycles" to 1
        ) + freeTrialPeriod.mapPeriod()
    } else if (introductoryPrice != null && introductoryPrice.isNotBlank()) {
        mapOf(
            "price" to introductoryPriceAmountMicros / 1000000.0,
            "priceString" to introductoryPrice,
            "period" to introductoryPricePeriod,
            "cycles" to introductoryPriceCycles?.ifBlank { "0" }?.toInt()
        ) + introductoryPricePeriod.mapPeriod()
    } else {
        mapOf(
            "price" to null,
            "priceString" to null,
            "period" to null,
            "cycles" to null
        ) + introductoryPricePeriod.mapPeriod()
    }
}

private fun String?.mapPeriodDeprecated(): Map<String, Any?> {
    return if (this == null || this.isBlank()) {
        mapOf(
            "intro_price_period_unit" to null,
            "intro_price_period_number_of_units" to null
        )
    } else {
        PurchasesPeriod.parse(this).let { period ->
            when {
                period.years > 0 -> mapOf(
                    "intro_price_period_unit" to "YEAR",
                    "intro_price_period_number_of_units" to period.years
                )
                period.months > 0 -> mapOf(
                    "intro_price_period_unit" to "MONTH",
                    "intro_price_period_number_of_units" to period.months
                )
                period.days > 0 -> mapOf(
                    "intro_price_period_unit" to "DAY",
                    "intro_price_period_number_of_units" to period.days
                )
                else -> mapOf(
                    "intro_price_period_unit" to "DAY",
                    "intro_price_period_number_of_units" to 0
                )
            }
        }
    }
}

private fun String?.mapPeriod(): Map<String, Any?> {
    return if (this == null || this.isBlank()) {
        mapOf(
            "periodUnit" to null,
            "periodNumberOfUnits" to null
        )
    } else {
        PurchasesPeriod.parse(this).let { period ->
            when {
                period.years > 0 -> mapOf(
                    "periodUnit" to "YEAR",
                    "periodNumberOfUnits" to period.years
                )
                period.months > 0 -> mapOf(
                    "periodUnit" to "MONTH",
                    "periodNumberOfUnits" to period.months
                )
                period.days > 0 -> mapOf(
                    "periodUnit" to "DAY",
                    "periodNumberOfUnits" to period.days
                )
                else -> mapOf(
                    "periodUnit" to "DAY",
                    "periodNumberOfUnits" to 0
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

fun JSONObject.convertToMap(): Map<String, String?> =
    this.keys().asSequence<String>().associate { key ->
        if (this.isNull(key)) {
            key to null
        } else {
            key to this.getString(key)
        }
    }

private fun Date.toMillis(): Double = this.time.div(1000.0)

private fun Date.toIso8601(): String = Iso8601Utils.format(this)