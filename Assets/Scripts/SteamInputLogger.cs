using UnityEngine;
using System;

namespace Assets.Scripts {

    public class SteamInputLogger {
        private string prefix = "[SteamInputMod]";

        public SteamInputLogger() {
        }
        
        public SteamInputLogger(string additionalPrefix) {
            this.prefix += "[" + additionalPrefix + "]"; 
        }

        public void Debug(string message) {
            this.Log(ELogLevel.DEBUG, message);
        }

        public void Info(string message) {
            this.Log(ELogLevel.INFO, message);
        }

        public void Warn(string message) {
            this.Log(ELogLevel.WARN, message);
        }

        public void Error(string message) {
            this.Log(ELogLevel.ERROR, message);
        }

        public void Fatal(string message) {
            this.Log(ELogLevel.FATAL, message);
        }

        public void Log(ELogLevel level, string message) {
            int settingValue = ModSettings.Instance.LogLevel.Value.GetLevelValue();
            int levelValue = level.GetLevelValue();
            if( levelValue >= settingValue ) {
                string formattedDateTime = DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss.fff");
                UnityEngine.Debug.Log(formattedDateTime + " "  + level.AsString() + " "  + this.prefix + " " + message);
            }
        }
    }
}