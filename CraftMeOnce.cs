using HarmonyLib;

namespace CraftMeOnce
{
    public class CraftMeOnce
    {
        public const string GUID = "Turbero.CraftMeOnce";
        public const string NAME = "Craft Me Once";
        public const string VERSION = "1.0.0";

        private readonly Harmony harmony = new Harmony(GUID);

        void Awake()
        {
            harmony.PatchAll();
        }

        void onDestroy()
        {
            harmony.UnpatchSelf();
        }
    }
}
