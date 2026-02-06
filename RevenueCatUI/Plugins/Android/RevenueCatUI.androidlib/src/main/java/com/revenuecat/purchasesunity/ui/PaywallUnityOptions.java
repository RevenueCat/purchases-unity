package com.revenuecat.purchasesunity.ui;

import android.os.Parcel;
import android.os.Parcelable;

import androidx.annotation.Nullable;

public class PaywallUnityOptions implements Parcelable {
    @Nullable
    private final String offeringId;
    private final boolean shouldDisplayDismissButton;
    @Nullable
    private final String requiredEntitlementIdentifier;
    @Nullable
    private final String presentedOfferingContextJson;
    @Nullable
    private final String customVariablesJson;

    public PaywallUnityOptions(
            @Nullable String offeringId,
            boolean shouldDisplayDismissButton,
            @Nullable String requiredEntitlementIdentifier,
            @Nullable String presentedOfferingContextJson,
            @Nullable String customVariablesJson
    ) {
        this.offeringId = offeringId;
        this.shouldDisplayDismissButton = shouldDisplayDismissButton;
        this.requiredEntitlementIdentifier = requiredEntitlementIdentifier;
        this.presentedOfferingContextJson = presentedOfferingContextJson;
        this.customVariablesJson = customVariablesJson;
    }

    protected PaywallUnityOptions(Parcel in) {
        offeringId = in.readString();
        shouldDisplayDismissButton = in.readByte() != 0;
        requiredEntitlementIdentifier = in.readString();
        presentedOfferingContextJson = in.readString();
        customVariablesJson = in.readString();
    }

    @Nullable
    public String getOfferingId() {
        return offeringId;
    }

    public boolean getShouldDisplayDismissButton() {
        return shouldDisplayDismissButton;
    }

    @Nullable
    public String getRequiredEntitlementIdentifier() {
        return requiredEntitlementIdentifier;
    }

    @Nullable
    public String getPresentedOfferingContextJson() {
        return presentedOfferingContextJson;
    }

    @Nullable
    public String getCustomVariablesJson() {
        return customVariablesJson;
    }

    @Override
    public void writeToParcel(Parcel dest, int flags) {
        dest.writeString(offeringId);
        dest.writeByte((byte) (shouldDisplayDismissButton ? 1 : 0));
        dest.writeString(requiredEntitlementIdentifier);
        dest.writeString(presentedOfferingContextJson);
        dest.writeString(customVariablesJson);
    }

    @Override
    public int describeContents() {
        return 0;
    }

    public static final Creator<PaywallUnityOptions> CREATOR = new Creator<PaywallUnityOptions>() {
        @Override
        public PaywallUnityOptions createFromParcel(Parcel in) {
            return new PaywallUnityOptions(in);
        }

        @Override
        public PaywallUnityOptions[] newArray(int size) {
            return new PaywallUnityOptions[size];
        }
    };
}

