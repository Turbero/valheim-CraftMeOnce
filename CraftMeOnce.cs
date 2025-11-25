using BepInEx;
using HarmonyLib;

namespace CraftMeOnce
{
    [BepInPlugin(GUID, NAME, VERSION)]
    public class CraftMeOnce : BaseUnityPlugin
    {
        public const string GUID = "Turbero.CraftMeOnce";
        public const string NAME = "Craft Me Once";
        public const string VERSION = "1.0.0";

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
}
