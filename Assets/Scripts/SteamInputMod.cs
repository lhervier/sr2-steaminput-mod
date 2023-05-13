using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Steamworks;
using ModApi.Craft;
using ModApi.Craft.Parts;
using ModApi.GameLoop;
using ModApi.Scenes.Events;

namespace Assets.Scripts {

    public class SteamInputMod : MonoBehaviourBase {

        // <summary>
        //  Logger
        // </summary>
        private static SteamInputLogger LOGGER = new SteamInputLogger("MainMod");
        
        // <summary>
        //  Delay before applying an action set (in frames)
        // </summary>
        private static int DELAY = 10;
        
        // ==================================================================================

        // <summary>
        //  Previous action set (so we don't display the message when the value has not changed)
        // </summary>
        private EActionSets prevActionSet = EActionSets.NotSet;

        // <summary>
        //  Connection Daemon to the controllers
        // </summary>
        private ControllerDaemon controllerDaemon;
        
        // <summary>
        //  Delayed Action daemon
        // </summary>
        private DelayedActionDaemon delayedActionDaemon;

        // <summary>
        //  The Vizzy Controller to detect open and close events
        // </summary>
        private VizzyController vizzyController;

        // ===============================================================================
        //                      Unity initialization
        // ===============================================================================

        // <summary>
        //  Component awaked
        // </summary>
        protected void Awake() {
            LOGGER.Debug("Awaking");
            DontDestroyOnLoad(this);
            LOGGER.Debug("Awaked");
        }

        // <summary>
        //  Start of the plugin
        // </summary>
        protected void Start() {
            LOGGER.Debug("Starting");
            
            if( !SteamManager.Initialized ) {
                LOGGER.Fatal("Steam not detected. Unable to start the mod.");
                return;
            }
            LOGGER.Debug("Steam is initialized");

            SteamInput.Init(false);
            LOGGER.Debug("SteamInput is initialized");

            // Attach to delayed action daemon
            this.delayedActionDaemon = gameObject.AddComponent<DelayedActionDaemon>();
            LOGGER.Debug("Delayed Actions Daemon attached");
            
            // Attach the controller daemon
            this.controllerDaemon = gameObject.AddComponent<ControllerDaemon>();
            this.controllerDaemon.OnControllerConnected.Add(this.OnControllerConnected);
            this.controllerDaemon.OnControllerDisconnected.Add(this.OnControllerDisconnected);
            LOGGER.Debug("Controller Daemon attached");

            // Attach the Vizzy controller
            this.vizzyController = gameObject.AddComponent<VizzyController>();
            this.vizzyController.VizzyOpened += OnVizzyOpened;
            this.vizzyController.VizzyClosed += OnVizzyClosed;
            LOGGER.Debug("Vizzy Controller attached");

            // Attach to SR2 Events
            Game.Instance.SceneManager.SceneLoaded += OnSceneLoaded;
            Game.Instance.SceneManager.SceneUnloading += OnSceneUnloading;
            Game.Instance.UserInterface.AnyDialogsOpenChanged += this.OnAnyDialogsOpenChanged;
            LOGGER.Debug("SR2 Events attached");

            LOGGER.Debug("Started");
        }

        // <summary>
        //  Plugin destroyed
        // </summary>
        public void OnDestroy() {
            LOGGER.Debug("Destroying");
            
            this.delayedActionDaemon.CancelDelayedAction(this.ChangeActionSet);
            Destroy(this.delayedActionDaemon);
            LOGGER.Debug("Delayed Actions Dameon detached");
            
            this.controllerDaemon.OnControllerDisconnected.Remove(this.OnControllerDisconnected);
            this.controllerDaemon.OnControllerConnected.Remove(this.OnControllerConnected);
            Destroy(this.controllerDaemon);
            LOGGER.Debug("Controller Daemon detached");

            this.vizzyController.VizzyOpened -= OnVizzyOpened;
            this.vizzyController.VizzyClosed -= OnVizzyClosed;
            Destroy(this.vizzyController);
            LOGGER.Debug("Vizzy Controller detached");
            
            Game.Instance.SceneManager.SceneLoaded -= OnSceneLoaded;
            Game.Instance.SceneManager.SceneUnloading -= OnSceneUnloading;
            Game.Instance.UserInterface.AnyDialogsOpenChanged -= this.OnAnyDialogsOpenChanged;
            LOGGER.Debug("SR2 Events dettached");

            LOGGER.Debug("Destroyed");
        }

        // ====================================================================================

        public void TriggerActionSetChange(string message) {
            LOGGER.Debug(message);
            this.delayedActionDaemon.TriggerDelayedAction(
                this.ChangeActionSet, 
                DELAY
            );
        }

        public void CancelActionSetChange(string message) {
            LOGGER.Debug(message);
            this.delayedActionDaemon.CancelDelayedAction(this.ChangeActionSet);
        }

        // <summary>
        // Change the action set
        // </summary>
        private void ChangeActionSet() {
            LOGGER.Debug("Changing action set");
            if( !this.controllerDaemon.ControllerConnected ) {
                LOGGER.Warn("No controller connected... Unable to change action set");
                return;
            }
            
            EActionSets actionSet = this.ComputeActionSet();
            if( actionSet.Equals(this.prevActionSet) ) {
                LOGGER.Debug("Action set " + actionSet + " is already set. Doing nothing...");
                return;
            }

            this.controllerDaemon.ChangeActionSet(actionSet);
            
            if( ModSettings.Instance.DisplayMessageOnActionSetChange.Value ) {
                string message = "SteamInput: Changing action set to " + actionSet;
                if( Game.InDesignerScene ) {
                    if( this.vizzyController.InVizzy ) {
                        this.vizzyController.ShowMessage(message);
                    } else {
                        Game.Instance.Designer.ShowMessage(message);
                    }
                } else if( Game.InFlightScene ) {
                    Game.Instance.FlightScene.FlightSceneUI.ShowMessage(message);
                } else if( Game.InPlanetStudioScene ) {
                    ModApi.PlanetStudio.PlanetStudioBase.Instance.PlanetStudioUI.ShowMessage(message);
                }
            }

            this.prevActionSet = actionSet;
        }

        // <summary>
        //  Compute the action set to use, depending on the context
        // </summary>
        private EActionSets ComputeActionSet() {
            if( Game.Instance.UserInterface.AnyDialogsOpen ) {
                return EActionSets.Menu;
            } else if( Game.InDesignerScene ) {
                if( this.vizzyController.InVizzy ) {
                    return EActionSets.Menu;
                }
                return EActionSets.Designer;
            } else if( Game.InTechTreeScene ) {
                return EActionSets.TechTree;
            } else if( Game.InPlanetStudioScene ) {
                return EActionSets.PlanetStudio;
            } else if( Game.InFlightScene ) {
                if( Game.Instance.FlightScene.ViewManager.MapViewManager.IsInForeground ) {
                    return EActionSets.Map;
                }

                foreach( ICommandPod pod in Game.Instance.FlightScene.CraftNode.CraftScript.CommandPods ) {
                    if( pod.EvaScript != null && pod.EvaScript.EvaActive ) {
                        return EActionSets.EVA;
                    }
                }
                
                return EActionSets.Flight;
            }
            
            return EActionSets.Menu;
        }
        
        // ==============================================================================
        //              Connection/disconnection events of controller
        // ==============================================================================
        
        // <summary>
        //  New controller connected
        // </summary>
        private void OnControllerConnected() {
            this.TriggerActionSetChange("Controller connected");
        }

        // <summary>
        //  Controller disconnected
        // </summary>
        private void OnControllerDisconnected() {
            this.CancelActionSetChange("Controller disconnected");
        }

        // ========================================================================================
        //                                      SR2 Events
        // ========================================================================================

        // <summary>
        //  Scene loaded. We only have a few scenes like Main Menu, Designer, Flight Scene, TechTree and PlanetStudio
        // </summary>
        public void OnSceneLoaded(object sender, SceneEventArgs args) {
            if( Game.InFlightScene ) {
                Game.Instance.FlightScene.ViewManager.MapViewManager.ForegroundStateChanged += this.OnForegroundMapViewStateChanged;
                Game.Instance.FlightScene.CraftChanged += this.OnCraftChanged;
            }
            this.TriggerActionSetChange("Scene Loaded : " + args.Scene);
        }

        // <summary>
        //  Scene unloading
        // </summary>
        public void OnSceneUnloading(object sender, SceneEventArgs args) {
            if( Game.InFlightScene ) {
                Game.Instance.FlightScene.ViewManager.MapViewManager.ForegroundStateChanged -= this.OnForegroundMapViewStateChanged;
                Game.Instance.FlightScene.CraftChanged -= this.OnCraftChanged;
            }
            this.CancelActionSetChange("Scene Unloading : " + args.Scene);
        }

        // <summary>
        //  A dialog box is opened or closed
        // </summary>
        public void OnAnyDialogsOpenChanged(bool inDialog) {
            this.TriggerActionSetChange("Dialog Opened ? " + inDialog);
        }

        // =======================================================================
        //      SR2 Events specific pour Flight Scene
        // =======================================================================

        public void OnForegroundMapViewStateChanged(bool foreground) {
            this.TriggerActionSetChange("Flight Scene: Map view in foreground ? " + foreground);
        }

        public void OnCraftChanged(ICraftNode craftNode) {
            this.TriggerActionSetChange("Flight Scene: Craft changed");
        }

        // ===============
        //  Vizzy Events
        // ===============

        public void OnVizzyOpened(object sender, EventArgs args) {
            this.TriggerActionSetChange("Vizzy Editor: Opened");
        }

        public void OnVizzyClosed(object sender, EventArgs args) {
            this.TriggerActionSetChange("Vizzy Editor: Closed");
        }
    }
}