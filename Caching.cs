using System.Collections.Generic;
using System.Text.RegularExpressions;
using HarmonyLib;
using TMPro;
using UnityEngine;

namespace CraftMeOnce
{
    public static class Caching
    {
        public static readonly Dictionary<string, string> itemDropTranslatedKeys = new Dictionary<string, string>();

        public static bool findTranslatedKey(TextMeshProUGUI translatedText, out string recipeKey)
        {
            bool found = itemDropTranslatedKeys.TryGetValue(translatedText.text, out recipeKey);
            if (found) return true;
            
            //Items that produces more than 1
            string translatedWithoutAmount = RemoveAmountSuffix(translatedText.text, " x");
            Logger.Log("translated quantity check: "+translatedWithoutAmount);
            found = itemDropTranslatedKeys.TryGetValue(translatedWithoutAmount, out recipeKey);
            if (found) return true;
            
            ///AAACrafting mod compatibility
            string translatedWithoutGarbage = CleanItemName(translatedText.text);
            Logger.Log("translated other mods check: "+translatedWithoutGarbage);
            return itemDropTranslatedKeys.TryGetValue(translatedWithoutGarbage, out recipeKey);
        }
        
        private static string RemoveAmountSuffix(string text, string indicator)
        {
            if (string.IsNullOrWhiteSpace(text))
                return text;

            int idx = text.LastIndexOf(indicator);
            if (idx > 0 && idx + 2 < text.Length)
            {
                // if it ends with a number
                string numberPart = text.Substring(idx + 2);

                if (int.TryParse(numberPart, out _))
                    return text.Substring(0, idx).Trim();
            }

            return text;
        }

        private static string CleanItemName(string raw)
        {
            if (string.IsNullOrEmpty(raw))
                return raw;

            string result = raw;

            // 1. Remove <size=...>...</size> that includes #5, #1, etc.
            result = Regex.Replace(result, @"<size=.*?</size>", "", RegexOptions.IgnoreCase);

            // 2. Remove colors or any other RichText)
            result = Regex.Replace(result, @"<.*?>", "", RegexOptions.IgnoreCase);

            // 3. Remove possible "#n" at the end of the name (ex: "Iron sword #3")
            result = Regex.Replace(result, @"\s+#\d+$", "", RegexOptions.IgnoreCase);

            // 4. Finally trim possible blank spaces at the beginning and end
            return result.Trim();
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
            foreach (Trader trader in Resources.FindObjectsOfTypeAll<Trader>())
            {
                List<Trader.TradeItem> tradeItems = trader.m_items ?? new List<Trader.TradeItem>();
                foreach(Trader.TradeItem tradeItem in tradeItems)
                {
                    if (tradeItem.m_prefab?.m_itemData?.m_shared != null) {
                        string translated = Localization.instance.Localize(tradeItem.m_prefab.m_itemData.m_shared.m_name);
                        if (!itemDropTranslatedKeys.ContainsKey(translated))
                            itemDropTranslatedKeys.Add(translated, tradeItem.m_prefab.m_itemData.m_shared.m_name);
                    }
                }
            }
        }
    }

    [HarmonyPatch(typeof(Player), "OnSpawned")]
    public static class Player_OnSpawned
    {
        static void Postfix(bool spawnValkyrie)
        {
            Logger.Log("Player_OnSpawned");
            Caching.AddItemDrops();
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