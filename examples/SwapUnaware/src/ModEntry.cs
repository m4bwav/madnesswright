using UnityEngine;

namespace SwapUnaware
{
    // Entry point named by EntryType in info.txt. The game calls Initialize() after
    // loading the DLL and Unload() when the mod is disabled.
    public class ModEntry : IModEntrypoint
    {
        public const string Id = "com.m4bwav.mpn.swapunaware";
        static GameObject host;

        public void Initialize()
        {
            if (host != null) return;
            host = new GameObject("SwapUnaware");
            Object.DontDestroyOnLoad(host);
            host.AddComponent<SwapUnawareBehaviour>();
            Debug.Log("[" + Id + "] loaded: F5 swap to the character under the mouse, F6 everyone else unaware, F7 keep them unaware, F8 back to your own body");
        }

        public void Unload()
        {
            if (host != null) Object.Destroy(host);
            host = null;
            Debug.Log("[" + Id + "] unloaded");
        }
    }
}
