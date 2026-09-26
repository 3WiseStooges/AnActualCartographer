using System;
using System.Collections;
using HarmonyLib;
using UnityEngine;

namespace AnActualCartographer
{
    /// <summary>
    /// Vanilla re-walks the entire explore circle every <c>m_exploreInterval</c> (2s) whether or
    /// not anything can have changed. At vanilla's 2-pixel radius that is 25 cells and nobody
    /// notices. At the radius this mod hands it the walk is thousands of cells, and the whole
    /// cost lands on one frame every two seconds - which is exactly what a 2-second hitch is.
    ///
    /// Standing still, that work is provably wasted. <c>Minimap.Explore</c> derives its circle
    /// from the player's fog pixel and the radius alone, so the same pixel at the same radius
    /// covers exactly the same cells, and the previous tick already explored every one of them.
    /// This skips the call outright in that case, which costs nothing and reveals nothing less.
    /// </summary>
    internal static class ExploreSkip
    {
        private static AccessTools.FieldRef<Minimap, BitArray> _exploredRef;
        private static bool _resolved;

        private static int _lastPx = int.MinValue;
        private static int _lastPy = int.MinValue;
        private static int _lastNum = -1;

        internal static void Invalidate()
        {
            _lastPx = int.MinValue;
            _lastPy = int.MinValue;
            _lastNum = -1;
        }

        internal static bool CanSkip(Minimap minimap, Vector3 p, float radius)
        {
            if (minimap == null) return false;

            // Only ever skip work this mod is responsible for creating. At vanilla's radius the
            // walk is 25 cells, and silently changing that for everyone else buys nothing.
            if (!CartographyState.IsWidening) return false;

            BitArray explored = Explored(minimap);
            if (explored == null) return false;

            float pixelSize = minimap.m_pixelSize;
            if (pixelSize <= 0f) return false;

            // Deliberately the same arithmetic as Minimap.Explore and Minimap.WorldToPixel,
            // down to Utils.RoundToInt - Mathf.RoundToInt rounds halves to even and would
            // disagree with vanilla on exact boundaries.
            int num = (int)Mathf.Ceil(radius / pixelSize);
            int half = minimap.m_textureSize / 2;
            int px = Utils.RoundToInt(p.x / pixelSize + (float)half);
            int py = Utils.RoundToInt(p.z / pixelSize + (float)half);

            bool unchanged = px == _lastPx && py == _lastPy && num == _lastNum;

            _lastPx = px;
            _lastPy = py;
            _lastNum = num;

            if (!unchanged) return false;

            // Resetting or reloading the map clears m_explored without the player moving, so
            // confirm the centre really is still explored rather than trusting position alone.
            int centre = py * minimap.m_textureSize + px;
            if (centre < 0 || centre >= explored.Length) return false;

            return explored[centre];
        }

        private static BitArray Explored(Minimap minimap)
        {
            if (!_resolved)
            {
                _resolved = true;
                try
                {
                    _exploredRef = AccessTools.FieldRefAccess<Minimap, BitArray>("m_explored");
                }
                catch (Exception ex)
                {
                    _exploredRef = null;
                    Plugin.Log.LogWarning(
                        $"Minimap.m_explored not found ({ex.GetType().Name}); explore-skip disabled, " +
                        "the map still works but large radii will cost more.");
                }
            }

            return _exploredRef?.Invoke(minimap);
        }
    }
}
