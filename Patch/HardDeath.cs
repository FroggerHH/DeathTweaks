using HarmonyLib;

namespace DeathTweaks;

[HarmonyPatch(typeof(Player), nameof(Player.OnDeath))]
file class HardDeath
{
    [HarmonyPrefix]
    private static bool Prefix(ref bool __result)
    {
        if (!modEnabled.Value || !noSkillProtection.Value) return true;
        __result = true;
        return false;
    }
}