using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using TMPro;
using UnityEngine;

namespace CraftMeOnce
{
    public class GameManager
    {
        private static readonly Dictionary<string, TMP_FontAsset> cachedFonts = new Dictionary<string, TMP_FontAsset>();
        
        public static object GetPrivateValue(object obj, string name, BindingFlags bindingAttr = BindingFlags.Instance | BindingFlags.NonPublic)
        {
            return obj.GetType().GetField(name, bindingAttr)?.GetValue(obj);
        }
        
        public static void SetPrivateValue(object obj, string name, object value, BindingFlags bindingAttr = BindingFlags.Instance | BindingFlags.NonPublic)
        {
            obj.GetType().GetField(name, bindingAttr)?.SetValue(obj, value);
        }
        
        public static object GetPrivateMethod(object obj, string name, BindingFlags bindingAttr = BindingFlags.Instance | BindingFlags.NonPublic, object[] args = null)
        {
            return obj.GetType().GetMethod(name, bindingAttr)?.Invoke(obj, args);
        }
        
        public static void CallPrivateMethod(object obj, string name, BindingFlags bindingAttr = BindingFlags.Instance | BindingFlags.NonPublic, object[] args = null)
        {
            obj.GetType().GetMethod(name, bindingAttr)?.Invoke(obj, args);
        }
        
        public static void BindGamePad(Transform buttonGo, KeyCode gamepadKeyCode, Vector2 hintAnchoredPosition, InventoryGui inventoryGui = null)
        {
            UIGamePad uiGamePad = null;
            if (buttonGo.TryGetComponent(out uiGamePad))
            {
                string gamepadKey = KeyCodeToString(gamepadKeyCode);
                if (ZInput.instance != null)
                {
                    uiGamePad.m_hint.GetComponentInChildren<TextMeshProUGUI>(true).text = ZInput.instance.GetBoundKeyString(gamepadKey, true);
                }
                else
                {
                    ZInput.Initialize();
                    uiGamePad.m_hint.GetComponentInChildren<TextMeshProUGUI>(true).text = ZInput.instance?.GetBoundKeyString(gamepadKey, true);
                }
                RectTransform hintRectTransform = uiGamePad.m_hint.GetComponent<RectTransform>();
                if (hintAnchoredPosition != Vector2.zero || hintRectTransform.anchoredPosition != Vector2.zero)
                    uiGamePad.m_hint.GetComponent<RectTransform>().anchoredPosition = hintAnchoredPosition;
                uiGamePad.m_zinputKey = gamepadKey;
                uiGamePad.m_keyCode = gamepadKeyCode;
                if (inventoryGui != null && inventoryGui.m_crafting.TryGetComponent(out UIGroupHandler group))
                {
                    SetPrivateValue(uiGamePad, "m_group", group);
                }
            }
        }
        
        public static string KeyCodeToString(KeyCode keyCode)
        {
            return ((int)keyCode - 330) switch
            {
                1 => "JoyButtonB", 
                2 => "JoyButtonX", 
                3 => "JoyButtonY", 
                4 => "JoyLBumper", 
                5 => "JoyRBumper", 
                6 => "JoyBack", 
                7 => "JoyStart", 
                8 => "JoyLStick", 
                9 => "JoyRStick", 
                10 => "JoyDPadLeft", 
                11 => "JoyDPadRight", 
                12 => "JoyDPadUp", 
                13 => "JoyDPadDown", 
                14 => "JoyLTrigger", 
                15 => "JoyRTrigger", 
                16 => "JoyButtonA", 
                17 => "JoyButtonB", 
                18 => "JoyButtonX", 
                19 => "JoyButtonY", 
                _ => "JoyButtonA", 
            };
        }
        
        public static TMP_FontAsset getFontAsset(String name)
        {
            if (!cachedFonts.ContainsKey(name))
            {
                Logger.Log($"Finding {name} font...");
                var allFonts = Resources.FindObjectsOfTypeAll<TMP_FontAsset>();
                for (var i = 0; i < allFonts.Length; i++)
                {
                    var font = allFonts[i];
                    if (font.name == name)
                    {
                        Logger.Log($"{name} font found.");
                        cachedFonts.Add(name, font);
                        return font;
                    }
                }
                Logger.Log($"{name} font NOT found.");
                return null;
            }
            else
            {
                return cachedFonts.GetValueSafe(name);
            }
        }
    }
}