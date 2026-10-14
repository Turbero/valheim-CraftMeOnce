using BepInEx;
using HarmonyLib;
using UnityEngine;

namespace CraftMeOnce
{
    [BepInPlugin(GUID, NAME, VERSION)]
    public class CraftMeOnce : BaseUnityPlugin
    {
        public const string GUID = "Turbero.CraftMeOnce";
        public const string NAME = "Craft Me Once";
        public const string VERSION = "1.1.0";

        private readonly Harmony harmony = new Harmony(GUID);

        void Awake()
        {
            ConfigurationFile.LoadConfig(this);
            harmony.PatchAll();
        }

        void onDestroy()
        {
            harmony.UnpatchSelf();
        }
    }
    
    [HarmonyPatch]
    public class NoWarnings
    {
        [HarmonyPatch(typeof(Debug), nameof(Debug.LogWarning), typeof(object), typeof(Object))]
        [HarmonyPrefix]
        private static bool LogWarning(object message, Object context)
        {
            if (message.ToString().Contains("The LiberationSans SDF Font Asset was not found") || 
                message.ToString().Contains("The character used for Ellipsis is not available in font asset") ||
                message.ToString().Contains("lacks a proper counterpart in the GamepadButton enum"))
            {
                return false;
            }

            return true;
        }
    }
}
