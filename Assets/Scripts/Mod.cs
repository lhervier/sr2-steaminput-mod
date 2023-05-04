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
            LOGGER.Log("Initializing Mod");

            if( !SteamManager.Initialized ) {
                LOGGER.Log("Steam not detected. Unable to start the mod.");
                return;
            }
            LOGGER.Log("Steam is initialized");

            SteamInput.Init(false);
            LOGGER.Log("SteamInput is initialized");
           
            Game.Instance.SceneManager.SceneLoaded += OnSceneLoaded;
            Game.Instance.SceneManager.SceneLoading += OnSceneLoading;

            Game.Instance.SceneManager.SceneUnloaded += OnSceneUnloaded;
            Game.Instance.SceneManager.SceneUnloading += OnSceneUnloading;
            this.OnSceneLoaded(null, null);
            LOGGER.Log("OnSceneLoaded event set");
            
            LOGGER.Log("Mod initialized");
        }

        // <summary>
        //  Return the gameObject corresponding to the currently loaded Scene
        // </summary>
        private GameObject SelectGameObject(object sender) {
            if( sender == null ) {
                LOGGER.Log("No sender in event. Trying to use Game Loops..");
                GameLoop.GameLoopRegistrar loop = Game.Loop;
                if( loop.Designer != null ) {
                    LOGGER.Log("Designer loop available");
                    return loop.Designer.gameObject;
                } else {
                    LOGGER.Log("No Designer loop available");
                }
                if( loop.Flight != null ) {
                    LOGGER.Log("Flight loop available");
                    return loop.Flight.gameObject;
                } else {
                    LOGGER.Log("No Flight loop available");
                }
                if( loop.Generic != null ) {
                    LOGGER.Log("Generic loop available");
                    return loop.Generic.gameObject;
                } else {
                    LOGGER.Log("No Generic loop available");
                }
                
                LOGGER.Log("Unable to find GameLoop...");
                return null;
            }

            Assets.Scripts.Scenes.SceneManager manager = sender as Assets.Scripts.Scenes.SceneManager;
            return manager.gameObject;
        }

        // =============================================================

        public void OnSceneLoading(object sender, ModApi.Scenes.Events.SceneEventArgs args) {
            LOGGER.Log("Scene loading");
        }

        public void OnSceneLoaded(object sender, ModApi.Scenes.Events.SceneEventArgs args) {
            LOGGER.Log("Scene loaded");
            GameObject gameObject = this.SelectGameObject(sender);
            if( gameObject != null ) {
                mod = gameObject.AddComponent<SteamInputMod>();
            } else {
                mod = null;
            }
        }

        public void OnSceneUnloading(object sender, ModApi.Scenes.Events.SceneEventArgs args) {
            LOGGER.Log("Scene unloading");
            if( mod != null ) {
                GameObject.Destroy(mod);
            }
        }

        public void OnSceneUnloaded(object sender, ModApi.Scenes.Events.SceneEventArgs args) {
            LOGGER.Log("Scene unloaded");
        }
    }
}