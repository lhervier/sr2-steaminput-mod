using UnityEngine;
using System;

namespace Assets.Scripts {

    public class SteamInputLogger {
        private string prefix = "[SteamInputPlugin]";

        public SteamInputLogger() {
        }
        
        public SteamInputLogger(string additionalPrefix) {
            this.prefix += "[" + additionalPrefix + "]"; 
        }

        public void Log(string message) {
            string formattedDateTime = DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss.fff");
            Debug.Log(formattedDateTime + " "  + this.prefix + " " + message);
        }
    }
}