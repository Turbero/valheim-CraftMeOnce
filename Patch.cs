using System.Collections.Generic;
using System.Text.RegularExpressions;
using HarmonyLib;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CraftMeOnce
{
    [HarmonyPatch(typeof(InventoryGui), "UpdateRecipeList")]
    public static class UpdateCraftingPanelPatch
    {
        static void Postfix(InventoryGui __instance, List<Recipe> recipes)
        {
            if (ConfigurationFile.modEnabled.Value == ConfigurationFile.Toggle.Off)
            {
                if (BtnExclamationPatch.btnExclamation != null)
                    BtnExclamationPatch.btnExclamation.gameObject.SetActive(false);
                return;
            }
            if (BtnExclamationPatch.btnExclamation == null) return;
            if (ConfigurationFile.showExclamation.Value == ConfigurationFile.Toggle.Off) return;
            
            BtnExclamationPatch.btnExclamation.gameObject.SetActive(true);
            
            Logger.Log("UpdateCraftingPanelPatch - Postfix");
            Player player = Player.m_localPlayer;
            if (player == null) return;
                
            Transform listRoot = __instance.transform.Find("root/Crafting/RecipeList/Recipes/ListRoot");
            int childrenCount = listRoot.childCount;

            for (int i = 0; i < childrenCount; i++)
            {
                TextMeshProUGUI translatedText = listRoot.GetChild(i).Find("name").GetComponent<TextMeshProUGUI>();
                if (translatedText == null) continue;
                
                Logger.Log("translatedText "+translatedText.text);
                bool found = findTranslatedKey(translatedText, out var recipeKey);
                if (found)
                {
                    Logger.Log("found itemRecipeKeyValue: "+ translatedText + " - "+ recipeKey);
                    bool known = player.IsKnownMaterial(recipeKey);
                    if (!known)
                        translatedText.text = "<color=yellow>" + ConfigurationFile.characterForNotcraftedItems.Value + "</color> " + Localization.instance.Localize(translatedText.text);
                    else
                        translatedText.text = Localization.instance.Localize(translatedText.text);
                }
            }
        }

        private static bool findTranslatedKey(TextMeshProUGUI translatedText, out string recipeKey)
        {
            bool found = Caching.itemDropTranslatedKeys.TryGetValue(translatedText.text, out recipeKey);
            if (found) return true;
            
            //Items that produces more than 1
            string translatedWithoutAmount = RemoveAmountSuffix(translatedText.text, " x");
            Logger.Log("translated quantity check: "+translatedWithoutAmount);
            found = Caching.itemDropTranslatedKeys.TryGetValue(translatedWithoutAmount, out recipeKey);
            if (found) return true;
            
            ///AAACrafting mod compatibility
            string translatedWithoutGarbage = CleanItemName(translatedText.text);
            Logger.Log("translated other mods check: "+translatedWithoutGarbage);
            return Caching.itemDropTranslatedKeys.TryGetValue(translatedWithoutGarbage, out recipeKey);
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
    }

    [HarmonyPatch(typeof(InventoryGui), "SetupCrafting")]
    public static class BtnExclamationPatch
    {
        private static GameObject btnExclamationGo;
        public static Button btnExclamation;
        private static TextMeshProUGUI buttonText;
        
        static void Postfix(InventoryGui __instance)
        {
            if (btnExclamationGo == null || btnExclamation == null || buttonText == null)
            {
                Transform copyButton = __instance.m_skillsDialog.transform.Find("SkillsFrame/Closebutton");
                Transform parent = __instance.m_crafting.transform;
                btnExclamationGo = Object.Instantiate(copyButton.gameObject, parent);
                btnExclamationGo.name = "BtnExclamation";
                btnExclamationGo.transform.SetParent(parent, false);
                
                RectTransform buttonTextRect = btnExclamationGo.GetComponent<RectTransform>();
                buttonTextRect.anchoredPosition = ConfigurationFile.btnPosition.Value;
                buttonTextRect.sizeDelta = ConfigurationFile.btnSize.Value;
                buttonText = btnExclamationGo.GetComponentInChildren<TextMeshProUGUI>();
                buttonText.font = GameManager.getFontAsset("Valheim-AveriaSerifLibre");
                buttonText.fontStyle = FontStyles.Normal;
                buttonText.alignment = TextAlignmentOptions.Center;

                btnExclamation = btnExclamationGo.GetComponent<Button>();
                btnExclamation.onClick = new Button.ButtonClickedEvent();
                btnExclamation.onClick.AddListener(() =>
                {
                    ConfigurationFile.showExclamation.Value =
                        ConfigurationFile.showExclamation.Value == ConfigurationFile.Toggle.Off
                            ? ConfigurationFile.Toggle.On
                            : ConfigurationFile.Toggle.Off;
                    //The config reload will call the setupCrafting after the previous line
                });
            }
            buttonText.text = ConfigurationFile.characterForNotcraftedItems.Value;
            buttonText.color = ConfigurationFile.showExclamation.Value == ConfigurationFile.Toggle.On ? Color.yellow : Color.gray;
        }
    }
}