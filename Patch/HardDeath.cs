using HarmonyLib;

namespace DeathTweaks;

[HarmonyPatch(typeof(Player), "OnDeath")] internal class HardDeath
{
    [HarmonyPrefix]
    private static bool Prefix()
    {
        if (!modEnabled.Value || !noSkillProtection.Value) return true;
        return false;
    }
}