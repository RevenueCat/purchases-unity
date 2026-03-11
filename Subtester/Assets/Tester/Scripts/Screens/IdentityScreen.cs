using System;

namespace RevenueCat.Tester.Screens
{
    public class IdentityScreen : ScreenBase
    {
        public IdentityScreen(Purchases purchases, LogConsole console)
            : base(purchases, console) { }

        protected override void Build()
        {
            AddSectionHeader("Log In");

            var userIdField = AddTextField("App User ID", "Enter custom user ID...");

            AddButton("Log In", () =>
            {
                var userId = userIdField.value;
                if (string.IsNullOrWhiteSpace(userId))
                {
                    LogError("Enter a user ID first");
                    return;
                }

                Log($"Logging in as \"{userId}\"...");
                Purchases.LogIn(userId, (info, created, error) =>
                {
                    if (error != null) { LogError(error); return; }
                    LogSuccess($"Logged in — created: {created}");
                    Log(info.ToString());
                });
            });

            AddButtonRow(
                ("Log In as \"test\"", () =>
                {
                    Log("Logging in as \"test\"...");
                    Purchases.LogIn("test", (info, created, error) =>
                    {
                        if (error != null) { LogError(error); return; }
                        LogSuccess($"Logged in as \"test\" — created: {created}");
                    });
                }),
                ("Log In as Random ID", () =>
                {
                    var randomId = Guid.NewGuid().ToString();
                    Log($"Logging in as \"{randomId}\"...");
                    Purchases.LogIn(randomId, (info, created, error) =>
                    {
                        if (error != null) { LogError(error); return; }
                        LogSuccess($"Logged in as random ID — created: {created}");
                    });
                })
            );

            AddSectionHeader("Log Out");

            AddButton("Log Out", () =>
            {
                Log("Logging out...");
                Purchases.LogOut((info, error) =>
                {
                    if (error != null) { LogError(error); return; }
                    LogSuccess("Logged out");
                    Log(info.ToString());
                });
            });

            AddSectionHeader("Status");

            AddButtonRow(
                ("Get App User ID", () =>
                {
                    var id = Purchases.GetAppUserId();
                    Log($"App User ID: {id}");
                }),
                ("Is Anonymous", () =>
                {
                    var anon = Purchases.IsAnonymous();
                    Log($"Is anonymous: {anon}");
                })
            );

            AddSecondaryButton("Is Configured", () =>
            {
                var configured = Purchases.IsConfigured();
                Log($"Is configured: {configured}");
            });
        }
    }
}
