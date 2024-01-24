using System.Reflection;
using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Configuration;
using HarmonyLib;
using static DeathTweaks.Config;

namespace DeathTweaks;

[BepInPlugin("aedenthorn.DeathTweaks", "Death Tweaks", "1.4.0")]
public class Plugin : BaseUnityPlugin
{
    public static ConfigEntry<bool> modEnabled;
    public static ConfigEntry<bool> isDebug;
    public static ConfigEntry<int> nexusID;

    public static ConfigEntry<bool> keepEquippedItems;
    public static ConfigEntry<bool> keepHotbarItems;
    public static ConfigEntry<bool> keepAllItems;
    public static ConfigEntry<bool> destroyAllItems;

    public static ConfigEntry<bool> useTombStone;
    public static ConfigEntry<bool> keepFoodLevels;
    public static ConfigEntry<bool> keepQuickSlotItems;
    public static ConfigEntry<bool> keepTeleportableItems;
    public static ConfigEntry<bool> createDeathEffects;
    public static ConfigEntry<bool> reduceSkills;
    public static ConfigEntry<bool> noSkillProtection;
    public static ConfigEntry<bool> useFixedSpawnCoordinates;
    public static ConfigEntry<bool> spawnAtStart;

    public static ConfigEntry<Vector3> fixedSpawnCoordinates;
    public static ConfigEntry<float> skillReduceFactor;

    public static ConfigEntry<string> keepItemTypes;
    public static ConfigEntry<string> dropItemTypes;
    public static ConfigEntry<string> destroyItemTypes;

    public static ConfigEntry<string> keepItemNames;
    public static ConfigEntry<string> dropItemNames;
    public static ConfigEntry<string> destroyItemNames;

    internal static Plugin context;
    private static readonly List<string> typeEnums = new();
    internal static Assembly quickSlotsAssembly;

    private void Awake()
    {
        context = this;
        foreach (int i in Enum.GetValues(typeof(ItemDrop.ItemData.ItemType)))
            typeEnums.Add(Enum.GetName(typeof(ItemDrop.ItemData.ItemType), i));

        InitConfig("DeathTweaks", "1.4.0");

        modEnabled = config("General", "Enabled", true, "Enable this mod");
        isDebug = config("General", "IsDebug", true, "Enable debug logs");
        nexusID = config("General", "NexusID", 1068, "Nexus mod ID for updates");
        keepItemTypes = config("ItemLists", "KeepItemTypes", "",
            $"List of items to keep (comma-separated). Leave empty if using DropItemTypes. Valid types: {string.Join(",", typeEnums)}");
        dropItemTypes = config("ItemLists", "DropItemTypes", "",
            $"List of items to drop (comma-separated). Leave empty if using KeepItemTypes. Valid types: {string.Join(",", typeEnums)}");
        destroyItemTypes = config("ItemLists", "DestroyItemTypes", "",
            $"List of items to destroy (comma-separated). Overrides other lists. Valid types: {string.Join(",", typeEnums)}");

        keepItemNames = config("ItemLists", "KeepItems", "",
            "List of items to keep (comma-separated). Use Item names, for example: Iron,IronScrap,BronzeOre");
        dropItemNames = config("ItemLists", "DropItems", "",
            "List of items to drop (comma-separated). Use Item names, for example: Iron,IronScrap,BronzeOre");
        destroyItemNames = config("ItemLists", "DestroyItems", "",
            "List of items to destroy (comma-separated). Overrides other lists. Use Item names, for example: Iron,IronScrap,BronzeOre");

        keepAllItems = config("Toggles", "KeepAllItems", false, "Overrides all other item options if true.");
        destroyAllItems = config("Toggles", "DestroyAllItems", false,
            "Overrides all other item options except KeepAllItems if true.");
        keepEquippedItems = config("Toggles", "KeepEquippedItems", false, "Overrides item lists if true.");
        keepTeleportableItems = config("Toggles", "KeepTeleportableItems", false, "Doesn't override item lists.");
        keepHotbarItems = config("Toggles", "KeepHotbarItems", false, "Overrides item lists if true.");
        useTombStone = config("Toggles", "UseTombStone", true, "Use tombstone (if false, drops items on ground).");
        createDeathEffects = config("Toggles", "CreateDeathEffects", true, "Create death effects.");
        keepFoodLevels = config("Toggles", "KeepFoodLevels", false, "Keep food levels.");
        keepQuickSlotItems = config("Toggles", "KeepQuickSlotItems", false, "Keep QuickSlot items.");

        useFixedSpawnCoordinates = config("Spawn", "UseFixedSpawnCoordinates", false, "Use fixed spawn coordinates.");
        spawnAtStart = config("Spawn", "SpawnAtStart", false, "Respawn at start location.");
        fixedSpawnCoordinates = config("Spawn", "FixedSpawnCoordinates", Vector3.zero, "Fixed spawn coordinates.");

        noSkillProtection = config("Skills", "NoSkillProtection", false, "Prevents skill protection after death.");
        reduceSkills = config("Skills", "ReduceSkills", true, "Reduce skills.");
        skillReduceFactor = config("Skills", "SkillReduceFactor", 0.25f, "Reduce skills by this fraction.");

        Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());
    }

    private void Start()
    {
        if (Chainloader.PluginInfos.ContainsKey("randyknapp.mods.equipmentandquickslots"))
            quickSlotsAssembly = Chainloader.PluginInfos["randyknapp.mods.equipmentandquickslots"].Instance.GetType()
                .Assembly;
    }


    private void _Debug(object msg)
    {
        if (!isDebug.Value) return;
        Logger.LogDebug(msg);
    }

    private void _DebugError(object msg)
    {
        if (!isDebug.Value) return;
        Logger.LogError(msg);
    }

    public static void Debug(object msg) { context._Debug(msg); }

    public static void DebugError(object msg) { context._DebugError(msg); }
}