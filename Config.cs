using System.IO;
using BepInEx;
using BepInEx.Configuration;
using ServerSync;

namespace DeathTweaks;

public static class Config
{
    private static readonly string ConfigFileName = "aedenthorn.DeathTweaks.cfg";
    private static DateTime LastConfigChange;
    private static ConfigSync configSync;

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

    public static void LockConfig()
    {
        configSync.AddLockingConfigEntry(config("General", "ServerConfigLock", true,
            "Locks client config file so it can't be modified"));
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
            context.Logger.LogError($"Configuration error: {e.Message}");
        }
    }
}