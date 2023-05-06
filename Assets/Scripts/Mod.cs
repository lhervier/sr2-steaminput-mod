using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ModApi;
using ModApi.Common;
using ModApi.Mods;
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

            if( !SteamManager.Initialized ) {
                LOGGER.Fatal("Steam not detected. Unable to start the mod.");
                return;
            }
            LOGGER.Debug("Steam is initialized");

            SteamInput.Init(false);
            LOGGER.Debug("SteamInput is initialized");
           
            Game.Instance.SceneManager.SceneLoaded += OnSceneLoaded;
            Game.Instance.SceneManager.SceneUnloading += OnSceneUnloading;
            this.OnSceneLoaded(null, null);
            LOGGER.Debug("Events binded");
            
            LOGGER.Debug("Mod initialized");
        }

        // <summary>
        //  Return the gameObject corresponding to the currently loaded Scene
        // </summary>
        private GameObject SelectGameObject(object sender) {
            if( sender == null ) {
                LOGGER.Debug("No sender in event. Trying to use Game Loops..");
                GameLoop.GameLoopRegistrar loop = Game.Loop;
                if( loop.Designer != null ) {
                    LOGGER.Debug("Designer loop available");
                    return loop.Designer.gameObject;
                } else {
                    LOGGER.Debug("No Designer loop available");
                }
                if( loop.Flight != null ) {
                    LOGGER.Debug("Flight loop available");
                    return loop.Flight.gameObject;
                } else {
                    LOGGER.Debug("No Flight loop available");
                }
                if( loop.Generic != null ) {
                    LOGGER.Debug("Generic loop available");
                    return loop.Generic.gameObject;
                } else {
                    LOGGER.Debug("No Generic loop available");
                }
                
                LOGGER.Error("Unable to find GameLoop...");
                return null;
            }

            Assets.Scripts.Scenes.SceneManager manager = sender as Assets.Scripts.Scenes.SceneManager;
            return manager.gameObject;
        }

        // =============================================================

        public void OnSceneLoaded(object sender, ModApi.Scenes.Events.SceneEventArgs args) {
            LOGGER.Debug("Scene loaded");
            GameObject gameObject = this.SelectGameObject(sender);
            if( gameObject != null ) {
                mod = gameObject.AddComponent<SteamInputMod>();
            } else {
                mod = null;
            }
        }

        public void OnSceneUnloading(object sender, ModApi.Scenes.Events.SceneEventArgs args) {
            LOGGER.Debug("Scene unloading");
            if( mod != null ) {
                GameObject.Destroy(mod);
            }
        }
    }
}