# AnActualCartographer

Vanilla lifts the map fog in a fixed circle around you no matter how far into the game you
are. A cartography table lets you copy maps between players, and then does nothing else.

This mod makes the table mean something: once you have built one and used it, you map the
world five times as far as you used to, for as long as that character plays that world.

## How it works

Build a cartography table and interact with it — either switch, "read map" or "save map".
That is the whole unlock. From then on the fog lifts **640m** around you instead of the
usual **128m**.

The unlock is remembered **per character, per world**, and is saved with your character, so
it survives logging out. A new world starts back at vanilla range until you build a table
there too — the table you built in the Black Forest does not help you in the next seed.

## Why 128m and 640m, when vanilla says 100m?

The map's fog is a grid, one pixel per 64m, and the game explores whole pixels: it takes
vanilla's 100m radius, rounds it up to 2 pixels, and clears those. So the number in the
game's own files is 100m, but the distance you watch clear is 128m.

This mod counts in the same pixels the game does, so `ExploreRadiusMultiplier = 5` clears
10 pixels — 640m, exactly five times as far. Scaling the raw 100m instead would have landed
on 512m, which is only 4x, and the config would have been quietly lying to you.

## Configuration

`BepInEx/config/com.ljindustries.valheim.anactualcartographer.cfg`, written on first run.

| Setting | Default | What it does |
| --- | --- | --- |
| `Enabled` | `true` | Turn off for vanilla range without uninstalling. While off the mod is inert — it widens nothing, says nothing, and records no new unlocks. One you already earned is kept, and comes back when you turn this on again. |
| `ExploreRadiusMultiplier` | `5` | How much further the fog lifts after the table. `5` = five times as far (640m). Range 1–50. |
| `ShowUnlockMessage` | `true` | Centre-screen message the first time you use a table in a world. Once per world, not per use. |
| `UnlockMessage` | `You read the land with a cartographer's eye` | Text for that message. |
| `VerboseLogging` | `false` | Log the unlock and the resulting radius to the BepInEx console. |

Changes to the multiplier take effect immediately — no reload.

## Multiplayer

Client-side. The fog is drawn from your own explored map and the unlock rides along in your
character file, so the server does not need the mod and neither do the people you play with.
Nobody else is affected by your copy of it.

Sharing a map through the table works exactly as it always did: this changes how fast you
fill your own map in, not what the table copies.

## Install

Through a mod manager (r2modman, Thunderstore Mod Manager), or by hand: drop
`AnActualCartographer.dll` into `BepInEx/plugins/`.

Requires [BepInExPack Valheim](https://thunderstore.io/c/valheim/p/denikson/BepInExPack_Valheim/).
