using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Steamworks;
using ModApi.Craft;
using ModApi.Craft.Parts;
using ModApi.GameLoop;

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
        private SR2ActionSets prevActionSet = SR2ActionSets.NotSet;

        // <summary>
        //  Connection Daemon to the controllers
        // </summary>
        private ControllerDaemon controllerDaemon;
        
        // <summary>
        //  Delayed Action daemon
        // </summary>
        private DelayedActionDaemon delayedActionDaemon;

        // ===============================================================================
        //                      Unity initialization
        // ===============================================================================

        // <summary>
        //  Make our plugin survive between scene loading
        // </summary>
        protected void Awake() {
            LOGGER.Log("Awaking");
            // DontDestroyOnLoad(this);
            LOGGER.Log("Awaked");
        }

        // <summary>
        //  Start of the plugin
        // </summary>
        protected void Start() {
            LOGGER.Log("Starting");
            
            // Attach to delayed action daemon
            this.delayedActionDaemon = gameObject.AddComponent<DelayedActionDaemon>();
            LOGGER.Log("Delayed Actions Daemon attached");
            
            this.controllerDaemon = gameObject.AddComponent<ControllerDaemon>();
            LOGGER.Log("Controller Daemon attached");

            // Attach to controller Daemon
            this.StartCoroutine(            
                this.WaitForControllerDaemon(
                    () => {
                        // When a controller is already connected
                        if( this.controllerDaemon.ControllerConnected ) {
                            LOGGER.Log("Controller already connected. Running callbacks");
                            this.OnControllerConnected();
                        }

                        this.controllerDaemon.OnControllerConnected.Add(this.OnControllerConnected);
                        this.controllerDaemon.OnControllerDisconnected.Add(this.OnControllerDisconnected);
                        LOGGER.Log("Controller Events attached");
                    }
                )
            );
            LOGGER.Log("Started");
        }

        // <summary>
        //  Plugin destroyed
        // </summary>
        public void OnDestroy() {
            LOGGER.Log("Destroying");
            this.controllerDaemon.OnControllerDisconnected.Remove(this.OnControllerDisconnected);
            this.controllerDaemon.OnControllerConnected.Remove(this.OnControllerConnected);
            Destroy(this.delayedActionDaemon);
            Destroy(this.controllerDaemon);
            LOGGER.Log("Destroyed");
        }

        // ==========================================================================

        // <summary>
        //  Wait for the controller daemon to be available
        // </summary>
        // <param name="next">Action to execute once the daemon is ready</param>
        private IEnumerator WaitForControllerDaemon(Action next) {
            LOGGER.Log("Waiting for Controller Daemon");
            while( !this.controllerDaemon.Started ) {
                yield return null;
            }
            LOGGER.Log("Controller Daemon found");
            next();
        }

        // ====================================================================================

        // <summary>
        //  Trigger an action set change
        // </summary>
        public void TriggerActionSetChange() {
            LOGGER.Log("Triggering action set change in " + DELAY +" frames");
            this.delayedActionDaemon.TriggerDelayedAction(this._TriggerActionSetChange, DELAY);
        }
        private void _TriggerActionSetChange() {
            SR2ActionSets actionSet = this.ComputeActionSet();
            this.ChangeActionSet(actionSet);
        }

        // <summary>
        //  Cancel an action set change
        // </summary>
        private void CancelActionSetChange() {
            LOGGER.Log("Canceling previous action set change (if any)");
            this.delayedActionDaemon.CancelDelayedAction(this._TriggerActionSetChange);
        }

        // <summary>
        //  Change action set NOW
        // </summary>
        public void ChangeActionSet(SR2ActionSets actionSet) {
            LOGGER.Log("Changing action set to " + actionSet);
            this.CancelActionSetChange();
            this._ChangeActionSet(actionSet);
        }
        private void _ChangeActionSet(SR2ActionSets actionSet) {
            if( !this.controllerDaemon.ControllerConnected ) {
                LOGGER.Log("No controller connected... Unable to change action set");
                return;
            }
            if( actionSet.Equals(this.prevActionSet) ) {
                LOGGER.Log("Action set " + actionSet + " is already set. Doing nothing...");
                return;
            }

            this.controllerDaemon.ChangeActionSet(actionSet);
            this.prevActionSet = actionSet;
        }

        // <summary>
        //  Compute the action set to use, depending on the context
        // </summary>
        private SR2ActionSets ComputeActionSet() {
            if( Game.InFlightScene ) {
                if( Game.Instance.FlightScene.ViewManager.MapViewManager.IsInForeground ) {
                    return SR2ActionSets.Map;
                }

                foreach( ICommandPod pod in Game.Instance.FlightScene.CraftNode.CraftScript.CommandPods ) {
                    if( pod.EvaScript != null && pod.EvaScript.EvaActive ) {
                        return SR2ActionSets.EVA;
                    }
                }
                
                return SR2ActionSets.Flight;
            } else if( Game.InDesignerScene ) {
                return SR2ActionSets.Designer;
            } else if( Game.InTechTreeScene ) {
                return SR2ActionSets.TechTree;
            } else if( Game.InPlanetStudioScene ) {
                return SR2ActionSets.PlanetStudio;
            }
            
            return SR2ActionSets.Menu;
        }
        
        // ==============================================================================
        //              Connection/disconnection events of controller
        // ==============================================================================
        
        // <summary>
        //  New controller connected
        // </summary>
        private void OnControllerConnected() {
            LOGGER.Log("New controller connected");
            
            if( Game.InFlightScene ) {
                Game.Instance.FlightScene.ViewManager.MapViewManager.ForegroundStateChanged += this.OnForegroundMapViewStateChanged;
                Game.Instance.FlightScene.CraftChanged += this.OnCraftChanged;
            }
            LOGGER.Log("DR2 hooks created");

            // Trigger an action set change to load the right action set
            this.TriggerActionSetChange();
        }

        // <summary>
        //  Controller disconnected
        // </summary>
        private void OnControllerDisconnected() {
            // Canceling eventual action set change
            this.CancelActionSetChange();

            // Unhooks to KSP
            if( Game.InFlightScene ) {
                Game.Instance.FlightScene.ViewManager.MapViewManager.ForegroundStateChanged -= OnForegroundMapViewStateChanged;
                Game.Instance.FlightScene.CraftChanged -= this.OnCraftChanged;
            }
            LOGGER.Log("SR2 hooks removed");
        }

        // ========================================================================================
        //                                      SR2 Events
        // ========================================================================================

        public void OnForegroundMapViewStateChanged(bool foreground) {
            this.TriggerActionSetChange();
        }

        public void OnCraftChanged(ICraftNode craftNode) {
            this.TriggerActionSetChange();
        }
    }
}