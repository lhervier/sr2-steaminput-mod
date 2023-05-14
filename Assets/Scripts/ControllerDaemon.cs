using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Steamworks;
using ModApi.GameLoop;

namespace Assets.Scripts {

    /// <summary>
    /// Daemon in charge of listening to controller connection/disconnection
    /// It also allow to change the current action set of the controller
    ///
    /// It only detects the first connected controller !
    /// </summary>
    public class ControllerDaemon : MonoBehaviourBase {
        
        // ==========================================================================================
        //                          Static properties
        // ==========================================================================================

        /// <summary>
        /// Logger object
        /// </summary>
        private static SteamInputLogger LOGGER = new SteamInputLogger("ControllerDaemon");

        // ===============================================

        /// <summary>
        /// Called when a the controller is connected
        /// </summary>
        public ControllerEvent OnControllerConnected { get; private set; }

        /// <summary>
        /// Called when the controller is disconnected
        /// </summary>
        public ControllerEvent OnControllerDisconnected {get; private set; }

        /// <summary>
        /// Is the controller connected ?
        /// </summary>
        public bool ControllerConnected { get; private set; } = false;

        // ==============================================

        /// <summary>
        ///  Handle to the first connected controller. No sense if ControllerConnected = false
        /// </summary>
        private InputHandle_t controllerHandle;

        /// <summary>
        ///  The action sets handles defined in the controller configuration template
        /// </summary>
        private IDictionary<EActionSets, InputActionSetHandle_t> actionsSetsHandles = new Dictionary<EActionSets, InputActionSetHandle_t>();

        /// <summary>
        ///  Handles to the connected controllers.
        ///  Don't use. This array is here to prevent from instanciating a new one every cycle.
        /// </summary>
        private InputHandle_t[] _controllerHandles = new InputHandle_t[Steamworks.Constants.STEAM_INPUT_MAX_COUNT];

        // =======================================================================

        /// <summary>
        ///  Coroutine to check for a controller connection every seconds.
        /// </summary>
        private IEnumerator checkForControllerCoroutine;

        // =======================================================================
        //              Unity Lifecycle
        // =======================================================================

        /// <summary>
        ///  Component awaken
        /// </summary>
        public void Awake() {
            LOGGER.Debug("Awaking");
            DontDestroyOnLoad(this);
            
            this.OnControllerConnected = new ControllerEvent("controller.OnConnected");
            this.OnControllerDisconnected = new ControllerEvent("controller.OnDisconnected");
            this.ControllerConnected = false;
            
            LOGGER.Debug("Awaked");
        }

        /// <summary>
        /// Startup of the component
        /// </summary>
        public void Start() {
            LOGGER.Debug("Starting");
            this.checkForControllerCoroutine = this.CheckForController();
            this.StartCoroutine(this.checkForControllerCoroutine);
            LOGGER.Debug("Started");
        }

        /// <summary>
        /// Component destroyed
        /// </summary>
        public void OnDestroy() {
            LOGGER.Debug("Destroying");
            this.StopCoroutine(this.checkForControllerCoroutine);
            LOGGER.Debug("Destroyed");
        }
        
        // ==============================================================================
        //              Detection of connection/disconnection of controllers
        // ==============================================================================
        
        /// <summary>
        /// Main loop to detect controller connection/disconnection
        /// </summary>
        private IEnumerator CheckForController() {
            WaitForSeconds waitFor1Second = new WaitForSeconds(1);
            while( true ) {
                SteamInput.RunFrame();

                // Detect connection/disconnection
                int nbControllers = SteamInput.GetConnectedControllers(this._controllerHandles);
                bool newController = false;
                bool disconnectedController = false;
                if( nbControllers == 0 ) {
                    if( this.ControllerConnected ) {
                        newController = false;
                        disconnectedController = true;
                    } else {
                        newController = false;
                        disconnectedController = false;
                    }
                } else {
                    if( this.ControllerConnected ) {
                        if( this.controllerHandle == this._controllerHandles[0] ) {
                            newController = false;
                            disconnectedController = false;
                        } else {
                            newController = true;
                            disconnectedController = true;
                        }
                    } else {
                        newController = true;
                        disconnectedController = false;
                    }
                }

                // Disconnect the controller
                if( disconnectedController ) {
                    LOGGER.Info("Controller disconnected");
                    this.ControllerConnected = false;
                    this.UnloadActionSets();
                    this.OnControllerDisconnected.Fire();
                }

                // Connects the controller
                if( newController ) {
                    LOGGER.Info("Controller connected");
                    this.controllerHandle = this._controllerHandles[0];
                    this.ControllerConnected = true;
                    this.LoadActionSets();
                    this.OnControllerConnected.Fire();
                }

                // Wait for 1 second
                yield return waitFor1Second;
            }
        }
        
        /// <summary>
        /// Load action sets handles.
        /// </summary>
        private void LoadActionSets() {
            LOGGER.Debug("Loading Action Sets Handles");
            foreach(EActionSets actionSet in Enum.GetValues(typeof(EActionSets))) {
                if( actionSet == EActionSets.NotSet ) {
                    continue;
                }
                LOGGER.Debug("- Getting action set handle for " + actionSet);
                InputActionSetHandle_t actionSetHandle = SteamInput.GetActionSetHandle(actionSet.GetId());
                if( actionSetHandle.m_InputActionSetHandle == 0L ) {
                    LOGGER.Debug("ERROR : Action set handle for " + actionSet + " not found. I will use the default action set instead");
                }
                this.actionsSetsHandles[actionSet] = actionSetHandle;
            }
        }

        /// <summary>
        /// Unloads the action sets
        /// </summary>
        private void UnloadActionSets() {
            this.actionsSetsHandles.Clear();
        }

        // =========================================================================================

        /// <summary>
        /// Change the current action set
        /// </summary>
        /// <param name="actionSet">The action set to set</param>
        public void ChangeActionSet(EActionSets actionSet) {
            LOGGER.Info("Changing Action Set to " + actionSet);
            if( !this.ControllerConnected ) {
                LOGGER.Warn("No controller connected. Nothing to change...");
                return;
            }

            SteamInput.ActivateActionSet(
                this.controllerHandle, 
                this.actionsSetsHandles[actionSet]
            );
        }
    }
}