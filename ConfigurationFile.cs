using BepInEx.Configuration;
using BepInEx;
using System;
using System.IO;

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
        
        public static ConfigFile configFile;
        private static readonly string ConfigFileName = CraftMeOnce.GUID + ".cfg";
        private static readonly string ConfigFileFullPath = Paths.ConfigPath + Path.DirectorySeparatorChar + ConfigFileName;

        internal static void LoadConfig(BaseUnityPlugin plugin)
        {
            {
                configFile = plugin.Config;

                modEnabled = configFile.Bind("1 - General", "Mod Enabled", Toggle.On, "Enabling/Disabling this mod (default = On)");
                debug = configFile.Bind("1 - General", "Debug Mode", Toggle.Off, "Enabling/Disabling the debugging in the console (default = Off)");
                                                
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
            
        }
    }
}
