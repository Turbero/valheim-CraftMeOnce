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