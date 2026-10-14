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
            HintButtonPatch.updateHintButton(__instance);
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
                
                Logger.Log("Workstation | translatedText "+translatedText.text);
                bool found = Caching.findTranslatedKey(translatedText, out var recipeKey);
                if (found)
                {
                    bool known = player.IsKnownMaterial(recipeKey);
                    Logger.Log("found itemRecipeKeyValue: "+ translatedText + " - "+ recipeKey + " - IsKnownMaterial: "+known);
                    if (!known && ConfigurationFile.hintMode.Value != ConfigurationFile.HintMode.Off)
                        translatedText.text = "<color=yellow>" + ConfigurationFile.characterForNotCraftedItems.Value + "</color> " + Localization.instance.Localize(translatedText.text);
                    else
                        translatedText.text = Localization.instance.Localize(translatedText.text);
                }
            }
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

        public static void updateHintButton(InventoryGui inventoryGui)
        {
            hintButton.gameObject.SetActive(
                ConfigurationFile.modEnabled.Value == ConfigurationFile.Toggle.On &&
                ConfigurationFile.hintMode.Value != ConfigurationFile.HintMode.Always &&
                inventoryGui.m_crafting.Find("TabsButtons/Craft").gameObject.activeSelf);
            hintButton.GetComponent<RectTransform>().anchoredPosition = ConfigurationFile.btnPosition.Value;
            hintButton.GetComponent<RectTransform>().sizeDelta = ConfigurationFile.btnSize.Value;
            GameManager.BindGamePad(hintButton.gameObject.transform, ConfigurationFile.btnGamepadKey.Value, new Vector2(ConfigurationFile.btnSize.Value.x - 1, 0), InventoryGui.instance);
        }
    }
}