using HarmonyLib;

namespace AnActualCartographer.Patches
{
    /// <summary>
    /// UpdateExplore is the one place vanilla reads m_exploreRadius, and it is handed the local
    /// player, so a prefix here can set the radius from that player's unlock with no dependence
    /// on when the minimap, the profile and ZNet's world finish loading relative to each other.
    /// Minimap.Update only calls it once it has a live local player, so player is never null.
    /// </summary>
    [HarmonyPatch(typeof(Minimap), "UpdateExplore", new[] { typeof(float), typeof(Player) })]
    internal static class MinimapUpdateExplorePatch
    {
        private static void Prefix(Minimap __instance, Player player)
        {
            CartographyState.ApplyTo(__instance, player);
        }
    }
}
