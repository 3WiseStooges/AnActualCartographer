using HarmonyLib;
using UnityEngine;

namespace AnActualCartographer.Patches
{
    /// <summary>
    /// Explore(Vector3, float) has exactly one caller - UpdateExplore - so taking it over when
    /// there is provably nothing to reveal affects nothing else. Returning false skips the
    /// original; the method returns void, so there is no result to fake.
    /// </summary>
    [HarmonyPatch(typeof(Minimap), "Explore", new[] { typeof(Vector3), typeof(float) })]
    internal static class MinimapExplorePatch
    {
        private static bool Prefix(Minimap __instance, Vector3 p, float radius)
        {
            return !ExploreSkip.CanSkip(__instance, p, radius);
        }
    }
}
