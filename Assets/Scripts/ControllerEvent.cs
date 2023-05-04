using System;
using System.Collections;
using System.Collections.Generic;

namespace Assets.Scripts {
    public class ControllerEvent {
        private SteamInputLogger LOGGER;
        private string name;
        private List<Action> actions = new List<Action>();

        public ControllerEvent(String name) {
            this.name = name;
            this.LOGGER = new SteamInputLogger("ControllerEvent " + name);
        }

        public void Add(Action action) {
            this.actions.Add(action);
        }

        public void Remove(Action action) {
            this.actions.Remove(action);
        }

        public void Fire() {
            this.LOGGER.Log("Firing event (" + this.actions.Count + " events)");
            foreach( Action action in this.actions ) {
                action();
            }
        }
    }
}