using System;
using System.Collections.Generic;

namespace RevenueCat.Tester.Screens
{
    public class AttributesScreen : ScreenBase
    {
        public AttributesScreen(Purchases purchases, LogConsole console)
            : base(purchases, console) { }

        protected override void Build()
        {
            AddSectionHeader("Quick Set All");

            AddButton("Set All Subscriber Attributes", () =>
            {
                var ts = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();

                Purchases.SetAttributes(new Dictionary<string, string>
                {
                    { "timestamp", ts },
                });
                Purchases.SetEmail($"email_{ts}@revenuecat.com");
                Purchases.SetPhoneNumber(ts);
                Purchases.SetDisplayName($"displayName_{ts}");
                Purchases.SetPushToken($"pushtoken_{ts}");
                Purchases.SetAdjustID($"adjustId_{ts}");
                Purchases.SetAppsflyerID($"appsflyerId_{ts}");
                Purchases.SetFBAnonymousID($"fbAnonymousId_{ts}");
                Purchases.SetMparticleID($"mparticleId_{ts}");
                Purchases.SetOnesignalID($"onesignalId_{ts}");
                Purchases.SetAirshipChannelID($"airshipChannelId_{ts}");
                Purchases.SetCleverTapID($"cleverTapID_{ts}");
                Purchases.SetMixpanelDistinctID($"mixpanelDistinctID_{ts}");
                Purchases.SetFirebaseAppInstanceID($"firebaseAppInstanceID_{ts}");
                Purchases.SetMediaSource($"mediaSource_{ts}");
                Purchases.SetCampaign($"campaign_{ts}");
                Purchases.SetAdGroup($"adgroup_{ts}");
                Purchases.SetAd($"ad_{ts}");
                Purchases.SetKeyword($"keyword_{ts}");
                Purchases.SetCreative($"creative_{ts}");

                LogSuccess($"All subscriber attributes set with timestamp {ts}");
            });

            AddSectionHeader("Custom Attribute");

            var keyField = AddTextField("Key", "attribute key");
            var valueField = AddTextField("Value", "attribute value");

            AddButton("Set Custom Attribute", () =>
            {
                var key = keyField.value;
                var val = valueField.value;
                if (string.IsNullOrWhiteSpace(key))
                {
                    LogError("Enter an attribute key");
                    return;
                }
                Purchases.SetAttributes(new Dictionary<string, string> { { key, val } });
                LogSuccess($"Set attribute \"{key}\" = \"{val}\"");
            });

            AddSectionHeader("Individual Attributes");

            var emailField = AddTextField("Email", "user@example.com");
            AddSecondaryButton("Set Email", () =>
            {
                Purchases.SetEmail(emailField.value);
                LogSuccess($"Email set to \"{emailField.value}\"");
            });

            var displayNameField = AddTextField("Display Name", "John Doe");
            AddSecondaryButton("Set Display Name", () =>
            {
                Purchases.SetDisplayName(displayNameField.value);
                LogSuccess($"Display name set to \"{displayNameField.value}\"");
            });

            var phoneField = AddTextField("Phone Number", "+1234567890");
            AddSecondaryButton("Set Phone Number", () =>
            {
                Purchases.SetPhoneNumber(phoneField.value);
                LogSuccess($"Phone number set to \"{phoneField.value}\"");
            });

            AddSectionHeader("Device");

            AddButton("Collect Device Identifiers", () =>
            {
                Purchases.CollectDeviceIdentifiers();
                LogSuccess("Device identifiers collected");
            });
        }
    }
}
