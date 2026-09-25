# Development

## Build

```bash
dotnet build AnActualCartographer/AnActualCartographer.csproj -c Release
```

Output goes to `package/AnActualCartographer.dll`, which is committed — the publish workflow
ships that file rather than building on CI.

The project finds Valheim at a standard Windows Steam layout. Override it:

```bash
dotnet build AnActualCartographer/AnActualCartographer.csproj -c Release -p:ValheimFolder="D:\Steam\steamapps\common\Valheim"
```

The build fails with a clear message if the game's assemblies are not where it looked.

BepInEx and HarmonyX come from NuGet (`BepInEx.Core`, `HarmonyX`), not from an r2modman
profile. `BepInEx.Core` is only on BepInEx's own feed — that is what `NuGet.config` is for.

## Where the game code lives

Valheim's own classes are in **`valheim_Data/Managed/assembly_valheim.dll`**, not
`Assembly-CSharp.dll`. Decompiling the wrong one turns up nothing:

```bash
ilspycmd -t Minimap "X:/SteamLibrary/steamapps/common/Valheim/valheim_Data/Managed/assembly_valheim.dll"
```

## The three things this mod touches

`Minimap.m_exploreRadius` is a public float, default 100. `Minimap.UpdateExplore(float, Player)`
is **private** and is the only place vanilla reads it — every two seconds, from `Minimap.Update`,
and only once there is a live local player. A prefix there is handed that player, so the mod
never has to guess when the minimap, the profile and ZNet's world have all finished loading.

`Minimap.Explore(Vector3, float)` does `ceil(radius / m_pixelSize)` and walks that many whole
pixels. `m_pixelSize` is 64 and `m_textureSize` is 256. **This is the detail that matters:**
vanilla's 100m radius becomes 2 pixels, so the fog really lifts 128m. Anything that scales the
raw radius is off by that rounding — 5x reads as 4x. `CartographyState.Widen` scales the pixel
count instead.

`MapTable` wires its two switches in `Start` to the **private** `OnRead(Switch, Humanoid,
ItemDrop.ItemData)` and `OnWrite(Switch, Humanoid, ItemDrop.ItemData)`. Both funnel into a
four-argument `OnRead` overload, and patching that one would also catch both — but the two
switch callbacks are delegate targets, which Mono will not inline out from under a patch, and
they map one-to-one onto what the player can actually do at the table. Both bail when the
player is holding an item, so the patches ignore that case too.

`Player.m_customData` is a plain `Dictionary<string, string>` with no accessors, written in
`Player.Save` and read back in `Player.Load`. Unknown keys round-trip untouched, which makes it
the right place for a per-character flag. The key carries `ZNet.instance.GetWorldUID()` —
which dereferences the world with no null check of its own, so guard it.

## Checking patch targets still resolve

Three of the four targets are private, so a rename would compile fine and silently no-op.
`Plugin.Awake` logs a `Harmony skip` line per type when that happens. To check without
launching the game, load the assembly with `MetadataLoadContext` and resolve each target by
name and parameter types — note the game ships its own Mono `mscorlib` in `Managed`, so
resolve entirely inside that folder rather than mixing in the host's .NET.

## Testing

Install through r2modman so you exercise the real load order rather than hand-copying into a
profile.

Worth checking after any change:

- Build a table, use it, walk: fog should clear noticeably wider, and only after the use
- Log out and back in: still wide, with no second unlock message
- A second world: back to vanilla range until you build a table there
- `Enabled = false`: exactly vanilla range, and the unlock survives being turned back on
- Both switches on the table ("read map" and "save map") should each unlock it
- Interacting with an item in hand should not unlock it

Set `Advanced / VerboseLogging = true` for the unlock and the radius actually applied.

## Publish to Thunderstore

Pushes to `main` publish automatically. The workflow bumps the patch version if Thunderstore
already has the repo version.

Also available as **Actions → Publish to Thunderstore → Run workflow**, or tag `v1.x.y` and
push.

1. `thunderstore.toml` `namespace` must match the Thunderstore team (`LJIndustries`)
2. Repo secret: `THUNDERSTORE_API_KEY`
3. Commit the rebuilt `package/AnActualCartographer.dll`
