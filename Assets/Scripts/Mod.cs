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
            Game.Instance.SceneManager.SceneUnloading += OnSceneUnloading;
            this.OnSceneLoaded(null, null);
            LOGGER.Log("OnSceneLoaded event set");
            
            LOGGER.Log("Mod initialized");
        }

        
        private GameObject SelectGameObject() {
            GameLoop.GameLoopRegistrar loop = Game.Loop;
            LOGGER.Log("In menu scene ? " + Game.InMenuScene);
            LOGGER.Log("In designer scene ? " + Game.InDesignerScene);
            LOGGER.Log("In flight scene ? " + Game.InFlightScene);
            LOGGER.Log("In planet studio scene ? " + Game.InPlanetStudioScene);
            
            if( loop.Designer != null ) {
                LOGGER.Log("Designer loop available");
                LOGGER.Log("Designer loop game object available ? " + (loop.Designer.gameObject != null));
            } else {
                LOGGER.Log("No Designer loop available");
            }
            if( loop.Flight != null ) {
                LOGGER.Log("Flight loop available");
                LOGGER.Log("Flight loop game object available ? " + (loop.Flight.gameObject != null));
            } else {
                LOGGER.Log("No Flight loop available");
            }
            if( loop.Generic != null ) {
                LOGGER.Log("Generic loop available");
                LOGGER.Log("Generic loop game object available ? " + (loop.Generic.gameObject != null));
            } else {
                LOGGER.Log("No Generic loop available");
            }

            if( Game.InDesignerScene ) {
                return loop.Designer.gameObject;
            } else if( Game.InFlightScene ) {
                return loop.Flight.gameObject;
            } else {
                return loop.Generic.gameObject;            // NPE !!!
            }
        }

        public void OnSceneLoaded(object sender, ModApi.Scenes.Events.SceneEventArgs args) {
            GameObject gameObject = this.SelectGameObject();
            mod = gameObject.AddComponent<SteamInputMod>();
        }

        public void OnSceneUnloading(object sender, ModApi.Scenes.Events.SceneEventArgs args) {
            GameObject.Destroy(mod);
        }
    }
}