using BepInEx.Configuration;

namespace AnActualCartographer
{
    internal static class ModConfig
    {
        internal static ConfigEntry<bool> Enabled;
        internal static ConfigEntry<float> ExploreRadiusMultiplier;

        internal static ConfigEntry<bool> ShowUnlockMessage;
        internal static ConfigEntry<string> UnlockMessage;

        internal static ConfigEntry<bool> VerboseLogging;

        internal static void Bind(ConfigFile config)
        {
            Enabled = config.Bind(
                "Cartography Table", "Enabled", true,
                "Widen the fog-lifting radius once you have used a cartography table in this world. " +
                "Turn this off to play vanilla without uninstalling: while off the mod is inert, and " +
                "will not widen anything, announce anything, or record a new unlock. An unlock already " +
                "earned is kept and takes effect again when you turn this back on.");

            ExploreRadiusMultiplier = config.Bind(
                "Cartography Table", "ExploreRadiusMultiplier", 5f,
                new ConfigDescription(
                    "How much further the fog lifts once the table has been used. 5 means five " +
                    "times as far as vanilla, so the 128m the fog normally clears becomes 640m. " +
                    "Counted in whole fog-map pixels, which is how the game explores, so the " +
                    "multiplier lands on the distance you actually watch clear.",
                    new AcceptableValueRange<float>(1f, 50f)));

            ShowUnlockMessage = config.Bind(
                "Cartography Table", "ShowUnlockMessage", true,
                "Show a centre-screen message the first time you use a table in a world. " +
                "Shown once per world, not on every use.");

            UnlockMessage = config.Bind(
                "Cartography Table", "UnlockMessage", "You read the land with a cartographer's eye",
                "Text for that message.");

            VerboseLogging = config.Bind(
                "Advanced", "VerboseLogging", false,
                "Log the unlock and the resulting explore radius to the BepInEx console.");
        }
    }
}
