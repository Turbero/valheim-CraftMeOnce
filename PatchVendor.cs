using System.Collections.Generic;
using HarmonyLib;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CraftMeOnce
{
    [HarmonyPatch(typeof(StoreGui), "Show")]
    public class StoreGuiShowPatch
    {
        private static GameObject vendorHintButtonGo;
        public static Button vendorHintButton;
        private static TextMeshProUGUI vendorButtonText;
        private static TextMeshProUGUI hiddenItemsText;
        
        private static GameObject hiddenItemsGO;
        
        public static void Postfix(StoreGui __instance, Trader trader)
        {
            if (trader == null)
                return;
            Logger.Log("Trader.m_name: " + trader.name);

            if (vendorHintButtonGo == null || vendorHintButton == null || vendorButtonText == null)
            {
                createVendorHintButton();
            }

            if (hiddenItemsGO == null)
            {
                hiddenItemsGO = new GameObject("HiddenItems", typeof(RectTransform), typeof(TextMeshProUGUI));
                hiddenItemsGO.transform.SetParent(__instance.transform.Find("Store"), false);
                hiddenItemsGO.GetComponent<RectTransform>().anchoredPosition = new Vector2(80, -265);
                hiddenItemsText = hiddenItemsGO.GetComponent<TextMeshProUGUI>();
                hiddenItemsText.GetComponent<TextMeshProUGUI>().font = GameManager.getFontAsset("Valheim-AveriaSerifLibre");
                hiddenItemsText.GetComponent<TextMeshProUGUI>().fontStyle = FontStyles.Bold;
                hiddenItemsText.GetComponent<TextMeshProUGUI>().fontSize = 16;
                hiddenItemsText.GetComponent<TextMeshProUGUI>().fontSizeMax = 16;
                hiddenItemsText.GetComponent<TextMeshProUGUI>().fontSizeMin = 8;
                hiddenItemsText.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;
            }
            updateVendorHintButton();
            updateVendorHintFlags(__instance);
        }

        public static void updateVendorHintFlags(StoreGui storeGui)
        {
            List<GameObject> m_itemList = (List<GameObject>)GameManager.GetPrivateValue(storeGui, "m_itemList");
            foreach (GameObject item in m_itemList)
            {
                GameObject name = Utils.FindChild(item.transform, "name")?.gameObject;
                if (name != null)
                {
                    TextMeshProUGUI itemText = name.GetComponent<TextMeshProUGUI>();
                    if (itemText == null) continue;
                
                    Logger.Log("Vendor | translatedText "+itemText.text);
                    bool found = Caching.findTranslatedKey(itemText, out var recipeKey);
                    if (found)
                    {
                        bool known = Player.m_localPlayer.IsKnownMaterial(recipeKey);
                        Logger.Log("found itemRecipeKeyValue: "+ itemText + " - "+ recipeKey + " - IsKnownMaterial: "+known);
                        if (!known && ConfigurationFile.vendorHintMode.Value != ConfigurationFile.HintMode.Off)
                            itemText.text = "<color=yellow>" + ConfigurationFile.vendorCharacterForNotObtainedItems.Value + "</color> " + Localization.instance.Localize(itemText.text);
                        else
                            itemText.text = Localization.instance.Localize(itemText.text);
                    }
                }
            }

            if (hiddenItemsGO == null) return;
            
            Trader trader = (Trader)GameManager.GetPrivateValue(storeGui, "m_trader");
            int hiddenCount = trader.m_items.Count - m_itemList.Count;
            foreach (var tradeItem in trader.m_items)
                if (!string.IsNullOrEmpty(tradeItem.m_buyKey) && Player.m_localPlayer.HaveUniqueKey(tradeItem.m_buyKey))
                    --hiddenCount;
            
            string hiddenText = hiddenCount == 0 ? "" : ConfigurationFile.vendorHiddenElementsText.Value+": "+hiddenCount;
            hiddenItemsGO.GetComponent<TextMeshProUGUI>().text = hiddenText;
        }

        private static void createVendorHintButton()
        {
            //Transform copyButton = StoreGui.instance.transform.Find("Store/BuyButton");
            //Transform parent = copyButton.parent.transform;
            Transform copyButton = InventoryGui.instance.m_skillsDialog.transform.Find("SkillsFrame/Closebutton");
            Transform parent = StoreGui.instance.transform.Find("Store");
            vendorHintButtonGo = Object.Instantiate(copyButton.gameObject, parent);
            vendorHintButtonGo.name = "VendorHintButton";
            vendorHintButtonGo.transform.SetParent(parent, false);
            
            vendorButtonText = vendorHintButtonGo.GetComponentInChildren<TextMeshProUGUI>();
            vendorButtonText.font = GameManager.getFontAsset("Valheim-AveriaSerifLibre");
            vendorButtonText.fontStyle = FontStyles.Normal;
            vendorButtonText.alignment = TextAlignmentOptions.Center;

            vendorHintButton = vendorHintButtonGo.GetComponent<Button>();
            vendorHintButton.onClick = new Button.ButtonClickedEvent();
            vendorHintButton.onClick.AddListener(() =>
            {
                ConfigurationFile.vendorHintMode.Value =
                    ConfigurationFile.vendorHintMode.Value == ConfigurationFile.HintMode.Off
                        ? ConfigurationFile.HintMode.On
                        : ConfigurationFile.HintMode.Off;
                //The config reload will call the setupCrafting after the previous line
            });
        }

        public static void updateVendorHintButton()
        {
            vendorHintButton.GetComponent<RectTransform>().anchoredPosition = ConfigurationFile.vendorBtnPosition.Value;
            vendorHintButton.GetComponent<RectTransform>().sizeDelta = ConfigurationFile.vendorBtnSize.Value;
            vendorButtonText.text = ConfigurationFile.vendorCharacterForNotObtainedItems.Value;
            vendorButtonText.color = ConfigurationFile.vendorHintMode.Value != ConfigurationFile.HintMode.Off ? Color.yellow : Color.gray;
            GameManager.BindGamePad(vendorHintButton.gameObject.transform, ConfigurationFile.vendorBtnGamepadKey.Value, new Vector2(ConfigurationFile.vendorBtnSize.Value.x - 1, 0));
            
            vendorHintButton.gameObject.SetActive(
                ConfigurationFile.modEnabled.Value == ConfigurationFile.Toggle.On &&
                ConfigurationFile.vendorHintMode.Value != ConfigurationFile.HintMode.Always);
        }
    }

    [HarmonyPatch(typeof(StoreGui), "FillList")]
    public class StoreGuiFillListPatch
    {
        public static void Postfix(StoreGui __instance)
        {
            StoreGuiShowPatch.updateVendorHintFlags(__instance);
        }
    }
}