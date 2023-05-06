using UnityEngine;
using System;

namespace Assets.Scripts {

    public enum ELogLevel {
        DEBUG,
        INFO,
        WARN,
        ERROR,
        FATAL
    }

    public static class ELogLevelUtils {
        public static int GetLevelValue(this ELogLevel level) {
            switch( level ) {
                case ELogLevel.DEBUG:
                    return 0;
                case ELogLevel.INFO:
                    return 1;
                case ELogLevel.WARN:
                    return 2;
                case ELogLevel.ERROR:
                    return 3;
                case ELogLevel.FATAL:
                    return 4;
            }
            return -1;      // Should not happen...
        }

        public static string AsString(this ELogLevel level) {
            return (level.ToString() + " ").Substring(0, 5);
        }
    }

}