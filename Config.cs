using System.IO;
using BepInEx;
using BepInEx.Configuration;
using ServerSync;

namespace DeathTweaks;

public static class Config
{
    private static readonly string ConfigFileName = "aedenthorn.DeathTweaks.cfg";
    private static DateTime LastConfigChange;
    public static ConfigSync configSync { get; private set; }

    public static void InitConfig(string modName, string modVersion)
    {
        configSync = new ConfigSync(modName)
            { DisplayName = modName, CurrentVersion = modVersion, MinimumRequiredVersion = modVersion };
        context.Config.SaveOnConfigSet = false;
        SetupWatcher();
        configSync.AddLockingConfigEntry(config("General", "ServerConfigLock", true,
            "Locks client config file so it can't be modified"));
        context.Config.ConfigReloaded += (_, _) => UpdateConfiguration();
        context.Config.SaveOnConfigSet = true;
        context.Config.Save();
    }

    public static ConfigEntry<T> config<T>(string group, string name, T value, ConfigDescription description,
        bool synchronizedSetting = true)
    {
        ConfigDescription extendedDescription =
            new(
                description.Description +
                (synchronizedSetting ? " [Synced with Server]" : " [Not Synced with Server]"),
                description.AcceptableValues, description.Tags);
        var configEntry = context.Config.Bind(group, name, value, extendedDescription);

        var syncedConfigEntry = configSync.AddConfigEntry(configEntry);
        syncedConfigEntry.SynchronizedConfig = synchronizedSetting;

        return configEntry;
    }

    public static ConfigEntry<T> config<T>(string group, string name, T value, string description,
        bool synchronizedSetting = true)
    {
        return config(group, name, value, new ConfigDescription(description), synchronizedSetting);
    }

    private static void SetupWatcher()
    {
        FileSystemWatcher fileSystemWatcher = new(Paths.ConfigPath, ConfigFileName);
        fileSystemWatcher.Changed += ConfigChanged;
        fileSystemWatcher.IncludeSubdirectories = true;
        fileSystemWatcher.SynchronizingObject = ThreadingHelper.SynchronizingObject;
        fileSystemWatcher.EnableRaisingEvents = true;
    }

    private static void ConfigChanged(object sender, FileSystemEventArgs e)
    {
        if ((DateTime.Now - LastConfigChange).TotalSeconds <= 2) return;
        LastConfigChange = DateTime.Now;

        try
        {
            context.Config.Reload();
        }
        catch
        {
            DebugError("Unable reload config");
        }
    }

    private static void UpdateConfiguration()
    {
        try
        {
            Debug("Configuration Received");
        }
        catch (Exception e)
        {
            DebugError($"Configuration error: {e.Message}");
        }
    }
}