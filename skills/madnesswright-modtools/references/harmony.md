# Harmony assemblies for Project Nexus

Source: official Modding Overview Part 8 (Harmony 2.4.2 support, beta v2.19.a, 2026-04-15); the game's own loader, read from a decompile of `Assembly-CSharp.dll` (release build 25615280, which carries the beta 2.2 loader, 2026-10-17); BepInEx docs on choosing a target framework (docs.bepinex.dev, plugin tutorial part 2); Harmony docs (harmony.pardeike.net). Read 2026-10-17.

## info.txt keys

| Key | Type | Meaning |
|---|---|---|
| AssemblyPath | string | DLL path relative to the mod folder, e.g. `Managed/MyMod.dll` |
| HarmonyID | string | unique patch id (spaces become `_`); blank gives `workshop.<id>` for a Workshop item and a new `local.<guid>` every launch for a local mod, so set it |
| EntryType | string | the class's full name as `Assembly.GetType` sees it (`Namespace.Class`, or `Class` in the global namespace), public, implementing `IModEntrypoint`, with a parameterless constructor |
| PatchOrder | int | patches apply in ascending PatchOrder and are removed in descending order |
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

`IModEntrypoint` sits in the global namespace of `Assembly-CSharp` (no `using` needed), with two members: `void Initialize()` and `void Unload()` (build 25615280; re-check after updates). The game ships a sample entry class, `HarmonyExample.ExampleClass`, in the same assembly.

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
    <TargetFramework>net472</TargetFramework> <!-- build 25615280 has no Managed/netstandard.dll; re-check Managed/ -->
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

The game ships `0Harmony.dll` 2.4.2.0 in `Managed/` (seen on build 25615280, even on the release branch): reference that copy with `Private=false`. Building without the game installed, reference the `Lib.Harmony` 2.4.2 NuGet package with `ExcludeAssets="runtime"` instead, so the game's own copy is used. Never ship game DLLs or decompiled code in a mod.

Other DLLs your mod needs go beside it: the game resolves a missing reference first from assemblies already loaded, then from `<AssemblyPath folder>/<name>.dll`. The loader counts every `.dll` under that folder and shows one club mark per DLL on the loading screen.

## Framework rule (BepInEx's, applies to any Unity Mono mod)

- `Managed/netstandard.dll` exists: `netstandard2.0`.
- Otherwise `mscorlib.dll` version 4.0.0.0 or later: `net46` (net472 also works with the 4.x reference assemblies).
- Older (2.0.0.0): `net35` (not expected for Unity 2019.2).
- Unity version: the first lines of Player.log (`Initialize engine version: 2019.2.6f1 (fe82a0e88406)` on build 25615280, matching the tool projects).
- Seen on build 25615280: no `netstandard.dll`, `mscorlib.dll` present, so `net472` (the Unity 2019.2 .NET 4.x profile); the decompiler reports `net40` for `Assembly-CSharp` and `net48` for `0Harmony`, both of which load into that runtime.

## Failure modes

- "Could not find method" or "No target method specified": the name or signature is wrong for this build. Re-decompile; check overloads and argument types.
- Patch worked, then broke after a game update: the developer renamed or changed the method. Re-check every target after each beta.
- Mod loads but nothing happens: `EntryType` misspelt (mods.log: `FAIL: Entrypoint "<type>" Not Found`), the class does not implement the interface (`does not implement IModEntrypoint`), or `ManualPatch` true without patching in `Initialize()`. A blank `EntryType` is allowed: `PatchAll()` runs and no entry class is created.
- What the log says: `Mods/mods.log` gets `> APPLIED HARMONY PATCH: <HarmonyID>` on success and `! ERROR: Harmony Patch <HarmonyID> Failed!` on failure, with the exception in Player.log. A missing DLL logs `HARMONY FAIL: Path <path> not found.`
- An exception in `Initialize()` counts as a failure: the game calls `Unload()`, unpatches everything under your HarmonyID and marks the mod inactive. Disabling the mod (Apply) also calls `Unload()` and `UnpatchAll(HarmonyID)`, so `Unload()` must undo anything `Initialize()` started.
- Two mods patch the same method: use `PatchOrder`, Harmony priorities, or a Postfix instead of a Prefix that skips the original.

## Tools

- ilspycmd (`dotnet tool install -g ilspycmd`) or ILSpy GUI; dnSpyEx for debugging.
- Optional MCP: mcilspy (PyPI, MIT, 2026-10-12), `claude mcp add ilspy -- python -m mcilspy.server`, gives search_methods, decompile_method and similar tools. The user installs it.
- universal-modder (github.com/rehan-remade/universal-modder) has a general game-modding workflow (read-only recon, reversible lab, verify in the running game) but nothing specific to this game.
