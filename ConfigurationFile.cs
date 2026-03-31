using BepInEx.Configuration;
using BepInEx;
using System;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace CraftMeOnce
{
    internal class ConfigurationFile
    {
        public enum Toggle {
            Off,
            On
        }
        
        public static ConfigEntry<Toggle> modEnabled;
        public static ConfigEntry<Toggle> debug;
        public static ConfigEntry<Toggle> showExclamation;

        public static ConfigEntry<KeyCode> btnGamepadKey;
        public static ConfigEntry<Vector2> btnPosition;
        public static ConfigEntry<Vector2> btnSize;
        public static ConfigEntry<string> characterForNotCraftedItems;
        public static ConfigEntry<Toggle> repairAll;
        public static ConfigEntry<string> repairAllItemsText;
        
        public static ConfigFile configFile;
        private static readonly string ConfigFileName = CraftMeOnce.GUID + ".cfg";
        private static readonly string ConfigFileFullPath = Paths.ConfigPath + Path.DirectorySeparatorChar + ConfigFileName;

        internal static void LoadConfig(BaseUnityPlugin plugin)
        {
            {
                configFile = plugin.Config;

                modEnabled = configFile.Bind("1 - General", "Mod Enabled", Toggle.On, "Enabling/Disabling this mod (default = On)");
                debug = configFile.Bind("1 - General", "Debug Mode", Toggle.Off, "Enabling/Disabling the debugging in the console (default = Off)");
                showExclamation = configFile.Bind("1 - General", "Show Exclamation", Toggle.On, "Turn on/off the exclamation mark in the names (default = On)");
                
                btnGamepadKey = configFile.Bind("2 - Config", "Button Gamepad Key", KeyCode.JoystickButton0, "Gamepad key to link the button (default: JoystickButton0 - A");
                btnPosition = configFile.Bind("2 - Config", "Button Position", new Vector2(-268, 566), "Left corner position for the map players list (default: x=-268, y=566)");
                btnSize = configFile.Bind("2 - Config", "Button Size", new Vector2(29, 29), "Width/Height of the button exclamation in the workstations (default: x=29, y=29)");
                characterForNotCraftedItems = configFile.Bind("2 - Config", "Character for Not Crafted Items", "!", "Character to show the item has never been crafted (default = '!')");
                repairAll = configFile.Bind("2 - Config", "Repair All", Toggle.On, "Enable/disable repairing all items in one click (default = true)");
                repairAllItemsText = configFile.Bind("2 - Config", "Repair All Text", "Repair all items", "Repair all text for repair button tooltip");
                
                SetupWatcher();
            }
        }

        private static void SetupWatcher()
        {
            FileSystemWatcher watcher = new FileSystemWatcher(Paths.ConfigPath, ConfigFileName);
            watcher.Changed += ReadConfigValues;
            watcher.Created += ReadConfigValues;
            watcher.Renamed += ReadConfigValues;
            watcher.IncludeSubdirectories = true;
            watcher.SynchronizingObject = ThreadingHelper.SynchronizingObject;
            watcher.EnableRaisingEvents = true;
        }

        private static void ReadConfigValues(object sender, FileSystemEventArgs e)
        {
            if (!File.Exists(ConfigFileFullPath)) return;
            try
            {
                Logger.Log("Attempting to reload configuration...");
                configFile.Reload();
                SettingsChanged(null, null);
            }
            catch
            {
                Logger.LogError($"There was an issue loading {ConfigFileName}");
            }
        }

        private static void SettingsChanged(object sender, EventArgs e)
        {
            //Reload mod stuff while game is active
            if (BtnExclamationPatch.btnExclamation != null)
            {
                BtnExclamationPatch.btnExclamation.gameObject.SetActive(modEnabled.Value == Toggle.On);
                GameManager.BindGamePad(BtnExclamationPatch.btnExclamation.gameObject.transform, btnGamepadKey.Value, new Vector2(-30, 0), InventoryGui.instance);
            }

            if (InventoryGui.IsVisible())
            {
                //Reload
                GameManager.CallPrivateMethod(InventoryGui.instance, "SetupCrafting");
            }
        }
    }
}
