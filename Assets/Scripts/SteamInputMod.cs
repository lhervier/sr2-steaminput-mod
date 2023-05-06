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
        private EActionSets prevActionSet = EActionSets.NotSet;

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
        //  Start of the plugin
        // </summary>
        protected void Start() {
            LOGGER.Debug("Starting");
            
            // Attach to delayed action daemon
            this.delayedActionDaemon = gameObject.AddComponent<DelayedActionDaemon>();
            LOGGER.Debug("Delayed Actions Daemon attached");
            
            this.controllerDaemon = gameObject.AddComponent<ControllerDaemon>();
            this.controllerDaemon.OnControllerConnected.Add(this.OnControllerConnected);
            this.controllerDaemon.OnControllerDisconnected.Add(this.OnControllerDisconnected);
            LOGGER.Debug("Controller Daemon attached");

            LOGGER.Debug("Started");
        }

        // <summary>
        //  Plugin destroyed
        // </summary>
        public void OnDestroy() {
            LOGGER.Debug("Destroying");
            
            this.delayedActionDaemon.CancelDelayedAction(this.ChangeActionSet);
            Destroy(this.delayedActionDaemon);
            
            this.controllerDaemon.OnControllerDisconnected.Remove(this.OnControllerDisconnected);
            this.controllerDaemon.OnControllerConnected.Remove(this.OnControllerConnected);
            Destroy(this.controllerDaemon);
            
            LOGGER.Debug("Destroyed");
        }

        // ====================================================================================

        // <summary>
        //  Trigger an action set change
        // </summary>
        public void TriggerActionSetChange() {
            LOGGER.Debug("Triggering action set change in " + DELAY +" frames");
            this.delayedActionDaemon.TriggerDelayedAction(
                this.ChangeActionSet, 
                DELAY
            );
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
            this.prevActionSet = actionSet;
        }

        // <summary>
        //  Compute the action set to use, depending on the context
        // </summary>
        private EActionSets ComputeActionSet() {
            if( Game.InFlightScene ) {
                if( Game.Instance.FlightScene.ViewManager.MapViewManager.IsInForeground ) {
                    return EActionSets.Map;
                }

                foreach( ICommandPod pod in Game.Instance.FlightScene.CraftNode.CraftScript.CommandPods ) {
                    if( pod.EvaScript != null && pod.EvaScript.EvaActive ) {
                        return EActionSets.EVA;
                    }
                }
                
                return EActionSets.Flight;
            } else if( Game.InDesignerScene ) {
                return EActionSets.Designer;
            } else if( Game.InTechTreeScene ) {
                return EActionSets.TechTree;
            } else if( Game.InPlanetStudioScene ) {
                return EActionSets.PlanetStudio;
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
            LOGGER.Debug("New controller connected");
            
            if( Game.InFlightScene ) {
                Game.Instance.FlightScene.ViewManager.MapViewManager.ForegroundStateChanged += this.OnForegroundMapViewStateChanged;
                Game.Instance.FlightScene.CraftChanged += this.OnCraftChanged;
            }
            LOGGER.Debug("DR2 hooks created");

            // Trigger an action set change to load the right action set
            this.TriggerActionSetChange();
        }

        // <summary>
        //  Controller disconnected
        // </summary>
        private void OnControllerDisconnected() {
            // Canceling eventual action set change
            this.delayedActionDaemon.CancelDelayedAction(this.ChangeActionSet);

            // Unhooks to KSP
            if( Game.InFlightScene ) {
                Game.Instance.FlightScene.ViewManager.MapViewManager.ForegroundStateChanged -= OnForegroundMapViewStateChanged;
                Game.Instance.FlightScene.CraftChanged -= this.OnCraftChanged;
            }
            LOGGER.Debug("SR2 hooks removed");
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