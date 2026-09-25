# Changelog

## 1.1.0

- **Default multiplier raised from 3x to 5x.** The fog now lifts 640m around you once the
  table has been used, up from 384m; 3x did not feel like enough of a reward for building one.
  Ten fog pixels instead of six, and still an exact multiple of the 128m vanilla clears.
- Only the *default* changed. A config file written by an earlier version keeps whatever
  `ExploreRadiusMultiplier` it already has — set it to 5 by hand, or delete the file and let it
  regenerate.

## 1.0.0

First release.

- **Using a cartography table widens the map fog.** Postfixes on the table's two switch
  callbacks, `MapTable.OnRead` and `MapTable.OnWrite`, so either way of using it counts. Both
  are delegate targets, which a patch can rely on in a way an inlineable private helper is not.
- **The unlock is per character, per world**, stored in `Player.m_customData` under a world-UID
  key. Valheim serialises that dictionary in `Player.Save`/`Player.Load`, so the mod adds no
  save file of its own and a fresh world starts at vanilla range.
- **The multiplier means what it says.** Vanilla explores whole 64m fog pixels, rounding its
  100m radius up to 128m, so scaling the raw radius by 3 would have produced 320m — 2.5x, not
  3x. The radius is scaled in pixels instead, landing exactly on 384m.
- The radius is set from a prefix on `Minimap.UpdateExplore`, the one place vanilla reads it,
  which is handed the local player. That sidesteps every question about the order in which the
  minimap, the character profile and the world finish loading.
- Vanilla's own radius is captured per `Minimap` instance before anything is written to it, so
  turning the mod off restores vanilla exactly, and a mod that sets its own radius first is
  scaled from rather than overwritten.
