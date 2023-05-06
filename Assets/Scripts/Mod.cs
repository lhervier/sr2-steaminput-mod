using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ModApi;
using ModApi.Common;
using ModApi.Mods;
using ModApi.Scenes;
using UnityEngine;
using Steamworks;

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
        //  Logger
        // </summary>
        private static SteamInputLogger LOGGER = new SteamInputLogger();
        
        // <summary>
        // The main mod as a Unity component
        // </summary>
        private static SteamInputMod mod;

        // <summary>
        // Mod initialisation
        // </summary>
        protected override void OnModInitialized() {
            base.OnModInitialized();
            LOGGER.Debug("Initializing Mod");
            mod = new GameObject("SteamInputMod").AddComponent<SteamInputMod>();

            LOGGER.Debug("Mod initialized");
        }
    }
}