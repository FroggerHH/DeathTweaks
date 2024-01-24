using HarmonyLib;

namespace DeathTweaks;

[HarmonyPatch(typeof(Skills), "OnDeath")] internal class SkillsOnDeath
{
    [HarmonyPrefix]
    private static bool Prefix()
    {
        if (!modEnabled.Value) return true;
        return reduceSkills.Value;
    }
}