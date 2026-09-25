using System;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;

namespace AnActualCartographer
{
    /// <summary>
    /// Client-side only. Map fog is drawn from the local player's own explored bitmap and the
    /// unlock rides along in the character save, so nothing here has to reach the server and
    /// the mod does not have to match between players on a shared world.
    /// </summary>
    [BepInPlugin(ModGuid, ModName, ModVersion)]
    public class Plugin : BaseUnityPlugin
    {
        public const string ModGuid = "com.ljindustries.valheim.anactualcartographer";
        public const string ModName = "AnActualCartographer";
        public const string ModVersion = "1.1.0";

        internal static ManualLogSource Log;

        private Harmony _harmony;

        private void Awake()
        {
            Log = Logger;
            ModConfig.Bind(Config);

            // Patch per type so one method the game has renamed cannot abort every other patch.
            _harmony = new Harmony(ModGuid);
            foreach (var type in typeof(Plugin).Assembly.GetTypes())
            {
                try
                {
                    _harmony.CreateClassProcessor(type).Patch();
                }
                catch (Exception ex)
                {
                    Logger.LogError($"Harmony skip {type.FullName}: {ex.GetType().Name}: {ex.Message}");
                }
            }
        }

        private void OnDestroy()
        {
            _harmony?.UnpatchSelf();
        }
    }
}
