using System.Globalization;
using UnityEngine;

namespace AnActualCartographer
{
    /// <summary>
    /// Owns the unlock flag and the explore radius derived from it.
    ///
    /// The unlock lives in <c>Player.m_customData</c>, a plain string dictionary that
    /// <c>Player.Save</c> and <c>Player.Load</c> serialise with the character, so it survives a
    /// logout without the mod adding a save file of its own. The key carries the world UID
    /// because map exploration is itself per world (see <c>PlayerProfile.GetMapData</c>) - a
    /// table built in one world should not widen the fog in a fresh one.
    /// </summary>
    internal static class CartographyState
    {
        private const string KeyPrefix = "AnActualCartographer.unlocked.";

        // Vanilla's serialised m_exploreRadius, captured before anything here writes to the
        // field. Tracked against the instance it came from: a world load builds a new Minimap
        // from the prefab, and that fresh instance is the only honest place to read the base
        // value again. If another mod got there first we take its value as the base and scale
        // from that, which is the polite way to stack.
        private static Minimap _capturedFrom;
        private static float _baseRadius;

        // IsUnlocked is asked once per frame from the UpdateExplore prefix, so the dictionary
        // lookup and the key string are cached against the player and world they were read for.
        private static Player _cachedPlayer;
        private static long _cachedWorldUid;
        private static bool _cachedUnlocked;

        private static float _lastApplied = -1f;

        /// <summary>
        /// True while the widened radius is actually in force. ExploreSkip uses this so it
        /// only ever short-circuits work this mod created.
        /// </summary>
        internal static bool IsWidening { get; private set; }

        /// <summary>
        /// Sets the radius vanilla is about to explore with. Cheap enough to call every frame:
        /// everything after the first call per player is a reference comparison and a multiply.
        /// </summary>
        internal static void ApplyTo(Minimap minimap, Player player)
        {
            if (minimap == null) return;

            float baseRadius = BaseRadiusFor(minimap);
            bool widen = ModConfig.Enabled.Value && IsUnlocked(player);
            IsWidening = widen;
            float radius = widen
                ? Widen(minimap, baseRadius, ModConfig.ExploreRadiusMultiplier.Value)
                : baseRadius;

            minimap.m_exploreRadius = radius;

            if (ModConfig.VerboseLogging.Value && !Mathf.Approximately(radius, _lastApplied))
            {
                Plugin.Log.LogInfo($"Explore radius {radius}m (vanilla {baseRadius}m, widened={widen}).");
            }

            _lastApplied = radius;
        }

        /// <summary>
        /// Vanilla explores in whole fog pixels: <c>Minimap.Explore</c> takes
        /// <c>ceil(radius / m_pixelSize)</c> and walks that many pixels out from the player. At
        /// the stock 100m radius and 64m pixels that ceiling turns 100 into 2 pixels, so the fog
        /// really lifts 128m, not 100m.
        ///
        /// Scaling the raw radius would therefore undersell the multiplier - 5x gives 500m,
        /// which ceils back to 8 pixels, or 512m, only 4x the distance the player watches
        /// clear. Scaling the pixel count instead makes 5x mean five times as far, which is
        /// what the config claims and what anyone reading it would measure.
        /// </summary>
        private static float Widen(Minimap minimap, float baseRadius, float multiplier)
        {
            float pixelSize = minimap.m_pixelSize;
            if (pixelSize <= 0f)
            {
                // Something has replaced the fog grid; scale the raw radius and let vanilla round.
                return baseRadius * multiplier;
            }

            int basePixels = Mathf.Max(1, Mathf.CeilToInt(baseRadius / pixelSize));
            int widenedPixels = Mathf.RoundToInt(basePixels * multiplier);

            // The whole map is m_textureSize pixels across, so there is nothing left to reveal
            // past that and Explore would only walk a bigger square for it.
            int maxPixels = Mathf.Max(basePixels, minimap.m_textureSize);
            widenedPixels = Mathf.Clamp(widenedPixels, basePixels, maxPixels);

            return widenedPixels * pixelSize;
        }

        /// <summary>
        /// Records that this player has used a cartography table in this world. Returns true
        /// only the first time, so the caller can announce it once.
        /// </summary>
        internal static bool Unlock(Player player)
        {
            if (player == null) return false;

            long worldUid = CurrentWorldUid();
            if (worldUid == 0L) return false;

            string key = KeyFor(worldUid);
            if (player.m_customData.ContainsKey(key)) return false;

            player.m_customData[key] = "1";
            InvalidateCache();

            if (ModConfig.VerboseLogging.Value)
            {
                Plugin.Log.LogInfo($"Cartography unlocked for world {worldUid}.");
            }

            return true;
        }

        internal static bool IsUnlocked(Player player)
        {
            if (player == null) return false;

            long worldUid = CurrentWorldUid();
            if (worldUid == 0L) return false;

            if (ReferenceEquals(_cachedPlayer, player) && _cachedWorldUid == worldUid)
            {
                return _cachedUnlocked;
            }

            _cachedPlayer = player;
            _cachedWorldUid = worldUid;
            _cachedUnlocked = player.m_customData.ContainsKey(KeyFor(worldUid));
            return _cachedUnlocked;
        }

        private static float BaseRadiusFor(Minimap minimap)
        {
            // ReferenceEquals rather than ==: a destroyed Minimap compares equal to null under
            // Unity's operator, and we want the plain "is this the same object" answer.
            if (!ReferenceEquals(_capturedFrom, minimap))
            {
                _capturedFrom = minimap;
                _baseRadius = minimap.m_exploreRadius;
                _lastApplied = -1f;
                InvalidateCache();
            }

            return _baseRadius;
        }

        private static string KeyFor(long worldUid)
        {
            return KeyPrefix + worldUid.ToString(CultureInfo.InvariantCulture);
        }

        private static long CurrentWorldUid()
        {
            // ZNet.GetWorldUID dereferences the world with no null check of its own, and the
            // world is absent in the main menu and for a moment during connect.
            ZNet znet = ZNet.instance;
            if (znet == null || ZNet.World == null) return 0L;

            return znet.GetWorldUID();
        }

        private static void InvalidateCache()
        {
            _cachedPlayer = null;
            _cachedWorldUid = 0L;
            _cachedUnlocked = false;
            ExploreSkip.Invalidate();
        }
    }
}
