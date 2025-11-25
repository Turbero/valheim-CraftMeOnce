using BepInEx.Logging;
using UnityEngine;
using UnityEngine.UI;

namespace CraftMeOnce
{
    public static class Logger
    {
        private static readonly ManualLogSource logger = BepInEx.Logging.Logger.CreateLogSource(CraftMeOnce.NAME);
        
        internal static void Log(object s)
        {
            if (ConfigurationFile.debug.Value == ConfigurationFile.Toggle.Off)
            {
                return;
            }

            logger.LogInfo(s?.ToString());
        }

        internal static void LogInfo(object s)
        {
            logger.LogInfo(s?.ToString());
        }

        internal static void LogWarning(object s)
        {
            var toPrint = $"{CraftMeOnce.NAME} {CraftMeOnce.VERSION}: {(s != null ? s.ToString() : "null")}";

            Debug.LogWarning(toPrint);
        }

        internal static void LogError(object s)
        {
            var toPrint = $"{CraftMeOnce.NAME} {CraftMeOnce.VERSION}: {(s != null ? s.ToString() : "null")}";

            Debug.LogError(toPrint);
        }
    }
}
