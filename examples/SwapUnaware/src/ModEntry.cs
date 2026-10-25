using UnityEngine;

namespace SwapUnaware
{
    // Entry point named by EntryType in info.txt. The game calls Initialize() after
    // loading the DLL and Unload() when the mod is disabled.
    public class ModEntry : IModEntrypoint
    {
        public const string Id = "com.m4bwav.mpn.swapunaware";
        public const string Version = "0.8.0";
        static GameObject host;

        public void Initialize()
        {
            if (host != null) return;
            host = new GameObject("SwapUnaware");
            Object.DontDestroyOnLoad(host);
            // One component per feature: one that keeps throwing switches itself off, the rest go on.
            host.AddComponent<Hud>();
            host.AddComponent<SwapUnawareBehaviour>();
            host.AddComponent<FreezeFeature>();
            host.AddComponent<ClearFeature>();
            host.AddComponent<AlertFeature>();
            host.AddComponent<GuardFeature>();
            host.AddComponent<IdleAnimFeature>();
            host.AddComponent<PatrolFeature>();
            host.AddComponent<SceneFeature>();
            host.AddComponent<PlayersFeature>();
            host.AddComponent<DirectorMenu>();
            Debug.Log("[" + Id + "] Director " + Version + " loaded: INSERT menu, F1 clear, F2 freeze, F3 patrol, F4 idle animations, F5 take over, F6 unaware, F7 keep unaware, F8 home, F9 guard, F10 alert cycle, F11 scenes (paused)");
        }

        public void Unload()
        {
            if (host != null) Object.Destroy(host);
            host = null;
            Debug.Log("[" + Id + "] unloaded");
        }
    }
}
