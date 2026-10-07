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
            Logger.Log("UpdateCraftingPanelPatch - Postfix");
            if (ConfigurationFile.modEnabled.Value == ConfigurationFile.Toggle.Off)
            {
                if (HintButtonPatch.hintButton != null)
                    HintButtonPatch.hintButton.gameObject.SetActive(false);
                return;
            }
            
            if (HintButtonPatch.hintButton == null) return;
            HintButtonPatch.updateHintButtonVisibility(__instance);
            if (ConfigurationFile.hintMode.Value == ConfigurationFile.HintMode.Off) return;
            
            if (Player.m_localPlayer == null) return;
            if (Player.m_localPlayer.GetCurrentCraftingStation() == null) return;
            
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
                        translatedText.text = "<color=yellow>" + ConfigurationFile.characterForNotCraftedItems.Value + "</color> " + Localization.instance.Localize(translatedText.text);
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
    public static class HintButtonPatch
    {
        private static GameObject hintButtonGo;
        public static Button hintButton;
        private static TextMeshProUGUI buttonText;
        
        static void Prefix(InventoryGui __instance)
        {
            if (hintButtonGo == null || hintButton == null || buttonText == null)
            {
                Transform copyButton = __instance.m_skillsDialog.transform.Find("SkillsFrame/Closebutton");
                Transform parent = __instance.m_crafting.transform;
                hintButtonGo = Object.Instantiate(copyButton.gameObject, parent);
                hintButtonGo.name = "HintButton";
                hintButtonGo.transform.SetParent(parent, false);
                GameManager.BindGamePad(hintButtonGo.transform, ConfigurationFile.btnGamepadKey.Value, new Vector2(-30, 0), __instance);
                
                RectTransform buttonTextRect = hintButtonGo.GetComponent<RectTransform>();
                buttonTextRect.anchoredPosition = ConfigurationFile.btnPosition.Value;
                buttonTextRect.sizeDelta = ConfigurationFile.btnSize.Value;
                buttonText = hintButtonGo.GetComponentInChildren<TextMeshProUGUI>();
                buttonText.font = GameManager.getFontAsset("Valheim-AveriaSerifLibre");
                buttonText.fontStyle = FontStyles.Normal;
                buttonText.alignment = TextAlignmentOptions.Center;

                hintButton = hintButtonGo.GetComponent<Button>();
                hintButton.onClick = new Button.ButtonClickedEvent();
                hintButton.onClick.AddListener(() =>
                {
                    ConfigurationFile.hintMode.Value =
                        ConfigurationFile.hintMode.Value == ConfigurationFile.HintMode.Off
                            ? ConfigurationFile.HintMode.On
                            : ConfigurationFile.HintMode.Off;
                    //The config reload will call the setupCrafting after the previous line
                });
            }
            buttonText.text = ConfigurationFile.characterForNotCraftedItems.Value;
            buttonText.color = ConfigurationFile.hintMode.Value != ConfigurationFile.HintMode.Off ? Color.yellow : Color.gray;
        }

        public static void updateHintButtonVisibility(InventoryGui inventoryGui)
        {
            HintButtonPatch.hintButton.gameObject.SetActive(
                ConfigurationFile.modEnabled.Value == ConfigurationFile.Toggle.On &&
                ConfigurationFile.hintMode.Value != ConfigurationFile.HintMode.Always &&
                inventoryGui.m_crafting.Find("TabsButtons/Craft").gameObject.activeSelf);
        }
    }
}