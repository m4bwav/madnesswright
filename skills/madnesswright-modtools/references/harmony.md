# Harmony assemblies for Project Nexus

Source: official Modding Overview Part 8 (Harmony 2.4.2 support, beta v2.19.a, 2026-04-15); BepInEx docs on choosing a target framework (docs.bepinex.dev, plugin tutorial part 2); Harmony docs (harmony.pardeike.net). Read 2026-10-17.

## info.txt keys

| Key | Type | Meaning |
|---|---|---|
| AssemblyPath | string | DLL path relative to the mod folder, e.g. `Managed/MyMod.dll` |
| HarmonyID | string | unique patch id; blank derives one from the Workshop id (local test mods have none, so set it) |
| EntryType | string | fully qualified `Namespace.Class` implementing `IModEntrypoint` |
| PatchOrder | int | when this mod's patch applies relative to others |
| ManualPatch | bool | true stops the automatic `PatchAll()`; patch yourself in `Initialize()` |

```json
{
  "Name": "My Mod With Assembly",
  "Author": "Mystery Modder",
  "Description": "Contains an assembly!",
  "AssemblyPath": "Managed/MyMod.dll",
  "HarmonyID": "com.example.mympnmod",
  "EntryType": "MyMpnMod.ModEntry",
  "PatchOrder": 0,
  "ManualPatch": false
}
```

## Entry class

The interface's namespace is not in the docs: find `IModEntrypoint` in the decompiled `Assembly-CSharp` and add the matching `using`.

```csharp
using HarmonyLib;
using UnityEngine;

namespace MyMpnMod
{
    public class ModEntry : IModEntrypoint
    {
        public void Initialize() { Debug.Log("[com.example.mympnmod] loaded"); }
        public void Unload() { Debug.Log("[com.example.mympnmod] unloaded"); }
    }

    // Target names below are placeholders: replace them with a class and method
    // found in the decompiled Assembly-CSharp of the user's current build.
    [HarmonyPatch(typeof(SomeGameClass), "SomeMethod")]
    static class SomeMethodPatch
    {
        static void Postfix() { }
    }
}
```

## Project file

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>netstandard2.0</TargetFramework> <!-- or net46+/net472: read Managed/ first -->
    <LangVersion>latest</LangVersion>
    <GameManaged>$(MPN_MANAGED)</GameManaged> <!-- path to Madness Project Nexus_Data/Managed -->
  </PropertyGroup>
  <ItemGroup>
    <Reference Include="Assembly-CSharp"><HintPath>$(GameManaged)/Assembly-CSharp.dll</HintPath><Private>false</Private></Reference>
    <Reference Include="UnityEngine.CoreModule"><HintPath>$(GameManaged)/UnityEngine.CoreModule.dll</HintPath><Private>false</Private></Reference>
    <Reference Include="0Harmony"><HintPath>$(GameManaged)/0Harmony.dll</HintPath><Private>false</Private></Reference>
  </ItemGroup>
</Project>
```

Whether the game ships `0Harmony.dll` in `Managed/` is unconfirmed: check the folder. If it is not there, reference the `Lib.Harmony` 2.4.2 NuGet package with `ExcludeAssets="runtime"` so the game's own copy is used. Never ship game DLLs or decompiled code in a mod.

## Framework rule (BepInEx's, applies to any Unity Mono mod)

- `Managed/netstandard.dll` exists: `netstandard2.0`.
- Otherwise `mscorlib.dll` version 4.0.0.0 or later: `net46` (net472 also works with the 4.x reference assemblies).
- Older (2.0.0.0): `net35` (not expected for Unity 2019.2).
- Unity version: the game exe's file version or the first lines of Player.log (2019.2.6f1 expected, matching the tool projects).

## Failure modes

- "Could not find method" or "No target method specified": the name or signature is wrong for this build. Re-decompile; check overloads and argument types.
- Patch worked, then broke after a game update: the developer renamed or changed the method. Re-check every target after each beta.
- Mod loads but nothing happens: `EntryType` misspelt, class not public, `ManualPatch` true without patching in `Initialize()`.
- Two mods patch the same method: use `PatchOrder`, Harmony priorities, or a Postfix instead of a Prefix that skips the original.

## Tools

- ilspycmd (`dotnet tool install -g ilspycmd`) or ILSpy GUI; dnSpyEx for debugging.
- Optional MCP: mcilspy (PyPI, MIT, 2026-10-12), `claude mcp add ilspy -- python -m mcilspy.server`, gives search_methods, decompile_method and similar tools. The user installs it.
- universal-modder (github.com/rehan-remade/universal-modder) has a general game-modding workflow (read-only recon, reversible lab, verify in the running game) but nothing specific to this game.
