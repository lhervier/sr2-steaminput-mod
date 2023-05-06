using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ModApi.GameLoop;

namespace Assets.Scripts {

    // <summary>
    //  Allows to launch an action in a set of frames. If the same action is triggered
    //  a second time, the launch of the action will be delayed again.
    // </summary>
    public class DelayedActionDaemon : MonoBehaviourBase {
        
        // <summary>
        //  Logger
        // </summary>
        private static SteamInputLogger LOGGER = new SteamInputLogger("DelayedActionDaemon");

        // <summary>
        // Has the behaviour been initialized ?
        // </summary>
        private bool initialized = false;

        // ===============================================

        // <summary>
        //  Frame count at which the delayed action will occur
        //  except if another operation
        //  ask for another update, which will increase this value.
        // </summary>
        private IDictionary<Action, int> actionThreshold = new Dictionary<Action, int>();

        // <summary>
        //  Co-routines used to delay the actions
        // </summary>
        private IDictionary<Action, Coroutine> coroutines = new Dictionary<Action, Coroutine>();

        // =======================================================================
        //              Unity Lifecycle
        // =======================================================================

        // <summary>
        //  Component awaked
        // </summary>
        public void Awake() {
            LOGGER.Debug("Awaking");
            // DontDestroyOnLoad(this);
            LOGGER.Debug("Awaked");
        }

        // <summary>
        //  Startup of the component
        // </summary>
        public void Start() {
            LOGGER.Debug("Starting");
            this.initialized = true;
            LOGGER.Debug("Started");
        }

        // <summary>
        //  Component destroyed
        // </summary>
        public void OnDestroy() {
            LOGGER.Debug("Destroying");
            foreach( Coroutine cr in this.coroutines.Values ) {
                this.StopCoroutine(cr);
            }
            this.coroutines.Clear();
            this.actionThreshold.Clear();
            this.initialized = false;
            LOGGER.Debug("Destroyed");
        }

        // ===============================================================

        // <summary>
        //  Trigger an action in the future
        // </summary>
        public void TriggerDelayedAction(Action action, int inFrames) {
            if( !this.initialized ) {
                return;
            }

            int threshold;
            if( this.actionThreshold.ContainsKey(action) ) {
                threshold = Math.Max(Time.frameCount + inFrames, this.actionThreshold[action]);
            } else {
                threshold = Time.frameCount + inFrames;
            }
            this.actionThreshold[action] = threshold;
            if( !this.coroutines.ContainsKey(action) ) {
                this.coroutines[action] = this.StartCoroutine(_TriggerDelayedAction(action));
            }
        }

        private IEnumerator _TriggerDelayedAction(Action action) {
            while( this.actionThreshold.ContainsKey(action) && Time.frameCount < this.actionThreshold[action] ) {
                yield return null;
            }
            if( this.actionThreshold.ContainsKey(action) ) {
                action();
            }
            this.coroutines.Remove(action);
            this.actionThreshold.Remove(action);
        }

        // <summary>
        //  Cancel any action set change request
        // </summary>
        public void CancelDelayedAction(Action action) {
            if( !this.initialized ) {
                return;
            }
            
            if( this.coroutines.ContainsKey(action) ) {
                this.StopCoroutine(this.coroutines[action]);
            }
            this.coroutines.Remove(action);
            this.actionThreshold.Remove(action);
        }
    }

}