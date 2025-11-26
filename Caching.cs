using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace CraftMeOnce
{
    public static class Caching
    {
        public static readonly Dictionary<string, string> itemDropTranslatedKeys = new Dictionary<string, string>();

        [HarmonyPatch(typeof(Player), "OnSpawned")]
        public static class Player_OnSpawned
        {
            static void Postfix(bool spawnValkyrie)
            {
                Logger.Log($"Player_OnSpawned");
                AddItemDrops();
            }
        }

        public static void AddItemDrops()
        {
            itemDropTranslatedKeys.Clear();
            foreach (Recipe recipe in Resources.FindObjectsOfTypeAll<Recipe>())
            {
                if (recipe.m_item != null && 
                    recipe.m_item.m_itemData != null &&
                    recipe.m_item.m_itemData.m_shared != null)
                {
                    string translated = Localization.instance.Localize(recipe.m_item.m_itemData.m_shared.m_name);
                    if (!itemDropTranslatedKeys.ContainsKey(translated))
                        itemDropTranslatedKeys.Add(translated, recipe.m_item.m_itemData.m_shared.m_name);
                }
            }
        }
    }
    
    [HarmonyPatch(typeof(Localization), "SetLanguage")]
    public class Localization_SetLanguage_Patch
    {
        static void Postfix(string language)
        {
            Logger.Log($"Language changed to: {language}");
            Caching.AddItemDrops();
        }
    }
}