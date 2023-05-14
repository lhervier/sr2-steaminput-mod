using ModApi.Mods;
using UnityEngine;

namespace Assets.Scripts {

    // <summary>
    // A singleton object representing this mod that is instantiated and initialize when the mod is loaded.
    // </summary>
    public class Mod : GameMod {
        
        // <summary>
        // Prevents a default instance of the <see cref="Mod"/> class from being created.
        // </summary>
        private Mod() : base() {
        }

        // <summary>
        // Gets the singleton instance of the mod object.
        // </summary>
        // <value>The singleton instance of the mod object.</value>
        public static Mod Instance { get; } = GetModInstance<Mod>();

        // <summary>
        // Mod initialisation
        // </summary>
        protected override void OnModInitialized() {
            base.OnModInitialized();
            new GameObject("SteamInputMod").AddComponent<SteamInputMod>();
        }
    }
}