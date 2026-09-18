using HarmonyLib;

namespace AnActualCartographer.Patches
{
    /// <summary>
    /// The cartography table's two switches - "read map" and "save map" - are wired in
    /// MapTable.Start to these methods. Patching the switch callbacks rather than the private
    /// four-argument OnRead they both funnel into means exactly one postfix per interaction,
    /// and a delegate target is never inlined out from under a patch.
    /// </summary>
    internal static class MapTableUse
    {
        internal static void Handle(Humanoid user, ItemDrop.ItemData item)
        {
            // Switched off means inert: nothing widened, nothing announced, and nothing new
            // written into the character save. An unlock already earned stays where it is.
            if (!ModConfig.Enabled.Value) return;

            // Vanilla returns immediately when the player is holding something, so this was an
            // attempt to use an item on the table rather than a look at the map.
            if (item != null) return;

            Player player = Player.m_localPlayer;
            if (player == null || user != player) return;

            if (!CartographyState.Unlock(player)) return;

            if (ModConfig.ShowUnlockMessage.Value)
            {
                player.Message(MessageHud.MessageType.Center, ModConfig.UnlockMessage.Value);
            }
        }
    }

    [HarmonyPatch(typeof(MapTable), "OnRead", new[] { typeof(Switch), typeof(Humanoid), typeof(ItemDrop.ItemData) })]
    internal static class MapTableReadPatch
    {
        private static void Postfix(Humanoid user, ItemDrop.ItemData item)
        {
            MapTableUse.Handle(user, item);
        }
    }

    [HarmonyPatch(typeof(MapTable), "OnWrite", new[] { typeof(Switch), typeof(Humanoid), typeof(ItemDrop.ItemData) })]
    internal static class MapTableWritePatch
    {
        private static void Postfix(Humanoid user, ItemDrop.ItemData item)
        {
            MapTableUse.Handle(user, item);
        }
    }
}
